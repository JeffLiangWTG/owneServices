using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class ExitSummaryUserControl : ZUserControl
	{
		public ExitSummaryUserControl()
		{
			InitializeComponent();
			InitializeLayoutMovementsGrid();
			InitializeLayoutItemsGrid();
			InitializeMessageControls();
			AddDynamicLayoutUserControl();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			InitTabsVisibility();
		}

		protected virtual ZBool DynamicLayoutApplied => ZBool.False;

		IPanelLayoutProvider ExitSummaryMainPanelLayout => exitSummaryMainPanelLayout ?? (exitSummaryMainPanelLayout = GetNewExitSummaryMainPanelLayout());

		IPanelLayoutProvider exitSummaryMainPanelLayout;

		protected virtual IPanelLayoutProvider GetNewExitSummaryMainPanelLayout() => new ExitSummaryMainPanelLayout();

		void AddDynamicLayoutUserControl()
		{
			if (DynamicLayoutApplied)
			{
				ExitSummaryMainPanelUserControl.SetExitSummaryMainPanelLayout(ExitSummaryMainPanelLayout);
			}
		}

		void InitTabsVisibility()
		{
			var dynamicLayoutApplied = DynamicLayoutApplied;
			NewTopPanel.Visible = dynamicLayoutApplied;
			TopPanel.Visible = !dynamicLayoutApplied;
		}

		protected virtual void InitializeLayoutMovementsGrid()
		{
		}

		protected virtual void InitializeLayoutItemsGrid()
		{
		}

		protected virtual Type GetMessagesTabUserControlType() => typeof(MessagesTabUserControl);

		void InitializeMessageControls()
		{
			MessagesUserControl.UserControlType = GetMessagesTabUserControlType();
			BindingSource.SetBindingMember(MessagesUserControl, "CusExitDetails.Messages");
		}
	}
}
