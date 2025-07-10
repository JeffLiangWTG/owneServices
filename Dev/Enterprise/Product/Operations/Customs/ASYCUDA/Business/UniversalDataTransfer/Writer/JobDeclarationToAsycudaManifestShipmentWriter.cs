using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Shipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class JobDeclarationToAsycudaManifestShipmentWriter : TopLevelDataObjectWriter<BaseJobDeclaration, Shipment>
	{
		public JobDeclarationToAsycudaManifestShipmentWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.AsycudaManifest;
		}

		protected override void PopulateDataObject(BaseJobDeclaration declaration, Shipment shipment)
		{
			PopulateHeaderData(declaration, shipment);
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var houseBillData = new Shipment(writeManager.WriterStrategy);
			shipment.SubShipmentCollection?.Add(houseBillData);
			PopulateHouseBillData(houseBillData, declaration);
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(BaseJobDeclaration sourceBO)
		{
			return sourceBO.GetUserDefinedValues();
		}

		protected override void InsertParents(BaseJobDeclaration sourceBO, ref Shipment dataObject)
		{
			base.InsertParents(sourceBO, ref dataObject);
			var obj = dataObject;
			var houseBillData = dataObject.SubShipmentCollection.FirstOrDefault();
			if (houseBillData != null)
			{
				houseBillData.SetCustomizedFieldCollection(() => obj.CustomizedFieldCollection);
				dataObject.SetCustomizedFieldCollection(() => null);
			}
		}

		protected virtual void PopulateHeaderData(BaseJobDeclaration declaration, Shipment shipment)
		{
			shipment.WayBillNumber = declaration.JE_MasterBill;
			shipment.VesselName = declaration.JE_VesselName;
			shipment.VoyageFlightNo = declaration.JE_VoyageFlightNo;
			shipment.TransportMode = new CodeDescriptionPair() { Code = declaration.JE_TransportMode };
			shipment.PortOfLoading = new UNLOCO() { Code = declaration.JE_RL_NKPortOfLoading };
			shipment.PortOfDischarge = new UNLOCO() { Code = declaration.JE_RL_NKPortOfArrival };
			shipment.SetDateCollection(() => CreateHeaderDates(declaration));
			PopulateCustomizedFields(shipment, declaration);
		}

		void PopulateHouseBillData(Shipment shipment, BaseJobDeclaration declaration)
		{
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);
			shipment.DataContext = dataContext;
			var countryCode = declaration.CountryCode;
			var entryInstruction = new UniversalCustoms.EntryInstruction(writeManager.WriterStrategy);
			PopulateHouseBillEntryInstruction(entryInstruction, declaration);
			entryInstruction.Link = 1;
			shipment.SetEntryInstructionCollection(() =>
			{
				var result = new List<UniversalCustoms.EntryInstruction>();
				result.Add(entryInstruction);
				return result;
			});
			shipment.SetEntryHeaderCollection(() =>
			{
				var result = new List<UniversalCustoms.EntryHeader>();
				var entryHeader = new UniversalCustoms.EntryHeader();
				result.Add(entryHeader);
				PopulateHouseBillEntryHeader(entryHeader, declaration);
				entryHeader.EntryInstructionLink = 1;
				entryHeader.Type = new EntryType() { Code = countryCode };
				return result;
			});
			PopulateHouseBillMainData(shipment, declaration);
			PolulateCommercialData(declaration, shipment, entryInstruction, countryCode);
		}

		protected void PopulateCustomizedFields(Shipment declarationData, BaseJobDeclaration declarationBO)
		{
			var docsBO = declarationBO.DocsAndCartage;
			if (docsBO != null)
			{
				declarationData.SetCustomizedFieldCollection(() =>
				{
					var customFieldsWritingHelper = new CustomFieldsDataObjectWritingHelper<JobDocsAndCartage>(docsBO, new JobDocsAndCartageCustomFieldsDescriptor());
					var docsBOCustomFields = customFieldsWritingHelper.GetCustomizedFieldValues().ToArray();

					if (docsBOCustomFields.Any())
					{
						var customizedFieldCollection = declarationData.CustomizedFieldCollection ?? new List<CustomizedField>();
						customizedFieldCollection.AddRange(docsBOCustomFields);
						return customizedFieldCollection.Count > 0 ? customizedFieldCollection : null;
					}
					return declarationData.CustomizedFieldCollection;
				});
			}
		}

		protected virtual void PopulateHouseBillEntryHeader(UniversalCustoms.EntryHeader entryHeader, BaseJobDeclaration declaration)
		{
		}

		protected virtual void PopulateHouseBillEntryInstruction(UniversalCustoms.EntryInstruction entryInstruction, BaseJobDeclaration declaration)
		{
		}

		protected virtual void PopulateHouseBillMainData(Shipment shipment, BaseJobDeclaration declaration)
		{
			shipment.WayBillType = new WayBillType() { Code = Core.Constants.ShipmentTypes.StandardHouse };
			shipment.WayBillNumber = GetHouseBillNumber(declaration);
			shipment.TotalWeight = declaration.JE_TotalWeight;
			shipment.TotalWeightUnit = new UnitOfWeight { Code = declaration.JE_TotalWeightUnit };
			shipment.TotalVolume = declaration.JE_TotalVolume;
			shipment.TotalVolumeUnit = new UnitOfVolume { Code = declaration.JE_TotalVolumeUnit };
			shipment.PortOfOrigin = new UNLOCO() { Code = declaration.JE_RL_NKOrigin };
			shipment.PortOfDestination = new UNLOCO() { Code = declaration.JE_RL_NKFinalDestination };
			shipment.GoodsDescription = declaration.JE_GoodsDescription;
			shipment.OuterPacks = declaration.JE_TotalNoOfPacks;
			shipment.OuterPacksPackageType = new PackageType() { Code = declaration.JE_TotalNoOfPacksPackType };
			shipment.AddOrgAddress(writeManager, declaration.NotifyParty, DocAddressType.NotifyParty);
			shipment.AddOrgAddress(writeManager, declaration.Supplier, DocAddressType.ConsignorDocumentaryAddress);
			shipment.AddOrgAddress(writeManager, declaration.Importer, DocAddressType.ConsigneeDocumentaryAddress);
		}

		protected virtual ZString GetHouseBillNumber(BaseJobDeclaration declaration) => declaration.JE_HouseBill;

		protected virtual List<Date> CreateHeaderDates(BaseJobDeclaration declaration)
		{
			var list = new List<Date>();
			list.Add(DateType.Departure, ZBool.False, declaration.JE_ExportDate);
			list.Add(DateType.Arrival, ZBool.False, declaration.JE_DateOfArrival);
			return list;
		}

		void PolulateCommercialData(BaseJobDeclaration declaration, Shipment shipment, UniversalCustoms.EntryInstruction entryInstruction, ZString countryCode)
		{
			bool addExciseValueToDutyAmount = ManifestCustomsDataRegistry.Instance.AddExciseValueToDutyAmount.GetValueWithoutFallback(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty);
			var commercialChargeCollection = new List<UniversalCustoms.CommercialCharge>();
			var packingLines = new DataObjectList<PackingLine>();
			var totalOverseaFreight = Money.Empty;
			var totalOverseasInsurance = Money.Empty;
			var totalOtherCharges = Money.Empty;
			var totalDiscount = Money.Empty;
			var totalGoodsValue = Money.Empty;
			var totalCustomsValue = ZDecimal.Zero;
			var totalTaxAmount = ZDecimal.Zero;
			var totalDutyAmount = ZDecimal.Zero;
			var localCurrency = declaration.Company?.LocalCurrency ?? GlbCompany.CurrentCompany.LocalCurrency;
			foreach (var invoice in declaration.Invoices.Cast<BaseJobComInvoiceHeader>().OrderBy(x => x.JZ_InvoiceNumber))
			{
				var currencyConverter = invoice.CurrencyConverter;
				var isInvoiceCurrExRateUserEnterable = invoice.IsJZ_InvoiceCurrExRateUserEnterable;
				var invoiceCurrency = invoice.Invoice_Currency;
				foreach (var invoiceLine in invoice.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x.JI_LineNo))
				{
					var packingLineData = new PackingLine(writeManager.WriterStrategy);
					packingLines.Add(packingLineData);
					var customsValue = invoiceLine.JI_CustomsValue;
					totalCustomsValue += customsValue;
					var dutyAmount = invoiceLine.JI_Calc_DutyAmountIncludingWHEstimate;
					if (addExciseValueToDutyAmount)
					{
						dutyAmount += GetExciseAmount(invoiceLine);
					}
					totalDutyAmount += dutyAmount;
					var linePrice = new Money(invoiceLine.JI_LinePrice, invoiceCurrency);
					if (isInvoiceCurrExRateUserEnterable)
					{
						linePrice = currencyConverter.ConvertExact(linePrice, localCurrency);
					}
					totalGoodsValue = currencyConverter.Add(totalGoodsValue, linePrice);
					var taxAmount = invoiceLine.JI_Calc_GSTVATAmountIncludingWHEstimate.Round(2);
					totalTaxAmount += taxAmount;
					GatherChargeDetails(invoiceLine.ApportionedCharges.Cast<BaseJobComInvHeaderCharge>(), currencyConverter, ref totalOverseaFreight, ref totalOverseasInsurance, ref totalOtherCharges, ref totalDiscount);
					GatherChargeDetails(invoiceLine.Charges.Cast<BaseJobComInvHeaderCharge>(), currencyConverter, ref totalOverseaFreight, ref totalOverseasInsurance, ref totalOtherCharges, ref totalDiscount);
					PopulatePackingLineData(packingLineData, invoiceLine, linePrice);
					packingLineData.SetPackingLineCollection(() =>
					{
						var packingLineCountryData = new PackingLine(writeManager.WriterStrategy);
						var list = new List<PackingLine>();
						list.Add(packingLineCountryData);
						PopulatePackingLineCountryData(packingLineCountryData, declaration, invoice, invoiceLine, countryCode, customsValue, dutyAmount, taxAmount);
						return list;
					});
				}
			}
			shipment.SetPackingLineCollection(() => packingLines); // Should this be wrapping the entire loop above? Unsure. Too much code here.
			AddCharge(commercialChargeCollection, Constants.CustomsChargeType.CustomsDutyCode, new Money(totalDutyAmount, localCurrency));
			AddCharge(commercialChargeCollection, Constants.CustomsChargeType.CustomsChargeCode, new Money(totalCustomsValue, localCurrency));
			AddCharge(commercialChargeCollection, CustomsChargeTypeList.Codes.Discount, totalDiscount);
			AddCharge(commercialChargeCollection, CustomsChargeTypeList.Codes.ExWorks, totalGoodsValue);
			AddCharge(commercialChargeCollection, Constants.CustomsChargeType.GSTCode, new Money(totalTaxAmount, localCurrency));
			AddCharge(commercialChargeCollection, CustomsChargeTypeList.Codes.OverseasFreight, totalOverseaFreight);
			AddCharge(commercialChargeCollection, CustomsChargeTypeList.Codes.OverseasInsurance, totalOverseasInsurance);
			AddCharge(commercialChargeCollection, CustomsChargeTypeList.Codes.OtherCharges, totalOtherCharges);
			AddAddInfo(entryInstruction, AddInfoConstants.Bill.TaxAmount, totalTaxAmount);
			AddAddInfo(entryInstruction.AddInfoCollection, AddInfoConstants.Bill.DutyAmount, totalDutyAmount);

			shipment.CommercialInfo = new UniversalCustoms.CommercialInfo()
			{
				CommercialChargeCollection = commercialChargeCollection
			};
		}

		void GatherChargeDetails(IEnumerable<BaseJobComInvHeaderCharge> charges, CurrencyConverter currencyConverter, ref Money totalOverseaFreight, ref Money totalOverseasInsurance, ref Money totalOtherCharges, ref Money totalDiscount)
		{
			foreach (var charge in charges)
			{
				if (IsOverseasFreight(charge.J7_ChargeType))
				{
					totalOverseaFreight = currencyConverter.Add(totalOverseaFreight, charge.Money);
				}
				else if (IsOverseasInsurance(charge.J7_ChargeType))
				{
					totalOverseasInsurance = currencyConverter.Add(totalOverseasInsurance, charge.Money);
				}
				else if (IsOtherCharges(charge.J7_ChargeType))
				{
					totalOtherCharges = currencyConverter.Add(totalOtherCharges, charge.Money);
				}
				else if (IsDiscount(charge.J7_ChargeType))
				{
					totalDiscount = currencyConverter.Add(totalDiscount, charge.Money);
				}
			}
		}

		protected virtual bool IsOverseasFreight(ZString chargeType)
		{
			return chargeType == CustomsChargeTypeList.Codes.OverseasFreight;
		}

		protected virtual bool IsOverseasInsurance(ZString chargeType)
		{
			return chargeType == CustomsChargeTypeList.Codes.OverseasInsurance;
		}

		protected virtual bool IsOtherCharges(ZString chargeType)
		{
			return chargeType == CustomsChargeTypeList.Codes.OtherCharges;
		}

		protected virtual bool IsDiscount(ZString chargeType)
		{
			return chargeType == CustomsChargeTypeList.Codes.Discount;
		}

		protected virtual ZDecimal GetExciseAmount(BaseJobComInvoiceLine invoiceLine)
		{
			return ZDecimal.Zero;
		}

		void AddCharge(List<UniversalCustoms.CommercialCharge> charges, ZString chargeType, Money value)
		{
			charges.Add(new UniversalCustoms.CommercialCharge()
			{
				ChargeType = new CodeDescriptionPair() { Code = chargeType },
				Currency = new Currency() { Code = value.Currency?.Code ?? ZString.Empty },
				Amount = value.Amount
			});
		}

		void PopulatePackingLineCountryData(PackingLine packingLineCountryData, BaseJobDeclaration declaration, BaseJobComInvoiceHeader invoice, BaseJobComInvoiceLine invoiceLine, ZString countryCode, ZDecimal customsValue, ZDecimal dutyAmount, ZDecimal taxAmount)
		{
			packingLineCountryData.CountryOfOrigin = new Country { Code = invoiceLine.JI_CountryOfOrigin };
			packingLineCountryData.GoodsDescription = invoiceLine.JI_Description;
			packingLineCountryData.HarmonisedCode = invoiceLine.JI_Tariff;
			packingLineCountryData.SetAddInfoGroupCollection(() =>
			{
				var addInfoCollection = new List<AddInfo>();
				AddAddInfo(addInfoCollection, AddInfoConstants.PackedItem.Country, countryCode);
				AddAddInfo(addInfoCollection, AddInfoConstants.PackedItem.CustomsValue, customsValue);
				AddAddInfo(addInfoCollection, AddInfoConstants.PackedItem.CustomsQty, invoiceLine.JI_CustomsQuantity);
				AddAddInfo(addInfoCollection, AddInfoConstants.PackedItem.CustomsUQ, invoiceLine.JI_CustomsUnitQty);
				AddAddInfo(addInfoCollection, AddInfoConstants.PackedItem.DutyValue, dutyAmount);
				AddAddInfo(addInfoCollection, AddInfoConstants.PackedItem.TaxValue, taxAmount);
				PopulateAdditionalPackingLineCountryData(addInfoCollection, declaration, invoice, invoiceLine);
				var list = new List<UniversalCustoms.AddInfoGroup>(new[]
				{
					new UniversalCustoms.AddInfoGroup()
					{
						Type = new CodeDescriptionPair() { Code = AddInfoConstants.PackedItem.PackingItemAddInfoType, Description = AddInfoConstants.PackedItem.PackingItemAddInfoTypeDescription },
						AddInfoCollection = addInfoCollection,
					}
				});
				return list;
			});
		}

		protected virtual void PopulateAdditionalPackingLineCountryData(List<AddInfo> addInfoCollection, BaseJobDeclaration declaration, BaseJobComInvoiceHeader invoice, BaseJobComInvoiceLine invoiceLine)
		{
		}

		protected void AddAddInfo(IAddInfoCollectionParent parent, ZString key, IZType value, bool addEmpty = true)
		{
			parent.AddAddInfo(key, value, addEmpty);
		}

		protected void AddAddInfo(List<AddInfo> addInfoCollection, ZString key, IZType value, bool addEmpty = true)
		{
			addInfoCollection.AddAddInfo(key, value, addEmpty);
		}

		protected virtual void PopulatePackingLineData(PackingLine packingLineData, BaseJobComInvoiceLine invoiceLine, Money linePrice)
		{
			packingLineData.Commodity = new Commodity() { Code = invoiceLine.JI_RH_NKCommodity_Code };
			packingLineData.GoodsDescription = invoiceLine.JI_Description;
			packingLineData.PackQty = invoiceLine.JI_InvoiceQuantity.ToZLong();
			packingLineData.PackType = new PackageType() { Code = invoiceLine.JI_InvoiceUQ };
			packingLineData.Weight = invoiceLine.JI_Weight;
			packingLineData.WeightUnit = new UnitOfWeight() { Code = invoiceLine.JI_WeightUQ };
			packingLineData.Volume = invoiceLine.JI_Volume;
			packingLineData.VolumeUnit = new UnitOfVolume() { Code = invoiceLine.JI_VolumeUQ };
			packingLineData.LinePrice = linePrice.Amount;
			packingLineData.LinePriceCurrency = Currency.New((IRefCurrency)linePrice.Currency);
			packingLineData.SetUNDGCollection(() => GetUNDGs(invoiceLine));
			packingLineData.ContainerNumber = invoiceLine.ContainersPivot.Cast<CusContainerInvoiceLinePivot>().Select(x => x.ContainerNumber).OrderBy(x => x).FirstOrDefault();
			packingLineData.ItemNo = invoiceLine.JI_LineNo;
		}

		List<UNDG> GetUNDGs(BaseJobComInvoiceLine invoiceLine)
		{
			List<UNDG> list = null;
			if (invoiceLine.UNDGs?.Count > 0)
			{
				var packageUNDG = invoiceLine.UNDGs[0];
				var writer = new UNDGDataObjectWriter(writeManager);
				var undgData = writer.GetDataObject(packageUNDG);
				list = new List<UNDG> { undgData };
			}
			return list;
		}
	}
}
