using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusExitReportValidation : EU.ExitControl.Business.CusExitReportValidation
	{
		public CusExitReportValidation(CusExitReport parent)
			: base(parent)
		{
		}
		ISingleElementListInternal ParentListInternals => Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (ParentListInternals.SuspendListChanged())
			{
				ValidateCER_Calc_FormattedDateTime();
			}
		}

		protected void ValidateCER_Calc_Discrepancies()
		{
			ValidateCalculatedProperty(Parent.CER_Calc_DiscrepanciesInfo);
		}
		protected void CheckCER_Calc_Discrepancies()
		{
			if (Parent.CER_Behavior == ExitReportDiscrepancyTypeList.Codes.Discrepancies && Parent.CusExitReportItems.Count == 0)
			{
				Parent.CER_Calc_DiscrepanciesInfo.AddMessageError(Res.GetString("E8959C80-12F6-4EE4-B39C-0BCE01EA8DEF", "There must be at least one Report Item when discrepancy is ticked."));
			}
		}

		public void ValidateCER_Calc_FormattedDateTime()
		{
			ValidateCalculatedProperty(Parent.CER_Calc_FormattedDateTimeInfo);
		}

		protected void CheckCER_Calc_FormattedDateTime()
		{
			ValidateCER_DateTime();
			Parent.CER_Calc_FormattedDateTimeInfo.AddAllNotificationsFrom(Parent.CER_DateTimeInfo);
		}

		protected override void CheckCER_DateTime()
		{
			base.CheckCER_DateTime();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CER_DateTimeInfo);
		}

		protected override void CheckCER_Behavior()
		{
			base.CheckCER_Behavior();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CER_BehaviorInfo);
			ValidateCER_Calc_Discrepancies();
		}

		protected new CusExitReport Parent => (CusExitReport)base.Parent;
	}
}
