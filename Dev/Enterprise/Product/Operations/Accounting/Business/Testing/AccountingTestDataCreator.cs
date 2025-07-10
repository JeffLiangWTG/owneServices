#if DEBUG
using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing
{
	public class AccountingTestDataCreator : IAccountingTestDataCreator
	{
		public void CreatePeriods(int periodNumber, ZDateTime startDate, ZDateTime endDate, bool isClosed)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(factory);
			AccPeriodManagement period = periodHelper.SetupSinglePeriod(periodNumber, startDate, endDate.EndOfDay());
			period.AM_IsGeneralLedgerClosed = isClosed;
			period.AM_IsSubLedgerClosed = isClosed;
			factory.Save();
		}

		public void CreateAccountingData(ZGuid jobHeader1PK, ZGuid jobHeader2PK, ZDateTime postDate)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			RefCurrency currencyAUD = factory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"))[0];
			var glHeader = factory.NewWithValidTestData<AccGLHeader>();

			// 2 JobHeaders, with transactionlines pointing to jobheader only and a transactionheader has lines for different jobheaders

			// AR1
			AccTransactionHeader accTransactionHeaderAR1 = factory.New<AccTransactionHeader>();
			accTransactionHeaderAR1.AH_Ledger = "AR";
			accTransactionHeaderAR1.AH_TransactionType = "INV";
			accTransactionHeaderAR1.AH_InvoiceDate = ZDateTime.Now;
			accTransactionHeaderAR1.AH_GB = Env.CurrentBranch.PK;
			accTransactionHeaderAR1.AH_GE = Env.CurrentDepartment.PK;
			accTransactionHeaderAR1.AH_TransactionNum = TestObjectCreator.GetRandomString(10);
			accTransactionHeaderAR1.AH_InvoiceDate = ZDateTime.Now;
			accTransactionHeaderAR1.AH_RX_NKTransactionCurrency = currencyAUD.RX_Code;
			accTransactionHeaderAR1.AH_PostDate = postDate;

			AccTransactionLines accTransactionLinesAR1Line1 = factory.New<AccTransactionLines>();
			accTransactionLinesAR1Line1.AL_AH = accTransactionHeaderAR1.PK;
			accTransactionLinesAR1Line1.AL_JH = jobHeader1PK;
			accTransactionLinesAR1Line1.AL_GB = Env.CurrentBranch.PK;
			accTransactionLinesAR1Line1.AL_GE = Env.CurrentDepartment.PK;
			accTransactionLinesAR1Line1.AL_PostDate = postDate;
			accTransactionLinesAR1Line1.AL_AG = glHeader.PK;
			accTransactionLinesAR1Line1.AL_LineType = TransactionLineTypes.WIP;

			AccTransactionLines accTransactionLinesAR1Line2 = factory.New<AccTransactionLines>();
			accTransactionLinesAR1Line2.AL_AH = accTransactionHeaderAR1.PK;
			accTransactionLinesAR1Line2.AL_JH = jobHeader2PK;
			accTransactionLinesAR1Line2.AL_GB = Env.CurrentBranch.PK;
			accTransactionLinesAR1Line2.AL_GE = Env.CurrentDepartment.PK;
			accTransactionLinesAR1Line2.AL_PostDate = postDate;
			accTransactionLinesAR1Line2.AL_AG = glHeader.PK;
			accTransactionLinesAR1Line2.AL_LineType = TransactionLineTypes.WIP;

			// AR2
			AccTransactionHeader accTransactionHeaderAR2 = factory.New<AccTransactionHeader>();
			accTransactionHeaderAR2.AH_Ledger = "AR";
			accTransactionHeaderAR2.AH_TransactionType = "INV";
			accTransactionHeaderAR2.AH_GB = Env.CurrentBranch.PK;
			accTransactionHeaderAR2.AH_GE = Env.CurrentDepartment.PK;
			accTransactionHeaderAR2.AH_TransactionNum = TestObjectCreator.GetRandomString(10);
			accTransactionHeaderAR2.AH_RX_NKTransactionCurrency = currencyAUD.RX_Code;
			accTransactionHeaderAR2.AH_InvoiceDate = ZDateTime.Now;
			accTransactionHeaderAR2.AH_PostDate = postDate;

			AccTransactionLines accTransactionLinesAR2Line1 = factory.New<AccTransactionLines>();
			accTransactionLinesAR2Line1.AL_AH = accTransactionHeaderAR2.PK;
			accTransactionLinesAR2Line1.AL_JH = jobHeader1PK;
			accTransactionLinesAR2Line1.AL_GB = Env.CurrentBranch.PK;
			accTransactionLinesAR2Line1.AL_GE = Env.CurrentDepartment.PK;
			accTransactionLinesAR2Line1.AL_PostDate = postDate;
			accTransactionLinesAR2Line1.AL_AG = glHeader.PK;
			accTransactionLinesAR2Line1.AL_LineType = TransactionLineTypes.WIP;

			AccTransactionLines accTransactionLinesAR2Line2 = factory.New<AccTransactionLines>();
			accTransactionLinesAR2Line2.AL_AH = accTransactionHeaderAR2.PK;
			accTransactionLinesAR2Line2.AL_JH = jobHeader2PK;
			accTransactionLinesAR2Line2.AL_GB = Env.CurrentBranch.PK;
			accTransactionLinesAR2Line2.AL_GE = Env.CurrentDepartment.PK;
			accTransactionLinesAR2Line2.AL_PostDate = postDate;
			accTransactionLinesAR2Line2.AL_AG = glHeader.PK;
			accTransactionLinesAR2Line2.AL_LineType = TransactionLineTypes.WIP;

			factory.Save();
		}

		public void CreateConsolAccountingData(ZGuid jobHeaderPK, ZGuid jobConsolPK, ZDateTime postDate, bool isHotChequeCancelled, bool isHotChequeLinkedToAH)
		{
			CreateAccountingData(jobHeaderPK, true, jobConsolPK, postDate, isHotChequeCancelled, isHotChequeLinkedToAH);
		}

		public void CreateAccountingData(ZGuid jobHeaderPK, ZDateTime postDate, bool isHotChequeCancelled, bool isHotChequeLinkedToAH)
		{
			CreateAccountingData(jobHeaderPK, false, ZGuid.Empty, postDate, isHotChequeCancelled, isHotChequeLinkedToAH);
		}

		void CreateAccountingData(ZGuid jobHeaderPK, bool isConsol, ZGuid jobConsolPK, ZDateTime postDate, bool isHotChequeCancelled, bool isHotChequeLinkedToAH)
		{
			ZGuid testOrgHeaderPK = new ZGuid("0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1");
			ZGuid testOrgContactPK = new ZGuid("7BC0BE56-074E-423A-8537-C9505A64F993");

			BusinessObjectFactory factory = new BusinessObjectFactory();
			var glHeader = factory.NewWithValidTestData<AccGLHeader>();
			// 1 JobHeader, Multiple TransactionHeaders (all point to same job header)

			RefCurrency currencyAUD = factory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "AUD"))[0];

			AccChargeCode chargeCode = factory.LoadTop1<AccChargeCode>(new ZQuery());

			// AR1
			AccTransactionHeader accTransactionHeaderAR1 = factory.New<AccTransactionHeader>();
			accTransactionHeaderAR1.AH_GB = Env.CurrentBranch.PK;
			accTransactionHeaderAR1.AH_Ledger = "AR";
			accTransactionHeaderAR1.AH_TransactionType = "INV";
			accTransactionHeaderAR1.AH_JH = jobHeaderPK;
			accTransactionHeaderAR1.AH_InvoiceDate = ZDateTime.Now;
			accTransactionHeaderAR1.AH_GE = Env.CurrentDepartment.PK;
			accTransactionHeaderAR1.AH_TransactionNum = TestObjectCreator.GetRandomString(10);
			accTransactionHeaderAR1.AH_RX_NKTransactionCurrency = currencyAUD.RX_Code;
			accTransactionHeaderAR1.AH_PostDate = postDate;

			AccTransactionLines accTransactionLinesAR1Line1 = factory.New<AccTransactionLines>();
			accTransactionLinesAR1Line1.AL_AH = accTransactionHeaderAR1.PK;
			accTransactionLinesAR1Line1.AL_LineType = TransactionLineTypes.WIP;
			accTransactionLinesAR1Line1.AL_JH = jobHeaderPK;
			accTransactionLinesAR1Line1.AL_GB = Env.CurrentBranch.PK;
			accTransactionLinesAR1Line1.AL_GE = Env.CurrentDepartment.PK;
			accTransactionLinesAR1Line1.AL_PostDate = postDate;
			accTransactionLinesAR1Line1.AL_AG = glHeader.PK;

			AccTransactionLines accTransactionLinesAR1Line2 = factory.New<AccTransactionLines>();
			accTransactionLinesAR1Line2.AL_AH = accTransactionHeaderAR1.PK;
			accTransactionLinesAR1Line2.AL_LineType = TransactionLineTypes.WIP;
			accTransactionLinesAR1Line2.AL_JH = jobHeaderPK;
			accTransactionLinesAR1Line2.AL_GB = Env.CurrentBranch.PK;
			accTransactionLinesAR1Line2.AL_GE = Env.CurrentDepartment.PK;
			accTransactionLinesAR1Line2.AL_PostDate = postDate;
			accTransactionLinesAR1Line2.AL_AG = glHeader.PK;

			factory.Save();

			// AR2
			AccTransactionHeader accTransactionHeaderAR2 = factory.New<AccTransactionHeader>();
			accTransactionHeaderAR2.AH_Ledger = "AR";
			accTransactionHeaderAR2.AH_TransactionType = "INV";
			accTransactionHeaderAR2.AH_JH = jobHeaderPK;
			accTransactionHeaderAR2.AH_InvoiceDate = ZDateTime.Now;
			accTransactionHeaderAR2.AH_GB = Env.CurrentBranch.PK;
			accTransactionHeaderAR2.AH_GE = Env.CurrentDepartment.PK;
			accTransactionHeaderAR2.AH_TransactionNum = TestObjectCreator.GetRandomString(10);
			accTransactionHeaderAR2.AH_RX_NKTransactionCurrency = currencyAUD.RX_Code;
			accTransactionHeaderAR2.AH_PostDate = postDate;

			AccTransactionLines accTransactionLinesAR2Line1 = factory.New<AccTransactionLines>();
			accTransactionLinesAR2Line1.AL_AH = accTransactionHeaderAR2.PK;
			accTransactionLinesAR2Line1.AL_LineType = TransactionLineTypes.WIP;
			accTransactionLinesAR2Line1.AL_JH = jobHeaderPK;
			accTransactionLinesAR2Line1.AL_GB = Env.CurrentBranch.PK;
			accTransactionLinesAR2Line1.AL_GE = Env.CurrentDepartment.PK;
			accTransactionLinesAR2Line1.AL_PostDate = postDate;
			accTransactionLinesAR2Line1.AL_AG = glHeader.PK;

			AccTransactionLines accTransactionLinesAR2Line2 = factory.New<AccTransactionLines>();
			accTransactionLinesAR2Line2.AL_AH = accTransactionHeaderAR2.PK;
			accTransactionLinesAR2Line2.AL_LineType = TransactionLineTypes.WIP;
			accTransactionLinesAR2Line2.AL_JH = jobHeaderPK;
			accTransactionLinesAR2Line2.AL_GB = Env.CurrentBranch.PK;
			accTransactionLinesAR2Line2.AL_GE = Env.CurrentDepartment.PK;
			accTransactionLinesAR2Line2.AL_PostDate = postDate;
			accTransactionLinesAR2Line2.AL_AG = glHeader.PK;

			factory.Save();

			// AP1
			AccTransactionHeader accTransactionHeaderAP1 = factory.New<AccTransactionHeader>();
			accTransactionHeaderAP1.AH_Ledger = "AP";
			accTransactionHeaderAP1.AH_TransactionType = "INV";
			accTransactionHeaderAP1.AH_JH = jobHeaderPK;
			accTransactionHeaderAP1.AH_InvoiceDate = ZDateTime.Now;
			accTransactionHeaderAP1.AH_GB = Env.CurrentBranch.PK;
			accTransactionHeaderAP1.AH_GE = Env.CurrentDepartment.PK;
			accTransactionHeaderAP1.AH_TransactionNum = TestObjectCreator.GetRandomString(10);
			accTransactionHeaderAP1.AH_RX_NKTransactionCurrency = currencyAUD.RX_Code;
			accTransactionHeaderAP1.AH_PostDate = postDate;

			AccTransactionLines accTransactionLinesAP1Line1 = factory.New<AccTransactionLines>();
			accTransactionLinesAP1Line1.AL_AH = accTransactionHeaderAP1.PK;
			accTransactionLinesAP1Line1.AL_LineType = TransactionLineTypes.WIP;
			accTransactionLinesAP1Line1.AL_JH = jobHeaderPK;
			accTransactionLinesAP1Line1.AL_GB = Env.CurrentBranch.PK;
			accTransactionLinesAP1Line1.AL_GE = Env.CurrentDepartment.PK;
			accTransactionLinesAP1Line1.AL_PostDate = postDate;
			accTransactionLinesAP1Line1.AL_AG = glHeader.PK;

			factory.Save();

			// Others
			JobCharge jobCharge1 = factory.New<JobCharge>();
			jobCharge1.JR_AC = chargeCode.PK;
			jobCharge1.JR_JH = jobHeaderPK;
			jobCharge1.JR_GB = Env.CurrentBranch.PK;
			jobCharge1.JR_GE = Env.CurrentDepartment.PK;

			JobChargeAttrib jobChargeAttrib1 = factory.New<JobChargeAttrib>();
			jobChargeAttrib1.EC_JR = jobCharge1.PK;
			jobChargeAttrib1.EC_Name = JobChargeAttribTypeList.Codes.UnroundedItemsToRate;

			jobCharge1.JR_AL_CFXLine = accTransactionLinesAR1Line1.PK;
			jobCharge1.JR_AL_ARLine = accTransactionLinesAR2Line2.PK;
			jobCharge1.JR_AL_APLine = accTransactionLinesAP1Line1.PK;
			accTransactionLinesAR2Line2.AL_LineType = "REV";
			accTransactionLinesAP1Line1.AL_LineType = "CST";
			accTransactionLinesAR2Line2.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			accTransactionLinesAP1Line1.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			AccHotCheque accHotCheque = factory.New<AccHotCheque>();
			accHotCheque.AQ_JH = jobHeaderPK;

			if (isHotChequeCancelled)
			{
				accHotCheque.AQ_Cancelled = true;
			}

			if (isHotChequeLinkedToAH)
			{
				accHotCheque.AQ_AH = accTransactionHeaderAP1.PK;
			}

			if (isConsol)
			{
				JobConsolCost jobConsolCost = factory.New<JobConsolCost>();
				jobConsolCost.E6_AH_ARInvoice = accTransactionHeaderAR1.PK;
				jobConsolCost.E6_AC_ChargeCode = chargeCode.PK;
				jobConsolCost.E6_GC = Env.CurrentCompany.PK;
				using (jobConsolCost.ReportSettingParentSuspender.GetSuspender())
				{
					jobConsolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(jobConsolPK, "JK");
				}

				jobConsolCost.E6_AH_APInvoice = accTransactionHeaderAP1.PK;
				jobCharge1.JR_E6 = jobConsolCost.PK;

				var paymentBasis = factory.New<JobPaymentBasis>();
				paymentBasis.PBS_IsCost = true;
				paymentBasis.PBS_JR = jobCharge1.PK;
				paymentBasis.PBS_PerUnitRate = 5m;
				paymentBasis.PBS_RX_NKRateCurrency = "AUD";
				paymentBasis.PBS_RateReference = "UNT";
				paymentBasis.PBS_RateUnit = "KG";
				paymentBasis.PBS_ChargeableAmount = 200m;
				paymentBasis.PBS_ChargeableUnit = "KG";
			}

			factory.Save();

			TransactionMatchLinkGroup group = new TransactionMatchLinkGroup(factory);
			AccTransactionMatchLink accTransactionMatchLink1 = group.AddNew();
			accTransactionMatchLink1.AP_AH = accTransactionHeaderAR2.PK;
			accTransactionMatchLink1.AP_MatchGroupNum = "001";
			TestObjectCreator.SetupMatchLinkMatchDate(accTransactionMatchLink1);

			accTransactionHeaderAR2.AH_AH_InvoiceStatement = accTransactionHeaderAR1.PK;

			APAccQueryClaim accQueryClaim = factory.New<APAccQueryClaim>();
			accQueryClaim.AY_AH = accTransactionHeaderAP1.PK;
			accQueryClaim.AY_OH_Debtor = testOrgHeaderPK;
			accQueryClaim.AY_OC = testOrgContactPK;

			AccPaymentApproval accPaymentApproval = factory.New<AccPaymentApproval>();
			accPaymentApproval.AV_AH = accTransactionHeaderAP1.PK;
			accPaymentApproval.AV_OH = testOrgHeaderPK;
			accPaymentApproval.AV_GB = Env.CurrentBranch.PK;
			accPaymentApproval.AV_GC = Env.CurrentCompany.PK;

			AccPaymentApprovalItem accPaymentApprovalItem = factory.New<AccPaymentApprovalItem>();
			accPaymentApprovalItem.A2_AH = accTransactionHeaderAP1.PK;
			accPaymentApprovalItem.A2_AV = accPaymentApproval.PK;

			ExchangeRate jobExRate1 = factory.New<ExchangeRate>();
			jobExRate1.JF_JH = jobHeaderPK;
			jobExRate1.JF_RX_NKRateCurrency = currencyAUD.RX_Code;

			factory.Save();
		}

		public void CreateJobHeader(BusinessObject jobHeaderParent, ZGuid organisationPKLocalCharges)
		{
			var parent = jobHeaderParent as IJobHeaderParent ?? throw new ArgumentException("The jobHeaderParent you pass in must implement IJobHeaderParent.");

			lastJobHeaderCreated = new Job.Loader(parent).TryLoadOrCreateWithoutMutexForTestOnly();
			if (lastJobHeaderCreated == null)
			{
				throw new InvalidOperationException("Job Header was returned as null - Job Header not created.");
			}
		}

		Job lastJobHeaderCreated;

		public void AddChargeLineToCreatedJobHeader(ZString chargeCode, ZString description, ZDecimal amount, ZString currencyCode)
		{
			if (lastJobHeaderCreated == null)
			{
				throw new InvalidOperationException("You must call CreateJobHeader at least once before calling this method as lines are added to the last JobHeader created.");
			}

			var factory = lastJobHeaderCreated.Factory;

			var chargeCodeBO = factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, chargeCode))
							   ?? throw new ArgumentException(string.Format("Charge Code [{0}] does not exist in the DB.", chargeCode));

			var chargeLine = lastJobHeaderCreated.Charges.AddNew();
			chargeLine.JR_AC = chargeCodeBO.PK;
			chargeLine.JR_Desc = description;
			chargeLine.JR_OSSellAmt = amount;
			chargeLine.JR_RX_NKSellCurrency = currencyCode;
		}

		public void CreateApprovalRequest(BusinessObject parent, ZGuid menuItemPK, int[] authorizationLevel, ZString description)
		{
			var factory = new BusinessObjectFactory();
			var approvalRequest = factory.New<CreditControlledDocumentsApproval>();
			approvalRequest.Initialize(parent, menuItemPK, authorizationLevel, description);

			factory.Save();
		}
	}
}
#endif
