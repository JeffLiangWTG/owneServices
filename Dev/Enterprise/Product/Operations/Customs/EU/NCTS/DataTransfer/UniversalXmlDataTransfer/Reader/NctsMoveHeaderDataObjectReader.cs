using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase4
{
	public class NctsMoveHeaderDataObjectReader : DataObjectReader<CommercialInfo, NctsCommonMovementHeader>
	{
		public NctsMoveHeaderDataObjectReader(CommercialInfo commercialInfoDataObject, Shipment shipmentDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, NctsHeader header)
			: base(commercialInfoDataObject, logger, helper.Factory)
		{
			this.header = Argument.NotNull(header, "header");
			this.helper = Argument.NotNull(helper, "helper");
			this.shipmentDataObject = Argument.NotNull(shipmentDataObject, "shipmentDataObject");
			isDepartureMovement = header.IsDepartureMovement;
			isArrivalMovement = header.IsArrivalMovement;
		}

		readonly NctsHeader header;
		readonly UniversalDataObjectReaderHelper helper;
		readonly Shipment shipmentDataObject;
		readonly bool isDepartureMovement;
		readonly bool isArrivalMovement;

		protected override NctsCommonMovementHeader GetNewBusinessObject() => GetExistingBusinessObject();

		protected override NctsCommonMovementHeader GetExistingBusinessObject() => isDepartureMovement ? header.MovementHeader : header.ArrivalMovementHeader;

		protected override void PopulateBusinessObject(NctsCommonMovementHeader targetBO)
		{
			if (isDepartureMovement)
			{
				PopulateDepartureMoveHeader();
			}
			if (isArrivalMovement)
			{
				PopulateArrivalMoveHeader();
				PopulateUnloadingMoveHeader();
			}
		}

		void PopulateUnloadingMoveHeader()
		{
			var commercialInvoice = dataObject?.CommercialInvoiceCollection?.FirstOrDefault(x => x.RelatedIndicator.GetCodeAsUpperCase() == Common.EU.NctsMoveHeaderType.Codes.Unloading);
			if (commercialInvoice != null)
			{
				var moveHeaderBO = header.UnloadingMovementHeader;
				FillGoodsItem(moveHeaderBO, commercialInvoice);
			}
		}

		void PopulateArrivalMoveHeader()
		{
			var commercialInvoice = dataObject?.CommercialInvoiceCollection?.FirstOrDefault(x => x.RelatedIndicator.GetCodeAsUpperCase() == Common.EU.NctsMoveHeaderType.Codes.Arrival);
			if (commercialInvoice != null)
			{
				var moveHeaderBO = header.ArrivalMovementHeader;
				var moveHeaderRow = GetColumnIndexer(moveHeaderBO);

				FillLocationAtClearance(moveHeaderRow, shipmentDataObject.SubLocationAtClearance);
				if (shipmentDataObject.AddInfoCollection.GetZStringValue(DataObjectWriterConstants.ArrivalMovementHeader.AddInfo.SimplifiedArrivalProcedureFlag)
					.GetValueOrDefault().EqualsIgnoringCase(DataObjectWriterConstants.ArrivalMovementHeader.AddInfo.IsSimplifiedArrivalProcedure))
				{
					SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_GONumber, NctsControlResult.Codes.AuthorizedTrader);
				}
				FillGoodsItem(moveHeaderBO, commercialInvoice);
			}
		}

		void PopulateDepartureMoveHeader()
		{
			var headerRow = GetColumnIndexer(header);
			var commercialInvoice = dataObject?.CommercialInvoiceCollection?.FirstOrDefault(x => x.RelatedIndicator.GetCodeAsUpperCase() == Common.EU.NctsMoveHeaderType.Codes.Departure);
			if (commercialInvoice != null)
			{
				var moveHeaderBO = header.MovementHeader;
				var moveHeaderRow = GetColumnIndexer(moveHeaderBO);

				if (header.BH_JobReference.Equals(shipmentDataObject.OwnerRef))
				{
					header.LocalReferenceNumber = ZString.Empty;
				}
				else
				{
					header.SetSystemDefinedValue(NctsHeader.Schema.LocalReferenceNumber, shipmentDataObject.OwnerRef.GetValueOrDefault());
				}

				SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_InBondEntryType, shipmentDataObject.MessageType?.Code);
				SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_GS_NKCusAgent, shipmentDataObject.CustomsBroker?.Code);
				SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_RL_NKForeignDestPort, shipmentDataObject.PortOfLoading?.Code);
				SetValue(headerRow, CusInBondHeaderSchema.BH_RL_NKImportLoadPort, shipmentDataObject.PortOfOrigin?.Code);
				SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_RL_NKDestinationPort, shipmentDataObject.PortOfDestination?.Code);
				SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_InlandTransportMode, shipmentDataObject.TransportMode?.Code);
				SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_TOLCarrierCode, shipmentDataObject.VesselCountryOfRegistration?.Code);
				SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_TOLCarrierID, shipmentDataObject.VesselName);
				SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_BTAIndicator, shipmentDataObject.DeliveryMode?.Code);
				SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_MethodOfPayment, shipmentDataObject.PaymentMethod?.Code);
				SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_PlaceOfUnloading, shipmentDataObject.PortOfDischarge?.Code);
				SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_ConveyanceNumber, shipmentDataObject.VoyageFlightNo);
				FillLocationAtClearance(moveHeaderRow, shipmentDataObject.LocationAtClearance);
				FillAddInfoCollection(moveHeaderRow, headerRow);
				FillItinerary();
				FillGoodsItem(moveHeaderBO, commercialInvoice);
				FillContainerCollection();
				FillSealInfo(moveHeaderRow, shipmentDataObject.SealInfo);
			}
		}

		void FillSealInfo(IColumnIndexer moveHeaderRow, SealInfo sealInfo)
		{
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_SealType, sealInfo?.Type?.Code);
			SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_SealQty, sealInfo?.Quantity);
		}

		void FillGoodsItem(NctsCommonMovementHeader moveHeaderBO, CommercialInvoiceHeader commercialInvoice)
		{
			if (commercialInvoice.CommercialInvoiceLineCollection != null)
			{
				using (moveHeaderBO.LineNumberGenerator.GetLineNumberSuspender())
				{
					moveHeaderBO.GoodsItems.DeleteAll();
				}

				var sortedInvoiceLineCollection = SortInvoiceLineByLineNo(commercialInvoice.CommercialInvoiceLineCollection, moveHeaderBO);

				var isDeparture = moveHeaderBO is NctsDepartureMovementHeader;
				foreach (var commercialInvoiceLine in sortedInvoiceLineCollection)
				{
					if (isDeparture)
					{
						new DepartureGoodsItemDataObjectReader(shipmentDataObject, logger, helper, header, moveHeaderBO, commercialInvoiceLine.Value, ZShort.ParseSafe(commercialInvoiceLine.Key.ToString(), ZShort.Zero))
							.ReadIntoBusinessObject();
					}
					else
					{
						new ArrivalAndUnloadingGoodsItemDataObjectReader(shipmentDataObject, logger, helper, header, moveHeaderBO, commercialInvoiceLine.Value, ZShort.ParseSafe(commercialInvoiceLine.Key.ToString(), ZShort.Zero))
							.ReadIntoBusinessObject();
					}
				}
			}
		}

		SortedDictionary<ZInt, CommercialInvoiceLine> SortInvoiceLineByLineNo(DataObjectList<CommercialInvoiceLine> commercialInvoiceLineCollection, NctsCommonMovementHeader moveHeaderBO)
		{
			var moveHeaderType = ZString.Empty;
			if (moveHeaderBO.BM_SubApplicationCode == Common.EU.NctsMoveHeaderType.Codes.Departure)
			{
				moveHeaderType = Res.GetString("7D3BCB2B-0FBB-47C1-83AB-6AD968AC8BB0", "Departure Move Header");
			}
			else if (moveHeaderBO.BM_SubApplicationCode == Common.EU.NctsMoveHeaderType.Codes.Arrival)
			{
				moveHeaderType = Res.GetString("49CF60A6-6700-4649-A4DA-CD9BCC62A9CF", "Arrival Move Header");
			}
			else if (moveHeaderBO.BM_SubApplicationCode == Common.EU.NctsMoveHeaderType.Codes.Unloading)
			{
				moveHeaderType = Res.GetString("76A4237C-5BDC-48DB-A640-2A78CF49856C", "Unloading Move Header");
			}

			var result = new SortedDictionary<ZInt, CommercialInvoiceLine>();
			var zeroLineNoInvoiceLineList = new List<CommercialInvoiceLine>();

			foreach (var commercialInvoiceLine in commercialInvoiceLineCollection)
			{
				var lineNo = commercialInvoiceLine.LineNo.GetValueOrDefault();
				if (lineNo != 0)
				{
					if (!result.ContainsKey(lineNo))
					{
						result.Add(lineNo, commercialInvoiceLine);
					}
					else
					{
						logger.Log(LogType.Warning, Res.GetString("6C40E16D-5D67-4697-BD3D-49D6C4CE23AF", "Attempted to insert a duplicated move detail to {0} with line no. ({1}).", moveHeaderType, lineNo));
					}
				}
				else
				{
					zeroLineNoInvoiceLineList.Add(commercialInvoiceLine);
				}
			}

			if (result.Any())
			{
				var maxKey = result.Max(x => x.Key);
				foreach (var zeroLineNoInvoiceLine in zeroLineNoInvoiceLineList)
				{
					maxKey++;
					result.Add(maxKey, zeroLineNoInvoiceLine);
				}
			}

			return result;
		}

		void FillItinerary()
		{
			if (shipmentDataObject.TransportLegCollection != null)
			{
				var uniqueVoyageIdentifier = ZString.Empty;
				for (var i = 1; i <= shipmentDataObject.TransportLegCollection.Count; i++)
				{
					uniqueVoyageIdentifier += shipmentDataObject.TransportLegCollection.FirstOrDefault(x => x.LegOrder == i)?.DepartureReference;
				}
				var headerRow = GetColumnIndexer(header);
				SetValue(headerRow, CusInBondHeaderSchema.BH_UniqueVoyageIdentifier, uniqueVoyageIdentifier);
			}
		}

		void FillContainerCollection()
		{
			if (shipmentDataObject.ContainerCollection != null)
			{
				foreach (var item in shipmentDataObject.ContainerCollection)
				{
					var container = header.DepartureHeaderContainers.FirstOrDefault(x => x.BC_ContainerNum.EqualsIgnoringCase(item.ContainerNumber)) ?? header.DepartureHeaderContainers.AddNew();

					var containerRow = GetColumnIndexer(container);
					SetValue(containerRow, CusInBondContainerSchema.BC_ContainerNum, item.ContainerNumber);
					SetValue(containerRow, CusInBondContainerSchema.BC_Seal1, item.Seal);
					SetValue(containerRow, CusInBondContainerSchema.BC_Seal2, item.SecondSeal);
				}
			}
		}

		void FillAddInfoCollection(IColumnIndexer moveHeaderRow, IColumnIndexer headerRow)
		{
			var addInfoCollection = shipmentDataObject.AddInfoCollection;
			if (addInfoCollection != null)
			{
				if (addInfoCollection.GetZStringValue(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.ControlResultCode)
					.GetValueOrDefault().EqualsIgnoringCase(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.IsSimplifiedNctsProcedureA3))
				{
					SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_GONumber, NctsControlResult.Codes.AuthorizedTrader);
				}
				var exportDate = addInfoCollection.GetZStringValue(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.ControlResultDateLimit);
				if (exportDate.HasValue)
				{
					SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_ExportDate, DateTime.ParseExact(exportDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture));
				}
				SetValue(headerRow, CusInBondHeaderSchema.BH_FTZMove, addInfoCollection.GetZBoolValue(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.SecurityIndicator));
				SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_ExportTransportMode, addInfoCollection.GetZStringValue(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.TransportModeAtBorder));
				SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_TransportAtDeparture, addInfoCollection.GetZStringValue(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.Box18TransportID));
				SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_RN_NKTransportAtDepartureCountry, addInfoCollection.GetZStringValue(DataObjectWriterConstants.DepartureMovementHeader.AddInfo.Box18TransportNationality));
			}
		}

		void FillLocationAtClearance<T>(IColumnIndexer moveHeaderRow, T codeDescriptionPair) where T : ICodeDescriptionDataObject
		{
			if (codeDescriptionPair != null)
			{
				var description = codeDescriptionPair.Description.GetValueOrDefault();
				if (description.EqualsIgnoringCase(DataObjectWriterConstants.MovementHeader.LocationDescriptions.AuthorisedLocationOfGoodsCode))
				{
					SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_LocationOfGoodsCode, codeDescriptionPair.Code);
				}
				else if (description.EqualsIgnoringCase(DataObjectWriterConstants.MovementHeader.LocationDescriptions.AgreedLocationOfGoodsCode))
				{
					SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_LocationOfGoodsCode, codeDescriptionPair.Code);
				}
				else if (description.EqualsIgnoringCase(DataObjectWriterConstants.MovementHeader.LocationDescriptions.AgreedLocationOfGoods))
				{
					SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_LocationOfGoods, codeDescriptionPair.Code);
				}
				else if (description.EqualsIgnoringCase(DataObjectWriterConstants.MovementHeader.LocationDescriptions.CustomsSubPlace))
				{
					SetValue(moveHeaderRow, CusInBondMoveHeaderSchema.BM_CustomsSubPlace, codeDescriptionPair.Code);
				}
			}
		}
	}
}
