using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(DeclarationJobDocAddressCollection))]
	sealed class DeclarationJobDocAddressCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadWithRelationshipfilter()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CA1";
			company.GC_RN_NKCountryCode = "CA";
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "GB1";
			branch.GB_GC = company.PK;
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = "IMP";
			var invoice1 = declaration1.Invoices.AddNew();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = "LVX";
			declaration2.JE_GB = branch.PK;
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = "IMP";
			declaration3.JE_GB = branch.PK;

			var jobDocAddress1 = Factory.New<JobDocAddress>();
			jobDocAddress1.E2_ParentTableCode = "JE";
			jobDocAddress1.E2_ParentID = declaration1.PK;
			jobDocAddress1.E2_Email = "t@a.com";

			var jobDocAddress2 = Factory.New<JobDocAddress>();
			jobDocAddress2.E2_ParentTableCode = "JE";
			jobDocAddress2.E2_ParentID = declaration2.PK;
			jobDocAddress2.E2_Email = "t@a.com";

			var jobDocAddress3 = Factory.New<JobDocAddress>();
			jobDocAddress3.E2_ParentTableCode = "JE";
			jobDocAddress3.E2_ParentID = declaration3.PK;
			jobDocAddress3.E2_Email = "t@a.com";

			var jobDocAddress4 = Factory.New<JobDocAddress>();
			jobDocAddress4.E2_ParentTableCode = "JZ";
			jobDocAddress4.E2_ParentID = invoice1.PK;
			jobDocAddress4.E2_Email = "t@a.com";

			Factory.Save();

			var collection = new DeclarationJobDocAddressCollection(Factory);
			collection.Load();
			var result = collection.Cast<JobDocAddress>().Where(o => o.E2_Email == "t@a.com").ToList();
			AssertEquals("count", 0, result.Count);
		}

		public void TestSetFilterBusinessObjectDefaults()
		{
			var filterDefaults = new DeclarationJobDocAddressCollection(Factory).FilterBusinessObjectDefaults;
			AssertEquals("Count", 2, filterDefaults.Cast<FilterBusinessObjectDefault>().Count());
			var filterDefault = filterDefaults[JobDocAddressesFilterBusinessObject.Schema.AddressDescription + ":Property"];
			AssertEquals("Value", DocAddressTypes.Codes.ImporterPickupDeliveryAddress, filterDefault.Value);
			filterDefault = filterDefaults[JobDocAddressesFilterBusinessObject.Schema.Organization + ":Property"];
			AssertEquals("Value", ZGuid.Empty, filterDefault.Value);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new DeclarationJobDocAddressCollection(Factory);
	}
}
