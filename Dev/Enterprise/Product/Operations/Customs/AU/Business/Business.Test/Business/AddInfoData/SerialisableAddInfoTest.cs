using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SerialisableAddInfoTest : TestCaseWithFactory
	{
		public void TestToString()
		{
			DummySerialisableAddInfo addInfo = new DummySerialisableAddInfo(Factory);
			using (addInfo.GetValidationSuspender()) // get around a poor design descision in AutoGenerator (autos accessing non-autos)
			{
				addInfo.ZA_ADJ = "0101";
				addInfo.ZA_AQIS = "NTR";
				addInfo.ZA_GSTE = "N20";
				addInfo.ZA_ICN = (ZString)addInfo.ZA_ICN.Default;

				ZString toString = addInfo.ToString();

				AssertEquals("ToString()", true, toString.Contains("GSTE=N20"));
				AssertEquals("ToString()", true, toString.Contains("AQIS=NTR"));
				AssertEquals("ToString()", true, toString.Contains("AQIS=NTR"));
				AssertEquals("ToString()", false, toString.Contains("ICN="));
				AssertEquals("ToString()", false, toString.Contains("ICN="));
				AssertEquals("ToString()", false, toString.Contains("ZZZ="));
				AssertEquals("ToString()", false, toString.Contains("ZZ1="));
				AssertEquals("* count", 2, toString.Occurrences("*"));
				AssertEquals("= count", 3, toString.Occurrences("="));
			}
		}

		sealed class DummySerialisableAddInfo : SerialisableAUAddInfo
		{
			public DummySerialisableAddInfo(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			[CargoWise.ComponentModel.MaxLength(10)]
			public ZString ZA_ZZZ => "BLAH";

			public ZPropertyInfo ZA_ZZZInfo => GetZPropertyInfo(nameof(ZA_ZZZ));

			[CargoWise.ComponentModel.MaxLength(10)]
			public ZString ZA_ZZ1
			{
				get => "BLAH2";
				set { }
			}

			public ZPropertyInfo ZA_ZZ1Info => GetZPropertyInfo(nameof(ZA_ZZZ));
		}
	}
}
