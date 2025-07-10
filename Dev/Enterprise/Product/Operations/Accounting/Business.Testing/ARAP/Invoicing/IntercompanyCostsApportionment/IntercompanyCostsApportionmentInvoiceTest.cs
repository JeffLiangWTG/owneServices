using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(IntercompanyCostsApportionmentInvoice))]
	public class IntercompanyCostsApportionmentInvoiceTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_internal ?? (TestObjectCreator_internal = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_internal;

		protected override void SetUp()
		{
			base.SetUp();

			testInvoice = new IntercompanyCostsApportionmentInvoice(Factory);
			intercompanyBranch = TestObjectCreator.LoadIntercompanyBranch();
			headerCurrentCompany = TestObjectCreator.CreateCurrentCompanyIntercompanyClearingGLHeader();
			headerIntercompany = TestObjectCreator.CreateIntercompanyIntercompanyClearingGLHeader();
			msg1 = TestObjectCreator.CreateTaxMsg("MSG1", "MSG1", "english msg1", "local msg1");

			var gst = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "GST").AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.Australia));
			if (gst != null)
			{
				gst.SetRate_ForTestOnly(10, 1);
			}

			Factory.Save();
		}

		protected IntercompanyCostsApportionmentInvoice testInvoice;
		protected AccountingPeriodTestHelper PeriodManagementTestHelper;
		protected GlbBranch intercompanyBranch;
		protected AccGLHeader headerCurrentCompany;
		protected AccGLHeader headerIntercompany;
		protected AccInvMsg msg1;

		#endregion

		#region Helpers

		string getErrorMessage(BusinessObject businessObject)
		{
			string errorMessage = string.Empty;
			foreach (INotification error in businessObject.GetErrors())
			{
				errorMessage += "\n\n" + error.Message;
			}
			return errorMessage;
		}

		void setupPeriodManagement(int year)
		{
			TestCaseHelper.ClearTable(AccPeriodManagement.Schema.TableName);
			PeriodManagementTestHelper = new AccountingPeriodTestHelper();
			PeriodManagementTestHelper.PostPeriodsForEntireYear(year, GlbCompany.CurrentCompany.PK, Enterprise.MasterFiles.Business.AccountingPeriodTestHelper.CalendarType.CalendarYear);
			PeriodManagementTestHelper.PostPeriodsForEntireYear(year, intercompanyBranch.GB_GC, Enterprise.MasterFiles.Business.AccountingPeriodTestHelper.CalendarType.CalendarYear);
		}

		void setupExchangeRate(ZDecimal rate)
		{
			RefExchangeRate exRate = Factory.New<RefExchangeRate>();
			exRate.RE_GC = intercompanyBranch.GB_GC;
			exRate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			exRate.RE_RX_NKExCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			exRate.RE_StartDate = new ZDateTime(2009, 1, 1);
			exRate.RE_ExpiryDate = new ZDateTime(2009, 12, 31);
			exRate.RE_SellRate = rate;
			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		#endregion

		#region Tests

		public void TestCalculateDueDateWithDocumentReceivedDate()
		{
			var currentDate = ZDateTime.Now;
			var invoice = new IntercompanyCostsApportionmentInvoice(Factory);
			invoice.AH_InvoiceTerm = Enterprise.Core.Constants.InvoiceTerms.CashOnDelivery;
			invoice.AH_InvoiceTermDays = 0;
			invoice.AH_InvoiceDate = currentDate.AddDays(-1);
			invoice.AH_DueDate = ZDateTime.Empty;

			AssertEquals(ZDateTime.Empty, invoice.AH_DueDate);

			invoice.AH_DocumentReceivedDate = currentDate.AddDays(1);
			AssertEquals(invoice.AH_InvoiceDate, invoice.AH_DueDate);
		}

		public void TestValidateDocumentReceivedDate()
		{
			var invoice = new IntercompanyCostsApportionmentInvoice(Factory);

			using (AccountingMasterFilesRegistry.Instance.DocumentReceivedDateMustBeEntered.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				invoice.AH_DocumentReceivedDate = ZDateTime.Empty;
				AssertNoErrorContaining(invoice.AH_DocumentReceivedDateInfo, MandatoryValidation.MustBeEntered);
			}

			using (AccountingMasterFilesRegistry.Instance.DocumentReceivedDateMustBeEntered.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				invoice.AH_DocumentReceivedDate = ZDateTime.Empty;
				AssertHasErrorContaining(invoice.AH_DocumentReceivedDateInfo, MandatoryValidation.MustBeEntered);

				invoice.AH_DocumentReceivedDate = new ZDateTime(2020, 01, 01);
				AssertNoErrorContaining(invoice.AH_DocumentReceivedDateInfo, MandatoryValidation.MustBeEntered);
			}
		}

		public void TestSetDefaultDocumentReceivedDate()
		{
			var invoice = new IntercompanyCostsApportionmentInvoice(Factory);
			AssertEquals(ZDateTime.Empty, invoice.AH_DocumentReceivedDate);

			invoice.InvoiceDate = ZDateTime.Empty;
			using (AccountingMasterFilesRegistry.Instance.DocumentReceivedDateDefaultingLogic.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.CreateDate))
			{
				invoice.InvoiceDate = ZDateTime.Today.AddDays(-1);
				AssertEquals(ZDateTime.Now.Date, invoice.AH_DocumentReceivedDate.Date);

				invoice.AH_DocumentReceivedDate = ZDateTime.Today.AddDays(1);
				invoice.InvoiceDate = ZDateTime.Today.AddDays(-2);
				AssertEquals(ZDateTime.Today.AddDays(1), invoice.AH_DocumentReceivedDate);
			}

			invoice.InvoiceDate = ZDateTime.Empty;
			using (AccountingMasterFilesRegistry.Instance.DocumentReceivedDateDefaultingLogic.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.InvoiceDate))
			{
				invoice.InvoiceDate = ZDateTime.Today.AddDays(-1);
				AssertEquals(invoice.AH_InvoiceDate, invoice.AH_DocumentReceivedDate);
			}
		}

		public void TestValidateNumberWhenPayablePreventCreationOfCreditNotes()
		{
			IntercompanyCostsApportionmentInvoice invoice;

			CreateInvoice(-500);
			AssertValidation(true, true, typeof(APCreditNote));
			AssertValidation(false, false, typeof(APCreditNote));

			CreateInvoice(-500, 499);
			AssertValidation(true, true, typeof(APCreditNote));
			AssertValidation(false, false, typeof(APCreditNote));

			CreateInvoice(-500, 500);
			AssertValidation(true, false, typeof(APInvoice));
			AssertValidation(false, false, typeof(APInvoice));

			CreateInvoice(-500, 501);
			AssertValidation(true, false, typeof(APInvoice));
			AssertValidation(false, false, typeof(APInvoice));

			CreateInvoice(500);
			AssertValidation(true, false, typeof(APInvoice));
			AssertValidation(false, false, typeof(APInvoice));

			void CreateInvoice(params decimal[] lineAmounts)
			{
				invoice = new IntercompanyCostsApportionmentInvoice(Factory);
				invoice.InvoiceNumber = "001";
				foreach (var amount in lineAmounts)
				{
					invoice.Lines.AddNew().Amount = amount;
				}
			}

			void AssertValidation(bool preventCreation, bool isErrorExpected, Type expectedTransactionType)
			{
				AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, preventCreation);
				var transactionGoingToBePosted = invoice.CreateBusinessObjectsForPosting();
				AssertType("transactionGoingToBePosted", expectedTransactionType, transactionGoingToBePosted);

				invoice.RunPreSaveValidation();
				if (isErrorExpected)
				{
					AssertHasError(invoice.InvoiceNumberInfo, AccountingMasterFilesUtils.APCreditNoteDisallowedMessage);
					AssertType<APCreditNote>("This error is allowed only when credit notes is expected to be posted.", transactionGoingToBePosted);
				}
				else
				{
					AssertNoErrors(invoice.InvoiceNumberInfo);
				}
			}
		}

		public void TestAH_IsDisbursementCalc()
		{
			foreach (string category in InvoiceTypeCalculationProvider.DisbursementInvoiceTypes)
			{
				testInvoice.TransactionCategory = category;
				AssertEquals(string.Format("Category {0} is Disbursement", category), InvoiceTypeCalculationProvider.IsDisbursementInvoiceType(category), testInvoice.AH_IsDisbursementCalc);
			}
			testInvoice.TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			AssertEquals("Non-disbursement", false, testInvoice.AH_IsDisbursementCalc);
		}

		public void TestDescriptionMaxLength()
		{
			AssertEquals(AccTransactionHeaderSchema.AH_Desc.MaxLength, testInvoice.DescriptionInfo.MaxLength);
		}

		[TestDate(2009, 09, 02)]
		public void TestCreateNewIntercompanyCostsApportionedCreditNote()
		{
			var taxRate = TestObjectCreator.CreateTaxRate("ABC", "Test Tax Rate", 10);
			var chargeCode = TestObjectCreator.CreateChargeCode("OVR1", "Test Charge Code", Core.Constants.ChargeType.Overhead, 1m, taxRate, null, GlbCompany.CurrentCompany);

			setupPeriodManagement(2009);
			TestObjectCreator.SetupIntercompanyClearingConfigurationRegistry(headerCurrentCompany, headerIntercompany, intercompanyBranch.GB_GC);
			setupExchangeRate(0.7);

			AccApportionmentTemplate template = TestObjectCreator.CreateIntercompanyApportionmentTemplate(intercompanyBranch.PK);

			testInvoice.InvoiceDate = new ZDateTime(2009, 06, 01);
			testInvoice.PostedDate = testInvoice.InvoiceDate;
			testInvoice.DueDate = testInvoice.InvoiceDate;

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(OrgHeaderSchema.OH_IsActive, ZBool.True);
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(query);
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			testInvoice.Creditor = orgHeader.PK;

			testInvoice.Currency = TestObjectCreator.USD.RX_Code;
			testInvoice.InvoiceNumber = "2222";
			testInvoice.ExchangeRate.Rate = 0.5;

			IntercompanyCostsApportionmentInvoiceLine line = testInvoice.Lines.AddNew();
			line.GenericCharge = chargeCode.PK;
			line.ApportionmentMethod = template.PK;
			line.Amount = -500.0m;
			line.AL_TaxDate = ZDate.Today.AddDays(-3);
			AssertNotEquals("AL_AT", ZGuid.Empty, line.AL_AT);

			testInvoice.RunPreSaveValidation();
			Assert(string.Format("Invoice has errors: {0}", getErrorMessage(testInvoice)), !testInvoice.HasErrors);
			Assert(string.Format("Charges line has errors: {0}", getErrorMessage(line)), !line.HasErrors);

			testInvoice.CreateBusinessObjectsForPosting();
			Factory.Save();

			APCreditNote apCreditNote = Factory.LoadTop1<APCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, testInvoice.InvoiceNumber));
			AssertIntercompanyAPCreditNote(template, apCreditNote);
			AssertIntercompanyGLJournal(template, apCreditNote, chargeCode);
		}

		[TestDate(2009, 09, 02)]
		public void TestCreateNewIntercompanyCostsApportionedInvoice()
		{
			var taxRate = TestObjectCreator.CreateTaxRate("ABC", "Test Tax Rate", 10);
			var chargeCode = TestObjectCreator.CreateChargeCode("OVR1", "Test Charge Code", Core.Constants.ChargeType.Overhead, 1m, taxRate, null, GlbCompany.CurrentCompany);

			setupPeriodManagement(2009);
			TestObjectCreator.SetupIntercompanyClearingConfigurationRegistry(headerCurrentCompany, headerIntercompany, intercompanyBranch.GB_GC);
			setupExchangeRate(0.7);
			AccApportionmentTemplate template = TestObjectCreator.CreateIntercompanyApportionmentTemplate(intercompanyBranch.PK);

			testInvoice.InvoiceDate = new ZDateTime(2009, 06, 01);
			testInvoice.PostedDate = testInvoice.InvoiceDate;
			testInvoice.DueDate = testInvoice.InvoiceDate;

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(OrgHeaderSchema.OH_IsActive, ZBool.True);
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(query);
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			testInvoice.Creditor = orgHeader.PK;

			testInvoice.Currency = TestObjectCreator.USD.RX_Code;
			testInvoice.InvoiceNumber = "2222";
			testInvoice.ExchangeRate.Rate = 0.5;

			IntercompanyCostsApportionmentInvoiceLine line = testInvoice.Lines.AddNew();
			line.GenericCharge = chargeCode.PK;
			line.ApportionmentMethod = template.PK;
			line.Amount = 500.0m;
			line.AL_GovtChargeCode = "GOVT1.2";
			line.AL_TaxDate = ZDate.Today.AddDays(-3);
			AssertNotEquals("AL_AT", ZGuid.Empty, line.AL_AT);

			testInvoice.RunPreSaveValidation();
			Assert(string.Format("Invoice has errors: {0}", getErrorMessage(testInvoice)), !testInvoice.HasErrors);
			Assert(string.Format("Charges line has errors: {0}", getErrorMessage(line)), !line.HasErrors);

			testInvoice.CreateBusinessObjectsForPosting();
			Factory.Save();

			APInvoice apInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, testInvoice.InvoiceNumber));
			AssertIntercompanyAPInvoice(template, apInvoice);
			AssertIntercompanyGLJournal(template, apInvoice, chargeCode);
		}

		[TestDate(2009, 08, 20)]
		public void TestCreateNewSameCompanyCostsApportionedInvoice()
		{
			setupPeriodManagement(2009);
			AccApportionmentTemplate template = TestObjectCreator.CreateSameCompanyApportionmentTemplate();

			testInvoice.InvoiceDate = new ZDateTime(2009, 06, 01);
			testInvoice.PostedDate = testInvoice.InvoiceDate;
			testInvoice.DueDate = testInvoice.InvoiceDate;

			ZDBOnlyQuery query1 = new ZDBOnlyQuery(typeof(OrgHeader));
			query1.AddToFilter(OrgHeaderSchema.OH_IsActive, ZBool.True);
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query1.AddSubQuery(subQuery, JoinCondition.And);
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(query1);
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			testInvoice.Creditor = orgHeader.PK;

			testInvoice.Currency = TestObjectCreator.USD.RX_Code;
			testInvoice.InvoiceNumber = "1111";
			testInvoice.ExchangeRate.Rate = 0.5;

			IntercompanyCostsApportionmentInvoiceLine line = testInvoice.Lines.AddNew();
			ZQuery query = new ZQuery(AccGLHeaderSchema.AG_IsActive, true);
			query.AddToFilter(AccGLHeaderSchema.AG_ControlAccount, false);
			query.AddToFilter(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.BalanceSheetAccount);
			AccGLHeader header = Factory.LoadTop1<AccGLHeader>(query);
			line.GenericCharge = header.PK;
			line.ApportionmentMethod = template.PK;
			line.Amount = 500.0m;
			query = new ZQuery(AccTaxRateSchema.AT_Type, AccTaxRate.Types.Rated);
			query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			line.AL_AT = Factory.Load<AccTaxRate>(query).First(x => x.GetRate_ForTestOnly() > 0).PK;
			line.AL_TaxDate = ZDate.Today.AddDays(-3);
			line.AL_A9_VATClass = msg1.PK;
			line.AL_GovtChargeCode = "GOVT1.1";

			testInvoice.RunPreSaveValidation();
			Assert(string.Format("Invoice has errors: {0}", getErrorMessage(testInvoice)), !testInvoice.HasErrors);
			Assert(string.Format("Charges line has errors: {0}", getErrorMessage(line)), !line.HasErrors);

			testInvoice.CreateBusinessObjectsForPosting();
			Factory.Save();
			testInvoice.RunPreSaveValidation();
			Assert(string.Format("Invoice has errors AFTER post: {0}", getErrorMessage(testInvoice)), !testInvoice.HasErrors);

			AssertSameCompanyAPInvoice(template);
		}

		void AssertSameCompanyAPInvoice(AccApportionmentTemplate template)
		{
			APInvoice apInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, testInvoice.InvoiceNumber));

			AssertInvoicingBase(template, apInvoice);

			AssertInvoicingBaseLine(template, apInvoice, 0, 0, 166.66m, 16.66m, 333.32m, 33.32m, "GOVT1.1");
			AssertInvoicingBaseLine(template, apInvoice, 0, 1, 166.66m, 16.67m, 333.32m, 33.34m, "GOVT1.1");
			AssertInvoicingBaseLine(template, apInvoice, 0, 2, 166.68m, 16.67m, 333.36m, 33.34m, "GOVT1.1");
		}

		void AssertIntercompanyAPInvoice(AccApportionmentTemplate template, APInvoice apInvoice)
		{
			AssertInvoicingBase(template, apInvoice);

			AssertInvoicingBaseLine(template, apInvoice, 0, 0, 166.67m, 16.66m, 333.34m, 33.32m, "GOVT1.2");
			AssertInvoicingBaseLine(template, apInvoice, 0, 1, 166.66m, 16.67m, 333.32m, 33.34m, "GOVT1.2");
			AssertInvoicingBaseLine(template, apInvoice, 0, 2, 166.67m, 16.67m, 333.34m, 33.34m, "GOVT1.2"); //intercompany clearing line
		}

		void AssertIntercompanyAPCreditNote(AccApportionmentTemplate template, APCreditNote apCreditNote)
		{
			AssertInvoicingBase(template, apCreditNote);

			AssertInvoicingBaseLine(template, apCreditNote, 0, 0, 166.66m, 16.67m, 333.32m, 33.34m);
			AssertInvoicingBaseLine(template, apCreditNote, 0, 1, 166.66m, 16.67m, 333.32m, 33.34m);
			AssertInvoicingBaseLine(template, apCreditNote, 0, 2, 166.68m, 16.66m, 333.36m, 33.32m);  //intercompany clearing line
		}

		void AssertInvoicingBase(AccApportionmentTemplate template, InvoicingBase apInvoice)
		{
			AssertNotNull("InvoicingBase", apInvoice);
			Assert("InvoicingBase IsInDatabase", apInvoice.IsInDatabase);
			AssertEquals("InvoicingBase Lines Count", 3, apInvoice.Lines.Count);
			AssertEquals("InvoicingBase AH_OSExTaxAmount", 500.0m, apInvoice.AH_OSExTaxAmount);
			AssertEquals("InvoicingBase AH_OH", testInvoice.Creditor, apInvoice.AH_OH);
			AssertEquals("InvoicingBase AH_TransactionNum", testInvoice.InvoiceNumber, apInvoice.AH_TransactionNum);
			AssertEquals("InvoicingBase AH_Desc", testInvoice.Description, apInvoice.AH_Desc);
			AssertEquals("InvoicingBase AH_PostDate", testInvoice.PostedDate, apInvoice.AH_PostDate);
			AssertEquals("InvoicingBase AH_InvoiceDate", testInvoice.InvoiceDate, apInvoice.AH_InvoiceDate);
			AssertEquals("InvoicingBase AH_DueDate", testInvoice.DueDate, apInvoice.AH_DueDate);
			AssertEquals("InvoicingBase AH_TransactionCategory", testInvoice.TransactionCategory, apInvoice.AH_TransactionCategory);
			AssertEquals("InvoicingBase AH_RX_NKTransactionCurrency", testInvoice.ExchangeRate.Currency, apInvoice.AH_RX_NKTransactionCurrency);
			AssertEquals("InvoicingBase AH_ExchangeRate", testInvoice.ExchangeRate.Rate, apInvoice.AH_ExchangeRate);
			AssertEquals("InvoicingBase AH_InvoiceTerm", testInvoice.AH_InvoiceTerm, apInvoice.AH_InvoiceTerm);
			AssertEquals("InvoicingBase AH_InvoiceTermDays", testInvoice.AH_InvoiceTermDays, apInvoice.AH_InvoiceTermDays);
			AssertEquals("InvoicingBase AH_GB", testInvoice.Branch, apInvoice.AH_GB);
			AssertEquals("InvoicingBase AH_GE", testInvoice.Department, apInvoice.AH_GE);
			AssertEquals("InvoicingBase AH_DocumentReceivedDate", testInvoice.DocumentReceivedDate, apInvoice.AH_DocumentReceivedDate);
		}

		void AssertInvoicingBaseLine(AccApportionmentTemplate template, InvoicingBase apInvoice, int testLineNumber, int lineNumber, ZDecimal oSExTaxAmount, ZDecimal oSTaxAmount, ZDecimal localExTaxAmount, ZDecimal localTaxAmount, string govtChargeCode = "")
		{
			GlbBranch branch = template.Lines[lineNumber].Branch;
			bool interCompany = branch.GB_GC != GlbCompany.CurrentCompany.PK;

			AssertEquals(string.Format("InvoicingBase Line {0} AL_OSExTaxAmount", lineNumber), oSExTaxAmount, apInvoice.Lines[lineNumber].AL_OSExTaxAmount);
			AssertEquals(string.Format("InvoicingBase Line {0} AL_LocalExTaxAmount", lineNumber), localExTaxAmount, apInvoice.Lines[lineNumber].AL_LocalExTaxAmount);
			AssertEquals(string.Format("InvoicingBase Line {0} AL_OSTaxAmount", lineNumber), oSTaxAmount, apInvoice.Lines[lineNumber].AL_OSTaxAmount);
			AssertEquals(string.Format("InvoicingBase Line {0} AL_LocalTaxAmount", lineNumber), localTaxAmount, apInvoice.Lines[lineNumber].AL_LocalTaxAmount);
			if (!interCompany)
			{
				AssertNotEquals(string.Format("InvoicingBase Line {0} GenericCharge", lineNumber), ZGuid.Empty, apInvoice.Lines[lineNumber].GenericCharge);
				AssertEquals(string.Format("InvoicingBase Line {0} GenericCharge", lineNumber), testInvoice.Lines[testLineNumber].GenericCharge, apInvoice.Lines[lineNumber].GenericCharge);
				AssertEquals(string.Format("InvoicingBase Line {0} AL_GB", lineNumber), template.Lines[lineNumber].Y0_GB, apInvoice.Lines[lineNumber].AL_GB);
				AssertEquals(string.Format("InvoicingBase Line {0} AL_GE", lineNumber), template.Lines[lineNumber].Y0_GE, apInvoice.Lines[lineNumber].AL_GE);
				AssertEquals(string.Format("InvoicingBase Line {0} AL_Desc", lineNumber), testInvoice.Lines[testLineNumber].Description + " " + template.Lines[lineNumber].Y0_Description, apInvoice.Lines[lineNumber].AL_Desc);
			}
			else
			{
				AssertNotEquals(string.Format("InvoicingBase Line {0} AL_AG", lineNumber), ZGuid.Empty, apInvoice.Lines[lineNumber].AL_AG);
				AssertEquals(string.Format("InvoicingBase Line {0} AL_GB", lineNumber), testInvoice.Lines[testLineNumber].Branch, apInvoice.Lines[lineNumber].AL_GB);
				AssertEquals(string.Format("InvoicingBase Line {0} AL_GE", lineNumber), testInvoice.Lines[testLineNumber].AL_GE, apInvoice.Lines[lineNumber].AL_GE);
				AssertEquals(string.Format("InvoicingBase Lines {0} AL_Desc", lineNumber), testInvoice.Lines[testLineNumber].Description, apInvoice.Lines[lineNumber].AL_Desc);
			}
			AssertEquals(string.Format("InvoicingBase Line {0} AL_AT", lineNumber), testInvoice.Lines[testLineNumber].AL_AT, apInvoice.Lines[lineNumber].AL_AT);
			AssertEquals(string.Format("InvoicingBase Line {0} AL_TaxDate", lineNumber), testInvoice.Lines[testLineNumber].AL_TaxDate, apInvoice.Lines[lineNumber].AL_TaxDate);
			AssertEquals(string.Format("InvoicingBase Line {0} AL_A9_VATClass", lineNumber), testInvoice.Lines[testLineNumber].AL_A9_VATClass, apInvoice.Lines[lineNumber].AL_A9_VATClass);
			AssertEquals(string.Format("InvoicingBase Line {0} AL_GovtChargeCode", lineNumber), govtChargeCode, apInvoice.Lines[lineNumber].AL_GovtChargeCode);
		}

		void AssertIntercompanyGLJournal(AccApportionmentTemplate template, InvoicingBase invoiceBase, AccChargeCode chargeCode)
		{
			AssertEquals("Journals contains exactly one value", 1, testInvoice.Journals.Count);
			GLJournal journal = Factory.Load<GLJournal>(testInvoice.Journals[template.Lines[2].Company].PK);
			AssertNotNull("GLJournal", journal);
			Assert("GLJournal IsInDatabase", journal.IsInDatabase);
			string description = "GENERAL LEDGER JOURNAL";
			AssertEquals("GLJournal AH_Desc", description, journal.AH_Desc);
			AssertEquals("GLJournal AH_GB", template.Lines[2].Y0_GB, journal.AH_GB);
			AssertEquals("GLJournal AH_GE", template.Lines[2].Y0_GE, journal.AH_GE);
			AssertEquals("GLJournal AH_TransactionType", TransactionTypes.GLStandardJournal, journal.AH_TransactionType);
			AssertEquals("GLJournal PostPeriod", 200906, journal.PostPeriod);

			AssertGLJournalLines(journal, template, 0, 2, 0, 163.34m, chargeCode.AC_AG_CostAccount, headerCurrentCompany.PK);
			AssertGLJournalLines(journal, template, 2, 3, 0, 70.00m, chargeCode.AC_AG_CostAccount, headerCurrentCompany.PK);
		}

		void AssertGLJournalLines(GLJournal journal, AccApportionmentTemplate template, int lineNumber, int templateLineNumber, int testInvoiceLineNumber, ZDecimal unsignedLineAmount, ZGuid costGLAccount, ZGuid companyPostToGLAccount)
		{
			GLJournalLine debitLine = (GLJournalLine)journal.Lines[lineNumber];
			ZString debitLineSign = testInvoice.Lines[testInvoiceLineNumber].Amount >= 0 ? nameof(DebitCredit.DR) : nameof(DebitCredit.CR);
			AssertEquals(string.Format("GLJournalLine AL_AG Line {0}", lineNumber), costGLAccount, debitLine.AL_AG);
			AssertEquals(string.Format("GLJournalLine AL_GB", lineNumber), template.Lines[templateLineNumber].Y0_GB, debitLine.AL_GB);
			AssertEquals(string.Format("GLJournalLine AL_GE", lineNumber), template.Lines[templateLineNumber].Y0_GE, debitLine.AL_GE);
			AssertEquals(string.Format("GLJournalLine DebitCreditSign", lineNumber), debitLineSign, debitLine.DebitCreditSign);
			AssertEquals(string.Format("GLJournalLine UnsignedLineAmount", lineNumber), unsignedLineAmount, debitLine.UnsignedOSLineAmount);
			GLJournalLine creditLine = (GLJournalLine)journal.Lines[lineNumber + 1];
			ZString creditLineSign = testInvoice.Lines[testInvoiceLineNumber].Amount >= 0 ? nameof(DebitCredit.CR) : nameof(DebitCredit.DR);
			AssertEquals(string.Format("GLJournalLine AL_AG Line {0}", lineNumber + 1), companyPostToGLAccount, creditLine.AL_AG);
			AssertEquals(string.Format("GLJournalLine AL_GB", lineNumber + 1), template.Lines[templateLineNumber].Y0_GB, creditLine.AL_GB);
			AssertEquals(string.Format("GLJournalLine AL_GE", lineNumber + 1), template.Lines[templateLineNumber].Y0_GE, creditLine.AL_GE);
			AssertEquals(string.Format("GLJournalLine DebitCreditSign", lineNumber + 1), creditLineSign, creditLine.DebitCreditSign);
			AssertEquals(string.Format("GLJournalLine UnsignedLineAmount", lineNumber + 1), unsignedLineAmount, creditLine.UnsignedOSLineAmount);
		}

		public void TestTaxBranch()
		{
			var currentBranchPK = GlbBranch.CurrentBranch.PK;
			testInvoice.TaxBranch = currentBranchPK;

			AssertEquals(currentBranchPK, testInvoice.TaxBranch);
			AssertNotNull(testInvoice.TaxBranchInfo);
			AssertEquals(GlbCompany.CurrentCompany.Branches.Count(x => x.GB_IsActive = true), testInvoice.Branches.Count);
		}

		public void TestTaxBranch_ReadOnly()
		{
			using (TestObjectCreator.SetUpTaxBranchRegistry(true))
			{
				TestObjectCreator.ResetSecurityCore();

				AssertTaxBranchReadOnly(true, true);
				AssertTaxBranchReadOnly(false, true);
				AssertTaxBranchReadOnly(true, false);
				AssertTaxBranchReadOnly(false, false);

				void AssertTaxBranchReadOnly(bool isTaxApplicable, bool isSecurityAllowed)
				{
					Env.Security.NewPayablesOverrideTaxBranchAllows.IsAllowed = isSecurityAllowed;
					testInvoice.Creditor = ZGuid.Empty;
					testInvoice.TaxBranch = GlbBranch.CurrentBranch.PK;
					TestObjectCreator.TestOrganisation.CompanyData.SetAPTaxApplicable(isTaxApplicable);
					testInvoice.Creditor = TestObjectCreator.TestOrganisation.PK;

					var expectedReadOnly = !isTaxApplicable || !isSecurityAllowed;

					AssertEquals(expectedReadOnly, testInvoice.TaxBranchInfo.ReadOnly);
				}
			}
		}

		public void TestTaxBranchWhenSetCreditor()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoice = new IntercompanyCostsApportionmentInvoice(Factory);

				invoice.Creditor = TestObjectCreator.AALSHI.PK;
				AssertEquals(GlbBranch.CurrentBranch.PK, invoice.TaxBranch);

				invoice = new IntercompanyCostsApportionmentInvoice(Factory);

				TestObjectCreator.AALSHI.CompanyData.SetAPTaxApplicable(false);
				invoice.Creditor = TestObjectCreator.AALSHI.PK;
				AssertEquals(ZGuid.Empty, invoice.TaxBranch);
			}
		}

		public void TestValidateTaxBranch()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoice = new IntercompanyCostsApportionmentInvoice(Factory);
				invoice.Creditor = TestObjectCreator.Creditor1.PK;

				invoice.TaxBranch = ZGuid.Empty;
				AssertHasError(invoice.TaxBranchInfo, "Please enter a value.");

				invoice.TaxBranch = ZGuid.Invalid;
				AssertHasError(invoice.TaxBranchInfo, "Enter a valid selection.");
			}
		}

		public void TestApplyTaxBranchOfInvoiceToInvoiceLine()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var invoice = new IntercompanyCostsApportionmentInvoice(Factory);
				var line = invoice.Lines.AddNew();

				invoice.TaxBranch = GlbBranch.CurrentBranch.PK;
				AssertEquals(GlbBranch.CurrentBranch.PK, line.TaxBranch);

				invoice.TaxBranch = TestObjectCreator.NonCurrentBranch.PK;
				AssertEquals(TestObjectCreator.NonCurrentBranch.PK, line.TaxBranch);
			}
		}

		[TestDate(2017, 01, 15)]
		public void TestAccountApportionmentRounding()
		{
			//SETUP PERIOD MANAGEMENT
			setupPeriodManagement(2017);
			Factory.Save();

			//CREATE APPORTIONMENT TEMPLATE IF DOES NOT EXIST YET
			ZQuery queryTemplate = new ZQuery(AccApportionmentTemplateSchema.A0_Description, "TEST APPORTIONMENT TEMPLATE");
			AccApportionmentTemplate template = Factory.LoadTop1<AccApportionmentTemplate>(queryTemplate);
			if (template == null)
			{
				template = Factory.New<AccApportionmentTemplate>();
				template.A0_GC = GlbCompany.CurrentCompany.PK;
				template.A0_Description = "TEST APPORTIONMENT TEMPLATE";
				template.A0_Notes = "Test Apportionment Notes";
			}

			//CREATE 3 BRANCHES
			List<GlbBranch> branches = new List<GlbBranch>();
			branches.Add(TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany, null));
			branches.Add(TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany, null));
			branches.Add(TestObjectCreator.CreateBranch("CCC", GlbCompany.CurrentCompany, null));

			//CREATE 8 DEPARTMENTS
			List<GlbDepartment> departments = new List<GlbDepartment>();
			departments.Add(TestObjectCreator.CreateDepartment("AAA"));
			departments.Add(TestObjectCreator.CreateDepartment("BBB"));
			departments.Add(TestObjectCreator.CreateDepartment("CCC"));
			departments.Add(TestObjectCreator.CreateDepartment("DDD"));
			departments.Add(TestObjectCreator.CreateDepartment("EEE"));
			departments.Add(TestObjectCreator.CreateDepartment("FFF"));
			departments.Add(TestObjectCreator.CreateDepartment("GGG"));
			departments.Add(TestObjectCreator.CreateDepartment("HHH"));

			//CREATE APPORTIONMENT TEMPLATE LINES
			for (int i = 0; i < 8; i++)
			{
				TestObjectCreator.CreateTemplateLine(template, "TEST APPORTIONMENT TEMPLATE LINE TYPE A - " + i, branches[0].PK, departments[i].PK, 6.250m);
			}
			for (int i = 0; i < 8; i++)
			{
				TestObjectCreator.CreateTemplateLine(template, "TEST APPORTIONMENT TEMPLATE LINE TYPE B -  " + i, branches[1].PK, departments[i].PK, 4.375m);
			}
			for (int i = 0; i < 8; i++)
			{
				TestObjectCreator.CreateTemplateLine(template, "TEST APPORTIONMENT TEMPLATE LINE TYPE C -  " + i, branches[2].PK, departments[i].PK, 1.875m);
			}

			//ASSIGN VALUES FOR INVOICE
			testInvoice.Creditor = TestObjectCreator.AALSHI.PK;
			testInvoice.InvoiceNumber = "TESTRND1";
			testInvoice.Currency = "HKD";// TestObjectCreator.HKD.RX_Code;
			testInvoice.ExchangeRate.Rate = 0.7546;

			//CREATE INVOICE LINE BASED ON TEMPLATE
			IntercompanyCostsApportionmentInvoiceLine line = testInvoice.Lines.AddNew();
			line.GenericCharge = TestObjectCreator.OverheadChargeCode.PK;
			line.ApportionmentMethod = template.PK;
			line.Amount = -2360.0m;
			line.Tax = TestObjectCreator.GSTFREE1.GetRate_ForTestOnly();
			testInvoice.Lines.Add(line);

			//PREPARE INVOICE FOR POSTING
			testInvoice.RunPreSaveValidation();
			Assert(string.Format("Invoice has errors: {0}", getErrorMessage(testInvoice)), !testInvoice.HasErrors);
			Assert(string.Format("Charges line has errors: {0}", getErrorMessage(line)), !line.HasErrors);

			//POST INVOICE
			testInvoice.CreateBusinessObjectsForPosting();
			Factory.Save();

			//FORCE FACTORY CACHE PURGE
			ReleaseFactory();

			//FORCE RELOAD VALUES FOR INVOICE
			ZQuery transactionHeaderQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "TESTRND1");
			transactionHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			APInvoice invoice = Factory.LoadTop1<APInvoice>(transactionHeaderQuery);

			//SELECT ALL THE TRANSACTION LINES FOR THIS HEADER
			AccTransactionLines[] transactionLines = invoice.Lines.Cast<AccTransactionLines>().ToArray();

			//SHOULD FIND 24 TRANSACTIONS
			AssertEquals("Number of Transaction Lines for Current Company should be 24", 24, transactionLines.Length);

			//INVOICE OSTOTALAMOUNT SHOULD BE AS EXPECTED
			AssertEquals("The OSAmount (Total in Foreign currency) should be 2360", 2360m, invoice.AH_OSTotal);

			//TRANSACTION OSAMOUNT SHOULD BE AS EXPECTED
			for (int i = 0; i < 8; i++)
			{
				AssertEquals("The OSAmount for apportionment number " + (i + 1) + " should be 147.50", 147.5m, transactionLines[i].AL_OSAmount);
			}

			for (int i = 8; i < 16; i++)
			{
				AssertEquals("The OSAmount for apportionment number " + (i + 1) + " should be 103.25", 103.25m, transactionLines[i].AL_OSAmount);
			}

			for (int i = 16; i < 24; i++)
			{
				AssertEquals("The OSAmount for apportionment number " + (i + 1) + " should be 44.25", 44.25m, transactionLines[i].AL_OSAmount);
			}

			//TRANSACTION LINEAMOUNT SHOULD BE AS EXPECTED
			for (int i = 0; i < 8; i++)
			{
				AssertEquals("The LineAmount for apportionment number " + (i + 1) + " should be 195.47", 195.47m, transactionLines[i].AL_LineAmount);
			}

			for (int i = 8; i < 16; i++)
			{
				AssertEquals("The LineAamount for apportionment number " + (i + 1) + " should be 136.83", 136.83m, transactionLines[i].AL_LineAmount);
			}

			for (int i = 16; i < 24; i++)
			{
				AssertEquals("The LineAmount for apportionment number " + (i + 1) + " should be 58.64", 58.64m, transactionLines[i].AL_LineAmount);
			}
		}

		[TestDate(2017, 06, 16)]
		public void TestDisallowMixtureOfBranches_WhenBranchLevelPostingIsEnabled()
		{
			var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);

			setupPeriodManagement(2009);

			AccApportionmentTemplate template = TestObjectCreator.CreateSameCompanyApportionmentTemplate();

			testInvoice.InvoiceDate = new ZDateTime(2009, 06, 01);
			testInvoice.PostedDate = testInvoice.InvoiceDate;
			testInvoice.DueDate = testInvoice.InvoiceDate;

			ZDBOnlyQuery query1 = new ZDBOnlyQuery(typeof(OrgHeader));
			query1.AddToFilter(OrgHeaderSchema.OH_IsActive, ZBool.True);
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query1.AddSubQuery(subQuery, JoinCondition.And);
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(query1);
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			testInvoice.Creditor = orgHeader.PK;

			testInvoice.Currency = TestObjectCreator.USD.RX_Code;
			testInvoice.InvoiceNumber = "1111";
			testInvoice.ExchangeRate.Rate = 0.5;

			IntercompanyCostsApportionmentInvoiceLine line1 = testInvoice.Lines.AddNew();
			ZQuery query = new ZQuery(AccGLHeaderSchema.AG_IsActive, true);
			query.AddToFilter(AccGLHeaderSchema.AG_ControlAccount, false);
			query.AddToFilter(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.BalanceSheetAccount);
			AccGLHeader header = Factory.LoadTop1<AccGLHeader>(query);
			line1.Branch = branch1.PK;
			line1.GenericCharge = header.PK;
			line1.ApportionmentMethod = template.PK;
			line1.Amount = 500.0m;
			query = new ZQuery(AccTaxRateSchema.AT_Type, AccTaxRate.Types.Rated);
			query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var taxRate = Factory.Load<AccTaxRate>(query).First(x => x.GetRate_ForTestOnly() > 0);
			line1.AL_AT = taxRate.PK;
			line1.AL_A9_VATClass = msg1.PK;

			IntercompanyCostsApportionmentInvoiceLine line2 = testInvoice.Lines.AddNew();
			line2.Branch = branch2.PK;
			line2.GenericCharge = header.PK;
			line2.ApportionmentMethod = template.PK;
			line2.Amount = 500.0m;
			line2.AL_AT = taxRate.PK;
			line2.AL_A9_VATClass = msg1.PK;

			testInvoice.RunPreSaveValidation();
			Assert(string.Format("Invoice has errors: {0}", getErrorMessage(testInvoice)), !testInvoice.HasErrors);
			Assert(string.Format("Charges line has errors: {0}", getErrorMessage(line1)), !line1.HasErrors);
			Assert(string.Format("Charges line has errors: {0}", getErrorMessage(line1)), !line2.HasErrors);

			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true });
			testInvoice.RunPreSaveValidation();

			Assert(string.Format("Invoice has errors: {0}", getErrorMessage(testInvoice)), testInvoice.HasErrors);
			Assert(string.Format("Charges line has errors: {0}", getErrorMessage(line1)), line1.HasErrors);
			Assert(string.Format("Charges line has errors: {0}", getErrorMessage(line1)), line2.HasErrors);

			var settings1 = new BranchGroupSettings();
			settings1.BranchPK = branch1.PK;
			settings1.GroupNumber = 1;
			settings1.IsParentBranch = false;

			var settings2 = new BranchGroupSettings();
			settings2.BranchPK = branch2.PK;
			settings2.GroupNumber = 1;
			settings2.IsParentBranch = true;

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);

			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			testInvoice.RunPreSaveValidation();
			Assert(string.Format("Invoice has errors: {0}", getErrorMessage(testInvoice)), !testInvoice.HasErrors);
			Assert(string.Format("Charges line has errors: {0}", getErrorMessage(line1)), !line1.HasErrors);
			Assert(string.Format("Charges line has errors: {0}", getErrorMessage(line1)), !line2.HasErrors);
		}

		[TestDate(2017, 08, 03)]
		public void TestCreateBusinessObjectsForPosting_ComplianceSubTypeSet()
		{
			var taxRate = TestObjectCreator.CreateTaxRate("ABC", "Test Tax Rate", 10);
			var chargeCode = TestObjectCreator.CreateChargeCode("OVR1", "Test Charge Code", Core.Constants.ChargeType.Overhead, 1m, taxRate, null, GlbCompany.CurrentCompany);

			setupPeriodManagement(2009);
			TestObjectCreator.SetupIntercompanyClearingConfigurationRegistry(headerCurrentCompany, headerIntercompany, intercompanyBranch.GB_GC);
			setupExchangeRate(0.7);
			AccApportionmentTemplate template = TestObjectCreator.CreateIntercompanyApportionmentTemplate(intercompanyBranch.PK);

			testInvoice.InvoiceDate = new ZDateTime(2009, 06, 01);
			testInvoice.PostedDate = testInvoice.InvoiceDate;
			testInvoice.DueDate = testInvoice.InvoiceDate;

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(OrgHeaderSchema.OH_IsActive, ZBool.True);
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(query);
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			testInvoice.Creditor = orgHeader.PK;

			testInvoice.Currency = TestObjectCreator.USD.RX_Code;
			testInvoice.InvoiceNumber = "2222";
			testInvoice.ExchangeRate.Rate = 0.5;

			IntercompanyCostsApportionmentInvoiceLine line = testInvoice.Lines.AddNew();
			line.GenericCharge = chargeCode.PK;
			line.ApportionmentMethod = template.PK;
			line.Amount = 500.0m;
			line.AL_GovtChargeCode = "GOVT1.2";
			AssertNotEquals("AL_AT", ZGuid.Empty, line.AL_AT);

			testInvoice.RunPreSaveValidation();
			Assert(string.Format("Invoice has errors: {0}", getErrorMessage(testInvoice)), !testInvoice.HasErrors);
			Assert(string.Format("Charges line has errors: {0}", getErrorMessage(line)), !line.HasErrors);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Peru))
			{
				AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				Assert(AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.Value);

				AssertNoExceptionThrown(() => testInvoice.CreateBusinessObjectsForPosting());
				AssertEquals("TXI", testInvoice.invoice.AH_ComplianceSubType);
			}
		}

		[TestDate(2017, 08, 03)]
		public void TestCreateBusinessObjectsForPosting_SupplyTypeSet()
		{
			var taxRate = TestObjectCreator.CreateTaxRate("ABC", "Test Tax Rate", 10);
			var chargeCode = TestObjectCreator.CreateChargeCode("OVR1", "Test Charge Code", Core.Constants.ChargeType.Overhead, 1m, taxRate, null, GlbCompany.CurrentCompany);

			setupPeriodManagement(2009);
			TestObjectCreator.SetupIntercompanyClearingConfigurationRegistry(headerCurrentCompany, headerIntercompany, intercompanyBranch.GB_GC);
			setupExchangeRate(0.7);
			var template = TestObjectCreator.CreateIntercompanyApportionmentTemplate(intercompanyBranch.PK);

			testInvoice.InvoiceDate = new ZDateTime(2009, 06, 01);
			testInvoice.PostedDate = testInvoice.InvoiceDate;
			testInvoice.DueDate = testInvoice.InvoiceDate;

			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(OrgHeaderSchema.OH_IsActive, ZBool.True);
			var subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);
			var orgHeader = Factory.LoadTop1<OrgHeader>(query);
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			testInvoice.Creditor = orgHeader.PK;

			testInvoice.Currency = TestObjectCreator.USD.RX_Code;
			testInvoice.InvoiceNumber = "2222";
			testInvoice.ExchangeRate.Rate = 0.5;

			var line = testInvoice.Lines.AddNew();
			line.GenericCharge = chargeCode.PK;
			line.ApportionmentMethod = template.PK;
			line.Amount = 500.0m;
			line.AL_GovtChargeCode = "GOVT1.2";
			line.AL_TaxDate = ZDate.Today.AddDays(-3);
			line.AL_SupplyType = "LOC";
			AssertNotEquals("AL_AT", ZGuid.Empty, line.AL_AT);

			testInvoice.RunPreSaveValidation();
			Assert(string.Format("Invoice has errors: {0}", getErrorMessage(testInvoice)), !testInvoice.HasErrors);
			Assert(string.Format("Charges line has errors: {0}", getErrorMessage(line)), !line.HasErrors);

			testInvoice.CreateBusinessObjectsForPosting();
			Factory.Save();

			var apInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, testInvoice.InvoiceNumber));
			AssertIntercompanyAPInvoice(template, apInvoice);
			AssertIntercompanyGLJournal(template, apInvoice, chargeCode);
			AssertEquals("All supply type of transaction lines should be LOC", true, apInvoice.Lines.All(x => ((AccTransactionLines)x).AL_SupplyType == "LOC"));
		}

		[TestDate(2017, 08, 03)]
		public void TestHandleEmptyComplianceSubtypeException_WhenDisallowBlankComplianceSubType()
		{
			var taxRate = TestObjectCreator.CreateTaxRate("ABC", "Test Tax Rate", 10);
			var chargeCode = TestObjectCreator.CreateChargeCode("OVR1", "Test Charge Code", Core.Constants.ChargeType.Overhead, 1m, taxRate, null, GlbCompany.CurrentCompany);

			setupPeriodManagement(2009);
			TestObjectCreator.SetupIntercompanyClearingConfigurationRegistry(headerCurrentCompany, headerIntercompany, intercompanyBranch.GB_GC);
			setupExchangeRate(0.7);
			AccApportionmentTemplate template = TestObjectCreator.CreateIntercompanyApportionmentTemplate(intercompanyBranch.PK);

			testInvoice.InvoiceDate = new ZDateTime(2009, 06, 01);
			testInvoice.PostedDate = testInvoice.InvoiceDate;
			testInvoice.DueDate = testInvoice.InvoiceDate;

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(OrgHeaderSchema.OH_IsActive, ZBool.True);
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(query);
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			testInvoice.Creditor = orgHeader.PK;

			testInvoice.Currency = TestObjectCreator.USD.RX_Code;
			testInvoice.InvoiceNumber = "2222";
			testInvoice.ExchangeRate.Rate = 0.5;

			IntercompanyCostsApportionmentInvoiceLine line = testInvoice.Lines.AddNew();
			line.GenericCharge = chargeCode.PK;
			line.ApportionmentMethod = template.PK;
			line.Amount = 500.0m;
			line.AL_GovtChargeCode = "GOVT1.2";
			AssertNotEquals("AL_AT", ZGuid.Empty, line.AL_AT);

			testInvoice.RunPreSaveValidation();
			Assert(string.Format("Invoice has errors: {0}", getErrorMessage(testInvoice)), !testInvoice.HasErrors);
			Assert(string.Format("Charges line has errors: {0}", getErrorMessage(line)), !line.HasErrors);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				Assert(AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.Value);

				ComplianceSubTypeAttributionRuleConfigurationCollection collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				ComplianceSubTypeAttributionRuleConfiguration item = collection.AddNew();
				item.Country = Enterprise.Core.Constants.CountryCodes.Italy;
				item.SubType = "ARI";
				item.LedgerType = "AR";
				item.InvoiceType = "CRD";
				item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID; // "TID";
				item.DisbursementRule = DisbursementRuleCodes.DisbursementOnly; // "DSB";
				item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly; //"OTO";
				item.OrganisationLocation = "";
				AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

				AssertExceptionThrown(typeof(EmptyComplianceSubTypeException), () => testInvoice.CreateBusinessObjectsForPosting());
			}
		}

		public void TestCreateNewIntercompanyCostsApportionedCompanyLocalAmount()
		{
			var taxRate = TestObjectCreator.CreateTaxRate("ABC", "Test Tax Rate", 10);
			var chargeCode = TestObjectCreator.CreateChargeCode("OVR1", "Test Charge Code", Core.Constants.ChargeType.Overhead, 1m, taxRate, null, GlbCompany.CurrentCompany);

			setupPeriodManagement(2009);
			TestObjectCreator.SetupIntercompanyClearingConfigurationRegistry(headerCurrentCompany, headerIntercompany, intercompanyBranch.GB_GC);
			setupExchangeRate(1);
			AccApportionmentTemplate template = this.CreateIntercompanyApportionmentTemplateForTestCompanyLocalAmount(intercompanyBranch.PK);

			testInvoice.InvoiceDate = new ZDateTime(2009, 06, 01);
			testInvoice.PostedDate = testInvoice.InvoiceDate;
			testInvoice.DueDate = testInvoice.InvoiceDate;

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(OrgHeaderSchema.OH_IsActive, ZBool.True);
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(query);
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			testInvoice.Creditor = orgHeader.PK;

			testInvoice.Currency = TestObjectCreator.USD.RX_Code;
			testInvoice.InvoiceNumber = "2222";
			testInvoice.ExchangeRate.Rate = 1;

			IntercompanyCostsApportionmentInvoiceLine line = testInvoice.Lines.AddNew();
			line.GenericCharge = chargeCode.PK;
			line.ApportionmentMethod = template.PK;
			line.Amount = 10m;
			line.AL_GovtChargeCode = "GOVT1.2";
			foreach (IntercompanyCostsApportionment intercompanyCostsApportionment in line.Apportionments)
			{
				AssertEquals("CompanyLocalAmount", 5m, intercompanyCostsApportionment.CompanyLocalAmount);
			}

			testInvoice.CreateBusinessObjectsForPosting();
			Factory.Save();

			APCreditNote apCreditNote = Factory.LoadTop1<APCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, testInvoice.InvoiceNumber));
			GLJournal journal = Factory.Load<GLJournal>(testInvoice.Journals[template.Lines[1].Company].PK);
			AssertNotNull("GLJournal", journal);
			GLJournalLine debitLine = (GLJournalLine)journal.Lines[0];
			AssertEquals("GLJournalLine UnsignedOSLineAmount", 5m, debitLine.UnsignedOSLineAmount);
		}

		[TestDate(2009, 09, 05)]
		public void TestCreateNewIntercompanyCostsApportioned_InvoicePostingExchangeRateOption()
		{
			var taxRate = TestObjectCreator.CreateTaxRate("ABC", "Test Tax Rate", 10);
			var chargeCode = TestObjectCreator.CreateChargeCode("OVR1", "Test Charge Code", Core.Constants.ChargeType.Overhead, 1m, taxRate, null, GlbCompany.CurrentCompany);

			setupPeriodManagement(2009);
			TestObjectCreator.SetupIntercompanyClearingConfigurationRegistry(headerCurrentCompany, headerIntercompany, intercompanyBranch.GB_GC);
			TestObjectCreator.CreateUSDBuyRate(0.8m, new ZDateTime(2009, 9, 2));
			TestObjectCreator.CreateUSDBuyRate(0.6m, new ZDateTime(2009, 9, 3));
			TestObjectCreator.CreateUSDBuyRate(0.7m, new ZDateTime(2009, 9, 4));
			TestObjectCreator.CreateUSDBuyRate(0.5m, new ZDateTime(2009, 9, 5));

			var template = this.CreateIntercompanyApportionmentTemplateForTestCompanyLocalAmount(intercompanyBranch.PK);

			AssertInvoicePostingExchangeRateOption(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code, 0.7m);
			AssertInvoicePostingExchangeRateOption(AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code, 0.8m);
			AssertInvoicePostingExchangeRateOption(AccountingConstants.InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code, 0.6m);
			AssertInvoicePostingExchangeRateOption(AccountingConstants.InvoicePostingExchangeRateOption.TodayExchangeRate.Code, 0.5m);
			AssertInvoicePostingExchangeRateOption(AccountingConstants.InvoicePostingExchangeRateOption.Default.Code, 0.5m);

			void AssertInvoicePostingExchangeRateOption(string exchangeRateOption, decimal expectedExchangeRate)
			{
				using (AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, exchangeRateOption))
				{
					var newFactory = new BusinessObjectFactory();
					var invoice = new IntercompanyCostsApportionmentInvoice(newFactory);
					invoice.InvoiceDate = new ZDateTime(2009, 09, 04);
					invoice.PostedDate = new ZDateTime(2009, 09, 02);
					invoice.DueDate = invoice.InvoiceDate;
					invoice.Creditor = TestObjectCreator.Creditor1.PK;
					invoice.InvoiceNumber = "22220" + exchangeRateOption;
					invoice.ExchangeRate.Currency = TestObjectCreator.USD.RX_Code;

					var line = invoice.Lines.AddNew();
					line.GenericCharge = chargeCode.PK;
					line.ApportionmentMethod = template.PK;
					line.Amount = 10m;
					line.AL_GovtChargeCode = "GOVT1.2";
					line.AL_TaxDate = new ZDate(2009, 09, 03);

					AssertEquals(expectedExchangeRate, invoice.ExchangeRate.Rate);

					invoice.CreateBusinessObjectsForPosting();
					newFactory.Save();

					var apCreditNote = newFactory.LoadTop1<APCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, invoice.InvoiceNumber));
					Assert(!apCreditNote.UseJobExchangeRate);
					AssertEquals(expectedExchangeRate, apCreditNote.AH_ExchangeRate);
				}
			}
		}

		public void TestDueDateInNewInvoiceIsSameAsSettingBeforePost()
		{
			testInvoice.InvoiceDate = new ZDateTime(2009, 06, 01);
			testInvoice.DueDate = new ZDateTime(2010, 06, 01);

			var invoice = testInvoice.CreateBusinessObjectsForPosting();

			AssertEquals("DueDate in new Invoice is same as setting before post", new ZDateTime(2010, 06, 01), invoice.AH_DueDate);
		}

		AccApportionmentTemplate CreateIntercompanyApportionmentTemplateForTestCompanyLocalAmount(ZGuid intercompanyBranch)
		{
			AccApportionmentTemplate template = Factory.New<AccApportionmentTemplate>();
			template.A0_Description = "Description";
			template.A0_GC = GlbCompany.CurrentCompany.PK;
			template.A0_Notes = "Notes";

			ZQuery query = new ZQuery(GlbDepartmentSchema.GE_IsActive, true);
			GlbDepartment department1 = Factory.LoadTop1<GlbDepartment>(query);
			query.AddToFilter(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, department1.PK);
			GlbDepartment department2 = Factory.LoadTop1<GlbDepartment>(query);

			TestObjectCreator.CreateTemplateLine(template, "Line 1", GlbBranch.CurrentBranch.PK, department1.PK, 49.950m);
			TestObjectCreator.CreateTemplateLine(template, "Line 2", intercompanyBranch, department2.PK, 50.050m);

			return template;
		}

		[TestDate(2024, 9, 2)]
		public void TestCachedInvoiceTaxDate()
		{
			AccountingConfigurationRegistry.Instance.EnablePostTransactionCalculatedPropertyCache.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CreateAndAssertInvoiceTaxDate();

			AccountingConfigurationRegistry.Instance.EnablePostTransactionCalculatedPropertyCache.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CreateAndAssertInvoiceTaxDate();
		}

		void CreateAndAssertInvoiceTaxDate()
		{
			var invoice = new IntercompanyCostsApportionmentInvoice(Factory);
			var taxDate = ZDate.Today;
			var invoiceDate = new ZDate(2024, 9, 1);
			invoice.AH_InvoiceDate = invoiceDate;
			var line1 = invoice.Lines.AddNew();
			line1.AL_AC = TestObjectCreator.CC1.PK;
			line1.AL_AT = TestObjectCreator.GST1.PK;
			line1.AL_TaxDate = taxDate;

			var taxDate2 = new ZDate(2024, 8, 31);
			var line2 = new IntercompanyCostsApportionmentInvoiceLine(Factory, invoice);
			line2.AL_AC = TestObjectCreator.CC1.PK;
			line2.AL_AT = TestObjectCreator.GST1.PK;
			line2.AL_TaxDate = taxDate2;

			AssertEquals(taxDate, invoice.InvoiceTaxDate);
			invoice.Lines.Add(line2);
			AssertEquals(taxDate2, invoice.InvoiceTaxDate);
		}

		#endregion

	}
}
