using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Environment;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using DataContext = Enterprise.Core.Constants.DataContext;
using Res = Enterprise.Customs.ASYCUDA.Gui.Res;
using ResString = Enterprise.Customs.ASYCUDA.Gui.ResString;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class ManifestForm : ZTemplateForm, IDevToolMessageBuilderMappingPathConfigurator
	{
		public ManifestForm()
		{
			InitializeComponent();
		}

		public ManifestForm(AsycudaManifestHeader header)
			: base(header)
		{
			InitializeComponent();

			if ((header.IsShippingLine || header.IsConsolidator) && header.IsRoutingEnabled)
			{
				PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.Routing, 1);
			}

			WorkflowTabPage.Initialize(header);
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			asycudaMenuItem = new AsycudaMenu(header, provider.GetAsycudaMenuCaption());
			var actionMenuItemIndex = MainMenu.MenuItems.IndexOf(ActionsMenuItem);
			MainMenu.MenuItems.Add(actionMenuItemIndex + 1, asycudaMenuItem);

			AddActionsMenuItems(provider);
			provider.AddEDocsMenuItems(this, header);

			DataContext = DataContext.AsycudaManifestHeader;
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.Audit);
		}

		void AddActionsMenuItems(ApplicationGUIProvider provider)
		{
			if (AsycudaManifestHeader.IsManifestConsolDecouplingEnabled)
			{
				ActionsMenuItem.MenuItems.Add(new ZMenuItem("-"));
				openParentJob = new ZMenuItem(ResString.GetMultilingualString("B43D4194-3D08-4E60-AAE1-B7E891759E52", "Open Manifest Parent (Consol)"), OpenManifestParent);
				if (BusinessEntity.Consol == null)
				{
					openParentJob.Enabled = false;
				}
				ActionsMenuItem.MenuItems.Add(openParentJob);
			}

			#region Prototype

#if DEBUG
			if (Env.CurrentUser.IsDeveloper)
			{
				ActionsMenuItem.MenuItems.Add(new ZMenuItem("-"));
				ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("4872196d-13c6-4c1b-a8f8-0a72b4b5861f", "[DEV] Show Template Form"), (sender, args) => new ExampleTemplateForm(BusinessEntity).Show(this)));
			}
