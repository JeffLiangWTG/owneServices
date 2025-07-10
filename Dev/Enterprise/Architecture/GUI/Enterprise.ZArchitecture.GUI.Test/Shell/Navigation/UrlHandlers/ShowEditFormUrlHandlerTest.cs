using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	class ShowEditFormUrlHandlerTest : ShowFormUrlHandlerTestCase
	{
		public void TestCreate_ControllerIDProvider()
		{
			using var resetRegistry = RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var serverNameAndDatabaseName = $"&ServerName={InstanceDetails.Current.ServerName}&DatabaseName={InstanceDetails.Current.DatabaseName}";
			var dummy1 = Factory.New<DummyWithControllerIDProvider>();
			var row = ((IBusinessObjectInternals)dummy1).Row;
			row[DummyWithControllerIDProvider.Schema.PK] = new Guid("f94a7007-a0aa-4d85-91df-dfd334d8d8c5");
			var dummy = new DummyWithControllerIDProvider(Factory, row);
			var creator = (IShowEditFormUrlCreator)UrlHandlerForTest;
			AssertEquals(
				"If the result of Create changes, the shortcuts for existing clients will break",
				"edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=Dummy&BusinessEntityPK=f94a7007-a0aa-4d85-91df-dfd334d8d8c5&VersionNumber=" + VersionNumber + serverNameAndDatabaseName + "&Hash=%2bntxIPnnplvyQ34FRpCfcVz8dmozZH3Bw",
				creator.Create(dummy));

			dummy.ControllerIDForTesting = DummyControllerIDs.Dummy4;
			AssertEquals(
				"The 'LicenceCode' parameter should not be embedded into the url if the MakeUrlsOnlyOpenableForCurrentCompany is set to false",
				"edient:Command=ShowEditForm&ControllerID=Dummy4&BusinessEntityPK=f94a7007-a0aa-4d85-91df-dfd334d8d8c5&VersionNumber=" + VersionNumber + serverNameAndDatabaseName + "&Hash=%2bLKvM0Me5XOfZxt0ID3RzeeqZXhL8MLlT",
				creator.Create(dummy));

			dummy.PKForTesting = Guid.Empty;
			AssertNull("Don't create URL if PK is empty", creator.Create(dummy));
		}

		public void TestCreate_ControllerIDProvider_Default()
		{
			using var resetRegistry = RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.Instance.AddDatabaseInfoToEdientUrls.DefaultValue);

			var dummy1 = Factory.New<DummyWithControllerIDProvider>();
			var row = ((IBusinessObjectInternals)dummy1).Row;
			row[DummyWithControllerIDProvider.Schema.PK] = new Guid("f94a7007-a0aa-4d85-91df-dfd334d8d8c5");
			var dummy = new DummyWithControllerIDProvider(Factory, row);
			var creator = (IShowEditFormUrlCreator)UrlHandlerForTest;
			AssertEquals(
				"If the result of Create changes, the shortcuts for existing clients will break",
				"edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=Dummy&BusinessEntityPK=f94a7007-a0aa-4d85-91df-dfd334d8d8c5&VersionNumber=" + VersionNumber + "&Hash=%2bntxIPnnplvyQ34FRpCfcVz8dmozZH3Bw",
				creator.Create(dummy));

			dummy.ControllerIDForTesting = DummyControllerIDs.Dummy4;
			AssertEquals(
				"The 'LicenceCode' parameter should not be embedded into the url if the MakeUrlsOnlyOpenableForCurrentCompany is set to false",
				"edient:Command=ShowEditForm&ControllerID=Dummy4&BusinessEntityPK=f94a7007-a0aa-4d85-91df-dfd334d8d8c5&VersionNumber=" + VersionNumber + "&Hash=%2bLKvM0Me5XOfZxt0ID3RzeeqZXhL8MLlT",
				creator.Create(dummy));

			dummy.PKForTesting = Guid.Empty;
			AssertNull("Don't create URL if PK is empty", creator.Create(dummy));
		}

		class DummyWithControllerIDProvider : DummyBusinessObject, IControllerIDProvider
		{
			public DummyWithControllerIDProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				PKForTesting = PK.ToGuid();
			}

			#region IControllerIDProvider Members

			ControllerID IControllerIDProvider.ControllerID
			{
				get { return ControllerIDForTesting ?? DummyControllerIDs.Dummy; }
			}
			public ControllerID ControllerIDForTesting;

			Guid IControllerIDProvider.BusinessObjectPK
			{
				get { return PKForTesting; }
			}
			public Guid PKForTesting;

			#endregion
		}

		protected override string UrlCommandForTest
		{
			get { return "ShowEditForm"; }
		}

		protected override ShowFormUrlHandler UrlHandlerForTest
		{
			get { return ShowEditFormUrlHandler.Instance; }
		}

		protected override string ExpectedFormCaption
		{
			get { return "Edit ZDummyForm"; }
		}
	}
}
