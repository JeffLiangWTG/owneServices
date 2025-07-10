using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.CA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACCasualImpRates))]
	sealed class CACCasualImpRatesTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadPSTRates()
		{
			var impRate1 = Factory.New<CACCasualImpRates>();
			impRate1.IR_Commodity = "PSTA";
			impRate1.IR_Province = "XX";
			impRate1.IR_ProcessingType = "P";
			impRate1.IR_EffectiveDateFrom = ZDate.BrettsBirthday;
			impRate1.IR_EffectiveDateTo = ZDate.BrettsBirthday.AddDays(10);

			var impRate2 = Factory.New<CACCasualImpRates>();
			impRate2.IR_Commodity = "PST";
			impRate2.IR_Province = "XX";
			impRate2.IR_ProcessingType = "P";
			impRate2.IR_EffectiveDateFrom = ZDate.BrettsBirthday.AddDays(11);
			impRate2.IR_EffectiveDateTo = ZDate.BrettsBirthday.AddDays(20);

			new UniversalReferenceTestDataHelper(Factory).CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				"Beer", "Alcohol - Beer", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, CasualImportConstants.CasualImpCommodityType.Alcohol);
			Factory.Save();

			AssertEquals(impRate2.PK, CACCasualImpRates.LoadPSTRates(Factory, "XX", "TEST", ZDate.BrettsBirthday.AddDays(15)).PK);
		}
	}
}
