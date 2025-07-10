using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PGACodesTest : TestCaseWithFactory
	{
		public void TestCodeDescriptionPairList()
		{
			var testData = new PGACodes();
			testData.RemoveCode(PGACodes.Codes.CBSA);
			foreach (var code in testData.GetAllCodes())
			{
				var programCodesList = PGACodes.GetProgramCodesList(code, Factory);
				Assert(ZString.Format("Please check PGA Type '{0}' weather need program description for PGA Tariff", code), programCodesList.Count > 0);
			}
		}

		public void TestGetPGACodeFromGovAgencyID()
		{
			AssertEquals(PGACodes.Codes.CFIA, PGACodes.GetPGACodeFromGovAgencyID("1"));
			AssertEquals(PGACodes.Codes.HC, PGACodes.GetPGACodeFromGovAgencyID("12"));
			AssertEquals(PGACodes.Codes.TC, PGACodes.GetPGACodeFromGovAgencyID("13"));
			AssertEquals(PGACodes.Codes.DFO, PGACodes.GetPGACodeFromGovAgencyID("20"));
			AssertEquals(PGACodes.Codes.NRCan, PGACodes.GetPGACodeFromGovAgencyID("21"));
			AssertEquals(PGACodes.Codes.ECCC, PGACodes.GetPGACodeFromGovAgencyID("22"));
			AssertEquals(PGACodes.Codes.PHAC, PGACodes.GetPGACodeFromGovAgencyID("23"));
			AssertEquals(PGACodes.Codes.CNSC, PGACodes.GetPGACodeFromGovAgencyID("24"));
			AssertEquals(PGACodes.Codes.GAC, PGACodes.GetPGACodeFromGovAgencyID("3"));
			AssertEquals(PGACodes.Codes.CBSA, PGACodes.GetPGACodeFromGovAgencyID("5"));
		}

		public void TestGetGovernmentAgencyCodesList()
		{
			var list = PGACodes.GetGovernmentAgencyCodesList(Factory);
			AssertEquals(9, list.Count);
			AssertEquals(true, list.ContainsCode(PGACodes.Codes.CFIA));
			AssertEquals(true, list.ContainsCode(PGACodes.Codes.CNSC));
			AssertEquals(true, list.ContainsCode(PGACodes.Codes.DFO));
			AssertEquals(true, list.ContainsCode(PGACodes.Codes.ECCC));
			AssertEquals(true, list.ContainsCode(PGACodes.Codes.GAC));
			AssertEquals(true, list.ContainsCode(PGACodes.Codes.TC));
			AssertEquals(true, list.ContainsCode(PGACodes.Codes.HC));
			AssertEquals(true, list.ContainsCode(PGACodes.Codes.NRCan));
			AssertEquals(true, list.ContainsCode(PGACodes.Codes.PHAC));
			AssertEquals(false, list.ContainsCode(PGACodes.Codes.CBSA));
		}
	}
}
