using System;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Registry;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public static class ReleasePrintHelper
	{
		internal static void FindFsnThenPrintReleaseOriginalAndReprint(ICcsukCusAwb awb, ILogger logger, EDIMessage baseMessage)
		{
			var cacFormatted = string.Format(CultureInfo.InvariantCulture, "CSN/{0}", awb.CustomsActionCode);
			if (baseMessage == null)  // willl be null when this is called from the manual C1 releaser.  When called from the FSN processor, we'll know the message in hand
			{
				baseMessage = (from EDIMessage m in awb.Messages
							   where m.EM_MessageType == CcsukTransmissionMessageFunction.CIM.Code
									   && m.EM_MessageSubType == CcsukTransmissionMessageFunction.CIM.CUKFSR.FSN.Subcode
									   && m.EM_ReceiveTransmit == EDIMessage.Direction.Receive
									   && m.EM_MessageText.Contains(cacFormatted, System.StringComparison.OrdinalIgnoreCase)
									   && (awb.SplitReference.IsEmpty || m.EM_ApplicationReference == awb.SplitReference)
									   && (m.Interchange == null || m.Interchange.EI_To == awb.Profile)  // Make sure we pick the right (shed/agent) recipient (doesn't really make any difference as the message content will be identical, but just to be clean)
							   orderby m.EM_SystemCreateTimeUtc
							   select m).LastOrDefault();
			}
			if (baseMessage != null)
			{
				var fsnMessage = awb.Factory.Load<GbEDIMessage>(baseMessage.PK);
				if (fsnMessage != null)
				{
					if (IsSentToRightAgent(fsnMessage, awb))
					{
						PrintC1OriginalAndReprint(fsnMessage, logger);
					}
					else if (IsSentToRightShed(fsnMessage, awb))
					{
						PrintRRAOriginalAndReprint(fsnMessage, awb, logger); // RRA hangs from FSN
					}
				}
			}
			else if (LicenceAndPimaHelper.IsFullShed(awb))
			{
				PrintRRAOriginalAndReprint(awb, awb, logger);  // RRA hangs from AWB. e.g. for EC status jobs with no FSN
			}
		}

		public static bool IsSentToRightAgent(GbEDIMessage fsnMessage, ICcsukCusAwb awb)
		{
			var pimaOfMessage = new ZString(fsnMessage.Interchange?.EI_To.Replace("/", ""));
			return pimaOfMessage.StartsWith(LicenceAndPimaHelper.AgentProfilePrefix, StringComparison.OrdinalIgnoreCase) && pimaOfMessage.Right(3) == awb.AgentBadge;
		}

		public static bool IsSentToRightShed(GbEDIMessage fsnMessage, ICcsukCusAwb awb)
		{
			var pimaOfMessage = new ZString(fsnMessage.Interchange?.EI_To.Replace("/", ""));
			return pimaOfMessage.StartsWith(LicenceAndPimaHelper.ShedProfilePrefix, StringComparison.OrdinalIgnoreCase) && pimaOfMessage.Right(6) == awb.CargoTerminalOperatorAirportAndShed;
		}

		internal static void PrintC1OriginalAndReprint(GbEDIMessage gbEdiMessage, ILogger logger)
		{
			// Print priginal to paper only, not eDocs
			var c1 = PrinterFromEdiMessageHelper_MenuKeys.Ccsuk.C1_AgentsTravellingCopyRemovalAuthority;
			new PrinterFromEdiMessageHelper(gbEdiMessage.Factory, logger).PrintToEdocsAndPaper(c1, gbEdiMessage, gbEdiMessage, GBCustomsDataRegistry.Instance.GetPrinterForCcsuk("C1", "C1"), false);

			// Print spare/reprint into eDocs only.  It will be already embossed with "REPRINT"
			var c1Reprint = PrinterFromEdiMessageHelper_MenuKeys.Ccsuk.C1_AgentsTravellingCopyRemovalAuthorityREPRINT;
			new PrinterFromEdiMessageHelper(gbEdiMessage.Factory, logger).PrintToEdocsAndPaper(c1Reprint, null, gbEdiMessage, null, true);
		}

		internal static void PrintRRAOriginalAndReprint(IDocumentSupportable documentSuporter, ICcsukCusAwb awb, ILogger logger)
		{
			// Print priginal to paper only, not eDocs
			var rra = PrinterFromEdiMessageHelper_MenuKeys.Ccsuk.RRA_ReleaseRemovalAuthority;
			new PrinterFromEdiMessageHelper(awb.Factory, logger).PrintToEdocsAndPaper(rra, awb, documentSuporter, GBCustomsDataRegistry.Instance.GetPrinterForCcsuk("RRA", "RRA"), false);

			// Print spare/reprint into eDocs only.  It will be already embossed with "REPRINT"
			var rraReprint = PrinterFromEdiMessageHelper_MenuKeys.Ccsuk.RRA_ReleaseRemovalAuthorityReprint;
			new PrinterFromEdiMessageHelper(awb.Factory, logger).PrintToEdocsAndPaper(rraReprint, null, documentSuporter, null, true);

			MarkReleaseableOutTurnsAsReleased(awb.OutTurns);
		}

		internal static void PrintP5OriginalAndReprint(GbEDIMessage gbEdiMessage, ICcsukCusAwb awb, ILogger logger)
		{
			var p5 = PrinterFromEdiMessageHelper_MenuKeys.Ccsuk.P5_InterAirportRemoval;
			new PrinterFromEdiMessageHelper(gbEdiMessage.Factory, logger).PrintToEdocsAndPaper(p5, awb, gbEdiMessage, GBCustomsDataRegistry.Instance.GetPrinterForCcsuk("P5", "P5"), true, awb.DocManagerInfo);
		}

		static void MarkReleaseableOutTurnsAsReleased(CusOutTurnList cusOutTurnList)
		{
			foreach (var ot in cusOutTurnList)
			{
				if (ot.IsBeingReleasedNow)
				{
					ot.IsBeingReleasedNow = false;
					ot.IsReleasedAlready = true;
				}
			}
		}
	}
}
