using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class LucalGenralResponseMaker_Hawb : LucalGenralResponseMaker_Mawb
	{
		public LucalGenralResponseMaker_Hawb(string requestText, IBusiness mainBusinessObject, IBusiness[] childrenBusinessObjects, EDIMessage inboundEdiMessage, ZString commonAccessReference)
			: base(requestText, mainBusinessObject, childrenBusinessObjects, inboundEdiMessage, commonAccessReference)
		{
		}

		internal override void MakeAndSendAllResponses()
		{
			string payload = "";
			var allHawbs = new List<IBusiness>();
			if (mainBusinessObject != null)
			{
				allHawbs.Add(mainBusinessObject);
			}
			allHawbs.AddRange(childrenBusinessObjects);

			if (allHawbs.Count == 1)
			{
				needsFooter = false;
				payload = MakeSingleHawbReply(allHawbs[0] as CusHAWB);
			}
			else
			{
				payload = MakeOneLargePayloadForAllChildren_ImportFSR(childrenBusinessObjects);
			}
			SendGenralsFromOneLargePayload(payload);
		}

		string MakeSingleHawbReply(CusHAWB hawb)
		{
			if (hawb != null)
			{
				return string.Format(LucalGenralResponseMaker_DUCR.SingleConsignmentMask,
											hawb.CS_HAWB.TrimAndPad(28),
											hawb.AirportOfOrigin.TrimAndPad(7),
											hawb.CS_PiecesManifested.ToString().TrimAndPad(8),
											hawb.CS_PiecesLanded.ToString().TrimAndPad(3),
											hawb.MAWB.CM_MAWB.TrimAndPad(28),
											hawb.AirportOfDestination.TrimAndPad(7),
											hawb.CS_Weight.ToString().TrimAndPad(8),
											"", // 7, NPD
											hawb.CS_HAWB.TrimAndPad(52),
											(hawb.HasEntryWithLodgedOrPrelodgedWithCustoms ? hawb.Declaration.ZG_CTStatusID : ZString.Empty).TrimAndPad(8),
											hawb.MAWB.CM_FlightNo.TrimAndPad(28),
											hawb.MAWB.CM_ArrivalDate.ToString("dd/MM/yyyy"), // flight date
											hawb.DescriptionOfGoods.TrimAndPad(63),
											(hawb.HasEntryWithLodgedOrPrelodgedWithCustoms ? hawb.Declaration.JE_DeclarationType : ZString.Empty).TrimAndPad(3),
											(hawb.HasEntryWithLodgedOrPrelodgedWithCustoms ? hawb.Declaration.DeclarationNumber : ZString.Empty).TrimAndPad(11),
											(hawb.HasEntryWithLodgedOrPrelodgedWithCustoms ? hawb.Declaration.EarliestCustomsEntryIssueDate.ToString("dd/MM/yyyy") : string.Empty),
											(hawb.HasEntryWithLodgedOrPrelodgedWithCustoms ? hawb.Declaration.JE_EntryStatus : ZString.Empty).TrimAndPad(7),
											(hawb.HasEntryWithLodgedOrPrelodgedWithCustoms ? hawb.Declaration.ZG_StyleOfEntrySOE : ZString.Empty).TrimAndPad(7),
											(hawb.HasEntryWithLodgedOrPrelodgedWithCustoms ? hawb.Declaration.ZG_ImportClearanceStatusICS : ZString.Empty).TrimAndPad(11),
											(hawb.CustomsActionCode + "-" + hawb.LatestCustomsActionText).TrimAndPad(28),
											hawb.CustomsActionDate.ToString("dd/MM/yyyy HH:mm"),
											"A:" + hawb.MAWB.CM_MAWB,
											(hawb.HasEntryWithLodgedOrPrelodgedWithCustoms ? hawb.Declaration.Supplier.OH_FullNameTruncated : ZString.Empty).TrimAndPad(63),  // supplier name
											(hawb.HasEntryWithLodgedOrPrelodgedWithCustoms ? hawb.Declaration.SupplierDocumentaryAddress.AddressAsASingleLineWithoutCompanyName : ZString.Empty).TrimAndPad(63),  // supplier address
											(hawb.HasEntryWithLodgedOrPrelodgedWithCustoms ? hawb.Declaration.Importer.OH_FullNameTruncated : ZString.Empty).TrimAndPad(63),  // importer name
											(hawb.HasEntryWithLodgedOrPrelodgedWithCustoms ? hawb.Declaration.ImporterDocumentaryAddress.AddressAsASingleLineWithoutCompanyName : ZString.Empty).TrimAndPad(63), // importer address
											"",  // Arrival
											"".TrimAndPad(28),  // Departure
											hawb.CargoTerminalOperatorAirport + hawb.CargoTerminalOperator
											);
			}

			return "";
		}
	}
}
