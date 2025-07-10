using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(AsycudaBillForm))]
	sealed class AsycudaBillFormTest : ZFormBasherTest
	{
		public void TestBillLock()
		{
			var helper = new ZZDataTestHelper(Factory);
			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_TransportMode = "AIR";
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			header.AMA_ManifestType = "MGI";

			bill.ABL_ShipmentType = Universal.Helper.ShipmentTypeList.Codes.Export22;

			var packedItem = pack.PackedItemForTesting();
			AssertEquals(false, bill.ReadOnly);
			AssertEquals(bill.ABL_RemarksInfo.Name, false, bill.ABL_RemarksInfo.ReadOnly);

			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();
				Application.DoEvents();

				var qtyCalcDropEdit = form.FindSingle<ZCalcDropEdit>(c => c.Name == "ManifestQtyCalcDropEdit");
				var unitDropEdit = qtyCalcDropEdit.Controls.Find("UnitDropEdit", true).First() as ZDropEdit;
				AssertEquals("pre-condition", false, bill.ABL_ManifestQtyInfo.ReadOnly);
				AssertEquals(false, unitDropEdit.ReadOnly);

				packedItem.API_MessageStatus = MessageStatusCodeList.Codes.Sent;
				Application.DoEvents();

				AssertEquals("pre-condition", true, bill.ABL_ManifestQtyInfo.ReadOnly);
				AssertEquals(true, unitDropEdit.ReadOnly);
			}
		}

		public void TestCustomFieldsTab()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			Factory.Save();

			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();
				var customFieldsTab = (ZTabPage)form.Controls.Find("BillCustomTabPage", true).First();
				customFieldsTab.Select();

				var customFieldsControl = (ProcessTemplateCustomFieldsControl)customFieldsTab.Controls.Find("CustomFieldsControl", true).First();
				AssertNotNull(customFieldsControl.Visible);
			}
		}

		public void TestLogsTab()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			Factory.Save();

			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();
				var logsTab = (ZTabPage)form.Controls.Find("logsTabPage", true).FirstOrDefault();
				AssertNotNull(logsTab);
			}
		}

		public void TestAddBillPlugins()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Ireland, "EH7", ApplicationCodeTypeList.Codes.EuH7V1);
			var bill = header.Bills.AddNew();
			Factory.Save();

			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();

				var plugins = form.PlugIns.Instances.Select(x => x.ControllerID).ToList();
				AssertContainsExactElementsInAnyOrder(new[] { ControllerIDs.eDocsPlugIn }, plugins);
			}
		}

		public void TestBillAdditionalTabPageVisibility()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, "HAVIHR");
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			var bill = header.Bills.AddNew();
			Factory.Save();

			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();

				var billAdditionalTabPage = (ZTabPage)form.Controls.Find("billsAndPacksTabControl_TabPage_TRBillAdditionalUserControl", true).FirstOrDefault();
				Assert(billAdditionalTabPage.TabVisible);

				header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
				Assert(!billAdditionalTabPage.TabVisible);
			}

			var header2 = (AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGE"));
			header2.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			var bill2 = header2.Bills.AddNew();
			Factory.Save();

			using (var form = new AsycudaBillForm(bill2))
			{
				form.Show();

				var billAdditionalTabPage = (ZTabPage)form.Controls.Find("billsAndPacksTabControl_TabPage_TRBillAdditionalUserControl", true).FirstOrDefault();
				AssertNull(billAdditionalTabPage);
			}
		}

		public void TestFormCaption()
		{
			var defaultFormCaption = "Manifest Bill";
			using (var form = new AsycudaBillForm(null))
			{
				AssertEquals("If Bill is null: default caption", defaultFormCaption, form.FormCaption);
			}

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "12345";

			var applicationGUIProvider = ApplicationGUIProviderForTesting.GetTestProvider(header);
			applicationGUIProvider.GetBillFormCaptionForTesting = (bill) => $"Bill {bill.ABL_BillNumber}";
			using (applicationGUIProvider)
			using (var form = new AsycudaBillForm(bill))
			{
				AssertEquals("Before Bill is saved: default caption", defaultFormCaption, form.FormCaption);
			}

			Factory.Save();

			using (applicationGUIProvider)
			using (var form = new AsycudaBillForm(bill))
			{
				AssertEquals("After Bill is saved, and GetBillFormCaption is not null or empty", "Bill 12345", form.FormCaption);
			}

			applicationGUIProvider.GetBillFormCaptionForTesting = (bill) => null;
			using (applicationGUIProvider)
			using (var form = new AsycudaBillForm(bill))
			{
				AssertEquals("After Bill is saved, and GetBillFormCaption returns null", defaultFormCaption, form.FormCaption);
			}

			applicationGUIProvider.GetBillFormCaptionForTesting = (bill) => string.Empty;
			using (applicationGUIProvider)
			using (var form = new AsycudaBillForm(bill))
			{
				AssertEquals("After Bill is saved, and GetBillFormCaption returns empty", defaultFormCaption, form.FormCaption);
			}
		}

		public void TestBillMessagesTab_WhenMessageUserControlIsDefault_ShouldDisplayCorrectly()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "COH";

			var applicationGUIProvider = ApplicationGUIProviderForTesting.GetTestProvider(header);
			applicationGUIProvider.ShouldPositionMessagesTabAccordingToMessageLevelForTesting = true;

			AssertMessagesTab<MessagesUserControl>(header.Bills.AddNew());
		}

		public void TestBillMessagesTab_WhenMessageUserControlIsCustomized_ShouldDisplayCorrectly()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "COH";

			var applicationGUIProvider = ApplicationGUIProviderForTesting.GetTestProvider(header);
			applicationGUIProvider.ShouldPositionMessagesTabAccordingToMessageLevelForTesting = true;
			applicationGUIProvider.GetBillMessagesUserControlForTesting = () => new MessagesUserControlForTesting();

			AssertMessagesTab<MessagesUserControlForTesting>(header.Bills.AddNew());
		}

		public void TestBillAndPacksTabControl_TabPageOrder()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "COH";

			var applicationGUIProvider = ApplicationGUIProviderForTesting.GetTestProvider(header);
			applicationGUIProvider.ShouldPositionMessagesTabAccordingToMessageLevelForTesting = true;

			var expectedTabPageNamesInOrder = new List<string>()
			{
				"billsTabPage",
				"billPartiesTabPage",
				"BillCustomTabPage",
				"billsAndPacksTabControl_TabPage_MessagesUserControl",
				"logsTabPage",
			};

			var bill = header.Bills.AddNew();
			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();
				var tabPages = form.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl").TabPages;
				var actualTabPageNames = tabPages.ToList<ZTabPage>().Select(x => x.Name);

				CombineAssertions(() =>
				{
					AssertEquals("Tab pages match expected number", expectedTabPageNamesInOrder.Count, tabPages.Count);
					AssertContainsExactElementsInExactOrder("Tab pages are in order", expectedTabPageNamesInOrder, actualTabPageNames);
				});
			}
		}

		public void TestOpenParentJob_WhenParentDoesNotHaveConsol_ShouldShowManifestForm()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			Factory.Save();

			using (var form = new AsycudaBillForm(bill))
			{
				ShowFormAndClickOpenParentJob(form);

				var headerForm = Application.OpenForms.Cast<ZForm>().FirstOrDefault(x => x is ManifestForm) as ManifestForm;

				CombineAssertions(() =>
				{
					AssertNotNull("Parent Job form should be opened", headerForm);
					AssertEquals("Expected Business Entity PK", headerForm.BusinessEntity.PK, header.PK);
					AssertEquals("Expected Controller ID", ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest, headerForm.ControllerID);
				});

				headerForm.Close();
			}
		}

		public void TestOpenParentJob_WhenParentIsConsol_ShouldShowConsolForm()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ParentId = consol.PK;
			header.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var bill = header.Bills.AddNew();
			Factory.Save();

			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new AsycudaBillForm(bill))
			{
				ShowFormAndClickOpenParentJob(form);

				var consolForm = Application.OpenForms.Cast<ZForm>().FirstOrDefault(x => x.Name.Contains("Consol"));

				CombineAssertions(() =>
				{
					AssertNotNull("Parent Job form should be opened", consolForm);
					AssertType("Expected Business Entity type", typeof(ForwardingConsol), consolForm.BusinessEntity);
					AssertEquals("Expected Business Entity PK", (consolForm.BusinessEntity as ForwardingConsol).PK, consol.PK);
					AssertEquals("Expected Controller ID", ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestConsol, consolForm.ControllerID);
				});

				consolForm.Close();
			}
		}

		public void TestOpenParentJob_WhenManifestConsolDecouplingIsEnabled_ShouldShowManifestForm()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ParentId = consol.PK;
			header.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var bill = header.Bills.AddNew();
			Factory.Save();

			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new AsycudaBillForm(bill))
			{
				ShowFormAndClickOpenParentJob(form);

				var headerForm = Application.OpenForms.Cast<ZForm>().FirstOrDefault(x => x is ManifestForm) as ManifestForm;

				CombineAssertions(() =>
				{
					AssertNotNull("Parent Job form should be opened", headerForm);
					AssertEquals("Expected Business Entity PK", headerForm.BusinessEntity.PK, header.PK);
					AssertEquals("Expected Controller ID", ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest, headerForm.ControllerID);
				});

				headerForm.Close();
			}
		}

		[Licensing.Billing.Business.Testing.FeatureDataTest(LicenceFeatureCodeList.Codes.EcommerceH7Feature)]
		public void TestOpenParentJob_WhenParentIsH7Job_ShouldH7JobForm()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.EUH7.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "EH7";
			var bill = header.Bills.AddNew();

			Factory.Save();

			var applicationGUIProvider = ApplicationGUIProviderForTesting.GetTestProvider(header);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Denmark))
			using (var form = new AsycudaBillForm(bill))
			{
				ShowFormAndClickOpenParentJob(form);

				var headerForm = Application.OpenForms.Cast<ZForm>().FirstOrDefault(x => x is ManifestForm) as ManifestForm;

				CombineAssertions(() =>
				{
					AssertNotNull("Parent Job form should be opened", headerForm);
					AssertEquals("Expected Business Entity PK", headerForm.BusinessEntity.PK, header.PK);
					AssertEquals("Expected Controller ID", ControllerIDs.Customs.EU.EUH7, headerForm.ControllerID);
				});

				headerForm.Close();
			}
		}

		public void TestOpenParentJob_WhenBillHasChanges_ShouldPromptToSave()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			Factory.Save();

			using (var form = new AsycudaBillForm(bill))
			{
				bill.ABL_BillNumber = "Bill01";

				ShowFormAndClickOpenParentJob(form);

				var message = UnitTestUserNotification.Instance.LastMessage;
				var headerForm = Application.OpenForms.Cast<ZForm>().FirstOrDefault(x => x is ManifestForm) as ManifestForm;

				CombineAssertions(() =>
				{
					AssertContains("Expected warning message", "This bill has not yet been saved. Do you want to save and proceed?", message.Text);
					AssertNotNull("Parent Job form should be opened", headerForm);
				});

				headerForm.Close();
			}
		}

		public void TestOpenParentJob_WhenUserDoesNotSave_ShouldNotShowParentForm()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			Factory.Save();

			using (var form = new AsycudaBillForm(bill))
			{
				bill.ABL_BillNumber = "Bill01";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				ShowFormAndClickOpenParentJob(form);

				var message = UnitTestUserNotification.Instance.LastMessage;
				var headerForm = Application.OpenForms.Cast<ZForm>().FirstOrDefault(x => x is ManifestForm) as ManifestForm;

				CombineAssertions(() =>
				{
					AssertContains("Expected warning message", "This bill has not yet been saved. Do you want to save and proceed?", message.Text);
					AssertNull("Parent Job form should not be opened", headerForm);
				});
			}
		}

		void ShowFormAndClickOpenParentJob(AsycudaBillForm form)
		{
			form.Show();

			var menuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.FindByText("Open Parent Job");
			AssertNotNull("Menu item should be found", menuItem);

			menuItem.PerformClick();
			Application.DoEvents();
		}

		protected override Form GetFormToBashCore()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.PapuaNewGuinea;

			Factory.Save();

			var result = new AsycudaBillForm(bill);
			return result;
		}

		void AssertMessagesTab<TMessagesUserControl>(AsycudaBill bill)
		{
			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();
				var billsAndPacksTabControl = form.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var messagesTab = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name.StartsWith("billsAndPacksTabControl_TabPage_MessagesUserControl"));
				billsAndPacksTabControl.SelectTab(messagesTab);
				var messagesUserControl = messagesTab.FindSingle<TMessagesUserControl>("ManifestSpecificProviderUserControl");

				CombineAssertions(() =>
				{
					AssertNotNull(messagesUserControl);
					AssertType<TMessagesUserControl>(messagesUserControl);
				});
			}
		}

		sealed class MessagesUserControlForTesting : MessagesUserControl { }
	}
}
