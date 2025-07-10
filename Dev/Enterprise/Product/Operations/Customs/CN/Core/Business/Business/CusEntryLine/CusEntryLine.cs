using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public partial class CusEntryLine : Customs.Business.CusEntryLine, Integration.Customs.CN.ICusEntryLine
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public new class Schema : Customs.Business.CusEntryLine.Schema
		{
			public const string ExciseAmount = "ExciseAmount";
			public const string EntryLRNAndEntryLineNo = "EntryLRNAndEntryLineNo";
		}

		protected override bool CanBeLinkedUpByPivot => Header?.IsChildEntry ?? false;

		protected override ZDecimal GetInvoiceLineCustomsValueToAggregate(BaseJobComInvoiceLine invoiceLine)
		{
			return 0m;
		}

		#region Duties & Taxes

		public ZString UniversalDutyRateType => Header.IsEntering ? Constants.UniversalReferenceConstants.RefCusRateTypes.CustomsDuty : Constants.UniversalReferenceConstants.RefCusRateTypes.ExportDuty;

		protected override ZDecimal GetDutyAmountCore()
		{
			return Declaration.IsDeclarationIntegrated ? Fees.GetAmount(Constants.EntryChargeTypes.CustomsDuty) : GetFeeAmountByRateType(UniversalDutyRateType);
		}

		public ZString UniversalVATRateType => RandomLine?.JI_ZZF_NKTaxType ?? Constants.UniversalReferenceConstants.RefCusRateTypes.VAT;

		protected override ZDecimal GetGSTVATAmountCore()
		{
			return Declaration.IsDeclarationIntegrated ? Fees.GetAmount(Constants.EntryChargeTypes.Vat) : GetFeeAmountByRateType(Constants.UniversalReferenceConstants.RefCusRateTypes.VAT);
		}

		public ZDecimal ExciseAmount => Declaration.IsDeclarationIntegrated ? ZDecimal.Zero : GetFeeAmountByRateType(Universal.Constants.RateTypes.Excise);

		public ZDecimal AntiDumpingAmount => Declaration.IsDeclarationIntegrated ? ZDecimal.Zero : GetFeeAmountByRateType(Universal.Constants.RateTypes.AntiDumping);

		public ZDecimal CountervailingAmount => Declaration.IsDeclarationIntegrated ? ZDecimal.Zero : GetFeeAmountByRateType(Universal.Constants.RateTypes.Countervailing);

		ZDecimal GetFeeAmountByRateType(ZString rateType)
		{
			return GetFees(rateType).Sum(x => x.CF_ChargeAmount);
		}

		public IEnumerable<CusEntryLineFee> GetFees(ZString rateType)
		{
			var rateCodes = rateType == Constants.UniversalReferenceConstants.RefCusRateTypes.VAT ? new ZString[] { UniversalVATRateType } : CNRefCusRateCodeLoader.GetRateCodesByRateType(Factory, rateType);

			foreach (var rateCode in rateCodes)
			{
				var fee = Fees.GetElementWithThisCode(rateCode);
				if (fee != null)
				{
					yield return fee;
				}
			}
		}

		#endregion

		#region New Properties

		public ZDecimal CustomsValueInUSD
		{
			get
			{
				var usdCurrency = Factory.GetCachedValue("RefCurrency_USD", () => RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates));
				var customsValueInUSD = CurrencyConverter.ConvertExact(CustomsValue, usdCurrency);
				return customsValueInUSD?.Amount ?? ZDecimal.Zero;
			}
		}

		public ZString CustomsSupervisionConditions => (Header.IsEntering ? Tariff?.InwardSupervisionConditions() : Tariff?.OutwardSupervisionConditions()) ?? ZString.Empty;

		public ZString InspectionSupervisionConditions => (Header.IsEntering ? Tariff?.CIQImportRequirements() : Tariff?.CIQExportRequirements()) ?? ZString.Empty;

		TariffView Tariff => RandomLine?.UniversalTariff;

		public ZString EntryLRNAndEntryLineNo
		{
			get
			{
				ZString lrn = Header.CH_BGMReference;
				ZString lineNum = CL_LineNumber.ToString().PadLeft(Header.CountOfLines.ToString().Length, '0');
				return lrn.IsEmpty ? string.Empty : string.Format(CultureInfo.InvariantCulture, "{0}/{1}", lrn, lineNum);
			}
		}

		public bool RequiresLegalInspection
		{
			get
			{
				var result = false;
				var tariff = Tariff;
				if (tariff != null)
				{
					var isImport = Declaration.IsImport;
					var isExport = Declaration.IsExport;

					result = isImport && (tariff.HasImportCUSRequirementA() || tariff.HasImportCIQRequirementL())
						|| isExport && tariff.HasExportCUSRequirementB();
				}
				return result;
			}
		}

		#endregion
	}
}
