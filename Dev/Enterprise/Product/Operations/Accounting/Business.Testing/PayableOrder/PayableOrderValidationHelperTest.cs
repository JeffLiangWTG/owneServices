using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.PayableOrder.Testing
{
	public class PayableOrderValidationHelperTest : LevelAuthorizationSecurityHelperTest<PaymentThreeLevelAuthorisationSettingsRegistryItem, PaymentThreeLevelAuthorisationSettingsCollection, PaymentThreeLevelAuthorisationSettings>
	{
		[GuiTest]
		public void TestCheckSecurityLevelsAndPromptForAuthorisation()
		{
			var order = Factory.New<AccPayableOrderHeader>();
			var line = order.OrderLines.AddNew();
			line.APL_LinePrice = 100m;
			PayableOrderValidationHelper validation = new PayableOrderValidationHelper();
			AssertEquals("No approval required as invoice is below 1000", true, validation.CheckSecurityLevelsAndPromptForAuthorisation(order));
			Env.Security.PayableOrderApprovalFirstLevelApproval.IsAllowed = false;
			line.APL_LinePrice = 1001m;
			AssertEquals("This transaction requires a higher level of approval authority.", false, validation.CheckSecurityLevelsAndPromptForAuthorisation(order));
			Env.Security.PayableOrderApprovalFirstLevelApproval.IsAllowed = true;
			AssertEquals("No approval required as invoice amount is within current apporval limits", true, validation.CheckSecurityLevelsAndPromptForAuthorisation(order));
			Env.Security.PayableOrderApprovalSecondLevelApproval.IsAllowed = false;
			line.APL_LinePrice = 2001m;
			AssertEquals("This transaction requires a higher level of approval authority.", false, validation.CheckSecurityLevelsAndPromptForAuthorisation(order));
			Env.Security.PayableOrderApprovalSecondLevelApproval.IsAllowed = true;
			AssertEquals("No approval required as invoice amount is within current apporval limits", true, validation.CheckSecurityLevelsAndPromptForAuthorisation(order));
			Env.Security.PayableOrderApprovalThirdLevelApproval.IsAllowed = false;
			line.APL_LinePrice = 3001m;
			AssertEquals("This transaction requires a higher level of approval authority.", false, validation.CheckSecurityLevelsAndPromptForAuthorisation(order));
			Env.Security.PayableOrderApprovalThirdLevelApproval.IsAllowed = true;
			AssertEquals("No approval required as invoice amount is within current apporval limits", true, validation.CheckSecurityLevelsAndPromptForAuthorisation(order));
		}

		public override void TestGetSecurityCheckPoint()
		{
			var testAmount = upTo1000.Amount;
			var testCheckpoint = new PayableOrderValidationHelper().GetSecurityCheckPoint(-1000);
			AssertEquals(Env.Security.None.Code, testCheckpoint.Code);
			testCheckpoint = new PayableOrderValidationHelper().GetSecurityCheckPoint(0);
			AssertEquals(Env.Security.None.Code, testCheckpoint.Code);
			testCheckpoint = new PayableOrderValidationHelper().GetSecurityCheckPoint(testAmount - 1);
			AssertEquals(Env.Security.None.Code, testCheckpoint.Code);
			testCheckpoint = new PayableOrderValidationHelper().GetSecurityCheckPoint(testAmount);
			AssertEquals(Env.Security.None.Code, testCheckpoint.Code);
			testCheckpoint = new PayableOrderValidationHelper().GetSecurityCheckPoint(testAmount + 1);
			AssertEquals(Env.Security.PayableOrderApprovalFirstLevelApproval.Code, testCheckpoint.Code);
			testCheckpoint = new PayableOrderValidationHelper().GetSecurityCheckPoint(testAmount + 1000);
			AssertEquals(Env.Security.PayableOrderApprovalFirstLevelApproval.Code, testCheckpoint.Code);
			testCheckpoint = new PayableOrderValidationHelper().GetSecurityCheckPoint(testAmount + 1001);
			AssertEquals(Env.Security.PayableOrderApprovalSecondLevelApproval.Code, testCheckpoint.Code);
			testCheckpoint = new PayableOrderValidationHelper().GetSecurityCheckPoint(testAmount + 2000);
			AssertEquals(Env.Security.PayableOrderApprovalSecondLevelApproval.Code, testCheckpoint.Code);
			testCheckpoint = new PayableOrderValidationHelper().GetSecurityCheckPoint(testAmount + 2001);
			AssertEquals(Env.Security.PayableOrderApprovalThirdLevelApproval.Code, testCheckpoint.Code);
			testCheckpoint = new PayableOrderValidationHelper().GetSecurityCheckPoint(testAmount + 5000);
			AssertEquals(Env.Security.PayableOrderApprovalThirdLevelApproval.Code, testCheckpoint.Code);
		}

		public override PaymentThreeLevelAuthorisationSettingsRegistryItem GetRegistryItem()
		{
			return AccountingConfigurationRegistry.Instance.PayableOrderAuthorizationSettings;
		}

		public override PaymentThreeLevelAuthorisationSettingsCollection PrepareAuthorisationSettingsCollection()
		{
			var valuesForTest = new PaymentThreeLevelAuthorisationSettingsCollection();
			upTo1000 = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 1000, AuthorisationCodes.NoApprovalRequired);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 2000, AuthorisationCodes.FirstApprovalRequiredOnly);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 3000, AuthorisationCodes.SecondApprovalRequiredOnly);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.Above, 3000, AuthorisationCodes.ThirdApprovalRequiredOnly);
			return valuesForTest;
		}

		AmountBasedMultiLevelAuthorisationRequirement upTo1000;
	}
}
