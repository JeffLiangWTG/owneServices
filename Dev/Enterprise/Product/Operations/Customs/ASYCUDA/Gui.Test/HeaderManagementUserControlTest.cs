using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class HeaderManagementUserControlTest : TestCaseWithFactory
	{
		public void TestDeleteExistingManifestButton()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header1.FillWithValidTestData();
			header1.AMA_ParentId = consol.PK;
			header1.AMA_ParentTableCode = "JK";
			wrapper.Headers.Add(header1);

			var header2 = wrapper.Headers.AddNew();
			header2.AMA_ParentId = consol.PK;
			header2.AMA_ParentTableCode = "JK";
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header2.AMA_ManifestType = "ASY";
			header2.AMA_MessageStatus = "SNT";
			header2.Messages.AddNew();

			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var tabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", true).First();
				AssertEquals(3, tabControl.TabPages.Count);

				var sgTabPageName = "SGMGITabPage";
				var actualNames = tabControl.TabPages.Cast<ZTabPage>().Select(c => c.Name);
				var expectedNames = new List<string>
				{
					sgTabPageName,
					"ERASYTabPage",
					"ManifestManagementTabPage"
				};

				AssertContainsExactElementsInAnyOrder(expectedNames, actualNames);
				expectedNames.Remove(sgTabPageName);

				var sgCountryControl = tabControl.TabPages[sgTabPageName].FindSingleOrDefault<AsycudaManifestUserControl>();
				var headerManagementTabPage = tabControl.TabPages.Cast<ZTabPage>().Last();
				var headerManagementUserControl = headerManagementTabPage.FindSingle<HeaderManagementUserControl>("HeaderManagementUserControl");
				tabControl.SelectTab(headerManagementTabPage);
				headerManagementUserControl.Select();
				var zButtonDeleteManifest = headerManagementUserControl.FindSingle<ZButton>("zButtonDeleteManifest", 2);
				wrapper.WR_KeywordCombination = "SG|MGI";

				var unitTestUserNotification = (UnitTestUserNotification)Globals.Message;
				unitTestUserNotification.ClearMessagesAndAnswers();
				unitTestUserNotification.AddUserResponse("yes");
				zButtonDeleteManifest.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("Are you sure you would like to delete this manifest?", unitTestUserNotification.LastMessage.Text);
					AssertEquals("yes", unitTestUserNotification.LastConfirmationStringShown);
					AssertEquals("", wrapper.WR_KeywordCombination);
					AssertContainsExactElementsInAnyOrder(expectedNames, tabControl.TabPages.Cast<ZTabPage>().Select(c => c.Name));
					Assert("Dispose the Controls of the Delete Header", sgCountryControl.IsDisposed);
				});

				wrapper.WR_KeywordCombination = "ER|ASY";

				unitTestUserNotification.ClearMessagesAndAnswers();
				zButtonDeleteManifest.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals("This manifest cannot be deleted as it contains submitted messages.", unitTestUserNotification.LastMessage.Text);
					AssertContainsExactElementsInAnyOrder(expectedNames, tabControl.TabPages.Cast<ZTabPage>().Select(c => c.Name));
				});
			}
		}

		public void TestDeleteExistingManifestButton_ManifestHeaderHasAnEDIMessageButMessageStatusIsEmpty()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>();
			header.AMA_ParentId = consol.PK;
			header.AMA_ParentTableCode = "JK";
			wrapper.Headers.Add(header);
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header.AMA_MessageStatus = ZString.Empty;
			header.Messages.AddNew();

			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var tabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", true).First();
				AssertEquals(2, tabControl.TabPages.Count);

				var actualNames = tabControl.TabPages.Cast<ZTabPage>().Select(c => c.Name);
				var expectedNames = new List<string>
				{
					Core.Constants.CountryCodes.Eritrea + "TabPage",
					"ManifestManagementTabPage"
				};

				AssertContainsExactElementsInAnyOrder(expectedNames, actualNames);

				wrapper.WR_KeywordCombination = "ER";

				var unitTestUserNotification = (UnitTestUserNotification)Globals.Message;
				unitTestUserNotification.ClearMessagesAndAnswers();

				var headerManagementTabPage = tabControl.TabPages.Cast<ZTabPage>().Last();
				var headerManagementUserControl = headerManagementTabPage.FindSingle<HeaderManagementUserControl>("HeaderManagementUserControl");
				tabControl.SelectTab(headerManagementTabPage);
				headerManagementUserControl.Select();
				var zButtonDeleteManifest = headerManagementUserControl.FindSingle<ZButton>("zButtonDeleteManifest", 2);
				zButtonDeleteManifest.PerformClick();
				Application.DoEvents();
				CombineAssertions(() =>
				{
					AssertEquals("This manifest cannot be deleted as it contains submitted messages.", unitTestUserNotification.LastMessage.Text);
					AssertContainsExactElementsInAnyOrder(expectedNames, tabControl.TabPages.Cast<ZTabPage>().Select(c => c.Name));
				});
			}
		}

		public void TestDeleteExistingManifestButton_ManifestHasAnEDIMessageButMessageStatusIsEmpty()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>();
			header.AMA_ParentId = consol.PK;
			header.AMA_ParentTableCode = "JK";
			wrapper.Headers.Add(header);
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header.AMA_MessageStatus = ZString.Empty;
			header.Messages.AddNew();

			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var tabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", true).First();
				AssertEquals(2, tabControl.TabPages.Count);

				var actualNames = tabControl.TabPages.Cast<ZTabPage>().Select(c => c.Name);
				var expectedNames = new List<string>
				{
					Core.Constants.CountryCodes.Eritrea + "TabPage",
					"ManifestManagementTabPage"
				};

				AssertContainsExactElementsInAnyOrder(expectedNames, actualNames);

				wrapper.WR_KeywordCombination = "ER";

				var unitTestUserNotification = (UnitTestUserNotification)Globals.Message;
				unitTestUserNotification.ClearMessagesAndAnswers();

				var headerManagementTabPage = tabControl.TabPages.Cast<ZTabPage>().Last();
				var headerManagementUserControl = headerManagementTabPage.FindSingle<HeaderManagementUserControl>("HeaderManagementUserControl");
				tabControl.SelectTab(headerManagementTabPage);
				headerManagementUserControl.Select();
				var zButtonDeleteManifest = headerManagementUserControl.FindSingle<ZButton>("zButtonDeleteManifest", 2);
				zButtonDeleteManifest.PerformClick();
				Application.DoEvents();
				CombineAssertions(() =>
				{
					AssertEquals("This manifest cannot be deleted as it contains submitted messages.", unitTestUserNotification.LastMessage.Text);
					AssertContainsExactElementsInAnyOrder(expectedNames, tabControl.TabPages.Cast<ZTabPage>().Select(c => c.Name));
				});
			}
		}

		public void TestDeleteExistingManifestButton_BillHasAnEDIMessageButMessageStatusIsEmpty()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>();
			header.AMA_ParentId = consol.PK;
			header.AMA_ParentTableCode = "JK";
			wrapper.Headers.Add(header);
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header.AMA_MessageStatus = ZString.Empty;
			var bill = header.Bills.AddNew();
			bill.Messages.AddNew();

			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var tabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", true).First();
				AssertEquals(2, tabControl.TabPages.Count);

				var actualNames = tabControl.TabPages.Cast<ZTabPage>().Select(c => c.Name);
				var expectedNames = new List<string>
				{
					Core.Constants.CountryCodes.Eritrea + "TabPage",
					"ManifestManagementTabPage"
				};

				AssertContainsExactElementsInAnyOrder(expectedNames, actualNames);

				wrapper.WR_KeywordCombination = "ER";

				var unitTestUserNotification = (UnitTestUserNotification)Globals.Message;
				unitTestUserNotification.ClearMessagesAndAnswers();

				var headerManagementTabPage = tabControl.TabPages.Cast<ZTabPage>().Last();
				var headerManagementUserControl = headerManagementTabPage.FindSingle<HeaderManagementUserControl>("HeaderManagementUserControl");
				tabControl.SelectTab(headerManagementTabPage);
				headerManagementUserControl.Select();
				var zButtonDeleteManifest = headerManagementUserControl.FindSingle<ZButton>("zButtonDeleteManifest", 2);
				zButtonDeleteManifest.PerformClick();
				Application.DoEvents();
				CombineAssertions(() =>
				{
					AssertEquals("This manifest cannot be deleted as it contains submitted messages.", unitTestUserNotification.LastMessage.Text);
					AssertContainsExactElementsInAnyOrder(expectedNames, tabControl.TabPages.Cast<ZTabPage>().Select(c => c.Name));
				});
			}
		}
	}

	sealed class HeaderManagementUserControl_Decoupled_Test : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var control = new HeaderManagementUserControl())
			{
				var manifestsButtonGrid = control.FindSingle<ZModuleButtonGrid>("ManifestsGrid");
				var grid = manifestsButtonGrid.InnerGrid;

				AssertEquals("AMA_JobReference is Mandatory", true, grid.GetColumnStyle("AMA_JobReference").IsMandatory);
				AssertEquals("CountryName is Mandatory", true, grid.GetColumnStyle("CountryName").IsMandatory);
				AssertEquals("AMA_ManifestType is Mandatory", true, grid.GetColumnStyle("AMA_ManifestType").IsMandatory);

				AssertEquals("AMA_Nature is not Mandatory", false, grid.GetColumnStyle("AMA_Nature").IsMandatory);
				AssertEquals("AMA_MasterBill is not Mandatory", false, grid.GetColumnStyle("AMA_MasterBill").IsMandatory);
				AssertEquals("AMA_RL_NKPortOfLoading is not Mandatory", false, grid.GetColumnStyle("AMA_RL_NKPortOfLoading").IsMandatory);
				AssertEquals("AMA_E_DEP is not Mandatory", false, grid.GetColumnStyle("AMA_E_DEP").IsMandatory);
				AssertEquals("AMA_RL_NKPortOfDischarge is not Mandatory", false, grid.GetColumnStyle("AMA_RL_NKPortOfDischarge").IsMandatory);
				AssertEquals("AMA_E_ARV is not Mandatory", false, grid.GetColumnStyle("AMA_E_ARV").IsMandatory);
				AssertEquals("AMA_MessageStatus is not Mandatory", false, grid.GetColumnStyle("AMA_MessageStatus").IsMandatory);
				AssertEquals("RegistrationStatusCodeAndDescription is not Mandatory", false, grid.GetColumnStyle("RegistrationStatusCodeAndDescription").IsMandatory);
			}
		}

		public void TestCountryTabsAndDeleteButtonAreHiddenWhenRegistryIsEnabled()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);
			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header1.FillWithValidTestData();
			header1.AMA_ParentId = consol.PK;
			header1.AMA_ParentTableCode = "JK";
			wrapper.Headers.Add(header1);

			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var tabControl = control.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertEquals("TabPages", 1, tabControl.TabPages.Count);
				var headerManagementTabPage = tabControl.TabPages[0];
				tabControl.SelectTab(headerManagementTabPage);

				var countryCodeDropEdit = headerManagementTabPage.FindSingle<ZDropEdit>("WR_CountryCodeDropEdit", 2);
				Assert("WR_CountryCodeDropEdit Visible", countryCodeDropEdit.Visible);
				Assert("WR_CountryCodeDropEdit Enabled", countryCodeDropEdit.Enabled);
				var manifestTypeDropEdit = headerManagementTabPage.FindSingle<ZDropEdit>("WR_ManifestTypeDropEdit", 2);
				Assert("WR_ManifestTypeDropEdit Visible", manifestTypeDropEdit.Visible);
				Assert("WR_ManifestTypeDropEdit Enabled", manifestTypeDropEdit.Enabled);
				var createHeaderButton = headerManagementTabPage.FindSingle<ZButton>("CreateHeaderButton", 2);
				Assert("CreateHeaderButton Visible", createHeaderButton.Visible);
				Assert("CreateHeaderButton Enabled", createHeaderButton.Enabled);

				var zDropEditManifestToDelete = headerManagementTabPage.FindSingle<ZDropEditWithFixedWidth>("zDropEditManifestToDelete", 2);
				Assert("zDropEditManifestToDelete NOT Visible", !zDropEditManifestToDelete.Visible);
				var zButtonDeleteManifest = headerManagementTabPage.FindSingle<ZButton>("zButtonDeleteManifest", 2);
				Assert("zButtonDeleteManifest NOT Visible", !zButtonDeleteManifest.Visible);

				var manifestsModuleGrid = headerManagementTabPage.FindSingle<ZModuleButtonGrid>("ManifestsGrid");
				var manifestsGrid = manifestsModuleGrid.InnerGrid;
				manifestsGrid.SelectSingleElementByPK(header1.PK);
				var deleteItem = manifestsGrid.ContextMenu.MenuItems.FindByText("Delete");
				Assert("Context menu item 'Delete' Visible", deleteItem.Visible);
				Assert("Context menu item 'Delete' Enabled", deleteItem.Enabled);
			}
		}

		public void TestDoubleClickManifestEntryInGridOpensManifestInAForm()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header1.FillWithValidTestData();
			header1.AMA_ParentId = consol.PK;
			header1.AMA_ParentTableCode = "JK";
			wrapper.Headers.Add(header1);
			Factory.Save();

			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var tabControl = control.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertEquals("TabPages", 1, tabControl.TabPages.Count);
				var headerManagementTabPage = tabControl.TabPages[0];
				tabControl.SelectTab(headerManagementTabPage);

				var manifestsButtonGrid = headerManagementTabPage.FindSingle<ZModuleButtonGrid>("ManifestsGrid");
				var manifestsGrid = manifestsButtonGrid.InnerGrid;
				AssertEquals("manifestsGrid rows", 1, manifestsGrid.VisibleRowCount);

				IZForm manifestForm = null;
				try
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					manifestsGrid.SelectSingleElementByPK(header1.PK);
					manifestsGrid.PerformMouseDownForTest(row: 0, clicks: 2);

					manifestForm = manifestsButtonGrid.LastShownZForm;
					AssertType<ManifestForm>("Last Shown Form", manifestForm);
				}
				finally
				{
					manifestForm?.Dispose();
				}
			}
		}

		public void TestCreateButtonOpensNewManifestInAForm()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			var wrapper = new ManifestHeadersWrapper(consol);
			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var tabControl = control.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertEquals(1, tabControl.TabPages.Count);
				var headerManagementTabPage = tabControl.TabPages[0];
				tabControl.SelectTab(headerManagementTabPage);

				var manifestsButtonGrid = headerManagementTabPage.FindSingle<ZModuleButtonGrid>("ManifestsGrid");
				var manifestsGrid = manifestsButtonGrid.InnerGrid;
				AssertNotNull("manifestsGrid", manifestsGrid);
				AssertEquals("manifestsGrid rows", 0, manifestsGrid.VisibleRowCount);

				var createHeaderButton = headerManagementTabPage.FindSingle<ZButton>("CreateHeaderButton", 2);
				createHeaderButton.PerformClick();

				CombineAssertions(() =>
				{
					AssertContains("There are errors that need to be corrected before this Manifest can be created.\r\n\r\nYou have not entered a New Manifest Country", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("manifestsGrid rows", 0, manifestsGrid.VisibleRowCount);
				});

				var countryCodeDropEdit = headerManagementTabPage.FindSingle<ZDropEdit>("WR_CountryCodeDropEdit", 2);
				countryCodeDropEdit.Text = "US";
				countryCodeDropEdit.CommitBoundValue();

				var manifestTypeDropEdit = headerManagementTabPage.FindSingle<ZDropEdit>("WR_ManifestTypeDropEdit", 2);
				manifestTypeDropEdit.Text = "IAM";
				manifestTypeDropEdit.CommitBoundValue();

				IZForm manifestForm = null;
				try
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					createHeaderButton.PerformClick();
					manifestForm = manifestsButtonGrid.LastShownZForm;

					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("wrapper.Headers", 1, wrapper.Headers.Count);
					AssertEquals("manifestsGrid rows", 1, manifestsGrid.VisibleRowCount);
					AssertType<ManifestForm>("Last Shown Form", manifestForm);
				}
				finally
				{
					manifestForm?.Dispose();
				}
			}
		}

		public void TestContextMenuDeleteRemovesManifest()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header1.FillWithValidTestData();
			header1.AMA_ParentId = consol.PK;
			header1.AMA_ParentTableCode = "JK";
			wrapper.Headers.Add(header1);

			var header2 = wrapper.Headers.AddNew();
			header2.AMA_ParentId = consol.PK;
			header2.AMA_ParentTableCode = "JK";
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header2.AMA_ManifestType = "ASY";
			header2.AMA_MessageStatus = "SNT";

			var message = Factory.New<Customs.Business.Testing.TestMessage>();
			header2.Messages.Add(message);
			Factory.Save();

			var header1Ref = header1.AMA_JobReference;
			var header2Ref = header2.AMA_JobReference;

			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var tabControl = control.FindSingle<ZTemplateTabControl>("MainTabControl");
				var headerManagementTabPage = tabControl.TabPages["ManifestManagementTabPage"];
				tabControl.SelectTab(headerManagementTabPage);

				var manifestsModuleGrid = headerManagementTabPage.FindSingle<ZModuleButtonGrid>("ManifestsGrid");
				var manifestsGrid = manifestsModuleGrid.InnerGrid;
				AssertEquals("manifestsGrid rows", 2, manifestsGrid.VisibleRowCount);

				var unitTestUserNotification = (UnitTestUserNotification)Globals.Message;
				unitTestUserNotification.ClearMessagesAndAnswers();
				unitTestUserNotification.AddUserResponse("yes");

				manifestsGrid.SelectSingleElementByPK(header1.PK);
				manifestsGrid.ContextMenu.MenuItems.FindByText("Delete").PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals($"Are you sure you would like to delete\r\n {header1Ref}", unitTestUserNotification.LastMessage.Text);
					AssertEquals("yes", unitTestUserNotification.LastConfirmationStringShown);
					AssertEquals("manifestsGrid rows", 1, manifestsGrid.VisibleRowCount);
					Assert("header1 Deleted", header1.IsDeleted);
				});

				unitTestUserNotification.ClearMessagesAndAnswers();
				manifestsGrid.SelectSingleElementByPK(header2.PK);
				manifestsGrid.ContextMenu.MenuItems.FindByText("Delete").PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals($"Manifests with submitted messages cannot be deleted.\r\n {header2Ref}", unitTestUserNotification.LastMessage.Text);
					AssertEquals("manifestsGrid rows", 1, manifestsGrid.VisibleRowCount);
					Assert("header2 NOT Deleted", !header2.IsDeleted);
				});

				Factory.Save();
			}
		}

		public void TestContextMenuCanDeleteNewlyCreatedManifest()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			var wrapper = new ManifestHeadersWrapper(consol);
			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var tabControl = control.FindSingle<ZTemplateTabControl>("MainTabControl");
				var headerManagementTabPage = tabControl.TabPages["ManifestManagementTabPage"];
				tabControl.SelectTab(headerManagementTabPage);

				var manifestsModuleGrid = headerManagementTabPage.FindSingle<ZModuleButtonGrid>("ManifestsGrid");
				var manifestsGrid = manifestsModuleGrid.InnerGrid;
				AssertEquals("manifestsGrid rows", 0, manifestsGrid.VisibleRowCount);

				var countryCodeDropEdit = headerManagementTabPage.FindSingle<ZDropEdit>("WR_CountryCodeDropEdit", 2);
				countryCodeDropEdit.Text = "US";
				countryCodeDropEdit.CommitBoundValue();

				var manifestTypeDropEdit = headerManagementTabPage.FindSingle<ZDropEdit>("WR_ManifestTypeDropEdit", 2);
				manifestTypeDropEdit.Text = "IAM";
				manifestTypeDropEdit.CommitBoundValue();

				ZForm manifestForm = null;
				try
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var createHeaderButton = headerManagementTabPage.FindSingle<ZButton>("CreateHeaderButton", 2);
					createHeaderButton.PerformClick();
					manifestForm = (ZForm)manifestsModuleGrid.LastShownZForm;

					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("wrapper.Headers", 1, wrapper.Headers.Count);
				}
				finally
				{
					manifestForm?.ForceClose();
					manifestForm?.Dispose();
				}

				var unitTestUserNotification = (UnitTestUserNotification)Globals.Message;
				unitTestUserNotification.ClearMessagesAndAnswers();
				unitTestUserNotification.AddUserResponse("yes");

				var header1 = wrapper.Headers[0];
				var header1Ref = header1.AMA_JobReference;

				manifestsGrid.SelectSingleElementByPK(header1.PK);
				manifestsGrid.ContextMenu.MenuItems.FindByText("Delete").PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals($"Are you sure you would like to delete\r\n {header1Ref}", unitTestUserNotification.LastMessage.Text);
					AssertEquals("yes", unitTestUserNotification.LastConfirmationStringShown);
					AssertEquals("wrapper.Headers", 0, wrapper.Headers.Count);
					Assert("header1 Deleted", header1.IsDeleted);
				});

				Factory.Save();
			}
		}

		public void TestContextMenuDeleteMultiselectRemovesManifests()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header1.FillWithValidTestData();
			header1.AMA_ParentId = consol.PK;
			header1.AMA_ParentTableCode = "JK";
			wrapper.Headers.Add(header1);
			var header2 = wrapper.Headers.AddNew();
			header2.AMA_ParentId = consol.PK;
			header2.AMA_ParentTableCode = "JK";
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header2.AMA_ManifestType = "ASY";
			Factory.Save();

			var header1Ref = header1.AMA_JobReference;
			var header2Ref = header2.AMA_JobReference;

			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var tabControl = control.FindSingle<ZTemplateTabControl>("MainTabControl");
				var headerManagementTabPage = tabControl.TabPages["ManifestManagementTabPage"];
				tabControl.SelectTab(headerManagementTabPage);

				var manifestsModuleGrid = headerManagementTabPage.FindSingle<ZModuleButtonGrid>("ManifestsGrid");
				var manifestsGrid = manifestsModuleGrid.InnerGrid;
				AssertEquals("manifestsGrid rows", 2, manifestsGrid.VisibleRowCount);

				var unitTestUserNotification = (UnitTestUserNotification)Globals.Message;
				unitTestUserNotification.ClearMessagesAndAnswers();
				unitTestUserNotification.AddUserResponse("yes");

				manifestsGrid.SelectAllElements();
				manifestsGrid.ContextMenu.MenuItems.FindByText("Delete").PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals($"Are you sure you would like to delete\r\n {header1Ref}, {header2Ref}", unitTestUserNotification.LastMessage.Text);
					AssertEquals("yes", unitTestUserNotification.LastConfirmationStringShown);
					AssertEquals("manifestsGrid rows", 0, manifestsGrid.VisibleRowCount);
					Assert("header1 Deleted", header1.IsDeleted);
					Assert("header2 Deleted", header2.IsDeleted);
				});
			}
		}

		public void TestContextMenuDeleteMultiselectFailsOnError()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header1.FillWithValidTestData();
			header1.AMA_ParentId = consol.PK;
			header1.AMA_ParentTableCode = "JK";
			wrapper.Headers.Add(header1);

			var header2 = wrapper.Headers.AddNew();
			header2.AMA_ParentId = consol.PK;
			header2.AMA_ParentTableCode = "JK";
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header2.AMA_ManifestType = "ASY";
			header2.AMA_MessageStatus = "SNT";

			var message = Factory.New<Customs.Business.Testing.TestMessage>();
			header2.Messages.Add(message);
			Factory.Save();

			var header1Ref = header1.AMA_JobReference;
			var header2Ref = header2.AMA_JobReference;

			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var tabControl = control.FindSingle<ZTemplateTabControl>("MainTabControl");
				var headerManagementTabPage = tabControl.TabPages["ManifestManagementTabPage"];
				tabControl.SelectTab(headerManagementTabPage);

				var manifestsModuleGrid = headerManagementTabPage.FindSingle<ZModuleButtonGrid>("ManifestsGrid");
				var manifestsGrid = manifestsModuleGrid.InnerGrid;
				AssertEquals("manifestsGrid rows", 2, manifestsGrid.VisibleRowCount);

				var unitTestUserNotification = (UnitTestUserNotification)Globals.Message;
				unitTestUserNotification.ClearMessagesAndAnswers();
				unitTestUserNotification.AddUserResponse("yes");

				manifestsGrid.SelectAllElements();
				manifestsGrid.ContextMenu.MenuItems.FindByText("Delete").PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals($"Manifests with submitted messages cannot be deleted.\r\n {header2Ref}", unitTestUserNotification.LastMessage.Text);
					AssertEquals("manifestsGrid rows", 2, manifestsGrid.VisibleRowCount);
					Assert("header1 NOT Deleted", !header1.IsDeleted);
					Assert("header2 NOT Deleted", !header2.IsDeleted);
				});
			}
		}

		[ExpectNoExceptions]
		public void TestContextMenuImportData()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var control = new HeaderManagementUserControl())
			using (var form = new ZForm())
			{
				control.SetDataBinding(wrapper, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var manifestsGrid = control.FindSingle<ZModuleButtonGrid>("ManifestsGrid").InnerGrid;
				manifestsGrid.ContextMenu.DoPopup();
				manifestsGrid.ContextMenu.MenuItems.FindByText("&Import Data...").PerformClick();
			}
		}
	}
}
