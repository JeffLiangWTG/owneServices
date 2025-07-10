using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters.Testing
{
	internal class ClassificationCodeLookupTest : TestCaseWithFactory
	{
		public void TestArguments()
		{
			ClassificationCodeLookup codeLookup = new ClassificationCodeLookup();

			var argNullEx = AssertExceptionThrown<System.ArgumentNullException>("lookupCode null", () => codeLookup.GetUniqueLookupCode(Factory, null, "X", "X"));
			AssertEquals("lookupCode", argNullEx.ParamName);
			AssertNoExceptionThrown("lookupCode empty", () => codeLookup.GetUniqueLookupCode(Factory, "", "X", "X"));

			argNullEx = AssertExceptionThrown<System.ArgumentNullException>("classificationType null", () => codeLookup.GetUniqueLookupCode(Factory, "X", null, "X"));
			AssertEquals("classificationType", argNullEx.ParamName);
			var argEx = AssertExceptionThrown<System.ArgumentException>("classificationType empty", () => codeLookup.GetUniqueLookupCode(Factory, "X", "", "X"));
			AssertEquals("classificationType", argEx.ParamName);

			argNullEx = AssertExceptionThrown<System.ArgumentNullException>("countryCode null", () => codeLookup.GetUniqueLookupCode(Factory, "X", "X", null));
			AssertEquals("countryCode", argNullEx.ParamName);
			argEx = AssertExceptionThrown<System.ArgumentException>("countryCode empty", () => codeLookup.GetUniqueLookupCode(Factory, "X", "X", ""));
			AssertEquals("countryCode", argEx.ParamName);

			AssertNoExceptionThrown(() => codeLookup.GetUniqueLookupCode(Factory, "X", "X", "X"));
		}

		public void TestGetUniqueLookupCode_WithNoDuplicates()
		{
			ClassificationCodeLookup codeLookup = new ClassificationCodeLookup();

			string code = codeLookup.GetUniqueLookupCode(Factory, "", "EXP", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertEquals("Result when original Code = ''", "1", code);

			code = codeLookup.GetUniqueLookupCode(Factory, "Some Code", "EXP", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertEquals("Result when original Code = 'Some Code'", "Some Code1", code);
		}

		public void TestGetUniqueLookupCode_WithDuplicates()
		{
			BaseCusClassification classif1 = Factory.New<BaseCusClassification>();
			classif1.CC_LookupCode = "Code1";
			classif1.CC_ClassificationType = "IMP";
			classif1.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			BaseCusClassification classif2 = Factory.New<BaseCusClassification>();
			classif2.CC_LookupCode = "Code14";
			classif2.CC_ClassificationType = "IMP";
			classif2.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Factory.Save();

			ClassificationCodeLookup codeLookup = new ClassificationCodeLookup();

			string code = codeLookup.GetUniqueLookupCode(Factory, classif1.CC_LookupCode, classif1.CC_ClassificationType, classif1.CC_RN_NKCountryCode.ToString());
			AssertEquals("Result when original Code = 'Code1'", "Code15", code);

			code = codeLookup.GetUniqueLookupCode(Factory, classif2.CC_LookupCode, classif2.CC_ClassificationType, classif2.CC_RN_NKCountryCode.ToString());
			AssertEquals("Result when original Code = 'Code14'", "Code141", code);
		}

		public void TestGetUniqueLookupCode_WithDuplicatesAndMaxCodeSize()
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			BaseCusClassification classif1 = Factory.New<BaseCusClassification>();
			classif1.CC_LookupCode = "xxxxxxxxx1xxxxxxxxx2xxxxxxxxx3xxxxx";
			classif1.CC_ClassificationType = "EXP";
			classif1.CC_RN_NKCountryCode = countryCode;

			BaseCusClassification classif2 = Factory.New<BaseCusClassification>();
			classif2.CC_LookupCode = "xxxxxxxxx1xxxxxxxxx2xxxxxxxxx3xxxx2";
			classif2.CC_ClassificationType = "EXP";
			classif2.CC_RN_NKCountryCode = countryCode;

			Factory.Save();

			ClassificationCodeLookup codeLookup = new ClassificationCodeLookup();

			string code = codeLookup.GetUniqueLookupCode(Factory, classif1.CC_LookupCode, classif1.CC_ClassificationType, countryCode);
			AssertEquals("Result when original Code = 'xxxxxxxxx1xxxxxxxxx2xxxxxxxxx3xxxxx'", "xxxxxxxxx1xxxxxxxxx2xxxxxxxxx3xxxx3", code);

			code = codeLookup.GetUniqueLookupCode(Factory, "xxxxxxxxx1xxxxxxxxx2xxxxxxxxx3xxxx", classif2.CC_ClassificationType, countryCode);
			AssertEquals("Result when original Code = 'xxxxxxxxx1xxxxxxxxx2xxxxxxxxx3xxxx'", "xxxxxxxxx1xxxxxxxxx2xxxxxxxxx3xxxx3", code);

			code = codeLookup.GetUniqueLookupCode(Factory, classif2.CC_LookupCode, classif2.CC_ClassificationType, countryCode);
			AssertEquals("Result when original Code = 'xxxxxxxxx1xxxxxxxxx2xxxxxxxxx3xxxx2'", "xxxxxxxxx1xxxxxxxxx2xxxxxxxxx3xxxx3", code);

			code = codeLookup.GetUniqueLookupCode(Factory, classif1.CC_LookupCode, "IMP", countryCode);
			AssertEquals("Result when original Code = 'xxxxxxxxx1xxxxxxxxx2xxxxxxxxx3xxxxx' (no dups)", "xxxxxxxxx1xxxxxxxxx2xxxxxxxxx3xxxx1", code);
		}
	}
}
