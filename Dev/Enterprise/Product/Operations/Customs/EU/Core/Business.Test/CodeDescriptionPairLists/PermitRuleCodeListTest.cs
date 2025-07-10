using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CodeDescriptionPairLists.Testing
{
	[TestedType(typeof(PermitRuleCodeList))]
	sealed class PermitRuleCodeListTest : TestCaseWithFactory
	{
		public void TestCodeList() => CombineAssertions(() =>
			AssertCodeDescriptionPairList(new PermitRuleCodeList(),
				("ADD", "Address"),
				("INV", "Guarantee Invalidity (countries in which this guarantee is NOT valid)"),
				("LAP", "Liability Applicable Percentage"),
				("TSP", "Temporary Storage Premises"),
				("CUS", "Customs Office"),
				("PCP", "Percent liability for para-fiscal taxes"),
				("PCV", "Percent liability for VAT"),
				("PCD", "Percent liability for duties")
			));
	}
}
