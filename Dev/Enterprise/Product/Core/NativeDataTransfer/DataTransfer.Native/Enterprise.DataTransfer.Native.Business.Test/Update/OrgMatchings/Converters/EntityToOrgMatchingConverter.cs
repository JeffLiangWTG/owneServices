using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.Converters
{
	public class EntityToOrgMatchingConverterTest : TransactionedTestCase
	{
		public void TestConvertProperties()
		{
			var factory = new BusinessObjectFactory();
			var converter = new EntityToOrgMatchingConverter(factory);

			var definition = TestUtil.FindEntityDefinition("Organization", "OrgHeader");
			var addrDef = TestUtil.FindEntityDefinition("Organization", "OrgHeader.OrgAddress");
			var cusCodeDef = TestUtil.FindEntityDefinition("Organization", "OrgHeader.OrgCusCode");

			var sessionServices = new AncillaryImportServices();
			var orgHeader = new Entity(definition, sessionServices);
			orgHeader["Code"] = "CODE";
			orgHeader["IsActive"] = true;
			orgHeader["FullName"] = "LEO LIANG";

			var orgAddress = new Entity(addrDef, sessionServices);
			orgHeader.ChildrenCollection.Add(orgAddress);

			var orgCusCode = new Entity(cusCodeDef, sessionServices);
			orgHeader.ChildrenCollection.Add(orgCusCode);

			var result = converter.Convert(orgHeader);

			AssertEquals("CODE", result.OH_Code);
			AssertEquals(true, result.OH_IsActive);
			AssertEquals("LEO LIANG", result.OH_FullName);

			AssertEquals(1, result.Addresses.Count());

			AssertEquals(1, result.CustomsCodes.Count);
		}
	}
}
