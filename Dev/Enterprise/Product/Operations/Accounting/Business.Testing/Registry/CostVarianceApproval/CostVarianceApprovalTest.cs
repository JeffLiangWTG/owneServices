using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CostVarianceApproval))]
	class CostVarianceApprovalTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals("VarianceCalculationStyle", Constants.CostVarianceCalculationStyle.LocalExTaxAmount, BizObj.VarianceCalculationStyle);
			AssertEquals("VarianceComparisonOption", Constants.CostVarianceComparisonOption.Job, BizObj.VarianceComparisonOption);
			AssertEquals("AutoTickFinalFlag", ZBool.False, BizObj.AutoTickFinalFlag);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.VarianceCalculationStyle = BizObj.VarianceComparisonOption = String.Empty;
			AssertHasErrors("VarianceCalculationStyle", BizObj.VarianceCalculationStyleInfo);
			AssertHasErrors("VarianceComparisonOption", BizObj.VarianceComparisonOptionInfo);

			BizObj.VarianceCalculationStyle = BizObj.VarianceComparisonOption = "ABC";
			AssertHasErrors("VarianceCalculationStyle", BizObj.VarianceCalculationStyleInfo);
			AssertHasErrors("VarianceComparisonOption", BizObj.VarianceComparisonOptionInfo);

			BizObj.ClearAllNotifications();
			BizObj.RunPreSaveValidation();
			AssertHasErrors("VarianceCalculationStyle", BizObj.VarianceCalculationStyleInfo);
			AssertHasErrors("VarianceComparisonOption", BizObj.VarianceComparisonOptionInfo);

			BizObj.VarianceCalculationStyle = Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			BizObj.VarianceComparisonOption = Constants.CostVarianceComparisonOption.Job;
			BizObj.ClearAllNotifications();
			BizObj.RunPreSaveValidation();
			AssertNoErrors("VarianceCalculationStyle", BizObj.VarianceCalculationStyleInfo);
			AssertNoErrors("VarianceComparisonOption", BizObj.VarianceComparisonOptionInfo);

			var costVarianceApproval = new CostVarianceApproval(new FallbackLevel(GlbCompany.CurrentCompany, null, null), Factory);
			AccountingConfigurationRegistry.Instance.PayableFinalFlag.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.PayableFinalFlagForConsolCost.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			costVarianceApproval.AutoTickFinalFlag = ZBool.True;
			AssertHasErrors("AutoTickFinalFlag", costVarianceApproval.AutoTickFinalFlagInfo);

			AccountingConfigurationRegistry.Instance.PayableFinalFlag.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.PayableFinalFlagForConsolCost.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			costVarianceApproval.ClearAllNotifications();
			costVarianceApproval.RunPreSaveValidation();
			AssertNoErrors("AutoTickFinalFlag", costVarianceApproval.AutoTickFinalFlagInfo);
		}

		public void TestVarianceComparisonOptionsList()
		{
			AssertEquals(6, BizObj.VarianceComparisonOptionsList.Count);
			var listCodes = BizObj.VarianceComparisonOptionsList.GetAllCodes();
			AssertCollectionContains(Constants.CostVarianceComparisonOption.Job, listCodes);
			AssertCollectionContains(Constants.CostVarianceComparisonOption.JobAndChargeCode, listCodes);
			AssertCollectionContains(Constants.CostVarianceComparisonOption.JobAndCreditor, listCodes);
			AssertCollectionContains(Constants.CostVarianceComparisonOption.ImportedChargeOrJob, listCodes);
			AssertCollectionContains(Constants.CostVarianceComparisonOption.ImportedChargeOrJobChargeCode, listCodes);
			AssertCollectionContains(Constants.CostVarianceComparisonOption.ImportedChargeOrCreditor, listCodes);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new CostVarianceApproval();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new CostVarianceApproval BizObj
		{
			get { return (CostVarianceApproval)base.BizObj; }
		}

		#endregion
	}
}
