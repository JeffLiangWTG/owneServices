using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class AccountingBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		protected TestObjectCreator TestObjectCreator;

		bool OldIsGSTRegistered;
		bool OldIsWHTRegistered;

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
			OldIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			OldIsWHTRegistered = GlbCompany.CurrentCompany.GC_IsWHTRegistered;
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = OldIsWHTRegistered;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = OldIsGSTRegistered;
		}

		protected ZDateTime Today = ZDateTime.Today;
		protected ZDateTime Tomorrow = ZDateTime.Today.AddDays(1);

		#region Currencies

		protected RefCurrency AUD
		{
			get { return this.TestObjectCreator.LocalCurrency; }
		}

		protected RefCurrency USD
		{
			get { return this.TestObjectCreator.USD; }
		}

		protected RefCurrency GBP
		{
			get { return this.TestObjectCreator.GBP; }
		}

		#endregion

		#region Bank Accounts

		#region AUD Account

		AccBankAccount fAUDBankAccount;
		protected AccBankAccount AUDBankAccount
		{
			get
			{
				if (fAUDBankAccount == null)
				{
					fAUDBankAccount = TestObjectCreator.CreateBankAccount("AUD", "AUD Account", AUD, null);
				}

				return fAUDBankAccount;
			}
		}

		#endregion

		#region USD Account

		AccBankAccount fUSDBankAccount;
		protected AccBankAccount USDBankAccount
		{
			get
			{
				if (fUSDBankAccount == null)
				{
					fUSDBankAccount = TestObjectCreator.CreateBankAccount("USD", "USD Account", USD, null);
				}

				return fUSDBankAccount;
			}
		}

		#endregion

		#region GBP Account

		AccBankAccount fGBPBankAccount;
		protected AccBankAccount GBPBankAccount
		{
			get
			{
				if (fGBPBankAccount == null)
				{
					fGBPBankAccount = TestObjectCreator.CreateBankAccount("GBP", "GBP Account", GBP, null);
				}

				return fGBPBankAccount;
			}
		}

		#endregion

		#endregion

		#region Cheque Books

		#region AUD

		AccChequeBook fAUDChequeBook;
		protected AccChequeBook AUDChequeBook
		{
			get
			{
				if (fAUDChequeBook == null)
				{
					fAUDChequeBook = TestObjectCreator.CreateChequeBook(1, 1, 10000, AUDBankAccount);
				}

				return fAUDChequeBook;
			}
		}

		#endregion

		#region USD

		AccChequeBook fUSDChequeBook;
		protected AccChequeBook USDChequeBook
		{
			get
			{
				if (fUSDChequeBook == null)
				{
					fUSDChequeBook = TestObjectCreator.CreateChequeBook(1, 1, 10000, USDBankAccount);
				}

				return fUSDChequeBook;
			}
		}

		#endregion

		#region GBP

		AccChequeBook fGBPChequeBook;
		protected AccChequeBook GBPChequeBook
		{
			get
			{
				if (fGBPChequeBook == null)
				{
					fGBPChequeBook = TestObjectCreator.CreateChequeBook(1, 1, 10000, GBPBankAccount);
				}

				return fGBPChequeBook;
			}
		}

		#endregion

		#endregion

		#region Charge Codes

		AccChargeCode fMargin100Code;
		protected AccChargeCode Margin100Code
		{
			get
			{
				if (fMargin100Code == null)
				{
					fMargin100Code = TestObjectCreator.CreateChargeCode("MRG100", "Margin100%", Constants.ChargeType.Margin, 100, GST, WHT, "ALL");
				}

				return fMargin100Code;
			}
		}

		AccChargeCode fMargin60Code;
		protected AccChargeCode Margin60Code
		{
			get
			{
				if (fMargin60Code == null)
				{
					fMargin60Code = TestObjectCreator.CreateChargeCode("MRG60", "Margin60%", Constants.ChargeType.Margin, 60, GST, WHT, "ALL");
				}

				return fMargin60Code;
			}
		}

		AccChargeCode fRevenueChargeCode;
		protected AccChargeCode RevenueChargeCode
		{
			get
			{
				if (fRevenueChargeCode == null)
				{
					fRevenueChargeCode = TestObjectCreator.CreateChargeCode("REV", "Revenue", Constants.ChargeType.Revenue, 100, GST, WHT, "ALL");
				}

				return fRevenueChargeCode;
			}
		}

		AccChargeCode fDisbursementChargeCode;
		protected AccChargeCode DisbursementChargeCode
		{
			get
			{
				if (fDisbursementChargeCode == null)
				{
					fDisbursementChargeCode = TestObjectCreator.CreateChargeCode("DSB", "Disbursement", Constants.ChargeType.Disbursement, 100, GST, WHT, "ALL");
				}

				return fDisbursementChargeCode;
			}
		}

		AccChargeCode fNonAccrualChargeCode;
		protected AccChargeCode NonAccrualChargeCode
		{
			get
			{
				if (fNonAccrualChargeCode == null)
				{
					fNonAccrualChargeCode = TestObjectCreator.CreateChargeCode("NON", "Non Accrual", Constants.ChargeType.Margin, 0, GST, WHT, "ALL");
				}

				return fNonAccrualChargeCode;
			}
		}

		AccChargeCode fCC01;
		protected AccChargeCode CC01
		{
			get
			{
				if (fCC01 == null)
				{
					fCC01 = TestObjectCreator.CreateChargeCode("MRG100_1", "Margin100%", Constants.ChargeType.Margin, 100, GST, WHT, "ALL");
				}

				return fCC01;
			}
		}

		AccChargeCode fCC02;
		protected AccChargeCode CC02
		{
			get
			{
				if (fCC02 == null)
				{
					fCC02 = TestObjectCreator.CreateChargeCode("MRG100_2", "Margin100%", Constants.ChargeType.Margin, 100, FREEGST, WHT, "ALL");
				}

				return fCC02;
			}
		}

		AccChargeCode fCC03;
		protected AccChargeCode CC03
		{
			get
			{
				if (fCC03 == null)
				{
					fCC03 = TestObjectCreator.CreateChargeCode("MRG100_3", "Margin100%", Constants.ChargeType.Margin, 100, GST, WHT, "ALL");
				}

				return fCC03;
			}
		}

		#endregion

		#region Organisations

		OrgHeader fTestOrganisation;
		protected OrgHeader TestOrganisation
		{
			get
			{
				if (fTestOrganisation == null)
				{
					fTestOrganisation = TestObjectCreator.CreateOrgHeader("Org", true, true, true, true, true, true);
				}

				return fTestOrganisation;
			}
		}

		protected OrgHeader fCreditor1;
		protected OrgHeader Creditor1
		{
			get
			{
				if (fCreditor1 == null)
				{
					fCreditor1 = TestObjectCreator.CreateOrgHeader("CREDITOR1", true, false, true, false, false, false);
				}

				return fCreditor1;
			}
		}

		protected OrgHeader fCreditor2;
		protected OrgHeader Creditor2
		{
			get
			{
				if (fCreditor2 == null)
				{
					fCreditor2 = TestObjectCreator.CreateOrgHeader("CREDITOR2", true, false, false, false, false, false);
				}

				return fCreditor2;
			}
		}

		protected OrgHeader fCreditor3;
		protected OrgHeader Creditor3
		{
			get
			{
				if (fCreditor3 == null)
				{
					fCreditor3 = TestObjectCreator.CreateOrgHeader("CREDITOR3", true, false, true, false, false, false);
				}

				return fCreditor3;
			}
		}

		protected OrgHeader fDebtor;
		protected OrgHeader Debtor
		{
			get
			{
				if (fDebtor == null)
				{
					fDebtor = TestObjectCreator.CreateOrgHeader("Debtor", false, true, true, false, false, false);
				}

				return fDebtor;
			}
		}

		#endregion

		#region Tax rates

		AccTaxRate fGST;
		protected AccTaxRate GST
		{
			get
			{
				if (fGST == null)
				{
					fGST = TestObjectCreator.CreateTaxRate("GST", "GSTRate", 10);
				}

				return fGST;
			}
		}

		AccTaxRate fFREEGST;
		protected AccTaxRate FREEGST
		{
			get
			{
				if (fFREEGST == null)
				{
					fFREEGST = TestObjectCreator.CreateTaxRate("Free", "FREEGST", 0);
				}

				return fFREEGST;
			}
		}

		AccWithholding fWHT;
		protected AccWithholding WHT
		{
			get
			{
				if (fWHT == null)
				{
					fWHT = TestObjectCreator.CreateOrLoadWithholdingTax("WHT", "WHTRate", 5);
				}

				return fWHT;
			}
		}

		#endregion

		#region Departments

		protected GlbDepartment CurrentDepartment
		{
			get { return GlbDepartment.CurrentDepartment; }
		}

		protected GlbDepartment NonCurrentDepartment
		{
			get { return TestObjectCreator.NonCurrentDepartment; }
		}

		#endregion

		#region Job

		Job fTestJob;
		protected virtual Job TestJob
		{
			get
			{
				if (fTestJob == null)
				{
					fTestJob = TestObjectCreator.CreateJob(Creditor1, 0, null, 0);
				}

				return fTestJob;
			}
		}

		#endregion
	}
}