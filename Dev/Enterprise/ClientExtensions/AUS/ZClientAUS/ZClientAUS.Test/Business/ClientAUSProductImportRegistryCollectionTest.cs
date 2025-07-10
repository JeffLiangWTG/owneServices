using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.AUS.Business.Testing
{
	[TestedType(typeof(ClientAUSProductImportRegistryCollection))]
	public class ClientAUSProductImportRegistryCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ClientAUSProductImportRegistryCollection(Factory);
		}

		public void TestCollection()
		{
			OrgHeader importer = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader supplier1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			ClientAUSProductImportRegistry importRegistry1 = Factory.New<ClientAUSProductImportRegistry>();
			importRegistry1.T6_OH_Importer = importer.PK;
			importRegistry1.T6_OH_Supplier = supplier1.PK;
			importRegistry1.T6_Email = "test@testco.au";
			ClientAUSProductImportRegistry importRegistry2 = Factory.New<ClientAUSProductImportRegistry>();
			OrgHeader supplier2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C"));
			importRegistry2.T6_OH_Importer = importer.PK;
			importRegistry2.T6_OH_Supplier = supplier2.PK;
			importRegistry2.T6_Email = "jsmith@aaa.co.nz";
			importRegistry2.T6_DirectoryToStoreFiles = @"Test\Client";
			Factory.Save();
			ClientAUSProductImportRegistryCollection registryCollection = new ClientAUSProductImportRegistryCollection(Factory);
			registryCollection.Load();
			AssertEquals("Registry Collection has been created", 2, registryCollection.Count);
			AssertEquals("Registry list contains Entry 1", true, registryCollection.Contains(importRegistry1.PK));
			AssertEquals("Registry list contains Entry 2", true, registryCollection.Contains(importRegistry2.PK));
			ClientAUSProductImportRegistry result = Factory.Load<ClientAUSProductImportRegistry>(importRegistry2.PK);
			AssertEquals("jsmith@aaa.co.nz", result.T6_Email);
			AssertEquals(@"Test\Client", result.T6_DirectoryToStoreFiles);
		}
	}
}
