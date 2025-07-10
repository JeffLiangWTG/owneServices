using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class SpecificCircumstanceListTest : TestCaseWithFactory
	{
		public void TestIsTransportModeSupported()
		{
			CombineAssertions(() =>
			{
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.A, "SEA", true);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.A, "RAI", true);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.A, "ROA", true);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.A, "AIR", true);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.A, "MAI", true);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.A, "INW", true);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.A, "FIX", false);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.C, "SEA", false);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.C, "RAI", false);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.C, "ROA", true);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.C, "AIR", false);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.C, "MAI", true);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.C, "INW", false);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.C, "FIX", false);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.D, "SEA", false);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.D, "RAI", true);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.D, "ROA", false);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.D, "AIR", false);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.D, "MAI", true);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.D, "INW", false);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.D, "FIX", false);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.E, "SEA", true);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.E, "RAI", true);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.E, "ROA", true);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.E, "AIR", true);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.E, "MAI", true);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.E, "INW", true);
				AssertIsTransportModeSupported(SpecificCircumstanceList.Codes.E, "FIX", true);
			});
		}

		void AssertIsTransportModeSupported(string code, string transportMode, bool support)
		{
			AssertEquals(string.Format("{0} + {1}", code, transportMode), support, SpecificCircumstanceList.IsTransportModeSupported(code, transportMode));
		}
	}
}
