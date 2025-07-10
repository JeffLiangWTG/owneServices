using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	class ShowNewFormUrlHandlerTest : ShowFormUrlHandlerTestCase
	{
		public override void TestCreate()
		{
			using var resetRegistry = RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var serverNameAndDatabaseName = $"&ServerName={InstanceDetails.Current.ServerName}&DatabaseName={InstanceDetails.Current.DatabaseName}";
			AssertEquals(
				"If the result of Create changes, the shortcuts for existing clients will break",
				$"edient:Command=ShowNewForm&LicenceCode=EDIEDIDAT&ControllerID=Dummy&BusinessEntityPK=00000000-0000-0000-0000-000000000000&VersionNumber=" + VersionNumber + serverNameAndDatabaseName + "&Hash=%2b6D7hM0HWsyH8H%2bZIeKYXSUEJ%2bRRLCA%2f4",
				UrlHandlerForTest.Create(DummyControllerIDs.Dummy, ZGuid.Empty));

			AssertEquals(
				"The 'LicenceCode' parameter should not be embedded into the url if the MakeUrlsOnlyOpenableForCurrentCompany is set to false",
				$"edient:Command=ShowNewForm&ControllerID=Dummy4&BusinessEntityPK=00000000-0000-0000-0000-000000000000&VersionNumber=" + VersionNumber + serverNameAndDatabaseName + "&Hash=%2bSJ7hxD6ee4yGsVmtAefHpZDLRSdT84LE",
				UrlHandlerForTest.Create(DummyControllerIDs.Dummy4, ZGuid.Empty));
		}

		public override void TestCreate_Default()
		{
			using var resetRegistry = RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.DefaultValue);

			AssertEquals(
				"If the result of Create changes, the shortcuts for existing clients will break",
				$"edient:Command=ShowNewForm&LicenceCode=EDIEDIDAT&ControllerID=Dummy&BusinessEntityPK=00000000-0000-0000-0000-000000000000&VersionNumber=" + VersionNumber + "&Hash=%2b6D7hM0HWsyH8H%2bZIeKYXSUEJ%2bRRLCA%2f4",
				UrlHandlerForTest.Create(DummyControllerIDs.Dummy, ZGuid.Empty));

			AssertEquals(
				"The 'LicenceCode' parameter should not be embedded into the url if the MakeUrlsOnlyOpenableForCurrentCompany is set to false",
				$"edient:Command=ShowNewForm&ControllerID=Dummy4&BusinessEntityPK=00000000-0000-0000-0000-000000000000&VersionNumber=" + VersionNumber + "&Hash=%2bSJ7hxD6ee4yGsVmtAefHpZDLRSdT84LE",
				UrlHandlerForTest.Create(DummyControllerIDs.Dummy4, ZGuid.Empty));
		}

		public void TestHandle()
		{
			var url = UrlHandlerForTest.Create(DummyControllerIDs.Dummy, ZGuid.Empty);
			var queryString = new QueryString();
			queryString.Deserialize(UrlHandler.GetQueryStringTextFromUrl(url));

			ZForm lastFormCreated = null;
			EventHandler formCreatedHandler = (sender, e) =>
			{
				lastFormCreated = (ZForm)sender;
			};
			ZForm.FormCreated += formCreatedHandler;

			try
			{
				var handled = UrlHandlerForTest.Handle(queryString);

				using (lastFormCreated)
				{
					AssertEquals("Should have been handled", true, handled);
					AssertNotNull("Should have shown form", lastFormCreated);
					AssertEquals(DummyControllerIDs.Dummy, lastFormCreated.ControllerID);
				}
			}
			finally
			{
				ZForm.FormCreated -= formCreatedHandler;
			}
		}

		public void TestHandle_WithoutBusinessEntityPk()
		{
			var url = "edient:Command=ShowNewForm&LicenceCode=EDIEDIDAT&ControllerID=Dummy";
			var queryString = new QueryString();
			queryString.Deserialize(UrlHandler.GetQueryStringTextFromUrl(url));

			ZForm lastFormCreated = null;
			EventHandler formCreatedHandler = (sender, e) =>
			{
				lastFormCreated = (ZForm)sender;
			};
			ZForm.FormCreated += formCreatedHandler;

			try
			{
				var handled = UrlHandlerForTest.Handle(queryString);

				using (lastFormCreated)
				{
					AssertEquals("Should have been handled", true, handled);
					AssertNotNull("Should have shown form", lastFormCreated);
					AssertEquals(DummyControllerIDs.Dummy, lastFormCreated.ControllerID);
				}
			}
			finally
			{
				ZForm.FormCreated -= formCreatedHandler;
			}
		}

		protected override string UrlCommandForTest
		{
			get { return "ShowNewForm"; }
		}

		protected override ShowFormUrlHandler UrlHandlerForTest
		{
			get { return ShowNewFormUrlHandler.Instance; }
		}

		protected override string ExpectedFormCaption
		{
			get { return "New ZDummyForm"; }
		}

		protected override string ExpectedHash => "%2b6D7hM0HWsyH8H%2bZIeKYXSUEJ%2bRRLCA%2f4";

		public override void TestWhenRecordNotFound()
		{
			Assert("Does not apply for new entities", true);
		}

		public override void TestWhenFormNotShownByController()
		{
			Assert("Does not apply for new entities", true);
		}

		public override void TestWhenLoggedIntoDifferenEnterpriseOrServerCode()
		{
			Assert("Does not apply for new entities", true);
		}

		public override void TestWhenLoggedIntoTheWrongCompany()
		{
			Assert("Does not apply for new entities", true);
		}

		public override void TestWhenLoggedInCorrectly()
		{
			Assert("Does not apply for new entities", true);
		}
	}
}
