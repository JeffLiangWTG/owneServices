using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.TransactionApproval.Testing
{
	public abstract class APInvoiceChargesApprovalBulkTest<RequestParentType> : TransactionApprovalBulkTest<APInvoiceChargesApprovalBulk, InvoicingBase, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails, RequestParentType>
		where RequestParentType : BusinessObject
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewApprovalBulk(new DefaultAccessSecurityProvider(), Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>());
		}

		protected override APInvoiceChargesApprovalBulk GetNewApprovalBulk(ISecurityOverrideProvider interactiveSecurityOverrideProvider, params APInvoiceChargesApprovalRequest[] approvalRequests)
		{
			return new APInvoiceChargesApprovalBulk(Factory, interactiveSecurityOverrideProvider, approvalRequests);
		}

		protected override void SetupSecurity(bool disallowSecondLevel = false, bool disallowTwoLevels = false, Guid? branchPK = null, Guid? departmentPK = null)
		{
			Env.Security.APInvoiceApproval_FirstApproval.IsAllowed = !disallowTwoLevels;
			Env.Security.APInvoiceApproval_SecondApproval.IsAllowed = !disallowSecondLevel && !disallowTwoLevels;

			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;

			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		protected override Mock<IPostingJobAndTransactionApprovalGUIProvider> GetIPostingJobAndTransactionApprovalGUIProvider()
		{
			var mock = new Mock<IPostingJobAndTransactionApprovalGUIProvider>();

			SetupPostingJobAndTransactionApprovalGUIProviderMock(mock);

			return mock;
		}

		protected override void PostApprovalsAndRemovePosted(APInvoiceChargesApprovalBulk approvalRequestBulk, IPostingTransactionApprovalGUIProvider guiProvider)
		{
			approvalRequestBulk.PostApprovalsAndRemovePosted((IPostingJobAndTransactionApprovalGUIProvider)guiProvider);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SecurityTestObject.CreateTestUser(true, Env.Security.APInvoiceApproval.Code, "tst", "newuser", "password");
		}
	}

	[TestedType(typeof(APInvoiceChargesApprovalBulk))]
	public class APInvoiceChargesApprovalBulk_TransactionRelatedTest : APInvoiceChargesApprovalBulkTest<InvoicingBase>
	{
		public override void TestPostApprovalsAndRemovePosted()
		{
			var aggregateInDB = Factory.Load<AccGLAggregate>(new ZQuery());
			AssertEquals("pre condition - No GL aggregation in DB", 0, aggregateInDB.Length);
			base.TestPostApprovalsAndRemovePosted();
			aggregateInDB = Factory.Load<AccGLAggregate>(new ZQuery());
			AssertEquals("post condition - Expect run GL aggregation", 12, aggregateInDB.Length);
			AssertEquals(0m, aggregateInDB.Sum(x => x.AA_Amount));
		}

		public void TestRejectSetsTransactionCount()
		{
			var invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest<APInvoice>(TestObjectCreator.Creditor1, 100);
			var approvalRequestInOtherFactory = TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoice);
			var approvalRequest = Factory.Load<APInvoiceChargesApprovalRequest>(approvalRequestInOtherFactory.PK);

			var mockSecurityProvider = new Mock<SecurityOverrideProvider>();
			mockSecurityProvider.CallBase = true;
			var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest);
			Assert("Precondition: IsCancelled", !invoice.AH_IsCancelled);
			byte expectedTransactionCount = 3;
			AssertNotEquals("Precondition: TransactionCount", expectedTransactionCount, invoice.AH_TransactionCount);
			approvalRequestBulk.Reject();
			Factory.Save();
			AssertEquals("approvalRequest1.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Rejected, approvalRequest.XP_ApprovalStatus);
			Assert("IsCancelled", invoice.AH_IsCancelled);
			AssertEquals("TransactionCount", expectedTransactionCount, invoice.AH_TransactionCount);
		}

		public void TestCancelSetsTransactionCount()
		{
			var invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest<APInvoice>(TestObjectCreator.Creditor1, 100);
			var approvalRequestInOtherFactory = TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoice);
			var approvalRequest = Factory.Load<APInvoiceChargesApprovalRequest>(approvalRequestInOtherFactory.PK);

			var mockSecurityProvider = new Mock<SecurityOverrideProvider>();
			var approvalRequestBulk = GetNewApprovalBulk(mockSecurityProvider.Object, approvalRequest);
			approvalRequestBulk.Cancel();
			Factory.Save();
			AssertEquals("approvalRequest1.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest.XP_ApprovalStatus);
			Assert("IsCancelled", invoice.AH_IsCancelled);
			AssertEquals("TransactionCount", (byte)3, invoice.AH_TransactionCount);
		}

		protected override APInvoiceChargesApprovalRequest GetNewApprovalRequest()
		{
			var newFactoryTestObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			var invoice = newFactoryTestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: TestObjectCreator.Creditor1);
			invoice.FillWithValidTestData();
			var line = newFactoryTestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.ExchangeGainLossControlAccount.PK, 10);
			line.PeriodApportionmentMethod = "PER";
			line.PeriodStartDate = ZDate.Today;
			line.PeriodEndDate = ZDate.Today.AddMonths(1);
			line.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;

			var request = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			request.InitializeInvoiceRelated(invoice);
			request.PrepareFoSaving();
			Factory.Save();

			return request;
		}

		protected override InvoicingBase GetParentFromApprovalRequest(APInvoiceChargesApprovalRequest request)
		{
			InvoicingBase.ShowErrorHandler oninvoiceLoadingErrorHandler = (message, caption) => { };

			return request.GetLinkedInvoice().Invoice;
		}

		protected override void ModifyParentToHaveValidationError(InvoicingBase parent)
		{
			var newFactory = new BusinessObjectFactory();
			var glAccount = newFactory.Load<AccGLHeader>(parent.Lines[0].AL_AG);
			glAccount.AG_IsActive = false;
			newFactory.Save();
		}

		protected override string GetErrorMessageForParentWithValidationError()
		{
			return "Generic Charge: Enter a valid selection.";
		}

		protected override void AssertParentIsPosted(string message, InvoicingBase parent, bool isPosted)
		{
			AssertEquals(message, isPosted, parent.IsPosted);
		}

		protected override void SetupPostingJobAndTransactionApprovalGUIProviderMock(Mock<IPostingJobAndTransactionApprovalGUIProvider> mock, params APInvoiceChargesApprovalRequest[] requestsWhichWillBePosted)
		{
			base.SetupPostingJobAndTransactionApprovalGUIProviderMock(mock);

			mock.Setup(m => m.BulkPrintAPInvoicesAndCreditNotes(It.IsAny<ZGuid[]>()));
			mock.Setup(m => m.PrintAPInvoiceAndCreditNote(It.IsAny<ZGuid>()));
			if (requestsWhichWillBePosted.Length > 1)
			{
				mock.Setup(m => m.BulkPrintAPInvoicesAndCreditNotes(requestsWhichWillBePosted.Select(x => x.XP_ParentID).ToArray()));
			}
			else
			{
				mock.Setup(m => m.PrintAPInvoiceAndCreditNote(requestsWhichWillBePosted.Select(x => x.XP_ParentID).FirstOrDefault()));
			}
		}

		protected override ZString ParentName => "transaction";
	}

	public abstract class APInvoiceChargesApprovalBulk_NonTransactionRelatedTest<RequestParentType> : APInvoiceChargesApprovalBulkTest<RequestParentType>
		where RequestParentType : BusinessObject
	{
		public override void TestGUIProviderOfPostApprovalsAndRemovePosted()
		{
			Assert("Can't be tested as level authorization code is in GUI and a only mock is used here.", true);
		}

		public override void TestTwiceClickingPostApprovalsAndRemovePosted()
		{
			Assert("Can't be tested as level authorization code is in GUI and a only mock is used here.", true);
		}

		protected override void SetupPostingJobAndTransactionApprovalGUIProviderMock(Mock<IPostingJobAndTransactionApprovalGUIProvider> mock, params APInvoiceChargesApprovalRequest[] requestsWhichWillBePosted)
		{
			base.SetupPostingJobAndTransactionApprovalGUIProviderMock(mock);

			mock.Setup(m => m.PostRequestInBulk(It.IsAny<APInvoiceChargesApprovalRequest>(), It.IsAny<IBulkPostingCache>()));
			mock.Setup(m => m.PostRequest(It.IsAny<APInvoiceChargesApprovalRequest>()));

			if (requestsWhichWillBePosted.Length > 1)
			{
				mock.Setup(m => m.PostRequestInBulk(It.IsAny<APInvoiceChargesApprovalRequest>(), It.IsAny<IBulkPostingCache>()))
					.Returns<APInvoiceChargesApprovalRequest, IBulkPostingCache>((arg1, arg2) =>
					{
						var methodArguments = new object[2] { arg1, arg2 };
						return (IApprovalRequestPostingResult)PostRequestInBulkMethodMock(methodArguments);
					});
			}
			else
			{
				mock.Setup(m => m.PostRequest(It.IsAny<APInvoiceChargesApprovalRequest>()))
					.Returns<APInvoiceChargesApprovalRequest>((arg) =>
					{
						var methodArguments = new object[1] { arg };
						return (string)PostRequestMethodMock(methodArguments);
					});
			}
		}

		protected abstract object PostRequestInBulkMethodMock(object[] methodArguments);

		protected abstract object PostRequestMethodMock(object[] methodArguments);
	}

	[TestedType(typeof(APInvoiceChargesApprovalBulk))]
	public class APInvoiceChargesApprovalBulk_JobRelatedTest : APInvoiceChargesApprovalBulk_NonTransactionRelatedTest<Job>
	{
		protected override APInvoiceChargesApprovalRequest GetNewApprovalRequest()
		{
			var newFactory = new BusinessObjectFactory();
			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(4), true);
			var job = TestObjectCreator.CreateJob(shipment, false, false, localClientOrg: TestObjectCreator.LocalClient, newFactory: newFactory);
			var charge = TestObjectCreator.CreateCharge(job, creditor: TestObjectCreator.Creditor1, invoiceNum: TestObjectCreator.GetRandomString(5));
			newFactory.Save();

			var apInvoiceCharges = new APInvoiceCharges(charge.CostAccount.OH_Code, charge.JR_APInvoiceNum, job.PK, job.TablePrefix, null);
			apInvoiceCharges.Charges.Add(charge);
			var request = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			request.InitializeJobRelated(apInvoiceCharges, job.PK, job.TablePrefix);

			return request;
		}

		protected override Job GetParentFromApprovalRequest(APInvoiceChargesApprovalRequest request)
		{
			return request.Factory.Load<Job>(request.XP_ParentID);
		}

		protected override void ModifyParentToHaveValidationError(Job parent)
		{
			var newFactory = new BusinessObjectFactory();
			var charge = newFactory.Load<Charge>(parent.Charges[0].PK);
			charge.JR_Desc = "";
			newFactory.Save();
		}

		protected override string GetErrorMessageForParentWithValidationError()
		{
			return "Error - JR_Desc: Description cannot be empty.";
		}

		protected override void AssertParentIsPosted(string message, Job parent, bool isPosted)
		{
			AssertEquals(message, isPosted, parent.Charges[0].JR_IsCostPosted);
		}

		protected override object PostRequestInBulkMethodMock(object[] methodArguments)
		{
			AssertEquals(2, methodArguments.Length);
			var request = (APInvoiceChargesApprovalRequest)methodArguments[0];
			var postingCache = (IBulkPostingCache)methodArguments[1];
			var errorMessage = PostRequestForMock(request);

			var postingResultMock = new Mock<IApprovalRequestPostingResult>();
			postingResultMock.Setup(m => m.ErrorMessage).Returns(errorMessage);
			if (postingCache == null)
			{
				var bulkPostinCacheMock = new Mock<IBulkPostingCache>();
				bulkPostinCacheMock.Setup(m => m.FinalizePosting(It.IsAny<IEnumerable<ZGuid>>()));
				postingResultMock.Setup(m => m.BulkPostingCache).Returns(bulkPostinCacheMock.Object);
			}
			else
			{
				postingResultMock.Setup(m => m.BulkPostingCache).Returns(postingCache);
			}

			return postingResultMock.Object;
		}

		protected override object PostRequestMethodMock(object[] methodArguments)
		{
			AssertEquals(1, methodArguments.Length);
			var request = (APInvoiceChargesApprovalRequest)methodArguments[0];

			return PostRequestForMock(request);
		}

		string PostRequestForMock(APInvoiceChargesApprovalRequest request)
		{
			var job = GetParentFromApprovalRequest(request);
			AssertEquals(1, job.Charges.Count);
			job.MarkAsNeedingValidationIncludingChildren();
			job.RunPreSaveValidation();
			string errorMessage = "";
			if (job.HasErrors)
			{
				errorMessage = new ZStringBuilder(job.NotificationsIncludingChildren.GetErrors().GetUniqueMessageList()).ToStringWithNewLineBetweenAppends();
			}
			else
			{
				request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;

				var localTestObjectCreator = new TestObjectCreator(request.Factory);
				var chargeToPost = job.Charges[0];
				var invoice = (APInvoice)localTestObjectCreator.CreateInvoice(typeof(APInvoice), chargeToPost.JR_APInvoiceNum, organisation: chargeToPost.CostAccount);
				chargeToPost.CreateCostTransactionLine(invoice, invoice.AH_PostDate);
				request.Factory.Save();
			}

			return errorMessage;
		}

		protected override ZString ParentName => "job";
	}

	[TestedType(typeof(APInvoiceChargesApprovalBulk))]
	public class APInvoiceChargesApprovalBulk_ConsolRelatedTest : APInvoiceChargesApprovalBulk_NonTransactionRelatedTest<ForwardingConsol>
	{
		protected override APInvoiceChargesApprovalRequest GetNewApprovalRequest()
		{
			var newFactory = new BusinessObjectFactory();
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", TestObjectCreator.GetRandomString(4));
			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(4), consol: consol, saveIt: true);
			var job = TestObjectCreator.CreateJob(shipment, false, false, localClientOrg: TestObjectCreator.LocalClient, newFactory: newFactory);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, TestObjectCreator.Creditor1, apportionmentListing: new ApportionmentListing(newFactory, consol));
			consolCost.E6_InvoiceNum = TestObjectCreator.GetRandomString(5);
			consolCost.E6_InvoiceDate = ZDateTime.Now;
			consolCost.E6_PaymentDate = ZDateTime.Now;
			newFactory.Save();

			var apInvoiceCharges = new APInvoiceCharges(consolCost.Creditor.OH_Code, consolCost.E6_InvoiceNum, ZGuid.Empty, "", null);
			apInvoiceCharges.Charges.AddRange(consolCost.Factory.Load<Charge>(new ZQuery(JobChargeSchema.PK, consolCost.ApportionmentCharges.GetPKs())));
			var request = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			request.InitializeJobRelated(apInvoiceCharges, consol.PK, consol.TablePrefix);

			return request;
		}

		protected override ForwardingConsol GetParentFromApprovalRequest(APInvoiceChargesApprovalRequest request)
		{
			return request.Factory.Load<ForwardingConsol>(request.XP_ParentID);
		}

		protected override void ModifyParentToHaveValidationError(ForwardingConsol parent)
		{
			var newFactory = new BusinessObjectFactory();
			var consolCost = new ApportionmentListing(newFactory, parent).CostsCollection[0];
			consolCost.E6_ApportionmentMethod = "";
			newFactory.Save();
		}

		protected override string GetErrorMessageForParentWithValidationError()
		{
			return "Error - E6_ApportionmentMethod: Please enter an App. Method.";
		}

		protected override void AssertParentIsPosted(string message, ForwardingConsol parent, bool isPosted)
		{
			var newFactory = new BusinessObjectFactory();
			var consolCost = new ApportionmentListing(newFactory, parent).CostsCollection[0];
			AssertEquals(message, isPosted, consolCost.IsPosted);
		}

		protected override object PostRequestInBulkMethodMock(object[] methodArguments)
		{
			AssertEquals(2, methodArguments.Length);
			var request = (APInvoiceChargesApprovalRequest)methodArguments[0];
			var postingCache = (IBulkPostingCache)methodArguments[1];
			string errorMessage = PostRequestForMock(request);

			var postingResultMock = new Mock<IApprovalRequestPostingResult>();
			postingResultMock.Setup(m => m.ErrorMessage).Returns(errorMessage);
			if (postingCache == null)
			{
				var bulkPostinCacheMock = new Mock<IBulkPostingCache>();
				bulkPostinCacheMock.Setup(m => m.FinalizePosting(It.IsAny<IEnumerable<ZGuid>>()));
				postingResultMock.Setup(m => m.BulkPostingCache).Returns(bulkPostinCacheMock.Object);
			}
			else
			{
				postingResultMock.Setup(m => m.BulkPostingCache).Returns(postingCache);
			}

			return postingResultMock.Object;
		}

		protected override object PostRequestMethodMock(object[] methodArguments)
		{
			AssertEquals(1, methodArguments.Length);
			var request = (APInvoiceChargesApprovalRequest)methodArguments[0];

			return PostRequestForMock(request);
		}

		string PostRequestForMock(APInvoiceChargesApprovalRequest request)
		{
			var consol = GetParentFromApprovalRequest(request);
			var consolCost = new ApportionmentListing(request.Factory, consol).CostsCollection[0];
			AssertEquals(1, consolCost.ApportionmentCharges.Count);
			consolCost.MarkAsNeedingValidationIncludingChildren();
			consolCost.RunPreSaveValidation();
			string errorMessage = "";
			if (consolCost.HasErrors)
			{
				errorMessage = new ZStringBuilder(consolCost.NotificationsIncludingChildren.GetErrors().GetUniqueMessageList()).ToStringWithNewLineBetweenAppends();
			}
			else
			{
				request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;

				var localTestObjectCreator = new TestObjectCreator(request.Factory);
				var chargeToPost = request.Factory.Load<Charge>(consolCost.ApportionmentCharges[0].PK);
				var invoice = (APInvoice)localTestObjectCreator.CreateInvoice(typeof(APInvoice), consolCost.E6_InvoiceNum, organisation: consolCost.Creditor);
				chargeToPost.CreateCostTransactionLine(invoice, invoice.AH_PostDate);
				consolCost.E6_AH_APInvoice = invoice.PK;
				request.Factory.Save();
			}

			return errorMessage;
		}

		protected override ZString ParentName => "consol";
	}
}
