using System;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;
using AccountingBusiness = Enterprise.Accounting.Business;
using FreightIntegration = Enterprise.Freight.Integration;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccountingBusiness.CreditControlledDocumentsApproval))]
	public class CreditControlledDocumentsApprovalTest : GenApprovalRequestTest<AccountingBusiness.CreditControlledDocumentsApproval>
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var menutItem = Factory.New<StmMenuItem>();
			menutItem.SU_MenuPath = "menu/path/newbizo";
			menutItem.SU_MenuName = "name";

			var businessObject = Factory.New<Forwarding.IForwardingShipment>();
			businessObject.JS_UniqueConsignRef = "S1";
			var approval = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			approval.Initialize((BusinessObject)businessObject, menutItem.PK);
			return approval;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var menutItem = factory.New<StmMenuItem>();
			menutItem.SU_MenuPath = "menu/path/newbizo";
			menutItem.SU_MenuName = "name";

			var businessObject = factory.New<Forwarding.IForwardingShipment>();
			businessObject.JS_UniqueConsignRef = "S1";
			var approval = factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			approval.Initialize((BusinessObject)businessObject, menutItem.PK);
			return approval;
		}

		public void TestEditingApprovalInTwoFactoriesDoNotThrowConcurrencyException()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			var approval = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			approval.Initialize(shipment);
			approval.DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TesteDocFile2", "MSC", description: "Original description");
			AssertEquals("One eDoc attached to approval2", 1, approval.DocManagerInfo.AllEDocs.Count);
			approval.SetupEDocsFactoryToBeSavedWithApprovalFactory();

			Factory.Save();

			var reloadFactory = new BusinessObjectFactory();
			var reloadedApproval = reloadFactory.Load<AccountingBusiness.CreditControlledDocumentsApproval>(approval.PK);
			reloadedApproval.SetupEDocsFactoryToBeSavedWithApprovalFactory();
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, reloadedApproval.XP_ApprovalStatus);
			reloadedApproval.XP_ApprovalStatus = Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Approved;

			AssertEquals(1, reloadedApproval.DocManagerInfo.AllEDocs.Count);
			var eDoc = reloadedApproval.DocManagerInfo.AllEDocs[0];
			AssertEquals("Original description", eDoc.Description);
			eDoc.Description = "This is a new description";

			AssertNoExceptionThrown(() => reloadFactory.Save());

			reloadFactory = new BusinessObjectFactory();
			reloadedApproval = reloadFactory.Load<AccountingBusiness.CreditControlledDocumentsApproval>(approval.PK);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Approved, reloadedApproval.XP_ApprovalStatus);
			AssertEquals(1, reloadedApproval.DocManagerInfo.AllEDocs.Count);
			eDoc = reloadedApproval.DocManagerInfo.AllEDocs[0];
			AssertEquals("This is a new description", eDoc.Description);
		}

		public void TestDepartment()
		{
			var loginDepartmentPK = Env.CurrentDepartment.PK;
			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GES"));
			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "DDD";
			var list = JobInvoicingConsumerTypes.New();
			BusinessObject bizO;

			foreach (JobInvoicingConsumerType type in list)
			{
				bizO = CreateBizObj(type);
				if (bizO is IJobInvoicingPlugIn && bizO is ICreditControlledDocumentDelivery)
				{
					using (type == JobInvoicingConsumerTypes.GatewayConsol ? Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, gatewayDepartment.PK.ToGuid()) : null)
					{
						if (type == JobInvoicingConsumerTypes.GatewayConsol)
						{
							SetupGatewayConsol(bizO);
						}

						var jobheader = new Job.Loader((IJobHeaderParent)bizO).TryCreateWithoutMutexForTestOnly();
						if (jobheader != null)
						{
							jobheader.JH_GE = department.PK;
							Factory.Save();

							var approval = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
							approval.Initialize(bizO);
							AssertEquals(department.GE_Code, approval.Department);
						}
						else if (type != JobInvoicingConsumerTypes.ForwardingConsol)
						{
							Fail($"Expected job header to be created for job invoicing consumer type : {type.Description}");
						}
					}
				}
			}
		}

		public void TestIncoTerm()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			shipment[JobShipmentSchema.JS_INCO.Name] = Enterprise.Core.Constants.IncoTerms.FreeOnBoard;
			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var approvalForShipment = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			approvalForShipment.Initialize(shipment);
			AssertEquals(Core.Constants.IncoTerms.FreeOnBoard, approvalForShipment.IncoTermCode);
			AssertEquals("Free On Board", approvalForShipment.IncoTermDescription);

			var consol = (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			consol[JobConsolSchema.JK_PrepaidCollect.Name] = Enterprise.Core.Constants.PaymentType.Prepaid;
			Factory.Save();

			var approvalForConsol = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			approvalForConsol.Initialize(consol);
			AssertEquals(Core.Constants.PaymentType.Prepaid, approvalForConsol.IncoTermCode);
			AssertEquals("Prepaid", approvalForConsol.IncoTermDescription);

			var domesticShipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			domesticShipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
			domesticShipment[JobShipmentSchema.JS_RL_NKDestination] = "AUMEL";
			domesticShipment[JobShipmentSchema.JS_INCO.Name] = Enterprise.Core.Constants.DomesticPaymentTerms.CollectThirdParty;
			job = new JobHeader.Loader((IJobHeaderParent)domesticShipment).TryCreateWithoutMutexForTestOnly();
			Factory.Save();

			var approvalForDomesticShipment = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			approvalForDomesticShipment.Initialize(domesticShipment);
			AssertEquals(Core.Constants.DomesticPaymentTerms.CollectThirdParty, approvalForDomesticShipment.IncoTermCode);
			AssertEquals("Collect 3rd Party", approvalForDomesticShipment.IncoTermDescription);
		}

		public void TestSetupEDocsFactoryToBeSavedWithApprovalFactory()
		{
			var shipment1 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			var approval1 = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			approval1.Initialize(shipment1);
			approval1.DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TesteDocFile1", "MSC");
			AssertEquals("One eDoc attached to approval1", 1, approval1.DocManagerInfo.AllEDocs.Count);

			var shipment2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			var approval2 = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			approval2.Initialize(shipment2);
			approval2.DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TesteDocFile2", "MSC");
			AssertEquals("One eDoc attached to approval2", 1, approval2.DocManagerInfo.AllEDocs.Count);
			approval2.SetupEDocsFactoryToBeSavedWithApprovalFactory();

			Factory.Save();

			var reloadedApproval1 = new BusinessObjectFactory().Load<AccountingBusiness.CreditControlledDocumentsApproval>(approval1.PK);
			AssertEquals("eDoc is not saved as eDoc factory was not saved with approval1 factory.", 0, reloadedApproval1.DocManagerInfo.AllEDocs.Count);

			var reloadedApproval2 = new BusinessObjectFactory().Load<AccountingBusiness.CreditControlledDocumentsApproval>(approval2.PK);
			AssertEquals("eDoc is saved with the approval2 factory save", 1, reloadedApproval2.DocManagerInfo.AllEDocs.Count);
			var eDoc = reloadedApproval2.DocManagerInfo.AllEDocs[0];
			AssertEquals("FileName", "TesteDocFile2", eDoc.FileName);
			AssertEquals("ImageData", new byte[] { 1, 2, 3 }, eDoc.ImageData);
			AssertEquals("DocType", "MSC", eDoc.DocType);
		}

		public void TestInitializeBizo()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var localClient = testObjectCreator.ABIGAS;
			var debtor1 = testObjectCreator.Debtor;
			var debtor2 = testObjectCreator.Agent;
			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.SellRate, 0.5M);
			var shipment = testObjectCreator.CreateShipment("S1");
			var job = testObjectCreator.CreateJob(shipment, localClient, 0M, testObjectCreator.AALSHI, 0M);

			testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Test charge 1", null, 0M, null, testObjectCreator.AUD, 100M, debtor1);
			testObjectCreator.CreateCharge(job, testObjectCreator.CC2, "Test charge 2", null, 0M, null, testObjectCreator.AUD, 200M, debtor1);
			var charge3 = testObjectCreator.CreateCharge(job, testObjectCreator.CC3, "Test charge 3", null, 0M, null, testObjectCreator.AUD, 300M, localClient);
			charge3.JR_AT_SellGSTRate = testObjectCreator.GST1.PK;

			var invoice = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001001", testObjectCreator.USD, 2M, 400M, 0M, 200M, 0M, localClient, testObjectCreator.CC3.PK);
			var charge4 = testObjectCreator.CreateCharge(invoice.Lines[0], job, testObjectCreator.CC3, testObjectCreator.USD);

			var orgCreditControlCollection = new OrgsEvaluatedForCreditControlCollection();
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollection);
			AccountingConfigurationRegistry.Instance.ResubmitCreditApprovalRequestsBasedOnBillingValues.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ResubmitCreditApprovalRequestsBasedOnBillingValuesTypes.Increased.Code);

			Factory.Save();

			long nextApprovalRequestNumber = Env.NumberFountains.CreditControlApproval.GetTodaysPeriodFountain().PeekPreliminary(Db.Connection);

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuPath = "menu/path";
			menuItem.SU_MenuName = "name";

			TestCreditControlledDocumentsApproval.Initialize(shipment, menuItem.PK, new int[] { 2 });
			AssertEquals(string.Empty, TestCreditControlledDocumentsApproval.XP_RequestID);
			AssertEquals(shipment.PK, TestCreditControlledDocumentsApproval.XP_ParentID);
			AssertEquals(shipment.TablePrefix, TestCreditControlledDocumentsApproval.XP_ParentTableCode);
			AssertEquals("2", TestCreditControlledDocumentsApproval.XP_PrivledgeRequired);
			AssertEquals("", TestCreditControlledDocumentsApproval.XP_ReasonDescription);

			AssertEquals("", 2, TestCreditControlledDocumentsApproval.ApprovalData_ForTestOnly.OrganizationsForCreditCheckWithAmountsCollection.Count);
			AssertOrganizationDetails(localClient, 200M, 330M);
			AssertOrganizationDetails(debtor1, 0M, 330M);

			var charge5 = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Test charge 5", null, 0M, null, testObjectCreator.AUD, 500M, debtor2);

			creditControlledDocumentsApproval = null;
			TestCreditControlledDocumentsApproval.Initialize(shipment, menuItem.PK, new int[] { 2 }, "TestReason");
			AssertEquals(string.Empty, TestCreditControlledDocumentsApproval.XP_RequestID);
			AssertEquals(shipment.PK, TestCreditControlledDocumentsApproval.XP_ParentID);
			AssertEquals(shipment.TablePrefix, TestCreditControlledDocumentsApproval.XP_ParentTableCode);
			AssertEquals("2", TestCreditControlledDocumentsApproval.XP_PrivledgeRequired);
			AssertEquals("TestReason", TestCreditControlledDocumentsApproval.XP_ReasonDescription);
			AssertEquals(1, TestCreditControlledDocumentsApproval.Documents.Count);
			AssertEquals("menu/path/name", TestCreditControlledDocumentsApproval.Documents[0].DocumentName);

			AssertOrganizationDetails(localClient, 200M, 330M);
			AssertOrganizationDetails(debtor1, 0M, 330M);
			AssertOrganizationDetails(debtor2, 0M, 550M);

			var maxLength = GenApprovalRequest.Schema.XP_ReasonDescriptionMaxLength;
			var longStringBuilder = new ZStringBuilder();
			for (int i = -3; i < (maxLength / 5); i++)
			{
				longStringBuilder.Append("ABCDE");
			}
			var testInput = longStringBuilder.ToString();
			TestCreditControlledDocumentsApproval.Initialize(shipment, menuItem.PK, new int[] { 2 }, testInput);
			AssertEquals(string.Empty, TestCreditControlledDocumentsApproval.XP_RequestID);
			AssertEquals(maxLength, TestCreditControlledDocumentsApproval.XP_ReasonDescription.Length);
			AssertNotEquals(testInput, TestCreditControlledDocumentsApproval.XP_ReasonDescription);
			AssertNotEquals(testInput.Substring(maxLength), TestCreditControlledDocumentsApproval.XP_ReasonDescription);

			Factory.Save();

			AssertEquals(nextApprovalRequestNumber + 1, long.Parse(TestCreditControlledDocumentsApproval.XP_RequestID));
		}

		void AssertOrganizationDetails(OrgHeader expectedOrg, ZDecimal expectedPostedAmount, ZDecimal expectedUnpostedAmount)
		{
			var abigasOrgFromApprovalData = TestCreditControlledDocumentsApproval.ApprovalData_ForTestOnly.OrganizationsForCreditCheckWithAmountsCollection.First(x => x.OrgPK == expectedOrg.PK);
			AssertNotNull(abigasOrgFromApprovalData);
			AssertEquals("Posted amount for ABIGAS", expectedPostedAmount, abigasOrgFromApprovalData.PostedAmount);
			AssertEquals("Unposted amount for ABIGAS", expectedUnpostedAmount, abigasOrgFromApprovalData.UnpostedAmount);
		}

		public void TestDefaultValues()
		{
			AssertEquals(GlbBranch.CurrentBranch.PK, TestCreditControlledDocumentsApproval.XP_GB_RequestingBranch);
			AssertEquals(Core.Constants.GenApprovalRequestSubSystem.Accounting, TestCreditControlledDocumentsApproval.XP_SubSystem);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalType.ARCreditControlledDocuments, TestCreditControlledDocumentsApproval.XP_ApprovalType);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, TestCreditControlledDocumentsApproval.XP_ApprovalStatus);
		}

		public void TestJobNumber()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1";
			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreateWithoutMutexForTestOnly();

			AssertEquals("", TestCreditControlledDocumentsApproval.JobNumber);
			TestCreditControlledDocumentsApproval.Initialize((BusinessObject)shipment);
			AssertEquals("S1", TestCreditControlledDocumentsApproval.JobNumber);
		}

		public void TestJobNumberWhenNoJobHeader()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1";
			TestCreditControlledDocumentsApproval.XP_ParentID = shipment.PK;
			TestCreditControlledDocumentsApproval.XP_ParentTableCode = "JS";

			AssertEquals("JobNumber should be populated without initialize", "S1", TestCreditControlledDocumentsApproval.JobNumber);
			TestCreditControlledDocumentsApproval.Initialize((BusinessObject)shipment);
			AssertEquals("JobNumber should be populated with initialize", "S1", TestCreditControlledDocumentsApproval.JobNumber);
		}

		public void TestJobNumberPopulation()
		{
			var list = JobInvoicingConsumerTypes.New();
			BusinessObject bizO;

			foreach (JobInvoicingConsumerType type in list)
			{
				bizO = CreateBizObj(type);
				if (bizO is IJobInvoicingPlugIn && bizO is ICreditControlledDocumentDelivery)
				{
					Factory.Save();
					var correctCode = AccountingBusiness.CreditControlledDocumentsApproval.GetJobNumber(bizO);
					AssertNotEquals("Precondition: bizO jobNumber should not be empty", ZString.Empty, correctCode);

					var request = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
					request.Initialize(bizO);
					AssertEquals("bizO jobNumber should not be empty", correctCode, request.JobNumber);
				}
			}
		}

		public void TestLastRequestRejectedForDocument()
		{
			var menuItem1 = Factory.New<StmMenuItem>();
			menuItem1.SU_MenuPath = "menu/path";
			menuItem1.SU_MenuName = "name1";
			var menuItem2 = Factory.New<StmMenuItem>();
			menuItem2.SU_MenuPath = "menu/path";
			menuItem2.SU_MenuName = "name2";

			Forwarding.IForwardingShipment shipment1 = Factory.New<Forwarding.IForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S001001";
			new JobHeader.Loader((IJobHeaderParent)shipment1).TryCreateWithoutMutexForTestOnly();

			var shipment1_menu1_Request1 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			shipment1_menu1_Request1.Initialize((BusinessObject)shipment1, menuItem1.PK);

			var shipment1_menu2_Request1 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			shipment1_menu2_Request1.Initialize((BusinessObject)shipment1, menuItem2.PK);

			Factory.Save();

			AssertNull(shipment1_menu1_Request1.GetLastRequestIfWasRejected());
			AssertNull(shipment1_menu2_Request1.GetLastRequestIfWasRejected());

			shipment1_menu1_Request1.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Rejected, true);
			shipment1_menu2_Request1.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Rejected, true);

			Factory.Save();

			AssertNotNull(shipment1_menu1_Request1.GetLastRequestIfWasRejected());
			AssertNotNull(shipment1_menu2_Request1.GetLastRequestIfWasRejected());

			var shipment1_menu1_Request2 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			shipment1_menu1_Request2.Initialize((BusinessObject)shipment1, menuItem1.PK);

			Factory.Save();

			AssertNull(shipment1_menu1_Request1.GetLastRequestIfWasRejected());
			AssertNotNull(shipment1_menu2_Request1.GetLastRequestIfWasRejected());
		}

		public void TestLastRequestRejectedForDocument_BasedOnDebtorAndAmountIncreasing()
		{
			AccountingConfigurationRegistry.Instance.ResubmitCreditApprovalRequestsBasedOnBillingValues.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ResubmitCreditApprovalRequestsBasedOnBillingValuesTypes.Increased.Code);
			SetupAndAssertLastRequestRejectedForDocument_BasedOnDebtorAndAmount(false);
		}

		public void TestLastRequestRejectedForDocument_BasedOnDebtorAndBothIncreasingOrDecreasingAmounts()
		{
			AccountingConfigurationRegistry.Instance.ResubmitCreditApprovalRequestsBasedOnBillingValues.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ResubmitCreditApprovalRequestsBasedOnBillingValuesTypes.BothIncreasedOrDecreased.Code);
			SetupAndAssertLastRequestRejectedForDocument_BasedOnDebtorAndAmount(true);
		}

		void SetupAndAssertLastRequestRejectedForDocument_BasedOnDebtorAndAmount(bool triggerApprovalRequestIfAmountDecreases)
		{
			var orgCreditControlCollection = new OrgsEvaluatedForCreditControlCollection();
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollection);

			var localClient = TestObjectCreator.LocalClient;
			var debtor1 = TestObjectCreator.Debtor;

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, localClient, 0M, TestObjectCreator.AALSHI, 0M);

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Test charge 1", null, 0M, null, TestObjectCreator.AUD, 100M, localClient);

			var request1 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request1.Initialize(shipment);
			Factory.Save();
			AssertNull("No previous request rejected", request1.GetLastRequestIfWasRejected());

			charge1.JR_OH_SellAccount = debtor1.PK;

			var request2 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request2.Initialize(shipment);
			Factory.Save();
			AssertNull("No previous request rejected yet", request2.GetLastRequestIfWasRejected());

			request2.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Rejected, true);

			var request3 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request3.Initialize(shipment);
			Factory.Save();
			AssertNotNull("Previous request rejected", request3.GetLastRequestIfWasRejected());

			charge1.JR_LocalSellAmt = 90M;
			request3.Initialize(shipment);
			if (triggerApprovalRequestIfAmountDecreases)
			{
				AssertNull("Charge amount decreased and it requires new Approval request", request3.GetLastRequestIfWasRejected());
			}
			else
			{
				AssertNotNull("Charge amount decreased but it does not require new Approval request hence previous request rejected still holds", request3.GetLastRequestIfWasRejected());
			}

			charge1.JR_LocalSellAmt = 200M;
			request3.Initialize(shipment);
			AssertNull("New approval request will be queued since the amount has changed, making the last request not rejected any more", request3.GetLastRequestIfWasRejected());
		}

		public void TestRequestAlreadyApprovedOrRejectedForDocument()
		{
			var menuItem1 = Factory.New<StmMenuItem>();
			menuItem1.SU_MenuPath = "menu/path";
			menuItem1.SU_MenuName = "name2";
			var menuItem2 = Factory.New<StmMenuItem>();
			menuItem2.SU_MenuPath = "menu/path";
			menuItem2.SU_MenuName = "name3";

			Forwarding.IForwardingShipment shipment1;
			Forwarding.IForwardingShipment shipment2;
			Forwarding.IForwardingShipment shipment3;
			SetupCurrentRequestsScenario(out shipment1, out shipment2, out shipment3);

			var request_shipment1 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_shipment1.Initialize((BusinessObject)shipment1, menuItem1.PK);
			AssertEquals(false, request_shipment1.RequestAlreadyApproved);
			AssertNull(request_shipment1.GetLastRequestIfWasRejected());

			var request_shipment1_2 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_shipment1_2.Initialize((BusinessObject)shipment1, menuItem1.PK);
			AssertEquals(false, request_shipment1.RequestAlreadyApproved);
			AssertNull(request_shipment1.GetLastRequestIfWasRejected());

			request_shipment1.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Approved, false);
			request_shipment1.Initialize((BusinessObject)shipment1, menuItem1.PK);
			AssertEquals(true, request_shipment1_2.RequestAlreadyApproved);
			AssertNull(request_shipment1_2.GetLastRequestIfWasRejected());
			request_shipment1.Initialize((BusinessObject)shipment1, menuItem2.PK);
			AssertEquals(false, request_shipment1_2.RequestAlreadyApproved);
			AssertNull(request_shipment1_2.GetLastRequestIfWasRejected());

			request_shipment1.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Approved, true);
			request_shipment1_2.Initialize((BusinessObject)shipment1, menuItem1.PK);
			AssertEquals(true, request_shipment1_2.RequestAlreadyApproved);
			AssertNull(request_shipment1_2.GetLastRequestIfWasRejected());
			request_shipment1_2.Initialize((BusinessObject)shipment1, menuItem2.PK);
			AssertEquals(true, request_shipment1_2.RequestAlreadyApproved);
			AssertNull(request_shipment1_2.GetLastRequestIfWasRejected());

			request_shipment1.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, true);
			request_shipment1_2.Initialize((BusinessObject)shipment1, menuItem1.PK);
			AssertEquals(false, request_shipment1_2.RequestAlreadyApproved);
			AssertNull(request_shipment1_2.GetLastRequestIfWasRejected());
			request_shipment1_2.Initialize((BusinessObject)shipment1, menuItem2.PK);
			AssertEquals(false, request_shipment1_2.RequestAlreadyApproved);
			AssertNull(request_shipment1_2.GetLastRequestIfWasRejected());

			request_shipment1.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Rejected, true);
			request_shipment1_2.Initialize((BusinessObject)shipment1, menuItem1.PK);
			AssertEquals(false, request_shipment1_2.RequestAlreadyApproved);
			AssertNotNull(request_shipment1_2.GetLastRequestIfWasRejected());
			request_shipment1_2.Initialize((BusinessObject)shipment1, menuItem2.PK);
			AssertEquals(false, request_shipment1_2.RequestAlreadyApproved);
			AssertNotNull(request_shipment1_2.GetLastRequestIfWasRejected());

			request_shipment1.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Approved, true);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var shipment1_newFactory = Factory.Load<Forwarding.IForwardingShipment>(shipment1.PK);
			var request_shipment1_newFactory = newFactory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_shipment1_newFactory.Initialize((BusinessObject)shipment1_newFactory, menuItem1.PK);
			AssertEquals(true, request_shipment1_newFactory.RequestAlreadyApproved);
			request_shipment1_newFactory.Initialize((BusinessObject)shipment1_newFactory, menuItem2.PK);
			AssertEquals(true, request_shipment1_newFactory.RequestAlreadyApproved);
		}

		public void TestRequestAlreadyApprovedOrRejectedForDocument_Consol()
		{
			var shipment = TestObjectCreator.CreateShipment("S1", false);
			TestObjectCreator.CreateJob(shipment);

			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "C1";
			Factory.Save();

			AssertNotNull(Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK)));

			AssertApprovalRequest(consol);
		}

		public void TestRequestAlreadyApprovedOrRejectedForDocument_ShipmentWithJob()
		{
			var orgCreditOnHold = Factory.NewWithValidTestData<OrgHeader>();
			orgCreditOnHold.CompanyData.OB_AROnCreditHold = true;
			orgCreditOnHold.OH_IsDebtor = true;

			var shipment = TestObjectCreator.CreateShipment("S1", false);
			shipment.ConsigneePK = orgCreditOnHold.PK;
			TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			AssertNotNull(Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK)));

			AssertApprovalRequest(shipment);
		}

		public void TestRequestAlreadyApprovedOrRejectedForDocument_ShipmentWithoutJob()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob
				.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);

			var shipment = Factory.New<Freight.Business.CommonShipment>();
			shipment.JS_UniqueConsignRef = "S1";
			Factory.Save();

			AssertNull(Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK)));

			AssertApprovalRequest(shipment);
		}

		void AssertApprovalRequest(BusinessObject parentBizO)
		{
			var stmMenuItem = Factory.New<StmMenuItem>();
			stmMenuItem.SU_MenuPath = "menu/path";
			stmMenuItem.SU_MenuName = "name2";

			AccountingConfigurationRegistry.Instance.ResubmitCreditApprovalRequestsBasedOnBillingValues
				.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ResubmitCreditApprovalRequestsBasedOnBillingValuesTypes.Default.Code);

			AssertRequestStatusWhenOtherReqeustIsApproved(stmMenuItem, parentBizO);

			AssertShouldNotRejectCancelledRequest(stmMenuItem, parentBizO);

			AssertWhenRejected(stmMenuItem, parentBizO);

			AssertReloadApprovedRequest(stmMenuItem, parentBizO);

			AccountingConfigurationRegistry.Instance.ResubmitCreditApprovalRequestsBasedOnBillingValues
					.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ResubmitCreditApprovalRequestsBasedOnBillingValuesTypes.Increased.Code);
			AssertReloadApprovedRequest(stmMenuItem, parentBizO, true);

			AccountingConfigurationRegistry.Instance.ResubmitCreditApprovalRequestsBasedOnBillingValues
				.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ResubmitCreditApprovalRequestsBasedOnBillingValuesTypes.BothIncreasedOrDecreased.Code);
			AssertReloadApprovedRequest(stmMenuItem, parentBizO, true);
		}

		void AssertRequestStatusWhenOtherReqeustIsApproved(StmMenuItem menuItem, BusinessObject parentBizO)
		{
			FormatRequestSetting(parentBizO);
			var approvedRequest = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			approvedRequest.Initialize(parentBizO, menuItem.PK);

			approvedRequest.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Approved, false);
			AssertEquals("No other approved request", false, approvedRequest.RequestAlreadyApproved);
			CreateAndAssertNewApprovalRequest(menuItem, parentBizO, newRequest => {
				AssertEquals(true, newRequest.RequestAlreadyApproved);
				AssertNull(newRequest.GetLastRequestIfWasRejected());
			});

			approvedRequest.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, false);
			CreateAndAssertNewApprovalRequest(menuItem, parentBizO, newRequest => {
				AssertEquals(false, newRequest.RequestAlreadyApproved);
				AssertNull(newRequest.GetLastRequestIfWasRejected());
			});
		}

		void AssertShouldNotRejectCancelledRequest(StmMenuItem menuItem, BusinessObject parentBizO)
		{
			FormatRequestSetting(parentBizO);
			var cancelledRequest = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			cancelledRequest.Initialize(parentBizO, menuItem.PK);
			cancelledRequest.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, false);

			CreateAndAssertNewApprovalRequest(menuItem, parentBizO, newRequest => {
				AssertEquals(false, newRequest.RequestAlreadyApproved);
				AssertNull(newRequest.GetLastRequestIfWasRejected());
			});
		}

		void AssertWhenRejected(StmMenuItem menuItem, BusinessObject parentBizO)
		{
			FormatRequestSetting(parentBizO);
			var rejectedRequest = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			rejectedRequest.Initialize(parentBizO, menuItem.PK);
			rejectedRequest.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Rejected, false);

			CreateAndAssertNewApprovalRequest(menuItem, parentBizO, newRequest => {
				AssertEquals(false, newRequest.RequestAlreadyApproved);
				AssertNotNull(newRequest.GetLastRequestIfWasRejected());
			});
		}

		void FormatRequestSetting(BusinessObject parentBizO)
		{
			var approvalRequests = Factory.Load<AccountingBusiness.CreditControlledDocumentsApproval>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, parentBizO.PK));
			approvalRequests.ForEach(request => {
				request.Initialize(parentBizO, ZGuid.Empty);
				request.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Requested, false);
				AssertEquals(false, request.RequestAlreadyApproved);
				AssertNull(request.GetLastRequestIfWasRejected());
			});
		}

		void AssertReloadApprovedRequest(StmMenuItem menuItem, BusinessObject parentBizO, bool isTestJobCreatingAfterRequestApprovedWitoutJob = false)
		{
			FormatRequestSetting(parentBizO);
			var approvedRequest = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			approvedRequest.Initialize(parentBizO, menuItem.PK);
			approvedRequest.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Approved, false);

			CreateAndAssertNewApprovalRequest(menuItem, parentBizO, newRequest => {
				Factory.Save();

				var requestInNewFactory = new BusinessObjectFactory().Load<AccountingBusiness.CreditControlledDocumentsApproval>(newRequest.PK);
				AssertEquals(true, requestInNewFactory.RequestAlreadyApproved);
				AssertNull(requestInNewFactory.GetLastRequestIfWasRejected());

				if (isTestJobCreatingAfterRequestApprovedWitoutJob
					&& parentBizO is IJobHeaderParent jobHeaderParent
					&& !requestInNewFactory.Factory.Exists(typeof(JobHeader), new ZQuery(JobHeaderSchema.JH_ParentID, parentBizO.PK))
					&& new JobHeader.Loader(requestInNewFactory.Factory, jobHeaderParent).TryCreateWithoutMutexForTestOnly() != null)
				{
					AssertEquals(false, requestInNewFactory.RequestAlreadyApproved);
					AssertNull(requestInNewFactory.GetLastRequestIfWasRejected());
				}
			});
			Factory.Save();
		}

		void CreateAndAssertNewApprovalRequest(StmMenuItem menuItem, BusinessObject parentBizO, Action<AccountingBusiness.CreditControlledDocumentsApproval> assert, bool keepNewRequest = false)
		{
			var newRequest = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			newRequest.Initialize(parentBizO, menuItem.PK);
			newRequest.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Requested, false);
			assert(newRequest);

			newRequest.Delete();
		}

		public void TestAlreadyApprovedWithApprovalLevel()
		{
			var menuItem1 = Factory.New<StmMenuItem>();
			menuItem1.SU_MenuPath = "menu/path";
			menuItem1.SU_MenuName = "name2";

			var shipment1 = Factory.New<Forwarding.IForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S1";
			new JobHeader.Loader((IJobHeaderParent)shipment1).TryCreateWithoutMutexForTestOnly();

			var shipment2 = Factory.New<Forwarding.IForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S2";
			new JobHeader.Loader((IJobHeaderParent)shipment2).TryCreateWithoutMutexForTestOnly();

			var shipment3 = Factory.New<Forwarding.IForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "S3";
			new JobHeader.Loader((IJobHeaderParent)shipment3).TryCreateWithoutMutexForTestOnly();

			var shipment4 = Factory.New<Forwarding.IForwardingShipment>();
			shipment4.JS_UniqueConsignRef = "S4";
			new JobHeader.Loader((IJobHeaderParent)shipment4).TryCreateWithoutMutexForTestOnly();

			#region Set1
			var requestSet1_shipment1 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet1_shipment1.Initialize((BusinessObject)shipment1, menuItem1.PK, new int[] { 1 });
			requestSet1_shipment1.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Approved, approveAllDocuments: false);

			var requestSet1_shipment1_2 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet1_shipment1_2.Initialize((BusinessObject)shipment1, menuItem1.PK, new int[] { 1 });
			AssertEquals("Set1 - Approval level 1", true, requestSet1_shipment1_2.RequestAlreadyApproved);

			var requestSet1_shipment1_3 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet1_shipment1_3.Initialize((BusinessObject)shipment1, menuItem1.PK, new int[] { 2 });
			AssertEquals("Set1 - Approval level 2", false, requestSet1_shipment1_3.RequestAlreadyApproved);

			var requestSet1_shipment1_4 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet1_shipment1_4.Initialize((BusinessObject)shipment1, menuItem1.PK, new int[] { 3 });
			AssertEquals("Set1 - Approval level 3", false, requestSet1_shipment1_4.RequestAlreadyApproved);

			var requestSet1_shipment1_5 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet1_shipment1_5.Initialize((BusinessObject)shipment1, menuItem1.PK, new int[] { 1, 2, 3 });
			AssertEquals("Set1 - Approval level 1,2,3", true, requestSet1_shipment1_5.RequestAlreadyApproved);

			var requestSet1_shipment1_6 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet1_shipment1_6.Initialize((BusinessObject)shipment1, menuItem1.PK, new int[] { 2, 3 });
			AssertEquals("Set1 - Approval level 2,3", false, requestSet1_shipment1_6.RequestAlreadyApproved);

			#endregion

			#region Set2
			var requestSet2_shipment2 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet2_shipment2.Initialize((BusinessObject)shipment2, menuItem1.PK, new int[] { 2 });
			requestSet2_shipment2.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Approved, approveAllDocuments: false);

			var requestSet2_shipment2_2 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet2_shipment2_2.Initialize((BusinessObject)shipment2, menuItem1.PK, new int[] { 1 });
			AssertEquals("Set2 - Approval level 1", false, requestSet2_shipment2_2.RequestAlreadyApproved);

			var requestSet2_shipment2_3 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet2_shipment2_3.Initialize((BusinessObject)shipment2, menuItem1.PK, new int[] { 2 });
			AssertEquals("Set2 - Approval level 2", true, requestSet2_shipment2_3.RequestAlreadyApproved);

			var requestSet2_shipment2_4 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet2_shipment2_4.Initialize((BusinessObject)shipment2, menuItem1.PK, new int[] { 3 });
			AssertEquals("Set2 - Approval level 3", false, requestSet2_shipment2_4.RequestAlreadyApproved);

			var requestSet2_shipment2_5 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet2_shipment2_5.Initialize((BusinessObject)shipment2, menuItem1.PK, new int[] { 1, 2, 3 });
			AssertEquals("Set2 - Approval level 1,2,3", true, requestSet2_shipment2_5.RequestAlreadyApproved);

			var requestSet2_shipment2_6 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet2_shipment2_6.Initialize((BusinessObject)shipment2, menuItem1.PK, new int[] { 2, 3 });
			AssertEquals("Set2 - Approval level 2,3", true, requestSet2_shipment2_6.RequestAlreadyApproved);
			#endregion

			#region Set3
			var requestSet3_shipment1 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet3_shipment1.Initialize((BusinessObject)shipment3, menuItem1.PK, new int[] { 3 });
			requestSet3_shipment1.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Approved, approveAllDocuments: false);

			var requestSet3_shipment3_2 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet3_shipment3_2.Initialize((BusinessObject)shipment3, menuItem1.PK, new int[] { 1 });
			AssertEquals("Set3 - Approval level 1", false, requestSet3_shipment3_2.RequestAlreadyApproved);

			var requestSet3_shipment3_3 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet3_shipment3_3.Initialize((BusinessObject)shipment3, menuItem1.PK, new int[] { 2 });
			AssertEquals("Set3 - Approval level 2", false, requestSet3_shipment3_3.RequestAlreadyApproved);

			var requestSet3_shipment3_4 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet3_shipment3_4.Initialize((BusinessObject)shipment3, menuItem1.PK, new int[] { 3 });
			AssertEquals("Set3 - Approval level 3", true, requestSet3_shipment3_4.RequestAlreadyApproved);

			var requestSet3_shipment3_5 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet3_shipment3_5.Initialize((BusinessObject)shipment3, menuItem1.PK, new int[] { 1, 2, 3 });
			AssertEquals("Set3 - Approval level 1,2,3", true, requestSet3_shipment3_5.RequestAlreadyApproved);

			var requestSet3_shipment3_6 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet3_shipment3_6.Initialize((BusinessObject)shipment3, menuItem1.PK, new int[] { 2, 3 });
			AssertEquals("Set3 - Approval level 2,3", true, requestSet3_shipment3_6.RequestAlreadyApproved);
			#endregion

			#region Set4
			var requestSet4_shipment4 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet4_shipment4.Initialize((BusinessObject)shipment4, menuItem1.PK);
			requestSet4_shipment4.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Approved, approveAllDocuments: false);

			var requestSet4_shipment4_2 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet4_shipment4_2.Initialize((BusinessObject)shipment4, menuItem1.PK, new int[] { 1 });
			AssertEquals("Set4 - Approval level 1", false, requestSet4_shipment4_2.RequestAlreadyApproved);

			var requestSet4_shipment4_3 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet4_shipment4_3.Initialize((BusinessObject)shipment4, menuItem1.PK, new int[] { 2 });
			AssertEquals("Set4 - Approval level 2", false, requestSet4_shipment4_3.RequestAlreadyApproved);

			var requestSet4_shipment4_4 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet4_shipment4_4.Initialize((BusinessObject)shipment4, menuItem1.PK, new int[] { 3 });
			AssertEquals("Set4 - Approval level 3", false, requestSet4_shipment4_4.RequestAlreadyApproved);

			var requestSet4_shipment4_5 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet4_shipment4_5.Initialize((BusinessObject)shipment4, menuItem1.PK, new int[] { 1, 2, 3 });
			AssertEquals("Set4 - Approval level 1,2,3", false, requestSet4_shipment4_5.RequestAlreadyApproved);

			var requestSet4_shipment4_6 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			requestSet4_shipment4_6.Initialize((BusinessObject)shipment4, menuItem1.PK, new int[] { 2, 3 });
			AssertEquals("Set4 - Approval level 2,3", false, requestSet4_shipment4_6.RequestAlreadyApproved);
			#endregion
		}

		public void TestRequestAlreadyMade()
		{
			var menuItem1 = Factory.New<StmMenuItem>();
			menuItem1.SU_MenuPath = "menu/path";
			menuItem1.SU_MenuName = "name2";
			var menuItem2 = Factory.New<StmMenuItem>();
			menuItem2.SU_MenuPath = "menu/path";
			menuItem2.SU_MenuName = "name3";

			Forwarding.IForwardingShipment shipment1;
			Forwarding.IForwardingShipment shipment2;
			Forwarding.IForwardingShipment shipment3;
			SetupCurrentRequestsScenario(out shipment1, out shipment2, out shipment3);

			var request_shipment1 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_shipment1.Initialize((BusinessObject)shipment1, menuItem1.PK, new int[] { 1 });
			AssertEquals("Returns false because it is only the instance approval that matches.", false, request_shipment1.RequestAlreadyMade);
			Factory.Save();
			AssertEquals("Returns false because it is only the instance approval that matches.", false, request_shipment1.RequestAlreadyMade);

			var request_shipment1_2 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_shipment1_2.Initialize((BusinessObject)shipment1, menuItem1.PK, new int[] { 1 });
			AssertEquals("Already an approval request is there", true, request_shipment1_2.RequestAlreadyMade);
			Factory.Save();
			AssertEquals("Already an approval request is there", true, request_shipment1_2.RequestAlreadyMade);

			var request_shipment1_3 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_shipment1_3.Initialize((BusinessObject)shipment1, menuItem1.PK, new int[] { 2 });
			AssertEquals("Different privilege", false, request_shipment1_3.RequestAlreadyMade);
			Factory.Save();
			AssertEquals("Different privilege", false, request_shipment1_3.RequestAlreadyMade);

			var request_shipment1_4 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_shipment1_4.Initialize((BusinessObject)shipment1, menuItem1.PK, new int[] { 2 });
			AssertEquals("Request already exists", true, request_shipment1_4.RequestAlreadyMade);
			Factory.Save();
			AssertEquals("Request already exists", true, request_shipment1_4.RequestAlreadyMade);

			var newFactory = new BusinessObjectFactory();
			var request_shipment1_newFacotry = newFactory.Load<GenApprovalRequest>(request_shipment1.PK);
			AssertEquals("The previous request now with obsolete privilege is now cancelled", Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, request_shipment1_newFacotry.XP_ApprovalStatus);

			var request_shipment1_3_newFacotry = newFactory.Load<GenApprovalRequest>(request_shipment1_3.PK);
			AssertEquals("This request is not cancelled because it is still the current approval request", Core.Constants.GenApprovalRequestApprovalStatus.Requested, request_shipment1_3_newFacotry.XP_ApprovalStatus);

			var request_shipment2 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_shipment2.Initialize((BusinessObject)shipment2, menuItem1.PK, new int[] { 1 });
			AssertEquals("No other request for this shipment", false, request_shipment2.RequestAlreadyMade);
			Factory.Save();
			AssertEquals("No other request for this shipment", false, request_shipment2.RequestAlreadyMade);

			var request_shipment3 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_shipment3.Initialize((BusinessObject)shipment3, menuItem1.PK, new int[] { 2 });
			request_shipment3.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Approved, true);
			AssertEquals("Returns false because it is only the instance approval that matches.", false, request_shipment3.RequestAlreadyMade);
			Factory.Save();
			AssertEquals("Returns false because it is only the instance approval that matches.", false, request_shipment3.RequestAlreadyMade);

			var request_shipment3_2 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_shipment3_2.Initialize((BusinessObject)shipment3, menuItem2.PK, new int[] { 2 });
			AssertEquals("Another approval for a different doc was approved for all docs", true, request_shipment3_2.RequestAlreadyMade);
			Factory.Save();
			AssertEquals("Another approval for a different doc was approved for all docs", true, request_shipment3_2.RequestAlreadyMade);

			request_shipment3_2.Initialize((BusinessObject)shipment3, menuItem1.PK, new int[] { 2 });
			AssertEquals("Another approval for the same doc was approved for all docs", true, request_shipment3_2.RequestAlreadyMade);
			Factory.Save();
			AssertEquals("Another approval for the same doc was approved for all docs", true, request_shipment3_2.RequestAlreadyMade);

			request_shipment1.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Approved, false);
			AssertEquals("Approved is included", true, request_shipment1_2.RequestAlreadyMade);
			Factory.Save();
			AssertEquals("Approved is included", true, request_shipment1_2.RequestAlreadyMade);

			request_shipment1.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, false);
			AssertEquals("Cancelled is excluded", false, request_shipment1_2.RequestAlreadyMade);
			Factory.Save();
			AssertEquals("Cancelled is excluded", false, request_shipment1_2.RequestAlreadyMade);

			request_shipment1.SetStatus(Core.Constants.GenApprovalRequestApprovalStatus.Rejected, false);
			AssertEquals("Rejected is included", true, request_shipment1_2.RequestAlreadyMade);
			Factory.Save();
			AssertEquals("Rejected is included", true, request_shipment1_2.RequestAlreadyMade);
		}

		public void TestRequestAlreadyMade_BasedOnDebtorAndIncreasingAmounts()
		{
			AccountingConfigurationRegistry.Instance.ResubmitCreditApprovalRequestsBasedOnBillingValues.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ResubmitCreditApprovalRequestsBasedOnBillingValuesTypes.Increased.Code);
			SetupAndAssertRequestAlreadyMade_BasedOnDebtorAndAmounts(false);
		}

		public void TestRequestAlreadyMade_BasedOnDebtorAndBothIncreasingAndDecreasingAmounts()
		{
			AccountingConfigurationRegistry.Instance.ResubmitCreditApprovalRequestsBasedOnBillingValues.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ResubmitCreditApprovalRequestsBasedOnBillingValuesTypes.BothIncreasedOrDecreased.Code);
			SetupAndAssertRequestAlreadyMade_BasedOnDebtorAndAmounts(true);
		}

		void SetupAndAssertRequestAlreadyMade_BasedOnDebtorAndAmounts(bool triggerApprovalRequestWhenAmountDecreases)
		{
			var orgCreditControlCollection = new OrgsEvaluatedForCreditControlCollection();
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollection);

			var localClient = TestObjectCreator.LocalClient;
			var debtor = TestObjectCreator.Debtor;

			localClient.CompanyData.SetARTaxApplicable(false);
			debtor.CompanyData.SetARTaxApplicable(false);

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, localClient, 0M, TestObjectCreator.AALSHI, 0M);

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Test charge 1", null, 0M, null, TestObjectCreator.AUD, 100M, localClient);
			var request1 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request1.Initialize(shipment);
			Assert("No previous request made yet", !request1.RequestAlreadyMade);
			Factory.Save();
			Assert("No previous request made yet", !request1.RequestAlreadyMade);

			var request2 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request2.Initialize(shipment);
			Assert("Same request already present", request2.RequestAlreadyMade);
			Factory.Save();
			Assert("Same request already present", request2.RequestAlreadyMade);

			charge1.JR_OSSellAmt = 150M;

			var request3 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request3.Initialize(shipment);
			Assert("New request should be queued as unposted amount increased", !request3.RequestAlreadyMade);
			Factory.Save();
			Assert("New request should be queued as unposted amount increased", !request3.RequestAlreadyMade);

			AssertEquals("Previous requests are cancelled: Request1", Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, request1.XP_ApprovalStatus);
			AssertEquals("Previous requests are cancelled: Request2", Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, request2.XP_ApprovalStatus);

			charge1.JR_OSSellAmt = 120M;

			var request4 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request4.Initialize(shipment);
			if (triggerApprovalRequestWhenAmountDecreases)
			{
				Assert("New request should be queued as unposted amount decreased", !request4.RequestAlreadyMade);
				Factory.Save();
				Assert("New request should be queued as unposted amount decreased", !request4.RequestAlreadyMade);
			}
			else
			{
				Assert("No new request should be queued as unposted amount decreased", request4.RequestAlreadyMade);
				Factory.Save();
				Assert("No new request should be queued as unposted amount decreased", request4.RequestAlreadyMade);
			}

			charge1.JR_OH_SellAccount = debtor.PK;

			var request5 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request5.Initialize(shipment);
			Assert("New request should be queued as debtor has changed", !request5.RequestAlreadyMade);
			Factory.Save();
			Assert("New request should be queued as debtor has changed", !request5.RequestAlreadyMade);

			AssertEquals("Previous requests are cancelled: Request3", Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, request3.XP_ApprovalStatus);
			AssertEquals("Previous requests are cancelled: Request4", Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, request4.XP_ApprovalStatus);

			charge1.JR_OSSellAmt = 60M;

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001001", TestObjectCreator.AUD, 1M, 60M, 0M, 60M, 0M, debtor, TestObjectCreator.CC1.PK);
			var charge2 = TestObjectCreator.CreateCharge(invoice.Lines[0], job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			charge2.JR_OH_SellAccount = debtor.PK;

			var request6 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request6.Initialize(shipment);
			Assert("No new request should be queued as total amount against the debtor has not changed. total amount from last approval request : 120", request6.RequestAlreadyMade);
			Factory.Save();
			Assert("No new request should be queued as total amount against the debtor has not changed. total amount from last approval request : 120", request6.RequestAlreadyMade);

			charge1.JR_OSSellAmt = 200M;

			var request7 = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request7.Initialize(shipment);
			Assert("New request should be queued as unposted amount increased", !request7.RequestAlreadyMade);
			Factory.Save();
			Assert("New request should be queued as unposted amount increased", !request7.RequestAlreadyMade);
		}

		public void TestDocumentsList()
		{
			var menuItem1 = Factory.New<StmMenuItem>();
			menuItem1.SU_MenuPath = "menu/path";
			menuItem1.SU_MenuName = "name";
			var menuItem2 = Factory.New<StmMenuItem>();
			menuItem2.SU_MenuPath = "menu/path";
			menuItem2.SU_MenuName = "name2";

			var bizo = Factory.New<Forwarding.IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S1";
			TestCreditControlledDocumentsApproval.Initialize((BusinessObject)bizo);
			AssertEquals(0, TestCreditControlledDocumentsApproval.Documents.Count);
			TestCreditControlledDocumentsApproval.Initialize((BusinessObject)bizo, menuItem1.PK);
			AssertEquals(1, TestCreditControlledDocumentsApproval.Documents.Count);
			AssertEquals("menu/path/name", TestCreditControlledDocumentsApproval.Documents[0].DocumentName);
			TestCreditControlledDocumentsApproval.Initialize((BusinessObject)bizo, menuItem2.PK);
			AssertEquals(1, TestCreditControlledDocumentsApproval.Documents.Count);
			AssertEquals("menu/path/name2", TestCreditControlledDocumentsApproval.Documents[0].DocumentName);
		}

		public void TestReadonly()
		{
			AssertEquals("XP_GB_RequestingBranchInfo.ReadOnly", true, TestCreditControlledDocumentsApproval.XP_GB_RequestingBranchInfo.ReadOnly);
			AssertEquals("XP_ParentIDInfo.ReadOnly", true, TestCreditControlledDocumentsApproval.XP_ParentIDInfo.ReadOnly);
			AssertEquals("XP_ParentTableCodeInfo.ReadOnly", true, TestCreditControlledDocumentsApproval.XP_ParentTableCodeInfo.ReadOnly);
			AssertEquals("XP_ApprovalTypeInfo.ReadOnly", true, TestCreditControlledDocumentsApproval.XP_ApprovalTypeInfo.ReadOnly);
			AssertEquals("XP_SubSystemInfo.ReadOnly", true, TestCreditControlledDocumentsApproval.XP_SubSystemInfo.ReadOnly);
			AssertEquals("XP_ApprovalDateInfo.ReadOnly", true, TestCreditControlledDocumentsApproval.XP_ApprovalDateInfo.ReadOnly);
			AssertEquals("XP_ApprovalStatusInfo.ReadOnly", true, TestCreditControlledDocumentsApproval.XP_ApprovalStatusInfo.ReadOnly);
			AssertEquals("XP_GS_NKApprovingUser1Info.ReadOnly", true, TestCreditControlledDocumentsApproval.XP_GS_NKApprovingUser1Info.ReadOnly);
			AssertEquals("XP_GS_NKApprovingUser2Info.ReadOnly", true, TestCreditControlledDocumentsApproval.XP_GS_NKApprovingUser2Info.ReadOnly);
			AssertEquals("XP_GS_NKApprovingUser3Info.ReadOnly", true, TestCreditControlledDocumentsApproval.XP_GS_NKApprovingUser3Info.ReadOnly);
			AssertEquals("XP_SystemCreateTimeInfoUtc.ReadOnly", true, TestCreditControlledDocumentsApproval.XP_SystemCreateTimeUtcInfo.ReadOnly);
			AssertEquals("XP_SystemCreateUserInfo.ReadOnly", true, TestCreditControlledDocumentsApproval.XP_SystemCreateUserInfo.ReadOnly);
			AssertEquals("XP_PrivledgeRequiredInfo.ReadOnly", true, TestCreditControlledDocumentsApproval.XP_PrivledgeRequiredInfo.ReadOnly);
		}

		public void TestPersistedData()
		{
			var menuItem1 = Factory.New<StmMenuItem>();
			menuItem1.SU_MenuPath = "menu/path";
			menuItem1.SU_MenuName = "name";

			var bizo = Factory.New<Forwarding.IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S1";
			TestCreditControlledDocumentsApproval.Initialize((BusinessObject)bizo, menuItem1.PK);
			AssertEquals(1, TestCreditControlledDocumentsApproval.Documents.Count);
			AssertEquals("menu/path/name", TestCreditControlledDocumentsApproval.Documents[0].DocumentName);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var reloadedApproval = factory2.Load<AccountingBusiness.CreditControlledDocumentsApproval>(TestCreditControlledDocumentsApproval.PK);

			AssertEquals(1, reloadedApproval.Documents.Count);
			AssertEquals("menu/path/name", reloadedApproval.Documents[0].DocumentName);
		}

		public void TestPersistedDataWhenCreditControlledDocumentIsRejected()
		{
			var menuItem1 = Factory.New<StmMenuItem>();
			menuItem1.SU_MenuPath = "menu/path/1";
			menuItem1.SU_MenuName = "name1";

			var menuItem2 = Factory.New<StmMenuItem>();
			menuItem2.SU_MenuPath = "menu/path/2";
			menuItem2.SU_MenuName = "name2";

			var bizo = Factory.New<Forwarding.IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S1";

			var approval1 = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			approval1.Initialize((BusinessObject)bizo, menuItem1.PK);

			var approval2 = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			approval2.Initialize((BusinessObject)bizo, menuItem2.PK);

			AssertEquals(1, approval1.Documents.Count);
			AssertEquals("menu/path/1/name1", approval1.Documents[0].DocumentName);
			AssertEquals(1, approval2.Documents.Count);
			AssertEquals("menu/path/2/name2", approval2.Documents[0].DocumentName);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var reloadedApproval1 = factory2.Load<AccountingBusiness.CreditControlledDocumentsApproval>(approval1.PK);
			reloadedApproval1.XP_ApprovalStatus = "REJ";
			factory2.Save();

			var approval3 = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			approval3.Initialize((BusinessObject)bizo, menuItem1.PK);
			Factory.Save();

			var factory3 = new BusinessObjectFactory();
			var approval1InNewFactory = factory3.Load<AccountingBusiness.CreditControlledDocumentsApproval>(reloadedApproval1.PK);
			var approval2InNewFactory = factory3.Load<AccountingBusiness.CreditControlledDocumentsApproval>(approval2.PK);
			var approval3InNewFactory = factory3.Load<AccountingBusiness.CreditControlledDocumentsApproval>(approval3.PK);
			AssertEquals(1, approval1InNewFactory.Documents.Count);
			AssertEquals("menu/path/1/name1", approval1InNewFactory.Documents[0].DocumentName);

			AssertEquals(1, approval2InNewFactory.Documents.Count);
			AssertEquals("menu/path/2/name2", approval2InNewFactory.Documents[0].DocumentName);

			AssertEquals(1, approval3InNewFactory.Documents.Count);
			AssertEquals("menu/path/1/name1", approval3InNewFactory.Documents[0].DocumentName);
		}

		public void TestEmailSendingConditions_Reject()
		{
			SpecificEmailSendingConditionsTest(Core.Constants.GenApprovalRequestApprovalStatus.Rejected);
		}

		public void TestEmailSendingConditions_Approve()
		{
			SpecificEmailSendingConditionsTest(Core.Constants.GenApprovalRequestApprovalStatus.Approved);
		}

		public void TestEmailSendingConditions_Cancel()
		{
			SpecificEmailSendingConditionsTest(Core.Constants.GenApprovalRequestApprovalStatus.Cancelled);
		}

		public void TestApprovalDataShouldNotBeNullWhenDeserializerReturnsNull()
		{
			var menutItem = Factory.New<StmMenuItem>();
			menutItem.SU_MenuPath = "menu/path/newbizo";
			menutItem.SU_MenuName = "name";

			var businessObject = Factory.New<Forwarding.IForwardingShipment>();
			businessObject.JS_UniqueConsignRef = "S1";
			var approval = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();

			var xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
						<CreditControlledDocumentsApprovalData xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
							xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xsi:nil=""true"" />";

			approval.XP_ApprovalRequestData = new ZBlob(Encoding.Unicode.GetBytes(xml));

			approval.Initialize((BusinessObject)businessObject, menutItem.PK);

			var approvalData = typeof(AccountingBusiness.CreditControlledDocumentsApproval).GetField("approvalData", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(approval);

			AssertNotNull(approvalData);
		}

		public void TestApprovalDataShouldNotBeNullWhenDeserializerThrowsException()
		{
			var menutItem = Factory.New<StmMenuItem>();
			menutItem.SU_MenuPath = "menu/path/newbizo";
			menutItem.SU_MenuName = "name";

			var businessObject = Factory.New<Forwarding.IForwardingShipment>();
			businessObject.JS_UniqueConsignRef = "S1";
			var approval = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();

			var xml = @"<?xml version=""1.0"" encoding=""utf-16""?><root>";

			approval.XP_ApprovalRequestData = new ZBlob(Encoding.Unicode.GetBytes(xml));

			ErrorReporter.Clear();
			approval.Initialize((BusinessObject)businessObject, menutItem.PK);

			AssertEquals(ErrorReporter.LastKeyReported, "CreditControlledDocumentsApproval.ReadApprovalData.serializer.Deserialize");

			var msg = string.Format("XP_ApprovalRequestData={0}", Convert.ToBase64String(approval.XP_ApprovalRequestData));
			AssertContains(msg, ErrorReporter.LastMessageReported);
			AssertContains("Exception=System.InvalidOperationException:", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();

			var approvalData = typeof(AccountingBusiness.CreditControlledDocumentsApproval).GetField("approvalData", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(approval);
			AssertNotNull(approvalData);
		}

		public void TestGetDocuments_ApprovalDataMenuItemPKIsValidButNotInDatabase()
		{
			var menuItem1 = Factory.New<StmMenuItem>();
			var bizo = Factory.New<Forwarding.IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S1";
			TestCreditControlledDocumentsApproval.Initialize((BusinessObject)bizo, menuItem1.PK);
			Factory.Save();

			menuItem1.Delete();

			AssertNoExceptionThrown(() => { var docs = TestCreditControlledDocumentsApproval.Documents; });
		}

		#region Helper Methods

		BusinessObject CreateBizObj(JobInvoicingConsumerType type)
		{
			BusinessObject bizO;
			if (type == JobInvoicingConsumerTypes.QuotedBooking)
			{
				var booking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(FreightIntegration.QuoteBookingType.QuickBooking, Factory);
				bizO = (BusinessObject)booking;
			}
			else
			{
				bizO = Factory.NewWithValidTestData(type.BizoType);
			}

			return bizO;
		}

		void SetupGatewayConsol(BusinessObject bizO)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = "AUSYD";
			port.O5_OA_AgentOfficeAddress = org.MainAddress.PK;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			org.AppointedGatewayAgentPorts.Add(port);
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;

			bizO[JobConsolSchema.JK_TransportMode.Name] = Enterprise.Core.Constants.TransportModes.Sea;
			bizO[JobConsolSchema.JK_AgentType.Name] = Enterprise.Core.Constants.AgentType.Agent;
			bizO[JobConsolSchema.JK_RL_NKLoadPort.Name] = "AUSYD";
			bizO[JobConsolSchema.JK_RL_NKDischargePort.Name] = "USLAX";
			bizO[JobConsolSchema.JK_OA_SendingForwarderAddress.Name] = org.MainAddress.PK;
			bizO[JobConsolSchema.JK_SendingForwarderHandlingType.Name] = AgentStatusList.Codes.GatewayAgent;
		}

		void SetupCurrentRequestsScenario(
			out Forwarding.IForwardingShipment shipment1,
			out Forwarding.IForwardingShipment shipment2,
			out Forwarding.IForwardingShipment shipment3)
		{
			var menuItem1 = Factory.New<StmMenuItem>();
			menuItem1.SU_MenuPath = "menu/path";
			menuItem1.SU_MenuName = "name";

			var otherCompany = Factory.New<GlbCompany>();
			var otherCompanyBranch = Factory.New<GlbBranch>();
			otherCompanyBranch.GB_GC = otherCompany.PK;
			Factory.Save();

			shipment1 = Factory.New<Forwarding.IForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S1";
			var job = new JobHeader.Loader((IJobHeaderParent)shipment1).TryCreateWithoutMutexForTestOnly();
			shipment2 = Factory.New<Forwarding.IForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S2";
			var job2 = new JobHeader.Loader((IJobHeaderParent)shipment2).TryCreateWithoutMutexForTestOnly();
			shipment3 = Factory.New<Forwarding.IForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "S3";
			var job3 = new JobHeader.Loader((IJobHeaderParent)shipment3).TryCreateWithoutMutexForTestOnly();

			// Requests that should be ignored due to being for a different company
			// or a different type.
			var request_otherType = Factory.New<GenApprovalRequest>();
			request_otherType.XP_GB_RequestingBranch = GlbBranch.CurrentBranch.PK;
			request_otherType.XP_SubSystem = Enterprise.Core.Constants.GenApprovalRequestSubSystem.Accounting;
			request_otherType.XP_ApprovalType = "QRS";
			request_otherType.XP_ApprovalStatus = Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Requested;
			request_otherType.XP_ParentID = shipment1.PK;
			request_otherType.XP_ParentTableCode = "JS";
			var request_anoterCompany = Factory.New<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_anoterCompany.Initialize((BusinessObject)shipment1, menuItem1.PK);
			request_anoterCompany.XP_GB_RequestingBranch = otherCompanyBranch.PK;
			Factory.Save();
		}

		void SpecificEmailSendingConditionsTest(ZString finalStatus)
		{
			var menuItem1 = Factory.New<StmMenuItem>();
			menuItem1.SU_MenuPath = "menu/path";
			menuItem1.SU_MenuName = "name";

			var requestingUser = Factory.NewWithValidTestData<GlbStaff>();
			requestingUser.GS_FullName = "Thomas";
			requestingUser.GS_EmailAddress = "thomas@shippingco.com";

			CreditControlledDocumentsApprovalEmailDefTest.SetupTestARCreditControlledDocumentsApprovalNotifyGroup(Factory);
			var previousEmailCount = Env.OutgoingMailManager.EmailsCreated.Count;

			var bizo = Factory.New<Forwarding.IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S1";
			TestCreditControlledDocumentsApproval.Initialize((BusinessObject)bizo, menuItem1.PK);
			TestCreditControlledDocumentsApproval.XP_SystemCreateUser = requestingUser.GS_Code;

			// Request
			AssertEquals("No emails sent until saving", 0, Env.OutgoingMailManager.EmailsCreated.Count - previousEmailCount);
			Factory.Save();
			AssertEquals("1 emails sent due to request", 1, Env.OutgoingMailManager.EmailsCreated.Count - previousEmailCount);
			var emailDef = Env.OutgoingMailManager.EmailsCreated.Last();
			AssertEquals("Email Recipient count", 2, emailDef.Recipients.Count);
			Assert("Email Recipient", emailDef.Recipients.Contains("pointyhairedguy1@shippingco.com"));
			Assert("Email Recipient", emailDef.Recipients.Contains("pointyhairedguy2@shippingco.com"));

			// Rejection
			previousEmailCount = Env.OutgoingMailManager.EmailsCreated.Count;
			TestCreditControlledDocumentsApproval.SetStatus(finalStatus, false);
			AssertEquals("No emails sent until saving", 0, Env.OutgoingMailManager.EmailsCreated.Count - previousEmailCount);
			Factory.Save();

			if (finalStatus == Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Rejected || finalStatus == Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Approved)
			{
				AssertEquals("1 emails sent due to action", 1, Env.OutgoingMailManager.EmailsCreated.Count - previousEmailCount);
				emailDef = Env.OutgoingMailManager.EmailsCreated.Last();
				AssertEquals("Email Recipient count", 1, emailDef.Recipients.Count);
				Assert("Email Recipient", emailDef.Recipients.Contains("thomas@shippingco.com"));
			}
			else
			{
				AssertEquals("Email not sent for cancellation", 0, Env.OutgoingMailManager.EmailsCreated.Count - previousEmailCount);
			}
		}

		#endregion

		protected AccountingBusiness.CreditControlledDocumentsApproval TestCreditControlledDocumentsApproval
		{
			get { return creditControlledDocumentsApproval ?? (creditControlledDocumentsApproval = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>()); }
		}
		AccountingBusiness.CreditControlledDocumentsApproval creditControlledDocumentsApproval;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
