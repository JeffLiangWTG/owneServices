using System;
using System.Reflection;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ComponentToolboxVisibilityTestCase : ComponentToolboxVisibilityBaseTestCase
	{
		protected override Type[] GetExpectedToolboxVisibleComponentTypes()
		{
			return new Type[]
			{
				typeof(NotificationRenderer),
				typeof(KNumericUpDown),
				typeof(KTreeView),
				typeof(KTextBox),
				typeof(KUserControl),
				typeof(KListBox),
				typeof(Layout.ControlVisibilityRelationshipProvider),
				typeof(KLabel),
				typeof(KLinkLabel),
				typeof(KButton),
				typeof(VerticalLabel),
				typeof(Layout.RowLayoutPanel),
				typeof(KCheckBox),
				typeof(KTabControl),
				typeof(KGroupBox),
				typeof(KNotificationProvider),
				typeof(KBindingSource),
				typeof(KPanel),
				typeof(KRadioButton),
				typeof(LabelCaptionRenderProvider),
				typeof(KChart),
				typeof(KCheckedListBox),
				typeof(KContextMenuStrip),
				typeof(KDataGrid),
				typeof(KDataGridView),
				typeof(KDateTimePicker),
				typeof(KElementHost),
				typeof(KFlowLayoutPanel),
				typeof(KListView),
				typeof(KMenuStrip),
				typeof(KMonthCalendar),
				typeof(KPictureBox),
				typeof(KProgressBar),
				typeof(KRichTextBox),
				typeof(KSplitContainer),
				typeof(KSplitter),
				typeof(KStatusBar),
				typeof(KTableLayoutPanel),
				typeof(KToolBar),
				typeof(KToolStrip),
				typeof(KToolTip),
				typeof(KTrackBar)
			};
		}

		protected override Assembly TargetAssembly
		{
			get
			{
				return Assembly.Load("CargoWise.Windows.UI");
			}
		}
	}
}
