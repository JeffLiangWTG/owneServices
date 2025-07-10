using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class PrintFromCusresProvider : PrintFromMessageProvider
	{
		public PrintFromCusresProvider(CUSRESResponseData cusRes, EDIMessage baseMessage, ILogger logger)
			: base(baseMessage, logger)
		{
			this.cusRes = cusRes;
		}

		public override void DoPrinting()
		{
			var guidOfDocumentCommandToPrint = GetDocumentCommandGuid();
			if (!guidOfDocumentCommandToPrint.IsEmpty)
			{
				new PrinterFromEdiMessageHelper(gbEdiMessage.Factory, logger).PrintToEdocsAndPaper(guidOfDocumentCommandToPrint, gbEdiMessage, gbEdiMessage, GBCustomsDataRegistry.Instance.PrinterCcsuk);
			}
		}

		ZGuid GetDocumentCommandGuid()
		{
			var result = ZGuid.Empty;
			if (gbEdiMessage.DocumentSupporter != null)
			{
				if (cusRes.DocumentNameCode == Fallback.RemovalCode)
				{
					result = PrinterFromEdiMessageHelper_MenuKeys.Ccsuk.F2_FallbackEntryAcceptanceOutput;
				}
				else if (cusRes.CustomsActionCode_StatusOfRequest == CustomsStatusCodes.Codes.EntryOrRequestAccepted)
				{
					result = PrinterFromEdiMessageHelper_MenuKeys.Ccsuk.GR_AdviceOfSelectedRemovalRequest;  // Produce GR from CUSRES/IAR, CUSRES/TSR. The GR is not produced for the shed from the FSN because the FSN/CA that the shed receives does not reveal what flavour of request is being accepted (for "Request Type" field) nor the agent name
				}
			}
			return result;
		}

		readonly CUSRESResponseData cusRes;
	}
}
