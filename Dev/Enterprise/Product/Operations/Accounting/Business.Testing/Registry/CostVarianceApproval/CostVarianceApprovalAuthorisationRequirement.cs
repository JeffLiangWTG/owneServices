using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	// Do not use AddNew to add new CostVarianceApprovalAuthorisationRequirement to test collection !!!
	// Use GetNewBusinessObject() then Add it to test collection as we should have correct VarianceSign for testing

	[TestedType(typeof(CostVarianceApprovalAuthorisationRequirement))]
	public class PositiveCostVarianceApprovalAuthorisationRequirementTest : CostVarianceApprovalAuthorisationRequirementBaseTest
	{
		protected override ZString VarianceSign
		{
			get { return CostVarianceApprovalAuthorisationRequirement.VarianceSigns.Plus; }
		}
	}

	[TestedType(typeof(CostVarianceApprovalAuthorisationRequirement))]
	public class NegativeCostVarianceApprovalAuthorisationRequirementTest : CostVarianceApprovalAuthorisationRequirementBaseTest
	{
		protected override ZString VarianceSign
		{
			get { return CostVarianceApprovalAuthorisationRequirement.VarianceSigns.Minus; }
		}
	}

	#region Base Test Class

	[TestedType(typeof(CostVarianceApprovalAuthorisationRequirement))]
	public abstract class CostVarianceApprovalAuthorisationRequirementBaseTest : AmountBasedTwoLevelAuthorisationRequirementTest
	{
		public void TestDefaults()
		{
			var testRequirement = GetNewBusinessObjectWithoutSignOverride();
			AssertEquals("Percentage", 0M, testRequirement.Percentage);
			AssertEquals("LocalCostAmount", 0M, testRequirement.LocalCostAmount);
			AssertEquals("MonitorTotalInvoiceVariance", false, testRequirement.MonitorTotalInvoiceVariance);
			AssertEquals("TotalInvoiceVarianceAmount", 0M, testRequirement.TotalInvoiceVarianceAmount);
			AssertEquals("VarianceSign", CostVarianceApprovalAuthorisationRequirement.VarianceSigns.Plus, testRequirement.VarianceSign);
		}

		public void TestPercentageValidation()
		{
			TestBizObj.Percentage = 0;
			AssertNoErrors("0", TestBizObj.PercentageInfo);
			TestBizObj.Percentage = 50;
			AssertNoErrors("50", TestBizObj.PercentageInfo);
			TestBizObj.Percentage = 100;
			AssertNoErrors("100", TestBizObj.PercentageInfo);
			TestBizObj.Percentage = -1;
			AssertHasErrors("-1", TestBizObj.PercentageInfo);
			TestBizObj.Percentage = 101;
			AssertHasErrors("101", TestBizObj.PercentageInfo);
		}

		public override void TestValidateAmount()
		{
			BizObj = (RegistryBusinessObjectTemplate)GetNewBusinessObject();
			CostVarianceApproval costApproval = new CostVarianceApproval();
			costApproval.AuthorisationRequirements.Add(BizObj);

			costApproval.VarianceCalculationStyle = Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			base.TestValidateAmount();

			costApproval.VarianceCalculationStyle = Constants.CostVarianceCalculationStyle.PercentageVariance;
			base.TestValidateAmount();

			costApproval.VarianceCalculationStyle = Constants.CostVarianceCalculationStyle.PercentageVarianceAndMaximumLocalExTaxVariance;
			base.TestValidateAmount();
		}

		public void TestValidatePercentage()
		{
			BizObj = (RegistryBusinessObjectTemplate)GetNewBusinessObject();
			CostVarianceApproval costApproval = new CostVarianceApproval();
			costApproval.AuthorisationRequirements.Add(BizObj);

			costApproval.VarianceCalculationStyle = Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			TestBizObj.Percentage = 1000;
			AssertNoErrors(TestBizObj.PercentageInfo);

			TestBizObj.Percentage = -1000;
			AssertNoErrors(TestBizObj.PercentageInfo);

			costApproval.VarianceCalculationStyle = Constants.CostVarianceCalculationStyle.PercentageVariance;
			TestBizObj.ValidatePercentage();
			AssertHasErrors(TestBizObj.PercentageInfo);

			TestBizObj.Percentage = 10;
			AssertNoErrors(TestBizObj.PercentageInfo);

			TestBizObj.Percentage = -10;
			AssertHasErrors(TestBizObj.PercentageInfo);

			costApproval.VarianceCalculationStyle = Constants.CostVarianceCalculationStyle.PercentageVarianceAndMaximumLocalExTaxVariance;
			TestBizObj.ValidatePercentage();
			AssertHasErrors(TestBizObj.PercentageInfo);

			TestBizObj.Percentage = 100;
			AssertNoErrors(TestBizObj.PercentageInfo);

			TestBizObj.Percentage = 101;
			AssertHasErrors(TestBizObj.PercentageInfo);

			TestBizObj.Percentage = 10;
			AssertNoErrors(TestBizObj.PercentageInfo);
		}

		public void TestValidateLocalCostAmount()
		{
			BizObj = (RegistryBusinessObjectTemplate)GetNewBusinessObject();
			CostVarianceApproval costApproval = new CostVarianceApproval();
			costApproval.AuthorisationRequirements.Add(BizObj);

			costApproval.VarianceCalculationStyle = Constants.CostVarianceCalculationStyle.PercentageVariance;
			TestBizObj.LocalCostAmount = -10;
			AssertNoErrors(TestBizObj.LocalCostAmountInfo);

			TestBizObj.LocalCostAmount = 10;
			AssertNoErrors(TestBizObj.LocalCostAmountInfo);

			TestBizObj.LocalCostAmount = 0;
			AssertNoErrors(TestBizObj.LocalCostAmountInfo);

			costApproval.VarianceCalculationStyle = Constants.CostVarianceCalculationStyle.PercentageVarianceAndMaximumLocalExTaxVariance;
			TestBizObj.LocalCostAmount = 0;
			AssertHasErrors(TestBizObj.LocalCostAmountInfo);

			TestBizObj.LocalCostAmount = 100;
			AssertNoErrors(TestBizObj.LocalCostAmountInfo);

			TestBizObj.LocalCostAmount = 101;
			AssertNoErrors(TestBizObj.LocalCostAmountInfo);

			TestBizObj.LocalCostAmount = -10;
			AssertHasErrors(TestBizObj.LocalCostAmountInfo);

			var authorisationRequirement1 = (CostVarianceApprovalAuthorisationRequirement)GetNewBusinessObject();
			costApproval.AuthorisationRequirements.Add(authorisationRequirement1);
			authorisationRequirement1.Range = CostVarianceApprovalAuthorisationRequirement.RangeCodes.UpTo;
			authorisationRequirement1.AuthorisationRequirement = CostVarianceApprovalAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			authorisationRequirement1.LocalCostAmount = 100;
			var oppositeAuthorisationRequirement1 = AddOppositeSignCopy(costApproval.AuthorisationRequirements, authorisationRequirement1);

			var authorisationRequirement2 = (CostVarianceApprovalAuthorisationRequirement)GetNewBusinessObject();
			costApproval.AuthorisationRequirements.Add(authorisationRequirement2);
			authorisationRequirement2.Range = CostVarianceApprovalAuthorisationRequirement.RangeCodes.Above;
			authorisationRequirement2.AuthorisationRequirement = CostVarianceApprovalAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			authorisationRequirement2.LocalCostAmount = 100;
			var oppositeAuthorisationRequirement2 = AddOppositeSignCopy(costApproval.AuthorisationRequirements, authorisationRequirement2);

			TestBizObj.Range = CostVarianceApprovalAuthorisationRequirement.RangeCodes.UpTo;
			TestBizObj.AuthorisationRequirement = CostVarianceApprovalAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			TestBizObj.LocalCostAmount = 100;
			AssertNoErrors(TestBizObj.LocalCostAmountInfo);
			authorisationRequirement1.ValidateLocalCostAmount();
			authorisationRequirement2.ValidateLocalCostAmount();
			AssertNoErrors(authorisationRequirement1.LocalCostAmountInfo);
			AssertNoErrors(authorisationRequirement2.LocalCostAmountInfo);

			oppositeAuthorisationRequirement1.ValidateLocalCostAmount();
			oppositeAuthorisationRequirement2.ValidateLocalCostAmount();
			AssertNoErrors(oppositeAuthorisationRequirement1.LocalCostAmountInfo);
			AssertNoErrors(oppositeAuthorisationRequirement2.LocalCostAmountInfo);

			TestBizObj.LocalCostAmount = 99;
			AssertHasErrors(TestBizObj.LocalCostAmountInfo);
			authorisationRequirement1.ValidateLocalCostAmount();
			authorisationRequirement2.ValidateLocalCostAmount();
			AssertHasErrors(authorisationRequirement1.LocalCostAmountInfo);
			AssertNoErrors(authorisationRequirement2.LocalCostAmountInfo);

			oppositeAuthorisationRequirement1.ValidateLocalCostAmount();
			oppositeAuthorisationRequirement2.ValidateLocalCostAmount();
			AssertNoErrors(oppositeAuthorisationRequirement1.LocalCostAmountInfo);
			AssertNoErrors(oppositeAuthorisationRequirement2.LocalCostAmountInfo);

			authorisationRequirement1.LocalCostAmount = 98;
			TestBizObj.ValidateLocalCostAmount();
			AssertNoErrors(TestBizObj.LocalCostAmountInfo);
			authorisationRequirement1.ValidateLocalCostAmount();
			authorisationRequirement2.ValidateLocalCostAmount();
			AssertNoErrors(authorisationRequirement1.LocalCostAmountInfo);
			AssertHasErrors(authorisationRequirement2.LocalCostAmountInfo);

			oppositeAuthorisationRequirement1.ValidateLocalCostAmount();
			oppositeAuthorisationRequirement2.ValidateLocalCostAmount();
			AssertNoErrors(oppositeAuthorisationRequirement1.LocalCostAmountInfo);
			AssertNoErrors(oppositeAuthorisationRequirement2.LocalCostAmountInfo);

			authorisationRequirement2.LocalCostAmount = 99;
			TestBizObj.ValidateLocalCostAmount();
			AssertNoErrors(TestBizObj.LocalCostAmountInfo);
			authorisationRequirement1.ValidateLocalCostAmount();
			authorisationRequirement2.ValidateLocalCostAmount();
			AssertNoErrors(authorisationRequirement1.LocalCostAmountInfo);
			AssertNoErrors(authorisationRequirement2.LocalCostAmountInfo);
		}

		public void TestAmount()
		{
			BizObj = (RegistryBusinessObjectTemplate)GetNewBusinessObject();
			CostVarianceApproval costApproval = new CostVarianceApproval();
			costApproval.AuthorisationRequirements.Add(BizObj);

			costApproval.VarianceCalculationStyle = Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			AssertEquals(TestBizObj.AmountInfo, TestBizObj.LocalCostAmountInfo);
			AssertNotEquals(TestBizObj.AmountInfo, TestBizObj.PercentageInfo);
			TestBizObj.LocalCostAmount = 10M;
			AssertEquals(TestBizObj.LocalCostAmount, 10M);
			AssertEquals(TestBizObj.Amount, TestBizObj.LocalCostAmount);

			TestBizObj.Amount = 15M;
			AssertEquals(TestBizObj.Amount, 15M);
			AssertEquals(TestBizObj.Amount, TestBizObj.LocalCostAmount);

			costApproval.VarianceCalculationStyle = Constants.CostVarianceCalculationStyle.PercentageVariance;
			AssertEquals(TestBizObj.AmountInfo, TestBizObj.PercentageInfo);
			AssertNotEquals(TestBizObj.AmountInfo, TestBizObj.LocalCostAmountInfo);
			TestBizObj.Percentage = 10M;
			AssertEquals(TestBizObj.Percentage, 10M);
			AssertEquals(TestBizObj.Amount, TestBizObj.Percentage);

			TestBizObj.Amount = 15M;
			AssertEquals(TestBizObj.Amount, 15M);
			AssertEquals(TestBizObj.Amount, TestBizObj.Percentage);

			costApproval.VarianceCalculationStyle = Constants.CostVarianceCalculationStyle.PercentageVarianceAndMaximumLocalExTaxVariance;
			AssertEquals(TestBizObj.AmountInfo, TestBizObj.PercentageInfo);
			AssertNotEquals(TestBizObj.AmountInfo, TestBizObj.LocalCostAmountInfo);
			TestBizObj.Percentage = 10M;
			AssertEquals(TestBizObj.Percentage, 10M);
			AssertEquals(TestBizObj.Amount, TestBizObj.Percentage);

			TestBizObj.Amount = 15M;
			AssertEquals(TestBizObj.Amount, 15M);
			AssertEquals(TestBizObj.Amount, TestBizObj.Percentage);
		}

		public void TestTotalInvoiceVarianceAmount()
		{
			TestBizObj.MonitorTotalInvoiceVariance = true;
			TestBizObj.TotalInvoiceVarianceAmount = 50M;
			Assert("TotalInvoiceVarianceAmount Not read only", !TestBizObj.TotalInvoiceVarianceAmountInfo.ReadOnly);
			AssertEquals("TotalInvoiceVarianceAmount", 50M, TestBizObj.TotalInvoiceVarianceAmount);

			TestBizObj.MonitorTotalInvoiceVariance = false;
			Assert("TotalInvoiceVarianceAmount read only", TestBizObj.TotalInvoiceVarianceAmountInfo.ReadOnly);
			AssertEquals("TotalInvoiceVarianceAmount", 0M, TestBizObj.TotalInvoiceVarianceAmount);
		}

		public void TestValidateTotalInvoiceVarianceAmount()
		{
			BizObj = (RegistryBusinessObjectTemplate)GetNewBusinessObject();
			CostVarianceApproval costApproval = new CostVarianceApproval();
			costApproval.AuthorisationRequirements.Add(BizObj);
			TestBizObj.LocalCostAmount = 100;
			TestBizObj.MonitorTotalInvoiceVariance = true;

			TestBizObj.TotalInvoiceVarianceAmount = 100;
			AssertNoErrors(TestBizObj.TotalInvoiceVarianceAmountInfo);

			TestBizObj.TotalInvoiceVarianceAmount = 0;
			AssertHasError(TestBizObj.TotalInvoiceVarianceAmountInfo, "Total Invoice Variance Amount Can not be less than Amount");

			TestBizObj.MonitorTotalInvoiceVariance = false;
			costApproval.RunPreSaveValidation();
			AssertNoErrors(TestBizObj.TotalInvoiceVarianceAmountInfo);

			TestBizObj.Range = CostVarianceApprovalAuthorisationRequirement.RangeCodes.Above;
			TestBizObj.AuthorisationRequirement = CostVarianceApprovalAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			TestBizObj.MonitorTotalInvoiceVariance = true;
			TestBizObj.TotalInvoiceVarianceAmount = 100;

			var authorisationRequirement1 = (CostVarianceApprovalAuthorisationRequirement)GetNewBusinessObject();
			costApproval.AuthorisationRequirements.Add(authorisationRequirement1);
			authorisationRequirement1.Range = CostVarianceApprovalAuthorisationRequirement.RangeCodes.UpTo;
			authorisationRequirement1.AuthorisationRequirement = CostVarianceApprovalAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			authorisationRequirement1.MonitorTotalInvoiceVariance = true;
			authorisationRequirement1.TotalInvoiceVarianceAmount = 100;
			var oppositeAuthorisationRequirement1 = AddOppositeSignCopy(costApproval.AuthorisationRequirements, authorisationRequirement1);

			AssertNoExceptionThrown("Should be no exception", costApproval.RunPreSaveValidation);

			var authorisationRequirement2 = (CostVarianceApprovalAuthorisationRequirement)GetNewBusinessObject();
			costApproval.AuthorisationRequirements.Add(authorisationRequirement2);
			authorisationRequirement2.Range = CostVarianceApprovalAuthorisationRequirement.RangeCodes.UpTo;
			authorisationRequirement2.AuthorisationRequirement = CostVarianceApprovalAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			authorisationRequirement2.MonitorTotalInvoiceVariance = true;
			authorisationRequirement2.TotalInvoiceVarianceAmount = 100;
			var oppositeAuthorisationRequirement2 = AddOppositeSignCopy(costApproval.AuthorisationRequirements, authorisationRequirement2);

			costApproval.RunPreSaveValidation();
			AssertNoErrors(TestBizObj.TotalInvoiceVarianceAmountInfo);
			AssertHasError(authorisationRequirement1.TotalInvoiceVarianceAmountInfo, "Higher Total Invoice Variance Amount must have Authorization level higher than lower Total Invoice Variance Amount.");
			AssertHasError(authorisationRequirement2.TotalInvoiceVarianceAmountInfo, "Higher Total Invoice Variance Amount must have Authorization level higher than lower Total Invoice Variance Amount.");

			AssertHasError(oppositeAuthorisationRequirement1.TotalInvoiceVarianceAmountInfo, "Higher Total Invoice Variance Amount must have Authorization level higher than lower Total Invoice Variance Amount.");
			AssertHasError(oppositeAuthorisationRequirement2.TotalInvoiceVarianceAmountInfo, "Higher Total Invoice Variance Amount must have Authorization level higher than lower Total Invoice Variance Amount.");

			TestBizObj.MonitorTotalInvoiceVariance = false;
			authorisationRequirement2.MonitorTotalInvoiceVariance = false;

			costApproval.RunPreSaveValidation();
			AssertNoErrors(TestBizObj.TotalInvoiceVarianceAmountInfo);
			AssertNoErrors(authorisationRequirement1.TotalInvoiceVarianceAmountInfo);
			AssertNoErrors(authorisationRequirement2.TotalInvoiceVarianceAmountInfo);

			AssertHasError(oppositeAuthorisationRequirement1.TotalInvoiceVarianceAmountInfo, "Higher Total Invoice Variance Amount must have Authorization level higher than lower Total Invoice Variance Amount.");
			AssertHasError(oppositeAuthorisationRequirement2.TotalInvoiceVarianceAmountInfo, "Higher Total Invoice Variance Amount must have Authorization level higher than lower Total Invoice Variance Amount.");

			TestBizObj.MonitorTotalInvoiceVariance = true;
			authorisationRequirement2.MonitorTotalInvoiceVariance = true;
			authorisationRequirement2.TotalInvoiceVarianceAmount = 200;

			costApproval.RunPreSaveValidation();
			AssertHasError(TestBizObj.TotalInvoiceVarianceAmountInfo, "The Above Line's Total Invoice Variance Amount must be 200.");
			AssertNoErrors(authorisationRequirement1.TotalInvoiceVarianceAmountInfo);
			AssertNoErrors(authorisationRequirement2.TotalInvoiceVarianceAmountInfo);

			AssertHasError(oppositeAuthorisationRequirement1.TotalInvoiceVarianceAmountInfo, "Higher Total Invoice Variance Amount must have Authorization level higher than lower Total Invoice Variance Amount.");
			AssertHasError(oppositeAuthorisationRequirement2.TotalInvoiceVarianceAmountInfo, "Higher Total Invoice Variance Amount must have Authorization level higher than lower Total Invoice Variance Amount.");

			TestBizObj.TotalInvoiceVarianceAmount = 200;
			costApproval.RunPreSaveValidation();
			AssertNoErrors(TestBizObj.TotalInvoiceVarianceAmountInfo);
			AssertNoErrors(authorisationRequirement1.TotalInvoiceVarianceAmountInfo);
			AssertNoErrors(authorisationRequirement2.TotalInvoiceVarianceAmountInfo);

			AssertHasError(oppositeAuthorisationRequirement1.TotalInvoiceVarianceAmountInfo, "Higher Total Invoice Variance Amount must have Authorization level higher than lower Total Invoice Variance Amount.");
			AssertHasError(oppositeAuthorisationRequirement2.TotalInvoiceVarianceAmountInfo, "Higher Total Invoice Variance Amount must have Authorization level higher than lower Total Invoice Variance Amount.");
		}

		public void TestValidateMonitorTotalInvoiceVariance()
		{
			BizObj = (RegistryBusinessObjectTemplate)GetNewBusinessObject();
			CostVarianceApproval costApproval = new CostVarianceApproval();
			costApproval.AuthorisationRequirements.Add(BizObj);

			var authorisationRequirement1 = (CostVarianceApprovalAuthorisationRequirement)GetNewBusinessObject();
			costApproval.AuthorisationRequirements.Add(authorisationRequirement1);
			authorisationRequirement1.Range = CostVarianceApprovalAuthorisationRequirement.RangeCodes.UpTo;
			authorisationRequirement1.AuthorisationRequirement = CostVarianceApprovalAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			authorisationRequirement1.MonitorTotalInvoiceVariance = true;
			authorisationRequirement1.TotalInvoiceVarianceAmount = 100;
			var oppositeAuthorisationRequirement1 = AddOppositeSignCopy(costApproval.AuthorisationRequirements, authorisationRequirement1);

			var authorisationRequirement2 = (CostVarianceApprovalAuthorisationRequirement)GetNewBusinessObject();
			costApproval.AuthorisationRequirements.Add(authorisationRequirement2);
			authorisationRequirement2.Range = CostVarianceApprovalAuthorisationRequirement.RangeCodes.UpTo;
			authorisationRequirement2.AuthorisationRequirement = CostVarianceApprovalAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			authorisationRequirement2.MonitorTotalInvoiceVariance = false;
			authorisationRequirement2.TotalInvoiceVarianceAmount = 200;
			var oppositeAuthorisationRequirement2 = AddOppositeSignCopy(costApproval.AuthorisationRequirements, authorisationRequirement2);

			TestBizObj.Range = CostVarianceApprovalAuthorisationRequirement.RangeCodes.Above;
			TestBizObj.AuthorisationRequirement = CostVarianceApprovalAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			TestBizObj.MonitorTotalInvoiceVariance = false;
			TestBizObj.TotalInvoiceVarianceAmount = 200;

			costApproval.RunPreSaveValidation();
			AssertNoErrors(TestBizObj.MonitorTotalInvoiceVarianceInfo);
			AssertNoErrors(authorisationRequirement1.MonitorTotalInvoiceVarianceInfo);
			AssertNoErrors(authorisationRequirement2.MonitorTotalInvoiceVarianceInfo);

			TestBizObj.MonitorTotalInvoiceVariance = true;
			authorisationRequirement2.MonitorTotalInvoiceVariance = true;
			authorisationRequirement1.MonitorTotalInvoiceVariance = false;

			costApproval.RunPreSaveValidation();
			AssertHasError(authorisationRequirement1.MonitorTotalInvoiceVarianceInfo, "When lines with higher Authorization level ticked lines with lower should be ticked also.");
			AssertNoErrors(TestBizObj.MonitorTotalInvoiceVarianceInfo);
			AssertNoErrors(authorisationRequirement2.MonitorTotalInvoiceVarianceInfo);

			authorisationRequirement1.MonitorTotalInvoiceVariance = true;
			costApproval.RunPreSaveValidation();
			AssertNoErrors(TestBizObj.MonitorTotalInvoiceVarianceInfo);
			AssertNoErrors(authorisationRequirement1.MonitorTotalInvoiceVarianceInfo);
			AssertNoErrors(authorisationRequirement2.MonitorTotalInvoiceVarianceInfo);
		}

		public void TestValidateVarianceSign()
		{
			BizObj = (RegistryBusinessObjectTemplate)GetNewBusinessObject();
			CostVarianceApproval costApproval = new CostVarianceApproval();
			costApproval.AuthorisationRequirements.Add(BizObj);

			TestBizObj.VarianceSign = "";
			AssertHasErrors(TestBizObj.VarianceSignInfo);

			TestBizObj.VarianceSign = "*";
			AssertHasErrors(TestBizObj.VarianceSignInfo);

			TestBizObj.VarianceSign = VarianceSign + " ";
			AssertNoErrors(TestBizObj.VarianceSignInfo);

			try
			{
				TestBizObj.VarianceSign = " " + VarianceSign;
				Fail("Exception should be thrown");
			}
			catch (MaxLengthExceededException)
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			CostVarianceApprovalAuthorisationRequirement result = new CostVarianceApprovalAuthorisationRequirement();

			result.Range = result.RangeList[0].Code;
			result.Amount = 500;
			result.AuthorisationRequirement = result.AuthorisationRequirementList[0].Code;

			return result;
		}

		CostVarianceApprovalAuthorisationRequirement TestBizObj
		{
			get { return (CostVarianceApprovalAuthorisationRequirement)BizObj; }
		}

		protected override AmountBasedAuthorisationRequirementCollection GetAuthorisationRequirementCollection()
		{
			return new CostVarianceApprovalAuthorisationRequirementCollection(new CostVarianceApproval(), CurrentFallbackLevel, Factory);
		}

		protected abstract ZString VarianceSign { get; }

		protected ZString OppositeVarianceSign
		{
			get
			{
				return VarianceSign == CostVarianceApprovalAuthorisationRequirement.VarianceSigns.Plus ?
					CostVarianceApprovalAuthorisationRequirement.VarianceSigns.Minus :
					CostVarianceApprovalAuthorisationRequirement.VarianceSigns.Plus;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = GetNewBusinessObjectWithoutSignOverride();
			result.VarianceSign = VarianceSign;
			return result;
		}

		protected CostVarianceApprovalAuthorisationRequirement GetNewBusinessObjectWithoutSignOverride()
		{
			return (CostVarianceApprovalAuthorisationRequirement)base.GetNewBusinessObject();
		}

		protected CostVarianceApprovalAuthorisationRequirement AddOppositeSignCopy(CostVarianceApprovalAuthorisationRequirementCollection collection, CostVarianceApprovalAuthorisationRequirement requirement)
		{
			var result = collection.AddNew();
			result.VarianceSign = OppositeVarianceSign;

			result.Range = requirement.Range;
			result.AuthorisationRequirement = requirement.AuthorisationRequirement;
			result.Percentage = requirement.Percentage;
			result.Amount = requirement.Amount;
			result.LocalCostAmount = requirement.LocalCostAmount;
			result.MonitorTotalInvoiceVariance = requirement.MonitorTotalInvoiceVariance;
			result.TotalInvoiceVarianceAmount = requirement.TotalInvoiceVarianceAmount;

			return result;
		}

		#endregion
	}

	#endregion
}
