using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using ResString = Enterprise.Customs.ASYCUDA.Gui.ResString;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public sealed partial class AsycudaBillForm : ZForm
	{
		public AsycudaBillForm(AsycudaBill bill) : base(bill)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
			var header = bill?.Header;

			if (header != null)
			{
				header.EnableBillsLock(true);
				AddBillAdditionalTabPage(header);
				AddBillMessagesTabPage(header);
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
				dutiesTabPage.TabVisible = provider.ShowTaxesUserControl(header);
				billPartiesTabPage.TabVisible = provider.GetBillPartiesTabPageVisibility(header);
				if (this.dutiesTabPage.TabVisible)
				{
					this.asycudaDutiesUserControl.SetControlVisibility(header);
				}

				AddBillPlugins(header);
				AddActionsExtraMenuItems();
			}
		}
		public new AsycudaBill BusinessEntity => (AsycudaBill)base.BusinessEntity;

		protected override bool AllowNew => false;

		void AddBillAdditionalTabPage(AsycudaManifestHeader asycudaManifestHeader)
		{
			if (asycudaManifestHeader != null && additionalUserControlsHelper == null)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(asycudaManifestHeader);
				var billAdditionalTabPageUserControls = provider.GetBillAdditionalTabPageUserControl();
				additionalUserControlsHelper = new UserControlGenerateHelper(asycudaManifestHeader, billAdditionalTabPageUserControls);
				additionalUserControlsHelper.AddAdditionalTabPages(billsAndPacksTabControl, BindingSource, "", () => new BillAdditionalTabPageUserControl(), 2);
			}
		}
		UserControlGenerateHelper additionalUserControlsHelper;

		#region AsycudaManifestBillMessagesTab

		void AddBillMessagesTabPage(AsycudaManifestHeader asycudaManifestHeader)
		{
			if (asycudaManifestHeader != null && additionalMessagesTabControlsHelper == null)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(asycudaManifestHeader);
				if (provider.ShouldPositionMessagesTabAccordingToMessageLevel && asycudaManifestHeader.ManifestType.IsBillMessageLevel())
				{
					var messageTab = new List<IAdditionalTabPage>() { provider.GetBillMessagesUserControl() };
					additionalMessagesTabControlsHelper = new UserControlGenerateHelper(asycudaManifestHeader, messageTab);
					additionalMessagesTabControlsHelper.AddAdditionalTabPages(billsAndPacksTabControl, BindingSource, "", () => new BillAdditionalTabPageUserControl(), 1);
				}
			}
		}
		UserControlGenerateHelper additionalMessagesTabControlsHelper;

		#endregion

		void AddBillPlugins(AsycudaManifestHeader asycudaManifestHeader)
		{
			if (asycudaManifestHeader != null)
			{
				ApplicationGUIProvider.GetApplicationGuiProvider(asycudaManifestHeader).AddBillPlugins(this);
			}
		}

		public override string FormCaption
		{
			get
			{
				string caption = null;
				if (!this.IsDesignMode() && BusinessEntity != null && BusinessEntity.IsInDatabase)
				{
					caption = ApplicationGUIProvider.GetApplicationGuiProvider(BusinessEntity.Header).GetBillFormCaption(BusinessEntity);
				}
				return caption.IsNullOrEmpty() ? base.FormCaption : caption;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
				additionalUserControlsHelper?.DisposeTabPages();
				additionalMessagesTabControlsHelper?.DisposeTabPages();
			}
			base.Dispose(disposing);
		}

		void AddActionsExtraMenuItems()
		{
			ActionsMenuItem.MenuItems.Add(new ZMenuItem("-"));
			ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("47402E5B-9545-4182-8EB5-66DBE96B5C80", "Open Parent Job"), OpenParentJob_Click));
		}

		void OpenParentJob_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.HasChanges)
			{
				var shouldSave = Globals.Message.Show(ResString.GetMultilingualString("0dbbc8e7-225c-4f2b-878c-2aa5bfbdc5d9", "This bill has not yet been saved. Do you want to save and proceed?"),
					ResString.GetMultilingualString("de2e5a3b-96bc-4d4c-9e2e-fe90831ad411", "Save Bill"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes;

				if (!shouldSave || FireSaveButton() != ContinueWithSave.Yes)
				{
					return;
				}
			}

			var manifestController = ZControllerFactory.Create(GetParentControllerID());
			manifestController.ShowFormOfGivenDisplayType(BusinessEntity.Header, DisplayMode);

			Close();
		}

		ControllerID GetParentControllerID()
		{
			var header = BusinessEntity.Header;
			if (header is IControllerIDProvider provider && provider.ControllerID != null)
			{
				return provider.ControllerID;
			}

			return (header?.Consol == null || ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.Value)
				? ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest
				: ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestConsol;
		}
	}
}
