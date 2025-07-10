using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	public class InventorySelectionHeader : DeclarationInventorySelectionHeader
	{
		public InventorySelectionHeader(BaseJobDeclaration declaration) : base(declaration)
		{
		}

		protected override void UpdateOutwardLineWithInventoryDetailCore(BaseJobComInvoiceLine invoiceLine, IWhsInventoryView inventory, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio)
		{
			base.UpdateOutwardLineWithInventoryDetailCore(invoiceLine, inventory, whsReceiveLine, whsBondedWarehouseAttribute, ratio);
			FillInvoiceLineWithInventoryDetails((JobComInvoiceLine)invoiceLine, whsReceiveLine, whsBondedWarehouseAttribute, ratio);
		}

		protected override void PopulateCountrySpecificInvoiceLineData(BaseJobComInvoiceLine invoiceLine, WhsInventoryWrapper inventoryWrapper, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, Dictionary<ZString, ZString> addInfos, ZDecimal invoiceQuantity, ZDecimal ratio)
		{
			base.PopulateCountrySpecificInvoiceLineData(invoiceLine, inventoryWrapper, whsReceiveLine, whsBondedWarehouseAttribute, addInfos, invoiceQuantity, ratio);
			FillInvoiceLineWithInventoryDetails((JobComInvoiceLine)invoiceLine, whsReceiveLine, whsBondedWarehouseAttribute, ratio, inventoryWrapper);
		}

		protected virtual void FillInvoiceLineWithInventoryDetails(JobComInvoiceLine invoiceLine, IWhsDocketLine whsReceiveLine,
			IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio, WhsInventoryWrapper inventoryWrapper = null)
		{
			var warehouseAttributeAddInfoDictionary = AddInfoParser.CreateDictionaryWithAddInfoString(whsBondedWarehouseAttribute.WB_AddInfo);

			FillLinePrice(invoiceLine, warehouseAttributeAddInfoDictionary, ratio);
			FillCharges(invoiceLine, whsBondedWarehouseAttribute, ratio);
			FillSupplementaryCodes(invoiceLine, whsBondedWarehouseAttribute);
			FillSecondAndThirdQuantity(invoiceLine, whsBondedWarehouseAttribute, ratio);
			FillOtherDetails(invoiceLine, warehouseAttributeAddInfoDictionary);
			FillGrossWeight(invoiceLine, warehouseAttributeAddInfoDictionary, ratio);
			if (invoiceLine.Declaration?.SupportInwardProcessing ?? false)
			{
				FillPreviousDocuments(invoiceLine, whsReceiveLine);
			}
			FillNetWeight(invoiceLine, invoiceLine.JI_CustomsQuantity, invoiceLine.JI_CustomsUnitQty);
		}

		void FillGrossWeight(JobComInvoiceLine invoiceLine, Dictionary<ZString, ZString> warehouseAttributeAddInfoDictionary, ZDecimal ratio)
		{
			if (warehouseAttributeAddInfoDictionary.TryGetValue(BondedWarehousingHelper.Constants.LineGrossWeight, out var grossWeightString)
				&& ZDecimal.TryParse(grossWeightString, out var grossWeight))
			{
				var effectiveGrossWeight = (ZDecimal)(grossWeight * ratio);
				invoiceLine.JI_Weight = effectiveGrossWeight.Round(JobComInvoiceLineSchema.JI_Weight.Scale);
			}

			if (warehouseAttributeAddInfoDictionary.TryGetValue(BondedWarehousingHelper.Constants.LineGrossWeightUnit, out var grossWeightUnit))
			{
				invoiceLine.JI_WeightUQ = grossWeightUnit;
			}
		}

		void FillNetWeight(JobComInvoiceLine invoiceLine, ZDecimal customsQuantity, ZString customsUnitQty)
		{
			if (customsUnitQty != Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram)
			{
				return;
			}

			using (invoiceLine.SuspendCalculateFromNetWeightToCustomsQty())
			{
				invoiceLine.JI_NetWeight = customsQuantity.Round(JobComInvoiceLineSchema.JI_NetWeight.Scale);

				if (invoiceLine.JI_NetWeightUQ != Core.Constants.Weight.Kilograms)
				{
					invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				}
			}
		}

		void FillLinePrice(JobComInvoiceLine invoiceLine, Dictionary<ZString, ZString> warehouseAttributeAddInfoDictionary, ZDecimal ratio)
		{
			if ((ShouldFillFinancialData(invoiceLine))
				&& warehouseAttributeAddInfoDictionary.TryGetValue(BondedWarehousingHelper.Constants.LinePrice, out var linePriceStr)
				&& ZDecimal.TryParse(linePriceStr, out var linePrice)
				&& warehouseAttributeAddInfoDictionary.TryGetValue(BondedWarehousingHelper.Constants.LinePriceCurrency, out var linePriceCurrency))
			{
				var linePriceRefCurrency = RefCurrency.LoadFromCurrencyCode(Factory, linePriceCurrency);
				if (linePriceRefCurrency != null)
				{
					var linePriceMoney = new Money(linePrice * ratio, linePriceRefCurrency);
					invoiceLine.JI_LinePrice = invoiceLine.CurrencyConverter.ConvertExact(linePriceMoney, invoiceLine.LinePriceRefCurrency ?? invoiceLine.LocalCurrency).Amount;
				}
			}
			else
			{
				invoiceLine.JI_LinePrice = 0m;
			}
		}

		protected override void SetDefaultCustomsProcedureCode(BaseJobComInvoiceLine invoiceLine)
		{
			if ((invoiceLine.Declaration?.SupportInwardProcessing ?? false) && invoiceLine.JI_Procedure.IsEmpty)
			{
				invoiceLine.JI_Procedure = DefaultInvoiceLineProcedureForInwardProcessing;
			}
		}

		protected virtual string DefaultInvoiceLineProcedureForInwardProcessing => Declaration.IsImport ? CPC_DefaultForImport4051000 : CPC_DefaultForExport3151000;

		const string CPC_DefaultForImport4051000 = "4051000";
		const string CPC_DefaultForExport3151000 = "3151000";

		protected virtual void FillCharges(JobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio)
		{
			if (ShouldFillFinancialData(invoiceLine))
			{
				var chargesQuery = new ZQuery(CusAddInfoSchema.B7_ParentTableCode, WhsBondedWarehouseAttributeSchema.Constants.Prefix)
				.AddToFilter(CusAddInfoSchema.B7_ParentID, whsBondedWarehouseAttribute.PK)
				.AddToFilter(CusAddInfoSchema.B7_Type, BondedWarehousingHelper.Constants.CommercialChargeAddInfo.Type);
				var charges = Factory.Load<WarehouseCustomsAttributeAddInfo>(chargesQuery);
				if (charges.Any())
				{
					invoiceLine.Charges.RemoveAndDeleteAll();
					foreach (var charge in charges)
					{
						var chargeAddInfoDict = AddInfoParser.CreateDictionaryWithAddInfoString(charge.B7_AddInfoData);
						if (chargeAddInfoDict.TryGetValue(BondedWarehousingHelper.Constants.CommercialChargeAddInfo.AddInfoKeys.ChargeType, out var chargeType)
							&& chargeAddInfoDict.TryGetValue(BondedWarehousingHelper.Constants.CommercialChargeAddInfo.AddInfoKeys.Amount, out var amountStr) && ZDecimal.TryParse(amountStr, out var amount)
							&& chargeAddInfoDict.TryGetValue(BondedWarehousingHelper.Constants.CommercialChargeAddInfo.AddInfoKeys.Currency, out var currency))
						{
							var newCharge = invoiceLine.Charges.AddNew(chargeType, amount * ratio, currency);
							newCharge.J7_IsDutiable = GetBooleanFromAddInfoDict(chargeAddInfoDict, BondedWarehousingHelper.Constants.CommercialChargeAddInfo.AddInfoKeys.IsDutiable);
							newCharge.J7_IsGSTApplicable = GetBooleanFromAddInfoDict(chargeAddInfoDict, BondedWarehousingHelper.Constants.CommercialChargeAddInfo.AddInfoKeys.IsGSTApplicable);
							newCharge.J7_IsIncludedInITOT = GetBooleanFromAddInfoDict(chargeAddInfoDict, BondedWarehousingHelper.Constants.CommercialChargeAddInfo.AddInfoKeys.IsIncludedInITOT);
							newCharge.J7_IsStatisticalValueApplicable = GetBooleanFromAddInfoDict(chargeAddInfoDict, BondedWarehousingHelper.Constants.CommercialChargeAddInfo.AddInfoKeys.IsStatisticalValueApplicable);
						}
					}
				}
			}
			else
			{
				invoiceLine.Charges.RemoveAndDeleteAll();
			}
		}

		void FillSupplementaryCodes(JobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
			var supplementaryCodesQuery = new ZQuery(CusAddInfoSchema.B7_ParentTableCode, WhsBondedWarehouseAttributeSchema.Constants.Prefix)
				.AddToFilter(CusAddInfoSchema.B7_ParentID, whsBondedWarehouseAttribute.PK)
				.AddToFilter(CusAddInfoSchema.B7_Type, BondedWarehousingHelper.Constants.SupplementaryCodeAddInfo.Type);
			var supplementaryCodes = Factory.Load<WarehouseCustomsAttributeAddInfo>(supplementaryCodesQuery);
			if (supplementaryCodes.Any())
			{
				invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
				foreach (var supplementaryCode in supplementaryCodes)
				{
					var supplementaryCodeAddInfoDict = AddInfoParser.CreateDictionaryWithAddInfoString(supplementaryCode.B7_AddInfoData);
					if (supplementaryCodeAddInfoDict.TryGetValue(BondedWarehousingHelper.Constants.SupplementaryCodeAddInfo.AddInfoKeys.Code, out var code) && supplementaryCodeAddInfoDict.TryGetValue(BondedWarehousingHelper.Constants.SupplementaryCodeAddInfo.AddInfoKeys.Order, out var orderStr) && ZShort.TryParse(orderStr, out var order))
					{
						if (order == 1)
						{
							invoiceLine.JI_SupplementaryCode1 = code;
						}
						else if (order == 2)
						{
							invoiceLine.JI_SupplementaryCode2 = code;
						}
						else if (order >= 3)
						{
							invoiceLine.AdditionalSupplementaryCodes.AddNew(code);
						}
					}
				}
			}
		}

		void FillSecondAndThirdQuantity(JobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio)
		{
			invoiceLine.JI_CustomsSecondQuantity = whsBondedWarehouseAttribute.WB_CustomsSecondQuantity * ratio;
			invoiceLine.JI_CustomsSecondUnitQty = whsBondedWarehouseAttribute.WB_CustomsSecondUnitQty;

			invoiceLine.JI_CustomsThirdQuantity = whsBondedWarehouseAttribute.WB_CustomsThirdQuantity * ratio;
			invoiceLine.JI_CustomsThirdUnitQty = whsBondedWarehouseAttribute.WB_CustomsThirdUnitQty;
		}

		void FillOtherDetails(JobComInvoiceLine invoiceLine, Dictionary<ZString, ZString> warehouseAttributeAddInfoDictionary)
		{
			if (warehouseAttributeAddInfoDictionary.TryGetValue(BondedWarehousingHelper.Constants.CountryOfSupply, out var countryOfSupply))
			{
				invoiceLine.ZG_CountryOfSupply = countryOfSupply;
			}

			if (warehouseAttributeAddInfoDictionary.TryGetValue(BondedWarehousingHelper.Constants.ValuationCode, out var valuationCode))
			{
				invoiceLine.JI_ValuationCode = valuationCode;
			}
		}

		protected virtual void FillPreviousDocuments(JobComInvoiceLine invoiceLine, IWhsDocketLine inventory)
		{
		}

		ZBool GetBooleanFromAddInfoDict(Dictionary<ZString, ZString> addInfoDict, ZString addInfoKey)
		{
			return addInfoDict.TryGetValue(addInfoKey, out var str) && ZBool.TryParse(str, out var value) && value;
		}

		protected virtual bool ShouldFillFinancialData(JobComInvoiceLine invoiceLine) => invoiceLine.Declaration?.IsImport ?? false;
	}
}
