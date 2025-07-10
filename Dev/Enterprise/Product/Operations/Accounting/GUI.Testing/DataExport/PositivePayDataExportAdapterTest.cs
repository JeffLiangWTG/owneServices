using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.DataExport.Testing
{
	sealed class PositivePayDataExportAdapterTest : TestCaseWithFactory
	{
		#region Implementation

		TransactionHeader[] GetTransactionHeaders()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupSinglePeriod(200911, new ZDateTime(2009, 11, 01), new ZDateTime(2009, 11, 30));

			OrgHeader orgHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
			AssertNotNull("OrgHeader", orgHeader);

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZDateTime date = new ZDateTime(2009, 11, 4);

			RefCurrency aUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);

			AccBankAccount bankAccount = BankAccount;

			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = 9999;
			chequeBook.AK_CurrentNo = 1;

			//1 - Unmatched Payment

			APPayment unmatchedPayment = Factory.New<APPayment>();
			unmatchedPayment.AH_InvoiceDate = date;
			unmatchedPayment.AH_PostDate = date;
			unmatchedPayment.AH_OH = orgHeader.PK;
			unmatchedPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			unmatchedPayment.AH_AB = bankAccount.PK;
			unmatchedPayment.ChequeBook = chequeBook.PK;
			unmatchedPayment.AH_ChequeOrReference = "000001";
			unmatchedPayment.AH_RX_NKTransactionCurrency = aUD.RX_Code;
			unmatchedPayment.AH_LocalExTaxAmount = 100.00m;
			unmatchedPayment.AH_OSExTaxAmount = 100.00m;

			AssertEquals("unmatched payment should have no errors: " + unmatchedPayment.NotificationsIncludingChildren.ToUniqueMessageListString(), false, unmatchedPayment.HasErrors);

			Factory.Save();

			//2 - Matched Payment & Invoice

			APInvoice invoice1 = Factory.New<APInvoice>();
			invoice1.AH_OH = orgHeader.PK;
			invoice1.AH_PostDate = date;
			invoice1.AH_InvoiceDate = date;
			invoice1.AH_DueDate = date;
			invoice1.AH_TransactionNum = "004";

			APInvoiceLine line1 = (APInvoiceLine)invoice1.Lines.AddNew();

			AccGLHeader gLHeader2 = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "7100.40.10");
			AssertNotNull("GLHeader2", gLHeader2);

			line1.GenericCharge = gLHeader2.PK;
			line1.AL_LocalExTaxAmount = 200.00m;
			line1.AL_OSExTaxAmount = 200.00m;

			ZQuery query = new ZQuery(AccTaxRateSchema.AT_IsActive, true);
			query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			query.AddToFilter(AccTaxRateSchema.AT_Code, "GST");
			AccTaxRate gSTtaxRate = Factory.LoadTop1<AccTaxRate>(query);
			gSTtaxRate.SetRateNumerator_ForTestOnly(10);
			line1.AL_AT = gSTtaxRate.PK;
			invoice1.AH_OutstandingAmount = 0m;
			invoice1.AH_FullyPaidDate = date;

			APPayment matchedPayment1 = Factory.New<APPayment>();
			matchedPayment1.AH_PostDate = date;
			matchedPayment1.AH_InvoiceDate = date;
			matchedPayment1.AH_OH = orgHeader.PK;
			matchedPayment1.AH_ReceiptType = ReceiptTypes.Cheque;
			matchedPayment1.AH_AB = bankAccount.PK;
			matchedPayment1.ChequeBook = chequeBook.PK;
			matchedPayment1.AH_ChequeOrReference = "000002";
			matchedPayment1.AH_RX_NKTransactionCurrency = aUD.RX_Code;
			matchedPayment1.AH_LocalExTaxAmount = 220.00m;
			matchedPayment1.AH_OSExTaxAmount = 220.00m;
			matchedPayment1.AH_OutstandingAmount = 0m;
			matchedPayment1.AH_FullyPaidDate = date;

			AssertEquals("matchedPayment1 should have no errors: " + matchedPayment1.NotificationsIncludingChildren.ToUniqueMessageListString(), false, matchedPayment1.HasErrors);

			TransactionMatchLink matchLink1 = ((IMatching)invoice1).CurrentMatchGroup.AddNew();
			matchLink1.AP_AH = invoice1.PK;
			matchLink1.AP_MatchGroupNum = "98765";
			matchLink1.AP_Amount = invoice1.AH_InvoiceAmount + invoice1.AH_GSTAmount;

			TransactionMatchLink matchLink2 = ((IMatching)invoice1).CurrentMatchGroup.AddNew();
			matchLink2.AP_AH = matchedPayment1.PK;
			matchLink2.AP_MatchGroupNum = matchLink1.AP_MatchGroupNum;
			matchLink2.AP_Amount = matchedPayment1.AH_InvoiceAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(invoice1);

			Factory.Save();

			//3 - Direct Payment

			DirectPayment directPayment = Factory.New<DirectPayment>();
			directPayment.AH_TransactionNum = "00001000";
			directPayment.AH_InvoiceDate = date;
			directPayment.AH_PostDate = date;
			directPayment.AH_AB = bankAccount.PK;
			directPayment.AH_RX_NKTransactionCurrency = aUD.RX_Code;
			directPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			directPayment.ChequeBookPK = chequeBook.PK;
			directPayment.AH_ChequeOrReference = "000003";
			directPayment.AH_ChequeDrawer = "JOHN SMITH";
			directPayment.AH_DrawerBank = "88884321";
			directPayment.AH_DrawerBranch = "4321765";

			AccGLHeader gLHeader3 = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "3510.00.00");
			AssertNotNull("GLHeader3", gLHeader3);

			DirectPaymentLine directPaymentLine1 = (DirectPaymentLine)directPayment.Lines.AddNew();
			directPaymentLine1.AL_AG = gLHeader2.PK;
			directPaymentLine1.AL_OSExTaxAmount = 1000.00m;
			directPaymentLine1.AL_LocalWHTAmount = 0.00m;
			directPaymentLine1.AL_AT = gSTtaxRate.PK;

			query = new ZQuery(AccTaxRateSchema.AT_IsActive, true);
			query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			query.AddToFilter(AccTaxRateSchema.AT_Code, "FREEGST");
			AccTaxRate fREEGST = Factory.LoadTop1<AccTaxRate>(query);

			DirectPaymentLine directPaymentLine2 = (DirectPaymentLine)directPayment.Lines.AddNew();
			directPaymentLine2.AL_AG = gLHeader3.PK;
			directPaymentLine2.AL_OSExTaxAmount = 10000.00m;
			directPaymentLine2.AL_LocalWHTAmount = 0.00m;
			directPaymentLine2.AL_AT = fREEGST.PK;

			Factory.Save();

			return new TransactionHeader[] { unmatchedPayment, matchedPayment1, directPayment };
		}

		AccBankAccount bankAccount;
		AccBankAccount BankAccount
		{
			get
			{
				if (bankAccount == null)
				{
					AccGLHeader gLHeader = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "6110.10.10");
					AssertNotNull("GLHeader", gLHeader);

					bankAccount = Factory.New<AccBankAccount>();
					bankAccount.AB_Code = "AAA";
					bankAccount.AB_Desc = "AAA BANK ACCOUNT";
					bankAccount.AB_AG = gLHeader.PK;
					bankAccount.AB_BankName = "AAA BANK";
					bankAccount.AB_BankAddress = "123 SOME STREET, SYDNEY, NSW, 2000";
					bankAccount.AB_BankAccountName = "EAGLE DATAMATION INTERNATIONAL";
					bankAccount.AB_BSB = "12345678";
					bankAccount.AB_AccountNum = "87654321";
					bankAccount.AB_BankAbbreviation = "AAA";
					bankAccount.AB_RX_NKAccountCurrency = "AUD";
					bankAccount.AB_AllowAutoDDR = true;
					bankAccount.AB_AutoDDRFormat = "BTM";
					bankAccount.AB_DetailedDepositSlip = true;
					bankAccount.AB_IsDefaultReceiptBankAccount = false;
				}
				return bankAccount;
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 07, 14)]
		public void TestPositivePayExport()
		{
			TransactionHeader[] transactionHeaders = GetTransactionHeaders();
			PositivePayDataExportAdapter adapter = new PositivePayDataExportAdapter(Factory, transactionHeaders, BankAccount);
			adapter.Sort(transactionHeaders);
			IEnumerable<BusinessObject> businessObjects = adapter.GetBusinessObjectsForExport(transactionHeaders);
			IExportCollectionInfo collectionInfo = adapter.GetMultiTypeCollectionInfo(businessObjects);
			ExportWizard exportWizard = new ExportWizard(collectionInfo, null, new FileMapper());
			string filePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\DataExport\Settings\PositivePayExportSettings.xml";
			exportWizard.SetSettings(DataExportWizardSettings.FromXml<DataExportWizardSettings>(File.ReadAllText(filePath)));
			List<string[]> results = new List<string[]>(exportWizard.ExportCollection(businessObjects, int.MaxValue));
			AssertPositivePayExport(results);
		}

		void AssertPositivePayExport(List<string[]> results)
		{
			int rowCount = 0;
			AssertPositivePayPayment(results[0], ++rowCount, "00087654321", "0000000001", "11042009", "0000010000", "A.A.L. SHIPPING AGENCIES P/L  ", "D", " ", "        ");
			AssertPositivePayPayment(results[1], ++rowCount, "00087654321", "0000000002", "11042009", "0000022000", "A.A.L. SHIPPING AGENCIES P/L  ", "D", " ", "        ");
			AssertPositivePayPayment(results[2], ++rowCount, "00087654321", "0000000003", "11042009", "0001110000", "JOHN SMITH                    ", "D", " ", "        ");
		}

		void AssertPositivePayPayment(string[] line, int rowCount, string accountNumber, string checkNumber, string issueDate, string checkAmount, string payee,
			string debitCode, string voidStatus, string voidDate)
		{
			AssertEquals(string.Format("Line {0} Length", rowCount), 8, line.Length);
			AssertEquals(accountNumber, line[0]);
			AssertEquals(checkNumber, line[1]);
			AssertEquals(issueDate, line[2]);
			AssertEquals(checkAmount, line[3]);
			AssertEquals(payee, line[4]);
			AssertEquals(debitCode, line[5]);
			AssertEquals(voidStatus, line[6]);
			AssertEquals(voidDate, line[7]);
		}

		#endregion
	}
}
