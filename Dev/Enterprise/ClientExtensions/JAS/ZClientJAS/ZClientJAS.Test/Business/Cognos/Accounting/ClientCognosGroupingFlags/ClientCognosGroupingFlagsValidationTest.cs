using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.ResourceStrings.Grammar;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class ClientCognosGroupingFlagsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateT4_Branch()
		{
			AssertNoErrors("Pre-condition", GroupingFlags.T4_BranchInfo);
			GroupingFlags.T4_Branch = 5;
			AssertWithinRangeValidationError(GroupingFlags.T4_BranchInfo, true);
			GroupingFlags.T4_Branch = 4;
			AssertWithinRangeValidationError(GroupingFlags.T4_BranchInfo, false);
			GroupingFlags.T4_Mode = 3;
			GroupingFlags.T4_Branch = 3;
			AssertHasError(GroupingFlags.T4_BranchInfo, "This grouping order has already been specified for another field.");
		}

		public void TestValidateT4_BusinessType()
		{
			AssertNoErrors("Pre-condition", GroupingFlags.T4_BusinessTypeInfo);
			GroupingFlags.T4_BusinessType = 5;
			AssertWithinRangeValidationError(GroupingFlags.T4_BusinessTypeInfo, true);
			GroupingFlags.T4_BusinessType = 4;
			AssertWithinRangeValidationError(GroupingFlags.T4_BusinessTypeInfo, false);
			GroupingFlags.T4_Mode = 3;
			GroupingFlags.T4_BusinessType = 3;
			AssertHasError(GroupingFlags.T4_BusinessTypeInfo, "This grouping order has already been specified for another field.");
		}

		public void TestValidateT4_Geographical()
		{
			AssertNoErrors("Pre-condition", GroupingFlags.T4_GeographicalInfo);
			GroupingFlags.T4_Geographical = 5;
			AssertWithinRangeValidationError(GroupingFlags.T4_GeographicalInfo, true);
			GroupingFlags.T4_Geographical = 4;
			AssertWithinRangeValidationError(GroupingFlags.T4_GeographicalInfo, false);
			GroupingFlags.T4_Mode = 3;
			GroupingFlags.T4_Geographical = 3;
			AssertHasError(GroupingFlags.T4_GeographicalInfo, "This grouping order has already been specified for another field.");
		}

		public void TestValidateT4_Mode()
		{
			AssertNoErrors("Pre-condition", GroupingFlags.T4_ModeInfo);
			GroupingFlags.T4_Mode = 5;
			AssertWithinRangeValidationError(GroupingFlags.T4_ModeInfo, true);
			GroupingFlags.T4_Mode = 4;
			AssertWithinRangeValidationError(GroupingFlags.T4_ModeInfo, false);
			GroupingFlags.T4_Branch = 3;
			GroupingFlags.T4_Mode = 3;
			AssertHasError(GroupingFlags.T4_ModeInfo, "This grouping order has already been specified for another field.");
		}

		public void TestValidateT4_Company()
		{
			AssertNoErrors("Pre-condition", GroupingFlags.T4_CompanyInfo);
			GroupingFlags.T4_Company = "X";
			AssertListValidationInvalidCodeError(GroupingFlags.T4_CompanyInfo, true);
			GroupingFlags.T4_Company = "I/A";
			AssertListValidationInvalidCodeError(GroupingFlags.T4_CompanyInfo, false);
			GroupingFlags.T4_Company = "J";
			AssertListValidationInvalidCodeError(GroupingFlags.T4_CompanyInfo, false);
			GroupingFlags.T4_Company = "ICT";
			AssertListValidationInvalidCodeError(GroupingFlags.T4_CompanyInfo, false);
			GroupingFlags.T4_Company = "NO";
			AssertListValidationInvalidCodeError(GroupingFlags.T4_CompanyInfo, false);
		}

		void AssertWithinRangeValidationError(ZPropertyInfo info, bool expectingError)
		{
			string prefix = Grammar.Instance.IndefiniteArticlePrefix(info.HumanReadableName);
			if (expectingError)
			{
				AssertHasError(info, string.Format("Please enter {0}'{1}' within the range 0 to 4.", prefix, info.HumanReadableName));
			}
			else
			{
				AssertNoError(info, string.Format("Please enter {0}'{1}' within the range 0 to 4.", prefix, info.HumanReadableName));
			}
		}

		CognosGroupingFlags GroupingFlags
		{
			get
			{
				if (fGroupingFlags == null)
				{
					fGroupingFlags = Factory.New<CognosGroupingFlags>();
				}

				return fGroupingFlags;
			}
		}

		CognosGroupingFlags fGroupingFlags;
	}
}
