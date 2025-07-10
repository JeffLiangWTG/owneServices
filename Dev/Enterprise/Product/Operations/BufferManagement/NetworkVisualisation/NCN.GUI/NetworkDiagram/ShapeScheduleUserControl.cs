using CargoWise.Windows.UI;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public partial class ShapeScheduleUserControl : ZUserControl
	{
		public ShapeScheduleUserControl()
		{
			InitializeComponent();

			// The designer doesn't allow this to be set inside InitializeComponent...
			EarliestStartDateEdit.ReadOnly =
				EarliestFinishDateEdit.ReadOnly =
				LatestStartDateEdit.ReadOnly =
				LatestFinishDateEdit.ReadOnly = true;
		}

		ShapeNetworkEntity ShapeEntity => (ShapeNetworkEntity)base.DataSource;

		protected new BMNCNShape DataSource
		{
			get { return ShapeEntity.AsShape(); }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			UpdateTimeAndDurationsVisibility();
		}

		void UpdateTimeAndDurationsVisibility()
		{
			var shape = DataSource;

			if (shape != null)
			{
				var showDates = ShouldShowDates(shape);

				DurationsPanel.Visible = !showDates;
				TimesPanel.Visible = showDates;

				ControlDpiScalingHelper.SetLeft(ref TimesPanel, DurationsPanel.Left, false);

				SplitContainer.Panel1Collapsed = ShapeEntity.ScheduleHintLabel.IsEmpty;
			}
		}

		static bool ShouldShowDates(BMNCNShape shape)
		{
			return shape.EarliestStartTimeUtc.IsValid
				|| shape.LatestFinishTimeUtc.IsValid
				|| shape.ScheduledStartTimeUtc.IsValid
				|| shape.ScheduledFinishTimeUtc.IsValid;
		}
	}
}
