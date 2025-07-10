using System;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public abstract partial class GldDateRangeForm<T> : ZChildForm
		where T : GldDateRangeSetting
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public GldDateRangeForm()
		{
			InitializeComponent();
			OverrideInit();
		}

		public GldDateRangeForm(T updateGldPeriodDateRangeSetting) : base(updateGldPeriodDateRangeSetting)
		{
			OverrideInit();
			UpdateGldPeriodsDateRangeSetting = updateGldPeriodDateRangeSetting;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected virtual void OverrideInit()
		{
		}

		public override string FormVerb
		{
			get
			{
				return "";
			}
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			if (UpdateGldPeriodsDateRangeSetting.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("CDADBC25-E891-4FBD-8B08-1AD940F9EDB1", "There are errors that need to be corrected."));
				return;
			}

			EventForOk(UpdateGldPeriodsDateRangeSetting);
		}

		protected abstract void EventForOk(T updateGldPeriodDateRangeSetting);

		T UpdateGldPeriodsDateRangeSetting { get; }
		ZButton CloseButton;
		protected ZButton OKButton;
		ZDateEdit zDateEdit1;
		ZDateEdit zDateEdit2;
	}
}
