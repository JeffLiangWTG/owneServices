using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.AUS.Business.Testing
{
	[TestedType(typeof(ClientAUSOriginPreferenceMappingCollection))]
	public class ClientAUSOriginPreferenceMappingCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ClientAUSOriginPreferenceMappingCollection(Factory);
		}

		public void TestCollection()
		{
			OrgHeader importer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader supplier1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			ClientAUSOriginPreferenceMapping mapping1 = Factory.New<ClientAUSOriginPreferenceMapping>();
			mapping1.T7_OH_Importer = importer.PK;
			mapping1.T7_OH_Supplier = supplier1.PK;
			mapping1.T7_RN_NKOrigin = "AU";
			ClientAUSOriginPreferenceMapping mapping2 = Factory.New<ClientAUSOriginPreferenceMapping>();
			OrgHeader supplier2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C"));
			mapping2.T7_OH_Importer = importer.PK;
			mapping2.T7_OH_Supplier = supplier2.PK;
			mapping2.T7_RN_NKOrigin = "AU";
			mapping2.T7_RN_NKPreferenceOrigin = "NZ";
			Factory.Save();
			ClientAUSOriginPreferenceMappingCollection mappingCollection = new ClientAUSOriginPreferenceMappingCollection(Factory);
			mappingCollection.Load();
			AssertEquals("Mapping Collection has been created", 2, mappingCollection.Count);
			AssertEquals("Mapping list contains Entry 1", true, mappingCollection.Contains(mapping1.PK));
			AssertEquals("Mapping list contains Entry 2", true, mappingCollection.Contains(mapping2.PK));
			ClientAUSOriginPreferenceMapping result = Factory.Load<ClientAUSOriginPreferenceMapping>(mapping2.PK);
			AssertEquals("NZ", result.T7_RN_NKPreferenceOrigin);
		}
	}
}
