using System;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public partial class G5V1TemporaryStorageUserControl : ZUserControl
	{
		public G5V1TemporaryStorageUserControl()
		{
			InitializeComponent();
			SetDetailsLayoutLayout();
			SetGuaranteeLayout();
			InitPreviousDocumentsUserControl();
			InitSupportingDocumentsUserControl();
			InitAdditionalInfosUserControl();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			SetDetailsLayoutLayout();
		}

		void SetDetailsLayoutLayout()
		{
			var temporaryStorageDetailsLayout = new G5V1TemporaryStorageDetailsLayout();
			DynamicDetailsUserControlLayoutPanel.UpdateLayout(temporaryStorageDetailsLayout);
		}

		void SetGuaranteeLayout()
		{
			DynamicGuaranteePanel.UpdateLayout(new EU.GUI.GuaranteeGroupBoxWithOverrideLayout());
		}

		void InitPreviousDocumentsUserControl()
		{
			var previousDocumentsUserControl = new EU.TemporaryStorage.GUI.UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid();
			previousDocumentsUserControl.Name = "UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid";
			SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(previousDocumentsUserControl, (NoResString)"Bills", SupportingInfoColumnLayoutContext);
			previousDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			PreviousDocumentsLayoutPanel.Controls.Add(previousDocumentsUserControl);
		}

		void InitSupportingDocumentsUserControl()
		{
			var supportingDocumentsUserControl = new EU.TemporaryStorage.GUI.UCC6TemporaryStorageSupportingDocumentsUserControlWithGrid();
			supportingDocumentsUserControl.Name = "UCC6TemporaryStorageSupportingDocumentsUserControlWithGrid";
			SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(supportingDocumentsUserControl, (NoResString)"Bills", SupportingInfoColumnLayoutContext);
			supportingDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			SupportingDocumentsLayoutPanel.Controls.Add(supportingDocumentsUserControl);
		}

		void InitAdditionalInfosUserControl()
		{
			var additionalInfoUserControl = new EU.TemporaryStorage.GUI.UCC6TemporaryStorageAdditionalInfosUserControlWithGrid();
			additionalInfoUserControl.Name = "UCC6TemporaryStorageAdditionalInfosUserControlWithGrid";
			SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(additionalInfoUserControl, (NoResString)"Bills", SupportingInfoColumnLayoutContext);
			additionalInfoUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			AdditionalInfosLayoutPanel.Controls.Add(additionalInfoUserControl);
		}

		const string SupportingInfoColumnLayoutContext = "STO";
	}
}
