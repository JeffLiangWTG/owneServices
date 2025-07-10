using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CodeDescriptionPairLists.Testing
{
	[TestedType(typeof(EUGuaranteeTypeList))]
	sealed class EUGuaranteeTypeListTest : TestCaseWithFactory
	{
		public void TestCodeList() => CombineAssertions(() =>
			AssertCodeDescriptionPairList(new EUGuaranteeTypeList(),
				("COD", "Ongoing/comprehensive guarantee"),
				("IMP", "Import"),
				("TRA", "Transit"),
				("TST", "Temporary Storage")
			));
	}
}
