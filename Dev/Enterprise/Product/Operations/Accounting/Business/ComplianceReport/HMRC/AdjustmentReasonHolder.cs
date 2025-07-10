using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	public class AdjustmentReasonHolder : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AdjustmentReasonHolder(Func<ZDecimal> adjustmentAmountGetter, AdjustmentReasonHolder source = null) : base()
		{
			AdjustmentAmountGetter = adjustmentAmountGetter;
			if (source != null)
			{
				this.CopyValuesFrom(source);
			}
		}

		bool HasAdjustmentAmount => !AdjustmentAmount.IsEmpty;
		ZDecimal AdjustmentAmount => AdjustmentAmountGetter?.Invoke() ?? ZDecimal.Zero;
		Func<ZDecimal> AdjustmentAmountGetter { get; }

		[MaxLength(3)]
		[List("TaxReturnAdjustmentReasons_List")]
		public ZString Code
		{
			get { return code; }
			set
			{
				var hasChanges = value != code;
				SetNonPersistentPropertyValue(CodeInfo, ref code, value.Left(CodeInfo.MaxLength));
				if (hasChanges)
				{
					ValidateCode();
					Reason = new ZString(TaxReturnAdjustmentReasons_List.GetDescriptionFromCode(Code));
				}
			}
		}
		ZString code;

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(nameof(Code)); }
		}

		[MaxLength(AutoAccTaxReturnColumn.Schema.ATC_CommentMaxLength)]
		public ZString Reason
		{
			get { return reason; }
			set
			{
				SetNonPersistentPropertyValue(ReasonInfo, ref reason, value.Left(ReasonInfo.MaxLength));
				ValidateReason();
			}
		}
		ZString reason;

		public ZPropertyInfo ReasonInfo
		{
			get { return GetZPropertyInfo(nameof(Reason)); }
		}

		public ReadOnlyCodeDescriptionPairList TaxReturnAdjustmentReasons_List => AccountingConfigurationRegistry.Instance.TaxReturnAdjustmentReasonsList.Value;

		internal void CopyValuesFrom(AdjustmentReasonHolder source)
		{
			Code = source.Code;
			Reason = source.Reason;
		}

		#region Validate

		protected override void RunPreSaveValidationCore()
		{
			ValidateCode();
			ValidateReason();
		}

		void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();
			if (HasAdjustmentAmount)
			{
				MandatoryValidation.CheckEntered(CodeInfo);
				ListValidation.ErrorIfInvalidCode(CodeInfo, TaxReturnAdjustmentReasons_List);
			}
			else
			{
				MandatoryValidation.CheckNotEntered(CodeInfo);
			}
		}

		void ValidateReason()
		{
			ReasonInfo.ClearAllNotifications();
			if (HasAdjustmentAmount)
			{
				MandatoryValidation.CheckEntered(ReasonInfo);
			}
			else
			{
				MandatoryValidation.CheckNotEntered(ReasonInfo);
			}
		}

		#endregion
	}
}
