using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class SecondCusBondDetail : CommonCusBondDetail
	{
		public SecondCusBondDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public const string ApplicationCode = "2ND";

		[BusinessObjectTestExclude]
		public override ZString PW_ApplicationCode
		{
			get => base.PW_ApplicationCode;
			set
			{
				if (value != ApplicationCode)
				{
					ErrorReporter.ReportOnce($"{nameof(PW_ApplicationCode)} for {nameof(SecondCusBondDetail)} should not be set to anything beside '{ApplicationCode}'.");
					value = ApplicationCode;
				}
				base.PW_ApplicationCode = value;
			}
		}

		[BusinessObjectTestExclude]
		public override ZString PW_ActivityCode
		{
			get => base.PW_ActivityCode;
			set
			{
				if (value != GuaranteeActivityCodeList.Codes.ReleasesGuarantee)
				{
					ErrorReporter.ReportOnce($"{nameof(PW_ActivityCode)} for {nameof(SecondCusBondDetail)} should not be set to anything beside '{GuaranteeActivityCodeList.Codes.ReleasesGuarantee}'.");
					value = GuaranteeActivityCodeList.Codes.ReleasesGuarantee;
				}
				base.PW_ActivityCode = value;
			}
		}

		[ReadOnlyMember(nameof(PW_BondNumber2_ReadOnly))]
		public override ZString PW_BondNumber2
		{
			get => base.PW_BondNumber2;
			set => SetValueIfCanAcquireGuaranteeManagementMutexLock(PW_BondNumber2Info, value, (oldValue, newValue) => base.PW_BondNumber2 = newValue);
		}

		protected bool PW_BondNumber2_ReadOnly => IsInDatabase && IsContinuousGuaranteeLinked;

		[ReadOnlyMember(nameof(PW_BondEffectiveDate_ReadOnly))]
		public override ZDateTime PW_BondEffectiveDate
		{
			get => base.PW_BondEffectiveDate;
			set => SetValueIfCanAcquireGuaranteeManagementMutexLock(PW_BondEffectiveDateInfo, value, (oldValue, newValue) => base.PW_BondEffectiveDate = newValue);
		}

		protected bool PW_BondEffectiveDate_ReadOnly => IsInDatabase && IsContinuousGuaranteeLinked;

		public override ZDecimal PW_BondAmount
		{
			get => base.PW_BondAmount;
			set => SetValueIfCanAcquireGuaranteeManagementMutexLock(PW_BondAmountInfo, value, (oldValue, newValue) =>
			{
				base.PW_BondAmount = newValue;
				if (oldValue != PW_BondAmount && !IsCopying)
				{
					Instruction?.MarkNeedAddGuaranteeTransactions();
					Guarantee?.RemainingInfo.RefreshBinding();
				}
			});
		}

		public new SecondCusBondDetailValidation Validation => (SecondCusBondDetailValidation)base.Validation;

		protected override ZString HumanReadableNameCore => Res.GetString("73147d5c-691f-4ce4-8718-e662aee173de"
			, "Release Guarantee ({0} '{1}', {2} '{3}')"
			, PW_BondNumber2Info.HumanReadableName
			, PW_BondNumber2
			, PW_BondEffectiveDateInfo.HumanReadableName
			, PW_BondEffectiveDate.ToBestReadableDateTimeString());

		protected override MasterFiles.Business.CusBondDetailValidation GetNewValidation() => new SecondCusBondDetailValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PW_ActivityCode = GuaranteeActivityCodeList.Codes.ReleasesGuarantee;
			PW_ApplicationCode = ApplicationCode;
		}

		public override void Delete()
		{
			if (HasEnteredMandatoryData && IsInDatabase && IsContinuousGuaranteeLinked)
			{
				Instruction?.MarkNeedingReversingTransaction(this);
			}
			base.Delete();
		}

		public bool HasEnteredMandatoryData => !PW_BondNumber2.IsEmpty && !PW_BondEffectiveDate.IsEmpty && PW_BondAmount > ZDecimal.Zero;

		internal SharedCusPermitLineTransaction AddTransaction()
		{
			return AddTransaction(PW_BondAmount, false, null);
		}

		internal SharedCusPermitLineTransaction AddReversingTransaction()
		{
			return AddTransaction((ZDecimal)PW_BondAmountInfo.OriginalValue, true, ThrowBurstException, true);
		}

		internal SharedCusPermitLineTransaction AddDifferenceTransaction()
		{
			var diff = (ZDecimal)PW_BondAmountInfo.Value - (ZDecimal)PW_BondAmountInfo.OriginalValue;
			return AddTransaction(diff, diff < 0, ThrowBurstException);
		}

		protected override CusGuaranteeHeader GetTransactionHeader() => Guarantee?.CusGuarantee;

		bool IsContinuousGuaranteeLinked => Factory.GetValue(ref isContinuousGuaranteeLinkedCached, () => Guarantee is CusBondDetail consumingGuarantee && consumingGuarantee.IsContinuous && consumingGuarantee.IsLinked);
		CachedProperty<bool> isContinuousGuaranteeLinkedCached;

		CusBondDetail Guarantee => Instruction?.GetGuarantee(false);
	}
}
