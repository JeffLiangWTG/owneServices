
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Messaging;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Chief.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class LucalGenralResponseMaker_DUCR : LucalGenralResponseMaker_Success
	{
		public LucalGenralResponseMaker_DUCR(string requestText, IBusiness mainBusinessObject, IBusiness[] childrenBusinessObjects, EDIMessage inboundEdiMessage, ZString commonAccessReference)
			: base(requestText, mainBusinessObject, childrenBusinessObjects, inboundEdiMessage, commonAccessReference)
		{
		}

		internal override void MakeAndSendAllResponses()
		{
			var payload = ZString.Empty;
			if (childrenBusinessObjects.Length == 0)
			{
				needsFooter = false;
				payload = CreateSingleResponsePayload();
			}
			else
			{
				var allItemsToMake = new List<IBusiness>();
				if (mainBusinessObject != null)
				{
					allItemsToMake.Add(mainBusinessObject);
				}
				allItemsToMake.AddRange(childrenBusinessObjects);
				payload = CreateMultipleResponseForAllItems(allItemsToMake);
			}
			SendGenralsFromOneLargePayload(payload);
		}

		protected string CreateMultipleResponseForAllItems(List<IBusiness> allItemsToMake)
		{
			var sb = new ZStringBuilder();
			sb.Append("ducr                                part  npx description     aod s");
			sb.Append("----------------------------------- ---- ---- --------------- --- -");

			foreach (var child in allItemsToMake)
			{
				var entry = child as CusEntryHeader;
				if (entry != null)
				{
					var ducr = (entry.CH_BGMReference + "/").Split('/')[0];
					ZString part = (entry.CH_BGMReference + "/").Split('/')[1];
					if (!part.IsEmpty)
					{ part = "/" + part; }
					var port = !entry.Declaration.JE_RL_NKFinalDestination.IsEmpty ? (entry.Declaration.PortOfArrival.RL_IATA.IsEmpty ? entry.Declaration.JE_RL_NKFinalDestination.Right(3) : entry.Declaration.PortOfArrival.RL_IATA) : ZString.Empty;
					var row = string.Format("{0} {1} {2} {3} {4} {5}",
											ducr.TrimAndPad(35),
											part.TrimAndPad(4),
											entry.Declaration.JE_TotalNoOfPieces.ToString().TrimAndPad(4),
											entry.Declaration.JE_GoodsDescription.TrimAndPad(15),
											port.TrimAndPad(3),
											entry.Declaration.ZG_StyleOfEntrySOE.TrimAndPad(1)
											);
					sb.Append(row);
				}
			}
			return sb.ToStringWithNewLineBetweenAppends();
		}

		protected string CreateSingleResponsePayload()
		{
			entry = mainBusinessObject as CusEntryHeader;
			declaration = entry.Declaration;
			if (declaration.IsExport)
			{
				headerWrap = new GbChiefExportHeader(entry);
			}
			else if (declaration.IsImport)
			{
				headerWrap = new GbChiefImportHeader(entry);
			}

			return string.Format(SingleConsignmentMask,
										entry.CH_BGMReference.TrimAndPad(28),
										headerWrap.PortOfDeparture.TrimAndPad(7),
										NPX.TrimAndPad(8),
										NPR.TrimAndPad(3),
										declaration.JE_MasterBill.TrimAndPad(28),
										headerWrap.IATAPortOfDestination.TrimAndPad(7),
										headerWrap.GrossWeightInKilograms.ToString().TrimAndPad(8),
										NPD.TrimAndPad(3),
										headerWrap.HouseBill.TrimAndPad(52),
										declaration.ZG_CTStatusID.TrimAndPad(8),
										headerWrap.FlightNumber.TrimAndPad(28),
										headerWrap.FlightDate.ToString("dd/MM/yyyy"),
										headerWrap.GoodsDescription.TrimAndPad(63),
										entry.EntryInstruction.CEI_Style.TrimAndPad(3),
										entry.EntryNumber.TrimAndPad(11),
										entry.CH_EntrySubmittedDate.ToString("dd/MM/yyyy"),
										declaration.JE_GBRouteOfEntry.TrimAndPad(7),
										declaration.ZG_StyleOfEntrySOE.TrimAndPad(7),
										declaration.ZG_ImportClearanceStatusICS.TrimAndPad(11),
										CACandCAT.TrimAndPad(28),
										CustomsActionDate.ToString("dd/MM/yyyy HH:mm"),
										declaration.JE_MasterUCR,
										declaration.Supplier.OH_FullName.TrimAndPad(63),
										declaration.SupplierDocumentaryAddress.AddressAsASingleLineWithoutCompanyName.TrimAndPad(63),
										declaration.Importer.OH_FullName.TrimAndPad(63),
										declaration.ImporterDocumentaryAddress.AddressAsASingleLineWithoutCompanyName.TrimAndPad(63),
										declaration.JE_DateOfArrival.ToString("dd/MM/yyyy HH:mm"),
										declaration.ZG_LCPDepart.ToString("dd/MM/yyyy HH:mm").TrimAndPad(28),
										headerWrap.LocationOfGoods + headerWrap.ShedCode
										);
		}

		internal const string SingleConsignmentMask = @"ref   :{0}orig:{1}npx:{2}npr:{3}
master:{4}dest:{5}gwt:{6}npd:{7}
house :{8}ct :{9}
shpmt :{10}date:{11}
descr :{12}
entry :{13} {14} {15}  roe: {16}soe: {17}ics: {18}
status:{19}date:{20}
mucr  :{21}

shpr  :{22}
addr  :{23}

cnsee :{24}
addr  :{25}

arrvd :{26}
dep   :{27}dest:{28}";

		ZString NPR
		{
			get { return ((Hawb != null) ? Hawb.CS_PiecesLanded : ShipmentPackingOutturnHelper.PackagesReceived).ToString(); }
		}

		ZString NPD
		{
			get { return ((Hawb != null) ? Hawb.NumberOfPiecesDelivered : ZInt.Zero).ToString(); }
		}

		ExportShipmentPackingOutturnHelper shipmentPackingOutturnHelper;
		ExportShipmentPackingOutturnHelper ShipmentPackingOutturnHelper
		{
			get { return shipmentPackingOutturnHelper ?? (shipmentPackingOutturnHelper = new ExportShipmentPackingOutturnHelper(declaration)); }
		}

		ZString NPX
		{
			get { return Hawb != null ? Hawb.CS_PiecesManifested.ToString() : headerWrap.ExpectedNumberOfPackages.ToString(); }
		}

		ZString CACandCAT
		{
			get { return Hawb != null ? new ZString(Hawb.CustomsActionCode + "-" + Hawb.LatestCustomsActionText) : ZString.Empty; }
		}

		ZDateTime CustomsActionDate
		{
			get { return Hawb != null ? Hawb.CustomsActionDate : ZDateTime.Empty; }
		}

		CusHAWB Hawb
		{
			get { return hawb ?? (hawb = declaration.Factory.LoadTop1<CusHAWB>(new ZQuery(CusHAWBSchema.CS_JE_CustomsFormalEntry, declaration.PK))); }
		}

		JobDeclaration declaration;
		GbHeader headerWrap;
		CusEntryHeader entry;
		CusHAWB hawb;
	}

	static class ZStringHelper
	{
		public static ZString TrimAndPad(this ZString input, int totalLength)
		{
			return input.Left(totalLength).PadRight(totalLength);
		}
		public static ZString TrimAndPad(this string input, int totalLength)
		{
			return new ZString(input).TrimAndPad(totalLength);
		}
	}
}
