using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CUSRES_2_912;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D04A.Messages.CONTRL;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.ServiceTask
{
	class CcsukNonChiefResponseProcessor : ApplicationTypeMessageProcessor
	{
		public CcsukNonChiefResponseProcessor(ILogger serviceLogger, LoggingInformation logger)
			: base(logger)
		{
			this.serviceLogger = serviceLogger;
		}

		protected override void ProcessMessageCore(EDIMessage ediMessage)
		{
			SegmentGroup edifactObject = null;
			using (ediMessage.SuspendSettingHasChanges())
			{
				var originalText = ediMessage.EM_MessageText;
				ediMessage.EM_MessageText = CUSCARGeneratorBase.RestoreFakeSegmentNames(ediMessage.CharacterSet, ediMessage.EM_MessageText);
				edifactObject = ediMessage.GetAutoEdifactMessageUsingNamedFactory(CcsukEdifactMessageFactory.Factory, ediMessage.CharacterSet);
				ediMessage.EM_MessageText = originalText;
			}

			if (edifactObject != null)
			{
				if (!TryParseCuscarInbound(edifactObject, ediMessage)
					&& !TryParseCim(edifactObject, ediMessage)
					&& !TryParseCukFsa(edifactObject, ediMessage)
					&& !TryParseCUSRESInbound(edifactObject, ediMessage)
					&& !TryParseGenral(edifactObject, ediMessage)
					&& !TryParseContrl(edifactObject, ediMessage))
				{
					serviceLogger.Log(LogType.Information, string.Format("Could not parse inbound message #{0}. Factory expected it, parser did not. Text begins {1}", ediMessage.EM_MessageNum, ediMessage.EM_MessageText.SubstringSafe(0, 100)));
					ediMessage.EM_Status = EDIMessage.Status.Error;
				}
				else
				{
					if (ediMessage.EM_ApplicationReference == "000001")
					{
						ediMessage.EM_ApplicationReference = "";
					}
				}
			}
			else
			{
				serviceLogger.Log(LogType.Information, string.Format("Could not parse inbound message #{0}. Factory did not expected it. Check factory registrations. Text begins {1}", ediMessage.EM_MessageNum, ediMessage.EM_MessageText.SubstringSafe(0, 100)));
				ediMessage.EM_Status = EDIMessage.Status.Failed;
			}
			if (ediMessage.Interchange != null)
			{
				ediMessage.Interchange.EI_Status = EDIInterchange.Status.Received;
			}
		}

		bool TryParseCim(SegmentGroup edifactObject, EDIMessage ediMessage)
		{
			var cargoFactMessage = edifactObject as CargoFactMessage;
			if (cargoFactMessage != null)
			{
				return new CargoFactMessageProcessor(cargoFactMessage, ediMessage, serviceLogger).DoProcessing();
			}
			return false;
		}

		bool TryParseCuscarInbound(SegmentGroup edifactObject, EDIMessage ediMessage)
		{
			var cuscarEdifact = edifactObject as CUSCARMessage;
			if (cuscarEdifact != null)
			{
				return new CuscarParserAndProcessor(cuscarEdifact, ediMessage, serviceLogger).DoProcessing();
			}
			return false;
		}

		bool TryParseContrl(SegmentGroup edifactObject, EDIMessage incomingMessage)
		{
			var contrl = edifactObject as CONTRLMessage;
			if (contrl != null)
			{
				var controlParser = new ContrlParserAndProcessor(contrl, incomingMessage, serviceLogger);
				var result = controlParser.Parse();
				serviceLogger.Log(LogType.Information, "Parsed CONTRL message " + incomingMessage.EM_MessageNum);
				return result;
			}
			return false; // not a contrl or a contrl from which we could not identify the original outbound messages 
		}

		bool TryParseGenral(SegmentGroup edifactObject, EDIMessage ediMessage)
		{
			if (edifactObject != null && edifactObject is GenralMessage)
			{
				new GenralMessageParser().ParseGenral(ediMessage, (GenralMessage)edifactObject);
				serviceLogger.Log(LogType.Information, "Parsed GENRAL message " + ediMessage.EM_MessageNum);
				return true;
			}
			return false;
		}

		bool TryParseCukFsa(SegmentGroup edifactObject, EDIMessage incomingMessage)
		{
			if (edifactObject != null && edifactObject is CUKFSAMessage)
			{
				incomingMessage.EM_MessageType = CcsukTransmissionMessageFunction.CUKFSR.FSA.Subcode;
				var parser = new FsaParser(incomingMessage);
				var report = parser.Parse();
				var outgoingMessagePk = ZGuid.Empty;
				var reportProcessor = new FsaReportProcessor(report, incomingMessage);
				if (!report.CargoWise_CommonAccessReference.IsEmpty)
				{
					// Solicited FSA in response to FSR
					reportProcessor.ProcessSolitictedResponseUsingSyscar();
					serviceLogger.Log(LogType.Information, delegate
					{ return string.Format("Processed FSR/FSA, message #{0}, consignment {1}", incomingMessage.EM_MessageNum, report.Header_AirwaybillPrefixAndAirwaybillNumber + report.Header_HouseWaybillNumber + report.Header_SplitReference); });
				}
				else
				{
					// Unsolicited FSA, e.g. E0 report. 
					reportProcessor.ProcessUnsolicitedReport();
					serviceLogger.Log(LogType.Information, delegate
					{ return string.Format("Processed report type {2}, message #{0}, consignment {1}", incomingMessage.EM_MessageNum, report.Header_AirwaybillPrefixAndAirwaybillNumber + report.Header_HouseWaybillNumber + report.Header_SplitReference, report.Header_ReportType); });
				}
				return true; // parsable even if unknown job
			}
			return false;
		}

		bool TryParseCUSRESInbound(SegmentGroup edifactObject, EDIMessage ediMessage)
		{
			if (edifactObject != null && edifactObject is CUSRESMessage)
			{
				new CUSRESParserAndProcessor(edifactObject, ediMessage, serviceLogger).DoProcessing();
				serviceLogger.Log(LogType.Information, "Parsed CUSRES message " + ediMessage.EM_MessageNum);
				return true;
			}
			return false;
		}

		protected override string ApplicationCodeCore
		{
			get { return ApplicationCodeList.Codes.GbCcsuk; }
		}

		protected override string MessageFriendlyNameCore
		{
			get { return "CCSUK Inbound Processor"; }
		}

		readonly ILogger serviceLogger;
	}
}
