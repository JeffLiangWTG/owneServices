using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AmountBasedMultiLevelAuthorisationRequirement))]
	public abstract class AmountBasedMultiLevelAuthorisationRequirementTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestSetCorrectCodeAfterSetLocalizedCode()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				TestBizObj.RangeLocalized = "不存在";
				AssertHasError(TestBizObj.RangeInfo, "输入一个有效的选择。");
				TestBizObj.RangeLocalized = "最高可达";
				AssertEquals("Up to", TestBizObj.Range);
				TestBizObj.RangeLocalized = "Up to";
				AssertEquals("Up to", TestBizObj.Range);

				TestBizObj.AuthorisationRequirementLocalized = "不存在";
				AssertHasError(TestBizObj.AuthorisationRequirementInfo, "输入一个有效的选择。");
				TestBizObj.AuthorisationRequirementLocalized = "无";
				AssertEquals("None", TestBizObj.AuthorisationRequirement);
				TestBizObj.AuthorisationRequirementLocalized = "1st Level Only";
				AssertEquals("1st Level Only", TestBizObj.AuthorisationRequirement);
			}

			foreach (var language in DataFile.GetAvailableLanguages())
			{
				using (Res.TemporarilySwitchLanguage(language))
				{
					if (language != Res.DefaultLanguage)
					{
						foreach (CodeDescriptionPair element in TestBizObj.RangeList)
						{
							if (element.Code == AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo)
							{
								TestBizObj.RangeLocalized = element.MultilingualCode;
								AssertEquals("Up to", TestBizObj.Range);
							}
						}

						foreach (CodeDescriptionPair element in TestBizObj.AuthorisationRequirementList)
						{
							if (element.Code == AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired)
							{
								TestBizObj.AuthorisationRequirementLocalized = element.MultilingualCode;
								AssertEquals("None", TestBizObj.AuthorisationRequirement);
							}
						}
					}
				}
			}
		}

		public void TestValidateRange()
		{
			AssertNoErrors("Precondition: Range should not have errors.", TestBizObj.RangeInfo);
			AssertNotNullOrEmpty("Precondition: RangeList[0].Code should not be empty.", TestBizObj.RangeList[0].Code);

			TestBizObj.Range = "!@#";
			AssertHasError(TestBizObj.RangeInfo, "Enter a valid selection.");

			TestBizObj.Range = TestBizObj.RangeList[0].Code;
			AssertHasError(TestBizObj.RangeInfo, "There must be at least one 'Up to' and one 'Above' line.");

			TestBizObj.Range = "";
			AssertHasError(TestBizObj.RangeInfo, "Please enter a value.");

			var collection = GetAuthorisationRequirementCollection();
			var setting1 = (AmountBasedMultiLevelAuthorisationRequirement)GetNewBusinessObject();
			var setting2 = (AmountBasedMultiLevelAuthorisationRequirement)GetNewBusinessObject();
			var setting3 = (AmountBasedMultiLevelAuthorisationRequirement)GetNewBusinessObject();
			collection.Add(setting1);
			collection.Add(setting2);
			collection.Add(setting3);

			AssertNoErrors("Precondition: setting1.RangeInfo should not have errors.", setting1.RangeInfo);
			AssertNoErrors("Precondition: setting2.RangeInfo should not have errors.", setting2.RangeInfo);
			AssertNoErrors("Precondition: setting3.RangeInfo should not have errors.", setting3.RangeInfo);

			setting1.Range = "Up to";
			AssertHasError(setting1.RangeInfo, "There must be at least one 'Up to' and one 'Above' line.");
			AssertNoErrors("Setting2.RangeInfo should not have errors.", setting2.RangeInfo);
			AssertNoErrors("Setting3.RangeInfo should not have errors.", setting3.RangeInfo);

			setting1.Range = "Above";
			AssertNoErrors("Setting1.RangeInfo should not have errors.", setting1.RangeInfo);
			AssertNoErrors("Setting2.RangeInfo should not have errors.", setting2.RangeInfo);
			AssertNoErrors("Setting3.RangeInfo should not have errors.", setting3.RangeInfo);

			setting1.Range = "Up to";
			setting2.Range = "Above";
			AssertNoErrors("Setting1.RangeInfo should not have errors.", setting1.RangeInfo);
			AssertNoErrors("Setting2.RangeInfo should not have errors.", setting2.RangeInfo);
			AssertNoErrors("Setting3.RangeInfo should not have errors.", setting3.RangeInfo);

			setting3.Range = "Above";
			AssertNoErrors("Setting1.RangeInfo should not have errors.", setting1.RangeInfo);
			AssertNoErrors("Setting2.RangeInfo should not have errors.", setting2.RangeInfo);
			AssertHasError(setting3.RangeInfo, "There must be only one 'Above' line.");

			setting3.Range = "Up to";
			AssertNoErrors("Setting1.RangeInfo should not have errors.", setting1.RangeInfo);
			AssertNoErrors("Setting2.RangeInfo should not have errors.", setting2.RangeInfo);
			AssertNoErrors("Setting3.RangeInfo should not have errors.", setting3.RangeInfo);
		}

		public virtual void TestValidateAmount()
		{
			AssertNoErrors("Precondition: Amount should not have errors.", TestBizObj.AmountInfo);

			TestBizObj.Amount = 100;
			AssertNoErrors(TestBizObj.AmountInfo);

			TestBizObj.Amount = -1;
			AssertHasErrorContaining(TestBizObj.AmountInfo, string.Format("'{0}' greater than or equal to 0.", TestBizObj.AmountInfo.HumanReadableName));

			TestBizObj.Amount = 0;
			AssertNoErrors("Single not ranged line can be equal 0.", TestBizObj.AmountInfo);

			TestBizObj.Range = "Up to";
			TestBizObj.Amount = -1;
			AssertHasErrorContaining(TestBizObj.AmountInfo, string.Format("'{0}' greater than 0.", TestBizObj.AmountInfo.HumanReadableName));

			TestBizObj.Range = "Above";
			TestBizObj.Amount = -1;
			AssertHasErrorContaining(TestBizObj.AmountInfo, string.Format("'{0}' greater than or equal to 0.", TestBizObj.AmountInfo.HumanReadableName));

			TestBizObj.Range = "Up to";
			TestBizObj.Amount = 0;
			AssertHasErrorContaining(TestBizObj.AmountInfo, string.Format("'{0}' greater than 0.", TestBizObj.AmountInfo.HumanReadableName));

			TestBizObj.Range = "Above";
			TestBizObj.Amount = 0;
			AssertNoErrors("Single 'Above' line can be equal 0.", TestBizObj.AmountInfo);

			var collection = GetAuthorisationRequirementCollection();
			var setting1 = (AmountBasedMultiLevelAuthorisationRequirement)GetNewBusinessObject();
			collection.Add(setting1);

			AssertNoErrors("Precondition: setting1.RangeInfo should not have errors.", setting1.AmountInfo);

			setting1.Amount = 0;
			AssertNoErrors("Zero is valid for not ranged line in one line collection.", setting1.AmountInfo);

			setting1.Range = "Above";
			setting1.Amount = 100;
			AssertNoErrors("Any positive value is valid for 'Above' line in one line collection.", setting1.AmountInfo);

			setting1.Range = "Up to";
			setting1.Amount = 0;
			AssertHasErrorContaining(setting1.AmountInfo, string.Format("'{0}' greater than 0.", setting1.AmountInfo.HumanReadableName));

			setting1.Range = "Up to";
			setting1.Amount = -1;
			AssertHasErrorContaining(setting1.AmountInfo, string.Format("'{0}' greater than 0.", setting1.AmountInfo.HumanReadableName));

			setting1.Range = "Above";
			setting1.Amount = -1;
			AssertHasErrorContaining(setting1.AmountInfo, string.Format("'{0}' greater than or equal to 0.", setting1.AmountInfo.HumanReadableName));

			setting1.Range = "Above";
			setting1.Amount = 0;
			AssertNoErrors("Zero is valid for 'Above' line in one line collection.", setting1.AmountInfo);

			var setting2 = (AmountBasedMultiLevelAuthorisationRequirement)GetNewBusinessObject();
			var setting3 = (AmountBasedMultiLevelAuthorisationRequirement)GetNewBusinessObject();
			var setting4 = (AmountBasedMultiLevelAuthorisationRequirement)GetNewBusinessObject();
			collection.Add(setting2);
			collection.Add(setting3);
			collection.Add(setting4);

			AssertNoErrors("Precondition: setting2.RangeInfo should not have errors.", setting2.AmountInfo);
			AssertNoErrors("Precondition: setting3.RangeInfo should not have errors.", setting3.AmountInfo);
			AssertNoErrors("Precondition: setting4.RangeInfo should not have errors.", setting4.AmountInfo);

			setting1.Range = "Up to";
			setting1.Amount = 700;
			setting2.Range = "Up to";
			setting2.Amount = 200;
			setting3.Range = "Up to";
			setting3.Amount = 200;
			AssertNoErrors(setting1.AmountInfo);
			AssertNoErrors(setting2.AmountInfo);
			AssertHasErrors("No two 'Up to' Lines can have the same amount.", setting3.AmountInfo);

			setting3.Amount = 300;
			setting4.Range = "Above";
			setting4.Amount = 450;

			AssertNoErrors(setting1.AmountInfo);
			AssertNoErrors(setting2.AmountInfo);
			AssertNoErrors(setting3.AmountInfo);
			AssertHasErrors("The ‘Above’ Line’s amount must be 700.", setting4.AmountInfo);

			setting4.Amount = 700;
			AssertNoErrors(setting4.AmountInfo);
		}

		public virtual void TestValidateAuthorisationRequirement()
		{
			AssertNoErrors("Precondition: AuthorisationRequirement should not have errors.", TestBizObj.AuthorisationRequirementInfo);

			AssertNotNullOrEmpty("Precondition: AuthorisationRequirementList[0].Code should not be empty.", TestBizObj.AuthorisationRequirementList[0].Code);

			TestBizObj.AuthorisationRequirement = "!@#";
			AssertHasError(TestBizObj.AuthorisationRequirementInfo, "Enter a valid selection.");

			TestBizObj.AuthorisationRequirement = TestBizObj.AuthorisationRequirementList[0].Code;
			AssertNoErrors(TestBizObj.AuthorisationRequirementInfo);

			TestBizObj.AuthorisationRequirement = "";
			AssertHasError(TestBizObj.AuthorisationRequirementInfo, "Please enter a value.");
		}

		public void TestRunPreSaveValidation()
		{
			TestBizObj.Range = "!@#";
			TestBizObj.AuthorisationRequirement = "!@#";

			TestBizObj.ClearAllNotifications();
			AssertNoErrors("Precondition: There should be no errors.", TestBizObj);

			TestBizObj.RunPreSaveValidation();
			AssertHasErrors(TestBizObj.RangeInfo);
			AssertHasErrors(TestBizObj.AuthorisationRequirementInfo);
		}

		public void TestRangeList()
		{
			AssertEquals("RangeList.Count", 2, TestBizObj.RangeList.Count);
			AssertEquals("The RangeList should contain 'Up to'", true, TestBizObj.RangeList.ContainsCode("Up to"));
			AssertEquals("The RangeList should contain 'Above'", true, TestBizObj.RangeList.ContainsCode("Above"));
		}

		public virtual void TestAuthorisationRequirementList()
		{
			AssertEquals("AuthorisationRequirementList.Count", 8, TestBizObj.AuthorisationRequirementList.Count);
			AssertEquals("AuthorisationRequirementList should contain 'None'", true, TestBizObj.AuthorisationRequirementList.ContainsCode("None"));
			AssertEquals("AuthorisationRequirementList should contain '1st, 2nd and 3rd Level'", true, TestBizObj.AuthorisationRequirementList.ContainsCode("1st, 2nd and 3rd Level"));
		}

		public void TestAmountDecimals()
		{
			AmountBasedAuthorisationRequirementCollection collection = GetAuthorisationRequirementCollection();
			AmountBasedMultiLevelAuthorisationRequirement requirement = collection.AddNew();
			AssertEquals(requirement.AmountDecimals, collection.CurrentFallbackCompanyCurrencyDecimals);
		}

		public void TestMultilingual()
		{
			const string hao = "好";
			using (var mockData = Res.UseMockData())
			{
				mockData.SetResourceGetter(key => new ResourceStringData(key, hao));

				AssertEquals(hao, ((CodeDescriptionPair)TestBizObj.RangeList[0]).MultilingualCode);
				AssertEquals(hao, ((CodeDescriptionPair)TestBizObj.AuthorisationRequirementList[0]).MultilingualCode);

				TestBizObj.Range = "bla";
				AssertEquals("bla", TestBizObj.RangeMultilingual.GetUnresolvedString());
				TestBizObj.Range = "Up to";
				AssertEquals("Up to", TestBizObj.RangeMultilingual.GetUnresolvedString());
				AssertEquals(hao, TestBizObj.RangeMultilingual);
				TestBizObj.Range = "Above";
				AssertEquals("Above", TestBizObj.RangeMultilingual.GetUnresolvedString());
				AssertEquals(hao, TestBizObj.RangeMultilingual);

				TestBizObj.AuthorisationRequirement = "bla";
				AssertEquals("bla", TestBizObj.AuthorisationRequirementMultilingual.GetUnresolvedString());
				TestBizObj.AuthorisationRequirement = "1st Level Only";
				AssertEquals("1st Level Only", TestBizObj.AuthorisationRequirementMultilingual.GetUnresolvedString());
				AssertEquals(hao, TestBizObj.AuthorisationRequirementMultilingual);
				TestBizObj.AuthorisationRequirement = "2nd Level Only";
				AssertEquals("2nd Level Only", TestBizObj.AuthorisationRequirementMultilingual.GetUnresolvedString());
				AssertEquals(hao, TestBizObj.AuthorisationRequirementMultilingual);
			}
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			AmountBasedMultiLevelAuthorisationRequirement result = (AmountBasedMultiLevelAuthorisationRequirement)GetNewBusinessObject();

			result.Range = result.RangeList[0].Code;
			result.Amount = 500;
			result.AuthorisationRequirement = result.AuthorisationRequirementList[0].Code;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		AmountBasedMultiLevelAuthorisationRequirement TestBizObj
		{
			get { return (AmountBasedMultiLevelAuthorisationRequirement)BizObj; }
		}

		protected abstract AmountBasedAuthorisationRequirementCollection GetAuthorisationRequirementCollection();

		#endregion
	}
}
