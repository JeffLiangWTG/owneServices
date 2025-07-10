using Enterprise.Customs.GB.CNS.ServiceTasks.StatusUpdateXML.TextProcessors;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.CNS
{
	public class CnsTextApplicationTypeMessageProcessor : CnsXmlApplicationTypeMessageProcessor
	{
		public CnsTextApplicationTypeMessageProcessor(ILogger serviceLogger, IEDocsDelayedSaver eDocsSaver)
			: base(serviceLogger, eDocsSaver)
		{
		}

		protected override void ProcessMessageCore(EDIMessage receivedEdiMessage)
		{
			var reportTypeId = receivedEdiMessage.EM_MessageText.Trim().Right(10).Trim();
			var regex = new System.Text.RegularExpressions.Regex(@"(...-...?-.+)");
			var match = regex.Match(reportTypeId);
			if (!match.Success)
			{
				HandleFailureToParse(receivedEdiMessage, "Could not determine report type");
				return;
			}
			else
			{
				receivedEdiMessage.EM_ApplicationReference = reportTypeId;
			}

			CnsChildProcessor childProcessor = null;
			switch (reportTypeId)
			{
				case "CER-CLR-2":
					childProcessor = new CnsChildProcessor_CerClr2();
					break;
				case "CER-OFF-2":
					childProcessor = new CnsChildProcessor_CerOff2();
					break;
				case "CER-NOM-3":
					childProcessor = new CnsChildProcessor_CerNom3();
					break;
				case "CER-REL-2":
					childProcessor = new CnsChildProcessor_CerRel2();
					break;
				case "CMI-ADV-1":
					childProcessor = new CnsChildProcessor_CmiAdv1();
					break;
				case "CMI-CLR-1":
					childProcessor = new CnsChildProcessor_CmiClr1();
					break;
				case "CMI-CLR-2":
					childProcessor = new CnsChildProcessor_CmiClr2();
					break;
				case "CMI-EXC-1":
					childProcessor = new CnsChildProcessor_CmiExc1();
					break;
				case "CMI-H3-1":
					childProcessor = new CnsChildProcessor_CmiH31();
					break;
				case "CMI-HLD-1":
					childProcessor = new CnsChildProcessor_CmiHld1();
					break;
				case "CMI-HLD-2":
					childProcessor = new CnsChildProcessor_CmiHld2();
					break;
				case "CMI-NOM-1":
					childProcessor = new CnsChildProcessor_CmiNom1();
					break;
				case "CMI-NOM-2":
					childProcessor = new CnsChildProcessor_CmiNom2();
					break;
				case "CMI-NOM-4":
					childProcessor = new CnsChildProcessor_CmiNom4();
					break;
				case "CMI-SLR-2":
					childProcessor = new CnsChildProcessor_CmiSlr2();
					break;
				case "DTI-IF-1":
					childProcessor = new CnsChildProcessor_DtiIf1();
					break;
				case "CMI-REL-4":
					childProcessor = new CnsChildProcessor_CmiRel4();
					break;
			}

			if (childProcessor != null)
			{
				childProcessor.Process(receivedEdiMessage, ServiceLogger, EDocsSaver);
			}
			else
			{
				HandleFailureToParse(receivedEdiMessage, "Unknown report type " + reportTypeId);
			}
			if (receivedEdiMessage.Interchange != null)
			{
				receivedEdiMessage.Interchange.EI_Status = EDIInterchange.Status.Received;
			}
		}

		static void HandleFailureToParse(EDIMessage receivedEdiMessage, string failureReasonPreamble)
		{
			receivedEdiMessage.EM_Status = "ERR";

			if (receivedEdiMessage.EM_ReceiveTransmit == EDIMessage.Direction.Receive && receivedEdiMessage.Interchange != null)
			{
				receivedEdiMessage.Interchange.EI_Status = "ERR";
			}

			CnsChildProcessor.InterpretMessage(receivedEdiMessage, failureReasonPreamble);
			var firstLine = CnsChildProcessor.GetFirstLineOfMessageText(receivedEdiMessage);
			CnsChildProcessor.SendFailureNotificationEmail(failureReasonPreamble + " - " + firstLine, receivedEdiMessage);
		}

		public const string TxtSubCode = "TXT";

		protected override string SubCodeCore { get { return TxtSubCode; } }
	}
}
