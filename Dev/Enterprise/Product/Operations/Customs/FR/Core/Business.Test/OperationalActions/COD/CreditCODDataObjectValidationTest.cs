using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using CusEntryLine = Enterprise.Customs.FR.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	public class CreditCODDataObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckCreditMethod()
		{
			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			item.Validation.ValidateCreditMethod();
			AssertHasError(item.CreditMethodInfo, "Please enter a Credit Method.");

			item.CreditMethod = "1";
			AssertNoError(item.CreditMethodInfo, "Please enter a Credit Method.");
			AssertHasError(item.CreditMethodInfo, "Enter a valid Credit Method.");

			item.CreditMethod = "CPL";
			AssertNoError(item.CreditMethodInfo, "Enter a valid Credit Method.");
		}

		public void TestCheckReleasingEntryReference_MandatoryCheck()
		{
			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			item.Validation.ValidateReleasingEntryReference();
			AssertHasErrorContaining(item.ReleasingEntryReferenceInfo, MandatoryValidation.MustBeEntered);

			item.ReleasingEntryReference = "XXX";
			AssertNoErrorContaining(item.ReleasingEntryReferenceInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckReleasingEntryReference_ValidityCheck()
		{
			_ = SetupDeclaration();

			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			item.ReleasingEntryReference = "BGM001";
			AssertNoErrorContaining(item.ReleasingEntryReferenceInfo, "Enter a Customs cleared (BAE) Releasing Entry Reference.");

			item.ReleasingEntryReference = "BGM002";
			AssertHasErrorContaining(item.ReleasingEntryReferenceInfo, "Enter a Customs cleared (BAE) Releasing Entry Reference.");
		}

		public void TestCheckReleasingEntryReference_DuplicationCheck()
		{
			var applicator = new FrCreditCODApplicator(Factory);
			var item1 = applicator.FrCreditCODItemApplicators.AddNew();
			item1.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			item1.ReleasingEntryReference = "REL0001";
			item1.PreviousEntryReference = "PRV0001";
			item1.PreviousEntryLineNo = 1;
			item1.Amount = 12m;

			var item2 = applicator.FrCreditCODItemApplicators.AddNew();
			item2.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			item2.ReleasingEntryReference = "REL0001";
			item2.PreviousEntryReference = "PRV0001";
			item2.PreviousEntryLineNo = 1;
			item2.Amount = 40m;
			item2.Validation.ValidateReleasingEntryReference();
			AssertHasError(item2.ReleasingEntryReferenceInfo, "There are duplicate combination of Credit Method, Releasing Entry No., Previous Entry No. and Previous Entry Line No.");

			var item3 = applicator.FrCreditCODItemApplicators.AddNew();
			item3.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			item3.ReleasingEntryReference = "REL0001";
			item3.PreviousEntryReference = "PRV0001";
			item3.PreviousEntryLineNo = 2; // different with item 1
			item3.Validation.ValidateReleasingEntryReference();
			AssertNoError(item3.ReleasingEntryReferenceInfo, "There are duplicate combination of Credit Method, Releasing Entry No., Previous Entry No. and Previous Entry Line No.");

			var item4 = applicator.FrCreditCODItemApplicators.AddNew();
			item4.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			item4.ReleasingEntryReference = "REL0001";
			item4.PreviousEntryReference = "PRV0002"; // different with item 1
			item4.PreviousEntryLineNo = 1;
			item4.Validation.ValidateReleasingEntryReference();
			AssertNoError(item4.ReleasingEntryReferenceInfo, "There are duplicate combination of Credit Method, Releasing Entry No., Previous Entry No. and Previous Entry Line No.");

			var item5 = applicator.FrCreditCODItemApplicators.AddNew();
			item5.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			item5.ReleasingEntryReference = "REL0002"; // different with item 1
			item5.PreviousEntryReference = "PRV0001";
			item5.PreviousEntryLineNo = 1;
			item5.Validation.ValidateReleasingEntryReference();
			AssertNoError(item5.ReleasingEntryReferenceInfo, "There are duplicate combination of Credit Method, Releasing Entry No., Previous Entry No. and Previous Entry Line No.");

			var item6 = applicator.FrCreditCODItemApplicators.AddNew();
			item6.CreditMethod = CreditMethodList.Codes.CreditPreviousEntryLine; // different with item 1
			item6.ReleasingEntryReference = "REL0002";
			item6.PreviousEntryReference = "PRV0001";
			item6.PreviousEntryLineNo = 1;
			item6.Validation.ValidateReleasingEntryReference();
			AssertNoError(item6.ReleasingEntryReferenceInfo, "There are duplicate combination of Credit Method, Releasing Entry No., Previous Entry No. and Previous Entry Line No.");
		}

		public void TestCheckReleasingEntryReference_ProcedureCheck()
		{
			SetUpProcedures();

			var (declaration, invoiceLine11, invoiceLine12, entryLine11, entryLine12) = SetupDeclaration();

			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			item.CreditMethod = CreditMethodList.Codes.CreditPreviousEntry;
			item.ReleasingEntryReference = "BGM001";
			AssertNoMessageError(item.ReleasingEntryReferenceInfo, "No procedure of this entry releases guarantee.");

			invoiceLine12.JI_Procedure = "7100000";
			item.Validation.ValidateReleasingEntryReference();
			AssertHasMessageError(item.ReleasingEntryReferenceInfo, "No procedure of this entry releases guarantee.");
		}

		public void TestCheckPreviousEntryReference_MandatoryCheck()
		{
			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			item.Validation.ValidatePreviousEntryReference();
			AssertHasErrorContaining(item.PreviousEntryReferenceInfo, MandatoryValidation.MustBeEntered);

			item.PreviousEntryReference = "XXX";
			AssertNoErrorContaining(item.PreviousEntryReferenceInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckPreviousEntryReference_ValidityCheck()
		{
			SetupDeclaration();

			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			item.PreviousEntryReference = "BGM001";
			AssertNoErrorContaining(item.PreviousEntryReferenceInfo, ListValidation.InvalidCodeError);

			item.PreviousEntryReference = "BGM002";
			AssertHasErrorContaining(item.PreviousEntryReferenceInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckPreviousEntryReference_ProcedureCheck()
		{
			SetUpProcedures();

			var (declaration, invoiceLine11, invoiceLine12, entryLine11, entryLine12) = SetupDeclaration();

			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			item.CreditMethod = CreditMethodList.Codes.CreditPreviousEntry;
			item.PreviousEntryReference = "BGM001";
			AssertNoMessageError(item.PreviousEntryReferenceInfo, "No procedure of this entry consumes guarantee.");

			invoiceLine11.JI_Procedure = "5171000";
			item.Validation.ValidatePreviousEntryReference();
			AssertHasMessageError(item.PreviousEntryReferenceInfo, "No procedure of this entry consumes guarantee.");
		}

		public void TestCheckPreviousEntryLineNo_ExistingCheck()
		{
			SetupDeclaration();

			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			item.CreditMethod = CreditMethodList.Codes.CreditPreviousEntryLine;
			item.PreviousEntryReference = "BGM001";
			item.PreviousEntryLineNo = 1;
			AssertNoErrorContaining(item.PreviousEntryLineNoInfo, "doesn't have line");

			item.PreviousEntryLineNo = 2;
			AssertNoErrorContaining(item.PreviousEntryLineNoInfo, "doesn't have line");

			item.PreviousEntryLineNo = 3;
			AssertHasErrorContaining(item.PreviousEntryLineNoInfo, "doesn't have line");
		}

		public void TestCheckPreviousEntryLineNo_ProcedureCheck()
		{
			SetUpProcedures();
			SetupDeclaration();

			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			item.CreditMethod = CreditMethodList.Codes.CreditPreviousEntryLine;
			item.PreviousEntryReference = "BGM001";
			item.PreviousEntryLineNo = 1;
			AssertNoMessageErrorContaining(item.PreviousEntryLineNoInfo, "doesn't consume guarantee");

			item.PreviousEntryLineNo = 2;
			AssertHasMessageErrorContaining(item.PreviousEntryLineNoInfo, "doesn't consume guarantee");
		}

		public void TestCheckAmount_MandatoryCheck()
		{
			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			item.Amount = 0;

			item.CreditMethod = CreditMethodList.Codes.CreditPreviousEntry;
			item.Validation.ValidateAmount();
			AssertNoErrorContaining(item.AmountInfo, MandatoryValidation.ValueCannotBeZero);

			item.CreditMethod = CreditMethodList.Codes.CreditPreviousEntryLine;
			item.Validation.ValidateAmount();
			AssertNoErrorContaining(item.AmountInfo, MandatoryValidation.ValueCannotBeZero);

			item.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			item.Validation.ValidateAmount();
			AssertHasErrorContaining(item.AmountInfo, MandatoryValidation.ValueCannotBeZero);

			item.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			item.Amount = 13m;
			item.Validation.ValidateAmount();
			AssertNoErrorContaining(item.AmountInfo, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckAmount_GuaranteeCheck()
		{
			SetUpProcedures();
			var (declaration, invoiceLine11, invoiceLine12, entryLine11, entryLine12) = SetupDeclaration();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = Core.Constants.CountryCodes.France;

			var dty = helper.CreateNewOrGetExistingRateType(currentCountry, Universal.Constants.RateTypes.Duty, "DTY");
			helper.LoadOrCreateNewCusRateCode(Factory, EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dty.PK);
			Factory.Save();

			var fee1 = entryLine11.Fees.AddNew();
			fee1.CF_ChargeAmount = 10;
			fee1.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			var fee2 = entryLine11.Fees.AddNew();
			fee2.CF_ChargeAmount = 20;
			fee2.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;

			var applicator = new FrCreditCODApplicator(Factory);
			var item = applicator.FrCreditCODItemApplicators.AddNew();
			item.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			item.PreviousEntryReference = "BGM001";
			item.PreviousEntryLineNo = 1;
			item.Amount = 20m;
			AssertNoMessageErrorContaining(item.AmountInfo, "Previous entry BGM001, line No. 1 has consumed €30, but you are going to release");

			item.Amount = 40m;
			AssertHasMessageErrorContaining(item.AmountInfo, "Previous entry BGM001, line No. 1 has consumed €30, but you are going to release");
		}

		void SetUpProcedures()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var inwardProcedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "71", "00", "000", "", "IMP", "71P");
			inwardProcedure.ZZ6_IsGuaranteeReleased = "N";
			inwardProcedure.ZZ6_IsGuaranteeConsumed = "Y";
			var outwardProcedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "51", "71", "000", "", "IMP", "51P");
			outwardProcedure.ZZ6_IsGuaranteeReleased = "Y";
			outwardProcedure.ZZ6_IsGuaranteeConsumed = "N";

			Factory.Save();
		}

		(JobDeclaration declaration, JobComInvoiceLine invoiceLine11, JobComInvoiceLine invoiceLine12, CusEntryLine entryLine11, CusEntryLine entryLine12) SetupDeclaration()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice1 = declaration1.Invoices.AddNew();
			var invoiceLine11 = invoice1.InvoiceLines.AddNew();
			invoiceLine11.JI_Procedure = "7100000";
			var invoiceLine12 = invoice1.InvoiceLines.AddNew();
			invoiceLine12.JI_Procedure = "5171000";

			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			entryHeader1.CH_BGMReference = "BGM001";
			entryHeader1.EntryNumber = "ENT001";
			var entryLine11 = (CusEntryLine)entryHeader1.AllEntryLines.AddNew();
			entryLine11.CL_LineNumber = 1;
			invoiceLine11.JI_CL = entryLine11.PK;
			var entryLine12 = (CusEntryLine)entryHeader1.AllEntryLines.AddNew();
			entryLine12.CL_LineNumber = 2;
			invoiceLine12.JI_CL = entryLine12.PK;
			Factory.Save();

			return (declaration1, invoiceLine11, invoiceLine12, entryLine11, entryLine12);
		}
	}
}
