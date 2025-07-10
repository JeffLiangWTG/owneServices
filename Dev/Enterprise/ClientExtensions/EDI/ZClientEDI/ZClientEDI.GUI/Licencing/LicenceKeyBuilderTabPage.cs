using System.ComponentModel;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module
{
	public class LicenceKeyBuilderTabPage : ZTabPage
	{
		public LicenceKeyBuilderTabPage(EDIOrgHeader orgHeader)
		{
			Text = Res.GetString("LicenceKeyBuilderTabPage|Text", "License");
			Organisation = orgHeader;
			Name = "LicenceKeyBuilderTabPage";
		}

		public LicenceKeyBuilderControl LicenceControl;
		public ILicenceViewController LicViewController { get; set; }

		public LicenceHeader SelectedLicenceHeader
		{
			get { return selectedLicenceHeader; }
			set { selectedLicenceHeader = value; }
		}
		LicenceHeader selectedLicenceHeader;

		public LicenceDatabase SelectedLicenceDatabase => selectedLicenceHeader?.Database;

		#region Organisation

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public EDIOrgHeader Organisation
		{
			get { return fOrganisation; }
			set { fOrganisation = value; }
		}

		EDIOrgHeader fOrganisation;

		#endregion

		public void AddLicenceControl()
		{
			LicenceControl = new LicenceKeyBuilderControl(LicViewController);
			Controls.Add(LicenceControl);
			LicenceControl.Dock = DockStyle.Fill;
			LicenceControl.DockPadding.All = 5;
			LicenceControl.SelectedLicenceHeader = selectedLicenceHeader;
		}

		protected override void SetVisibleCore(bool value)
		{
			base.SetVisibleCore(value);

			if (value)
			{
				if (!Organisation.IsInDatabase)
				{
					ShowCoveringLabel("You cannot modify License information until the Organization has been saved.", value);
				}
				else if (Organisation.LicCompany == null)
				{
					ShowCoveringLabel("This Organisation Record does not have a license key. Please select Actions > Create Licence if you wish to create one.", value);
				}
				else
				{
					HideCoveringLabel();
				}
			}
		}

		public void ShowCoveringLabel(string text, bool isVisible)
		{
			CoveringLabel.Text = text;
			CoveringLabel.Visible = isVisible;
			if (LicenceControl != null)
			{
				LicenceControl.Visible = !isVisible;
			}
		}

		public void HideCoveringLabel()
		{
			CoveringLabel.Visible = false;
			if (LicenceControl == null)
			{
				AddLicenceControl();
			}
			LicenceControl.Visible = true;
		}

		ZLabel CoveringLabel
		{
			get
			{
				if (coveringLabel == null)
				{
					coveringLabel = new ZLabel();
					coveringLabel.Dock = DockStyle.Fill;
					coveringLabel.Visible = false;
					coveringLabel.IsFontBold = true;
					coveringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
					this.Controls.Add(coveringLabel);
#if DEBUG
					TypeDescriptor.AddAttributes(coveringLabel, new SuppressFormsLocalizedTestAttribute());
#endif
				}
				return coveringLabel;
			}
		}
		ZLabel coveringLabel;
	}
}
