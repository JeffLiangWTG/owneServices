using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitConsignmentItemValidation : ExitControlBase.Business.CusExitConsignmentItemValidation
	{
		public CusExitConsignmentItemValidation(CusExitConsignmentItem parent)
			: base(parent)
		{
		}

		protected new CusExitConsignmentItem Parent => (CusExitConsignmentItem)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateCCI_Calc_ReportNetMass();
				ValidateCCI_Calc_ReportGrossMass();
			}
		}

		#region Validate CCI_LineNumber

		protected override void CheckCCI_LineNumber()
		{
			base.CheckCCI_LineNumber();
			var parent = Parent;
			var lineNumber = parent.CCI_LineNumber;
			if (lineNumber > 0)
			{
				if (parent.Consignment is CusExitConsignment consignment && consignment.HasMoreThanOneConsignmentItemMatching(lineNumber))
				{
					var info = parent.CCI_LineNumberInfo;
					info.AddMessageError(Res.GetString("{40945EDF-63A5-4799-92F1-3944A482807C}", "{0} ({1}) should not be duplicated", info.Description, lineNumber));
				}
			}
			else
			{
				MandatoryValidation.CheckNotNegative(parent.CCI_LineNumberInfo);
				MandatoryValidation.CheckNotZero(parent.CCI_LineNumberInfo);
			}
		}

		#endregion

		#region Validate CCI_Calc_ReportNetMass
		public void ValidateCCI_Calc_ReportNetMass()
		{
			ValidateCalculatedProperty(Parent.CCI_Calc_ReportNetMassInfo);
		}

		protected void CheckCCI_Calc_ReportNetMass()
		{
			var parent = Parent;
			if (parent.IsCreatonOrUpdatingReportItemData && parent.CCI_Calc_ShouldReportItem)
			{
				MandatoryValidation.CheckNotNegative(parent.CCI_Calc_ReportNetMassInfo);
				MandatoryValidation.MessageErrorIfIsZero(parent.CCI_Calc_ReportNetMassInfo);
			}
		}
		#endregion

		#region Validate CCI_Calc_ReportGrossMass
		public void ValidateCCI_Calc_ReportGrossMass()
		{
			ValidateCalculatedProperty(Parent.CCI_Calc_ReportGrossMassInfo);
		}

		protected void CheckCCI_Calc_ReportGrossMass()
		{
			var parent = Parent;
			if (parent.IsCreatonOrUpdatingReportItemData && parent.CCI_Calc_ShouldReportItem)
			{
				MandatoryValidation.CheckNotNegative(parent.CCI_Calc_ReportGrossMassInfo);
				MandatoryValidation.MessageErrorIfIsZero(parent.CCI_Calc_ReportGrossMassInfo);
			}
		}
		#endregion

		protected override void CheckCCI_DiscrepancyStatus()
		{
			base.CheckCCI_DiscrepancyStatus();
			if (IsUcc6RuleActive(Parent, x => x.ValidateCCI_DiscrepancyStatusLookup))
			{
				ListValidation.ErrorIfInvalidCode(Parent.CCI_DiscrepancyStatusInfo, Parent.Lookups.StatusList);
			}
		}

		protected override void CheckCCI_UniqueConsignmentReferenceStatus()
		{
			base.CheckCCI_UniqueConsignmentReferenceStatus();
			if (IsUcc6RuleActive(Parent, x => x.ValidateCCI_UniqueConsignmentReferenceStatusLookup))
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CCI_UniqueConsignmentReferenceStatusInfo);
			}
		}

		bool IsUcc6RuleActive(CusExitConsignmentItem cusExitConsignmentItem, Func<ICusExitConsignmentItemUcc6ValidationDecider, bool> ruleCheck)
		{
			if (cusExitConsignmentItem.ValidationDecider is ICusExitConsignmentItemUcc6ValidationDecider phase5ValidationDecider)
			{
				return ruleCheck(phase5ValidationDecider);
			}

			return false;
		}
	}
}
