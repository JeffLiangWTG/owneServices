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
	public sealed class IE829MessageProcessor : EMCSMessageProcessor<IIE829>
	{
		public IE829MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("B8B232BE-1F75-4531-A93E-66092848FF70", "EMCS IE829 Message Processor");

		protected override bool MustHaveLinkedObject => false;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE829 provider)
		{
			if (provider != null)
			{
				var eadNumbersWithNoDeclaration = new ZStringBuilder();
				foreach (var exciseMovementEad in provider.ExciseMovementEads)
				{
					var administrativeReferenceCode = exciseMovementEad.AdministrativeReferenceCode;
					var emcsDeclaration = EMCSHelper.GetDeclarationFromEADNumber(message, exciseMovementEad.AdministrativeReferenceCode, exciseMovementEad.SequenceNumber);
					if (emcsDeclaration != null)
					{
						emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.EXP;
						linkedEMCSDeclaration = emcsDeclaration;
						SendEmailNotification(message, Res.GetString("{49A912D6-2655-48F7-97E5-9FA551387CA5}", "EMCS Notification of Accepted Export"), false, provider, exciseMovementEad, GetEmailBody);
					}
					else
					{
						eadNumbersWithNoDeclaration.Append(administrativeReferenceCode);
					}
				}

				if (!eadNumbersWithNoDeclaration.IsEmpty)
				{
					factory.CreateStmNoteForEdiMessage(message.PK, "EADNumber's for which a Declaration could not be found:" + System.Environment.NewLine + eadNumbersWithNoDeclaration.ToStringWithNewLineBetweenAppends());
				}
			}
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent exciseMovementEad)
		{
			var provider = (IIE829)dataProvider;
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("8D0E1FCF-27B7-431E-B5D6-5DD459BEB3F9", "Your EMCS Declaration for Job {0} received a notification of accepted export. For details please follow the link to the Job.", declaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("CC5D1B62-6BEA-4917-A804-7AB38F3E5577", "Sending Customs Office"), provider.SendingCustomsOffice);
			tableCreator.WriteRow(Res.GetString("A94AF585-5FC5-48CE-83A2-7D9E5F5E738A", "Date of Acceptance"), provider.AcceptanceDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
			tableCreator.WriteRow(Res.GetString("D5DA1F4F-3891-41C9-9C14-FDFEB065F131", "MRN"), provider.Mrn);
			htmlBody.Append(tableCreator.ToHtml());

			htmlBody.Append("<br />");

			var arcHtmlTableCreator = new HtmlTableCreator(new[] { Res.GetString("427653BF-39B7-4C1D-97C7-76D01410619F", "Sequence No."), Res.GetString("DCEA4702-D8D5-44CE-9876-D521FFC3FFB6", "ARC") });
			arcHtmlTableCreator.WriteRow(exciseMovementEad.SequenceNumber, exciseMovementEad.AdministrativeReferenceCode);
			htmlBody.Append(arcHtmlTableCreator.ToHtml());

			return htmlBody.ToString();
		}
	}
}
