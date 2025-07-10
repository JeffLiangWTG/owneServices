using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.Converters
{
	public class EntityToOrgBrandMatchingConverterTest : TransactionedTestCase
	{
		public void TestConvert()
		{
			var converter = new EntityToOrgBrandMatchingConverter();

			var cusCodeDefinition = TestUtil.FindEntityDefinition("Organization", "OrgHeader.OrgBrandOrRelatedName");
			var sessionServices = new AncillaryImportServices() ;
			var cusCode = new Entity(cusCodeDefinition, sessionServices);

			cusCode["RelatedName"] = "RegNo";

			var result = new OrgBrandOrRelatedNameForMatching();

			converter.Convert(result, cusCode);

			AssertEquals("RegNo", result.P1_RelatedName);
		}
	}
}
