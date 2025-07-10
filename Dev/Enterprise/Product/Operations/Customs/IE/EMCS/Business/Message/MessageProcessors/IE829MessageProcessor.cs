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
	public class IE829MessageProcessor : EMCSMessageProcessor<IIE829>
	{
		public IE829MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("6CDCC83C-A3FF-4724-B371-DA4D0797EAA9", "EMCS IE829 Message Processor");

		protected override bool MustHaveLinkedObject => false;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE829 provider)
		{
			if (provider != null)
			{
				var eadNumbersWithNoDeclaration = new ZStringBuilder();
				foreach (var exciseMovementEad in provider.ExciseMovementEads)
				{
					var administrativeReferenceCode = exciseMovementEad.AdministrativeReferenceCode;
					var emcsDeclaration = GetDeclarationFromEADNumber(message, exciseMovementEad.AdministrativeReferenceCode, exciseMovementEad.SequenceNumber);
					if (emcsDeclaration != null)
					{
						emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.EXP;
						linkedEMCSDeclaration = emcsDeclaration;
						SendEmailNotification(message, Res.GetString("9A7AC3E8-6825-4C38-98B6-77984563B567", "EMCS Notification of Accepted Export"), false, provider, exciseMovementEad, GetEmailBody);
					}
					else
					{
						eadNumbersWithNoDeclaration.Append(administrativeReferenceCode);
					}
				}

				if (!eadNumbersWithNoDeclaration.IsEmpty)
				{
					factory.CreateStmNoteForEdiMessage(message.PK, (NoResString)"EADNumber's for which a Declaration could not be found:" + System.Environment.NewLine + eadNumbersWithNoDeclaration.ToStringWithNewLineBetweenAppends());
				}
			}
		}

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent exciseMovementEad)
		{
			var provider = (IIE829)dataProvider;
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("D48647AE-04AA-4F39-9F67-6AD81CC94831", "Your EMCS Declaration for Job {0} received a notification of accepted export. For details please follow the link to the Job.", declaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("9C724BF9-684C-4605-B88C-1D50D73BA366", "Sending Customs Office"), provider.SendingCustomsOffice);
			tableCreator.WriteRow(Res.GetString("71D86A1B-0246-48F0-9CBF-8486F89224E1", "Date of Acceptance"), provider.AcceptanceDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
			tableCreator.WriteRow(Res.GetString("B0215577-1B53-44B3-AA37-75CA1CB446E5", "MRN"), provider.Mrn);
			htmlBody.Append(tableCreator.ToHtml());

			htmlBody.Append("<br />");

			var arcHtmlTableCreator = new HtmlTableCreator(new[] { Res.GetString("908BCA38-5BA3-4C08-A69D-E87DA7A54F45", "Sequence No."), Res.GetString("8B781716-3016-4D77-8ADE-585F6043BB33", "ARC") });
			arcHtmlTableCreator.WriteRow(exciseMovementEad.SequenceNumber, exciseMovementEad.AdministrativeReferenceCode);
			htmlBody.Append(arcHtmlTableCreator.ToHtml());

			return htmlBody.ToString();
		}
	}
}
