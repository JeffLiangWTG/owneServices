using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CodeDescriptionPairListDumpingExemptionTypeTestClass : TestCase
	{
		public void TestCodeDescriptionPairListDumpingExemptionType()
		{
			CodeDescriptionPairListDumpingExemptionType codeList = new CodeDescriptionPairListDumpingExemptionType();
			AssertEquals("3 codes in the list", 3, codeList.Count);
		}
	}
}
