using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.AUS.Business.Testing
{
	[TestedType(typeof(ClientAUSOriginPreferenceMapping))]
	public class ClientAUSOriginPreferenceMappingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOriginPreferenceMapping()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			ZString testOrigin = "FJ";
			ClientAUSOriginPreferenceMapping testMapping = Factory.LoadTop1<ClientAUSOriginPreferenceMapping>(new ZQuery());
			AssertEquals("Importer/Supplier/Origin mapping should not exist", true, (testMapping == null));
			ClientAUSOriginPreferenceMapping testMappingData = Factory.NewWithValidTestData<ClientAUSOriginPreferenceMapping>();
			testMappingData.T7_OH_Importer = testImporter.PK;
			testMappingData.T7_OH_Supplier = testSupplier.PK;
			testMappingData.T7_RN_NKOrigin = testOrigin;
			Factory.Save();
			testMapping = Factory.LoadTop1<ClientAUSOriginPreferenceMapping>(new ZQuery());
			AssertEquals("Importer/Supplier/Origin mapping should now exist", true, (testMapping != null));
			AssertEquals(testOrigin, testMapping.T7_RN_NKOrigin);
		}
	}
}
