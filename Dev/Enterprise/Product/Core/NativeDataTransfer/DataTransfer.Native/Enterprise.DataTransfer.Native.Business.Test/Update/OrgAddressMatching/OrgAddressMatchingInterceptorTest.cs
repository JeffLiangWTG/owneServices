using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgAddressMatching
{
	public class OrgAddressMatchingInterceptorTest : TransactionedTestCase
	{
		public void TestMatchOrgAddressBySingleOrgCusCode()
		{
			var factory = new BusinessObjectFactory();
			var manufacturer = factory.New<OrgHeader>();
			manufacturer.OH_Code = "ORG";
			var address = manufacturer.Addresses.AddNew();
			address.OA_Code = "184 Test Street";
			address.Address1 = "184 Test Street";
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "003008787159", Core.Constants.CountryCodes.UnitedStates);
			factory.Save();

			var sessionServices = new AncillaryImportServices();
			var updateSetting = SetupSetting(sessionServices);
			var handler = new OrgAddressMatchingInterceptor(updateSetting, sessionServices) { Function = DummyMethod };

			var productEntitySet = PrepareProductEntitySet();
			var root = productEntitySet.Root;
			handler.Invoke(productEntitySet);

			var addressEntity = root.Children.FirstOrDefault(c => c.Definition.EntityName == "CusClassPartPivot")
															.Children.FirstOrDefault(c => c.Definition.EntityName == "CusUSClassification")
															.Children.FirstOrDefault(c => c.Definition.EntityName == "Manufacturer");
			AssertEquals("Matching OrgAddress by the single OrgCusCode", "184 Test Street", addressEntity["Code"]);
			Assert("All of the OrgCusCode entity should be removed", !addressEntity.ChildrenCollection.Any(x => x.EntityName == "OrgCusCode"));
		}

		protected EntitySet PrepareProductEntitySet()
		{
			var productDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart");
			var sessionServices = new AncillaryImportServices();
			var product = new Entity(productDefinition, sessionServices);
			product["PartNum"] = "PART";
			var pivotDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.CusClassPartPivot");
			var pivot = new Entity(pivotDefinition, sessionServices);
			pivot["ChildType"] = "HTI";
			pivot["TariffNum"] = "10101010";
			product.ChildrenCollection.Add(pivot);
			var classificationDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.CusClassPartPivot.CusUSClassification");
			var classification = new Entity(classificationDefinition, sessionServices);
			pivot.ChildrenCollection.Add(classification);
			var addressDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.CusClassPartPivot.CusUSClassification.Manufacturer");
			var address = new Entity(addressDefinition, sessionServices);
			classification.ChildrenCollection.Add(address);
			var orgCusCodeDefinition = addressDefinition.Children.First(x => x.EntityName == "OrgCusCode");
			var orgCusCode = new Entity(orgCusCodeDefinition, sessionServices);
			orgCusCode["CustomsRegNo"] = "003008787159";
			orgCusCode["CodeType"] = "MID";
			var orgCusCodeCountry = new Entity(orgCusCodeDefinition.Parents.First(x => x.EntityName == "CodeCountry"), sessionServices);
			orgCusCodeCountry["Code"] = "US";
			orgCusCode.ParentCollection.Add(orgCusCodeCountry);
			address.ChildrenCollection.Add(orgCusCode);

			return new EntitySet("Product") { Root = product };
		}

		static OrgAddressMatchingSetting SetupSetting(AncillaryImportServices sessionServices)
		{
			var result = new OrgAddressMatchingSetting();
			var context = new EntityContext(sessionServices, new FactoryProvider());
			result.Context = context;
			return result;
		}

		static void DummyMethod(IEntitySet entitySet)
		{
		}
	}
}
