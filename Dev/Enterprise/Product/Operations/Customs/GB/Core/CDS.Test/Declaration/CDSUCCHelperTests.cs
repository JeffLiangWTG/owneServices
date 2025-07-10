using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.CDS.Declaration;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSUCCHelperTests : TestCaseWithFactory
	{
		public void TestGetAuthorisationCode()
		{
			var uccHelper = new CDSUCCHelperForTest();
			AssertEquals("AEOC", uccHelper.GetAuthorisationCode("C501"));
			AssertEquals("AEOS", uccHelper.GetAuthorisationCode("C502"));
			AssertEquals("AEOF", uccHelper.GetAuthorisationCode("C503"));
			AssertEquals("CVA", uccHelper.GetAuthorisationCode("C504"));
			AssertEquals("CGU", uccHelper.GetAuthorisationCode("C505"));
			AssertEquals("DPO", uccHelper.GetAuthorisationCode("C506"));
			AssertEquals("REP", uccHelper.GetAuthorisationCode("C507"));
			AssertEquals("REM", uccHelper.GetAuthorisationCode("C508"));
			AssertEquals("TST", uccHelper.GetAuthorisationCode("C509"));
			AssertEquals("RSS", uccHelper.GetAuthorisationCode("C510"));
			AssertEquals("ACP", uccHelper.GetAuthorisationCode("C511"));
			AssertEquals("SDE", uccHelper.GetAuthorisationCode("C512"));
			AssertEquals("CCL", uccHelper.GetAuthorisationCode("C513"));
			AssertEquals("EIR", uccHelper.GetAuthorisationCode("C514"));
			AssertEquals("SAS", uccHelper.GetAuthorisationCode("C515"));
			AssertEquals("TEA", uccHelper.GetAuthorisationCode("C516"));
			AssertEquals("CWP", uccHelper.GetAuthorisationCode("C517"));
			AssertEquals("CW1", uccHelper.GetAuthorisationCode("C518"));
			AssertEquals("CW2", uccHelper.GetAuthorisationCode("C519"));
			AssertEquals("ACT", uccHelper.GetAuthorisationCode("C520"));
			AssertEquals("ACR", uccHelper.GetAuthorisationCode("C521"));
			AssertEquals("ACE", uccHelper.GetAuthorisationCode("C522"));
			AssertEquals("SSE", uccHelper.GetAuthorisationCode("C523"));
			AssertEquals("TRD", uccHelper.GetAuthorisationCode("C524"));
			AssertEquals("ETD", uccHelper.GetAuthorisationCode("C525"));
			AssertEquals("AWB", uccHelper.GetAuthorisationCode("C526"));
			AssertEquals("IPO", uccHelper.GetAuthorisationCode("C601"));
			AssertEquals("ATR", uccHelper.GetAuthorisationCode("1ATR"));
			AssertEquals("AOR", uccHelper.GetAuthorisationCode("1AOR"));
			AssertEquals("AVR", uccHelper.GetAuthorisationCode("1AVR"));
			AssertEquals(ZString.Empty, uccHelper.GetAuthorisationCode(ZString.Empty));
		}
	}

	class CDSUCCHelperForTest : CDSUCCHelper
	{
		public new ZString GetAuthorisationCode(ZString documentCode)
		{
			return base.GetAuthorisationCode(documentCode);
		}
	}
}
