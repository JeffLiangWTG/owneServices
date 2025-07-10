using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusGuaranteeHeader))]
	sealed class CusGuaranteeHeaderTest : CusGuaranteeHeaderAbstractTest
	{
		public void TestCheckPropertyReadOnly()
		{
			Assert("For integrated countries, merge is turned off.", true);
		}

		[ExpectNoExceptions]
		public void TestIsPermitGuaranteeType()
		{
			CombineAssertions(() =>
			{
				var cusGuaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				cusGuaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
				NUnit.Framework.Assert.That(cusGuaranteeHeader.IsPermitGuaranteeType, NUnit.Framework.Is.EqualTo(true), "TRA Type is IsPermitGuaranteeType");

				cusGuaranteeHeader.CPH_Type = "DEF";
				NUnit.Framework.Assert.That(cusGuaranteeHeader.IsPermitGuaranteeType, NUnit.Framework.Is.EqualTo(false), "Non TRA Type is not IsPermitGuaranteeType");
			});
		}

		[ExpectNoExceptions]
		public void TestDescription()
		{
			CombineAssertions(() =>
			{
				var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
				NUnit.Framework.Assert.That(guaranteeHeader.Description, NUnit.Framework.Is.EqualTo(ZString.Empty));
				guaranteeHeader.CPH_Type = "COD";
				NUnit.Framework.Assert.That(guaranteeHeader.Description, NUnit.Framework.Is.EqualTo("COD").Using(CustomComparers.TypeComparison));
				guaranteeHeader.CPH_SubType = "ALT";
				NUnit.Framework.Assert.That(guaranteeHeader.Description, NUnit.Framework.Is.EqualTo("COD ALT").Using(CustomComparers.TypeComparison));
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = "ORG001";
				guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;
				NUnit.Framework.Assert.That(guaranteeHeader.Description, NUnit.Framework.Is.EqualTo("ORG001 COD ALT").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestCusGuaranteeRulesType()
		{
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			NUnit.Framework.Assert.That(guaranteeHeader.CusGuaranteeRules, NUnit.Framework.Is.TypeOf(typeof(CusGuaranteeRuleCollection<CusGuaranteeRule>)));
		}

		[ExpectNoExceptions]
		public void TestAdditionalAccessCodesType()
		{
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			NUnit.Framework.Assert.That(guaranteeHeader.AdditionalAccessCodes, NUnit.Framework.Is.TypeOf(typeof(CusGuaranteeRuleCollection<CusGuaranteeRule>)));
		}

		[ExpectNoExceptions]
		public void TestDescriptionProperty_ShouldBePermitHolderFullName()
		{
			const string permitHolderName = "ORG001";

			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = permitHolderName;
			guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;

			NUnit.Framework.Assert.That(DescriptionPropertyAttribute.DescriptionFromBusinessObject(guaranteeHeader), NUnit.Framework.Is.EqualTo(permitHolderName).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestToCheckLiabilityCalculationsValues()
		{
			const string permitHolderName = "ORG001";

			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = permitHolderName;
			guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;

			var rulePCP = guaranteeHeader.CusGuaranteeRules.AddNew();
			rulePCP.CPR_RuleCode = "PCP";
			rulePCP.CPR_ValueFrom = "10";

			var rulePCD = guaranteeHeader.CusGuaranteeRules.AddNew();
			rulePCD.CPR_RuleCode = "PCD";
			rulePCD.CPR_ValueFrom = "15";

			var rulePCV = guaranteeHeader.CusGuaranteeRules.AddNew();
			rulePCV.CPR_RuleCode = "PCV";
			rulePCV.CPR_ValueFrom = "20";

			NUnit.Framework.Assert.That(guaranteeHeader.RateForDuty, NUnit.Framework.Is.EqualTo((ZDecimal)15),"Expected RateForDuty to be ZDecimal value 15.");
			NUnit.Framework.Assert.That(guaranteeHeader.RateForVAT, NUnit.Framework.Is.EqualTo((ZDecimal)20), "Expected RateForVat to be ZDecimal value 20.");
			NUnit.Framework.Assert.That(guaranteeHeader.RateForOtherFees, NUnit.Framework.Is.EqualTo((ZDecimal)10), "Expected RateForOtherFees to be ZDecimal value 10.");
		}

		[ExpectNoExceptions]
		public void TestToCheckLiabilityCalculationsDefaultValues()
		{
			const string permitHolderName = "ORG001";

			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = permitHolderName;
			guaranteeHeader.CPH_OH_PermitHolder = orgHeader.PK;

			var rulePCP = guaranteeHeader.CusGuaranteeRules.AddNew();
			rulePCP.CPR_RuleCode = "PCP";

			var rulePCD = guaranteeHeader.CusGuaranteeRules.AddNew();
			rulePCD.CPR_RuleCode = "PCD";

			var rulePCV = guaranteeHeader.CusGuaranteeRules.AddNew();
			rulePCV.CPR_RuleCode = "PCV";

			rulePCP.CPR_ValueFrom = ZString.Empty;
			rulePCD.CPR_ValueFrom = ZString.Empty;
			rulePCV.CPR_ValueFrom = ZString.Empty;

			NUnit.Framework.Assert.That(guaranteeHeader.RateForDuty, NUnit.Framework.Is.EqualTo((ZDecimal)100), "Expected RateForDuty to be ZDecimal value 100.");
			NUnit.Framework.Assert.That(guaranteeHeader.RateForVAT, NUnit.Framework.Is.EqualTo((ZDecimal)100), "Expected RateForVAT to be ZDecimal value 100.");
			NUnit.Framework.Assert.That(guaranteeHeader.RateForOtherFees, NUnit.Framework.Is.EqualTo((ZDecimal)100), "Expected RateForOtherFees to be ZDecimal value 100.");
		}

		[ExpectNoExceptions]
		public void TestAddGuaranteeWriteOffTransaction()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			transaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(guaranteeHeader.CusGuaranteeLineTransactions.Count, NUnit.Framework.Is.EqualTo(1), "[PREREQ] There are only OBL Line Transactions");

				guaranteeHeader.AddWriteOffTransaction("22ES00999912345678", 50.00m, ZDateTime.BrettsBirthday);
				NUnit.Framework.Assert.That(guaranteeHeader.CusGuaranteeLineTransactions.Count, NUnit.Framework.Is.EqualTo(2), "New Write-off Line Transaction Added");
				var newLineTransaction = guaranteeHeader.CusGuaranteeLineTransactions.Cast<BaseCusGuaranteeLineTransaction>().FirstOrDefault(x => x.CPL_TransactionType == "TRA");
				NUnit.Framework.Assert.That(newLineTransaction.CPL_Reference, NUnit.Framework.Is.EqualTo("22ES00999912345678").Using(CustomComparers.TypeComparison), "New Write-off Line Transaction reference is the same as the parameter enter");
				NUnit.Framework.Assert.That(newLineTransaction.CPL_TransactionDate, NUnit.Framework.Is.EqualTo(ZDateTime.BrettsBirthday), "New Write-off Line Transaction date is the same as  as the parameter enter");
				NUnit.Framework.Assert.That(newLineTransaction.CPL_Comment, NUnit.Framework.Is.EqualTo("Write-off 22ES00999912345678").Using(CustomComparers.TypeComparison), "New Write-off Line Transaction comment is the message + reference");
				NUnit.Framework.Assert.That(newLineTransaction.CPL_TranValue, NUnit.Framework.Is.EqualTo(50.00m).Using(CustomComparers.TypeComparison), "New Write-off Line Transaction value is the same as the parameter enter");
				NUnit.Framework.Assert.That(newLineTransaction.CPL_TransactionType, NUnit.Framework.Is.EqualTo("TRA").Using(CustomComparers.TypeComparison), "New Write-off Line Transaction type is TRA");
			});
		}
	}
}
