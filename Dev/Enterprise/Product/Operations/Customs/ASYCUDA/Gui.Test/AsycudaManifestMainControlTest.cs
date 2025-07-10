using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class AsycudaManifestMainControlTest : TestCaseWithFactory
	{
		public void TestLoadCountryTabs()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			wrapper.Headers.Add(header1);
			header1.AMA_ManifestType = "MGI";

			var header2 = wrapper.Headers.AddNew();
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header2.AMA_ManifestType = "ASY";
			factory.Save();

			consol = Factory.Load<ForwardingConsol>(consol.PK);

			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var tabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", true).First();
				AssertEquals(3, tabControl.TabPages.Count);

				var tabPages = tabControl.TabPages.Cast<ZTabPage>();
				AssertNotNull(tabPages.FirstOrDefault(t => t.Name == "ManifestManagementTabPage"));
				var erTabPage = tabPages.First(t => t.Name == "ERASYTabPage");
				AssertEquals(1, erTabPage.Controls.Count);
				var fjManifestUserControl = erTabPage.Controls[0];
				AssertEquals("Enterprise.Customs.ASYCUDA.GUI.AsycudaManifestUserControl", fjManifestUserControl.GetType().FullName);
				AssertSame(fjManifestUserControl.BindingContext, tabControl.BindingContext);
				var sgTabPage = tabPages.First(t => t.Name == "SGMGITabPage");
				AssertEquals(1, sgTabPage.Controls.Count);
				var sgManifestUserControl = sgTabPage.Controls[0];
				AssertEquals("Enterprise.Customs.ASYCUDA.GUI.AsycudaManifestUserControl", sgManifestUserControl.GetType().FullName);
				AssertSame(sgManifestUserControl.BindingContext, tabControl.BindingContext);
			}
		}

		public void TestManifestsAreSychronisedWithConsol()
		{
			var etd = ZDate.Today;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "VUTAH";
			consol.JK_RL_NKDischargePort = "ERTES";
			consol.MostInterestingTransportForBinding[0].JW_ETDForBinding = etd;
			consol.MostInterestingTransportForBinding[0].JW_ETAForBinding = ZDate.Today.AddDays(5);

			var wrapper = new ManifestHeadersWrapper(consol);
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.FillWithValidTestData();
			header.AMA_ParentId = consol.PK;
			header.AMA_ParentTableCode = "JK";
			header.AMA_E_DEP = etd;
			wrapper.Headers.Add(header);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var consolIn2 = factory2.Load<ForwardingConsol>(consol.PK);
			var wrapperIn2 = new ManifestHeadersWrapper(consolIn2);
			var headerIn2 = wrapperIn2.Headers[0];
			AssertEquals("ETD was persisted", etd, headerIn2.AMA_E_DEP);

			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(consolIn2))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapperIn2, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var newEtd = ZDate.Today.AddDays(2);
				consolIn2.MostInterestingTransportForBinding[0].JW_ETDForBinding = newEtd;
				AssertEquals("ETD has synchronised", newEtd, headerIn2.AMA_E_DEP);
			}
		}

		public void TestSelectAndShowBill()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header1.AMA_OverrideFreightDefaults = true;
			var bill11 = header1.Bills.AddNew();
			var bill12 = header1.Bills.AddNew();
			wrapper.Headers.Add(header1);
			header1.AMA_ManifestType = "MGI";

			var header2 = wrapper.Headers.AddNew();
			header2.AMA_OverrideFreightDefaults = true;
			wrapper.Headers.Add(header2);
			var bill21 = header2.Bills.AddNew();
			var bill22 = header2.Bills.AddNew();
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header2.AMA_ManifestType = "ASY";
			factory.Save();

			consol = Factory.Load<ForwardingConsol>(consol.PK);
			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);

				form.Controls.Add(control);
				var tabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", false).First();
				var erTabPage = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "ERASYTabPage");
				var erUserControl = erTabPage.Controls.OfType<AsycudaManifestUserControl>().First();
				var erGrid = (ZGrid)erUserControl.Controls.Find("BillsGrid", true).Single();
				var sgTabPage = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "SGMGITabPage");
				var sgUserControl = sgTabPage.Controls.OfType<AsycudaManifestUserControl>().First();
				var sgGrid = (ZGrid)sgUserControl.Controls.Find("BillsGrid", true).Single();

				form.Show();

				control.SelectAndShowBill(bill21.PK);
				AssertEquals("erTabPage is selected.", tabControl.SelectedTab, erTabPage);
				AssertEquals("bill21 is selected.", bill21, erGrid.ListManager.GetCurrent());

				control.SelectAndShowBill(bill11.PK);
				AssertEquals("sgTabPage is selected.", tabControl.SelectedTab, sgTabPage);
				AssertEquals("bill11 is selected.", bill11, sgGrid.ListManager.GetCurrent());

				control.SelectAndShowBill(bill22.PK);
				AssertEquals("erTabPage is selected.", tabControl.SelectedTab, erTabPage);
				AssertEquals("bill22 is selected.", bill22, erGrid.ListManager.GetCurrent());

				control.SelectAndShowBill(bill12.PK);
				AssertEquals("sgTabPage is selected.", tabControl.SelectedTab, sgTabPage);
				AssertEquals("bill12 is selected.", bill12, sgGrid.ListManager.GetCurrent());
			}
		}

		public void TestUpdateManifestTab_TabNameChange()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var europeanUnionCode = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(europeanUnionCode);
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, "IC2MS", "DE", "Germany", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, "IC2MS", "BE", "Belgium", startDate, endDate);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			wrapper.Headers.Add(header1);
			header1.AMA_ManifestType = "MGI";

			var header2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.EUICS2.IAsycudaManifestHeader>();
			wrapper.Headers.Add(header2);
			header2.AMA_ManifestType = "ENS";
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Belgium;
			factory.Save();

			consol = Factory.Load<ForwardingConsol>(consol.PK);
			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);

				form.Controls.Add(control);
				var tabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", false).First();
				var sgTabPage = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "SGMGITabPage");
				var euTabPage = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "BEENSTabPage");

				form.Show();
				AssertEquals("Singapore - SG ACCESS Import Manifest", sgTabPage.Text);

				header1.AMA_ManifestType = "MGE";
				AssertEquals("Singapore - SG ACCESS Export Manifest", sgTabPage.Text);

				header1.AMA_ManifestType = "ASY";
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("Manifest Type cannot be set to an invalid value: ASY.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("MGE", header1.AMA_ManifestType);
				AssertEquals("Singapore - SG ACCESS Export Manifest", sgTabPage.Text);

				AssertEquals("Belgium - Entry Summary Declaration (ICS2)", euTabPage.Text);

				header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Germany;
				AssertEquals("Germany - Entry Summary Declaration (ICS2)", euTabPage.Text);
			}
		}

		public void TestUpdateManifestTab_AlreadyExists()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			wrapper.Headers.Add(header1);
			header1.AMA_ManifestType = "MGE";

			var header2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			wrapper.Headers.Add(header2);
			header2.AMA_ManifestType = "MGI";
			factory.Save();

			consol = Factory.Load<ForwardingConsol>(consol.PK);
			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);

				form.Controls.Add(control);
				var tabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", false).First();
				var sgTabPage = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "SGMGITabPage");

				form.Show();
				AssertEquals("Singapore - SG ACCESS Import Manifest", sgTabPage.Text);

				header2.AMA_ManifestType = "MGE";
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("The following manifest already exists on this consol, only one is allowed: Singapore - SG ACCESS Export Manifest", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("MGI", header2.AMA_ManifestType);
				AssertEquals("Singapore - SG ACCESS Import Manifest", sgTabPage.Text);
			}
		}

		public void TestSelectTabByCountry()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header1.Bills.AddNew();
			wrapper.Headers.Add(header1);
			header1.AMA_ManifestType = "MGI";

			var header2 = wrapper.Headers.AddNew();
			wrapper.Headers.Add(header2);
			header2.Bills.AddNew();
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header2.AMA_ManifestType = "ASY";
			factory.Save();

			consol = Factory.Load<ForwardingConsol>(consol.PK);
			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);

				form.Controls.Add(control);
				var tabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", false).First();
				var erTabPage = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "ERASYTabPage");
				var erUserControl = erTabPage.Controls.OfType<AsycudaManifestUserControl>().First();
				var erGrid = (ZGrid)erUserControl.Controls.Find("BillsGrid", true).Single();
				var sgTabPage = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "SGMGITabPage");
				var sgUserControl = sgTabPage.Controls.OfType<AsycudaManifestUserControl>().First();
				var sgGrid = (ZGrid)sgUserControl.Controls.Find("BillsGrid", true).Single();

				form.Show();

				control.SelectTabByCountry("ER", "ASY");
				AssertEquals("erTabPage is selected.", tabControl.SelectedTab, erTabPage);
				Assert(!Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, consol.PK, Core.Constants.CountryCodes.Fiji));

				control.SelectTabByCountry("SG", "MGI");
				AssertEquals("sgTabPage is selected.", tabControl.SelectedTab, sgTabPage);
				Assert(!Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, consol.PK, Core.Constants.CountryCodes.Singapore));

				control.SelectTabByCountry("ER", "ASY");
				AssertEquals("erTabPage is selected.", tabControl.SelectedTab, erTabPage);
			}
		}

		public void TestSavedConsolManifestDuplicates()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory, new ModeAndCountry(ZString.Empty, Core.Constants.CountryCodes.Fiji));
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			var wrapper = new ManifestHeadersWrapper(consol);

			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);

				form.Controls.Add(control);
				var tabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", false).First();
				form.Show();

				var dateTimeNow = ZDateTime.Now;
				var currentUserInitials = Env.CurrentUser.Initials;
				Db.Connection.ExecuteNonQuery($@"INSERT INTO dbo.AsycudaManifestHeader (AMA_PK, AMA_ClusterKey, AMA_ApplicationCode, AMA_ManifestType, AMA_JobReference, AMA_ParentId, AMA_ParentTableCode, AMA_SystemCreateTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditTimeUtc, AMA_SystemLastEditUser, AMA_RN_NKCountry, AMA_GB)
					VALUES ('{ZGuid.NewZGuid()}', 1, '{RefCusCodeListAttributeTypes.Codes.NVC}', 'ASY', '{consol.JK_UniqueConsignRef + "_1"}', '{consol.PK}', 'JK', '{dateTimeNow}', '{currentUserInitials}', '{dateTimeNow}', '{currentUserInitials}', '{Core.Constants.CountryCodes.Fiji}', '{GlbBranch.CurrentBranch.PK}')");

				var headerManagementUserControl = tabControl.TabPages[0].FindSingle<HeaderManagementUserControl>("HeaderManagementUserControl");
				var createHeaderButton = (ZButton)headerManagementUserControl.Controls.Find("CreateHeaderButton", true).First();
				wrapper.WR_CountryCode = Core.Constants.CountryCodes.Fiji;
				wrapper.WR_ManifestType = "ASY";
				createHeaderButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertContains("Another user has already created a Manifest for Fiji. Please close and re-open the Consol to see any newly added Manifest Countries", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions()]
		public void TestSelectTabByCountryWithNullHeaderWrapper()
		{
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(null, string.Empty);
				control.SelectTabByCountry(ZString.Empty, ZString.Empty);
			}
		}

		public void TestCreateNewCountryHasMutexDeleteRemovesMutex()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory, new ModeAndCountry(ZString.Empty, Core.Constants.CountryCodes.Fiji));
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);

				form.Controls.Add(control);
				var tabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", false).First();
				form.Show();

				var headerManagementTabPage = tabControl.TabPages.Cast<ZTabPage>().Last();
				var headerManagementUserControl = headerManagementTabPage.FindSingle<HeaderManagementUserControl>("HeaderManagementUserControl");
				tabControl.SelectTab(headerManagementTabPage);
				headerManagementUserControl.Select();

				var createHeaderButton = headerManagementUserControl.FindSingle<ZButton>("CreateHeaderButton");
				wrapper.WR_CountryCode = Core.Constants.CountryCodes.Fiji;
				wrapper.WR_ManifestType = "ASY";
				createHeaderButton.PerformClick();
				Assert(Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, consol.PK, Core.Constants.CountryCodes.Fiji));
				tabControl.SelectTab(headerManagementTabPage);
				var deleteHeaderButton = headerManagementUserControl.FindSingle<ZButton>("zButtonDeleteManifest");
				wrapper.WR_KeywordCombination = Core.Constants.CountryCodes.Fiji + "|ASY";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddUserResponse("yes");
				deleteHeaderButton.PerformClick();
				Assert(!Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, consol.PK, Core.Constants.CountryCodes.Fiji));
			}
		}

		public void TestSaveOfNewCountryRemovesMutex()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory, new ModeAndCountry(ZString.Empty, Core.Constants.CountryCodes.Fiji));
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);

				form.Controls.Add(control);
				var tabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", false).First();
				form.Show();

				var headerManagementUserControl = tabControl.TabPages[0].FindSingle<HeaderManagementUserControl>("HeaderManagementUserControl");
				var createHeaderButton = (ZButton)headerManagementUserControl.Controls.Find("CreateHeaderButton", true).First();
				wrapper.WR_CountryCode = Core.Constants.CountryCodes.Fiji;
				wrapper.WR_ManifestType = "ASY";
				createHeaderButton.PerformClick();
				Assert(Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, consol.PK, Core.Constants.CountryCodes.Fiji));
				Factory.Save();
				Assert(!Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, consol.PK, Core.Constants.CountryCodes.Fiji));
			}
		}

		public void TestClosingOfFormRemovesAllMutex()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory, new ModeAndCountry(ZString.Empty, Core.Constants.CountryCodes.Fiji));
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);

			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);

				form.Controls.Add(control);
				var tabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", false).First();
				form.Show();

				var headerManagementUserControl = tabControl.TabPages[0].FindSingle<HeaderManagementUserControl>("HeaderManagementUserControl");
				var createHeaderButton = (ZButton)headerManagementUserControl.Controls.Find("CreateHeaderButton", true).First();
				wrapper.WR_CountryCode = Core.Constants.CountryCodes.Fiji;
				wrapper.WR_ManifestType = "ASY";
				createHeaderButton.PerformClick();
				Assert(Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, consol.PK, Core.Constants.CountryCodes.Fiji));
			}
			Assert(!Factory.IsChildLocked(MutexIDs.AsycudaManJobBeingCreated, consol.PK, Core.Constants.CountryCodes.Fiji));
		}

		public void TestClosingOfFormRemovesManifestSynchronisers()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory, new ModeAndCountry(ZString.Empty, Core.Constants.CountryCodes.Fiji));
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "VUTAH";
			consol.JK_RL_NKDischargePort = "FJSUV";

			AsycudaManifestHeader header;
			var wrapper = new ManifestHeadersWrapper(consol);

			using (ManifestCustomsDataRegistry.Instance.EnableManifestConsolDecoupling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(consol))
			using (var control = new AsycudaManifestMainControl())
			{
				control.SetDataBinding(wrapper, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var tabControl = (ZTemplateTabControl)control.Controls.Find("MainTabControl", false).First();
				var tab = tabControl.TabPages[0];
				tabControl.SelectTab(tab);

				header = wrapper.CreateCountry(Core.Constants.CountryCodes.Fiji, "ASY");

				consol.MostInterestingTransportForBinding[0].JW_ETDForBinding = ZDate.Today;
				AssertEquals("ETD has synchronised after creating", ZDate.Today, header.AMA_E_DEP);
			}

			consol.MostInterestingTransportForBinding[0].JW_ETDForBinding = ZDate.Today.AddDays(1);
			AssertEquals("Does not sync without form", ZDate.Today, header.AMA_E_DEP);
		}
	}
}
