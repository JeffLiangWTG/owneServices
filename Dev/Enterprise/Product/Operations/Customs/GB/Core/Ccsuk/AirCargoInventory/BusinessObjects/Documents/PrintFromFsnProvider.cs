using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class PrintFromFsnProvider : PrintFromMessageProvider
	{
		public PrintFromFsnProvider(CIMFSN cIMFSN, EDIMessage baseMessage, ILogger logger)
			: base(baseMessage, logger)
		{
			this.fsn = cIMFSN;
		}

		public override void DoPrinting()
		{
			var pkOfMenuToPrint_Original = ZGuid.Empty;
			var pkOfMenuToPrint_Reprint = ZGuid.Empty;
			var printOriginalToEdocsToo = false;
			var isMessageSentToShed = IsMessageSentTo(LicenceAndPimaHelper.ShedProfilePrefix);
			switch (fsn.CAC)
			{
				case CustomsStatusCodes.Codes.EntryOrRequestAccepted:  // CA
					if (Awb.IsThroughAwb && isMessageSentToShed)
					{
						pkOfMenuToPrint_Original = PrinterFromEdiMessageHelper_MenuKeys.Ccsuk.G2_AdviceOfCustomsAction;
						// TODO - set pkOfMenuToPrint_Reprint to be that of G2 reprint PK
					}
					break;

				case CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval:
				case CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval:
				case CustomsStatusCodes.Codes.ReleasedForInterShedRemoval:
					if (Awb.NumberOfPiecesReceived > 0 && Awb.NumberOfPiecesExpected == Awb.NumberOfPiecesReceived)  // if NPR != NPX then it will be the FRC that generates a print
					{
						if (!isMessageSentToShed && fsn.CAC != CustomsStatusCodes.Codes.ReleasedForInterShedRemoval)
						{
							if (GBCustomsDataRegistry.Instance.CcsukAutoPrintC1WhenAllPiecesReceivedAndReleased.Value)
							{
								pkOfMenuToPrint_Original = PrinterFromEdiMessageHelper_MenuKeys.Ccsuk.C1_AgentsTravellingCopyRemovalAuthority;
								pkOfMenuToPrint_Reprint = PrinterFromEdiMessageHelper_MenuKeys.Ccsuk.C1_AgentsTravellingCopyRemovalAuthorityREPRINT;
								Awb.ReleaseThisNumberOfPieces(Awb.NumberOfPiecesReceived, NumberOfPiecesReleasedHelper.AgentC1Event);
							}
						}
						else if (isMessageSentToShed)
						{
							MaybeAutoPrintRra();
						}
					}
					break;

				case CustomsStatusCodes.Codes.ClearedByCustoms:
					if (isMessageSentToShed)
					{
						MaybeAutoPrintRra();
					}
					else
					{
						pkOfMenuToPrint_Original = PrinterFromEdiMessageHelper_MenuKeys.Ccsuk.CC_CustomsClearance;
						printOriginalToEdocsToo = true;
					}
					break;

				case CustomsStatusCodes.Codes.ThroughAirWaybillReleased: //CU
					pkOfMenuToPrint_Original = PrinterFromEdiMessageHelper_MenuKeys.Ccsuk.TFM_TransferFreightManifest;
					// TODO - set pkOfMenuToPrint_Reprint to be that of TFM reprint PK
					break;
			}

			if (IsAgentClearanceUnderElectronicFallback)
			{
				pkOfMenuToPrint_Original = PrinterFromEdiMessageHelper_MenuKeys.Ccsuk.CC_CustomsClearance;
				printOriginalToEdocsToo = true;
			}

			if (!pkOfMenuToPrint_Original.IsEmpty)
			{
				// To paper:
				new PrinterFromEdiMessageHelper(gbEdiMessage.Factory, logger).PrintToEdocsAndPaper(pkOfMenuToPrint_Original, gbEdiMessage, gbEdiMessage, GBCustomsDataRegistry.Instance.PrinterCcsuk, printOriginalToEdocsToo);
			}
			if (!pkOfMenuToPrint_Reprint.IsEmpty)
			{
				// To eDocs
				new PrinterFromEdiMessageHelper(gbEdiMessage.Factory, logger).PrintToEdocsAndPaper(pkOfMenuToPrint_Reprint, null, gbEdiMessage, null, true);
			}
		}

		void MaybeAutoPrintRra()
		{
			var helper = new PrintFromFsnForShedAutoPrintingHelper(gbEdiMessage, Awb, logger);
			helper.CalculatePrintJobToCreateAndUpdateAllOutturns();
		}

		bool IsMessageSentTo(string pimaPrefix)
		{
			// NB the AWB's current profile and the recipient of the FSN might be different if the shed and agent share a database.  So use the message's recipient to determine printing, thus we get a different results for the two FSNs.
			return gbEdiMessage != null && gbEdiMessage.Interchange != null && gbEdiMessage.Interchange.EI_To.StartsWith(pimaPrefix);
		}

		bool IsAgentClearanceUnderElectronicFallback
		{
			get
			{
				return fsn.CAC == CustomsStatusCodes.Codes.EntryOrRequestAccepted
					&& fsn.CAT.ToUpper().Contains("FALLBACK RELEASED")
					&& IsMessageSentTo(LicenceAndPimaHelper.AgentProfilePrefix)
					&& GBCustomsDataRegistry.Instance.ChiefFallbackImports.Value;
			}
		}

		readonly CIMFSN fsn;
	}
}
