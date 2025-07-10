using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common
{
	class EntitySetBuilderTest : TransactionedTestCase
	{
		public void TestBuildEntitySet()
		{
			var orgHeader = SetupOrgData();
			var pk = orgHeader.PK;
			var rowFactory = new RowFactory();
			var row = rowFactory.LoadFromPK("OrgHeader", pk);

			var definition = TestUtil.FindEntityDefinition("Organization", "OrgHeader");
			var builder = new EntitySetBuilder();
			var entity = builder.BuildEntitySet(row, definition, new AncillaryImportServices());

			AssertEquals(orgHeader.Addresses.Count, entity.Children.Count(c => c.Definition.EntityName == "OrgAddress"));
			var addressEntities = entity.Children.Where(c => c.Definition.EntityName == "OrgAddress");
			foreach (var addressEntity in addressEntities)
			{
				var capabilities = addressEntity.Children.Where(c => c.Definition.EntityName == "OrgAddressCapability");
				Assert(capabilities.Any());
			}

			AssertEquals(orgHeader.Contacts.Count, entity.Children.Count(c => c.Definition.EntityName == "OrgContact"));

			Assert(entity.Children.Any(c => c.Definition.EntityName == "OrgCompanyData"));
			var orgCompanyData = entity.Children.First(c => c.Definition.EntityName == "OrgCompanyData");
			Assert(entity.Children.Any(c => c.Definition.EntityName == "OrgRateTariffLevel"));
			var orgRateTariffLevel = entity.Children.First(c => c.Definition.EntityName == "OrgRateTariffLevel");

			Assert(orgCompanyData.Parents.Any(c => c.Definition.EntityName == "GlbCompany"));
			Assert(orgRateTariffLevel.Parents.Any(c => c.Definition.EntityName == "GlbCompany"));
		}

		public void TestMultipleKeys()
		{
			var orgHeader = SetupOrgData();
			var pk = orgHeader.PK;
			var rowFactory = new RowFactory();
			var row = rowFactory.LoadFromPK("OrgHeader", pk);

			var definition = TestUtil.FindEntityDefinition("Organization", "OrgHeader");
			var builder = new EntitySetBuilder();
			var entity = builder.BuildEntitySet(row, definition, new AncillaryImportServices());

			var definitions = definition.PropertyDefinitions;
			var property = new Property(definitions.First());

			var (success, error) = entity.AddProperty(property);
			AssertEquals(false, success);
			AssertEquals($"Duplicate {property} added to Entity: {entity.GetDictionaryContents()}", error);
		}

		public void TestSettingCorrectOrgAddressForOrgCusCode()
		{
			var product = SetupProductData();

			var pk = product.PK;
			var rowFactory = new RowFactory();
			var row = rowFactory.LoadFromPK("OrgSupplierPart", pk);

			var definition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart");
			var builder = new EntitySetBuilder();
			var entity = builder.BuildEntitySet(row, definition, new AncillaryImportServices());

			var manufacturers = new List<IEntity>();

			foreach (var classification in entity.Children.Where(part => part.Definition.EntityName == "CusClassPartPivot")
				.SelectMany(pivot => pivot.ChildrenCollection.Where(c => c.Definition.EntityName == "CusUSClassification")))
			{
				var manufacturer = classification.Parents.FirstOrDefault(c => c.Definition.EntityName == "Manufacturer");
				var orgCusCode = manufacturer.Children.FirstOrDefault(e => e.EntityName == "OrgCusCode");
				AssertEquals("OrgCusCode should only have one Manufacturer as its parent", 1, orgCusCode.Parents.Count(p => p.EntityName == "Manufacturer"));
				manufacturers.Add(manufacturer);
			}

			AssertEquals("Two Manufacturers should be found", 2, manufacturers.Count);
		}

		OrgHeader SetupOrgData()
		{
			var factory = new BusinessObjectFactory();

			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = "DUMMYCORP";
			orgHeader.OH_FullName = "DUMMY CORP";
			orgHeader.OH_IsActive = true;
			orgHeader.OH_IsConsignee = true;

			orgHeader.Addresses.RemoveAll();

			var address = orgHeader.Addresses.AddNew();
			address.OA_Code = "Main Address";
			address.OA_Address1 = "Fly Street";
			address.OA_City = "SYDNEY";
			address.OA_Email = "Jone.Don@cargowise.com";
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);

			var address2 = orgHeader.Addresses.AddNew();
			address2.OA_Code = "Second Address";
			address2.OA_Address1 = "Ship Street";
			address2.OA_City = "Melbourn";
			address2.OA_Email = "Jone.Don@cargowise.com";
			address2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "Jone.Don";
			contact.OC_Phone = "+6182308001";

			orgHeader.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			factory.Save();
			return orgHeader;
		}

		OrgSupplierPart SetupProductData()
		{
			var factory = new BusinessObjectFactory(TestUtil.Connection);

			var manufacturer = factory.New<OrgHeader>();
			manufacturer.OH_Code = "ORG";
			manufacturer.MainAddress.OA_Code = "184 Test Street";
			manufacturer.MainAddress.Address1 = "184 Test Street";
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "003008787159", Core.Constants.CountryCodes.UnitedStates);

			var part = factory.New<Integration.Customs.US.IOrgSupplierPart>() as OrgSupplierPart;
			part.OP_PartNum = "PART";
			part.OP_Desc = "PART Value";

			var ou = part.RelatedOrganisations.AddNew();
			ou.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			ou.OU_OH = manufacturer.PK;

			var pivot1 = factory.New<Integration.Customs.US.ICusClassPartPivot>() as BusinessObject;
			pivot1["CI_OP"] = part.PK;
			pivot1["CI_ChildType"] = "HTI";
			pivot1["CI_RN_NKCountry"] = "US";
			pivot1["CI_TariffNum"] = "0101";
			pivot1["CD_OA_Manufacturer"] = manufacturer.MainAddress.PK;
			var pivot2 = factory.New<Integration.Customs.US.ICusClassPartPivot>() as BusinessObject;
			pivot2["CI_OP"] = part.PK;
			pivot2["CI_OH"] = manufacturer.PK;
			pivot2["CI_ChildType"] = "HTI";
			pivot2["CI_RN_NKCountry"] = "US";
			pivot2["CI_TariffNum"] = "0202";
			pivot2["CD_OA_Manufacturer"] = manufacturer.MainAddress.PK;

			factory.Save();

			return part;
		}
	}
}
