using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public sealed class CFCDECLineProvider : SingleDecLineProvider, ICFCDECLine
	{
		public CFCDECLineProvider(CusEntryLine entryLine) : base(entryLine)
		{
		}

		public string TobaccoRevenueStampNumber => RandomInvoiceLine.JI_TobaccoStamp;

		public IImportLineCustomsValue CustomsValue => CachedValueHelper.GetValue(ref customsValue, () => Declaration.ZG_IsHighValueOvrd && !IsProcedureInE01OrE02 ? new ImportLineCustomsValueProvider(Declaration, InvoiceLines) : null);
		CachedValue<IImportLineCustomsValue> customsValue;

		public decimal AssessmentOutwardProcessingFee => GetAssessmentOutwardProcessingFeeOrTaxCosts(new ZString[] { ImportChargeCodeList.Codes.OPF });

		public decimal AssessmentTaxCosts => GetAssessmentOutwardProcessingFeeOrTaxCosts(new ZString[] { ImportChargeCodeList.Codes.TCE, ImportChargeCodeList.Codes._014 });

		public ILinePreferentialTreatment PreferentialTreatment => CachedValueHelper.GetValue(ref preferentialTreatment, () => Declaration.JE_EntryStyle == EntryStyleListImport.Codes.ImportFromSpecialTerritory || IsProcedureInF01OrF02OrF03 ? null : new LinePreferentialTreatmentProvider(RandomInvoiceLine));
		CachedValue<ILinePreferentialTreatment> preferentialTreatment;

		public IReadOnlyCollection<IImportSpecialCase> SpecialCases => specialCases ?? (specialCases = RandomInvoiceLine.Taxes.Cast<JobComInvoiceLineTax>().Select(c => new ImportSpecialCaseProvider(c)).ToArray());
		IReadOnlyCollection<IImportSpecialCase> specialCases;

		public string PreferentialOriginCountry => (int.TryParse(RandomInvoiceLine.JI_PrimaryPreference, out var primaryPreferenceAsInteger) && primaryPreferenceAsInteger >= 200) ? (string)RandomInvoiceLine.ZG_CountryOfSupply : null;

		public string CessionManagementFlag
		{
			get
			{
				string result = null;
				var procedure = RandomInvoiceLine.JI_Procedure;
				if (!procedure.IsEmpty && !procedure.SubstringSafe(4, 3).IsEmpty)
				{
					result = RandomInvoiceLine.JI_CessionFlag;
				}
				return result;
			}
		}

		bool IsProcedureInF01OrF02OrF03 => RandomInvoiceLine.Concession == CustomsProcedureCodeList.Import.Concession._F01 || RandomInvoiceLine.Concession == CustomsProcedureCodeList.Import.Concession._F02 || RandomInvoiceLine.Concession == CustomsProcedureCodeList.Import.Concession._F03;

		bool IsProcedureInE01OrE02 => RandomInvoiceLine.Concession == CustomsProcedureCodeList.Import.Concession._E01 || RandomInvoiceLine.Concession == CustomsProcedureCodeList.Import.Concession._E02;

		decimal GetAssessmentOutwardProcessingFeeOrTaxCosts(ZString[] chargeTypes)
		{
			var sumCharges = InvoiceLines.SelectMany(l => l.Charges.Cast<InvoiceLineCharge>()).Where(c => c.J7_ChargeType.In(chargeTypes)).Sum(c => c.CurrencyConverter.ConvertExact(c.Money, EURCurrency).Amount);
			var sumApportionedCharges = InvoiceLines.SelectMany(l => l.ApportionedCharges.Cast<InvoiceLineApportionCharge>()).Where(c => c.J7_ChargeType.In(chargeTypes)).Sum(c => c.CurrencyConverter.ConvertExact(c.Money, EURCurrency).Amount);
			return new ZDecimal(sumCharges + sumApportionedCharges).FormatDecimal(2);
		}

		RefCurrency EURCurrency => CachedValueHelper.GetValue(ref eurCurrency, () => RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.EuropeanUnion));
		CachedValue<RefCurrency> eurCurrency;
	}
}
