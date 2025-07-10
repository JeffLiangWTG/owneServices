using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsDEPINCMessageProcessor : NctsMessageProcessor<AtlasInboundEDIMessage<IDEPINC>, IDEPINC>
	{
		public NctsDEPINCMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore =>
			Res.GetString("1a4bc32a-083d-4600-9dac-e20a1f06b900", "NCTS DEPINC Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IDEPINC> message)
		{
			return GetLinkedObjectFromMRN<NctsHeader>(message.Factory, message.DataProvider?.MovementReferenceNumber)?.MovementHeader;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory,
			AtlasInboundEDIMessage<IDEPINC> message)
		{
			var dataProvider = message.DataProvider;
			var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var header = movementHeader.Header;

			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Accepted;

			message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			message.SetLogbookRegistrationNumber(dataProvider.MovementReferenceNumber);

			var emailBody = CreateEmailBody(dataProvider, header);
			var subject = Res.GetString("d9fad6c1-447f-4820-ac0d-d74e55f949eb", "NCTS Departure Incident Notification.");
			emailSubjectSuffixProvider = new NCTSEmailSubjectSuffixProvider(movementHeader);

			GenerateHtmlEmailAndSendToOriginalOrGroup(factory, header, subject, emailBody, false,
				message.Branch, header, () => movementHeader.Messages.LastSentOutgoingMessage);
		}

		NCTSEmailSubjectSuffixProvider emailSubjectSuffixProvider;

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			var email = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			emailSubjectSuffixProvider.SetEmailSubjectSuffix(email);
			return email;
		}

		readonly ReadOnlyCodeDescriptionPairList depincIncidentTypeList = new C0019IncidentCodeList();

		string CreateEmailBody(IDEPINC provider, NctsHeader nctsHeader)
		{
			var emailBody = new StringBuilder();

			var linkToDepartureDeclaration = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(nctsHeader);

			emailBody.Append(ZString.Format(Res.GetString("b8ab3371-90fb-4170-b606-7b6d762241b9",
				"Your NCTS Departure Declaration for Job {0} has received an incident notification.",
				nctsHeader.BH_JobReference)));
			emailBody.Append(@"<br />");
			var emailTable = new HtmlTableCreator();
			emailTable.WriteRow(Res.GetString("a68ea5b2-188f-4729-a3ae-bd022ce00902", "MRN"), provider.MovementReferenceNumber);
			emailTable.WriteRow(Res.GetString("0ca28cb8-793e-4df2-ada5-62fa12bb3c6e", "Incident Time:"),
				provider.IncidentTime.ToString("dd-MMM-yy hh:mm"));

			foreach (var (incident, index) in provider.Incidents.Select((c, i) => (c, i)))
			{
				WriteIncidentRows(emailTable, incident, index + 1);
			}

			emailBody.Append(emailTable.ToHtml());
			return emailBody.ToString();
		}

		void WriteIncidentRows(HtmlTableCreator table, IDEPINCIncident incident, int sequenceNumber)
		{
			var numberedIncidentTypeCell = ZString.Format(Res.GetString("107f9802-0442-4a85-815e-e3f8e466dc1d",
				"Incident {0} Type", sequenceNumber));

			var numberedIncidentTextCell = ZString.Format(Res.GetString("cd289d4e-fa7b-4720-ad29-01f03e5bbd5d",
				"Incident {0} Text", sequenceNumber));
			var typeWithDescription =
				$@"{incident.Type} - {depincIncidentTypeList.GetDescriptionFromCode(incident.Type)}";

			table.WriteRow(numberedIncidentTypeCell, typeWithDescription);
			table.WriteRow(numberedIncidentTextCell, incident.Text);
		}
	}
}
