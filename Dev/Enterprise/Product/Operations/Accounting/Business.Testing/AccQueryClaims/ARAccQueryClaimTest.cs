using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	[TestedType(typeof(ARAccQueryClaim))]
	class ARAccQueryClaimTest : AccQueryClaimBaseTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<ARAccQueryClaim>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestApproveWhenCreditNoteIsNotAllowed()
		{
			APAccQueryClaim apClaim;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				apClaim = TestObjectCreator.CreateAPClaim(10);
				apClaim.CreateAndAttachRelatedCreditNote();
				AssertNotNull("Precondition: RelatedUnapprovedCreditNote", apClaim.RelatedUnapprovedCreditNote);
			}

			var apClaimLoadedAsAR = Factory.Load<ARAccQueryClaim>(apClaim.PK);

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(TestObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var expectedMessage = "This claim is not allowed to be approved. A Credit Note was going to be created as a result of this claim approval. Posting of Credit Notes is prevented. This is controlled by the registry setting Accounting -> Receivable Defaults -> Default Settings -> Prevent Creation of Credit Notes.";
			AssertExceptionThrown<InvalidOperationException>("Both registries are activated", expectedMessage, () => apClaimLoadedAsAR.Approve());

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(TestObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertExceptionThrown<InvalidOperationException>("Both registries are activated", expectedMessage, () => apClaimLoadedAsAR.Approve());

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(TestObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			expectedMessage = $"This claim is not allowed to be approved. This would create a Credit Note in the Claiming company (DEM). Posting of Credit Notes is not allowed in DEM.";
			AssertExceptionThrown<InvalidOperationException>("Both registries are activated", expectedMessage, () => apClaimLoadedAsAR.Approve());

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(TestObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			expectedMessage = "Multiple AR Credit Notes can't be created during approving. All charges must be posted as in one AR Credit Note.";
			AssertExceptionThrown<InvalidOperationException>("Both registries are activated", expectedMessage, () => apClaimLoadedAsAR.Approve());
		}

		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestApproveWhenRelatedLoginCompanyJobIsJobReadyForFinancialJobClosure()
		{
			var cacheValue = Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed;

			GlbBranch otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			var otherCompanyChargeCode = TestObjectCreator.CreateChargeCode("FOROTCMP", "For Other Compamy", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, otherBranch.Company);
			var currentCompanyChargeCode = TestObjectCreator.CreateChargeCode("FOROTCMP", "For current Compamy", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHT1, GlbCompany.CurrentCompany);
			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("1234");
			Job job = Job.CreateWithMutex_ForTestOnly(Factory, shipment);
			job.JH_JobNum = "111";
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			APInvoice invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m);
			invoice.AH_JH = shipment.Job.PK;
			invoice.AH_GB = otherBranch.PK;
			APInvoiceLine line = (APInvoiceLine)TestObjectCreator.CreateInvoiceLine(invoice, 100, TestObjectCreator.AUD, 1m);
			invoice.Lines.Add(line);
			line.AL_JH = shipment.Job.PK;
			line.AL_OH = TestObjectCreator.AALSHI.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_GB = invoice.Company.Branches[0].PK;
			TestObjectCreator.CreateCharge(line);
			UACreditNote creditNote = (UACreditNote)TestObjectCreator.CreateInvoice(typeof(UACreditNote), TestObjectCreator.AUD, 1m);
			creditNote.AH_JH = shipment.Job.PK;
			creditNote.AH_GB = otherBranch.PK;
			UACreditNoteLine creditNoteLine = (UACreditNoteLine)TestObjectCreator.CreateInvoiceLine(creditNote, 20, TestObjectCreator.AUD, 1m);
			creditNote.Lines.Add(creditNoteLine);
			creditNoteLine.AL_JH = shipment.Job.PK;
			creditNoteLine.AL_OH = TestObjectCreator.AALSHI.PK;
			creditNoteLine.AL_AC = otherCompanyChargeCode.PK;
			creditNoteLine.AL_GB = creditNote.Company.Branches[0].PK;

			var apClaim = Factory.New<APAccQueryClaim>();
			apClaim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			apClaim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			apClaim.AY_AH = invoice.PK;
			apClaim.AY_GB = otherBranch.PK;
			apClaim.TransactionHeader.AH_TransactionBelongsToGroup = invoice.PK;
			creditNote.AH_TransactionBelongsToGroup = invoice.PK;
			Factory.Save();

			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Factory.Save();

			var claim = Factory.Load<ARAccQueryClaim>(apClaim.PK);

			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				var exceptionMessage = "Can not approve this claim as related job has Jobs Ready for Financial Closure status.";
				AssertExceptionThrown<InvalidOperationException>("Should throw InvalidOperationException.", exceptionMessage, () => claim.Approve());
			}

			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				AssertNoExceptionThrown(() => claim.Approve());
			}
		}

		public void TestApproveThrowsExceptionWhenRelatedLoginCompanyJobIsClosed()
		{
			var otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			var otherCompanyChargeCode = TestObjectCreator.CreateChargeCode("FOROTCMP", "For Other Compamy", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, otherBranch.Company);

			var shipment = TestObjectCreator.CreateShipment("1234");
			var job = Job.CreateWithMutex_ForTestOnly(Factory, shipment);
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m);
			invoice.AH_JH = shipment.Job.PK;
			invoice.AH_GB = otherBranch.PK;

			var creditNote = (UACreditNote)TestObjectCreator.CreateInvoice(typeof(UACreditNote), TestObjectCreator.AUD, 1m);
			creditNote.AH_JH = shipment.Job.PK;
			creditNote.AH_GB = otherBranch.PK;
			var creditNoteLine = (UACreditNoteLine)TestObjectCreator.CreateInvoiceLine(creditNote, 20, TestObjectCreator.AUD, 1m);
			creditNote.Lines.Add(creditNoteLine);
			creditNoteLine.AL_JH = shipment.Job.PK;
			creditNoteLine.AL_OH = TestObjectCreator.AALSHI.PK;
			creditNoteLine.AL_AC = otherCompanyChargeCode.PK;
			creditNoteLine.AL_GB = creditNote.Company.Branches[0].PK;

			var apClaim = Factory.New<APAccQueryClaim>();
			apClaim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			apClaim.AY_AH = invoice.PK;
			apClaim.AY_GB = otherBranch.PK;
			apClaim.TransactionHeader.AH_TransactionBelongsToGroup = invoice.PK;
			creditNote.AH_TransactionBelongsToGroup = invoice.PK;

			var arClaim = Factory.Load<ARAccQueryClaim>(apClaim.PK);
			using (new DisposableAction(() => job.JH_Status = JobHeaderStatus.Closed.Code, () => job.Dispose()))
			{
				Assert("Precondition : Job is Closed", job.IsClosed);
				var exceptionMessage = "This claim is not allowed to be approved as it is linked to closed jobs. If you need to approve this claim, please reopen the Closed job then approve again.";
				AssertExceptionThrown<InvalidOperationException>("Should throw InvalidOperationException.", exceptionMessage, () => arClaim.Approve());
			}
		}

		public void TestNumberFountainNo()
		{
			ARAccQueryClaim claim = Factory.New<ARAccQueryClaim>();
			AssertEquals("Should be AR Claim number fountain", Env.NumberFountains.QueryClaimNo.PeekPreliminaryFormatted(Db.Connection), claim.NumberFountainNo_ForTestOnly.PeekPreliminaryFormatted(Db.Connection));
			using (Db.Connection.BeginTransactionWithManager())
			{
				Env.NumberFountains.QueryClaimNo.GetNextFormatted(Db.Connection);
			}

			AssertEquals("Should be AR Claim number fountain", Env.NumberFountains.QueryClaimNo.PeekPreliminaryFormatted(Db.Connection), claim.NumberFountainNo_ForTestOnly.PeekPreliminaryFormatted(Db.Connection));
		}

		public void TestApprove()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("1234");
			Job job = Job.CreateWithMutex_ForTestOnly(Factory, shipment);
			job.JH_JobNum = "111";
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_AgentDeclaredSellAmt = 2000m;
			charge.JR_AgentDeclaredCostAmt = 1000m;
			charge.JR_IsIncludedInProfitShare = true;
			GlbBranch otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			APInvoice invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m);
			invoice.AH_JH = shipment.Job.PK;
			invoice.AH_GB = otherBranch.PK;
			APInvoiceLine line = (APInvoiceLine)TestObjectCreator.CreateInvoiceLine(invoice, 100, TestObjectCreator.AUD, 1m);
			invoice.Lines.Add(line);
			line.AL_JH = shipment.Job.PK;
			line.AL_OH = TestObjectCreator.AALSHI.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_GB = invoice.Company.Branches[0].PK;
			TestObjectCreator.CreateCharge(line);
			UACreditNote creditNote = (UACreditNote)TestObjectCreator.CreateInvoice(typeof(UACreditNote), TestObjectCreator.AUD, 1m);
			creditNote.AH_JH = shipment.Job.PK;
			creditNote.AH_GB = otherBranch.PK;
			UACreditNoteLine creditNoteLine = (UACreditNoteLine)TestObjectCreator.CreateInvoiceLine(creditNote, 20, TestObjectCreator.AUD, 1m);
			creditNote.Lines.Add(creditNoteLine);
			creditNoteLine.AL_JH = shipment.Job.PK;
			creditNoteLine.AL_OH = TestObjectCreator.AALSHI.PK;
			creditNoteLine.AL_AC = TestObjectCreator.CC1.PK;
			creditNoteLine.AL_GB = creditNote.Company.Branches[0].PK;
			TestObjectCreator.CreateCharge(creditNoteLine);
			ARAccQueryClaim claim = Factory.New<ARAccQueryClaim>();
			claim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			claim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			claim.AY_AH = invoice.PK;
			Factory.Save();
			AssertEquals("2 invoices should exist", 2, Factory.GetDatabaseCount(typeof(InvoicingBase)));
			claim.Approve();
			Factory.Save();
			ARCreditNote newCreditNote = Factory.LoadTop1<ARCreditNote>(new ZQuery(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, creditNote.PK));
			AssertNotNull("New AR Credit Note should have been created", newCreditNote);
			AssertEquals("Credit Note amount", 100m, newCreditNote.AH_OSExTaxAmount);
			AssertEquals("Claim status", QueryClaimStatusCodeList.Codes.QCStatus3AcceptedAndCreditNoteIssuedAndClosed, claim.AY_QueryClaimStatus);
		}

		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestConfirmApprove()
		{
			TestObjectCreator.AALSHI.CompanyData.OB_RX_NKARDDefltCurrency = "USD";
			RefExchangeRate exchangeRate = Factory.NewWithValidTestData<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			exchangeRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.BuyRate;
			exchangeRate.RE_SellRate = 1M;
			exchangeRate.RE_RX_NKExCurrency = "USD";
			exchangeRate.RE_GC = EnvProxy.Instance.CurrentCompany.PK;
			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("1234");
			Job job = Job.CreateWithMutex_ForTestOnly(Factory, shipment);
			job.JH_JobNum = "111";
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_AgentDeclaredSellAmt = 2000m;
			charge.JR_AgentDeclaredCostAmt = 1000m;
			charge.JR_IsIncludedInProfitShare = true;
			GlbBranch otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			APInvoice invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m);
			invoice.AH_JH = shipment.Job.PK;
			invoice.AH_GB = otherBranch.PK;
			APInvoiceLine line = (APInvoiceLine)TestObjectCreator.CreateInvoiceLine(invoice, 100, TestObjectCreator.AUD, 1m);
			invoice.Lines.Add(line);
			line.AL_JH = shipment.Job.PK;
			line.AL_OH = TestObjectCreator.AALSHI.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_GB = invoice.Company.Branches[0].PK;
			TestObjectCreator.CreateCharge(line);
			UACreditNote creditNote = (UACreditNote)TestObjectCreator.CreateInvoice(typeof(UACreditNote), TestObjectCreator.USD, 1m);
			creditNote.AH_JH = shipment.Job.PK;
			creditNote.AH_GB = otherBranch.PK;
			UACreditNoteLine creditNoteLine = (UACreditNoteLine)TestObjectCreator.CreateInvoiceLine(creditNote, 20, TestObjectCreator.USD, 1m);
			creditNote.Lines.Add(creditNoteLine);
			creditNoteLine.AL_JH = shipment.Job.PK;
			creditNoteLine.AL_OH = TestObjectCreator.AALSHI.PK;
			creditNoteLine.AL_AC = TestObjectCreator.CC1.PK;
			creditNoteLine.AL_GB = creditNote.Company.Branches[0].PK;
			ARAccQueryClaim claim = Factory.New<ARAccQueryClaim>();
			claim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			claim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			claim.AY_AH = invoice.PK;
			claim.TransactionHeader.AH_TransactionBelongsToGroup = invoice.PK;
			creditNote.AH_TransactionBelongsToGroup = invoice.PK;
			Factory.Save();
			AssertNoExceptionThrown(() => claim.Approve());
		}

		public void TestReject()
		{
			ARAccQueryClaim claim = Factory.New<ARAccQueryClaim>();
			AssertNotEquals("Newly created claim shouldn't be rejected already", QueryClaimStatusCodeList.Codes.QCStatus4RejectedNotClosed, claim.AY_QueryClaimStatus);
			claim.Reject();
			AssertEquals("Should be rejected now", QueryClaimStatusCodeList.Codes.QCStatus4RejectedNotClosed, claim.AY_QueryClaimStatus);
		}

		public void TestLedger()
		{
			ARAccQueryClaim claim = Factory.New<ARAccQueryClaim>();
			AssertEquals("Details filled in", ZArchitecture.Core.LedgerTypes.AccountsReceivable, claim.Ledger);
		}

		public void TestAY_HoldOption_Readonly()
		{
			var claim = (ARAccQueryClaim)GetNewBusinessObject();
			claim.FillWithValidTestData();
			AssertEquals("AY_HoldOption is always non-editable for AR claims.", true, claim.AY_HoldOption_ReadOnly);
		}

		public void TestRaiseTransactionNumberSetForApproveClaim()
		{
			var shipment = TestObjectCreator.CreateShipment("1234");

			var job = TestObjectCreator.CreateJob(shipment);

			var invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m);
			invoice.AH_JH = job.PK;
			invoice.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var line = TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 100m);
			line.AL_GB = invoice.AH_GB;
			TestObjectCreator.CreateCharge(line);

			var creditNote = (UACreditNote)TestObjectCreator.CreateInvoice(typeof(UACreditNote), TestObjectCreator.AUD, 1m);
			creditNote.AH_JH = job.PK;
			creditNote.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			creditNote.AH_OH = TestObjectCreator.AALSHI.PK;

			var creditNoteLine = (UACreditNoteLine)TestObjectCreator.CreateInvoiceLine(creditNote, job, TestObjectCreator.CC1, 20m);
			creditNoteLine.AL_GB = creditNote.AH_GB;

			var claim = Factory.New<ARAccQueryClaim>();
			claim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			claim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			claim.AY_AH = invoice.PK;
			claim.TransactionHeader.AH_TransactionBelongsToGroup = invoice.PK;
			creditNote.AH_TransactionBelongsToGroup = invoice.PK;
			Factory.Save();

			AssertNoExceptionThrown(() => claim.Approve());

			AssertNotEquals("00001000", creditNote.AH_TransactionNum);

			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals("00001000", creditNote.AH_TransactionNum);

			Assert(!creditNote.HasContext(BusinessContext.PostUnapprovedCreditNoteForApproveClaim));

			creditNote.AH_TransactionNum = "TestChange";

			ExceptionReporterTestListener.Instance.Clear();

			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals("Critical Validation on AH_TransactionNum should not be reported", 0, ExceptionReporterTestListener.Instance.Count);
		}

		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestApprove_ChargeCodeOfUACreditNoteLineNotExistsCurrentCompany()
		{
			GlbBranch otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			var otherCompanyChargeCode = TestObjectCreator.CreateChargeCode("FOROTCMP", "For Other Compamy", Core.Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, TestObjectCreator.WHTFREE1, otherBranch.Company);
			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("1234");
			Job job = Job.CreateWithMutex_ForTestOnly(Factory, shipment);
			job.JH_JobNum = "111";
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			APInvoice invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m);
			invoice.AH_JH = shipment.Job.PK;
			invoice.AH_GB = otherBranch.PK;
			APInvoiceLine line = (APInvoiceLine)TestObjectCreator.CreateInvoiceLine(invoice, 100, TestObjectCreator.AUD, 1m);
			invoice.Lines.Add(line);
			line.AL_JH = shipment.Job.PK;
			line.AL_OH = TestObjectCreator.AALSHI.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_GB = invoice.Company.Branches[0].PK;
			TestObjectCreator.CreateCharge(line);
			UACreditNote creditNote = (UACreditNote)TestObjectCreator.CreateInvoice(typeof(UACreditNote), TestObjectCreator.AUD, 1m);
			creditNote.AH_JH = shipment.Job.PK;
			creditNote.AH_GB = otherBranch.PK;
			UACreditNoteLine creditNoteLine = (UACreditNoteLine)TestObjectCreator.CreateInvoiceLine(creditNote, 20, TestObjectCreator.AUD, 1m);
			creditNote.Lines.Add(creditNoteLine);
			creditNoteLine.AL_JH = shipment.Job.PK;
			creditNoteLine.AL_OH = TestObjectCreator.AALSHI.PK;
			creditNoteLine.AL_AC = otherCompanyChargeCode.PK;
			creditNoteLine.AL_GB = creditNote.Company.Branches[0].PK;
			ARAccQueryClaim claim = Factory.New<ARAccQueryClaim>();
			claim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			claim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			claim.AY_AH = invoice.PK;
			claim.TransactionHeader.AH_TransactionBelongsToGroup = invoice.PK;
			creditNote.AH_TransactionBelongsToGroup = invoice.PK;
			Factory.Save();

			ZQuery chargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_Code, creditNoteLine.ChargeCode.AC_Code);
			chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			var chargeCodeForCurrentCompany = Factory.LoadTop1<AccChargeCode>(chargeCodeQuery);

			AssertNull("Precondition", chargeCodeForCurrentCompany);

			var exceptionMessage = "Charge code 'ZZFOROTCMP' does not exist in current company, please create charge code before approving claim.";
			AssertExceptionThrown<InvalidOperationException>("Should throw InvalidOperationException.", exceptionMessage, () => claim.Approve());
		}

		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestApprove_NoExceptionWhenCreditorHasInvoicePostingCurrencyOnOrgInvoiceRollupOrGroup()
		{
			TestObjectCreator.AALSHI.CompanyData.OB_RX_NKARDDefltCurrency = "AUD";
			var currencyAUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.AUD, Core.Constants.ExchangeRateTypes.Code.BuyRate, 1, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			var shipment = TestObjectCreator.CreateShipment("1234");
			var job = Job.CreateWithMutex_ForTestOnly(Factory, shipment);
			job.JH_JobNum = "111";
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.LoadAccChargeCode("FRT", GlbCompany.CurrentCompany.PK), 1000m, 2000m);
			charge.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;

			var otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			var invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m);
			invoice.AH_JH = shipment.Job.PK;
			invoice.AH_GB = otherBranch.PK;

			var line = (APInvoiceLine)TestObjectCreator.CreateInvoiceLine(invoice, 100, TestObjectCreator.AUD, 1m);
			line.AL_JH = shipment.Job.PK;
			line.AL_OH = TestObjectCreator.AALSHI.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_GB = invoice.Company.Branches[0].PK;
			TestObjectCreator.CreateCharge(line);

			var creditNote = (UACreditNote)TestObjectCreator.CreateInvoice(typeof(UACreditNote), TestObjectCreator.AUD, 1m);
			creditNote.AH_JH = shipment.Job.PK;
			creditNote.AH_GB = otherBranch.PK;
			creditNote.AH_TransactionBelongsToGroup = invoice.PK;
			var creditNoteLine = (UACreditNoteLine)TestObjectCreator.CreateInvoiceLine(creditNote, 20, TestObjectCreator.AUD, 1m);
			creditNote.Lines.Add(creditNoteLine);
			creditNoteLine.AL_JH = shipment.Job.PK;
			creditNoteLine.AL_OH = TestObjectCreator.AALSHI.PK;
			creditNoteLine.AL_AC = TestObjectCreator.CC1.PK;
			creditNoteLine.AL_GB = creditNote.Company.Branches[0].PK;

			var claim = Factory.New<ARAccQueryClaim>();
			claim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			claim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			claim.AY_AH = invoice.PK;
			claim.TransactionHeader.AH_TransactionBelongsToGroup = invoice.PK;
			Factory.Save();

			var allGroup = TestObjectCreator.CreateOrgInvoiceRollupOrGroup(invoice.Branch.Company.OrgProxy, JobInvoicingConsumerTypes.Shipment.Code, "ALL", "ALL", "DEF", "DEF", "DEF", "DEF");
			allGroup.PG_RX_NKInvoicePostingCurrency = "USD";
			Factory.Save();

			AssertNoExceptionThrown(() => claim.Approve());
		}

		#region AdjustPostedInvoiceHelper

		public void TestAdjustPostedInvoiceHelperType()
		{
			var claim = Factory.New<ARAccQueryClaim>();
			AssertType<AdjustPostedInvoiceHelper>(claim.AdjustPostedInvoiceHelper_ExposedForTestOnly);
		}

		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAdjustPostedInvoiceHelper_AddRoundingLine()
		{
			var shipment = TestObjectCreator.CreateShipment("S00000001");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var otherBranch = TestObjectCreator.NonCurrentCompanyBranch;

			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "AP001", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m, TestObjectCreator.AALSHI, TestObjectCreator.CC3.PK, "FIN");
			apInvoice.AH_JH = job.PK;
			apInvoice.AH_GB = otherBranch.PK;
			var apLine = apInvoice.Lines[0];
			apLine.AL_JH = shipment.Job.PK;
			apLine.AL_GB = otherBranch.PK;
			TestObjectCreator.CreateCharge(apLine);

			var uaCreditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(UACreditNote), "UA001", TestObjectCreator.AUD, 1m, 20m, 0m, 20m, 0m, TestObjectCreator.AALSHI, TestObjectCreator.CC3.PK, "FIN");
			uaCreditNote.AH_JH = job.PK;
			uaCreditNote.AH_GB = otherBranch.PK;
			var uaLine = uaCreditNote.Lines[0];
			uaLine.AL_JH = shipment.Job.PK;
			uaLine.AL_GB = otherBranch.PK;

			var claim = Factory.New<ARAccQueryClaim>();
			claim.AY_OH_Debtor = TestObjectCreator.AALSHI.PK;
			claim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			claim.AY_AH = apInvoice.PK;
			claim.TransactionHeader.AH_TransactionBelongsToGroup = apInvoice.PK;
			uaCreditNote.AH_TransactionBelongsToGroup = apInvoice.PK;

			Factory.Save();

			AssertEquals("2 invoices should exist", 2, Factory.GetDatabaseCount(typeof(InvoicingBase)));

			var adjustPostedInvoiceHelper = new Mock<IAdjustPostedInvoiceHelper>(MockBehavior.Strict);
			adjustPostedInvoiceHelper.Setup(x => x.AdjustPostedInvoice(It.IsAny<InvoicingBase>()));
			claim.SubstituteAdjustPostedInvoiceHelper_ForTestOnly(adjustPostedInvoiceHelper.Object);

			claim.Approve();
			adjustPostedInvoiceHelper.Verify(x => x.AdjustPostedInvoice(It.Is<InvoicingBase>(y => y.AH_OSTotalAmount == 20m)), Times.Once);

			var query = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote);
			var newCreditNote = Factory.LoadTop1<ARCreditNote>(query);
			AssertNotNull("New AR Credit Note should have been created", newCreditNote);
			AssertEquals("Credit Note amount", 20m, newCreditNote.AH_OSExTaxAmount);
		}

		#endregion
	}

	[TestedType(typeof(ARAccQueryClaim))]
	public class ARAccQueryClaimDocumentTest : AccQueryClaimBaseDocumentTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<ARAccQueryClaim>();
		}
	}
}
