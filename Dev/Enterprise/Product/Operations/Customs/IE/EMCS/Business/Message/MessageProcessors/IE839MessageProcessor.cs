using System;
using System.Globalization;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.EMCS.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public sealed class IE839MessageProcessor : EMCSMessageProcessor<IIE839>
	{
		public IE839MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("C262414A-2A73-4129-B8AE-9C5EFFB24833", "EMCS IE839 Message Processor");

		protected override bool MustHaveLinkedObject => false;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE839 provider)
		{
			var eadNumbersWithNoDeclaration = new ZStringBuilder();

			foreach (var rejectedEad in provider.RejectedEads)
			{
				var administrativeReferenceCode = rejectedEad.AdministrativeReferenceCode;
				var emcsDeclaration = GetDeclarationFromEADNumber(message, administrativeReferenceCode, rejectedEad.SequenceNumber);
				if (emcsDeclaration != null)
				{
					emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.ERJ;
					linkedEMCSDeclaration = emcsDeclaration;
					SendEmailNotification(message, Res.GetString("8037B297-E67A-49B0-8E22-69D43BBD5062", "EMCS e-AD rejected"), false, provider, rejectedEad, GetEmailBody);
				}
				else
				{
					eadNumbersWithNoDeclaration.Append(administrativeReferenceCode);
				}
			}

			if (eadNumbersWithNoDeclaration.Length > 0)
			{
				factory.CreateStmNoteForEdiMessage(message.PK, (NoResString)"EADNumber's for Declaration's that could not be found:" + System.Environment.NewLine + eadNumbersWithNoDeclaration.ToStringWithNewLineBetweenAppends());
			}
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent rejectedEad)
		{
			var provider = (IIE839)dataProvider;
			var rejectionReasonCode = provider.RejectionReasonCode;
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("9B5CE567-F697-4C05-8E7A-52626ACCD8C4", "Your EMCS Declaration for Job {0} was rejected by customs. For details please follow the link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("18E616F9-D2C1-461B-8CA9-6F3C7545A338", "Sending Customs Office"), provider.SendingCustomsOffice);
			tableCreator.WriteRow(Res.GetString("70A23118-E625-41B3-842C-09900E69F1C7", "Date of Issuance"), provider.IssuanceDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
			if (!provider.MrnNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("3CF69939-7AF5-4751-B7ED-5CCBBE88D178", "MRN"), provider.MrnNumber);
			}

			tableCreator.WriteRow(Res.GetString("258D69EF-17F4-43E9-97C7-834A414FD9AD", "Rejection Reason"), rejectionReasonCode + " - " + RejectionReasonList.GetDescriptionFromCode(rejectionReasonCode));
			htmlBody.Append(tableCreator.ToHtml());
			htmlBody.Append("<br />");

			var arcHtmlTableCreator = new HtmlTableCreator(new[] { Res.GetString("25D9C527-4B47-48EB-8B9E-23C24F91D930", "Sequence No."), Res.GetString("6C905CA6-D86A-46A7-AA48-D926916F901F", "ARC") });
			arcHtmlTableCreator.WriteRow(rejectedEad.SequenceNumber, rejectedEad.AdministrativeReferenceCode);
			htmlBody.Append(arcHtmlTableCreator.ToHtml());

			return htmlBody.ToString();
		}

		EmcsRejectionReasonList RejectionReasonList
		{
			get
			{
				if (emcsRejectionReasonList == null)
				{
					emcsRejectionReasonList = new EmcsRejectionReasonList();
				}

				return emcsRejectionReasonList;
			}
		}
		EmcsRejectionReasonList emcsRejectionReasonList;
	}
}
