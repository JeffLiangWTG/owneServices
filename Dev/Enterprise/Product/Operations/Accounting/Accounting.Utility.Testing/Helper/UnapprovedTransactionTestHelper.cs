using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Utility.Testing
{
	public class UnapprovedTransactionTestHelper
	{
		public UnapprovedTransactionTestHelper(BusinessObjectFactory factory, TestObjectCreator creator)
		{
			this.Factory = factory;
			this.TestObjectCreator = creator;
		}

		public void SetupSource()
		{
			SetupSourceDebtor();
			Assertion.AssertEquals("Precondition: Source Debtor is org proxy for for target company", SourceDebtor.PK, TargetCompany.OrgProxy.PK);
			Assertion.Assert("Precondition: Source Creditor is AR in source company", SourceDebtor.CompanyData.OB_IsDebtor);

			SetupSourceProxy();
			SourceProxy.CompanyDataCollection.Load(new ZQuery(OrgCompanyDataSchema.OB_GC, TargetCompany.PK));
			OrgCompanyData sourceProxyTargetCompanyData = SourceProxy.CompanyDataCollection[0];
			Assertion.Assert("Precondition: SourceCompany Org proxy must be AP in target company", sourceProxyTargetCompanyData.OB_IsCreditor);
		}

		public void SetupTarget()
		{
			SetupTargetDebtor();
			Assertion.Assert("Precondition: Source Creditor is AR in source company", TargetDebtor.CompanyData.OB_IsCreditor);
		}

		public void SetUpTargetRegistryForTest()
		{
			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			GetNewAuthorisationSetting(valuesForTest, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo, 1000, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired);
			GetNewAuthorisationSetting(valuesForTest, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, 1000, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly);

			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(TargetCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		#region Target

		public GlbBranch TargetBranch
		{
			get { return targetBranch ?? (targetBranch = TestObjectCreator.CreateBranch("TB1", "Target", TargetCompany)); }
		}
		GlbBranch targetBranch;

		public GlbCompany TargetCompany
		{
			get
			{
				if (companyTarget == null)
				{
					companyTarget = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "EDI"));
					targetBranch = TestObjectCreator.CreateBranch("TB1", "Target", companyTarget);
					TestObjectCreator.Factory.Save();
				}
				return companyTarget;
			}
		}
		GlbCompany companyTarget;

		void SetupTargetDebtor()
		{
			OrgCompanyData targetDebtorSourceCompanyData = GetOrCreateOrgCompanyData(TargetDebtor, TargetCompany);
			targetDebtorSourceCompanyData.OB_IsCreditor = true;
			targetDebtorSourceCompanyData.SetAPTaxApplicableIgnoringRegistrySetting(false);
		}

		public OrgHeader TargetDebtor
		{
			get { return targetDebtor ?? (targetDebtor = TestObjectCreator.CreateOrgHeader("TD", true, true)); }
		}
		OrgHeader targetDebtor;

		#endregion

		#region Source

		public void SetupSourceDebtor()
		{
			var sourceDebtorSourceCompanyData = GetOrCreateOrgCompanyData(SourceDebtor, SourceCompany);
			sourceDebtorSourceCompanyData.OB_IsDebtor = true;
			TargetCompany.GC_OH_OrgProxy = SourceDebtor.PK;

			var sourceDebtorSourceCompanyData2 = GetOrCreateOrgCompanyData(SourceDebtor2, SourceCompany);
			sourceDebtorSourceCompanyData2.OB_IsDebtor = true;
		}

		public void SetupSourceProxy()
		{
			SourceCompany.GC_OH_OrgProxy = SourceProxy.PK;
			OrgCompanyData sourceProxyTargetCompanyData = GetOrCreateOrgCompanyData(SourceProxy, TargetCompany);
			sourceProxyTargetCompanyData.OB_IsCreditor = true;
			sourceProxyTargetCompanyData.SetAPTaxApplicableIgnoringRegistrySetting(false);
			var contact = SourceProxy.Contacts.AddNew();
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Payables.Code;
			document.OD_DefaultContact = true;
		}

		public GlbCompany SourceCompany
		{
			get
			{
				if (companySource1 == null)
				{
					companySource1 = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "DEM"));
					sourceCompanyBranch = TestObjectCreator.CreateBranch("SB1", "SourceBranch", companySource1);
					TestObjectCreator.Factory.Save();
				}
				return companySource1;
			}
		}
		GlbCompany companySource1;

		public GlbBranch SourceCompanyBranch
		{
			get { return sourceCompanyBranch ?? (sourceCompanyBranch = TestObjectCreator.CreateBranch("SB1", "SourceBranch", SourceCompany)); }
		}
		GlbBranch sourceCompanyBranch;

		public OrgHeader SourceDebtor
		{
			get { return sourceDebtor ?? (sourceDebtor = TestObjectCreator.CreateOrgHeader("SD", true, true)); }
		}
		OrgHeader sourceDebtor;

		public OrgHeader SourceDebtor2
		{
			get { return sourceDebtor2 ?? (sourceDebtor2 = TestObjectCreator.CreateOrgHeader("SD2", true, true)); }
		}
		OrgHeader sourceDebtor2;

		public OrgHeader SourceProxy
		{
			get { return sourceProxy ?? (sourceProxy = TestObjectCreator.CreateOrgHeader("SP", true, true)); }
		}
		OrgHeader sourceProxy;

		#endregion

		#region Implementation

		public Job CreateJobForTargetDebtor(PostType postType, ZString jobNum, GlbDepartment department = null, bool bypassPreSaveValidation = false)
		{
			return CreateJobForDebtor(postType, jobNum, TargetDebtor, TestObjectCreator.CC1, null, null, department, bypassPreSaveValidation);
		}

		public Job CreateJobAndPost(PostType postType, ZString jobNum)
		{
			return CreateJobAndPost(postType, jobNum, null, null);
		}

		public Job CreateJobAndPost(PostType postType, ZString jobNum, ZDecimal jr_LocalSellAmt)
		{
			return CreateJobAndPost(postType, jobNum, jr_LocalSellAmt, null);
		}

		Job CreateJobForDebtor(PostType postType, ZString jobNum, OrgHeader debtor, AccChargeCode chargeCode, ZDecimal? jr_LocalSellAmt = null, Action<Job> jobAdditionalSetup = null,
			GlbDepartment department = null, bool bypassPreSaveValidation = false)
		{
			var job = new Job.Loader(Shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_JobNum = jobNum;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = department == null ? TestObjectCreator.NonCurrentDepartment.PK : department.PK;
			job.LocalChargesPK = debtor.PK;
			job.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;

			if (jobAdditionalSetup != null)
			{
				jobAdditionalSetup(job);
			}
			Factory.Save();

			var charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OH_SellAccount = debtor.PK;

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_Desc = "Test Charge";
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			charge.JR_LocalSellAmt = jr_LocalSellAmt == null ? (postType == PostType.CreditNote ? -1 : 1) : jr_LocalSellAmt.Value;
			charge.JR_OSCostAmt = 0;
			if (!bypassPreSaveValidation)
			{
				charge.RunPreSaveValidation();
				TestCaseWithFactory.AssertNoRowErrors(charge);
				TestCaseWithFactory.AssertNoErrors(charge);

				job.RunPreSaveValidation();
				TestCaseWithFactory.AssertNoRowErrors(job);
				TestCaseWithFactory.AssertNoErrors(job);
			}
			Factory.Save();

			return job;
		}

		Job CreateJobAndPost(PostType postType, ZString jobNum, ZDecimal? jr_LocalSellAmt, Action<Job> jobAdditionalSetup)
		{
			var job = CreateJobForDebtor(postType, jobNum, SourceDebtor, TestObjectCreator.CC1, jr_LocalSellAmt);
			var charge = job.Charges[0];
			charge.JR_GE = TestObjectCreator.FIADepartment.PK;

			var postManager = new InvoicingPostManager(job);
			postManager.Poster.Post(charge);
			InvoicingBase[] invoices = postManager.Poster.GetInvoices(SourceDebtor);
			Assertion.AssertEquals("There can be only one...invoice", 1, invoices.Length);
			Assertion.AssertEquals("There can be only one... line", 1, invoices[0].Lines.Count);
			Factory.Save();

			invoices[0].RunPreSaveValidation();
			TestCaseWithFactory.AssertNoRowErrors(invoices[0]);
			TestCaseWithFactory.AssertNoErrors(invoices[0]);
			Assertion.AssertEquals("Posting created and invoice", 1, postManager.Poster.PostedInvoices.Count);
			return job;
		}

		public void CreateConsolAndPost()
		{
			var chargeCode = TestObjectCreator.RevenueNoTaxChargeCode;
			var job = CreateJobForDebtor(PostType.Invoice, "1", SourceDebtor, chargeCode, jobAdditionalSetup: jobToSetup =>
			{
				jobToSetup.JH_OA_AgentCollectAddr = SourceDebtor.Addresses.AddNewMainAddress().PK;
				jobToSetup.JH_OA_LocalChargesAddr = TargetDebtor.Addresses.AddNewMainAddress().PK;
			});

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Shipments.Add(Shipment);

			var jobs = new[] { job };

			ConsolInvoicingPostManager postManager = new ConsolInvoicingPostManager(Factory, jobs, consol, new Business.ConsolCosting.ApportionmentListing(Factory, consol));

			TransactionCreatorHashtable transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Agent);
			Assertion.AssertEquals("Receivable Transactions Count", 1, transactions.ARTransactionsCount);

			TransactionHeader[] invoices = transactions.GetAllARTransactions();
			Assertion.AssertEquals(1, invoices.Length);
			invoices[0].AH_JH = ZGuid.Empty;
			Assertion.AssertEquals("Invoice Line", 1, ((ARInvoice)invoices[0]).Lines.Count);
			((ARInvoice)invoices[0]).Lines[0].GenericCharge = chargeCode.PK;
			((ARInvoice)invoices[0]).Lines[0].AL_AT = TestObjectCreator.GST1.PK;

			invoices[0].RunPreSaveValidation();
			TestCaseWithFactory.AssertNoRowErrors(invoices[0]);
			TestCaseWithFactory.AssertNoErrors(invoices[0]);
			invoices[0].Factory.Save();
			Assertion.AssertEquals("Posting created and invoice", 1, postManager.Poster.PostedInvoices.Count);
		}

		public OrgCompanyData GetOrCreateOrgCompanyData(OrgHeader org, GlbCompany company, BusinessObjectFactory factory = null)
		{
			if (factory == null)
			{
				factory = Factory;
			}

			OrgCompanyData orgCompanyData;
			org.CompanyDataCollection.Load(new ZQuery(OrgCompanyDataSchema.OB_GC, company.PK));
			if (org.CompanyDataCollection.Count == 0)
			{
				orgCompanyData = factory.New<OrgCompanyData>();
				orgCompanyData.OB_GC = company.PK;
				orgCompanyData.OB_OH = org.PK;
			}
			else
			{
				orgCompanyData = org.CompanyDataCollection[0];
			}
			return orgCompanyData;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void ChangeCurrentCompanyViaBranch(GlbCompany expectedCompany, GlbBranch branch)
		{
			Env.SetUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			{
				Env.Security.APUnapprovedInvoicesThirdApproval.IsAllowed = false;
			}
			GlbCompany.CurrentCompany.Refresh();
			Assertion.AssertEquals("Branch is in company", expectedCompany.PK, branch.GB_GC);
			Assertion.AssertEquals("Current Company is now expected company", GlbCompany.CurrentCompany.PK, expectedCompany.PK);
			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Now.Year, 1, 1));
		}

		public void ForceNewShipment()
		{
			fShipment = null;
		}

		public ForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<ForwardingShipment>();
				}
				return fShipment;
			}
		}

		ForwardingShipment fShipment;

		public T CreateInvoice<T>(GlbBranch branch, OrgHeader org, ZBool isPostedInternal, ZString invoiceNum, ZGuid transactionGroup, ZString transactionReference, ZDecimal amount, BusinessObjectFactory factory = null, bool isManuallySetTransactionNumber = false) where T : TransactionHeader
		{
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				if (factory == null)
				{
					factory = Factory;
				}

				T result = factory.New<T>();
				result.AH_Ledger = typeof(T).Name.Substring(0, 2);
				result.AH_PostedInternal = isPostedInternal;
				result.AH_OH = org.PK;
				result.AH_TransactionNum = invoiceNum;
				result.IsManuallySetTransactionNumber_ForTestOnly = isManuallySetTransactionNumber;
				result.AH_TransactionBelongsToGroup = transactionGroup;
				result.AH_TransactionReference = transactionReference;

				if (!amount.IsEmpty)
				{
					TransactionHeaderWithLines resultAsTransactionHeaderWithLines = result as TransactionHeaderWithLines;

					if (resultAsTransactionHeaderWithLines != null)
					{
						DependentTransactionLine line = resultAsTransactionHeaderWithLines.Lines.AddNew();
						line.FillWithValidTestData();
						line.AL_OSExTaxAmount = amount;
						line.AL_AG = TestObjectCreator.GLHeader1.PK;
					}
					else
					{
						result.AH_OSTotalAmount = result.AH_OSExTaxAmount = result.AH_InvoiceAmount = result.AH_OutstandingAmount = amount;
					}
				}

				return result;
			}
		}

		PaymentTwelveLevelAuthorisationSettings GetNewAuthorisationSetting(PaymentTwelveLevelAuthorisationSettingsCollection collection,
			ZString range, ZInt amount, ZString requirement)
		{
			var newSetting = collection.AddNew();
			newSetting.Amount = (ZDecimal)amount;
			newSetting.AuthorisationRequirement = requirement;
			newSetting.Range = range;

			return newSetting;
		}

		#endregion

		public void EnsureCC1ChargeCodeInDBForCurrentCompany()
		{
			if (TestObjectCreator.CC1.AC_GC == GlbCompany.CurrentCompany.PK)
			{
				Factory.Save();
			}
			else
			{
				Assertion.Assert(TestObjectCreator.CC1.AC_Code + "Must be create for current company so that it can be used for TransactionLine.GenericChrage", false);
			}
		}

		readonly BusinessObjectFactory Factory;
		readonly TestObjectCreator TestObjectCreator;
	}
}
