using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public interface IAsycudaBillDataObjectWriter : ITopLevelDataObjectWriter
	{
		Shipment GetDataObject(AsycudaBill sourceBO);
	}

	public class AsycudaBillDataObjectWriter<T> : TopLevelDataObjectWriter<T, Shipment>, IAsycudaBillDataObjectWriter
		where T : AsycudaBill
	{
		public AsycudaBillDataObjectWriter(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
			: base(manager)
		{
			this.helper = helper;
		}

		protected readonly AsycudaManifestHeaderDataObjectWriterHelper helper;

		protected override void PopulateDataObject(T sourceBill, Shipment uxml)
		{
			helper.ClearEntryInstructionLinkMap();
			PopulateHouseBill(sourceBill, uxml);
			PopulateWeight(sourceBill, uxml);
			PopulateVolume(sourceBill, uxml);
			PopulatePorts(sourceBill, uxml);
			PopulateGoodsDescription(sourceBill, uxml);

			uxml.OuterPacks = sourceBill.ABL_ManifestQty;
			uxml.OuterPacksPackageType = new PackageType { Code = sourceBill.ABL_ManifestUQ, Description = sourceBill.Lookups.PackageTypeList.GetDescriptionFromCode(sourceBill.ABL_ManifestUQ) };
			var packsForMessage = sourceBill.Header.GetCurrentManifestContext()?.ManifestSendPacks?.Where(pack => pack.Bill == sourceBill) ?? helper.Load<AsycudaPack>(sourceBill.Packs.CompleteFilter).OfType<AsycudaPack>();
			uxml.SetPackingLineCollection(() => CreatePackingLineCollection(packsForMessage));
			PopulateCommercialData(sourceBill, uxml);

			if (!sourceBill.ABL_Remarks.IsEmpty)
			{
				uxml.SetNoteCollection(() => new DataObjectList<Note>(new[]
				{
					new Note
					{
						Description = "Remarks",
						IsCustomDescription = ZBool.True,
						NoteText = sourceBill.ABL_Remarks
					}
				}));
			}

			PopulateNotifyParty(sourceBill, uxml);
			PopulateShipper(sourceBill, uxml);
			PopulateConsignee(sourceBill, uxml);
			PopulateForwarder(sourceBill, uxml);

			PopulateAddInfos(sourceBill, uxml);
			PopulateCustomsSupportingInfo(sourceBill, uxml);
			var billCountries = new[] { sourceBill };
			uxml.SetEntryInstructionCollection(() => ProcessCollection(billCountries, CreateNewAsycudaBillEntryInstructionDataObjectWriter()));
			uxml.SetEntryHeaderCollection(() => ProcessCollection(billCountries, CreateNewAsycudaBillEntryHeaderDataObjectWriter()));

			PopulateSpecificData(sourceBill, uxml);
		}

		protected virtual void PopulateSpecificData(T sourceBill, Shipment uxml)
		{
		}

		protected virtual AsycudaBillEntryInstructionDataObjectWriter<T> CreateNewAsycudaBillEntryInstructionDataObjectWriter() => new AsycudaBillEntryInstructionDataObjectWriter<T>(writeManager, helper);
		protected virtual AsycudaBillEntryHeaderDataObjectWriter<T> CreateNewAsycudaBillEntryHeaderDataObjectWriter() => new AsycudaBillEntryHeaderDataObjectWriter<T>(writeManager, helper);

		protected DataObjectList<PackingLine> CreatePackingLineCollection(IEnumerable<AsycudaPack> packs)
		{
			var writer = CreateNewAsycudaPackDataObjectWriter();
			var list = new DataObjectList<PackingLine>(packs.Select(p => writer.GetDataObject(p)));
			list.Content = CollectionContent.Complete;
			return list;
		}

		protected virtual IAsycudaPackDataObjectWriter CreateNewAsycudaPackDataObjectWriter() => new AsycudaPackDataObjectWriter<AsycudaPack>(writeManager, helper);

		protected virtual void PopulateShipper(T sourceBill, Shipment uxml)
		{
			var addressType = nameof(DocAddressType.ConsignorDocumentaryAddress);
			if (sourceBill.ShipperUseRealOrg)
			{
				uxml.AddOrgAddress(writeManager, sourceBill.Shipper, addressType);
			}
			else
			{
				uxml.AddOrgAddress(CreateOrgAddress(sourceBill, addressType, AsycudaBillSchema.ABL_ShipperName, AsycudaBillSchema.ABL_ShipperStreet1, AsycudaBillSchema.ABL_ShipperStreet2, AsycudaBillSchema.ABL_ShipperCity, AsycudaBillSchema.ABL_ShipperState, AsycudaBillSchema.ABL_ShipperPostcode, AsycudaBillSchema.ABL_RN_NKShipperCountry, AsycudaBillSchema.ABL_ShipperPhone));
			}
		}

		protected virtual void PopulateConsignee(T sourceBill, Shipment uxml)
		{
			var addressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);
			if (sourceBill.ConsigneeUseRealOrg)
			{
				uxml.AddOrgAddress(writeManager, sourceBill.Consignee, addressType);
			}
			else
			{
				uxml.AddOrgAddress(CreateOrgAddress(sourceBill, addressType, AsycudaBillSchema.ABL_ConsigneeName, AsycudaBillSchema.ABL_ConsigneeStreet1, AsycudaBillSchema.ABL_ConsigneeStreet2, AsycudaBillSchema.ABL_ConsigneeCity, AsycudaBillSchema.ABL_ConsigneeState, AsycudaBillSchema.ABL_ConsigneePostcode, AsycudaBillSchema.ABL_RN_NKConsigneeCountry, AsycudaBillSchema.ABL_ConsigneePhone));
			}
		}

		void PopulateNotifyParty(T sourceBill, Shipment uxml)
		{
			var addressType = nameof(DocAddressType.NotifyParty);
			if (sourceBill.NotifyPartyUseRealOrg)
			{
				uxml.AddOrgAddress(writeManager, sourceBill.NotifyParty, addressType);
			}
			else
			{
				uxml.AddOrgAddress(CreateOrgAddress(sourceBill, addressType, AsycudaBillSchema.ABL_NotifyPartyName, AsycudaBillSchema.ABL_NotifyPartyStreet1, AsycudaBillSchema.ABL_NotifyPartyStreet2, AsycudaBillSchema.ABL_NotifyPartyCity, AsycudaBillSchema.ABL_NotifyPartyState, AsycudaBillSchema.ABL_NotifyPartyPostcode, AsycudaBillSchema.ABL_RN_NKNotifyPartyCountry, AsycudaBillSchema.ABL_NotifyPartyPhone));
			}
		}

		protected virtual void PopulateForwarder(T sourceBill, Shipment uxml)
		{
			uxml.AddOrgAddress(writeManager, sourceBill.Forwarder, nameof(DocAddressType.Forwarder));
		}

		protected virtual OrganizationAddress CreateOrgAddress(T sourceBill, ZString addressType, SchemaStringColumn nameColumn, SchemaStringColumn address1Column, SchemaStringColumn address2Column, SchemaStringColumn cityColumn, SchemaStringColumn stateColumn, SchemaStringColumn postcodeColumn, SchemaStringColumn countryColumn, SchemaStringColumn phoneColumn = null)
		{
			var result = new OrganizationAddress()
			{
				AddressType = addressType,
				CompanyName = sourceBill.GetValue(nameColumn),
				Address1 = sourceBill.GetValue(address1Column),
				Address2 = sourceBill.GetValue(address2Column),
				City = sourceBill.GetValue(cityColumn),
				State = sourceBill.GetValue(stateColumn),
				Postcode = sourceBill.GetValue(postcodeColumn),
				Country = Country.New(sourceBill.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, sourceBill.GetValue(countryColumn)))
			};

			if (phoneColumn != null)
			{
				result.Phone = sourceBill.GetValue(phoneColumn);
			}
			return result;
		}

		protected virtual bool ShouldIncludeCustomsValue => true;
		protected virtual bool ShouldIncludeZeroValueCharges => true;

		protected virtual void AddCharge(List<UniversalXml.Customs.CommercialCharge> charges, ZString chargeType, ZDecimal amount, RefCurrency currency)
		{
			if (ShouldIncludeZeroValueCharges || !amount.IsEmpty)
			{
				charges.Add(new UniversalXml.Customs.CommercialCharge()
				{
					ChargeType = ListHelper.GetWithDescription<CodeDescriptionPair>(chargeType, helper.CustomsChargeTypeList),
					Currency = Currency.New(currency),
					Amount = amount
				});
			}
		}

		protected virtual void PopulateCommercialData(T billBO, Shipment billData)
		{
			var commercialChargeCollection = new List<UniversalXml.Customs.CommercialCharge>();
			AddCharge(commercialChargeCollection, CustomsChargeTypeList.Codes.OverseasFreight, billBO.ABL_TransportValue, billBO.TransportValueCurrency);
			AddCharge(commercialChargeCollection, CustomsChargeTypeList.Codes.OverseasInsurance, billBO.ABL_InsuranceValue, billBO.InsuranceValueCurrency);
			AddCharge(commercialChargeCollection, CustomsChargeTypeList.Codes.OtherCharges, billBO.OtherChargesValue, billBO.RefOtherChargesValueCurrency);
			AddCharge(commercialChargeCollection, CustomsChargeTypeList.Codes.Discount, billBO.DiscountValue, billBO.RefDiscountValueCurrency);

			if (ShouldIncludeCustomsValue)
			{
				AddCharge(commercialChargeCollection, CustomsChargeTypeList.Codes.ExWorks, billBO.ABL_FreightValue, billBO.FreightValueCurrency);
				AddCharge(commercialChargeCollection, Constants.CustomsChargeType.CustomsChargeCode, billBO.ABL_CustomsValue, billBO.CustomsValueCurrency);
				var localCurrency = billBO.Header?.Country?.LocalCurrency;
				AddCharge(commercialChargeCollection, Constants.CustomsChargeType.GSTCode, billBO.TaxAmount, localCurrency);
				AddCharge(commercialChargeCollection, Constants.CustomsChargeType.CustomsDutyCode, billBO.DutyAmount, localCurrency);
			}
			billData.CommercialInfo = new UniversalXml.Customs.CommercialInfo { CommercialChargeCollection = commercialChargeCollection.OrderBy(x => x.ChargeType.GetCodeAsUpperCase()).ToList() };
		}

		protected virtual void PopulateAddInfos(T billBO, Shipment billData)
		{
			billData.SetAddInfoCollection(() =>
			{
				var list = new List<AddInfo>();
				var source = new List<string>() { AsycudaBill.Schema.ABL_CarrierReference, AsycudaBill.Schema.ABL_BolType, AsycudaBill.Schema.ABL_PrepaidCollect, AsycudaBill.Schema.MatchingReference };
				foreach (var key in source)
				{
					var value = (ZString)billBO[key];
					if (!value.IsEmpty)
					{
						list.Add(new AddInfo() { Key = key, Value = value });
					}
				}

				var location = billBO.ABL_GoodsLocation;
				var locationInformation = billBO.ABL_LocationInformation;
				if (!location.IsEmpty)
				{
					list.Add(new AddInfo() { Key = AddInfoConstants.Bill.ABL_LocationOfGoods, Value = location });
				}
				if (!locationInformation.IsEmpty)
				{
					list.Add(new AddInfo() { Key = AddInfoConstants.Bill.ABL_LocationInformation, Value = locationInformation });
				}

				foreach (var pair in helper.GetBillCountryAdditionalAddInfos(billBO))
				{
					if (!pair.Value.IsEmpty)
					{
						list.Add(AddInfo.New(pair.Key, pair.Value));
					}
				}

				list.Add(new AddInfo() { Key = AddInfoConstants.Bill.ABL_ShipmentType, Value = GetShipmentType(billBO) });
				if (!billBO.ABL_MarksAndNumbers.IsEmpty)
				{
					list.Add(new AddInfo() { Key = AddInfoConstants.Bill.ABL_MarksAndNumbers, Value = billBO.ABL_MarksAndNumbers });
				}

				if (!billBO.ABL_UCRNumber.IsEmpty)
				{
					list.Add(new AddInfo() { Key = AddInfoConstants.Bill.ABL_UCRNumber, Value = billBO.ABL_UCRNumber });
				}
				return list;
			});
		}

		protected virtual void PopulateHouseBill(T sourceBill, Shipment uxml)
		{
			uxml.WayBillNumber = sourceBill.ABL_BillNumber;
			uxml.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };
		}

		protected virtual void PopulateWeight(T sourceBill, Shipment uxml)
		{
			uxml.TotalWeight = new ZWeight(sourceBill.ABL_GrossWeight, sourceBill.ABL_GrossWeightUQ).InKilogramsSafe;
			uxml.TotalWeightUnit = new UnitOfWeight() { Code = Core.Constants.Weight.Kilograms, Description = "kilogram" };  // lower case singular is correct
		}

		protected virtual void PopulateVolume(T sourceBill, Shipment uxml)
		{
			uxml.TotalVolume = sourceBill.VolumeInM3;
			uxml.TotalVolumeUnit = new UnitOfVolume() { Code = Core.Constants.Volume.CubicMetres, Description = "Cubic metre" };
		}

		protected virtual void PopulatePorts(T sourceBill, Shipment uxml)
		{
			uxml.PortOfOrigin = ListHelper.GetWithName(sourceBill.ABL_RL_NKOrigin, sourceBill.Factory.GetRefUNLOCOList());
			uxml.PortOfDestination = ListHelper.GetWithName(sourceBill.ABL_RL_NKFinalDestination, sourceBill.Factory.GetRefUNLOCOList());
		}

		protected virtual void PopulateGoodsDescription(T sourceBill, Shipment uxml)
		{
			uxml.GoodsDescription = sourceBill.ABL_GoodsDescription;
		}

		void PopulateCustomsSupportingInfo(T sourceBill, Shipment uxml)
		{
			uxml.SetCustomsSupportingInformationCollection(() =>
			{
				var customsSupportingInfos = helper.GetBillCustomsSupportingInfos(sourceBill);
				var customsSupportingInfosList = customsSupportingInfos.Where(info => info != null).Select(info => CustomsSupportingInformationCollectionCreator.Create(info, null, writeManager)).ToList();
				return customsSupportingInfosList;
			});
		}

		ZString? GetShipmentType(T billBO)
		{
			return billBO.ABL_ShipmentType;
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.AsycudaBill;
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(T sourceBO)
		{
			return sourceBO.GetUserDefinedValues();
		}

		Shipment IAsycudaBillDataObjectWriter.GetDataObject(AsycudaBill sourceBO) => GetDataObject(sourceBO as T);
	}
}
