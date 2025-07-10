using System.Windows.Forms;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CusCAeMHItemUserControl : ZUserControl
	{
		public CusCAeMHItemUserControl()
		{
			InitializeComponent();
			undgManager = new UNDGDataItemFormManager(this.eMHItemsGrid);
			undgManager.Initialize(this.DGSubtanceLinkLabel, this.DGDetailsLinkLabel, this.DGSubtanceGuidFindBox);
			overrideDefaultValuesSupporter = new ZGridOverrideDefaultValuesSupporter(this.eMHItemsGrid);
		}

		#region Dangerous Goods Management

		void DGManagementLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			undgManager?.ShowMultipleItemForm();
		}

		readonly UNDGDataItemFormManager undgManager;
		#endregion

		readonly ZGridOverrideDefaultValuesSupporter overrideDefaultValuesSupporter;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				overrideDefaultValuesSupporter.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
