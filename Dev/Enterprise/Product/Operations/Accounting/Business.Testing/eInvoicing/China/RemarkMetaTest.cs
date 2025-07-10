using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.eInvoicing.China;

namespace Enterprise.Accounting.Business.EInvoicing.China.Testing
{
	public class RemarkMetaTest : TestCaseWithFactory
	{
		public void TestInitializeRemarkMeta()
		{
			var expectedOriginalCaptionAndValue = "a";
			var expectedOriginalValue = "b";
			var expectedHasNoMacro = true;
			var remarkMeta = new RemarkMeta(expectedOriginalCaptionAndValue, expectedOriginalValue, expectedHasNoMacro);

			AssertEquals(expectedOriginalCaptionAndValue, remarkMeta.OriginalCaptionAndValue);
			AssertEquals(expectedOriginalValue, remarkMeta.OriginalValue);
			AssertEquals(expectedHasNoMacro, remarkMeta.HasNoMacro);
		}
	}
}
