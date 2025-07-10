using System;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.IL.Business
{
	[SystemDefinedValues]
	public partial class CusEntryLineFee : AutoCusEntryLineFee, IDisposable
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new partial class Schema : AutoCusEntryLineFee.Schema
		{
			public const string ChargeTypeDescription = "ChargeTypeDescription";
			public const int NationalFeeTypeCodeMaxLength = 5;
		}

		public void Dispose()
		{
			userEnteredStashSource = null;
		}

		public ZString ChargeTypeDescription => Lookups.ChargeTypeList.GetDescriptionFromCode(CF_ChargeType);

		public bool IncludeForVatCalculation => IncludeForVatCalculationCore;

		public bool IsNationalIndirectTaxationFee => Regex.IsMatch(CF_ChargeType, @"^\d");

		public bool IsSystemAddedVatFee => CF_ChargeType == Constants.EntryLineFee.VATFeeTypeCode && CF_RateOverrideReasonCode.IsEmpty;

		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.ChargeTypeList))]
		[ReadOnlyMember(nameof(IsActionBlank))]
		public override ZString CF_ChargeType { get => base.CF_ChargeType; set => base.CF_ChargeType = value; }

		public bool IsActionBlank => CF_RateOverrideReasonCode.IsEmpty;

		public void DefaultMethodOfPaymentIfEmpty()
		{
			if (CF_MethodOfPayment.IsEmpty)
			{
				CF_MethodOfPayment = ZString.Empty;
			}
		}

		public IUserEnteredStashSource UserEnteredStashSource => userEnteredStashSource ?? (userEnteredStashSource = GetUserEnteredStashSourceCore());
		IUserEnteredStashSource userEnteredStashSource;

		protected virtual IUserEnteredStashSource GetUserEnteredStashSourceCore() => new CusEntryLineFeeUserEnteredStashSource(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CF_RateOverrideReasonCode = ILRateOverrideReasonList.Codes.Additional;
		}

		protected virtual bool IncludeForVatCalculationCore => true;
	}
}
