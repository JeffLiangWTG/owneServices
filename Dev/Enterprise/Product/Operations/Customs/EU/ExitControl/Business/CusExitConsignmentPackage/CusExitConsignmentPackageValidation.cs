using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ExitControlBase.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitConsignmentPackageValidation : ExitControlBase.Business.CusExitConsignmentPackageValidation
	{
		public CusExitConsignmentPackageValidation(AutoCusExitConsignmentPackage parent)
			: base(parent)
		{
		}

		protected new CusExitConsignmentPackage Parent => (CusExitConsignmentPackage)base.Parent;
		protected CusExitHeader Header => Parent.Header;

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateCXP_Calc_ReportQuantity();
			}
		}

		protected override void CheckCXP_Quantity()
		{
			base.CheckCXP_Quantity();

			CompareValidation.CheckWithinRange(Parent.CXP_QuantityInfo, 0, 99999999);
		}

		protected override void CheckCXP_PackageType()
		{
			base.CheckCXP_PackageType();

			ListValidation.MessageErrorIfInvalidCode(Parent.CXP_PackageTypeInfo);
		}

		protected override void CheckCXP_Sequence()
		{
			base.CheckCXP_Sequence();
			var parent = Parent;
			var sequence = parent.CXP_Sequence;
			var sequenceInfo = parent.CXP_SequenceInfo;

			if (!sequenceInfo.ReadOnly)
			{
				if (sequence > 0)
				{
					if ((!parent.IsInDatabase || sequenceInfo.HasChanges)
						&& parent.ConsignmentPivot is CusExitConsignmentPivot consignmentPivot
						&& consignmentPivot.ConsignmentItem is CusExitConsignmentItem consignmentItem
						&& consignmentItem.PackagesSequenceDictionary.TryGetValue(sequence, out var count)
						&& count > 1)
					{
						sequenceInfo.AddError(Res.GetString("{CEB72C2B-5328-4123-8B95-2C745E330337}", "{0} ({1}) should not be duplicated", sequenceInfo.Description, sequence));
					}
				}
				else
				{
					MandatoryValidation.CheckNotNegative(sequenceInfo);
					MandatoryValidation.CheckNotZero(sequenceInfo);
				}
			}
		}

		#region Validate CXP_Calc_ReportQuantity

		public void ValidateCXP_Calc_ReportQuantity()
		{
			ValidateCalculatedProperty(Parent.CXP_Calc_ReportQuantityInfo);
		}

		protected void CheckCXP_Calc_ReportQuantity()
		{
			var parent = Parent;
			if (parent.IsCreatonOrUpdatingReportItemData && parent.CXP_Calc_ShouldReportItem)
			{
				MandatoryValidation.CheckNotNegative(parent.CXP_Calc_ReportQuantityInfo);
				MandatoryValidation.MessageErrorIfIsZero(parent.CXP_Calc_ReportQuantityInfo);
			}
		}

		#endregion

		protected override void CheckCXP_MarksAndNumbersStatus()
		{
			base.CheckCXP_MarksAndNumbersStatus();
			if (IsUcc6RuleActive(Parent, x => x.ValidateCXP_MarksAndNumbersStatusLookup))
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CXP_MarksAndNumbersStatusInfo);
			}
		}

		bool IsUcc6RuleActive(CusExitConsignmentPackage package, Func<ICusExitConsignmentPackageUcc6ValidationDecider, bool> ruleCheck)
		{
			if (package.ValidationDecider is ICusExitConsignmentPackageUcc6ValidationDecider phase5ValidationDecider)
			{
				return ruleCheck(phase5ValidationDecider);
			}

			return false;
		}
	}
}
