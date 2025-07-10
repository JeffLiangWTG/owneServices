using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ExchangeRate = Enterprise.Accounting.Business.JobInvoicing.ExchangeRate;

namespace Enterprise.Accounting.Utility.Testing
{
	public abstract class TransactionCreatorBaseTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			ChequeCount = 0;
			ChequeNumber = 0;

			base.SetUp();
			OldIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			OldIsWHTRegistered = GlbCompany.CurrentCompany.GC_IsWHTRegistered;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;

			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			TestObjectCreator = new TestObjectCreator(Factory);

			Now = ZDateTime.Now;
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = OldIsWHTRegistered;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = OldIsGSTRegistered;
		}

		protected string GetContentsStringPayable(TransactionCreatorHashtable transactions)
		{
			string message = "No payable transactions created.";

			if (transactions.APTransactionsCount > 0)
			{
				message = "Payable transactions:\r\n";
				foreach (TransactionHeader head in transactions.GetAllAPTransactions())
				{
					message = message + head.ToString() + "...\r\n";
				}
				message = message + transactions.APTransactionsCount + " created in total.";
			}

			return message;
		}

		#region TransactionHeader and TransactionLine Assertions

		protected void AssertInvoicesContainConsolidatedInvoiceRef(InvoicingBaseCollection invoices, string consolidatedInvoiceRef)
		{
			bool found = false;

			foreach (InvoicingBase invoice in invoices)
			{
				if (invoice.AH_ConsolidatedInvoiceRef == consolidatedInvoiceRef)
				{
					found = true;
					break;
				}
			}

			AssertEquals("Invoice for Consolidated Ref should be found: " + consolidatedInvoiceRef, true, found);
		}

		protected void AssertTransactionHeaderValues(AccTransactionHeader header, string ledger, string type, string number, string description, ZDateTime invoiceDate, ZDateTime dueDate, decimal invoiceAmount, decimal gSTAmount, decimal wHTAmount, decimal oSTotal, RefCurrency currency, decimal exchangeRate, ZDateTime postDate, bool isDisbursment, OrgHeader client, JobHeader job, string invoiceTerm, int invoiceTermDays, string chequeOrReference, string receiptType, AccBankAccount bankAccount, bool paymentApproved)
		{
			AssertEquals("AH_Ledger", ledger, header.AH_Ledger);
			AssertEquals("AH_TransactionType", type, header.AH_TransactionType);
			if (header is APInvoice)
			{
				AssertEquals("AH_TransactionNum", number, header.AH_TransactionNum);
			}
			AssertContains("AH_Desc", description.ToUpper(), header.AH_Desc.ToUpper());
			AssertZDatesWithin5Minutes("AH_InvoiceDate", invoiceDate, header.AH_InvoiceDate);
			AssertZDatesWithin5Minutes("AH_DueDate", dueDate, header.AH_DueDate);
			AssertEquals("AH_InvoiceAmount", invoiceAmount, header.AH_InvoiceAmount);
			AssertEquals("AH_GSTAmount", gSTAmount, header.AH_GSTAmount);
			AssertEquals("AH_WithholdingTax", wHTAmount, header.AH_WithholdingTax);
			AssertEquals("AH_OSTotal", oSTotal, header.AH_OSTotal);
			AssertEquals("AH_RX_NKTransactionCurrency", currency.RX_Code, header.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_ExchangeRate", exchangeRate, header.AH_ExchangeRate);
			AssertZDatesWithin5Minutes("AH_PostDate", postDate, header.AH_PostDate);
			AssertEquals("AH_IsDisbursementCalc", isDisbursment, header.AH_IsDisbursementCalc);
			AssertEquals("AH_OH", client != null ? client.PK : ZGuid.Empty, header.AH_OH);
			AssertEquals("AH_JH", job != null ? job.PK : ZGuid.Empty, header.AH_JH);
			AssertEquals("AH_InvoiceTerm", invoiceTerm, header.AH_InvoiceTerm);
			AssertEquals("AH_InvoiceTermDays", invoiceTermDays, header.AH_InvoiceTermDays);
			AssertEquals("AH_ChequeOrReference", chequeOrReference, header.AH_ChequeOrReference);
			AssertEquals("AH_ReceiptType", receiptType, header.AH_ReceiptType);
			AssertEquals("AH_AB", bankAccount != null ? bankAccount.PK : ZGuid.Empty, header.AH_AB);
		}

		protected void AssertTransactionHeaderDefaults(AccTransactionHeader header)
		{
			AssertTransactionHeaderDefaults(header, false);
		}

		protected void AssertOverseaAPTransactionHeaderDefaults_ForBasePostManager(AccTransactionHeader header, bool isAgentConsolInvoice = false)
		{
			AssertEquals("AH_Ledger", LedgerTypes.AccountsPayable, header.AH_Ledger);
			AssertNotEquals("OverseaCurrency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, header.AH_RX_NKTransactionCurrency);
			AssertTransactionHeaderDefaults(header, isAgentConsolInvoice, true);
		}

		protected void AssertTransactionHeaderDefaults(AccTransactionHeader header, bool isAgentConsolInvoice)
			=> AssertTransactionHeaderDefaults(header, isAgentConsolInvoice, false);

		void AssertTransactionHeaderDefaults(AccTransactionHeader header, bool isAgentConsolInvoice, bool useJobExchangeRate)
		{
			AssertEquals("AH_TransactionCount", (byte)1, header.AH_TransactionCount);
			//AssertEquals("AH_TransactionReference", "", Header.AH_TransactionReference);
			AssertEquals("AH_AgePeriod", 0, header.AH_AgePeriod);
			AssertEquals("AH_PostPeriod", 0, header.AH_PostPeriod);
			AssertEquals("AH_CashBasisGSTIndicator", ZBool.False, header.AH_CashBasisGSTIndicator);
			AssertEquals("AH_CashBasisGSTRealisedToGL", ZBool.False, header.AH_CashBasisGSTRealisedToGL);
			AssertEquals("AH_ChequeDrawer", "", header.AH_ChequeDrawer);
			AssertEquals("AH_DrawerBank", "", header.AH_DrawerBank);
			AssertEquals("AH_DrawerBranch", "", header.AH_DrawerBranch);
			AssertEquals("AH_GB", GlbBranch.CurrentBranch.PK, header.AH_GB);
			if (!isAgentConsolInvoice)
			{
				ZGuid expectedDepartment = header.AH_TransactionType != TransactionTypes.Payment &&
					!(header.AH_TransactionCategory == "FID" || header.AH_TransactionCategory == "CUD") ?
					TestObjectCreator.FESDepartment.PK :
					GlbDepartment.CurrentDepartment.PK;
				AssertEquals("AH_GE", expectedDepartment, header.AH_GE);
			}
			AssertEquals("AH_AG", ZGuid.Empty, header.AH_AG);

			AssertEquals("AH_InvoicePrinted", ZBool.False, header.AH_InvoicePrinted);
			AssertEquals("AH_IsCancelled", ZBool.False, header.AH_IsCancelled);
			//AssertEquals("AH_IsClearedInCashbook", ZBool.False, Header.AH_IsClearedInCashbook);
			AssertEquals("AH_DateClearedInCashbook", ZDateTime.Empty, header.AH_DateClearedInCashbook);
			AssertEquals("AH_NotAllocated", ZBool.False, header.AH_NotAllocated);

			AssertEquals("AH_POST1", ZBool.False, header.AH_POST1);
			AssertEquals("AH_POST2", ZBool.False, header.AH_POST2);
			AssertEquals("AH_POST3", ZBool.False, header.AH_POST3);
			AssertEquals("AH_POST4", ZBool.False, header.AH_POST4);
			AssertEquals("AH_PostedToEFT", useJobExchangeRate, header.AH_PostedToEFT);
			AssertEquals("AH_PostToGL", "N", header.AH_PostToGL);
			AssertEquals("AH_ReceiptBatchNo", "", header.AH_ReceiptBatchNo);
			AssertEquals("AH_TransactionBelongsToGroup", ZGuid.Empty, header.AH_TransactionBelongsToGroup);
		}

		protected void AssertTransactionLineValues(TransactionLine line, string type, int sequence, string description, decimal lineAmount, AccTaxRate gST, decimal gSTAmount,
			AccWithholding wHT, decimal wHTAmount, decimal oSAmount, RefCurrency currency, decimal exchangeRate, ZDateTime postDate, ZBool preventInvoicePrintGrouping,
			AccTransactionHeader transactionHeader, JobHeader job, AccChargeCode chargeCode, AccGLHeader gLHeader, OrgHeader client)
		{
			AssertTransactionLineValues(line, type, sequence, description, lineAmount, gST, gSTAmount, wHT, wHTAmount, oSAmount, currency, exchangeRate, postDate, ZDateTime.Empty, preventInvoicePrintGrouping, transactionHeader, job, chargeCode, gLHeader, client);
		}

		protected void AssertTransactionLineValues(TransactionLine line, string type, int sequence, string description, decimal lineAmount, AccTaxRate gST, decimal gSTAmount,
			AccWithholding wHT, decimal wHTAmount, decimal oSAmount, RefCurrency currency, decimal exchangeRate, ZDateTime postDate, ZDateTime reverseDate, ZBool preventInvoicePrintGrouping,
			AccTransactionHeader transactionHeader, JobHeader job, AccChargeCode chargeCode, AccGLHeader gLHeader, OrgHeader client)
		{
			AssertEquals("AL_LineType", type, line.AL_LineType);
			AssertEquals("AL_Sequence", sequence, line.AL_Sequence);
			AssertEquals("AL_Desc", description, line.AL_Desc);
			AssertEquals("AL_LineAmount", lineAmount, line.AL_LineAmount);
			AssertEquals("AL_AT", gST != null ? gST.PK : ZGuid.Empty, line.AL_AT);
			AssertEquals("AL_GSTVAT", gSTAmount, line.AL_GSTVAT);
			AssertEquals("AL_AW", wHT != null ? wHT.PK : ZGuid.Empty, line.AL_AW);
			AssertEquals("AL_WithholdingTax", wHTAmount, line.AL_WithholdingTax);
			AssertEquals("AL_OSAmount", oSAmount, line.AL_OSAmount);
			AssertEquals("AL_RX_NKTransactionCurrency", currency.RX_Code, line.AL_RX_NKTransactionCurrency);
			if (!line.IsInDatabase)
			{
				AssertEquals("AL_ExchangeRate", exchangeRate, line.AL_ExchangeRate);
			}
			else
			{
				int decimals = AccTransactionLinesSchema.AL_ExchangeRate.Scale;
				AssertEquals("AL_ExchangeRate", Environment.Env.CurrentCompany.ExchangeRate.GetRate(line.AL_LocalExTaxAmount, line.AL_OSExTaxAmount, decimals), line.AL_ExchangeRate);
			}

			AssertZDatesWithin5Minutes("AL_PostDate", postDate, line.AL_PostDate);
			AssertZDatesWithin5Minutes("AL_ReverseDate", reverseDate, line.AL_ReverseDate);
			AssertEquals("AL_PreventInvoicePrintGrouping", preventInvoicePrintGrouping, line.AL_PreventInvoicePrintGrouping);
			AssertEquals("AL_AH", transactionHeader.PK, line.AL_AH);
			AssertEquals("AL_JH", job != null ? job.PK : ZGuid.Empty, line.AL_JH);
			AssertEquals("AL_AC", chargeCode != null ? chargeCode.PK : ZGuid.Empty, line.AL_AC);
			AssertEquals("AL_AG", gLHeader != null ? gLHeader.PK : ZGuid.Empty, line.AL_AG);
			AssertEquals("AL_OH", client != null ? client.PK : ZGuid.Empty, line.AL_OH);
		}

		protected void AssertTransactionLineDefaults(AccTransactionLines line)
		{
			AssertEquals("AL_UnitQty", 0, line.AL_UnitQty);
			AssertEquals("AL_UnitPrice", 0M, line.AL_UnitPrice);
			AssertEquals("AL_OSUnitPrice", 0M, line.AL_OSUnitPrice);
			AssertEquals("AL_PostPeriod", 0, line.AL_PostPeriod);
			AssertEquals("AL_PostToGL", "N", line.AL_PostToGL);
			AssertEquals("AL_ReversePeriod", 0, line.AL_ReversePeriod);
			AssertEquals("AL_ReverseToGL", "N", line.AL_ReverseToGL);
			ZGuid expectedDepartment = line.AL_JH.IsEmpty ? GlbDepartment.CurrentDepartment.PK : TestObjectCreator.FESDepartment.PK;
			AssertEquals("AL_GE", expectedDepartment, line.AL_GE);
			AssertEquals("AL_GB", GlbBranch.CurrentBranch.PK, line.AL_GB);
			AssertEquals("AL_AG_PercentOf", ZGuid.Empty, line.AL_AG_PercentOf);
			AssertEquals("AL_PercentageOfPeriod", 0, line.AL_PercentageOfPeriod);
		}

		protected void AssertAPInvoiceShowsAsPaid(AccTransactionHeader header)
		{
			AssertEquals("AH_FullyPaidDate", Now.Date, header.AH_FullyPaidDate.Date);
			AssertEquals("AH_OutstandingAmount", 0M, header.AH_OutstandingAmount);
		}

		protected void AssertMatchLinkDefaults(AccTransactionMatchLink matchLink)
		{
			AssertEquals("GST Realised", 0M, matchLink.AP_GSTRealised);
			AssertEquals("Match Period", 0, matchLink.AP_MatchPeriod);
			AssertEquals("Reason", ZString.Empty, matchLink.AP_Reason.Trim());
		}

		protected void AssertInvoiceAndPaymentAreInSameGroup(TransactionMatchLinkCollection links, TransactionMatchLink invoiceLink, TransactionMatchLink paymentLink)
		{
			AssertEquals("Invoice Link Found", true, LinkFound(links, invoiceLink));
			AssertEquals("Payment Link Found", true, LinkFound(links, paymentLink));
		}

		protected bool LinkFound(TransactionMatchLinkCollection links, TransactionMatchLink linkToFind)
		{
			foreach (TransactionMatchLink link in links)
			{
				if (link.PK == linkToFind.PK)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Create Business Objects

		protected ForwardingConsol CreateConsol(string origin, string destination, string consolNum)
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = origin;
			transport.JW_RL_NKDiscPort = destination;
			transport.JW_ETD = Now.AddDays(10);
			transport.JW_ETA = Now.AddDays(13);
			transport.JW_VoyageFlight = "QF105";
			consol.JK_UniqueConsignRef = consolNum;
			Factory.Save();
			return consol;
		}

		protected Job CreateJob(ZString jobNumber, OrgHeader localClient, decimal localClientCFX, OrgHeader agent, decimal agentCFX)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			SetJobDetails(job, jobNumber, localClient, localClientCFX, agent, agentCFX);
			return job;
		}

		protected void SetJobDetails(Job job, ZString jobNumber, OrgHeader localClient, decimal localClientCFX, OrgHeader agent, decimal agentCFX)
		{
			job.JH_JobNum = jobNumber;
			job.JH_GE = TestObjectCreator.FESDepartment.PK;
			job.LocalChargesPK = localClient != null ? localClient.PK : ZGuid.Empty;
			job.AgentCollectPK = agent != null ? agent.PK : ZGuid.Empty;
			localClient?.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", localClientCFX);
			agent?.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", agentCFX);
		}

		protected ExchangeRate CreateExchangeRate(Job parentJob, RefCurrency currency, decimal buyRate)
		{
			ExchangeRate exchangeRate = parentJob.ExchangeRates.AddNew();
			exchangeRate.JF_RX_NKRateCurrency = currency.RX_Code;
			exchangeRate.JF_BaseRate = buyRate;
			exchangeRate.JF_IsTransformed = true;
			return exchangeRate;
		}

		protected Charge CreateCharge(Job parentJob, AccChargeCode chargeCode, string desc, RefCurrency costCurrency, ZDecimal oSCostAmt, OrgHeader creditor,
			RefCurrency sellCurrency, ZDecimal oSSellAmt, OrgHeader debtor)
		{
			return this.CreateCharge(parentJob, chargeCode, desc, costCurrency, oSCostAmt, creditor, sellCurrency, oSSellAmt,
				debtor, ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice, GlbBranch.CurrentBranch.PK);
		}

		protected Charge CreateCharge(Job parentJob, AccChargeCode chargeCode, string desc, RefCurrency costCurrency, ZDecimal oSCostAmt, OrgHeader creditor,
			RefCurrency sellCurrency, ZDecimal oSSellAmt, OrgHeader debtor, string invoiceType, ZGuid branch)
		{
			if (costCurrency != null && costCurrency.RX_Code != GlbCompany.CurrentCompany.LocalCurrency.RX_Code)
			{
				var rate = parentJob.AddCurrency(costCurrency, ExchangeRateValidLedgerEnum.None);
			}

			if (sellCurrency != null && sellCurrency.RX_Code != GlbCompany.CurrentCompany.LocalCurrency.RX_Code)
			{
				parentJob.AddCurrency(sellCurrency, ExchangeRateValidLedgerEnum.None);
			}

			foreach (ExchangeRate rate in parentJob.ExchangeRates)
			{
				rate.JF_IsTransformed = true; //preventing auto replacement of the exchange rate
				if (rate.JF_BaseRate.IsEmpty)
				{
					rate.JF_BaseRate = 1m;
				}
			}

			Charge charge = parentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			if (desc != null)
			{
				charge.JR_Desc = desc;
			}

			charge.JR_OH_CostAccount = creditor != null ? creditor.PK : ZGuid.Empty;
			charge.JR_RX_NKCostCurrency = costCurrency != null ? costCurrency.RX_Code : ZString.Empty;
			charge.JR_OSCostAmt = oSCostAmt;

			charge.JR_OH_SellAccount = debtor != null ? debtor.PK : ZGuid.Empty;
			charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;
			charge.JR_InvoiceType = invoiceType;

			charge.JR_OSSellAmt = oSSellAmt;
			charge.JR_GE = TestObjectCreator.FESDepartment.PK;
			charge.JR_GB = branch;

			return charge;
		}

		protected AccChargeCode CreateChargeCode(string code, string description, string chargeType, decimal marginPercentage, AccTaxRate gST, AccWithholding wHT)
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "ZZ" + code;
			chargeCode.AC_Desc = description;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_MarginPercentage = marginPercentage;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			if (gST != null)
			{
				chargeCode.AC_AT_GSTRate = gST.PK;
			}

			if (wHT != null)
			{
				chargeCode.AC_AW_WithholdingTaxRate = wHT.PK;
			}

			FillChargeCodeWithValidGLAccountData(chargeCode);
			return chargeCode;
		}

		void FillChargeCodeWithValidGLAccountData(AccChargeCode chargeCode)
		{
			if (chargeCode.RequiredProperties(chargeCode.HighestChargeType).AccrualAccount)
			{
				chargeCode.AC_AG_AccrualAccount = GLHeader1.PK;
			}

			if (chargeCode.RequiredProperties(chargeCode.HighestChargeType).CostAccount)
			{
				chargeCode.AC_AG_CostAccount = GLHeader1.PK;
			}

			if (chargeCode.RequiredProperties(chargeCode.HighestChargeType).RevenueAccount)
			{
				chargeCode.AC_AG_RevenueAccount = GLHeader1.PK;
			}

			if (chargeCode.RequiredProperties(chargeCode.HighestChargeType).WIPAccount)
			{
				chargeCode.AC_AG_WIPAccount = GLHeader1.PK;
			}
		}

		protected AccTaxRate CreateTaxRate(string code, string description, int rate)
		{
			AccTaxRate taxRate = Factory.New<AccTaxRate>();
			taxRate.AT_Code = "ZZ" + code;
			taxRate.AT_Description = description;
			taxRate.AT_IsActive = true;
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.SetRateNumerator_ForTestOnly(rate);
			return taxRate;
		}

		protected AccWithholding CreateWithholdingTax(string code, string description, decimal rate)
		{
			AccWithholding taxRate = Factory.New<AccWithholding>();
			taxRate.AW_Code = "ZZ" + code;
			taxRate.AW_Description = description;
			taxRate.AW_IsActive = true;
			taxRate.AW_Rate = rate;
			taxRate.AW_GC = GlbCompany.CurrentCompany.PK;
			return taxRate;
		}

		protected AccChequeBook CreateChequeBook(string description, int numberOfCheques)
		{
			AccChequeBook chequeBook = Factory.New<AccChequeBook>();
			chequeBook.AK_Code = ChequeCount.ToString();
			chequeBook.AK_Desc = description;
			chequeBook.AK_AB = AUDBankAccount.PK;
			chequeBook.AK_GB = GlbBranch.CurrentBranch.PK;
			chequeBook.AK_StartNo = ChequeNumber;
			chequeBook.AK_LastNo = ChequeNumber + numberOfCheques - 1;

			ChequeNumber += numberOfCheques;
			++ChequeCount;

			return chequeBook;
		}

		protected void SetAPInvoiceInfo(Charge charge, string invoiceNumber, ZDateTime invoiceDate, ZDateTime dueDate)
		{
			charge.JR_APInvoiceNum = invoiceNumber;
			charge.JR_APInvoiceDate = invoiceDate;
			charge.JR_PaymentDate = dueDate;
		}

		protected void SetAPPaymentInfo(Charge charge, string paymentType, AccBankAccount bankAccount, string chequeOrReference)
		{
			SetAPPaymentInfo(charge, paymentType, bankAccount, chequeOrReference, null);
		}

		protected void SetAPPaymentInfo(Charge charge, string paymentType, AccBankAccount bankAccount, string chequeOrReference, AccChequeBook chequeBook)
		{
			charge.JR_PaymentType = paymentType;
			charge.JR_AB = bankAccount.PK;
			charge.JR_ChequeNo = chequeOrReference;

			if (chequeBook != null)
			{
				charge.JR_AK = chequeBook.PK;
			}
		}

		protected OrgHeader CreateOrgHeader(string code, bool creditor, bool debtor)
		{
			BusinessObjectFactory orgFactory = new BusinessObjectFactory();

			OrgHeader header = orgFactory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "Test Company Name";
			header.MainAddress.OA_Address1 = "184 Bourke Road";
			header.MainAddress.OA_City = "Alexandria";
			header.MainAddress.OA_State = "NSW";
			header.OH_Code = "Z" + code;
			header.OH_IsDebtor = debtor;
			header.OH_IsCreditor = creditor;
			if (debtor)
			{
				header.CompanyData.SetARTaxApplicable(true);
				header.MiscServ.OM_ARWHTApplicable = true;
			}
			if (creditor)
			{
				header.CompanyData.SetAPTaxApplicable(true);
				header.MiscServ.OM_APWHTApplicable = true;
			}

			orgFactory.Save();
			return header;
		}

		protected AccBankAccount CreateBankAccount(string code, string desc, string name, string abbreviation, RefCurrency currency, string bSB, string accountNumber, AccGLHeader gLHeader)
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_Code = code;
			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount.AB_Desc = desc;

			bankAccount.AB_AG = gLHeader.PK;
			bankAccount.AB_BankName = name;
			bankAccount.AB_BankAbbreviation = abbreviation;
			bankAccount.AB_BSB = bSB;
			bankAccount.AB_AccountNum = accountNumber;
			bankAccount.AB_RX_NKAccountCurrency = currency.RX_Code;

			return bankAccount;
		}

		protected AccHotCheque CreateHotCheque(Job chequeJob, string payee, ZDecimal amount, ZString description)
		{
			AccHotCheque hotCheque = Factory.New<AccHotCheque>();

			hotCheque.AQ_ChequeDate = Now;
			hotCheque.AQ_OH = Creditor1.PK;
			//HotCheque.AQ_AH 
			hotCheque.AQ_AK = ChequeBook.PK;
			hotCheque.AQ_ChequeNumber = "1";
			hotCheque.AQ_ChequePayee = payee;
			hotCheque.AQ_Amount = amount;
			hotCheque.AQ_JH = chequeJob.PK;
			hotCheque.AQ_Description = description;
			hotCheque.AQ_GS_NKResponsibleStaff = GlbStaff.CurrentUser.GS_Code;

			return hotCheque;
		}

		protected void Create2MonthPeriod()
		{
			new AccountingPeriodTestHelper(Factory).SetupSinglePeriod(1, ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
		}

		#endregion

		#region Test business objects

		#region Job

		protected Job fJob1;
		protected Job Job1
		{
			get
			{
				if (fJob1 == null)
				{
					fJob1 = CreateJob("Z00001000", LocalClient, 1M, Agent, 2M);
				}
				return fJob1;
			}
		}

		#endregion

		#region AUD

		protected RefCurrency fAUD;
		protected RefCurrency AUD
		{
			get
			{
				if (fAUD == null)
				{
					fAUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
				}
				return fAUD;
			}
		}

		#endregion

		#region USD

		protected RefCurrency fUSD;
		protected RefCurrency USD
		{
			get
			{
				if (fUSD == null)
				{
					fUSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				}
				return fUSD;
			}
		}

		#endregion

		#region GBP

		protected RefCurrency fGBP;
		protected RefCurrency GBP
		{
			get
			{
				if (fGBP == null)
				{
					fGBP = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "GBP");
				}
				return fGBP;
			}
		}

		#endregion

		#region GLHeader1

		public AccGLHeader GLHeader1
		{
			get
			{
				if (fGLHeader1 == null)
				{
					fGLHeader1 = Factory.New<AccGLHeader>();
				}

				return fGLHeader1;
			}
		}

		AccGLHeader fGLHeader1;

		#endregion

		#region CC1

		protected AccChargeCode CC1
		{
			get { return TestObjectCreator.CC1; }
		}

		#endregion

		#region CC2

		protected AccChargeCode CC2
		{
			get { return TestObjectCreator.CC2; }
		}

		#endregion

		#region CC3

		protected AccChargeCode CC3
		{
			get { return TestObjectCreator.CC3; }
		}

		#endregion

		#region CC4

		protected AccChargeCode CC4
		{
			get { return TestObjectCreator.CC4; }
		}

		#endregion

		#region CC5

		protected AccChargeCode CC5
		{
			get { return TestObjectCreator.CC5; }
		}

		#endregion

		#region CC6

		protected AccChargeCode CC6
		{
			get { return TestObjectCreator.CC6; }
		}

		#endregion

		#region CC7

		protected AccChargeCode CC7
		{
			get { return TestObjectCreator.CC7; }
		}

		#endregion

		#region CC8

		protected AccChargeCode CC8
		{
			get { return TestObjectCreator.CC8; }
		}

		#endregion

		#region CC9

		protected AccChargeCode CC9
		{
			get { return TestObjectCreator.CC9; }
		}

		#endregion

		#region Creditor 1

		protected OrgHeader Creditor1
		{
			get { return TestObjectCreator.Creditor1; }
		}

		#endregion

		#region Creditor 2

		protected OrgHeader Creditor2
		{
			get { return TestObjectCreator.Creditor2; }
		}

		#endregion

		#region Creditor 3

		protected OrgHeader Creditor3
		{
			get { return TestObjectCreator.Creditor3; }
		}

		#endregion

		#region Cheque Book

		protected AccChequeBook ChequeBook
		{
			get
			{
				if (fChequeBook == null)
				{
					fChequeBook = CreateChequeBook("Test chequebook", 100);
				}
				return fChequeBook;
			}
		}
		AccChequeBook fChequeBook;

		protected AccChequeBook AUDChequeBook
		{
			get
			{
				if (fAUDChequeBook == null)
				{
					fAUDChequeBook = CreateChequeBook("Test AUD chequebook", 100);
					fAUDChequeBook.AK_AB = AUDBankAccount.PK;
				}
				return fAUDChequeBook;
			}
		}
		AccChequeBook fAUDChequeBook;

		#endregion

		#region Hot Cheque

		protected AccHotCheque fHotCheque;
		protected AccHotCheque HotCheque
		{
			get
			{
				if (fHotCheque == null)
				{
					fHotCheque = CreateHotCheque(Job1, "Luke", 1, "Test hot cheque from TransactionCreatorBaseTest");
				}
				return fHotCheque;
			}
		}

		#endregion

		#region Local Client

		protected OrgHeader LocalClient
		{
			get { return TestObjectCreator.LocalClient; }
		}

		#endregion

		#region Local Client 2

		protected OrgHeader LocalClient2
		{
			get { return TestObjectCreator.LocalClient2; }
		}

		#endregion

		#region Agent

		protected OrgHeader Agent
		{
			get { return TestObjectCreator.Agent; }
		}

		#endregion

		#region GST1

		protected AccTaxRate GST1
		{
			get { return TestObjectCreator.GST1; }
		}

		#endregion

		#region GSTFREE1

		protected AccTaxRate GSTFREE1
		{
			get { return TestObjectCreator.GSTFREE1; }
		}

		#endregion

		#region WHT1

		protected AccWithholding WHT1
		{
			get { return TestObjectCreator.WHT1; }
		}

		#endregion

		#region WHTFREE1

		protected AccWithholding WHTFREE1
		{
			get { return TestObjectCreator.WHTFREE1; }
		}

		#endregion

		#region AUDBankAccount

		protected AccBankAccount fAUDBankAccount;
		protected AccBankAccount AUDBankAccount
		{
			get
			{
				if (fAUDBankAccount == null)
				{
					var header = Factory.NewWithValidTestData<AccGLHeader>();
					header.AG_AccountNum = "ZAUDHeader";
					fAUDBankAccount = CreateBankAccount("ZHSBCAUD", "HSBC AUD ACCT", "HSBC", "AUD", AUD, "123456", "12345678", header);
				}
				return fAUDBankAccount;
			}
		}

		#endregion

		#region USDBankAccount

		protected AccBankAccount fUSDBankAccount;
		protected AccBankAccount USDBankAccount
		{
			get
			{
				if (fUSDBankAccount == null)
				{
					var header = Factory.NewWithValidTestData<AccGLHeader>();
					header.AG_AccountNum = "ZUSDHeader";
					fUSDBankAccount = CreateBankAccount("ZHSBCUSD", "HSBC USD ACCT", "HSBC", "USD", USD, "654321", "87654321", header);
				}
				return fUSDBankAccount;
			}
		}

		#endregion

		#endregion

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;

		bool OldIsGSTRegistered;
		bool OldIsWHTRegistered;

		int ChequeCount;
		int ChequeNumber;

		protected ZDateTime Now;

		protected TestObjectCreator TestObjectCreator;
	}
}
