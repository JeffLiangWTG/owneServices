using System;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCargoOutturnBillsControl : CusUnderbondDetailsUserControl
	{
		public AirCargoOutturnBillsControl()
		{
			InitializeComponent();

			CoveringLabel.AllowOverlap(underbondArrivalDateEdit);
			CoveringLabel.AllowOverlap(underbondArrivalDateLabel);
			CoveringLabel.AllowOverlap(underbondFlightNoTextBox);
			CoveringLabel.AllowOverlap(flightLabel);
			CoveringLabel.AllowOverlap(mAWBLabel);
			CoveringLabel.AllowOverlap(mAWBNumberTextBox);
			underbondArrivalDateEdit.AllowOverlap(OutturnUserControl);
			underbondArrivalDateLabel.AllowOverlap(OutturnUserControl);
			underbondFlightNoTextBox.AllowOverlap(OutturnUserControl);
			flightLabel.AllowOverlap(OutturnUserControl);
			mAWBLabel.AllowOverlap(OutturnUserControl);
			mAWBNumberTextBox.AllowOverlap(OutturnUserControl);
		}

		#region Splitter

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			outturnMessagesSplitter.MinSize = outturnMessagesControl.Height;
			outturnMessagesSplitter.MinExtra = 70;
			outturnMessagesSplitter.SplitPosition = outturnMessagesSplitter.SplitPosition;
			MainTabControl.TabPages.Remove(MessagesTabPage);
		}

		private void OutturnTabPage_Resize(object sender, EventArgs e)
		{
			if (!outturnMessagesSplitter.Disposing && !outturnMessagesSplitter.IsDisposed)
			{
				outturnMessagesSplitter.SplitPosition = outturnMessagesSplitter.SplitPosition;
			}
		}

		#endregion

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			outturnMessagesUserControl.SetBindPrepend("Outturn");
			cargoMessagesControl.SetBindPrepend("Cargo");
			outturnMessagesControl.SetBindPrepend("Outturns.");
			OutturnUserControl.RemoveColumnFromGrid(CusOutturnSchema.C5_OuterPacks.Name);
			base.SetDataBinding(dataSource, dataMember);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (BindingSource.DataSource != null)
			{
				if (MainTabControl.TabPages.Contains(OutturnTabPage))
				{
					MainTabControl.SelectedTab = OutturnTabPage;
				}
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				MainTabControl.TabPages.Add(MessagesTabPage);
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
