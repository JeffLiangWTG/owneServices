using System.Data;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business.CustomValues;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusEntryLineFee : EU.Business.Declaration.CusEntryLineFee, Integration.Customs.ES.ICusEntryLineFee, IESDocSADHLineTaxBoxSupporter
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : EU.Business.Declaration.CusEntryLineFee.Schema
		{
			public const string MaxMin = "MaxMin";
			public const int MaxMinMaxLength = 2;
		}

		#region TypeSafe

		public new CusEntryLineFeeLookups Lookups => (CusEntryLineFeeLookups)base.Lookups;
		protected override Customs.Business.CusEntryLineFeeLookups GetNewLookups() => new CusEntryLineFeeLookups(this);

		public new CusEntryLineFeeValidation Validation => (CusEntryLineFeeValidation)base.Validation;
		protected override Customs.Business.CusEntryLineFeeValidation GetNewValidation() => new CusEntryLineFeeValidation(this);

		#endregion

		protected override bool AllowZeroOrEmptyAmountCore => !(new ZBool(((JobDeclaration)EntryLine?.Declaration)?.HasAnyDiffT2CEntry));

		[MaxLength(Schema.MaxMinMaxLength)]
		public ZString MaxMin
		{
			get => this.GetSystemDefinedValue<ZString>(GenAddOnHelper.MaxMin);
			set
			{
				var oldValue = MaxMin;
				CheckMaximumLength(MaxMinInfo, value);
				this.SetSystemDefinedValue(GenAddOnHelper.MaxMin, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMaxMin();
				}
				MaxMinInfo.RefreshBinding(oldValue);
			}
		}

		const string DecimalFormatSpain = "N2";

		protected override ZString GetDefaultMethodOfPaymentValue()
		{
			var addInfo = ESOrgImpAddInfo.Get(EntryLine?.Declaration?.Importer);
			var entryHeader = (CusEntryHeader)EntryLine?.Header;
			var isH2 = entryHeader?.IsH2Style ?? false;
			var isT2LorT2C = entryHeader != null && (entryHeader.IsT2L || entryHeader.IsT2C);
			var isInvoiceLineMethodOfPaymentA = (EntryLine?.RandomLine?.ZG_MethodOfPayment ?? ZString.Empty) == MethodOfPaymentList.Codes.A;
			var isSimplifiedImportH1 = entryHeader != null && entryHeader.IsImport && !entryHeader.IsH2Style && entryHeader.IsUCC6 && (entryHeader.EntryInstruction?.IsSubStyleBOrC ?? false);
			var methodOfPayment = ZString.Empty;

			if (isH2 || isT2LorT2C || isInvoiceLineMethodOfPaymentA || isSimplifiedImportH1)
			{
				methodOfPayment = UniversalReferenceConstants.FeeMethodOfPayment.NonBillableTax;
			}
			else if ((addInfo?.ZO_VATDeferment ?? false) && CF_ChargeType == RefCusRateCodes.Vat)
			{
				methodOfPayment = UniversalReferenceConstants.FeeMethodOfPayment.Deferred;
			}
			return methodOfPayment;
		}

		public ZPropertyInfo MaxMinInfo => GetZPropertyInfo(Schema.MaxMin);

		protected override bool GetIsNationalIndirectTaxationFee()
		{
			return Regex.IsMatch(CF_ChargeType, @"^[012456789]");
		}

		protected override IFeeRounder GetNewChargeAmountRounder() => new TwoDigitsChargeAmountRounder();

		protected override ZString RateDutyCore => (CF_MethodOfCalculation.IsEmpty || CF_MethodOfCalculation == Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage) ? new ZString(Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage) : CF_MethodOfCalculation.ConvertCargoWiseToES(Factory);

		ZBool IESDocSADHLineTaxBoxSupporter.DestinationStateIsCanaryIsland => new ZBool(((JobDeclaration)EntryLine?.Declaration)?.DestinationStateIsCanaryIsland);
		ZString IESDocSADHLineTaxBoxSupporter.EntryLineMethodOfPayment => EntryLine.RandomLine.ZG_MethodOfPayment;
		ZString IESDocSADHLineTaxBoxSupporter.EntryLineMethodOfPayment2 => ((CusEntryLine)EntryLine).RandomLine.ZG_MethodOfPayment2;

		protected override ZString FormatDecimalDefault(ZDecimal number) => number.ToString(DecimalFormatSpain);

		public bool IsVAT => CF_ChargeType == RefCusRateCodes.Vat;

		const string ChargeTypeB00 = "B00";
		const string ChargeType3IG = "3IG";

		public ZString GetTaxClass(bool isCanaryIslands) => (isCanaryIslands && CF_ChargeType == ChargeTypeB00) ? ChargeType3IG : CF_ChargeType;
	}
}
