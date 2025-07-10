using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.FR.Business.Declaration
{
	[SystemDefinedValues]
	public class CusEntryHeaderCharges : EU.Business.Declaration.CusEntryHeaderCharges
	{
		public CusEntryHeaderCharges(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new partial class Schema : AutoCusEntryHeaderCharges.Schema
		{
			public const string ChargePaymentOrDestinationID = "ChargePaymentOrDestinationID";
			public const string NationalFeeTypeCode = "NationalFeeTypeCode";
			public const int NationalFeeTypeCodeMaxLength = 5;
			public const string TaxStatus = "TaxStatus";
			public const int TaxStatusMaxLength = 5;
		}

		[MaxLength(Schema.NationalFeeTypeCodeMaxLength)]
		[ResourceStringData("FR.CusEntryHeaderCharges.NationalFeeTypeCode", Caption = "National Type")]
		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderChargesLookups.NationalFeeTypeCodeList))]
		public ZString NationalFeeTypeCode
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.NationalFeeTypeCode);
			set
			{
				var oldValue = NationalFeeTypeCode;
				CheckMaximumLength(NationalFeeTypeCodeInfo, value);
				this.SetSystemDefinedValue(Schema.NationalFeeTypeCode, value);
				NationalFeeTypeCodeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo NationalFeeTypeCodeInfo => GetZPropertyInfo(Schema.NationalFeeTypeCode);

		[MaxLength(Schema.TaxStatusMaxLength)]
		[ResourceStringData("FR.CusEntryHeaderCharges.TaxStatus", Caption = "Tax Status")]
		public ZString TaxStatus
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.TaxStatus);
			set
			{
				var oldValue = TaxStatus;
				CheckMaximumLength(TaxStatusInfo, value);
				this.SetSystemDefinedValue(Schema.TaxStatus, value);
				TaxStatusInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TaxStatusInfo => GetZPropertyInfo(Schema.TaxStatus);

		public new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;

		[ResourceStringData("FR.CusEntryHeaderCharges.C1_ChargeType", Caption = "Fee Code")]
		public override ZString C1_ChargeType { get => base.C1_ChargeType; set => base.C1_ChargeType = value; }

		[ReadOnlyMember(nameof(C1_ChargeAmountReadOnly))]
		[ResourceStringData("FR.CusEntryHeaderCharges.C1_ChargeAmount", Caption = "Amount")]
		public override ZDecimal C1_ChargeAmount { get => base.C1_ChargeAmount; set => base.C1_ChargeAmount = value; }

		protected ZBool C1_ChargeAmountReadOnly => C1_RateOverrideReasonCode.IsEmpty;

		[ResourceStringData("FR.CusEntryHeaderCharges.C1_MethodOfPayment", Caption = "Method Of Payment")]
		public override ZString C1_MethodOfPayment { get => base.C1_MethodOfPayment; set => base.C1_MethodOfPayment = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderChargesLookups.RateOverrideReasonCodeList))]
		[ResourceStringData("FR.CusEntryHeaderCharges.C1_RateOverrideReasonCode", Caption = "Action")]
		public override ZString C1_RateOverrideReasonCode { get => base.C1_RateOverrideReasonCode; set => base.C1_RateOverrideReasonCode = value; }

		public new CusEntryHeaderChargesValidation Validation => (CusEntryHeaderChargesValidation)base.Validation;

		public new CusEntryHeaderChargesLookups Lookups => (CusEntryHeaderChargesLookups)base.Lookups;

		protected override Customs.Business.CusEntryHeaderChargesLookups GetNewLookups() => new CusEntryHeaderChargesLookups(this);

		protected override Customs.Business.CusEntryHeaderChargesValidation GetNewValidation() => new CusEntryHeaderChargesValidation(this);

		protected override ZString AmountInDeclarationCurrencyCore() => C1_ChargeAmount.Round(0).ToString();

		protected override IFeeRounder GetNewChargeAmountRounder() => new ChargeAmountRounderFiftyCentsDownMinimumOne();

		public IUserEnteredStashSource UserEnteredStashSource => new CusEntryHeaderChargeUserEnteredStashSource(this);
	}
}
