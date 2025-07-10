using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.Utilities.Testing
{
	internal class PortCodeFinderTest : TestCaseWithFactory
	{
		public void TestGetPortCodeFromIATACode()
		{
			AssertEquals("ID002", Finder.GetPortCodeFromIATACode(Factory, "002"));
			AssertEquals("123", Finder.GetPortCodeFromIATACode(Factory, "123"));
			AssertEquals("US123", Finder.GetPortCodeFromIATACode(Factory, "123", "US"));
			AssertEquals("AU001", Finder.GetPortCodeFromIATACode(Factory, "001", "AU"));
			AssertEquals("US003", Finder.GetPortCodeFromIATACode(Factory, "003"));
			AssertEquals("", Finder.GetPortCodeFromIATACode(Factory, ""));
			AssertEquals("AU023", Finder.GetPortCodeFromIATACode(Factory, "023333333333333333333333333", "AU"));
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			InsertUNLOCOsForTest();
		}

		void InsertUNLOCOsForTest()
		{
			InsertUNLOCO("AU001", "001");
			InsertUNLOCO("CA001", "001");
			InsertUNLOCO("US001", "001");
			InsertUNLOCO("ID001", "");
			InsertUNLOCO("ID002", "002");
			InsertUNLOCO("AU003", "003");
			InsertUNLOCO("US003", "003");
			InsertUNLOCO("ZA111", "003");
		}

		void InsertUNLOCO(ZString code, ZString iATACode)
		{
			RefUNLOCO port = Factory.New<RefUNLOCO>();
			port.RL_Code = code;
			port.RL_IATA = iATACode;
			RefCountry country = RefCountry.LoadFromCountryCode(Factory, code.Left(2));
			if (country != null)
			{
				port.RL_RN_NKCountryCode = country.Code;
			}
		}

		PortCodeFinder Finder
		{
			get
			{
				if (fFinder == null)
				{
					fFinder = new PortCodeFinder();
				}

				return fFinder;
			}
		}

		PortCodeFinder fFinder;
		#endregion
	}
}
