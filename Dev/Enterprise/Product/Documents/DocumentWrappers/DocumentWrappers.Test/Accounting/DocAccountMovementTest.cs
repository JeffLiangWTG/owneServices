using System;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocAccountMovement))]
	sealed class DocAccountMovementTest : DocumentWrapperTestCase
	{
		public void TestGSTAmount()
		{
			foreach (bool regValue in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regValue))
				{
					AccMovementInv.IsMultipleCurrency = false;
					AssertEquals("ARInvoice: GST Amount when Currency is not Local", Invoice.AH_OSTaxAmount, AccMovementInvWrapper.GSTAmount);

					AccMovementInv.IsMultipleCurrency = true;
					AssertEquals("ARInvoice: GST Amount when Currency is Local", Invoice.AH_LocalTaxAmount, AccMovementInvWrapper.GSTAmount);

					AccMovementCrd.IsMultipleCurrency = false;
					AssertEquals("ARCreditNote: GST Amount when Currency is not Local", (regValue ? 1 : -1) * CreditNote.AH_OSTaxAmount, AccMovementCrdWrapper.GSTAmount);

					AccMovementCrd.IsMultipleCurrency = true;
					AssertEquals("ARCreditNote: GST Amount when Currency is Local", (regValue ? 1 : -1) * CreditNote.AH_LocalTaxAmount, AccMovementCrdWrapper.GSTAmount);
				}
			}
		}

		public void TestInvoiceAmountWithGST()
		{
			foreach (bool regValue in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regValue))
				{
					AccMovementInv.IsMultipleCurrency = false;
					AssertEquals("ARInvoice: Amount when Currency is not Local", Invoice.AH_OSTotal, AccMovementInvWrapper.InvoiceAmountWithGST);

					AccMovementInv.IsMultipleCurrency = true;
					AssertEquals("ARInvoice: Amount when Currency is Local", Invoice.AH_InvoiceAmount + Invoice.AH_GSTAmount, AccMovementInvWrapper.InvoiceAmountWithGST);

					AccMovementCrd.IsMultipleCurrency = false;
					AssertEquals("ARCreditNote: Amount when Currency is not Local", (regValue ? -1 : 1) * CreditNote.AH_OSTotal, AccMovementCrdWrapper.InvoiceAmountWithGST);

					AccMovementCrd.IsMultipleCurrency = true;
					AssertEquals("ARCreditNote: Amount when Currency is Local", (regValue ? -1 : 1) * (CreditNote.AH_InvoiceAmount + CreditNote.AH_GSTAmount), AccMovementCrdWrapper.InvoiceAmountWithGST);
				}
			}
		}

		public void TestAccountMovementBalance()
		{
			foreach (bool regValue in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regValue))
				{
					AccMovementInv.OpeningBalance = 5000;
					foreach (bool isFirstTransaction in new bool[] { true, false })
					{
						AccMovementInv.IsFirstTransaction = isFirstTransaction;
						AccMovementInv.IsMultipleCurrency = false;
						AssertEquals("ARInvoice: Amount when Currency is not Local and isFirstTransaction = " + isFirstTransaction.ToString(), (isFirstTransaction ? 5000 : 0) + Invoice.AH_OSTotal, AccMovementInvWrapper.AccountMovementBalance);

						AccMovementInv.IsMultipleCurrency = true;
						AssertEquals("ARInvoice: Amount when Currency is Local and isFirstTransaction = " + isFirstTransaction.ToString(), (isFirstTransaction ? 5000 : 0) + Invoice.AH_InvoiceAmount + Invoice.AH_GSTAmount, AccMovementInvWrapper.AccountMovementBalance);

						AccMovementCrd.IsMultipleCurrency = false;
						AssertEquals("ARCreditNote: Amount when Currency is not Local" + isFirstTransaction.ToString(), CreditNote.AH_OSTotal, AccMovementCrdWrapper.AccountMovementBalance);

						AccMovementCrd.IsMultipleCurrency = true;
						AssertEquals("ARCreditNote: Amount when Currency is Local" + isFirstTransaction.ToString(), CreditNote.AH_InvoiceAmount + CreditNote.AH_GSTAmount, AccMovementCrdWrapper.AccountMovementBalance);
					}
				}
			}
		}

		public void TestBalance()
		{
			foreach (bool regValue in new[] { true, false })
			{
				using (AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regValue))
				{
					AccMovementInv.IsMultipleCurrency = false;
					AssertEquals("ARInvoice: Balance when Currency is not Local", AccMovementInvWrapper.InvoiceAmountWithGST, AccMovementInvWrapper.Balance);

					AccMovementInv.IsMultipleCurrency = true;
					AssertEquals("ARInvoice: Balance when Currency is Local", AccMovementInvWrapper.InvoiceAmountWithGST, AccMovementInvWrapper.Balance);

					AccMovementCrd.IsMultipleCurrency = false;
					AssertEquals("ARCreditNote: Balance when Currency is not Local", CreditNote.AH_OSTotal, AccMovementCrdWrapper.Balance);

					AccMovementCrd.IsMultipleCurrency = true;
					AssertEquals("ARCreditNote: Balance when Currency is Local", CreditNote.AH_InvoiceAmount + CreditNote.AH_GSTAmount, AccMovementCrdWrapper.Balance);
				}
			}
		}

		public void TestBalance_UnallocateReceipt()
		{
			ARReceipt receipt = ObjectCreator.CreateARReceipt(1m, 1000m, Env.Time.CurrentLocalDate.AddDays(-3), Env.Time.CurrentLocalDate.AddDays(-3), TestObjectCreator.ABIGAS.PK, TestObjectCreator.AUDBankAccount.PK);

			ARInvoice invoice1 = ObjectCreator.CreateARInvoice<ARInvoice>("001", ObjectCreator.AUD, 1, ObjectCreator.ABIGAS);
			invoice1.AH_PostDate = Env.Time.CurrentLocalDate.AddDays(-2);
			invoice1.AH_OSExTaxAmount = 100m;
			invoice1.AH_LocalExTaxAmount = 100m;
			ObjectCreator.CreateInvoiceLine(invoice1, 100m, setTaxes: false);

			ARInvoice invoice2 = ObjectCreator.CreateARInvoice<ARInvoice>("002", ObjectCreator.AUD, 1, ObjectCreator.ABIGAS);
			invoice2.AH_PostDate = Env.Time.CurrentLocalDate.AddDays(-1);
			invoice2.AH_OSExTaxAmount = 200m;
			invoice2.AH_LocalExTaxAmount = 200m;
			ObjectCreator.CreateInvoiceLine(invoice2, 200m, setTaxes: false);

			Factory.Save();

			AccTransactionMatchLink matchLink1 = Factory.New<AccTransactionMatchLink>();
			AccTransactionMatchLink matchLink2 = Factory.New<AccTransactionMatchLink>();
			matchLink1.AP_AH = invoice1.PK;
			matchLink1.AP_Amount = 100m;
			matchLink1.AP_MatchDate = Env.Time.CurrentLocalDate.AddDays(-2);
			matchLink1.AP_MatchGroupNum = "M001";

			matchLink2.AP_AH = receipt.PK;
			matchLink2.AP_Amount = -100m;
			matchLink2.AP_MatchDate = Env.Time.CurrentLocalDate.AddDays(-2);
			matchLink2.AP_MatchGroupNum = "M001";

			TransactionMatchLinkGroup matchlinks = new TransactionMatchLinkGroup(Factory);
			matchlinks.Add(matchLink1);
			matchlinks.Add(matchLink2);

			invoice1.AH_OutstandingAmount = 0m;
			invoice1.AH_FullyPaidDate = Env.Time.CurrentLocalDate.AddDays(-2);
			receipt.AH_OutstandingAmount = -900m;
			Factory.Save();

			AccTransactionMatchLink matchLink3 = Factory.New<AccTransactionMatchLink>();
			AccTransactionMatchLink matchLink4 = Factory.New<AccTransactionMatchLink>();
			matchLink3.AP_AH = invoice2.PK;
			matchLink3.AP_Amount = 200m;
			matchLink3.AP_MatchDate = Env.Time.CurrentLocalDate.AddDays(-1);
			matchLink3.AP_MatchGroupNum = "M002";

			matchLink4.AP_AH = receipt.PK;
			matchLink4.AP_Amount = -200m;
			matchLink4.AP_MatchDate = Env.Time.CurrentLocalDate.AddDays(-1);
			matchLink4.AP_MatchGroupNum = "M002";

			TransactionMatchLinkGroup matchlinks2 = new TransactionMatchLinkGroup(Factory);
			matchlinks2.Add(matchLink3);
			matchlinks2.Add(matchLink4);

			invoice2.AH_OutstandingAmount = 0m;
			invoice2.AH_FullyPaidDate = Env.Time.CurrentLocalDate.AddDays(-1);
			receipt.AH_OutstandingAmount = -700m;
			Factory.Save();

			PrintStatementForAccountMovement statement = new PrintStatementForAccountMovement(Factory, GlbBranch.CurrentBranch, true);
			statement.PostDateFrom = Env.Time.CurrentLocalDate.AddDays(-3);
			statement.PostDateTo = Env.Time.CurrentLocalDate.AddDays(-2);
			statement.OrganisationPK = TestObjectCreator.ABIGAS.PK;
			DocStatementForAccountMovement statementWrapper = DocStatementForAccountMovement.New(statement, Factory) as DocStatementForAccountMovement;
			AssertEquals("Total of Receipts not Matched", "Total Unallocated Receipts: -900.00 AUD", statementWrapper.ReceiptsNotMatched);

			statement.PostDateTo = Env.Time.CurrentLocalDate.AddDays(-1);
			statementWrapper = DocStatementForAccountMovement.New(statement, Factory) as DocStatementForAccountMovement;
			AssertEquals("Total of Receipts not Matched", "Total Unallocated Receipts: -700.00 AUD", statementWrapper.ReceiptsNotMatched);
		}

		public void TestSplitAmountByChargeCode()
		{
			using (AccountingConfigurationRegistry.Instance.ShowARCreditNoteAmountsWithOppositeSign.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AccMovementInv.IsMultipleCurrency = false;
				AssertEquals("Amount when Currency is not Local", 165m, AccMovementInvWrapper.AmountSplittedByChargeCode["ZZCC1"].TotalAmount);
				AssertEquals("Amount when Currency is not Local", 275m, AccMovementInvWrapper.AmountSplittedByChargeCode["ZZCC2"].TotalAmount);
				AssertEquals("Amount when Currency is not Local", 165m, AccMovementInvWrapper.SumOfAmountExceptTheseChargeCodes["ZZCC2"].TotalAmount);
				AssertEquals("Amount when Currency is not Local", 275m, AccMovementInvWrapper.SumOfAmountExceptTheseChargeCodes["ZZCC1"].TotalAmount);
				AssertEquals("Amount when Currency is not Local", 0m, AccMovementInvWrapper.SumOfAmountExceptTheseChargeCodes["ZZCC1, ZZCC2"].TotalAmount);
			}
		}

		protected override void SetUp()
		{
			StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			ObjectCreator.Debtor.CompanyData.SetARTaxApplicable(true);

			var job = ObjectCreator.CreateJob("J0000123", ObjectCreator.Debtor, 1.0m, ObjectCreator.Agent, 1.0m);

			Invoice = ObjectCreator.CreateInvoice(typeof(ARInvoice), "INV000125", ObjectCreator.USD, 2.0m, ObjectCreator.Debtor) as ARInvoice;
			var line1 = ObjectCreator.CreateInvoiceLine("REV", Invoice, job, ObjectCreator.CC1, ObjectCreator.USD, 2.0m, "Desc", 150m);
			var line2 = ObjectCreator.CreateInvoiceLine("REV", Invoice, job, ObjectCreator.CC2, ObjectCreator.USD, 2.0m, "Desc", 250m);

			line1.AL_AT = line2.AL_AT = ObjectCreator.GST1.PK;

			ObjectCreator.CreateJobCharge(line1, job, ObjectCreator.CC1);
			ObjectCreator.CreateJobCharge(line2, job, ObjectCreator.CC2);

			CreditNote = ObjectCreator.CreateARCreditNote("CRD000125", ObjectCreator.Debtor, ObjectCreator.USD, 2.0m);
			var crline1 = ObjectCreator.CreateARCreditNoteLine(CreditNote, job, ObjectCreator.CC1, 50m, ObjectCreator.USD, 2.0M, "Desc CR 1");
			var crline2 = ObjectCreator.CreateARCreditNoteLine(CreditNote, job, ObjectCreator.CC2, 100m, ObjectCreator.USD, 2.0M, "Desc CR 2");
			ObjectCreator.CreateJobCharge(crline1, job, ObjectCreator.CC1);
			ObjectCreator.CreateJobCharge(crline2, job, ObjectCreator.CC2);

			crline1.AL_AT = crline2.AL_AT = ObjectCreator.GST1.PK;

			Factory.Save();

			AccMovementInv = Factory.Load<AccountMovement>(Invoice.PK);
			AccMovementInvWrapper = DocAccountMovement.New(AccMovementInv, Factory) as DocAccountMovement;
			AccMovementCrd = Factory.Load<AccountMovement>(CreditNote.PK);
			AccMovementCrdWrapper = DocAccountMovement.New(AccMovementCrd, Factory) as DocAccountMovement;

			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocAccountMovement.New(AccMovementInv, Factory) };
		}

		protected override string TestingCountry
		{
			get { return null; }
		}

		TestObjectCreator ObjectCreator
		{
			get
			{
				if (fObjectCreator == null)
				{
					fObjectCreator = new TestObjectCreator(Factory);
				}
				return fObjectCreator;
			}
		}
		TestObjectCreator fObjectCreator;

		ARInvoice Invoice;
		ARCreditNote CreditNote;
		AccountMovement AccMovementInv;
		DocAccountMovement AccMovementInvWrapper;
		AccountMovement AccMovementCrd;
		DocAccountMovement AccMovementCrdWrapper;
	}
}

