using System;
using System.Globalization;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE839MessageProcessor : EMCSMessageProcessor<IIE839>
	{
		public IE839MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("2795709A-0AAB-4C81-BF66-29A5436669AF", "EMCS IE839 Message Processor");

		protected override bool MustHaveLinkedObject => false;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE839 provider)
		{
			var eadNumbersWithNoDeclaration = new ZStringBuilder();
			foreach (var rejectedEad in provider.RejectedEads)
			{
				var administrativeReferenceCode = rejectedEad.AdministrativeReferenceCode;
				var emcsDeclaration = EMCSHelper.GetDeclarationFromEADNumber(message, administrativeReferenceCode, rejectedEad.SequenceNumber);
				if (emcsDeclaration != null)
				{
					emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.ERJ;
					linkedEMCSDeclaration = emcsDeclaration;
					SendEmailNotification(message, Res.GetString("7F656007-538B-45E7-AFCA-31A784FA34CC", "EMCS e-AD rejected"), false, provider, rejectedEad, GetEmailBody);
				}
				else
				{
					eadNumbersWithNoDeclaration.Append(administrativeReferenceCode);
				}
			}

			if (eadNumbersWithNoDeclaration.Length > 0)
			{
				factory.CreateStmNoteForEdiMessage(message.PK, "EADNumber's for Declaration's that could not be found:" + System.Environment.NewLine + eadNumbersWithNoDeclaration.ToStringWithNewLineBetweenAppends());
			}
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent rejectedEad)
		{
			var provider = (IIE839)dataProvider;
			var rejectionReasonCode = provider.RejectionReasonCode;
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("82509F92-AC5D-4D54-A0B9-5799E68C2A03", "Your EMCS Declaration for Job {0} was rejected by customs. For details please follow the link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("0FC1B931-41E2-4C09-AED6-6842ABFFA67A", "Sending Customs Office"), provider.SendingCustomsOffice);
			tableCreator.WriteRow(Res.GetString("33E682C5-8CDA-43F8-A0BD-F2ABF9B0D153", "Date of Issuance"), provider.IssuanceDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
			if (!provider.MrnNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("580CAC6C-768C-49CE-9001-B9499525D1F6", "MRN"), provider.MrnNumber);
			}

			tableCreator.WriteRow(Res.GetString("0B6C4272-F9DE-4E17-BC98-69670EF0C53A", "Rejection Reason"), rejectionReasonCode + " - " + RejectionReasonList.GetDescriptionFromCode(rejectionReasonCode));
			htmlBody.Append(tableCreator.ToHtml());
			htmlBody.Append("<br />");

			var arcHtmlTableCreator = new HtmlTableCreator(new[] { Res.GetString("B5278522-C5DA-4448-975D-5F30B894DCFD", "Sequence No."), Res.GetString("5DFE8396-5B87-4336-8634-945E90A4293C", "ARC") });
			arcHtmlTableCreator.WriteRow(rejectedEad.SequenceNumber, rejectedEad.AdministrativeReferenceCode);
			htmlBody.Append(arcHtmlTableCreator.ToHtml());

			return htmlBody.ToString();
		}

		EMCSGBRejectionReasonList RejectionReasonList
		{
			get
			{
				if (emcsRejectionReasonList == null)
				{
					emcsRejectionReasonList = new EMCSGBRejectionReasonList();
				}

				return emcsRejectionReasonList;
			}
		}
		EMCSGBRejectionReasonList emcsRejectionReasonList;
	}
}
