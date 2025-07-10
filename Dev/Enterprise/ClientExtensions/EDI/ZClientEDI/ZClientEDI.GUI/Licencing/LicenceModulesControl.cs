using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	public partial class LicenceModulesControl : ZUserControl
	{
		public LicenceModulesControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			UpdateControls();
		}

		public const string InSyncMessage = "License key matches client's.";
		public const string NotInSyncMessage = "License key not in sync with client's.";

		void UpdateControls()
		{
			var licHeader = CurrentDataItem as LicenceHeader;
			if (licHeader != null && !licHeader.IsDeleted)
			{
				LicenceKeyInSyncLabel.Text = "";
				var db = licHeader.Database;
				productBox.Text = db.Lookups.ProductTypeList.GetDescriptionFromCode(db.LD_Product);
				bool showModules = db.HasCompanyLicence() || licHeader.HasPurchasedModules;

				if (showModules)
				{
					bool licenceInSync = licHeader.LA_LastLicenceCheckInSync;
					LicenceKeyInSyncLabel.Text = licenceInSync ? InSyncMessage : NotInSyncMessage;
					LicenceKeyInSyncLabel.IsFontBold = !licenceInSync;
					LicenceKeyInSyncLabel.ForeColor = licenceInSync ? Color.Black : Color.Red;
				}

				foreach (Control child in Controls)
				{
					if (child != LicenceKeyInSyncLabel &&
						child != productBox &&
						child != productLabel &&
						child != EditionDropEdit)
					{
						child.Visible = showModules;
					}
				}
			}
		}
	}
}