#endif

			if (Env.CurrentUser.IsDeveloper)
			{
				ActionsMenuItem.MenuItems.Add(new ZMenuItem("-"));
				ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("3827dc78-ccbc-435c-bc39-1579b50a7539", "[DEV] SHA-256"), (sender, args) =>
				{
					byte[] req = BitConverter.GetBytes(ZDateTime.Now.ToDateTime().ToBinary());

					byte[] expected;
					using (var sha256 = SHA256.Create())
					{
						expected = sha256.ComputeHash(req);
					}

					byte[] res;
					try
					{
						res = RemoteCryptoApi.Instance.Sha256(req);
					}
					catch (IOException ex)
					{
						Enterprise.ZArchitecture.Environment.Globals.Message.Show(
							ex.GetType().Name + ": " + ex.Message,
							ResString.GetMultilingualString("fad4c9fe-5276-47f0-b7ff-768e846c0aa3", "SHA-256"), MessageBoxButtons.OK, MessageBoxIcon.Error
						); // For Developers Only
						return;
					}

					Enterprise.ZArchitecture.Environment.Globals.Message.Show(
						FormattableString.Invariant($"Request: {BitConverter.ToString(req)}\r\nExpected Response: {BitConverter.ToString(expected)}\r\nResponse: {BitConverter.ToString(res)}"),
						ResString.GetMultilingualString("62011e29-e468-4798-b964-d6b3a8fbba5c", "SHA-256"), MessageBoxButtons.OK, MessageBoxIcon.Information
					); // For Developers Only
				}));

				ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("ff10593f-d849-4250-b3cf-19f9f2c69961", "[DEV] PKCS#11 Libraries"), (sender, args) =>
				{
					string info;
					try
					{
						info = RemoteCryptoApi.Instance.GetPksc11Info();
					}
					catch (IOException ex)
					{
						Enterprise.ZArchitecture.Environment.Globals.Message.Show(
							ex.GetType().Name + ": " + ex.Message,
							ResString.GetMultilingualString("910d20a8-a88a-47b8-b65e-117ef01f824d", "PKCS#11 Libraries"), MessageBoxButtons.OK, MessageBoxIcon.Error
						); // For Developers Only
						return;
					}

					Enterprise.ZArchitecture.Environment.Globals.Message.Show(
						info,
						ResString.GetMultilingualString("a2abd5b1-5040-4b53-9d6a-a9a9c4a228b2", "PKCS#11 Libraries"), MessageBoxButtons.OK, MessageBoxIcon.Information
					); // For Developers Only
				}));
			}

			#endregion

			ActionsMenuItem.MenuItems.AddRange(provider.GetActionsExtraMenuItems().ToArray<MenuItem>());
		}
		ZMenuItem openParentJob;

		void OpenManifestParent(object sender, EventArgs e)
		{
			var canOpenParentForm = true;
			if (BusinessEntity.HasChanges)
			{
				if ((Globals.Message.Show(ResString.GetMultilingualString("07D25F08-708F-44D6-9CB0-2B6B340E7ABA", "This Manifest has not yet been saved. Do you want to save and proceed?"),
					ResString.GetMultilingualString("F9AC823B-C967-4D9C-844B-3F9C3102A670", "Save Manifest"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes))
				{
					canOpenParentForm = FireSaveButton() == CargoWise.EntityFramework.ContinueWithSave.Yes;
				}
				else
				{
					canOpenParentForm = false;
				}
			}

			if (canOpenParentForm)
			{
				ZControllerFactory.Create(ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestConsol).ShowFormOfGivenDisplayType(BusinessEntity.Consol, this.DisplayMode);
				this.Close();
			}
		}

		protected override bool AllowNew => false;

		readonly AsycudaMenu asycudaMenuItem;

		public new AsycudaManifestHeader BusinessEntity => (AsycudaManifestHeader)base.BusinessEntity;

		public override string FormCaption
		{
			get
			{
				var caption = "Manifest";
				if (!this.IsDesignMode())
				{
					var readablePrefix = BusinessEntity.HumanReadableNamePrefix ?? ZString.Empty;
					if (BusinessEntity.IsInDatabase)
					{
						caption = BusinessEntity.AMA_JobReference;
						if (BusinessEntity.IsShippingLine)
						{
							caption += Res.GetString("ManifestForm|FormCaption|Carrier", " - {0} - Carrier", readablePrefix);
						}
						else if (BusinessEntity.IsConsolidator)
						{
							caption += Res.GetString("ManifestForm|FormCaption|Forwarder", " - {0} - Forwarder", readablePrefix);
						}
						return caption;
					}
					else
					{
						caption = BusinessEntity.IsShippingLine
						? Res.GetString("ManifestForm|FormCaption|CarrierManifest", "{0} Carrier Manifest", readablePrefix)
						: BusinessEntity.IsConsolidator
							? Res.GetString("ManifestForm|FormCaption|ForwarderManifest", "{0} Forwarder Manifest", readablePrefix)
							: Res.GetString("ManifestForm|FormCaption", "{0} Manifest", readablePrefix);
					}
				}
				return caption;
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (asycudaMenuItem != null)
			{
				asycudaMenuItem.Dispose();
			}

			base.Dispose(disposing);
		}

		public void SelectAndShowBill(ZGuid bilPK)
		{
			if (BusinessEntity != null)
			{
				var bill = (AsycudaBill)BusinessEntity.Bills.FindByPK(bilPK);
				if (bill != null)
				{
					asycudaManifestUserControl.SelectAndShowBill(bill.PK);
				}
			}
		}

		public new void RunActionWithProcessBox(string loadingText, Action action)
		{
			base.RunActionWithProcessBox(loadingText, action);
		}

		#region SkipRecentItems

		public bool? SkipRecentItems { get; set; }

		protected override void SaveToRecentItems()
		{
			if (!SkipRecentItems.HasValue || !SkipRecentItems.Value)
			{
				base.SaveToRecentItems();
			}
		}

		#endregion

		bool IDevToolMessageBuilderMappingPathConfigurator.CanDisplayMessageBuilderMappingPath => true;
	}
}
