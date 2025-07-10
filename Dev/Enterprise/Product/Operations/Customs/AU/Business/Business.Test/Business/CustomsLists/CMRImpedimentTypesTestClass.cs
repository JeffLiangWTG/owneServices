using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRImpedimentTypesTestClass : TestCase
	{
		public void TestCMRImpedimentTypes()
		{
			CMRImpedimentTypes codeList = new CMRImpedimentTypes();
			AssertEquals("19 codes in the list", 19, codeList.Count);

			Assert(codeList.ContainsCode("APMATCH"));
			Assert(codeList.ContainsCode("COND RELS"));
			Assert(codeList.ContainsCode("CONDRELS"));
			Assert(codeList.ContainsCode("EVA HOLD"));
			Assert(codeList.ContainsCode("EVAHOLD"));
			Assert(codeList.ContainsCode("IFCONDRELS"));
			Assert(codeList.ContainsCode("IFEVAHOLD"));
			Assert(codeList.ContainsCode("CRA IMPHLD"));
			Assert(codeList.ContainsCode("CRAIMPHLD"));
			Assert(codeList.ContainsCode("AQISCONCRN"));
			Assert(codeList.ContainsCode("CPQUAL"));
			Assert(codeList.ContainsCode("REFER AQIS"));
			Assert(codeList.ContainsCode("REFERAQIS"));
			Assert(codeList.ContainsCode("AQISIFACE"));
			Assert(codeList.ContainsCode("AQISACCRED"));
			Assert(codeList.ContainsCode("USERSELECT"));
			Assert(codeList.ContainsCode("USERREFER"));
			Assert(codeList.ContainsCode("APMATCH"));
			Assert(codeList.ContainsCode("USERASSMT"));
		}
	}
}
