using System;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ImportBottomSectionUserControl : ZUserControl
	{
		public ImportBottomSectionUserControl()
		{
			InitializeComponent();
			InitPreviousDocumentsUserControl();
		}

		public new ImportEntryMessageSendingAction CurrentDataItem => (ImportEntryMessageSendingAction)base.CurrentDataItem;

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			var currentDataItem = CurrentDataItem;
			if (currentDataItem != null)
			{
				currentDataItem.CusConInfo.ValueChanged -= CusConInfo_ValueChanged;
			}
			base.OnCurrentDataItemChanging(e);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var currentDataItem = CurrentDataItem;
			if (currentDataItem != null)
			{
				currentDataItem.CusConInfo.ValueChanged += CusConInfo_ValueChanged;
			}
			CusConInfo_ValueChanged(null, null);
		}

		void CusConInfo_ValueChanged(object sender, EventArgs e)
		{
			BottomGroupBox.Visible = CurrentDataItem?.CusCon ?? ZBool.False;
		}

		void InitPreviousDocumentsUserControl()
		{
			PreviousDocumentsUserControl.UserControlType = typeof(SendingImportPreviousDocumentsUserControl);
			PreviousDocumentsUserControl.HostedControlCreated += (sender, args) =>
			{
				if (PreviousDocumentsUserControl.HostedControl is SendingImportPreviousDocumentsUserControl control)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(control, "EntryInstruction", "INS");// binding member
				}
			};
		}
	}
}
