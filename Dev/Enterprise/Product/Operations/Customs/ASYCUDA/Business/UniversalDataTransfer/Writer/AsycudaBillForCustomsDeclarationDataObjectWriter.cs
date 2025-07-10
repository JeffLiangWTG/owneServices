using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public interface IAsycudaForCustomsDeclarationDataObjectWriter : IAsycudaBillDataObjectWriter { }

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public class AsycudaForCustomsDeclarationDataObjectWriter<T, TPack, TPackedItem> : AsycudaBillDataObjectWriter<T>, IAsycudaForCustomsDeclarationDataObjectWriter
		where T : AsycudaBill
		where TPack : AsycudaPack
		where TPackedItem : AsycudaPackedItem
	{
		public AsycudaForCustomsDeclarationDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected override void PopulateDataObject(T sourceBill, Shipment uxml)
		{
			base.PopulateDataObject(sourceBill, uxml);
			uxml.Branch = Branch.New(GlbBranch.CurrentBranch);
			uxml.CustomsBroker = Staff.New(GlbStaff.CurrentUser);
			PopulateMessageType(sourceBill, uxml);
			PopulateTransportMode(sourceBill, uxml);
			PopulateContainerMode(sourceBill, uxml);
			PopulateContainerCount(sourceBill, uxml);
			uxml.ServiceLevel = new ServiceLevel { Code = Constants.ServiceLevel.Standard };
			PopulateVesselAndVoyage(sourceBill, uxml);
			uxml.PortOfLoading = new UNLOCO { Code = helper.Header.AMA_RL_NKPortOfLoading };
			uxml.PortOfDischarge = new UNLOCO { Code = helper.Header.AMA_RL_NKPortOfDischarge };
			PopulateDates(sourceBill, uxml);
		}

		protected virtual void PopulateVesselAndVoyage(T sourceBill, Shipment uxml)
		{
			if (helper.Header.AMA_TransportMode == Core.Constants.TransportModes.Air)
			{
				uxml.VesselName = ZString.Empty;
				uxml.VoyageFlightNo = helper.Header.AMA_Voyage;
			}
			else if (helper.Header.AMA_TransportMode == Core.Constants.TransportModes.Road)
			{
				uxml.VesselName = ZString.Empty;
				uxml.VoyageFlightNo = helper.Header.AMA_VehicleRegistration;
			}
			else
			{
				uxml.VesselName = helper.Header.AMA_VesselName;
				uxml.VoyageFlightNo = helper.Header.AMA_Voyage;
			}
		}

		protected override void PopulateHouseBill(T sourceBill, Shipment uxml)
		{
			base.PopulateHouseBill(sourceBill, uxml);
			PopulateAdditionalBills(sourceBill, uxml);
		}

		protected override bool ShouldIncludeCustomsValue => false;
		protected override bool ShouldIncludeZeroValueCharges => false;

		protected override void PopulateCommercialData(T sourceBill, Shipment uxml)
		{
			base.PopulateCommercialData(sourceBill, uxml);

			var invoiceData = new CommercialInvoiceHeader(writeManager.WriterStrategy)
			{
				InvoiceNumber = sourceBill.ABL_BillNumber,
				InvoiceAmount = sourceBill.ABL_FreightValue,
				InvoiceCurrency = new Currency() { Code = sourceBill.ABL_RX_NKFreightValueCurrency },
				CommercialChargeCollection = uxml.CommercialInfo.CommercialChargeCollection,
			};
			invoiceData.SetCommercialInvoiceLineCollection(() => CreateCommercialInvoiceLineCollection(sourceBill));
			uxml.CommercialInfo.CommercialChargeCollection = null;
			uxml.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[] { invoiceData });
		}

		DataObjectList<CommercialInvoiceLine> CreateCommercialInvoiceLineCollection(T sourceBill)
		{
			var invoiceLineDataList = new DataObjectList<CommercialInvoiceLine>();
			var lineNo = 0;
			var isOnePackedItemRelationship = sourceBill.IsOnePackedItemRelationship;
			foreach (var sourcePack in sourceBill.Packs.OfType<TPack>().OrderBy(x => x.ConsignmentReference))
			{
				var packedItem = isOnePackedItemRelationship ? (TPackedItem)sourcePack.GetPackedItemFromCollection() : null;
				invoiceLineDataList.Add(CreateInvoiceLineData(sourcePack, packedItem, ++lineNo));
			}
			return invoiceLineDataList;
		}

		protected void AddAddInfo(Shipment uxml, ZString key, ZString value)
		{
			uxml.SetAddInfoCollection(() =>
			{
				var addInfoCollection = uxml.AddInfoCollection ?? new List<AddInfo>();
				addInfoCollection.Add(new AddInfo() { Key = key, Value = value });
				return addInfoCollection;
			});
		}

		protected virtual CommercialInvoiceLine CreateInvoiceLineData(TPack sourcePack, TPackedItem packedItem, ZInt lineNo)
		{
			var invoiceLineData = new CommercialInvoiceLine
			{
				LineNo = lineNo,
				InvoiceQuantity = new ZDecimal(sourcePack.APA_PackQty),
				InvoiceQuantityUnit = new CodeDescriptionPair() { Code = sourcePack.APA_PackUQ },
				Description = sourcePack.APA_GoodsDescription,
				Weight = sourcePack.APA_Weight,
				WeightUnit = new UnitOfWeight() { Code = sourcePack.APA_WeightUQ },
				Volume = sourcePack.APA_Volume,
				VolumeUnit = new UnitOfVolume() { Code = sourcePack.APA_VolumeUQ }
			};
			var commodityCode = sourcePack.APA_CommodityCode;
			if (commodityCode.Length < 5)
			{
				invoiceLineData.Commodity = new Commodity() { Code = commodityCode };
			}
			if (packedItem != null)
			{
				invoiceLineData.HarmonisedCode = packedItem.API_Tariff;
				invoiceLineData.CustomsQuantity = packedItem.API_CustomsQty;
				invoiceLineData.CustomsQuantityUnit = new CodeDescriptionPair6Char { Code = packedItem.API_CustomsUQ };
				invoiceLineData.LinePrice = sourcePack.LinePrice;
				invoiceLineData.CountryOfOrigin = new Country { Code = packedItem.API_RN_NKGoodsOrigin };
			}
			return invoiceLineData;
		}

		protected virtual void PopulateAdditionalBills(T sourceBill, Shipment uxml)
		{
			var parentBillNumber = helper.Header.AMA_MasterBill;
			uxml.SetAdditionalBillCollection(() =>
			{
				var list = new List<AdditionalBill>();
				list.Add(new AdditionalBill(writeManager.WriterStrategy)
				{
					BillNumber = sourceBill.ABL_BillNumber,
					BillType = new WayBillType() { Code = WayBillTypeList.Codes.House },
					ParentBillNumber = parentBillNumber
				});
				list.Add(new AdditionalBill(writeManager.WriterStrategy)
				{
					BillNumber = parentBillNumber,
					BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master }
				});
				return list;
			});
		}

		protected virtual void PopulateDates(T sourceBill, Shipment uxml)
		{
			uxml.SetDateCollection(() =>
			{
				var list = new List<Date>();
				list.Add(DateType.Departure, ZBool.False, helper.Header.AMA_E_DEP);
				list.Add(DateType.Arrival, ZBool.False, helper.Header.AMA_E_ARV);
				return list;
			});
		}

		protected virtual void PopulateTransportMode(T sourceBill, Shipment uxml)
		{
			uxml.TransportMode = new CodeDescriptionPair() { Code = helper.Header.AMA_TransportMode };
		}

		protected virtual void PopulateContainerMode(T sourceBill, Shipment uxml)
		{
		}

		protected virtual void PopulateContainerCount(T sourceBill, Shipment uxml)
		{
		}

		protected virtual void PopulateMessageType(T sourceBill, Shipment uxml)
		{
			uxml.MessageType = new CodeDescriptionPair() { Code = sourceBill.ABL_ShipmentType };
		}

		protected override void PopulateAddInfos(T sourceBill, Shipment uxml)
		{
		}

		protected override void PopulateWeight(T sourceBill, Shipment uxml)
		{
			uxml.TotalWeight = sourceBill.ABL_GrossWeight;
			uxml.TotalWeightUnit = new UnitOfWeight { Code = sourceBill.ABL_GrossWeightUQ };
		}

		protected override void PopulateVolume(T sourceBill, Shipment uxml)
		{
			uxml.TotalVolume = sourceBill.ABL_Volume;
			uxml.TotalVolumeUnit = new UnitOfVolume { Code = sourceBill.ABL_VolumeUQ };
		}
	}
}
