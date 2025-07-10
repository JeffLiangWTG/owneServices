using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings.Converters
{
	public class EntityToOrgCusCodeMatchingConverterTest : TransactionedTestCase
	{
		public void TestConvert()
		{
			var converter = new EntityToOrgCusCodeMatchingConverter();

			var cusCodeDefinition = TestUtil.FindEntityDefinition("Organization", "OrgHeader.OrgCusCode");
			var addrDefinition = TestUtil.FindEntityDefinition("Organization", "OrgHeader.OrgCusCode.PremisesAddress");
			var sessionServices = new AncillaryImportServices();
			var cusCode = new Entity(cusCodeDefinition, sessionServices);
			var premisesAddr = new Entity(addrDefinition, sessionServices);

			cusCode["CustomsRegNo"] = "RegNo";
			cusCode["CodeType"] = "CodeType";

			cusCode.ParentCollection.Add(premisesAddr);

			var result = new OrgCusCodeForMatching();

			converter.Convert(result, cusCode);

			AssertEquals("RegNo", result.OK_CustomsRegNo);
			AssertEquals("CodeType", result.OK_CodeType);
		}
	}
}
