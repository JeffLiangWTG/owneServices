using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Analyzer suggests BaseSupplementaryCode.Loader, which is less readible")]
	public class SupplementaryCodeValidationTest : CusCodeDataValidationTest
	{
		public void TestCheckSupplementaryCode()
		{
			var classification = Factory.New<CusClassification>();
			classification.CC_EcSupplement1 = "AAA";
			AssertHasMessageError(classification.CC_EcSupplement1Info, SupplementaryCodeValidation.SupplementaryCodeLength(classification.CC_EcSupplement1Info));
			classification.CC_EcSupplement1 = "AAAAA";
			AssertHasMessageError(classification.CC_EcSupplement1Info, SupplementaryCodeValidation.SupplementaryCodeLength(classification.CC_EcSupplement1Info));
			classification.CC_EcSupplement1 = "AAAA";
			AssertNoMessageError(classification.CC_EcSupplement1Info, SupplementaryCodeValidation.SupplementaryCodeLength(classification.CC_EcSupplement1Info));

			AssertNoMessageError(classification.CC_EcSupplement1Info, SupplementaryCodeValidation.DuplicateSupplementaryCode(classification.CC_EcSupplement1Info));
			classification.CC_EcSupplement2 = "AAAA";
			AssertHasMessageError(classification.CC_EcSupplement2Info, SupplementaryCodeValidation.DuplicateSupplementaryCode(classification.CC_EcSupplement2Info));
			classification.CC_EcSupplement2 = "BBBB";
			AssertNoMessageError(classification.CC_EcSupplement2Info, SupplementaryCodeValidation.DuplicateSupplementaryCode(classification.CC_EcSupplement2Info));
			var supplement3 = classification.AdditionalSupplementaryCodes.AddNew();
			supplement3.CY_Code = "AAAA";
			AssertHasMessageError(supplement3.CY_CodeInfo, SupplementaryCodeValidation.DuplicateSupplementaryCode(supplement3.CY_CodeInfo));
			supplement3.CY_Code = "CCCC";
			AssertNoMessageError(supplement3.CY_CodeInfo, SupplementaryCodeValidation.DuplicateSupplementaryCode(supplement3.CY_CodeInfo));
		}

		[ExpectNoExceptions]
		public void TestUnderlying_CusCodeDataRow_MarkedForDeletion_GivesNoRedValidation()
		{
			var underlying_CusCodeDataRow = Factory.New<SupplementaryCode>();
			underlying_CusCodeDataRow.CY_Code = "";
			underlying_CusCodeDataRow.CY_Order = 1;
			var classification = Factory.New<CusClassification>();

			underlying_CusCodeDataRow.Parent = classification;
			var loader = new SupplementaryCode.Loader(Factory);
			var loadedCode = loader.Load<SupplementaryCode, CusClassification>(classification, 1);
			AssertHasError(loadedCode.CY_CodeInfo, MandatoryValidation.MustBeEnteredMessage(loadedCode.CY_CodeInfo.HumanReadableName));

			underlying_CusCodeDataRow.CY_Code = "AAAA";
			classification.CC_EcSupplement1 = ZString.Empty;
			NUnit.Framework.Assert.That(loadedCode?.IsDeleted, NUnit.Framework.Is.EqualTo(true), "IsDeleted");
			AssertNoError(loadedCode?.CY_CodeInfo, MandatoryValidation.MustBeEnteredMessage(loadedCode.CY_CodeInfo.HumanReadableName));
		}

		public void TestCheckCY_CodeWithMaxLength()
		{
			const string dtyTariffCode = "222";
			const string stdPreferenceCode = "STD";

			var lineMergerTestHelper = new LineMergerTestHelper(Factory);
			lineMergerTestHelper.CreateDataGrouping();
			lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode);

			var declaration = lineMergerTestHelper.CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode, Common.CustomsChargeTypeList.Codes.AdditionCharge, "");
			var invoice = declaration.InvoiceLines[0];

			var loader = new SupplementaryCode.Loader(Factory);
			var code = loader.LoadOrCreate<SupplementaryCode, JobComInvoiceLine>(invoice, 1);

			AssertNoExceptionThrown(() => code.CY_Code = new ZString('A', code.CY_CodeInfo.MaxLength));
		}
	}
}
