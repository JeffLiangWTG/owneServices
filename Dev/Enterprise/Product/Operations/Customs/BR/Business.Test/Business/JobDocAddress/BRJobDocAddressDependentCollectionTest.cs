using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(BRJobDocAddressDependentCollection))]
	class BRJobDocAddressDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new BRJobDocAddressDependentCollection(Factory.New<JobDeclaration>());

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<JobDocAddress>();

		public void TestClearanceLocalInvolvedParty_UpdateGeoLocation()
		{
			var orgAdress = Factory.NewWithValidTestData<OrgAddress>();
			orgAdress.OA_GeoLocation = ZGeography.CreatePoint(10, 20);

			var orgAdress2 = Factory.NewWithValidTestData<OrgAddress>();
			orgAdress2.OA_GeoLocation = ZGeography.CreatePoint(20, 40);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			var docAddresses = new BRJobDocAddressDependentCollection(declaration);
			var docAddress = docAddresses.AddNew(DocAddressType.ClearanceLocalInvolvedParty);
			docAddress.E2_OA_Address = orgAdress.PK;
			AssertNotNull("E2_GeoLocation", docAddress.E2_GeoLocation);
			AssertEquals("E2_Longitude", new ZDecimal(10), docAddress.E2_Longitude);
			AssertEquals("E2_Latitude", new ZDecimal(20), docAddress.E2_Latitude);

			docAddress.E2_AddressOverride = true;
			AssertNotNull("E2_GeoLocation", docAddress.E2_GeoLocation);
			AssertEquals("E2_Longitude", new ZDecimal(10), docAddress.E2_Longitude);
			AssertEquals("E2_Latitude", new ZDecimal(20), docAddress.E2_Latitude);

			Factory.Save();

			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			docAddresses = new BRJobDocAddressDependentCollection(declaration);
			docAddresses.Load();

			docAddress = docAddresses.FindByDocAddressType(DocAddressType.ClearanceLocalInvolvedParty);
			AssertNotNull("E2_GeoLocation", docAddress.E2_GeoLocation);
			AssertEquals("E2_Longitude", new ZDecimal(10), docAddress.E2_Longitude);
			AssertEquals("E2_Latitude", new ZDecimal(20), docAddress.E2_Latitude);
			AssertEquals("E2_AddressOverride", true, docAddress.E2_AddressOverride);

			docAddress.E2_AddressOverride = false;
			docAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("E2_Longitude", new ZDecimal(0), docAddress.E2_Longitude);
			AssertEquals("E2_Latitude", new ZDecimal(0), docAddress.E2_Latitude);

			docAddress.E2_OA_Address = orgAdress2.PK;
			docAddress.E2_AddressOverride = true;

			AssertNotNull("E2_GeoLocation", docAddress.E2_GeoLocation);
			AssertEquals("E2_Longitude", new ZDecimal(20), docAddress.E2_Longitude);
			AssertEquals("E2_Latitude", new ZDecimal(40), docAddress.E2_Latitude);
		}

		public void TestNonClearanceLocalInvolvedParty()
		{
			var orgAdress = Factory.New<OrgAddress>();
			orgAdress.OA_GeoLocation = ZGeography.CreatePoint(10, 20);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			var docAddress = declaration.DocAddresses.AddNew(DocAddressType.BoardingLocalDocumentaryAddress);
			docAddress.E2_OA_Address = orgAdress.PK;
			AssertNotNull("E2_GeoLocation", docAddress.E2_GeoLocation);
			AssertEquals("E2_Longitude", new ZDecimal(10), docAddress.E2_Longitude);
			AssertEquals("E2_Latitude", new ZDecimal(20), docAddress.E2_Latitude);

			docAddress.E2_AddressOverride = true;
			AssertEquals("E2_Longitude", new ZDecimal(0), docAddress.E2_Longitude);
			AssertEquals("E2_Latitude", new ZDecimal(0), docAddress.E2_Latitude);
		}
	}
}
