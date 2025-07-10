using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobBranchDefaultOrderRule))]
	class JobBranchDefaultOrderRuleTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals("DefaultToBlank", (short)0, BizObj.DefaultToBlank);
			AssertEquals("DefaultToBranchRelatedToPortOrWarehouseBranch", (short)1, BizObj.DefaultToBranchRelatedToPortOrWarehouseBranch);
			AssertEquals("DefaultToBranchOfOrganisation", (short)2, BizObj.DefaultToBranchOfOrganisation);
			AssertEquals("DefaultToLoginUserDefault", (short)3, BizObj.DefaultToLoginUserDefault);
		}

		public void TestRangeValidation()
		{
			ZPropertyInfo[] propertyInfos = new ZPropertyInfo[]
			{
				BizObj.DefaultToBlankInfo,
				BizObj.DefaultToBranchRelatedToPortOrWarehouseBranchInfo,
				BizObj.DefaultToBranchOfOrganisationInfo,
				BizObj.DefaultToLoginUserDefaultInfo
			};

			BizObj.DefaultToBlank = 0;
			BizObj.DefaultToBranchRelatedToPortOrWarehouseBranch = 0;
			BizObj.DefaultToBranchOfOrganisation = 0;
			BizObj.DefaultToLoginUserDefault = 0;

			foreach (ZPropertyInfo propertyInfo in propertyInfos)
			{
				propertyInfo.Value = ZShort.Parse("-1");
				AssertHasError(propertyInfo, string.Format("Please enter a '{0}' within the range 0 to 4.", propertyInfo.HumanReadableName));

				propertyInfo.Value = ZShort.Parse("0");
				AssertNoErrors(propertyInfo);

				propertyInfo.Value = ZShort.Parse("1");
				AssertNoErrors(propertyInfo);

				propertyInfo.Value = ZShort.Parse("2");
				AssertNoErrors(propertyInfo);

				propertyInfo.Value = ZShort.Parse("3");
				AssertNoErrors(propertyInfo);

				propertyInfo.Value = ZShort.Parse("4");
				AssertNoErrors(propertyInfo);

				propertyInfo.Value = ZShort.Parse("5");
				AssertHasError(propertyInfo, string.Format("Please enter a '{0}' within the range 0 to 4.", propertyInfo.HumanReadableName));
			}
		}

		public void TestUniqueValidation()
		{
			BizObj.DefaultToBlank = 0;
			BizObj.DefaultToBranchRelatedToPortOrWarehouseBranch = 0;
			BizObj.DefaultToBranchOfOrganisation = 0;
			BizObj.DefaultToLoginUserDefault = 0;

			AssertNoErrors(BizObj.DefaultToBlankInfo);
			AssertNoErrors(BizObj.DefaultToBranchRelatedToPortOrWarehouseBranchInfo);
			AssertNoErrors(BizObj.DefaultToBranchOfOrganisationInfo);
			AssertNoErrors(BizObj.DefaultToLoginUserDefaultInfo);

			BizObj.DefaultToBlank = 1;
			BizObj.DefaultToBranchRelatedToPortOrWarehouseBranch = 1;
			BizObj.DefaultToBranchOfOrganisation = 2;
			BizObj.DefaultToLoginUserDefault = 2;
			BizObj.ValidateDefaultToBlank();
			BizObj.ValidateDefaultToBranchOfOrganisation();

			AssertHasError(BizObj.DefaultToBlankInfo, "The 'Default to Blank' you have entered has the same value as the 'Default to Branch Related to Port / Warehouse Branch'. Please enter a unique value.");
			AssertHasError(BizObj.DefaultToBranchRelatedToPortOrWarehouseBranchInfo, "The 'Default to Branch Related to Port / Warehouse Branch' you have entered has the same value as the 'Default to Blank'. Please enter a unique value.");
			AssertHasError(BizObj.DefaultToBranchOfOrganisationInfo, "The 'Default to Branch of Organisation' you have entered has the same value as the 'Default to Login User Default'. Please enter a unique value.");
			AssertHasError(BizObj.DefaultToLoginUserDefaultInfo, "The 'Default to Login User Default' you have entered has the same value as the 'Default to Branch of Organisation'. Please enter a unique value.");

			BizObj.DefaultToLoginUserDefault = 4;
			BizObj.DefaultToBranchOfOrganisation = 3;
			BizObj.DefaultToBranchRelatedToPortOrWarehouseBranch = 2;
			BizObj.DefaultToBlank = 1;

			AssertNoErrors(BizObj.DefaultToBlankInfo);
			AssertNoErrors(BizObj.DefaultToBranchRelatedToPortOrWarehouseBranchInfo);
			AssertNoErrors(BizObj.DefaultToBranchOfOrganisationInfo);
			AssertNoErrors(BizObj.DefaultToLoginUserDefaultInfo);
		}

		public void TestValidateAtLeastOneValueIsGreaterThanZero()
		{
			TestRowValidation(0, 0, 0, 0, "Please enter at least one value that is greater than zero.");
			TestRowValidation(0, 1, 2, 3, null);
		}

		public void TestValidateNoGapsBetweenValues()
		{
			TestRowValidation(0, 1, 2, 4, "There is a gap between the values 2 and 4. There should not be any gaps between values.");
			TestRowValidation(0, 1, 3, 4, "There is a gap between the values 1 and 3. There should not be any gaps between values.");
			TestRowValidation(0, 2, 3, 4, "There is a gap between the values 0 and 2. There should not be any gaps between values.");
			TestRowValidation(1, 4, 2, 0, "There is a gap between the values 2 and 4. There should not be any gaps between values.");
			TestRowValidation(4, 1, 0, 0, "There is a gap between the values 1 and 4. There should not be any gaps between values.");

			TestRowValidation(0, 1, 2, 3, null);
			TestRowValidation(1, 2, 3, 4, null);
			TestRowValidation(1, 4, 3, 2, null);
			TestRowValidation(1, 0, 3, 2, null);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.DefaultToBlank = 1;
			BizObj.DefaultToBranchRelatedToPortOrWarehouseBranch = 1;
			BizObj.DefaultToBranchOfOrganisation = 2;
			BizObj.DefaultToLoginUserDefault = 2;

			BizObj.ClearAllNotifications();
			BizObj.RunPreSaveValidation();

			AssertHasErrors(BizObj.DefaultToBlankInfo);
			AssertHasErrors(BizObj.DefaultToBranchRelatedToPortOrWarehouseBranchInfo);
			AssertHasErrors(BizObj.DefaultToBranchOfOrganisationInfo);
			AssertHasErrors(BizObj.DefaultToLoginUserDefaultInfo);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			JobBranchDefaultOrderRule result = new JobBranchDefaultOrderRule();

			result.DefaultToBlank = 0;
			result.DefaultToBranchRelatedToPortOrWarehouseBranch = 1;
			result.DefaultToBranchOfOrganisation = 2;
			result.DefaultToLoginUserDefault = 3;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			JobBranchDefaultOrderRule result = new JobBranchDefaultOrderRule();

			result.DefaultToBlank = 3;
			result.DefaultToBranchRelatedToPortOrWarehouseBranch = 2;
			result.DefaultToBranchOfOrganisation = 1;
			result.DefaultToLoginUserDefault = 0;

			return result;
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new JobBranchDefaultOrderRule BizObj
		{
			get { return (JobBranchDefaultOrderRule)base.BizObj; }
		}

		void TestRowValidation(ZShort defaultToBlank, ZShort defaultToBranchRelatedToPortOrWarehouseBranch, ZShort defaultToBranchOfOrganisation, ZShort defaultToLoginUserDefault, string expectedErrorMessage)
		{
			BizObj.DefaultToBlank = defaultToBlank;
			BizObj.DefaultToBranchRelatedToPortOrWarehouseBranch = defaultToBranchRelatedToPortOrWarehouseBranch;
			BizObj.DefaultToBranchOfOrganisation = defaultToBranchOfOrganisation;
			BizObj.DefaultToLoginUserDefault = defaultToLoginUserDefault;

			BizObj.RunPreSaveValidation();

			if (expectedErrorMessage == null)
			{
				AssertNoErrors(BizObj);
			}
			else
			{
				AssertEquals("RowErrors.Length", 1, BizObj.RowErrors.Count());
				AssertEquals("RowErrors[0]", expectedErrorMessage, BizObj.RowErrors.GetFirstMessage());
			}
		}

		#endregion
	}
}
