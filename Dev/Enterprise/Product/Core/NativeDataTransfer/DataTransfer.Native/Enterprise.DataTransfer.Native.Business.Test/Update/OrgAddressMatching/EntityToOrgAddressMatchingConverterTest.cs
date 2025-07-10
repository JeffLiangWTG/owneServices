using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgAddressMatching.Converters
{
	public class EntityToOrgHeaderMatchingConverterTest : TransactionedTestCase
	{
		public void TestConvert()
		{
			var factory = new BusinessObjectFactory();
			var converter = new EntityToOrgHeaderMatchingConverter(factory);

			var addressDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.CusClassPartPivot.CusUSClassification.Manufacturer");
			var orgHeaderDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.CusClassPartPivot.CusUSClassification.Manufacturer.OrgHeader");
			var orgCusCodeDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.CusClassPartPivot.CusUSClassification.Manufacturer.OrgCusCode");
			var codeCountryDefinition = TestUtil.FindEntityDefinition("Product", "OrgSupplierPart.CusClassPartPivot.CusUSClassification.Manufacturer.OrgCusCode.CodeCountry");
			var sessionServices = new AncillaryImportServices();
			var address = new Entity(addressDefinition, sessionServices);
			address["Code"] = "ADDRESS";

			var orgHeader = new Entity(orgHeaderDefinition, sessionServices);
			orgHeader["Code"] = "ORG";
			address.ParentCollection.Add(orgHeader);

			var orgCusCode = new Entity(orgCusCodeDefinition, sessionServices);
			orgCusCode["CustomsRegNo"] = "REGNO";
			orgCusCode["CodeType"] = "MID";
			address.ChildrenCollection.Add(orgCusCode);

			var codeCountry = new Entity(codeCountryDefinition, sessionServices);
			codeCountry["Code"] = "US";
			orgCusCode.ParentCollection.Add(codeCountry);

			var result = converter.Convert(address);

			AssertEquals("ORG", result.OH_Code);
			AssertEquals(1, result.Addresses.Count());
			AssertEquals("ADDRESS", result.Addresses.First().OA_Code);
			AssertEquals(1, result.CustomsCodes.Count);
			AssertEquals("REGNO", result.CustomsCodes.First().OK_CustomsRegNo);
			AssertEquals("MID", result.CustomsCodes.First().OK_CodeType);
			AssertEquals("US", result.CustomsCodes.First().OK_RN_NKCodeCountry);
		}
	}
}
