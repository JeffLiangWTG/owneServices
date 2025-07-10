using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(CostConfirmationDocTypeBizo))]
	public class CostConfirmationDocTypeBizoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCostConfirmationDocTypeDefaultValue()
		{
			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.CostConfirmationDocumentSettingsCodes.Both);
			AssertEquals(AccountingConstants.CostConfirmationDocumentSettingsCodes.Both, new CostConfirmationDocTypeBizo().CostConfirmationDocType);

			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.CostConfirmationDocumentSettingsCodes.Detail);
			AssertEquals(AccountingConstants.CostConfirmationDocumentSettingsCodes.Detail, new CostConfirmationDocTypeBizo().CostConfirmationDocType);

			AccountingConfigurationRegistry.Instance.CostConfirmationDocumentSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.CostConfirmationDocumentSettingsCodes.Summary);
			AssertEquals(AccountingConstants.CostConfirmationDocumentSettingsCodes.Summary, new CostConfirmationDocTypeBizo().CostConfirmationDocType);
		}

		public void TestValidateCostConfirmationDocType()
		{
			CostConfirmationDocTypeBizo testObject = new CostConfirmationDocTypeBizo();
			testObject.CostConfirmationDocType = "XXX";
			AssertHasErrors(testObject.CostConfirmationDocTypeInfo);

			using (testObject.GetValidationSuspender())
			{
				testObject.CostConfirmationDocType = AccountingConstants.CostConfirmationDocumentSettingsCodes.Summary;
			}
			AssertHasErrors(testObject.CostConfirmationDocTypeInfo);

			testObject.RunPreSaveValidation();
			AssertNoErrors(testObject.CostConfirmationDocTypeInfo);

			testObject.CostConfirmationDocType = "";
			AssertHasErrors(testObject.CostConfirmationDocTypeInfo);

			testObject.CostConfirmationDocType = AccountingConstants.CostConfirmationDocumentSettingsCodes.Both;
			AssertNoErrors(testObject.CostConfirmationDocTypeInfo);

			testObject.CostConfirmationDocType = AccountingConstants.CostConfirmationDocumentSettingsCodes.Detail;
			AssertNoErrors(testObject.CostConfirmationDocTypeInfo);
		}
	}
}
