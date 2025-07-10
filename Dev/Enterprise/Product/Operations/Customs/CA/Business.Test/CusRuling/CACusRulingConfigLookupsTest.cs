using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using ConfigTypes = Enterprise.Customs.Universal.RefCusRulingConfigTypes;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CACusRulingConfigLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRulingConfigTypeList()
		{
			var ruling = Factory.NewWithValidTestData<ZZRefCusRulingCombined>();
			var rulingConfig = ruling.Configurations.AddNew();
			AssertEquals(0, rulingConfig.Lookups.RulingConfigTypeList.Count);

			rulingConfig.ZZY_Category = RefCusRulingConfigCategories.Codes.DTY;
			AssertEquals(8, rulingConfig.Lookups.RulingConfigTypeList.Count);
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Ad-Valorem"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Minimum"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Maximum"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Specific"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Accept Amount"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Accept Rate"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Treatment Code"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("None/Free"));

			rulingConfig.ZZY_Category = RefCusRulingConfigCategories.Codes.EXD;
			AssertEquals(8, rulingConfig.Lookups.RulingConfigTypeList.Count);
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Ad-Valorem"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Minimum"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Maximum"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Specific"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Accept Amount"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Accept Rate"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Treatment Code"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("None/Free"));

			rulingConfig.ZZY_Category = RefCusRulingConfigCategories.Codes.GST;
			AssertEquals(8, rulingConfig.Lookups.RulingConfigTypeList.Count);
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Ad-Valorem"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Minimum"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Maximum"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Specific"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Accept Amount"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Accept Rate"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Exempt Code"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("None/Free"));

			rulingConfig.ZZY_Category = RefCusRulingConfigCategories.Codes.EXC;
			AssertEquals(8, rulingConfig.Lookups.RulingConfigTypeList.Count);
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Ad-Valorem"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Minimum"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Maximum"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Specific"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Accept Amount"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Accept Rate"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Exempt Code"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("None/Free"));

			rulingConfig.ZZY_Category = RefCusRulingConfigCategories.Codes.SIM;
			AssertEquals(8, rulingConfig.Lookups.RulingConfigTypeList.Count);
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Ad-Valorem"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Minimum"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Maximum"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Specific"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Accept Amount"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Accept Rate"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Exempt Code"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("None/Free"));

			rulingConfig.ZZY_Category = RefCusRulingConfigCategories.Codes.EXC;
			AssertEquals(8, rulingConfig.Lookups.RulingConfigTypeList.Count);
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Ad-Valorem"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Minimum"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Maximum"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Specific"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Accept Amount"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Accept Rate"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Exempt Code"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("None/Free"));

			rulingConfig.ZZY_Category = RefCusRulingConfigCategories.Codes.DAT;
			AssertEquals(2, rulingConfig.Lookups.RulingConfigTypeList.Count);
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("DSD"));
			Assert(rulingConfig.Lookups.RulingConfigTypeList.ContainsCode("Release Date"));
		}

		public void TestRulingConfigValueList()
		{
			var ruling = Factory.NewWithValidTestData<ZZRefCusRulingCombined>();
			var rulingConfig = ruling.Configurations.AddNew();
			AssertEquals(0, rulingConfig.Lookups.RulingConfigValueList.Count);

			rulingConfig.ZZY_Category = RefCusRulingConfigCategories.Codes.DTY;
			rulingConfig.ZZY_Type = ConfigTypes.Codes.AdValorem;
			AssertEquals(0, rulingConfig.Lookups.RulingConfigValueList.Count);
			rulingConfig.ZZY_Type = ConfigTypes.Codes.Minimum;
			AssertEquals(0, rulingConfig.Lookups.RulingConfigValueList.Count);
			rulingConfig.ZZY_Type = ConfigTypes.Codes.Specific;
			AssertEquals(typeof(CodeDescriptionPairList), rulingConfig.Lookups.RulingConfigValueList.GetType());
			rulingConfig.ZZY_Type = ConfigTypes.Codes.TreatmentCode;
			AssertEquals(typeof(TariffTreatmentCodes), rulingConfig.Lookups.RulingConfigValueList.GetType());
			rulingConfig.ZZY_Type = ConfigTypes.Codes.AcceptAmount;
			AssertEquals(0, rulingConfig.Lookups.RulingConfigValueList.Count);

			rulingConfig.ZZY_Category = RefCusRulingConfigCategories.Codes.EXD;
			rulingConfig.ZZY_Type = ConfigTypes.Codes.AdValorem;
			AssertEquals(0, rulingConfig.Lookups.RulingConfigValueList.Count);
			rulingConfig.ZZY_Type = ConfigTypes.Codes.Minimum;
			AssertEquals(0, rulingConfig.Lookups.RulingConfigValueList.Count);
			rulingConfig.ZZY_Type = ConfigTypes.Codes.Specific;
			AssertEquals(typeof(CodeDescriptionPairList), rulingConfig.Lookups.RulingConfigValueList.GetType());
			rulingConfig.ZZY_Type = ConfigTypes.Codes.TreatmentCode;
			AssertEquals(typeof(TariffTreatmentCodes), rulingConfig.Lookups.RulingConfigValueList.GetType());
			rulingConfig.ZZY_Type = ConfigTypes.Codes.AcceptAmount;
			AssertEquals(0, rulingConfig.Lookups.RulingConfigValueList.Count);

			rulingConfig.ZZY_Category = RefCusRulingConfigCategories.Codes.GST;
			rulingConfig.ZZY_Type = ConfigTypes.Codes.Rate;
			AssertEquals(0, rulingConfig.Lookups.RulingConfigValueList.Count);
			rulingConfig.ZZY_Type = ConfigTypes.Codes.AcceptAmount;
			AssertEquals(typeof(CodeDescriptionPairList), rulingConfig.Lookups.RulingConfigValueList.GetType());
			AssertEquals(0, rulingConfig.Lookups.RulingConfigValueList.Count);
			rulingConfig.ZZY_Type = ConfigTypes.Codes.ExemptCode;
			AssertEquals(typeof(GSTStatusCodes), rulingConfig.Lookups.RulingConfigValueList.GetType());

			rulingConfig.ZZY_Category = RefCusRulingConfigCategories.Codes.EXC;
			rulingConfig.ZZY_Type = ConfigTypes.Codes.ExemptCode;
			AssertEquals(typeof(ExciseTaxExemptionCodes), rulingConfig.Lookups.RulingConfigValueList.GetType());

			rulingConfig.ZZY_Category = RefCusRulingConfigCategories.Codes.SIM;
			rulingConfig.ZZY_Type = ConfigTypes.Codes.ExemptCode;
			AssertEquals(typeof(SIMACodes), rulingConfig.Lookups.RulingConfigValueList.GetType());
		}
	}
}
