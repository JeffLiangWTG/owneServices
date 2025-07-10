using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ApplicationIdentifierCollection))]
	sealed class ApplicationIdentifierCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ApplicationIdentifierCollection>
	{
		public void TestGetDefault()
		{
			ApplicationIdentifierCollection defaultValue = ApplicationIdentifierCollection.GetDefault();
			AssertEquals(83, defaultValue.Count);

			ApplicationIdentifier appID1 = defaultValue[0];
			AssertEquals("00", appID1.ApplicationID);
			AssertEquals("Serial Shipping Container Code", appID1.FullTitle);
			AssertEquals("SSCC", appID1.DataTitle);
			AssertEquals(DataTypeCodeList.Codes.Digits, appID1.DataType);
			AssertEquals(18, appID1.MinFieldLength);
			AssertEquals(18, appID1.MaxFieldLength);

			ApplicationIdentifier appID2 = defaultValue[82];
			AssertEquals("99", appID2.ApplicationID);
			AssertEquals("Company Internal Information", appID2.FullTitle);
			AssertEquals("INTERNAL", appID2.DataTitle);
			AssertEquals(DataTypeCodeList.Codes.Alphanumeric, appID2.DataType);
			AssertEquals(0, appID2.MinFieldLength);
			AssertEquals(30, appID2.MaxFieldLength);
		}

		public void TestAllowNew()
		{
			AssertEquals("Must allow new rows", true, this.Collection.AllowNew);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override ApplicationIdentifierCollection GetCollectionToTest()
		{
			return new ApplicationIdentifierCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ApplicationIdentifier();
		}

		#endregion
	}
}
