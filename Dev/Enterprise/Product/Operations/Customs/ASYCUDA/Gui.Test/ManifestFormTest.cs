using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(ManifestForm))]
	sealed class ManifestFormTest : ZFormBasherTest
	{
		public void TestDataContext()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();

			using (var form = new ManifestForm(header))
			{
				AssertEquals(DataContext.AsycudaManifestHeader, form.DataContext);
			}
		}

		public void TestDocDataPlugIn()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			using (var form = new ManifestForm(header))
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		public void TestAuditPlugInPresent()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			using (var form = new ManifestForm(header))
			{
				Assert("Audit PlugIn should be available", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		public void TestSelectAndShowBill()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, "ASY");
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				Application.DoEvents();

				var control = (AsycudaManifestUserControl)form.Controls.Find("asycudaManifestUserControl", true).Single();
				var tabControl = (ZTabControl)control.Controls.Find("mainTabControl", true).Single();
				AssertNotEquals("billsAndPacksTabPage is not selected.", "billsAndPacksTabPage", tabControl.SelectedTab.Name);

				var grid = (ZGrid)control.Controls.Find("BillsGrid", true).Single();

				form.SelectAndShowBill(bill1.PK);
				AssertEquals("should have selected bill1", bill1, grid.ListManager.GetCurrent());
				AssertEquals("billsAndPacksTabPage is selected", "billsAndPacksTabPage", tabControl.SelectedTab.Name);

				form.SelectAndShowBill(bill2.PK);
				AssertEquals("should have selected bill2", bill2, grid.ListManager.GetCurrent());
				AssertEquals("billsAndPacksTabPage is selected", "billsAndPacksTabPage", tabControl.SelectedTab.Name);
			}
		}

		public void TestNoExceptionWhenDeleteBill()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
			var bill1 = header.Bills.AddNew();
			bill1.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;

			using (var form = new ManifestForm(header))
			{
				form.Show();
				Application.DoEvents();

				var control = (AsycudaManifestUserControl)form.Controls.Find("asycudaManifestUserControl", true).Single();
				var tabControl = (ZTabControl)control.Controls.Find("mainTabControl", true).Single();
				AssertNotEquals("billsAndPacksTabPage is not selected.", "billsAndPacksTabPage", tabControl.SelectedTab.Name);

				var grid = (ZGrid)control.Controls.Find("BillsGrid", true).Single();

				form.SelectAndShowBill(bill1.PK);
				AssertEquals("should have selected bill1", bill1, grid.ListManager.GetCurrent());
				AssertEquals("billsAndPacksTabPage is selected", "billsAndPacksTabPage", tabControl.SelectedTab.Name);

				var billsAndPacksTabControl = (ZTabControl)tabControl.Controls.Find("billsAndPacksTabControl", true).Single();
				billsAndPacksTabControl.Select();

				var customTabPage = (ZTabPage)tabControl.Controls.Find("BillCustomTabPage", true).Single();
				billsAndPacksTabControl.SelectedTab = customTabPage;
				AssertEquals("BillCustomTabPage is selected", "BillCustomTabPage", billsAndPacksTabControl.SelectedTab.Name);

				AssertNoExceptionThrown(() =>
				{
					grid.DeleteMenuItem.PerformClick();
					Application.DoEvents();
				});
			}
		}

		public void TestFormCaption()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var readablePrefix = header.HumanReadableNamePrefix ?? ZString.Empty;
			using (var form = new ManifestForm(header))
			{
				header.AMA_ApplicationCode = ZString.Empty;
				form.Show();
				AssertEquals($"{readablePrefix} Manifest", form.FormCaption);
				form.FireSaveButton();
				AssertEquals($"{readablePrefix} Manifest", form.FormCaption);
			}

			using (var form = new ManifestForm(header))
			{
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;

				form.Show();
				AssertEquals($"{readablePrefix} Carrier Manifest", form.FormCaption);
				form.FireSaveButton();
				AssertEquals($"{readablePrefix} Carrier Manifest", form.FormCaption);
			}

			using (var form = new ManifestForm(header))
			{
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				form.Show();
				AssertEquals($"{readablePrefix} Forwarder Manifest", form.FormCaption);
				form.FireSaveButton();
				AssertEquals($"{readablePrefix} Forwarder Manifest", form.FormCaption);
			}

			Factory.Save();

			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			using (var form = new ManifestForm(header))
			{
				form.Show();
				AssertEquals($"MAN0000001 - {readablePrefix} - Carrier", form.FormCaption);
				form.FireSaveButton();
				AssertEquals($"MAN0000001 - {readablePrefix} - Carrier", form.FormCaption);
			}
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			using (var form = new ManifestForm(header))
			{
				form.Show();
				AssertEquals($"MAN0000001 - {readablePrefix} - Forwarder", form.FormCaption);
				form.FireSaveButton();
				AssertEquals($"MAN0000001 - {readablePrefix} - Forwarder", form.FormCaption);
			}
		}

		public void TestFormNewable()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Factory.Save();
			using (var form = new ManifestForm(header))
			{
				form.Show();
				IPostingButtonsProvider buttonsProvider = form;
				var commandButton = buttonsProvider.CommandButtonApply;
				AssertNotEquals("Text of Command Button shall not be \"New\"", commandButton.Text, "&New");
			}
		}

		public void TestOpenParentMenu()
		{
			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				Factory.Save();
				using (var form = new ManifestForm(header))
				{
					form.Show();
					var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Open Manifest Parent (Consol)");
					AssertNotNull("menuItem exists", menuItem);
					Assert("menuItem should be greyed out", !menuItem.Enabled);
				}

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				header.AMA_ParentId = consol.PK;
				header.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;
				Factory.Save();
				using (var form = new ManifestForm(header))
				{
					form.Show();
					var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Open Manifest Parent (Consol)");
					AssertNotNull("menuItem exists", menuItem);
					Assert("menuItem should enable", menuItem.Enabled);

					menuItem.PerformClick();

					var consolForm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x.Name.Contains("Consol"));
					AssertNotNull(consolForm);

					consolForm.Close();
				}
			}
		}

		public void TestMessagesTabOnHeaderLevel()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var form = new ManifestForm(header))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var messagesTabPage = mainTabControl.FindSingle<ZTabPage>("mainTabControl_TabPage_MessagesUserControl");
				mainTabControl.SelectedTab = messagesTabPage;

				var messagesUserControl = messagesTabPage.FindSingle<MessagesUserControl>();
				AssertNotNull(messagesUserControl);
			}
		}

		public void TestRoutingPlugin() => CombineAssertions(() =>
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "ABC";
			using (var form = new ManifestForm(header))
			{
				AssertNull("Should not have routing plugin when application code is not VOC or NVC", form.PlugIns.GetPlugIn(ControllerIDs.Routing));
			}

			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			using (var form = new ManifestForm(header))
			{
				AssertNotNull("Should have routing plugin when application code is not VOC", form.PlugIns.GetPlugIn(ControllerIDs.Routing));
			}

			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			using (var form = new ManifestForm(header))
			{
				AssertNotNull("Should have routing plugin when application code is not NVC", form.PlugIns.GetPlugIn(ControllerIDs.Routing));
			}
		});

		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BolType = AsycudaBill.ChildBolCode;
			Factory.Save();
			header.HasChanges = false;
			var result = new ManifestForm(header);
			result.ControllerID = ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest;
			return result;
		}

		public override void TestMinimumSizeNotTooBig()
		{
			int minScreenWidthSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1300);
			int minScreenHeightSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(750);

			using (var form = GetFormToBashCore())
			{
				Assert("Manifest Form min size too wide (" + form.MinimumSize.Width + ") for the screen. Should be less than or equal to " + minScreenWidthSupported, form.MinimumSize.Width <= minScreenWidthSupported);
				Assert("Manifest Form min size too high (" + form.MinimumSize.Height + ") for the screen. Should be less than or equal to " + minScreenHeightSupported, form.MinimumSize.Height <= minScreenHeightSupported);
			}
		}

		public void TestCanDisplayMessageBuilderMappingPath()
		{
			using var form = new ManifestForm(Factory.NewWithValidTestData<AsycudaManifestHeader>());
			var configurator = (IDevToolMessageBuilderMappingPathConfigurator)form;
			AssertNotNull("Manifest Form as Mapping Path Configurator", configurator);
			AssertEquals("CanDisplayMessageBuilderMappingPath", expected: true, configurator.CanDisplayMessageBuilderMappingPath);
		}
	}
}
