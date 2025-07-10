using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public class TransactionLineValidation_InnerTest : AccTransactionLinesValidationTest
	{
		public void TestVATRecoverable()
		{
			var testObjCreator = new TestObjectCreator(Factory);
			var apInvoice = Factory.New<APInvoice>();
			var line = (TransactionLine)apInvoice.Lines.AddNew();
			var testValidation = new TransactionLineValidation(line);

			line.AL_Calc_InputGSTVATRecoverablePercentage = -1m;
			AssertHasError(line.AL_Calc_InputGSTVATRecoverablePercentageInfo, "GST Recoverable % must be between 0 and 100.");

			line.AL_Calc_InputGSTVATRecoverablePercentage = 101m;
			AssertHasError(line.AL_Calc_InputGSTVATRecoverablePercentageInfo, "GST Recoverable % must be between 0 and 100.");

			line.AL_Calc_InputGSTVATRecoverablePercentage = 100m;
			AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);

			var expectedError = "GST Recoverable % must be 100% for all Charge Codes that are not Overheads and for all GL Accounts.";
			line.AL_AG = testObjCreator.GLHeader1.PK;
			line.AL_Calc_InputGSTVATRecoverablePercentage = 100m;
			AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);

			line.AL_Calc_InputGSTVATRecoverablePercentage = 90m;
			AssertHasError(line.AL_Calc_InputGSTVATRecoverablePercentageInfo, expectedError);

			testObjCreator.CC1.AC_ChargeType = Constants.ChargeType.Overhead;
			line.AL_AC = testObjCreator.CC1.PK;
			line.AL_Calc_InputGSTVATRecoverablePercentage = 80m;
			AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);

			line.AL_AT = testObjCreator.REV.PK;
			AssertHasError(line.AL_Calc_InputGSTVATRecoverablePercentageInfo, "GST Recoverable % must be 100% when line Tax type is 'RVS'.");

			line.AL_Calc_InputGSTVATRecoverablePercentage = 100m;
			AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);

			line.AL_AT = testObjCreator.GST1.PK;
			line.AL_Calc_InputGSTVATRecoverablePercentage = 70m;
			AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);

			testObjCreator.CC1.AC_ChargeType = Constants.ChargeType.Margin;
			testValidation.ValidateAL_InputGSTVATRecoverable();
			AssertHasError(line.AL_Calc_InputGSTVATRecoverablePercentageInfo, expectedError);

			line.AL_Calc_InputGSTVATRecoverablePercentage = 100m;
			AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);

			var directPayment = Factory.New<DirectPayment>();
			line = directPayment.Lines.AddNew();
			line.AL_Calc_InputGSTVATRecoverablePercentage = 90m;
			AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);

			line.AL_AG = testObjCreator.GLHeader1.PK;
			line.AL_Calc_InputGSTVATRecoverablePercentage = 80m;
			AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);

			line.AL_AT = testObjCreator.REV.PK;
			AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);
		}

		public void TestAL_OSTaxAmount()
		{
			var header = Factory.NewWithValidTestData<GLJournal>();
			var line = (GLJournalLine)header.Lines.AddNew();
			AssertNotNull("Precondition: line has transaction header", line.TransactionHeader);
			AssertEquals("Precondition: line transaction header is expected one", line.TransactionHeader.PK, header.PK);
			Assert("Precondition: IsTransactionInDatabaseReadOnly", !header.IsTransactionInDatabaseReadOnly);
			Assert("Precondition: lineValidation has correct type", line.Validation.GetType().IsSubclassOf(typeof(TransactionLineValidation)));

			line.AL_OSTaxAmount = 1000000000000000m;
			Assert(line.AL_OSTaxAmountInfo.HasErrors());
			line.AL_OSTaxAmount = 999999999999999.99m;
			Assert(!line.AL_OSTaxAmountInfo.HasErrors());

			line.AL_OSExTaxAmount = -100m;
			line.AL_OSTaxAmount = 10m;
			AssertHasError(line.AL_OSTaxAmountInfo, TransactionLineValidation.AmountAndTaxAmountMustHaveSameSign_ForTestOnly);

			line.AL_OSExTaxAmount = 100m;
			line.AL_OSTaxAmount = 10m;
			AssertEquals(false, line.AL_OSTaxAmountInfo.HasErrors());

			line.AL_OSExTaxAmount = 100m;
			line.AL_OSTaxAmount = -10m;
			AssertHasError(line.AL_OSTaxAmountInfo, TransactionLineValidation.AmountAndTaxAmountMustHaveSameSign_ForTestOnly);

			((INeedRow)line).Row.AcceptChanges(); // this makes 'isindatabase' return 'true'
			Assert("Precondition: lineValidation has correct type", line.Validation.GetType().IsSubclassOf(typeof(TransactionLineValidation)));
			line.AL_OSTaxAmount = -5m;
			AssertNoError(line.AL_OSTaxAmountInfo, TransactionLineValidation.AmountAndTaxAmountMustHaveSameSign_ForTestOnly);
		}

		public void TestCheckAL_GB()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			APInvoice aPInvoiceToTest = Factory.NewWithValidTestData<APInvoice>();
			TransactionLine line = aPInvoiceToTest.Lines.AddNew();
			TransactionLineValidation testValidation = new TransactionLineValidation(line);

			line.AL_GB = ZGuid.NewZGuid();
			testValidation.ValidateAL_GB();
			AssertHasErrors(line.AL_GBInfo);

			line.AL_GB = GlbBranch.CurrentBranch.PK;
			testValidation.ValidateAL_GB();
			AssertNoErrors(line.AL_GBInfo);

			line.AL_GB = creator.NonCurrentCompanyBranch.PK;
			testValidation.ValidateAL_GB();
			AssertHasErrors(line.AL_GBInfo);

			line.AL_GB = GlbBranch.CurrentBranch.PK;
			testValidation.ValidateAL_GB();
			AssertNoErrors(line.AL_GBInfo);
		}

		public virtual void TestCheckAL_GE()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			TransactionLine line = apInvoice.Lines.AddNew();
			TransactionLineValidation testValidation = new TransactionLineValidation(line);
			line.AL_AG = creator.GLHeader1.PK;

			line.AL_GE = ZGuid.NewZGuid();
			testValidation.ValidateAL_GE();
			AssertHasErrors(line.AL_GEInfo);

			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			testValidation.ValidateAL_GE();
			AssertNoErrors(line.AL_GEInfo);

			creator.NonCurrentDepartment.GE_IsActive = false;
			line.AL_GE = creator.NonCurrentDepartment.PK;
			testValidation.ValidateAL_GE();
			AssertHasErrors(line.AL_GEInfo);

			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			testValidation.ValidateAL_GE();
			AssertNoErrors(line.AL_GEInfo);
		}

		public void TestCheckAL_LineAmount()
		{
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.Lines.AddNew();
			var line = apInvoice.Lines[0];
			var testValidation = new TransactionLineValidation(line);

			var maximumAllowedLineAmount = 50M;
			var registryValue = new MaximumAllowedTransactionAmount();
			registryValue.MaximumAllowedHeaderAmount = 10000M;
			registryValue.MaximumAllowedLineAmount = maximumAllowedLineAmount;
			var registry = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount;

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				line.AL_LineAmount = 49M;
				testValidation.ValidateAL_LineAmount();

				Assert("Precondition: IsInDatabase is false", !line.IsInDatabase);
				Assert("Precondition: IsTransactionHeaderReversing is false", !line.IsTransactionHeaderReversing);
				Assert("Precondition: AL_LineAmount is less than registry setting.", Math.Abs(line.AL_LineAmount) < maximumAllowedLineAmount);
				AssertNoErrors(line.AL_LineAmountInfo);

				line.AL_LineAmount = 51M;
				testValidation.ValidateAL_LineAmount();

				Assert("Precondition: AL_LineAmount is greater than registry setting.", Math.Abs(line.AL_LineAmount) > maximumAllowedLineAmount);
				var expectedMessage = $"The transaction line amount exceed the maximum allowed amount {registryValue.MaximumAllowedLineAmount.ToString(GlbCompany.CurrentCompany.GetLocalDecimals())} which is defined in the '{registry.HumanReadableRegistryPath()}' registry.";

				AssertHasError(line.AL_LineAmountInfo, expectedMessage);

				var testObjectCreator = new TestObjectCreator(Factory);
				var reversedTransaction = (InvoicingBase)testObjectCreator.ReverseTransaction(apInvoice, out _);
				var reversedLine = reversedTransaction.Lines[0];
				var reversedLineValidation = new TransactionLineValidation(line);

				Assert("Precondition: IsTransactionHeaderReversing is true", reversedLine.IsTransactionHeaderReversing);

				reversedLine.AL_LineAmount = 52M;
				reversedLineValidation.ValidateAL_LineAmount();

				AssertNoErrors(reversedLine.AL_LineAmountInfo);

				var newFactory = new BusinessObjectFactory();
				var testObjectCreator1 = new TestObjectCreator(newFactory);
				var arInvoice1 = testObjectCreator1.CreateInvoiceWithLine(typeof(ARInvoice), "ARNV1", testObjectCreator1.AUD, 1m, 10m, 0m, 10m, 0m);
				var line1 = arInvoice1.Lines[0];
				var testValidation1 = new TransactionLineValidation(line1);

				newFactory.Save();

				Assert("Precondition: IsInDatabase is true", line1.IsInDatabase);
				Assert("Precondition: HasChanges is false", !line1.AL_LineAmountInfo.HasChanges);

				testValidation1.ValidateAL_LineAmount();
				AssertNoErrors(line1.AL_LineAmountInfo);

				line1.AL_LineAmount = 53M;
				Assert("Precondition: HasChanges is true", line1.AL_LineAmountInfo.HasChanges);

				testValidation1.ValidateAL_LineAmount();
				AssertHasError(line1.AL_LineAmountInfo, expectedMessage);
			}
		}

		public void TestCheckAL_GSTVAT()
		{
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			apInvoice.Lines.AddNew();
			var line = apInvoice.Lines[0];
			var testValidation = new TransactionLineValidation(line);

			var maximumAllowedHeaderAmount = 50M;
			var registryValue = new MaximumAllowedTransactionAmount();
			registryValue.MaximumAllowedHeaderAmount = 10000M;
			registryValue.MaximumAllowedLineAmount = maximumAllowedHeaderAmount;
			var registry = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount;

			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				line.AL_GSTVAT = 49M;
				testValidation.ValidateAL_GSTVAT();

				Assert("Precondition: IsTransactionHeaderReversing is false", !line.IsTransactionHeaderReversing);
				Assert("Precondition: AL_GSTVAT is less than registry setting.", Math.Abs(line.AL_GSTVAT) < maximumAllowedHeaderAmount);
				AssertNoErrors(line.AL_GSTVATInfo);

				line.AL_GSTVAT = 51M;
				testValidation.ValidateAL_GSTVAT();

				AssertNotEquals("Precondition: AL_GSTVAT is greater than registry setting.", Math.Abs(line.AL_GSTVAT) > maximumAllowedHeaderAmount);
				var expectedMessage = $"The transaction line amount exceed the maximum allowed amount {registryValue.MaximumAllowedLineAmount.ToString(GlbCompany.CurrentCompany.GetLocalDecimals())} which is defined in the '{registry.HumanReadableRegistryPath()}' registry.";

				AssertHasError(line.AL_GSTVATInfo, expectedMessage);

				var testObjectCreator = new TestObjectCreator(Factory);
				var reversedTransaction = (InvoicingBase)testObjectCreator.ReverseTransaction(apInvoice, out _);
				var reversedLine = reversedTransaction.Lines[0];
				var reversedLineValidation = new TransactionLineValidation(line);

				Assert("Precondition: IsTransactionHeaderReversing is true", reversedLine.IsTransactionHeaderReversing);

				reversedLine.AL_GSTVAT = 52M;
				reversedLineValidation.ValidateAL_GSTVAT();

				AssertNoErrors(reversedLine.AL_GSTVATInfo);

				var newFactory = new BusinessObjectFactory();
				var testObjectCreator1 = new TestObjectCreator(newFactory);
				var arInvoice1 = testObjectCreator1.CreateInvoiceWithLine(typeof(ARInvoice), "ARNV1", testObjectCreator1.AUD, 1m, 10m, 0m, 10m, 0m);
				var line1 = arInvoice1.Lines[0];
				var testValidation1 = new TransactionLineValidation(line1);

				newFactory.Save();

				Assert("Precondition: IsInDatabase is true", line1.IsInDatabase);
				Assert("Precondition: HasChanges is false", !line1.AL_GSTVATInfo.HasChanges);

				testValidation1.ValidateAL_GSTVAT();
				AssertNoErrors(line1.AL_GSTVATInfo);

				line1.AL_GSTVAT = 53M;
				Assert("Precondition: HasChanges is true", line1.AL_GSTVATInfo.HasChanges);

				testValidation1.ValidateAL_GSTVAT();
				AssertHasError(line1.AL_GSTVATInfo, expectedMessage);
			}
		}

		public void TestValidateAL_OSExTaxAmount()
		{
			var mockLine = Factory.NewMoq<TransactionLineTest.MockableTransactionLine>();

			TransactionLine line = mockLine.Object;
			line.AL_OSExTaxAmount = 0m;

			TransactionLineValidation validation = new TransactionLineValidation(line);

			mockLine.Setup(m => m.IsCommentCharge).Returns(false);
			validation.ValidateAL_OSExTaxAmount();
			AssertMandatoryValidationError(line.AL_OSExTaxAmountInfo, true);
			mockLine.VerifyAll();

			mockLine.Setup(m => m.IsCommentCharge).Returns(true);
			validation.ValidateAL_OSExTaxAmount();
			AssertMandatoryValidationError(line.AL_OSExTaxAmountInfo, false);
			mockLine.VerifyAll();
		}

		public void TestValidateAL_OSExTaxAmount_ValidDecimal()
		{
			APInvoice aRInvoiceToTest = Factory.NewWithValidTestData<APInvoice>();
			aRInvoiceToTest.Lines.AddNew();

			aRInvoiceToTest.Lines[0].AL_OSExTaxAmount = 1000000000000000m;
			Assert(aRInvoiceToTest.Lines[0].AL_OSExTaxAmountInfo.HasErrors());
			aRInvoiceToTest.Lines[0].AL_OSExTaxAmount = 999999999999999.99m;
			Assert(!aRInvoiceToTest.Lines[0].AL_OSExTaxAmountInfo.HasErrors());
		}

		public void TestValidateAL_OverseasTotal()
		{
			APInvoice testAPInvoice = Factory.NewWithValidTestData<APInvoice>();
			testAPInvoice.Lines.AddNew();
			var line = testAPInvoice.Lines[0];

			line.AL_OSExTaxAmount = 9.11M;
			line.AL_LocalTaxAmount = 1.73M;
			line.AL_OverseasTotal = 9.11M;

			TransactionLineValidation validation = new TransactionLineValidation(line);
			validation.ValidateAL_OverseasTotal();

			AssertHasError(line.AL_OverseasTotalInfo, "Overseas Total does not equal Overseas Amount + Overseas Tax.");
		}

		public void TestCheckAL_PostDateIfPosted()
		{
			var factory = new BusinessObjectFactory();
			APInvoice aRInvoiceToTest = factory.NewWithValidTestData<APInvoice>();
			var line = (InvoicingLineBase)aRInvoiceToTest.Lines.AddNew();
			line.AL_AG = new TestObjectCreator(factory).GLHeader1.PK;

			factory.Save();

			TransactionLineValidation testValidation = new TransactionLineValidation(aRInvoiceToTest.Lines[0]);
			testValidation.ValidateAL_PostDate();
			Assert(!aRInvoiceToTest.Lines[0].AL_PostDateInfo.HasErrors());
		}

		public void TestCheckAL_PostDate()
		{
			APInvoice aRInvoiceToTest = Factory.NewWithValidTestData<APInvoice>();
			var line = (InvoicingLineBase)aRInvoiceToTest.Lines.AddNew();
			line.AL_AG = new TestObjectCreator(Factory).GLHeader1.PK;

			TransactionLineValidation testValidation = new TransactionLineValidation(aRInvoiceToTest.Lines[0]);
			aRInvoiceToTest.Lines[0].AL_PostDate = ZDateTime.BrettsBirthday;
			testValidation.ValidateAL_PostDate();
			Assert(aRInvoiceToTest.Lines[0].AL_PostDateInfo.HasErrors());
		}

		public virtual void TestCheckAL_AC()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "My";
			chargeCode.AC_Desc = "My Charge Code";
			chargeCode.AC_ChargeType = "CST";
			chargeCode.AC_AG_CostAccount = testObjectCreator.GLHeader1.PK;

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionNum = "5";
			InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.AL_OSExTaxAmount = 100m;
			invoiceLine.AL_OSTaxAmount = 10m;

			invoiceLine.AL_AC = chargeCode.PK;
			TransactionLineValidation testValidation = new TransactionLineValidation(invoiceLine);
			testValidation.ValidateAL_AC();

			Assert("No errors expected on AL_AC.", !invoiceLine.AL_ACInfo.HasErrors());
			chargeCode.AC_AG_CostAccount = ZGuid.Empty;
			testValidation.ValidateAL_AC();

			Assert("Must Have the Error on AL_AC.", invoiceLine.AL_ACInfo.HasError(TransactionLine.GetInvalidChargeCodeError(invoiceLine.ChargeCode.AC_Code, invoiceLine.AL_LineType)));

			chargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			testValidation.ValidateAL_AC();
			Assert("No errors expected on AL_AC.", !invoiceLine.AL_ACInfo.HasErrors());

			invoiceLine.AL_AC = ZGuid.Empty;
			invoiceLine.AL_AG = ZGuid.Empty;
			testValidation.ValidateAL_AC();
			Assert("Must Have the Error on AL_AC.", invoiceLine.AL_ACInfo.HasError(TransactionLine.EmptyChargeCodeAndGLHeaderError));
		}

		public virtual void TestCheckAL_AG()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionNum = "5";
			InvoicingLineBase invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.AL_OSExTaxAmount = 100m;
			invoiceLine.AL_OSTaxAmount = 10m;

			invoiceLine.AL_AC = ZGuid.Empty;
			invoiceLine.AL_AG = testObjectCreator.GLHeader1.PK;
			TransactionLineValidation testValidation = new TransactionLineValidation(invoiceLine);
			testValidation.ValidateAL_AG();

			Assert("No errors expected on AL_AG.", !invoiceLine.AL_AGInfo.HasErrors());
			invoiceLine.AL_AG = ZGuid.Empty;
			testValidation.ValidateAL_AG();

			Assert("Must Have the Error on AL_AG.", invoiceLine.AL_AGInfo.HasError(TransactionLine.EmptyChargeCodeAndGLHeaderError));

			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_IsGlobal = false;
			var companyFilter = glHeader.CompanyFilters.AddNew();
			companyFilter.ACF_GC_Company = testObjectCreator.NonCurrentCompany.PK;
			invoiceLine.AL_AG = glHeader.PK;
			AssertHasError(invoiceLine.AL_AGInfo, "This GL Account cannot be used");

			companyFilter.ACF_GC_Company = GlbCompany.CurrentCompany.PK;
			testValidation.ValidateAL_AG();
			AssertNoError(invoiceLine.AL_AGInfo, "This GL Account cannot be used");
		}

		public void TestCheckAL_PlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var placeOfSupplyCode = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;
				var placeOfSupplyTypeCode = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(GlbCompany.CurrentCompany, placeOfSupplyCode);
				var apInvoice = Factory.New<APInvoice>();
				var line = (TransactionLine)apInvoice.Lines.AddNew();

				line.AL_PlaceOfSupplyType = placeOfSupplyTypeCode;
				AssertHasError("Place of Supply can not be empty when palce of supply is not empty", line.AL_PlaceOfSupplyInfo, "Please enter a Place of Supply.");

				line.AL_PlaceOfSupplyType = placeOfSupplyTypeCode;
				AssertNoErrors(line.AL_PlaceOfSupplyTypeInfo);
				line.AL_PlaceOfSupply = placeOfSupplyCode;
				AssertNoErrors(line.AL_PlaceOfSupplyInfo);

				line.AL_PlaceOfSupply = "ZZZ";
				AssertHasError(line.AL_PlaceOfSupplyInfo, "Enter a valid Place of Supply.");
			}
		}

		public void TestCheckAL_PlaceOfSupplyType()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var placeOfSupplyCode = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;
				var placeOfSupplyTypeCode = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(GlbCompany.CurrentCompany, placeOfSupplyCode);
				var apInvoice = Factory.New<APInvoice>();
				var line = (TransactionLine)apInvoice.Lines.AddNew();

				line.AL_PlaceOfSupply = placeOfSupplyCode;
				line.AL_PlaceOfSupplyType = string.Empty;
				AssertNoErrors(line.AL_PlaceOfSupplyInfo);
				AssertHasError("Place of Supply Type can not be empty when palce of supply is not empty", line.AL_PlaceOfSupplyTypeInfo, "Please enter a Place of Supply Type.");

				line.AL_PlaceOfSupplyType = placeOfSupplyTypeCode;
				AssertNoErrors(line.AL_PlaceOfSupplyTypeInfo);
				line.AL_PlaceOfSupply = placeOfSupplyCode;
				AssertNoErrors(line.AL_PlaceOfSupplyInfo);

				line.AL_PlaceOfSupplyType = "ZZZ";
				AssertHasError(line.AL_PlaceOfSupplyTypeInfo, "Enter a valid Place of Supply Type.");
			}
		}

		public virtual void TestCheckAL_SupplyType()
		{
			var expectedLackOfCodeErrorMessage = "Please enter a value.";
			var expectedLackOfCodeWarningMessage = "The Supply Type is not specified. Please check if a supply type is needed before posting.";
			var expectedInvalidCodeErrorMessage = "Enter a valid selection.";

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var apInvoice = Factory.New<ARInvoice>();
				var line = (TransactionLine)apInvoice.Lines.AddNew();

				line.AL_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.INA;
				line.AL_SupplyType = ZString.Empty;
				AssertHasWarning(line.AL_SupplyTypeInfo, expectedLackOfCodeWarningMessage);

				line.AL_SupplyType = "111";
				AssertHasError(line.AL_SupplyTypeInfo, expectedInvalidCodeErrorMessage);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var apInvoice = Factory.New<ARInvoice>();
				var line = (TransactionLine)apInvoice.Lines.AddNew();

				line.AL_SupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.INA;
				line.AL_SupplyType = ZString.Empty;
				AssertHasError(line.AL_SupplyTypeInfo, expectedLackOfCodeErrorMessage);

				line.AL_SupplyType = "111";
				AssertHasError(line.AL_SupplyTypeInfo, expectedInvalidCodeErrorMessage);
			}
		}

		public void TestValidateAL_GovtChargeCode()
		{
			var expectedErrorMessage = "Please enter a value.";
			var expectedWarningMessage = "Government Charge Code is empty.";

			var testObjectCreator = new TestObjectCreator(Factory);

			foreach (var enableGovtChargeCode in new[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableGovtChargeCode))
				{
					APInvoice invoiceToTest = Factory.NewWithValidTestData<APInvoice>();
					invoiceToTest.Lines.AddNew();

					TransactionLineValidation testValidation = new TransactionLineValidation(invoiceToTest.Lines[0]);
					var line = invoiceToTest.Lines[0];

					//setup line
					line.AL_AC = testObjectCreator.CC1.PK;
					line.AL_AT = testObjectCreator.GST1.PK;

					line.AL_GovtChargeCode = "GVTCC1";

					testValidation.ValidateAL_GovtChargeCode();
					AssertNoErrors(line.AL_GovtChargeCodeInfo);

					line.AL_GovtChargeCode = "";
					if (enableGovtChargeCode)
					{
						AssertHasError(line.AL_GovtChargeCodeInfo, expectedErrorMessage);

						var previousChargeCodePk = line.AL_AC;
						var previousTaxId = line.AL_AT;
						line.AL_AC = CommentChargeCode.PK;
						AssertNoError("field is optional if linked to comment charge code", line.AL_GovtChargeCodeInfo, expectedErrorMessage);
						AssertHasWarning(line.AL_GovtChargeCodeInfo, expectedWarningMessage);

						line.AL_AC = previousChargeCodePk; //reverting to previous state
						line.AL_AT = previousTaxId;
						AssertHasError(line.AL_GovtChargeCodeInfo, expectedErrorMessage);

						line.AL_AT = ZGuid.Empty;
						AssertNoError("field is optional if no tax id", line.AL_GovtChargeCodeInfo, expectedErrorMessage);
						AssertHasWarning(line.AL_GovtChargeCodeInfo, expectedWarningMessage);

						line.AL_AT = previousTaxId; //reverting to previous state
						AssertHasError(line.AL_GovtChargeCodeInfo, expectedErrorMessage);

						using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
						{
							line.AL_AT = testObjectCreator.ServiceTax.PK;
							AssertNoError("field is optional if tax id is India Service tax", line.AL_GovtChargeCodeInfo, expectedErrorMessage);
							AssertHasWarning(line.AL_GovtChargeCodeInfo, expectedWarningMessage);

							line.AL_AT = previousTaxId; //reverting to previous state
							AssertHasError(line.AL_GovtChargeCodeInfo, expectedErrorMessage);

							line.AL_AT = testObjectCreator.ExtraServiceTax.PK;
							AssertNoError("field is optional if tax id is India Service tax", line.AL_GovtChargeCodeInfo, expectedErrorMessage);
							AssertHasWarning(line.AL_GovtChargeCodeInfo, expectedWarningMessage);
						}
					}
					else
					{
						AssertNoError(line.AL_GovtChargeCodeInfo, expectedErrorMessage);
					}
				}
			}
		}

		protected AccChargeCode CommentChargeCode
		{
			get
			{
				if (fCommentChargeCode == null)
				{
					fCommentChargeCode = new TestObjectCreator(Factory).CreateChargeCode("CMT", "Comment", Constants.ChargeType.Comment, 0, null, null, "ALL");
				}

				return fCommentChargeCode;
			}
		}

		AccChargeCode fCommentChargeCode;

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
	}
}
