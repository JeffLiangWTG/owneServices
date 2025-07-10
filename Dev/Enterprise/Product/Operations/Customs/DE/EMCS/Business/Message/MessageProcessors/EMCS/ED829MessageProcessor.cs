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
	public class ED829MessageProcessor : EmcsMessageProcessor<EmcsInboundEDIMessage<IED829>, IED829>
	{
		public ED829MessageProcessor(LoggingInformation logger)
		: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("E94DFCA2-C799-4291-8EB1-A7B31CD4317F", "EMCS ED829 Message Processor");

		protected override bool MustHaveLinkedObject => false;

		protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IED829> message)
		{
			BusinessObject result = null;
			var provider = message.DataProvider;
			if (provider != null)
			{
				var exciseMovementEads = provider.ExciseMovementEads.Take(2).ToArray();
				if (exciseMovementEads.Length == 1)
				{
					var exciseMovementEad = exciseMovementEads[0];
					result = GetDeclarationFromEADNumber(message, exciseMovementEad.AdministrativeReferenceCode, provider.MessageGroup, exciseMovementEad.SequenceNumber);
				}
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IED829> message)
		{
			var provider = message.DataProvider;
			var status = EDIMessage.Status.Discarded;
			if (provider != null)
			{
				messageGroup = provider.MessageGroup;
				var eadNumbersWithNoDeclaration = new ZStringBuilder();
				var administrativeReferenceCodeList = new List<ZString> { provider.MRN };

				foreach (var exciseMovementEad in provider.ExciseMovementEads)
				{
					var administrativeReferenceCode = exciseMovementEad.AdministrativeReferenceCode;
					var emcsDeclaration = GetDeclarationFromEADNumber(message, administrativeReferenceCode, provider.MessageGroup, exciseMovementEad.SequenceNumber);
					if (emcsDeclaration != null)
					{
						status = EDIMessage.Status.ProcessedOK;
						emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.EXP;
						SubscribeDocumentLinking(emcsDeclaration);
						SendEmail(message, emcsDeclaration, exciseMovementEad);
					}
					else
					{
						eadNumbersWithNoDeclaration.Append(administrativeReferenceCode);
					}
					administrativeReferenceCodeList.Add(administrativeReferenceCode);
				}

				factory.CreateStmNoteForEdiMessage(message.PK, (NoResString)"EADNumber's for which a Declaration could not be found:" + System.Environment.NewLine + eadNumbersWithNoDeclaration.ToStringWithNewLineBetweenAppends());
				message.SetLogbookRegistrationNumber(administrativeReferenceCodeList);
			}
			message.EM_Status = status;
		}

		void SendEmail(EmcsInboundEDIMessage<IED829> message, EMCSJobDeclaration emcsDeclaration, IEMCSEvent exciseMovementEad)
		{
			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
				, emcsDeclaration
				, Res.GetString("ab8716e9-1932-40b0-b4a1-dd93a1130809", "EMCS Notification of Accepted Export")
				, GetEmailBody(emcsDeclaration, message.DataProvider, exciseMovementEad)
				, false
				, message.Branch
				, emcsDeclaration
				, () => emcsDeclaration.Messages.LastSentOutgoingMessage);
		}

		static ZString GetEmailBody(EMCSJobDeclaration declaration, IED829 provider, IEMCSEvent exciseMovementEad)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("e171a2c9-9799-476d-9f2b-014086a8b0c7", "Your EMCS Declaration for Job {0} received a notification of accepted export. For details please follow the link to the Job.", declaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("f7c550b7-7c4e-414c-a542-e0380259c64b", "Sending Customs Office"), provider.SendingCustomsOffice);
			tableCreator.WriteRow(Res.GetString("d54a3509-da9c-43c6-8396-7bfa99bd3b3b", "Date of Acceptance"), provider.AcceptanceDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
			tableCreator.WriteRow(Res.GetString("4f1e28dd-32ae-49b1-bed3-9c0a1e7ee299", "MRN"), provider.MRN);
			htmlBody.Append(tableCreator.ToHtml());

			htmlBody.Append("<br />");

			var arcHtmlTableCreator = new HtmlTableCreator(new[] { Res.GetString("a0251f45-da1f-42d4-8cbc-98621ab3fc3b", "Sequence No."), Res.GetString("2b6ed81e-1cb7-4500-8af1-93609835e718", "ARC") });
			arcHtmlTableCreator.WriteRow(exciseMovementEad.SequenceNumber, exciseMovementEad.AdministrativeReferenceCode);
			htmlBody.Append(arcHtmlTableCreator.ToHtml());

			return htmlBody.ToString();
		}
	}
}
