using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Edifact;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class NEXDOCMessageProcessor<T> : NEXDOCMessageProcessor
		where T : class
	{
		protected NEXDOCMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected const string RexNumberFieldName = "RexNumber";
		protected const string DynamicHtmlSegment = "<!--DynamicHtml-->";
		protected string DocumentType => typeof(T).Name;

		public override bool HandlesDocument(string documentType) => documentType == DocumentType;

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			var result = EDIMessage.Status.Error;

			try
			{
				if (NEXDOCMessageHelper.TryDeserialize<T>(message.EM_MessageText) is T response)
				{
					ProcessMessageInternal(message, response);
					result = EDIMessage.Status.Received;
				}
				else
				{
					throw new InvalidFormatException("Invalid " + DocumentType + ". Cannot Process.");
				}
			}
			catch (InvalidFormatException messageProcessingException)
			{
				var responseEmail = new EmailDef();
				responseEmail.Subject = "ERROR PROCESSING";
				string emailBodyHeader = "FATAL PROCESSING ERROR: " + messageProcessingException.Message;
				responseEmail.Body = emailBodyHeader + System.Environment.NewLine + System.Environment.NewLine + message.EM_MessageText;
				SendErrorReport(null, responseEmail);
			}

			return result;
		}

		protected virtual void ProcessMessageInternal(EDIMessage message, T response)
		{
			throw new NotImplementedException();
		}

		protected ZString GetRexNumberFromInterchangeHeader(EDIMessage message)
		{
			var interchangeHeader = GetInterchangeHeaderFromMessage(message);
			return interchangeHeader?.DeliveryMetadata.ValueCollection.FirstOrDefault(x => x.Name.Equals(RexNumberFieldName, StringComparison.OrdinalIgnoreCase))?.Data;
		}

		protected NEXDOCAcknowledgeInterchangeHeader GetInterchangeHeaderFromMessage(EDIMessage message)
		{
			var headerText = message.Interchange?.EI_HeaderText ?? ZString.Empty;
			if (!headerText.IsEmpty)
			{
				var universalXml = ZString.Format(@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">{0}<Body xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11""/></UniversalInterchange>", headerText);
				return NEXDOCMessageHelper.TryDeserialize<NEXDOCAcknowledgeInterchange>(universalXml, "UniversalInterchange")?.Header;
			}

			return null;
		}

		protected JobComInvoiceHeader FindInvoiceByRexNumber(BusinessObjectFactory factory, ZString rexNumber)
		{
			JobComInvoiceHeader result = null;

			if (!rexNumber.IsEmpty)
			{
				var commercialInvoiceQuery = new ZDBOnlyQuery(typeof(JobComInvoiceHeader));
				var exHeaderQuery = new ZDBOnlySubQuery(typeof(QuarantineExDocHeader), QuarantineExDocHeaderSchema.QH_JZ);

				var entryQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entryQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, rexNumber);
				entryQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
				entryQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.RequestForPermitStatus);
				entryQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);

				exHeaderQuery.AddSubQuery(entryQuery, JoinCondition.And);
				commercialInvoiceQuery.AddSubQuery(exHeaderQuery, JoinCondition.And);

				result = factory.LoadTop1<JobComInvoiceHeader>(commercialInvoiceQuery);
			}

			return result;
		}

		protected EmailDef CreateEmail(ZString subject, ZString mainMessage, ZString details)
		{
			string emailTemplateHtml;
			using (Stream stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.AU.Declaration.Business.MessageProcessors.NEXDOC.HtmlTemplates.Notification.html"))
			{
				emailTemplateHtml = new StreamReader(stream).ReadToEnd();
			}

			emailTemplateHtml = emailTemplateHtml.Replace("{0}", mainMessage);
			emailTemplateHtml = emailTemplateHtml.Replace(DynamicHtmlSegment, details.Trim());

			var emailSender = new HtmlNotificationEmailSender();
			return emailSender.CreateEmail(subject, emailTemplateHtml);
		}

		protected override ZGuid AcknowledgementEmailGroup => AUCustomsDataRegistry.Instance.SendAQISAcknowledgementsToGroup.GetFallBackValueAtAllLevels(Company, Branch, Department);
		protected override ZString AcknowledgementEmailMode => AUCustomsDataRegistry.Instance.SendAQISAcknowledgements.GetFallBackValueAtAllLevels(Company, Branch, Department);
		protected override ZGuid ImpedimentEmailGroup => AUCustomsDataRegistry.Instance.SendAQISImpedimentsToGroup.GetFallBackValueAtAllLevels(Company, Branch, Department);
		protected override ZString ImpedimentEmailMode => AUCustomsDataRegistry.Instance.SendAQISImpediments.GetFallBackValueAtAllLevels(Company, Branch, Department);
		protected override ZGuid ErrorEmailGroup => AUCustomsDataRegistry.Instance.SendAQISErrorsToGroup.Value;
		protected override ZString ErrorEmailMode => AUCustomsDataRegistry.Instance.SendAQISErrors.Value;

		Guid Company => GlbCompany.CurrentCompany.PK.ToGuid();
		Guid Branch => GlbBranch.CurrentBranch.PK.ToGuid();
		Guid Department => Guid.Empty;
	}
}
