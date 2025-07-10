using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	public partial class GLJournal_InnerTest
	{
		protected override ZDecimal GetExpectedOutstandindAmount(ZDecimal expectedValue) => 0;

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRelinkEDocsFromRequestToNewJournal()
		{
			var times = Enumerable.Range(0, 6).Select(index => ZDateTime.Now.AddDays(index)).ToArray();
			var users = Enumerable.Range(0, 6).Select(index => Factory.NewWithValidTestData<GlbStaff>()).ToArray();
			Factory.Save();

			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
			var storageMainOnJournal = journal.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(journal, Core.Constants.DocManagerCodes.GLJournal) as StorageMain;

			var lastPostedRequest = AddApprovalRequestToJournal(journal, users[0].GS_Code, users[1].GS_Code, times[0], times[1], Core.Constants.GenApprovalRequestApprovalStatus.Posted, true);
			var storageMainOnRequest = lastPostedRequest.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(lastPostedRequest, Core.Constants.DocManagerCodes.GLJournalApprovalRequest) as StorageMain;
			byte[] tIFfileContents = DocumentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.tif");
			StorageDocs document = (StorageDocs)storageMainOnRequest.AddFileOrDocument(tIFfileContents, new AddFileOrDocumentDto
			{
				FileName = "small.tif",
				DocumentType = Core.Constants.RefDocTypes.MiscellaneousDocument,
			});
			var requestFactory = lastPostedRequest.Factory;
			requestFactory.Save();

			AssertEquals("Storage docs count", 0, storageMainOnJournal.eDocs.Count);
			AssertEquals("Storage docs count", 1, storageMainOnRequest.eDocs.Count);
			journal.RelinkEDocsFromRequestToNewJournal(lastPostedRequest);
			Factory.Save();
			AssertNullOrEmpty("No exceptions should be reported", ErrorReporter.LastMessageReported);
			var eDoc = journal.DocManagerInfo.MasterFactory.FindEDocsFromAllSDDatabasesByPK(document.PK);
			AssertEquals("Doc Name", "small.tif", storageMainOnJournal.eDocs[0].GetFileNameOnlyWithExtension());
			AssertEquals("Doc Type", Core.Constants.RefDocTypes.MiscellaneousDocument, storageMainOnJournal.eDocs[0].DocType.RT_DocType);
			AssertEquals("Storage docs count", 0, storageMainOnRequest.eDocs.Count);
			Assert("Old document is deleted as new document is created and contents are copied over", document.IsDeleted);
		}

		public void TestLastPostedApprovalChange()
		{
			var times = Enumerable.Range(0, 6).Select(index => ZDateTime.Now.AddDays(index)).ToArray();
			var users = Enumerable.Range(0, 6).Select(index => Factory.NewWithValidTestData<GlbStaff>()).ToArray();
			Factory.Save();

			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);

			var messagePrefix = "Before journal saving: ";
			AssertEquals(messagePrefix + "LastPostedRequest_RequestedTime", ZDateTime.Empty, journal.LastPostedRequest_RequestedTime);
			AssertEquals(messagePrefix + "LastPostedRequest_RequesterFullName", ZString.Empty, journal.LastPostedRequest_RequesterFullName);
			AssertEquals(messagePrefix + "LastPostedRequest_ApprovedTime", ZDateTime.Empty, journal.LastPostedRequest_ApprovedTime);
			AssertEquals(messagePrefix + "LastPostedRequest_ApproverFullName", ZString.Empty, journal.LastPostedRequest_ApproverFullName);

			var lastPostedRequest = AddApprovalRequestToJournal(journal, users[0].GS_Code, users[1].GS_Code, times[0], times[1], Core.Constants.GenApprovalRequestApprovalStatus.Posted);
			Factory.Save();

			messagePrefix = "After journal saving: ";
			AssertEquals(messagePrefix + "LastPostedRequest_RequestedTime", times[0].ToLocalBranchTime(), journal.LastPostedRequest_RequestedTime);
			AssertEquals(messagePrefix + "LastPostedRequest_RequesterFullName", users[0].GS_FullName, journal.LastPostedRequest_RequesterFullName);
			AssertEquals(messagePrefix + "LastPostedRequest_ApprovedTime", times[1], journal.LastPostedRequest_ApprovedTime);
			AssertEquals(messagePrefix + "LastPostedRequest_ApproverFullName", users[1].GS_FullName, journal.LastPostedRequest_ApproverFullName);

			var factory2 = new BusinessObjectFactory();
			var journalInFactory2 = factory2.Load<GLJournal>(journal.PK);
			((GLJournalLine)journalInFactory2.Lines[0]).UnsignedOSLineAmount = 20.00m;
			((GLJournalLine)journalInFactory2.Lines[0]).DebitCreditSign = nameof(DebitCredit.DR);
			((GLJournalLine)journalInFactory2.Lines[1]).UnsignedOSLineAmount = 20.00m;
			((GLJournalLine)journalInFactory2.Lines[1]).DebitCreditSign = nameof(DebitCredit.CR);
			var lastPostedRequest2 = AddApprovalRequestToJournal(journalInFactory2, users[2].GS_Code, users[3].GS_Code, times[2], times[3], Core.Constants.GenApprovalRequestApprovalStatus.Posted);
			factory2.Save();

			messagePrefix = "After journalInFactory2 saving: ";
			var expectedLastPostedRequest_RequestedTime = times[2].ToLocalBranchTime();
			var expectedLastPostedRequest_RequesterFullName = users[2].GS_FullName;
			var expectedLastPostedRequest_ApprovedTime = times[3];
			var expectedLastPostedRequest_ApproverFullName = users[3].GS_FullName;
			AssertEquals(messagePrefix + "LastPostedRequest_RequestedTime", expectedLastPostedRequest_RequestedTime, journal.LastPostedRequest_RequestedTime);
			AssertEquals(messagePrefix + "LastPostedRequest_RequesterFullName", expectedLastPostedRequest_RequesterFullName, journal.LastPostedRequest_RequesterFullName);
			AssertEquals(messagePrefix + "LastPostedRequest_ApprovedTime", expectedLastPostedRequest_ApprovedTime, journal.LastPostedRequest_ApprovedTime);
			AssertEquals(messagePrefix + "LastPostedRequest_ApproverFullName", expectedLastPostedRequest_ApproverFullName, journal.LastPostedRequest_ApproverFullName);

			var factory3 = new BusinessObjectFactory();
			factory3.RefreshEnabled = false;
			var journalInFactory3 = factory3.Load<GLJournal>(journal.PK);
			((GLJournalLine)journalInFactory3.Lines[0]).UnsignedOSLineAmount = 30.00m;
			((GLJournalLine)journalInFactory3.Lines[0]).DebitCreditSign = nameof(DebitCredit.DR);
			((GLJournalLine)journalInFactory3.Lines[1]).UnsignedOSLineAmount = 30.00m;
			((GLJournalLine)journalInFactory3.Lines[1]).DebitCreditSign = nameof(DebitCredit.CR);
			var lastPostedRequest3 = AddApprovalRequestToJournal(journalInFactory3, users[4].GS_Code, users[5].GS_Code, times[4], times[5], Core.Constants.GenApprovalRequestApprovalStatus.Posted);
			factory3.Save();

			messagePrefix = "After journalInFactory3 saving: ";
			AssertEquals(messagePrefix + "LastPostedRequest_RequestedTime", expectedLastPostedRequest_RequestedTime, journal.LastPostedRequest_RequestedTime);
			AssertEquals(messagePrefix + "LastPostedRequest_RequesterFullName", expectedLastPostedRequest_RequesterFullName, journal.LastPostedRequest_RequesterFullName);
			AssertEquals(messagePrefix + "LastPostedRequest_ApprovedTime", expectedLastPostedRequest_ApprovedTime, journal.LastPostedRequest_ApprovedTime);
			AssertEquals(messagePrefix + "LastPostedRequest_ApproverFullName", expectedLastPostedRequest_ApproverFullName, journal.LastPostedRequest_ApproverFullName);
		}

		public void TestNullForLastAndOriginalApprovalAndRelatedProperties()
		{
			var journal = Factory.NewWithValidTestData<GLJournal>();

			AssertNull(journal.OriginalRequest_ForTestOnly);
			AssertNull(journal.LastRequest_ForTestOnly);

			AssertEquals(ZString.Empty, journal.LastPostedRequest_ApproverFullName);
			AssertEquals(ZString.Empty, journal.LastRequest_RequesterFullName);
			AssertEquals(ZString.Empty, journal.LastPostedRequest_RequesterFullName);
			AssertEquals(ZString.Empty, journal.OriginalPostedRequest_ApproverFullName);
			AssertEquals(ZString.Empty, journal.OriginalRequest_RequesterFullName);
			AssertEquals(ZDateTime.Empty, journal.LastPostedRequest_RequestedTime);
			AssertEquals(ZDateTime.Empty, journal.LastPostedRequest_ApprovedTime);

			Factory.Save();

			var users = Factory.Load<GlbStaff>(new ZQuery() { MaximumRows = 3 });
			var firstUser = users[0];
			var secondUser = users[1];
			var thirdUser = users[2];

			var currentUser = Enterprise.ZArchitecture.Environment.EnvProxy.Instance.CurrentUser;

			var timesCount = 14;

			var times = new ZDateTime[timesCount];
			for (var index = 0; index < timesCount; index++)
			{
				times[index] = ZDateTime.Now.AddDays(index);
			}

			var approvalRequests = new System.Collections.Generic.List<GLJournalApprovalRequest>();

			var secondFactory = new BusinessObjectFactory();
			var journalInSecondFactory = secondFactory.Load<GLJournal>(journal.PK);

			approvalRequests.Add(AddApprovalRequestToJournal(journalInSecondFactory, firstUser.GS_Code, secondUser.GS_Code, times[0], times[6], Core.Constants.GenApprovalRequestApprovalStatus.Cancelled));
			approvalRequests.Add(AddApprovalRequestToJournal(journalInSecondFactory, currentUser.Initials, secondUser.GS_Code, times[1], times[7], Core.Constants.GenApprovalRequestApprovalStatus.Requested));
			approvalRequests.Add(AddApprovalRequestToJournal(journalInSecondFactory, secondUser.GS_Code, firstUser.GS_Code, times[2], times[8], Core.Constants.GenApprovalRequestApprovalStatus.Posted));
			approvalRequests.Add(AddApprovalRequestToJournal(journalInSecondFactory, thirdUser.GS_Code, thirdUser.GS_Code, times[3], times[9], Core.Constants.GenApprovalRequestApprovalStatus.Posted));
			approvalRequests.Add(AddApprovalRequestToJournal(journalInSecondFactory, secondUser.GS_Code, firstUser.GS_Code, times[4], times[10], Core.Constants.GenApprovalRequestApprovalStatus.Requested));
			approvalRequests.Add(AddApprovalRequestToJournal(journalInSecondFactory, thirdUser.GS_Code, secondUser.GS_Code, times[5], times[11], Core.Constants.GenApprovalRequestApprovalStatus.Cancelled));

			journalInSecondFactory.AH_Desc += "1";
			secondFactory.Save();

			AssertEquals(thirdUser.GS_FullName, journal.LastPostedRequest_ApproverFullName);
			AssertEquals(firstUser.GS_FullName, journal.OriginalPostedRequest_ApproverFullName);
			AssertEquals(thirdUser.GS_FullName, journal.LastPostedRequest_RequesterFullName);

			AssertEquals(currentUser.FullName, journal.OriginalRequest_RequesterFullName);
			AssertEquals(secondUser.GS_FullName, journal.LastRequest_RequesterFullName);

			AssertEquals(times[3].ToLocalBranchTime(), journal.LastPostedRequest_RequestedTime);
			AssertEquals(times[9], journal.LastPostedRequest_ApprovedTime);

			approvalRequests.Add(AddApprovalRequestToJournal(journal, thirdUser.GS_Code, thirdUser.GS_Code, times[12], times[13], Core.Constants.GenApprovalRequestApprovalStatus.Posted));
			journalInSecondFactory.AH_Desc += "1";
			secondFactory.Save();

			AssertEquals(times[12].ToLocalBranchTime(), journal.LastPostedRequest_RequestedTime);
			AssertEquals(times[13], journal.LastPostedRequest_ApprovedTime);
		}

		public void TestCompleteFilter()
		{
			var query1 = new ZQuery(new GLJournalApprovalRequestCollection(Factory).CompleteFilter);
			var query2 = new ZQuery(new GLJournalApprovalRequestCollection(Factory).CompleteFilter);

			AssertEquals(query1.LiteralTextSqlFormatted, query2.LiteralTextSqlFormatted);

			query1.AddToFilter(GenApprovalRequestSchema.XP_ApprovalStatus, SQLComparisonOperator.NotEqual, Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Cancelled);

			AssertNotEquals(query1.LiteralTextSqlFormatted, query2.LiteralTextSqlFormatted);
		}

		GLJournalApprovalRequest AddApprovalRequestToJournal(GLJournal journal, ZString creater, ZString approver, ZDateTime createdTime, ZDateTime approvedTime, ZString status, bool createInNewFactory = false)
		{
			var factoryToUse = createInNewFactory ? new BusinessObjectFactory() : journal.Factory;
			var request = factoryToUse.New<GLJournalApprovalRequest>();
			request.XP_SystemCreateUser = creater;
			request.XP_GS_NKApprovingUser1 = approver;
			request.XP_SystemCreateTimeUtc = createdTime;
			request.XP_ApprovalDate = approvedTime;
			request.XP_ApprovalStatus = status;
			request.Initialize(journal);
			request.PrepareToPost(journal.PK, "");

			return request;
		}
	}
}
