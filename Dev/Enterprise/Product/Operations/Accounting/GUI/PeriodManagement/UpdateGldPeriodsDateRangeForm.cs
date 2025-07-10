using System;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class UpdateGldPeriodsDateRangeForm : GldDateRangeForm<UpdateGldPeriodsDateRangeSetting>
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public UpdateGldPeriodsDateRangeForm() : base()
		{
		}

		public UpdateGldPeriodsDateRangeForm(UpdateGldPeriodsDateRangeSetting updateGldPeriodDateRangeSetting) : base(updateGldPeriodDateRangeSetting)
		{
		}

		protected override void OverrideInit()
		{
			CaptionResourceString = Res.GetData("UpdateGLDPeriodDateRangeForm|5CCA7A13-770B-49E5-8A13-D384E13750D1", "Update General Ledger Data Records");
		}

		protected override void EventForOk(UpdateGldPeriodsDateRangeSetting updateGldPeriodDateRangeSetting)
		{
			GeneralLedgerDataScriptHelper.ExecuteUpdateGLDPeriodScript(updateGldPeriodDateRangeSetting.StartDate, updateGldPeriodDateRangeSetting.EndDate);
			Globals.Message.ShowInformation(Res.GetString("1900AF90-3C29-4ADB-B15D-09258386782A", "All journal entries within the specified date range have been successfully updated."));
		}
	}
}
