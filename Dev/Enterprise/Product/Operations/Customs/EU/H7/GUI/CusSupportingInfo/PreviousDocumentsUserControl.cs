using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public partial class PreviousDocumentsUserControl : ZUserControl, IAdditionalTabPage
	{
		public PreviousDocumentsUserControl()
		{
			InitializeComponent();
			CustomizeLayout();
		}

		void CustomizeLayout()
		{
			PreviousDocumentsGrid.SuspendLayout();
			SuspendLayout();

			CustomizeLayoutCore();

			PreviousDocumentsGrid.ResumeLayout(false);
			PreviousDocumentsGrid.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		protected virtual void CustomizeLayoutCore()
		{
			var zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			zCodeFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			PreviousDocumentsGrid.ColumnStyles.Insert(0, zCodeFindBoxColumnStyleInfo1);
		}

		#region IAdditionalTabPage Members

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("400e970b-af66-4986-80d4-88487e791eef", "Previous Documents");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		int IAdditionalTabPage.TabPageSequence => 50;

		#endregion
	}
}
