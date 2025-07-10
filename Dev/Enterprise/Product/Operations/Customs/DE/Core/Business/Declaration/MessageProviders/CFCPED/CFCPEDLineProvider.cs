using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public class CFCPEDLineProvider : MonthlyClosingDecLineProvider, ICFCPEDLine
	{
		public CFCPEDLineProvider(CusReconEntryLine entryLine, bool isModificationMessage) : base(entryLine, isModificationMessage)
		{
		}

		public string CessionManagementFlag => IsModificationMessage ? LineProvider.CessionManagementFlag : CurrentSnapshot.CessionManagementFlag;

		public string PreferentialOriginCountry => IsModificationMessage ? LineProvider.PreferentialOriginCountry : CurrentSnapshot.PreferentialCountry;

		public string TobaccoRevenueStampNumber => LineProvider.TobaccoRevenueStampNumber;

		public decimal AssessmentOutwardProcessingFee => CachedValueHelper.GetValue(ref assessmentOutwardProcessingFee, () => GetOutwardProcessingFeeOrTaxCosts(new[] { ImportChargeCodeList.Codes.OPF }));
		CachedValue<decimal> assessmentOutwardProcessingFee;

		public decimal AssessmentTaxCosts => CachedValueHelper.GetValue(ref assessmentTaxCosts, () => GetOutwardProcessingFeeOrTaxCosts(new[] { ImportChargeCodeList.Codes.TCE, ImportChargeCodeList.Codes._014 }));
		CachedValue<decimal> assessmentTaxCosts;

		public ILinePreferentialTreatment PreferentialTreatment => IsModificationMessage ? LineProvider.PreferentialTreatment : LinePreferentialTreatmentFromSnapshotProvider.NewOrNull(CurrentSnapshot.PreferentialTreatment);

		public IReadOnlyCollection<IImportSpecialCase> SpecialCase => specialCase ?? (specialCase = EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(l => l.Taxes.Cast<JobComInvoiceLineTax>(), (_, t) => new ImportSpecialCaseProvider(t)).ToArray());
		IReadOnlyCollection<IImportSpecialCase> specialCase;

		protected override IImportDecHeader GetHeaderProvider(CusEntryHeader entryHeader) => new CFCRECHeaderProvider(entryHeader);

		protected override IImportDecLine GetLineProvider(CusEntryLine entryLine) => new CFCRECLineProvider(entryLine);

		decimal IImportDecLine.AssessmentCustomsValue
		{
			get
			{
				var result = decimal.Zero;
				if (!Declaration.ZG_IsHighValueOvrd && !IsProcedureInE01OrE02)
				{
					result = IsModificationMessage ? LineProvider.AssessmentCustomsValue : CurrentSnapshot?.Assessment?.CustomsValue ?? decimal.Zero;
				}
				return result;
			}
		}

		new ICFCRECLine LineProvider => (ICFCRECLine)base.LineProvider;

		decimal GetOutwardProcessingFeeOrTaxCosts(string[] chargeTypes)
		{
			var sumCharges = InvoiceLines.SelectMany(l => l.Charges.Cast<InvoiceLineCharge>().Where(c => c.J7_ChargeType.ToString().In(chargeTypes))).Sum(c => c.CurrencyConverter.ConvertExact(c.Money, EURCurrency).Amount);
			var sumApportionedCharges = InvoiceLines.SelectMany(l => l.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Where(c => c.J7_ChargeType.ToString().In(chargeTypes))).Sum(c => c.CurrencyConverter.ConvertExact(c.Money, EURCurrency).Amount);
			return decimal.Round(sumCharges + sumApportionedCharges, 2);
		}

		RefCurrency EURCurrency => CachedValueHelper.GetValue(ref eurCurrency, () => RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.EuropeanUnion));
		CachedValue<RefCurrency> eurCurrency;
	}
}
