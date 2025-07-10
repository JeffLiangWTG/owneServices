using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public abstract class ShowFormUrlHandlerTestCase : TestCaseWithFactory
	{
		#region Create

		protected abstract ShowFormUrlHandler UrlHandlerForTest { get; }
		protected abstract string UrlCommandForTest { get; }

		public virtual void TestCreate()
		{
			using var resetRegistry = RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var serverNameAndDatabaseName = $"&ServerName={InstanceDetails.Current.ServerName}&DatabaseName={InstanceDetails.Current.DatabaseName}";
			AssertEquals(
				"If the result of Create changes, the shortcuts for existing clients will break",
				"edient:Command=" + UrlCommandForTest + "&LicenceCode=EDIEDIDAT&ControllerID=Dummy&BusinessEntityPK=69bb50ad-518b-3360-8adb-52dad4dcbc02&VersionNumber=" + VersionNumber + serverNameAndDatabaseName + "&Hash=%2bndahYMOqwvqDAacEENsOZ4xKM119aKFA",
				UrlHandlerForTest.Create(DummyControllerIDs.Dummy, new ZGuid("{69BB50AD-518B-3360-8ADB-52DAD4DCBC02}")));

			AssertEquals(
				"The 'LicenceCode' parameter should not be embedded into the url if the MakeUrlsOnlyOpenableForCurrentCompany is set to false",
				"edient:Command=" + UrlCommandForTest + "&ControllerID=Dummy4&BusinessEntityPK=11111111-2222-3333-4444-555555555555&VersionNumber=" + VersionNumber + serverNameAndDatabaseName + "&Hash=%2bydMQIYv6GkP41kawCP4wAXa%2fnXqgWOFS",
				UrlHandlerForTest.Create(DummyControllerIDs.Dummy4, new ZGuid("{11111111-2222-3333-4444-555555555555}")));

			AssertEquals(
				"The 'LicenceCode' parameter should be the specified licence code",
				"edient:Command=" + UrlCommandForTest + "&LicenceCode=AAABBBCCC&ControllerID=Dummy&BusinessEntityPK=11111111-2222-3333-4444-555555555555&VersionNumber=" + VersionNumber + serverNameAndDatabaseName + "&Hash=%2bfxBLt1yxvcT5%2f1k8kLrzYmkB4imUC3rr",
				UrlHandlerForTest.CreateWithSpecifiedLicenceCode(DummyControllerIDs.Dummy, new ZGuid("{11111111-2222-3333-4444-555555555555}"), "AAABBBCCC"));
		}

		public virtual void TestCreate_Default()
		{
			using var resetRegistry = RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.DefaultValue);

			AssertEquals(
				"If the result of Create changes, the shortcuts for existing clients will break",
				"edient:Command=" + UrlCommandForTest + "&LicenceCode=EDIEDIDAT&ControllerID=Dummy&BusinessEntityPK=69bb50ad-518b-3360-8adb-52dad4dcbc02&VersionNumber=" + VersionNumber + "&Hash=%2bndahYMOqwvqDAacEENsOZ4xKM119aKFA",
				UrlHandlerForTest.Create(DummyControllerIDs.Dummy, new ZGuid("{69BB50AD-518B-3360-8ADB-52DAD4DCBC02}")));

			AssertEquals(
				"The 'LicenceCode' parameter should not be embedded into the url if the MakeUrlsOnlyOpenableForCurrentCompany is set to false",
				"edient:Command=" + UrlCommandForTest + "&ControllerID=Dummy4&BusinessEntityPK=11111111-2222-3333-4444-555555555555&VersionNumber=" + VersionNumber + "&Hash=%2bydMQIYv6GkP41kawCP4wAXa%2fnXqgWOFS",
				UrlHandlerForTest.Create(DummyControllerIDs.Dummy4, new ZGuid("{11111111-2222-3333-4444-555555555555}")));

			AssertEquals(
				"The 'LicenceCode' parameter should be the specified licence code",
				"edient:Command=" + UrlCommandForTest + "&LicenceCode=AAABBBCCC&ControllerID=Dummy&BusinessEntityPK=11111111-2222-3333-4444-555555555555&VersionNumber=" + VersionNumber + "&Hash=%2bfxBLt1yxvcT5%2f1k8kLrzYmkB4imUC3rr",
				UrlHandlerForTest.CreateWithSpecifiedLicenceCode(DummyControllerIDs.Dummy, new ZGuid("{11111111-2222-3333-4444-555555555555}"), "AAABBBCCC"));
		}

		public void TestRegisteredDomainInstanceContainsDomainAndInstanceName()
		{
			var mockCargoWiseInstanceClass = new Mock<ICargoWiseOneInstanceClass>();
			var mockCargoWiseOneInstanceSearchResult = new Mock<ICargoWiseOneInstanceSearchResult>();
			mockCargoWiseInstanceClass.Setup(o => o.ExistsInCurrentSchema()).Returns(true);
			mockCargoWiseInstanceClass.Setup(o => o.FindInstanceByDatabase(Db.ServerName, Db.DatabaseName, It.IsAny<DirectorySearcherWrapper>(), It.IsAny<DirectorySearchOptions>())).Returns(mockCargoWiseOneInstanceSearchResult.Object);
			mockCargoWiseOneInstanceSearchResult.SetupGet(o => o.Name).Returns("Foo");
			ObjectFactory.Substitute(mockCargoWiseInstanceClass.Object);

			var domain = Domain.GetCurrentDomain();
			AssertNotNull("Prerequisite: test must run on a machine joined to a domain", domain);

			using (InstanceDetails.ReCalculateForTest())
			{
				var url = UrlHandlerForTest.CreateWithoutApplicationContext(DummyControllerIDs.Dummy, new ZGuid("{69BB50AD-518B-3360-8ADB-52DAD4DCBC02}"));
				AssertContains("Domain=" + domain.Name, url);
				AssertContains("Instance=Foo", url);
			}
		}

		public void TestCreate_WithArgs()
		{
			using var resetRegistry = RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var serverNameAndDatabaseName = $"&ServerName={InstanceDetails.Current.ServerName}&DatabaseName={InstanceDetails.Current.DatabaseName}";

			var args = new[] { "first", "second", "third" };
			var url = UrlHandlerForTest.Create(DummyControllerIDs.Dummy, new ZGuid("{69BB50AD-518B-3360-8ADB-52DAD4DCBC02}"), args);

			AssertEquals("Args should be included in the url, and yet...",
				string.Format(CultureInfo.InvariantCulture, "edient:Command={0}&LicenceCode=EDIEDIDAT&ControllerID=Dummy&BusinessEntityPK=69bb50ad-518b-3360-8adb-52dad4dcbc02&VersionNumber={1}{2}&Args=first%7csecond%7cthird&Hash={3}", UrlCommandForTest, VersionNumber, serverNameAndDatabaseName, ExpectedHash), url);

			args = new[] { "first | after pipe", "second, with comma", "third with + a plus", "|", ",", "+" };
			url = UrlHandlerForTest.Create(DummyControllerIDs.Dummy, new ZGuid("{69BB50AD-518B-3360-8ADB-52DAD4DCBC02}"), args);

			AssertEquals("Args should be included in the url, and yet...",
				string.Format(CultureInfo.InvariantCulture, "edient:Command={0}&LicenceCode=EDIEDIDAT&ControllerID=Dummy&BusinessEntityPK=69bb50ad-518b-3360-8adb-52dad4dcbc02&VersionNumber={1}{2}&Args=first+_ArgsDelimiter_+after+pipe%7csecond%2c+with+comma%7cthird+with+%2b+a+plus%7c_ArgsDelimiter_%7c%2c%7c%2b&Hash={3}", UrlCommandForTest, VersionNumber, serverNameAndDatabaseName, ExpectedHash), url);

			var queryString = new QueryString(url);
			var returnedArgs = UrlHandler.GetArgs(queryString);

			AssertArrayEqualsByElements(args, returnedArgs.ToArray());
		}

		public void TestCreate_WithArgs_Default()
		{
			using var resetRegistry = RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.DefaultValue);

			var args = new[] { "first", "second", "third" };
			var url = UrlHandlerForTest.Create(DummyControllerIDs.Dummy, new ZGuid("{69BB50AD-518B-3360-8ADB-52DAD4DCBC02}"), args);

			AssertEquals("Args should be included in the url, and yet...",
				string.Format(CultureInfo.InvariantCulture, "edient:Command={0}&LicenceCode=EDIEDIDAT&ControllerID=Dummy&BusinessEntityPK=69bb50ad-518b-3360-8adb-52dad4dcbc02&VersionNumber={1}&Args=first%7csecond%7cthird&Hash={2}", UrlCommandForTest, VersionNumber, ExpectedHash), url);

			args = new[] { "first | after pipe", "second, with comma", "third with + a plus", "|", ",", "+" };
			url = UrlHandlerForTest.Create(DummyControllerIDs.Dummy, new ZGuid("{69BB50AD-518B-3360-8ADB-52DAD4DCBC02}"), args);

			AssertEquals("Args should be included in the url, and yet...",
				string.Format(CultureInfo.InvariantCulture, "edient:Command={0}&LicenceCode=EDIEDIDAT&ControllerID=Dummy&BusinessEntityPK=69bb50ad-518b-3360-8adb-52dad4dcbc02&VersionNumber={1}&Args=first+_ArgsDelimiter_+after+pipe%7csecond%2c+with+comma%7cthird+with+%2b+a+plus%7c_ArgsDelimiter_%7c%2c%7c%2b&Hash={2}", UrlCommandForTest, VersionNumber, ExpectedHash), url);

			var queryString = new QueryString(url);
			var returnedArgs = UrlHandler.GetArgs(queryString);

			AssertArrayEqualsByElements(args, returnedArgs.ToArray());
		}

		public void TestWebLaunchUrlInTrampolineData()
		{
			using (RawDataRegistry.Instance.WebVersionLaunchUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost:5001"))
			{
				var url = UrlHandlerForTest.CreateFromTrampolineData("controller", new Guid("11111111-2222-3333-4444-555555555555"), "licence");
				AssertContains("WebVersionLaunchUrl=https%3a%2f%2flocalhost%3a5001", url);
			}
		}

		public void TestNoWebLaunchUrlInTrampolineData()
		{
			using (RawDataRegistry.Instance.WebVersionLaunchUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, null))
			{
				var url = UrlHandlerForTest.CreateFromTrampolineData("controller", new Guid("11111111-2222-3333-4444-555555555555"), "licence");
				AssertNotContains("WebVersionLaunchURL", url);
			}
		}

		protected virtual string ExpectedHash => "%2bndahYMOqwvqDAacEENsOZ4xKM119aKFA";

		#endregion

		public void TestGetCorrectControllerWithUrlException()
		{
			var handler = new TestShowFormUrlHandlerWithErrorReport(this);
			EnterpriseUrlHandlerService.UnregisterUrlHandler(UrlHandlerForTest);
			EnterpriseUrlHandlerService.RegisterUrlHandler(handler);

			try
			{
				var dummyNavigationProvider = Factory.New<DummyBusinessObject>();
				dummyNavigationProvider.Z0_Guid = Guid.NewGuid();
				Factory.Save();
				var url = handler.Create(DummyControllerIDs.Dummy1, ZGuid.NewZGuid());
				handler.IsMainFormAvailable = false;

				var ex = AssertExceptionThrown<EnterpriseUrlHandlerException>("Should throw EnterpriseUrlHandlerException", () => { var result = handler.GetCorrectControllerForTest(DummyControllerIDs.DummyControllerWithPlugInAndNavigationProvider, dummyNavigationProvider.Z0_Guid); });
				var expectedErrorMessage = $"This is not a valid {Enterprise.Core.Constants.ProductName} shortcut or hyperlink. Record state was changed and this operation is now invalid."; // Developer Error Message
				AssertEquals("Should contain error message about inconsistent Controller ID", expectedErrorMessage, ex.Message);
			}
			finally
			{
				EnterpriseUrlHandlerService.UnregisterUrlHandler(handler);
				EnterpriseUrlHandlerService.RegisterUrlHandler(UrlHandlerForTest);
			}
		}

		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Checking for existing exception message.")]
		public void TestShouldNotHandleIncidentRequests_WhenNotInEDIExtension()
		{
			// see also ShowFormUrlHandlerEDITest in ZClientEDI for the counterpart test
			AssertNotEquals("Precondition: not in EDI extension", Clients.EDI, ClientHookLoader.Instance?.Client);
			var pk = Guid.NewGuid();
			var url = $"edient:Command={UrlCommandForTest}&ControllerID=IncidentRequest&BusinessEntityPK={pk}";

			try
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, waitForAppToStart: false);
				Fail("Expected an exception");
			}
			catch (EnterpriseUrlHandlerException ex)
			{
				AssertEquals($"Record type is unknown to the current version of {Enterprise.Core.Constants.ProductName}. The Controller Name is IncidentRequest. ", ex.Message);
			}
		}

		#region UrlHandler Overrides

		public void TestCanHandle()
		{
			AssertEquals("No Command", false, UrlHandlerForTest.CanHandle(QueryString));

			QueryString["Command"] = "InvalidCommand";
			AssertEquals("Command=InvalidCommand", false, UrlHandlerForTest.CanHandle(QueryString));

			QueryString["Command"] = UrlCommandForTest;
			AssertEquals("Command=" + UrlCommandForTest, true, UrlHandlerForTest.CanHandle(QueryString));
		}

		#endregion

		#region HandleShowForm

		public void TestForEmailHyperlink()
		{
			Dummy.Factory.Save();
			AssertEquals("Form should not be shown initially for the test", false, IsDummyFormOpen);

			var url = UrlHandlerForTest.Create(DummyControllerIDs.Dummy, Dummy.PK);
			url = url.Replace("-", "%2D");
			AssertEquals("Should contain some url encoding for the test", true, url.Contains("%2D"));
			EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
			AssertEquals("Form should be opened for edit", true, IsDummyFormOpen);
		}

		public void TestForInternetExplorerHyperlink()
		{
			Dummy.Factory.Save();
			AssertEquals("Form should not be shown initially for the test", false, IsDummyFormOpen);

			var url = UrlHandlerForTest.Create(DummyControllerIDs.Dummy, Dummy.PK);
			url = WebUtility.UrlDecode(url); // IE does this before passing the url to the application!

			EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
			AssertEquals("Form should be opened for edit", true, IsDummyFormOpen);
		}

		public void TestShowEditForm_UsesLoadBusinessEntity()
		{
			var dependent = Dummy.Dependents.AddNew();
			Dummy.Factory.Save();

			var url = UrlHandlerForTest.Create(DummyControllerIDs.Dummy, dependent.PK);
			EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
			AssertEquals("Form opened, business entity comes from LoadBusinessEntity", true, IsDummyFormOpen);
			AssertEquals(ExpectedFormCaption, OpenDummyForm.Text);
		}

		protected abstract string ExpectedFormCaption { get; }

		public void TestInvalidUrlSecurityHash()
		{
			var url = UrlHandlerForTest.Create(DummyControllerIDs.Dummy, Dummy.PK);
			url = url.Replace(DummyControllerIDs.Dummy.Name, "FakeModule");

			try
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				Fail("Expected an exception");
			}
			catch (EnterpriseUrlHandlerException ex)
			{
				AssertEquals("Form should not be opened due to an invalid url", false, IsDummyFormOpen);
				AssertEquals($"This is not a valid {Enterprise.Core.Constants.ProductName} shortcut or hyperlink.", ex.Message);
			}
		}

		public void TestInvalidUrlSecurityHash_HashIsNotAValidBase64String()
		{
			var url = UrlHandlerForTest.Create(DummyControllerIDs.Dummy, Dummy.PK);
			url = url.Replace("Hash=", "Hash=Tampered");

			try
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				Fail("Expected an exception");
			}
			catch (EnterpriseUrlHandlerException ex)
			{
				AssertEquals("Form should not be opened due to an invalid url", false, IsDummyFormOpen);
				AssertEquals($"This is not a valid {Enterprise.Core.Constants.ProductName} shortcut or hyperlink.", ex.Message);
			}
		}

		public virtual void TestWhenRecordNotFound()
		{
			try
			{
				var url = UrlHandlerForTest.Create(DummyControllerIDs.Dummy, ZGuid.NewZGuid());
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				Fail("Expected an exception");
			}
			catch (EnterpriseUrlHandlerException ex)
			{
				AssertEquals($"The system has searched all open {Enterprise.Core.Constants.ProductName} programs and could not find the record", ex.Message);
			}
		}

		public void TestWhenControllerIDNotFoundButWithVersionInfomation()
		{
			try
			{
				var invalidControllerID = new ControllerID("DoesntExist");
				var url = UrlHandlerForTest.Create(invalidControllerID, ZGuid.NewZGuid());
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				Fail("Expected an exception");
			}
			catch (EnterpriseUrlHandlerException ex)
			{
				AssertEquals($"Record type is unknown to the current version of {Enterprise.Core.Constants.ProductName}. The Controller Name is DoesntExist. The Recommended Application version is " + VersionNumber, ex.Message);
			}
		}

		public virtual void TestWhenFormNotShownByController()
		{
			var handler = new TestShowFormUrlHandler(this);
			EnterpriseUrlHandlerService.RegisterUrlHandler(handler);
			EnterpriseUrlHandlerService.UnregisterUrlHandler(UrlHandlerForTest);

			var dummyNotInDatabase = this.Dummy;
			var url = UrlHandlerForTest.Create(DummyControllerIDs.Dummy, dummyNotInDatabase.PK);

			try
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				AssertEquals("Dummy form should not be shown for the test", null, OpenDummyForm);
			}
			finally
			{
				EnterpriseUrlHandlerService.UnregisterUrlHandler(handler);
				EnterpriseUrlHandlerService.RegisterUrlHandler(UrlHandlerForTest);
			}
		}

		public void TestWhenBusinessEntityPkIsInvalid()
		{
			var url = UrlHandlerForTest.Create(ControllerIDs.WorkItem, ZGuid.NewZGuid());
			var queryString = new QueryString();
			queryString.Deserialize(UrlHandler.GetQueryStringTextFromUrl(url));
			queryString["BusinessEntityPk"] = "RandomNonGuidCharsManuallyEntered";
			queryString["Hash"] = UrlHandlerForTest.CreateQueryStringSecurityHash(queryString);

			url = UrlHandler.GetUrlFromQueryString(queryString);
			AssertExceptionThrown(typeof(EnterpriseUrlHandlerException), $"This is not a valid {Enterprise.Core.Constants.ProductName} shortcut or hyperlink.", () =>
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
			});
		}

		public virtual void TestWhenLoggedIntoDifferenEnterpriseOrServerCode()
		{
			Dummy.Factory.Save();
			var url = UrlHandlerForTest.CreateWithSpecifiedLicenceCode(DummyControllerIDs.Dummy, Dummy.PK, "TSTEDITST");

			var handler = new TestShowFormUrlHandler(this);
			EnterpriseUrlHandlerService.RegisterUrlHandler(handler);
			EnterpriseUrlHandlerService.UnregisterUrlHandler(UrlHandlerForTest);

			handler.SetCurrentCompany((IGlbCompany)Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany")));

			try
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				Fail("Expected an exception");
			}
			catch (EnterpriseUrlHandlerException ex)
			{
				AssertEquals("Cannot process url for the licence key specified.", ex.Message);
			}
			finally
			{
				EnterpriseUrlHandlerService.UnregisterUrlHandler(handler);
				EnterpriseUrlHandlerService.RegisterUrlHandler(UrlHandlerForTest);
			}
		}

		public virtual void TestWhenLoggedIntoTheWrongCompany()
		{
			Dummy.Factory.Save();
			var url = UrlHandlerForTest.Create(DummyControllerIDs.Dummy, Dummy.PK);

			var handler = new TestShowFormUrlHandler(this);
			EnterpriseUrlHandlerService.RegisterUrlHandler(handler);
			EnterpriseUrlHandlerService.UnregisterUrlHandler(UrlHandlerForTest);

			handler.SetCurrentCompany(Factory.New<IGlbCompany>());
			try
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				Fail("Expected an exception");
			}
			catch (EnterpriseUrlHandlerException ex)
			{
				AssertEquals(
					$"{Enterprise.Core.Constants.ProductName} is not running in the correct company or from the correct licensed server installation directory.\r\n" +
					"Log into the 'Eagle Datamation International' company and try again.", ex.Message);
			}
			finally
			{
				EnterpriseUrlHandlerService.UnregisterUrlHandler(handler);
				EnterpriseUrlHandlerService.RegisterUrlHandler(UrlHandlerForTest);
			}
		}

		public virtual void TestWhenLoggedInCorrectly()
		{
			Dummy.Factory.Save();
			var url = UrlHandlerForTest.Create(DummyControllerIDs.Dummy, Dummy.PK);

			var handler = new TestShowFormUrlHandler(this);

			try
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				AssertEquals("Form opened", true, IsDummyFormOpen);
			}
			catch (EnterpriseUrlHandlerException)
			{
				Fail("Did not Expect an exception");
			}
		}

		public void TestWhenNoMainFormPresent()
		{
			var handler = new TestShowFormUrlHandler(this);
			EnterpriseUrlHandlerService.RegisterUrlHandler(handler);
			EnterpriseUrlHandlerService.UnregisterUrlHandler(UrlHandlerForTest);

			try
			{
				Dummy.Factory.Save();
				var url = handler.Create(DummyControllerIDs.Dummy, ZGuid.NewZGuid());
				handler.IsMainFormAvailable = false;
				AssertNoExceptionThrown(() => EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false));
			}
			finally
			{
				EnterpriseUrlHandlerService.UnregisterUrlHandler(handler);
				EnterpriseUrlHandlerService.RegisterUrlHandler(UrlHandlerForTest);
			}
		}

		public void TestShowForm_ShouldPassArgsToForm()
		{
			Dummy.Factory.Save();
			var args = new[] { "first", "second", "third" };
			var url = UrlHandlerForTest.Create(DummyControllerIDs.Dummy, Dummy.PK, args);
			EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
			AssertEquals("The form should be open, and yet...", true, IsDummyFormOpen);
			AssertEquals(ExpectedFormCaption, OpenDummyForm.Text);
			AssertArrayEqualsByElements("The opened form should have the args passed to it from the url, and yet...", args, ((ZDummyForm)OpenDummyForm).LoadedFormArgs.ToArray());
		}

		public void TestLoadBusinessEntityForPlugIn()
		{
			var dummyParent = Factory.New<DummyBusinessObject>();
			dummyParent.Z0_Description = "Parent";

			var dummyChild = Factory.New<DummyBusinessObject>();
			dummyChild.Z0_Guid = dummyParent.PK;
			dummyChild.Z0_Description = "Child";
			Factory.Save();

			var handler = new TestShowFormUrlHandlerWithErrorReport(this);
			var result = handler.GetCorrectControllerForTest(DummyControllerIDs.DummyForPlugIn, dummyParent.PK);
			AssertEquals(typeof(DummyControllerForPlugInTest), result.Item1.GetType());
			AssertEquals(dummyChild.PK, result.Item2.PK);
		}

		public string VersionNumber
		{
			get { return _versionNumber ?? (_versionNumber = new EnterpriseInformationRetriever().VersionNumber); }
		}
		string _versionNumber;

		public void TestLoadBusinessEntityForPlugInAndNavigation()
		{
			var dummyParent = Factory.New<DummyBusinessObject>();
			dummyParent.Z0_Description = "Parent";

			var dummyChild = Factory.New<DummyBusinessObject>();
			dummyChild.Z0_Guid = dummyParent.PK;
			dummyChild.Z0_Description = "Child";
			Factory.Save();

			var handler = new TestShowFormUrlHandlerWithErrorReport(this);
			handler.SetIsReportError(false);
			var result = handler.GetCorrectControllerForTest(DummyControllerIDs.DummyControllerWithPlugInAndNavigationProvider, dummyParent.PK);
			AssertEquals(typeof(DummyController), result.Item1.GetType());
			AssertEquals(dummyChild.PK, result.Item2.PK);
		}

		bool IsDummyFormOpen
		{
			get { return OpenDummyForm != null; }
		}

		[SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		Form OpenDummyForm
		{
			get
			{
				foreach (Form form in Application.OpenForms)
				{
					if (form is ZDummyForm)
					{
						return form;
					}
				}
				return null;
			}
		}

		#endregion

		#region Test Classes

		internal class TestShowFormUrlHandler : ShowFormUrlHandler
		{
			public TestShowFormUrlHandler(ShowFormUrlHandlerTestCase owner)
			{
				this.owner = owner;
			}

			protected override string ExpectedCommandText
			{
				get { return owner.UrlCommandForTest; }
			}

			public bool IsMainFormAvailable = true;

			public INavigationControllerIDProvider CorrectControllerForTest;

#if !WINZOR
			protected override Form LocateMainForm()
			{
				return IsMainFormAvailable ? new Form() : null;
			}
#endif

			public void SetCurrentCompany(IGlbCompany value)
			{
				currentCompany = value;
			}

			protected override IGlbCompany CurrentCompany
			{
				get { return currentCompany ?? base.CurrentCompany; }
			}
			IGlbCompany currentCompany;

			protected override BusinessObjectFactory NewFactory()
			{
				return owner.Factory;
			}

			protected override Form PerformFormAction(ControllerID controllerID, ZGuid pK, IEnumerable<string> args)
			{
				return null;
			}

			readonly ShowFormUrlHandlerTestCase owner;
		}

		internal class TestShowFormUrlHandlerWithErrorReport : TestShowFormUrlHandler
		{
			public TestShowFormUrlHandlerWithErrorReport(ShowFormUrlHandlerTestCase owner) : base(owner)
			{
			}

			public (ZController, BusinessObject) GetCorrectControllerForTest(ControllerID controllerID, ZGuid pk)
			{
				return ZControllerFactory.GetCorrectControllerAndBusinessObject(controllerID, pk, IsReportError);
			}

			protected override bool IsReportError
			{
				get { return isReportError; }
			}

			public void SetIsReportError(bool value)
			{
				isReportError = value;
			}
			internal bool isReportError = true;
		}

		#endregion

		#region Implementation

		readonly QueryString QueryString = new QueryString();

		DummyWithDependentsBusinessObject Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyWithDependentsBusinessObject>();
				}
				return dummy;
			}
		}
		DummyWithDependentsBusinessObject dummy;

		protected override void SetUp()
		{
			base.SetUp();

			var handlers = EnterpriseUrlHandlerService.UrlHandlers;
			foreach (var handler in handlers)
			{
				UnregisteredHandlers.Add(handler);
				EnterpriseUrlHandlerService.UnregisterUrlHandler(handler);
			}

			EnterpriseUrlHandlerService.RegisterUrlHandler(UrlHandlerForTest);
		}

		readonly List<UrlHandler> UnregisteredHandlers = new List<UrlHandler>();

		protected override void TearDown()
		{
			base.TearDown();

			EnterpriseUrlHandlerService.UnregisterUrlHandler(UrlHandlerForTest);

			foreach (var handler in UnregisteredHandlers)
			{
				EnterpriseUrlHandlerService.RegisterUrlHandler(handler);
			}

			if (OpenDummyForm != null)
			{
				OpenDummyForm.Dispose();
			}
		}

		#endregion
	}
}
