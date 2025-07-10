using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class APCreditNoteCreatorTest : TransactionCreatorBaseTest
	{
		#region Create Credit Note Using Non-Reciprocal Company With Small Exchange Rate

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestCreateCreditNoteNonReciprocalSmallExRate()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlbCompany currentCompany = newFactory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.GC_IsReciprocal = true;
			newFactory.Save();

			var user = Env.CurrentUser;
			Guid branch = Env.CurrentBranch.PK;
			Guid department = Env.CurrentDepartment.PK;

			Env.ClearUserContext();
			Env.SetUserContext(new UserContext(user, branch, department));
			Job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(Job, USD, 0.1814m);

			Charge charge1 = Job.Charges.AddNew();
			charge1.JR_AC = CC1.PK;
			charge1.JR_RX_NKCostCurrency = USD.RX_Code;
			charge1.JR_OSCostAmt = -7710.52m;
			charge1.JR_OH_CostAccount = Creditor1.PK;
			charge1.JR_APInvoiceNum = "inv123";
			charge1.JR_APInvoiceDate = ZDateTime.Now;

			Factory.Save();

			APInvoiceCreator creator = new APInvoiceCreator(Job);
			APCreditNoteCreator creditNoteCreator = new APCreditNoteCreator(Job);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			creator.CreateTransactions(transactions);
			creditNoteCreator.CreateTransactions(transactions);

			InvoicingBase[] invoices = transactions.GetAllAPInvoicesAndCreditNotes();
			AssertEquals("Should only be 1 item posted", 1, invoices.Length);
			Assert("Should be a credit note", invoices[0] is APCreditNote);
			APCreditNote creditNote = (APCreditNote)invoices[0];
			AssertEquals("Credit note amounnt should be same as charge amount", 7710.52m, creditNote.AH_OSExTaxAmount);
		}

		#endregion

		#region Create Credit Note suspends ListChanged on deleting Original APInvoice Lines

		public void TestCreateCreditNoteSuspendsListChangedOnDeletingOriginalAPInvoiceLines()
		{
			Job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			var rate1 = CreateExchangeRate(Job, USD, 0.1814m);

			var charge1 = Job.Charges.AddNew();
			charge1.JR_AC = CC1.PK;
			charge1.JR_RX_NKCostCurrency = USD.RX_Code;
			charge1.JR_OSCostAmt = -7710.52m;
			charge1.JR_OH_CostAccount = Creditor1.PK;
			charge1.JR_APInvoiceNum = "inv123";
			charge1.JR_APInvoiceDate = ZDateTime.Now;

			var charge2 = Job.Charges.AddNew();
			charge2.JR_AC = CC2.PK;
			charge2.JR_RX_NKCostCurrency = USD.RX_Code;
			charge2.JR_OSCostAmt = -3309.48m;
			charge2.JR_OH_CostAccount = Creditor1.PK;
			charge2.JR_APInvoiceNum = "inv123";
			charge2.JR_APInvoiceDate = ZDateTime.Now;

			Factory.Save();

			var creator = new APInvoiceCreator(Job);
			var creditNoteCreator = new APCreditNoteCreator(Job);
			var transactions = new TransactionCreatorHashtable();
			creator.CreateTransactions(transactions);

			var invoices = transactions.GetAllAPInvoicesAndCreditNotes();
			AssertEquals("Should only be 1 item posted", 1, invoices.Length);
			Assert("Should be an Invoice", invoices[0] is APInvoice);
			var invoice = (APInvoice)invoices[0];
			AssertEquals("Invoice Amount should be sum of charge amounts", -11020m, invoice.AH_OSExTaxAmount);

			int invoiceLinesListChangedHitCount = 0;
			var invoiceLinesListChangedHandler = new ListChangedEventHandler(
				(sender, e) =>
				{ invoiceLinesListChangedHitCount++; }
			);
			((IBindingList)invoice.Lines).ListChanged += invoiceLinesListChangedHandler;

			int creditNoteLinesListChangedHitCount = 0;
			var creditNoteLinesListChangedHandler = new ListChangedEventHandler(
				(sender, e) =>
				{ creditNoteLinesListChangedHitCount++; }
			);
			creditNoteCreator.NewCreditNoteAction_ForTestOnly += new Action<InvoicingBase>((x) => ((IBindingList)x.Lines).ListChanged += creditNoteLinesListChangedHandler);

			creditNoteCreator.CreateTransactions(transactions);
			AssertEquals("ListChanged on invoice.Lines should be called once at we use ListChanged suspender", 1, invoiceLinesListChangedHitCount);
			AssertEquals("ListChanged on creditNote.Lines should be called once at we use ListChanged suspender", 1, creditNoteLinesListChangedHitCount);

			invoices = transactions.GetAllAPInvoicesAndCreditNotes();
			AssertEquals("Should only be 1 item posted", 1, invoices.Length);
			Assert("Should be a Credit Note", invoices[0] is APCreditNote);
			var creditNote = (APCreditNote)invoices[0];
			AssertEquals("Credit Note Amount should be sum of charge amounts", 11020m, creditNote.AH_OSExTaxAmount);
		}

		#endregion

		#region TEST: Create All Cost Invoices

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesCostsOnly()
		{
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			SetupCharges();
			SetupAPInvoiceInfo();
			Factory.Save();

			InitializeWIPAccruals();

			AssertNull("Not Accrual from Charge 1", Charge1.Accrual);
			AccTransactionLines charge2Accrual = Charge2.Accrual;
			AssertNull("Not Accrual from Charge 3", Charge3.Accrual);
			AccTransactionLines charge5Accrual = Charge5.Accrual;
			AssertNull("Not Accrual from Charge 6", Charge6.Accrual);
			AccTransactionLines charge7Accrual = Charge7.Accrual;

			APInvoiceCreator creator = new APInvoiceCreator(Job);
			APCreditNoteCreator creditNoteCreator = new APCreditNoteCreator(Job);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();

			SetUpRegistryForTest();
			try
			{
				Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = true;
				creator.CreateTransactions(transactions);
				AssertEquals("Invoice Count", 4, transactions.APTransactionsCount);
				AssertEquals("Receivable Transactions Count", 0, transactions.ARTransactionsCount);
				creditNoteCreator.CreateTransactions(transactions);
			}
			finally
			{
				ResetRegistryForTest();
			}

			AssertEquals("Invoice Count", 4, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 0, transactions.ARTransactionsCount);

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-100M, 10M, 0M, -90M, AUD, 1, Now, ZBool.False, Creditor1, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor1Inv1);

			TransactionLine cC1Line = creditor1Inv1.FindTransactionLine("CST", CC1, Job.PK);
			AssertTransactionLineValues(cC1Line, "CST", 1, "Charge Code 1", 100M, GST1, 10M, WHTFREE1, 0M, 110M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv1, Job, CC1, CC1.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC1Line);

			TransactionLine cC7Line = creditor1Inv1.FindTransactionLine("CST", CC7, Job.PK);
			AssertTransactionLineValues(cC7Line, "CST", 2, "Charge Code 7", -200M, GSTFREE1, 0M, WHTFREE1, 0M, -200M, AUD, 1, Now, Now,
				ZBool.False, creditor1Inv1, Job, CC7, CC7.CostAccount, Creditor1);
			AssertTransactionLineDefaults(cC7Line);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor2Inv1, "AP", "INV", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-200M, -20M, -10M, -220M, AUD, 1, Now, ZBool.False, Creditor2, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC2Line = creditor2Inv1.FindTransactionLine("CST", CC2, Job.PK);
			AssertTransactionLineValues(cC2Line, "CST", 1, "Charge Code 2", -200M, GST1, -20M, WHT1, -10M, -220M, AUD, 1, Now, Now,
				ZBool.False, creditor2Inv1, Job, CC2, CC2.CostAccount, Creditor2);
			AssertTransactionLineDefaults(cC2Line);

			#endregion

			#region Creditor 3 Invoice 1

			APCreditNote creditor3Inv1 = transactions.RetrieveAPCreditNote(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);

			AssertTransactionHeaderValues(creditor3Inv1, "AP", "CRD", "1", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				300M, 0M, 15M, 300M, AUD, 1, Now, ZBool.False, Creditor3, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertTransactionHeaderDefaults(creditor2Inv1);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine("CST", CC3, Job.PK);
			AssertTransactionLineValues(cC3Line, "CST", 1, "Charge Code 3", 300M, GSTFREE1, 0M, WHT1, 15M, 300M, AUD, 1, Now, Now,
				ZBool.False, creditor3Inv1, Job, CC3, CC3.CostAccount, Creditor3);
			AssertEquals("Charge AP Link", cC3Line.PK, Charge3.JR_AL_APLine);
			AssertTransactionLineDefaults(cC3Line);

			#endregion

			#region Creditor 1 Invoice 2

			APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
			AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);

			AssertTransactionHeaderValues(creditor1Inv2, "AP", "INV", "2", "Z00001000", Now.AddDays(10), Now.AddDays(20),
				-250M, -25M, 0M, -110M, GBP, .4M, Now, ZBool.False, Creditor1, Job,
				"COD", 0, ZString.Empty, ZString.Empty, null, ZBool.False);
			AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(creditor1Inv2);

			TransactionLine cC5Line = creditor1Inv2.FindTransactionLine("CST", CC5, Job.PK);
			AssertTransactionLineValues(cC5Line, "CST", 1, "Charge Code 5", -250M, GST1, -25M, WHTFREE1, 0M, -110M, GBP, .4M, Now, Now,
				ZBool.False, creditor1Inv2, Job, CC5, CC5.CostAccount, Creditor1);
			AssertEquals("Charge AP Link", cC5Line.PK, Charge5.JR_AL_APLine);
			AssertTransactionLineDefaults(cC5Line);

			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", false, Charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", false, Charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", false, Charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", false, Charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", false, Charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", false, Charge6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 WIP Reversed", false, Charge7WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 2 Accrual Reversed", true, charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", true, charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 Accrual Reversed", true, charge7Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#region TestCreateInvoicesCostsOnlyAsUAInvoices

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesCostsOnlyAsUAInvoices()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertTestCreateInvoicesCostsOnlyAsUAInvoices(false);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateInvoicesCostsOnlyAsRequestsWhenChargeApprovalActivated()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertTestCreateInvoicesCostsOnlyAsUAInvoices(true);
		}

		void AssertTestCreateInvoicesCostsOnlyAsUAInvoices(bool isChargeApprovalActivated)
		{
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			SetupCharges();

			SetupAPInvoiceInfo();
			Factory.Save();

			InitializeWIPAccruals();

			AssertNull("Not Accrual from Charge 1", Charge1.Accrual);
			AccTransactionLines charge2Accrual = Charge2.Accrual;
			AssertNull("Not Accrual from Charge 3", Charge3.Accrual);
			AccTransactionLines charge5Accrual = Charge5.Accrual;
			AssertNull("Not Accrual from Charge 6", Charge6.Accrual);
			AccTransactionLines charge7Accrual = Charge7.Accrual;

			var guiWrapperMock = new Mock<IPostingJobTransactionsApprovalGUIProvider>();
			if (isChargeApprovalActivated)
			{
				guiWrapperMock.Setup(m => m.IsForPreviewOnly).Returns(false);
				guiWrapperMock.Setup(m => m.IsBulkPosting).Returns(false);
				guiWrapperMock.Setup(m => m.ShowLoginFormForTest).Returns(true);
				var securityProviderMock = new Mock<ISecurityOverrideProviderWithApprovalRequest>();
				securityProviderMock.Setup(m => m.ShouldApprovalRequestBeCreated).Returns(true);
				guiWrapperMock.Setup(m => m.GetNewSecurityOverrideProvider(It.IsAny<bool>(), It.IsAny<bool>())).Returns(securityProviderMock.Object);
				guiWrapperMock.Setup(m => m.GetParentIdAndTableCodeForJobPostingAction()).Returns(Tuple.Create(Job.PK, (ZString)Job.TablePrefix));
				APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights_ForTestOnly = x => Env.Security.APInvoiceApproval_FirstApproval.IsAllowed;
				guiWrapperMock.Setup(m => m.ShowPostingConfirmationForm(It.IsAny<APInvoiceCharges[]>())).Returns((ZDialogResult)DialogResult.OK);
			}
			APInvoiceCreator creator = new APInvoiceCreator(Job, guiWrapperMock.Object);
			APCreditNoteCreator creditNoteCreator = new APCreditNoteCreator(Job);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();

			SetUpRegistryForTest();
			try
			{
				if (isChargeApprovalActivated)
				{
					Env.Security.APInvoiceApproval_FirstApproval.IsAllowed = false;
				}
				else
				{
					Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
				}
				creator.CreateTransactions(transactions);
				AssertEquals("Invoice Count", isChargeApprovalActivated ? 3 : 4, transactions.Count);
				AssertEquals("AP Invoices Count", 3, transactions.APTransactionsCount);
				AssertEquals("Receivable Transactions Count", 0, transactions.ARTransactionsCount);
				creditNoteCreator.CreateTransactions(transactions);
			}
			finally
			{
				ResetRegistryForTest();
			}

			AssertEquals("Invoice Count", isChargeApprovalActivated ? 3 : 4, transactions.Count);
			AssertEquals("AP Invoices Count", 3, transactions.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 0, transactions.ARTransactionsCount);
			var requests = transactions.GetAllAPInvoiceApprovalRequests();
			AssertEquals("Request Count after finalizing the operation", isChargeApprovalActivated ? 1 : 0, requests.Length);

			#region Creditor 1 Invoice 1

			APInvoice creditor1Inv1 = transactions.RetrieveAPInvoice(Creditor1, "1");
			AssertEquals("Invoice Line Count", 2, creditor1Inv1.Lines.Count);
			Assert("Should be APInvoice", creditor1Inv1 is APInvoice);

			TransactionLine cC1Line = creditor1Inv1.FindTransactionLine(ZArchitecture.Core.TransactionLineTypes.Cost, CC1, Job.PK);
			Assert("Should be APInvoiceLine", cC1Line is APInvoiceLine);

			TransactionLine cC7Line = creditor1Inv1.FindTransactionLine(ZArchitecture.Core.TransactionLineTypes.Cost, CC7, Job.PK);
			Assert("Should be APInvoiceLine", cC7Line is APInvoiceLine);

			#endregion

			#region Creditor 2 Invoice 1

			APInvoice creditor2Inv1 = transactions.RetrieveAPInvoice(Creditor2, "1");
			AssertEquals("Invoice Line Count", 1, creditor2Inv1.Lines.Count);
			Assert("Should be APInvoice", creditor2Inv1 is APInvoice);

			TransactionLine cC2Line = creditor2Inv1.FindTransactionLine(ZArchitecture.Core.TransactionLineTypes.Cost, CC2, Job.PK);
			Assert("Should be APInvoiceLine", cC2Line is APInvoiceLine);

			#endregion

			#region Creditor 3 Invoice 1

			APCreditNote creditor3Inv1 = transactions.RetrieveAPCreditNote(Creditor3, "1");
			AssertEquals("Invoice Line Count", 1, creditor3Inv1.Lines.Count);
			Assert("Should be APCreditNote", creditor3Inv1 is APCreditNote);

			TransactionLine cC3Line = creditor3Inv1.FindTransactionLine(ZArchitecture.Core.TransactionLineTypes.Cost, CC3, Job.PK);
			Assert("Should be APCreditNoteLine", cC3Line is APCreditNoteLine);

			cC3Line = creditor3Inv1.FindTransactionLine(TransactionLineTypes.UnapprovedCost, CC3, Job.PK);
			AssertNull("Should be approved", cC3Line);

			#endregion

			#region Creditor 1 Invoice 2

			if (isChargeApprovalActivated)
			{
				var request = requests.Where(x => x.PostingDetails.Creditor == Creditor1.OH_Code && x.PostingDetails.TransactionNumber == "2").FirstOrDefault();
				AssertEquals("Request lines", 1, request.PostingDetails.Charges.Count);
				var charges = request.PostingDetails.Charges.Cast<APInvoiceChargesApprovalRequestChargeDetails>();
				var charge = charges.FirstOrDefault(x => x.ChargeCode == CC5.AC_Code && x.JobNumber == Job.JH_JobNum);
				AssertNotNull("Should be created correct charge.", charge);
			}
			else
			{
				APInvoice creditor1Inv2 = transactions.RetrieveAPInvoice(Creditor1, "2");
				AssertEquals("Invoice Line Count", 1, creditor1Inv2.Lines.Count);
				Assert("Should be UAInvoice", creditor1Inv2.AH_Ledger == ZArchitecture.Core.LedgerTypes.UnapprovedPayableTransactions);
				Assert("Should be UAInvoice", creditor1Inv2.AH_TransactionType == ZArchitecture.Core.TransactionTypes.UAInvoice);
				TransactionLine cC5Line = creditor1Inv2.FindTransactionLine(TransactionLineTypes.UnapprovedCost, CC5, Job.PK);
				AssertNotNull("Should be UAInvoiceLine", cC5Line);
			}
			#endregion

			#region WIP and Accrual Reversal Assertions

			AssertEquals("Charge 1 WIP Reversed", false, Charge1WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 2 WIP Reversed", false, Charge2WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 3 WIP Reversed", false, Charge3WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 4 WIP Reversed", false, Charge4WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 WIP Reversed", false, Charge5WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 6 WIP Reversed", false, Charge6WIP.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 WIP Reversed", false, Charge7WIP.AL_ReverseDate.IsValid);

			AssertEquals("Charge 2 Accrual Reversed", true, charge2Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 5 Accrual Reversed", !isChargeApprovalActivated, charge5Accrual.AL_ReverseDate.IsValid);
			AssertEquals("Charge 7 Accrual Reversed", true, charge7Accrual.AL_ReverseDate.IsValid);

			#endregion
		}

		#endregion

		#endregion

		#region TEST: Create Credit Note Copies Correctly

		public void TestCreateCreditNoteCopiesCorrectly()
		{
			TransactionCreatorHashtable hashtable = new TransactionCreatorHashtable();
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_RX_NKAPDefltCurrency = currency.RX_Code;

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_TransactionNum = "00001";
			aPInv.AH_OH = org.PK;
			aPInv.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			aPInv.AH_InvoiceAmount = 10m;

			hashtable.AddAPInvoice(aPInv, org.OH_Code, "00001");

			APCreditNoteCreator creator = new APCreditNoteCreator(Array.Empty<Job>(), Factory);
			creator.CreateTransactions(hashtable);
			AssertEquals("There should be 1 transaction in the Hashtable", 1, hashtable.APTransactionsCount);
			AssertEquals("Receivable Transactions Count", 0, hashtable.ARTransactionsCount);

			APCreditNote aPCrd = null;
			foreach (object value in hashtable.Values)
			{
				aPCrd = value as APCreditNote;
			}
			AssertNotNull("The transaction in the hashtable should be an APCreditNote", aPCrd);
			AssertEquals("APCreditNote currency should be Local currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, aPCrd.AH_RX_NKTransactionCurrency);
		}

		#endregion

		#region Create Credit Note Line With Invoice Message

		public void TestCreateCreditNoteWithInvoiceMessage()
		{
			Job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);

			var charge1 = Job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OSCostAmt = -100m;
			charge1.JR_OH_CostAccount = Creditor1.PK;
			charge1.JR_APInvoiceNum = "inv123";
			charge1.JR_APInvoiceDate = ZDateTime.Now;
			charge1.JR_A9_CostVATClass = TestObjectCreator.TaxMsg1.PK;

			Factory.Save();

			var creator = new APInvoiceCreator(Job);
			var creditNoteCreator = new APCreditNoteCreator(Job);
			var transactions = new TransactionCreatorHashtable();
			creator.CreateTransactions(transactions);
			creditNoteCreator.CreateTransactions(transactions);

			var invoices = transactions.GetAllAPInvoicesAndCreditNotes();
			AssertEquals("Should only be 1 item posted", 1, invoices.Length);
			Assert("Should be a credit note", invoices[0] is APCreditNote);
			var creditNote = (APCreditNote)invoices[0];
			AssertEquals("Should copy the invoice message.", TestObjectCreator.TaxMsg1.PK, creditNote.Lines[0].AL_A9_VATClass);
		}

		public void TestSetCreditNotValues()
		{
			Job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);

			var charge1 = Job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_RX_NKCostCurrency = TestObjectCreator.AUD.RX_Code;
			charge1.JR_OSCostAmt = -100m;
			charge1.JR_OH_CostAccount = Creditor1.PK;
			charge1.JR_APInvoiceNum = "inv123";
			charge1.JR_APInvoiceDate = ZDateTime.Now;
			charge1.JR_A9_CostVATClass = TestObjectCreator.TaxMsg1.PK;

			Factory.Save();

			var creator = new APInvoiceCreator(Job);
			var creditNoteCreator = new APCreditNoteCreator(Job);
			var transactions = new TransactionCreatorHashtable();

			creator.CreateTransactions(transactions);
			creditNoteCreator.CreateTransactions(transactions);

			var invoices = transactions.GetAllAPInvoicesAndCreditNotes();
			var creditNote = (APCreditNote)invoices[0];

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Enterprise.Core.Constants.CountryCodes.Peru)
			{
				AssertEquals(true, creditNote.AH_ComplianceSubType == PeruComplianceInfo.ComplianceSubTypeCodes.TCR);
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Enterprise.Core.Constants.CountryCodes.Mexico)
			{
				AssertEquals(true, creditNote.AH_ComplianceSubType == MexicoComplianceInfo.ComplianceSubTypeCodes.TCR);
			}
			else
			{
				AssertEquals(true, creditNote.AH_ComplianceSubType == "");
			}
		}

		#endregion

		#region Test UseJobExchangeRateDefault is False AH_PostedToEFT(UseJobExchangeRate) should be still true

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateTransactions_UseJobExchangeRateDefaultIsFalse_JobCharge()
		{
			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);

			var shipment = TestObjectCreator.CreateShipment("S00001000");
			var shipmentJob = TestObjectCreator.CreateJob(shipment, localClientOrg: LocalClient);
			var charge = TestObjectCreator.CreateCharge(shipmentJob, CC1, "desc1", USD, -80M, Creditor1, USD, -100M, LocalClient);
			SetAPInvoiceInfo(charge, "123", Now.AddDays(10), Now.AddDays(20));
			Factory.Save();

			var creator = new APInvoiceCreator(shipmentJob);
			var creditNoteCreator = new APCreditNoteCreator(shipmentJob);
			var transactions = new TransactionCreatorHashtable();

			creator.CreateTransactions(transactions);
			creditNoteCreator.CreateTransactions(transactions);
			Assert("Charge posted.", charge.IsCostPosted);

			var apTransactions = transactions.GetAllAPInvoicesAndCreditNotes();
			AssertEquals("PreCondition", 1, transactions.Count);
			AssertEquals("PreCondition", 1, transactions.GetAllAPCreditNotes().Length);
			AssertEquals("PreCondition", 1, apTransactions.Length);
			AssertEquals("Since transaction is posted from job, we should always enable UseJobExchangeRate", true, apTransactions[0].UseJobExchangeRate);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateTransactions_UseJobExchangeRateDefaultIsFalse_ConsolCost()
		{
			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);

			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var shipmentJob = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.USD, 1.1234m, -150M, creditor: TestObjectCreator.Creditor1);
			consolCost.E6_InvoiceNum = "INV2";
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			consolCost.E6_PaymentDate = ZDateTime.Today.AddDays(1);
			AssertEquals("PreCondition", 1, consolCost.ApportionmentCharges.Count);
			Factory.Save();

			var creator = new APInvoiceCreator(shipmentJob, consol, false, new JobConsolCostCollection(Factory, consol), true);
			var creditNoteCreator = new APCreditNoteCreator(shipmentJob);
			var transactions = new TransactionCreatorHashtable();

			creator.CreateTransactions(transactions);
			creditNoteCreator.CreateTransactions(transactions);
			Assert("Charge posted.", consolCost.ApportionmentCharges[0].IsCostPosted);

			var apTransactions = transactions.GetAllAPInvoicesAndCreditNotes();
			AssertEquals("PreCondition", 1, transactions.Count);
			AssertEquals("PreCondition", 1, transactions.GetAllAPCreditNotes().Length);
			AssertEquals("PreCondition", 1, apTransactions.Length);
			AssertEquals("Since transaction is posted from job, we should always enable UseJobExchangeRate", true, apTransactions[0].UseJobExchangeRate);
		}

		#endregion

		#region Implementation

		void SetupCharges()
		{
			Job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(Job, USD, .7M);
			ExchangeRate rate2 = CreateExchangeRate(Job, GBP, .4M);

			Charge1 = CreateCharge(Job, CC1, "Charge Code 1", AUD, -100M, Creditor1, AUD, 150M, LocalClient);
			Charge2 = CreateCharge(Job, CC2, "Charge Code 2", AUD, 200M, Creditor2, AUD, 200M, LocalClient);
			Charge3 = CreateCharge(Job, CC3, "Charge Code 3", AUD, -300M, Creditor3, AUD, 350M, Agent);
			Charge4 = CreateCharge(Job, CC4, "Charge Code 4", null, 0M, null, USD, 500M, Agent);
			Charge5 = CreateCharge(Job, CC5, "Charge Code 5", GBP, 100M, Creditor1, GBP, 125M, LocalClient);
			Charge6 = CreateCharge(Job, CC6, "Charge Code 6", USD, -200M, Creditor2, USD, 275M, Agent);
			Charge7 = CreateCharge(Job, CC7, "Charge Code 7", AUD, 200M, Creditor1, AUD, 300M, LocalClient);
		}

		void SetupAPInvoiceInfo()
		{
			SetAPInvoiceInfo(Charge1, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(Charge2, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(Charge3, "1", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(Charge5, "2", Now.AddDays(10), Now.AddDays(20));
			SetAPInvoiceInfo(Charge7, "1", Now.AddDays(10), Now.AddDays(20));
		}

		void InitializeWIPAccruals()
		{
			Charge1WIP = Charge1.WIP;
			Charge2WIP = Charge2.WIP;
			Charge3WIP = Charge3.WIP;
			Charge4WIP = Charge4.WIP;
			Charge5WIP = Charge5.WIP;
			Charge6WIP = Charge6.WIP;
			Charge7WIP = Charge7.WIP;
		}

		#region Registry Setup

		PaymentTwelveLevelAuthorisationSettings GetNewAuthorisationSetting(PaymentTwelveLevelAuthorisationSettingsCollection collection,
			ZString range, ZInt amount, ZString requirement)
		{
			var newSetting = collection.AddNew();
			newSetting.Amount = (ZDecimal)amount;
			newSetting.AuthorisationRequirement = requirement;
			newSetting.Range = range;

			return newSetting;
		}

		protected void SetUpRegistryForTest()
		{
			OriginalRegistryValueBeforeTest = AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.Value;

			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var upTo = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 250, AuthorisationCodes.NoApprovalRequired);
			var above = GetNewAuthorisationSetting(valuesForTest, RangeCodes.Above, 250, AuthorisationCodes.FirstApprovalRequiredOnly);

			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		void ResetRegistryForTest()
		{
			if (OriginalRegistryValueBeforeTest != null)
			{
				AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, OriginalRegistryValueBeforeTest);
			}
		}

		PaymentTwelveLevelAuthorisationSettingsCollection OriginalRegistryValueBeforeTest;

		#endregion

		Job Job;
		Charge Charge1;
		Charge Charge2;
		Charge Charge3;
		Charge Charge4;
		Charge Charge5;
		Charge Charge6;
		Charge Charge7;

		AccTransactionLines Charge1WIP;
		AccTransactionLines Charge2WIP;
		AccTransactionLines Charge3WIP;
		AccTransactionLines Charge4WIP;
		AccTransactionLines Charge5WIP;
		AccTransactionLines Charge6WIP;
		AccTransactionLines Charge7WIP;

		#endregion
	}
}
