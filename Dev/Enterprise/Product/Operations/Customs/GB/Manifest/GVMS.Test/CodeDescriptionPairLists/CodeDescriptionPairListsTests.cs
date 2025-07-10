using NUnit.Framework;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	public class CodeDescriptionPairListsTests : TestCase
	{
		public void TestGVMSCustomsStatus()
		{
			var codes = new GVMSCustomsStatus().GetAllCodes();
			AssertContainsExactElementsInAnyOrder(new string[] { "NOT", "OPN", "NOF", "CKD", "DEP", "CAN", "FIN", "COM" }, codes);
		}
	}
}
