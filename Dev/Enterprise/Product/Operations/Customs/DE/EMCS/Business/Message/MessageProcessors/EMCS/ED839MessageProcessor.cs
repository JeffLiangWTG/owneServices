using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED839MessageProcessor : EmcsMessageProcessor<EmcsInboundEDIMessage<IED839>, IED839>
	{
		public ED839MessageProcessor(LoggingInformation logger)
			: base(logger)
		{ }

		protected override string MessageFriendlyNameCore => Res.GetString("cf1b9b41-eff6-4027-b7ad-9bb5408cf91c", "EMCS ED839 Message Processor");

		protected override bool MustHaveLinkedObject => false;

		protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IED839> message)
		{
			BusinessObject result = null;
			var provider = message.DataProvider;
			if (provider != null)
			{
				var rejectedEads = provider.RejectedEads.Take(2).ToArray();
				if (rejectedEads.Length == 1)
				{
					var rejectedEad = rejectedEads[0];
					result = GetDeclarationFromEADNumber(message, rejectedEad.AdministrativeReferenceCode, provider.MessageGroup, rejectedEad.SequenceNumber);
				}
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IED839> message)
		{
			var provider = message.DataProvider;
			var status = EDIMessage.Status.Discarded;
			if (provider != null)
			{
				messageGroup = provider.MessageGroup;
				var eadNumbersWithNoDeclaration = new ZStringBuilder();
				var administrativeReferenceCodeList = new List<ZString>() { provider.MRN };

				foreach (var rejectedEad in provider.RejectedEads)
				{
					var administrativeReferenceCode = rejectedEad.AdministrativeReferenceCode;
					var emcsDeclaration = GetDeclarationFromEADNumber(message, administrativeReferenceCode, provider.MessageGroup, rejectedEad.SequenceNumber);
					if (emcsDeclaration != null)
					{
						status = EDIMessage.Status.ProcessedOK;
						emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.ERJ;
						SubscribeDocumentLinking(emcsDeclaration);
						GenerateHtmlEmailAndSendToOriginalOrGroup(factory
							, emcsDeclaration
							, Res.GetString("47757793-A547-4C3F-83EA-882650BB9C70", "EMCS e-AD rejected")
							, GetEmailBody(emcsDeclaration, provider, rejectedEad)
							, false
							, message.Branch
							, emcsDeclaration
							, () => emcsDeclaration.Messages.LastOutgoingMessage);
					}
					else
					{
						eadNumbersWithNoDeclaration.Append(administrativeReferenceCode);
					}
					administrativeReferenceCodeList.Add(administrativeReferenceCode);
				}

				factory.CreateStmNoteForEdiMessage(message.PK, (NoResString)"EADNumber's for which a Declaration could not be found:" + System.Environment.NewLine + eadNumbersWithNoDeclaration.ToStringWithNewLineBetweenAppends());
				message.SetLogbookLocalReferenceNumber(provider.LocalReferenceNumber);
				message.SetLogbookRegistrationNumber(administrativeReferenceCodeList);
			}

			message.EM_Status = status;
		}

		static ZString GetEmailBody(EMCSJobDeclaration declaration, IED839 provider, IEMCSEvent rejectedEad)
		{
			var rejectionReasonCode = provider.RejectionReasonCode;
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("90D189A6-6554-4A8F-B652-BDE93C9FE8F6", "Your EMCS Declaration for Job {0} was rejected by customs. For details please follow the link to the Job.", declaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("4BA416D4-48B9-4914-BE63-6941569DFDC8", "Sending Customs Office"), provider.SendingCustomsOffice);
			tableCreator.WriteRow(Res.GetString("A1F51404-8189-4286-93F1-F2CAF44E9C88", "Date of Issuance"), provider.IssuanceDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
			if (!provider.MRN.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("CCB520D3-47B5-4414-893D-04D22CD633E2", "MRN"), provider.MRN);
			}
			tableCreator.WriteRow(Res.GetString("41800F10-7350-4EDB-8F6B-9880AB23DBC1", "Rejection Reason"), rejectionReasonCode + " - " + declaration.Factory.GetCachedValue<EmcsRejectionReasonList>().GetDescriptionFromCode(rejectionReasonCode));
			htmlBody.Append(tableCreator.ToHtml());

			htmlBody.Append("<br />");

			var arcHtmlTableCreator = new HtmlTableCreator(new[] { Res.GetString("C629DFFC-058F-443B-8945-71705CFD2DE5", "Sequence No."), Res.GetString("CEE7A078-7851-4789-98AC-39E4DA44A5FB", "ARC") });
			arcHtmlTableCreator.WriteRow(rejectedEad.SequenceNumber, rejectedEad.AdministrativeReferenceCode);
			htmlBody.Append(arcHtmlTableCreator.ToHtml());

			return htmlBody.ToString();
		}
	}
}
