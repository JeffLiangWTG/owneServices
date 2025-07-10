using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business.Testing
{
	public class IncomingInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestExecute()
		{
			var otherBranch = CreateBranchAndStaff();

			var interchange1 = CreateAndSaveInterchange(Factory, InboundInterchangeProcessorForTesting.AppCode, "SND", "RCV", "header1", "throwexception", "");
			AssertEquals("pre-condition", EDIInterchange.Status.Queued, interchange1.EI_Status);

			var interchange2 = CreateAndSaveInterchange(Factory, InboundInterchangeProcessorForTesting.AppCode, "SND", "RCV", "header2", "body2", "");
			AssertEquals(EDIInterchange.Status.Queued, interchange2.EI_Status);

			var interchangeOfOtherBranch = CreateAndSaveInterchange(Factory, InboundInterchangeProcessorForTesting.AppCode, "SND", "RCV", "header3", "body3", "");
			interchangeOfOtherBranch.EI_GB = otherBranch.PK;

			var interchangeInError = CreateAndSaveInterchange(Factory, InboundInterchangeProcessorForTesting.AppCode, "SND", "RCV", "header4", "setInError", "");
			AssertEquals(EDIInterchange.Status.Queued, interchangeInError.EI_Status);

			Factory.Save();

			AssertEquals(EDIInterchange.Status.Queued, interchangeOfOtherBranch.EI_Status);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var logger = new LoggingInformation();
			var processor = new InboundInterchangeProcessorForTesting(logger);
			processor.ExecuteBatch();

			AssertEquals(EDIInterchange.Status.Queued, interchange1.EI_Status);
			interchange1.Reload();
			AssertEquals(EDIInterchange.Status.Failed, interchange1.EI_Status);

			AssertEquals(EDIInterchange.Status.Queued, interchange2.EI_Status);
			interchange2.Reload();
			AssertEquals(EDIInterchange.Status.Received, interchange2.EI_Status);

			AssertEquals(EDIInterchange.Status.Queued, interchangeInError.EI_Status);
			interchangeInError.Reload();
			AssertEquals(EDIInterchange.Status.Failed, interchangeInError.EI_Status);

			var ediMessageQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, InboundInterchangeProcessorForTesting.AppCode);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageNum, "1");
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			ediMessageQuery.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));

			var msgs = Factory.Load<EDIMessage>(ediMessageQuery);
			AssertEquals(1, msgs.Length);
			var msg = msgs[0];

			AssertEquals("EM_MessageText", "body2", msg.EM_MessageText);

			interchangeOfOtherBranch.Reload();
			AssertEquals("This interchange should not be processed as it doesn't belong to the current branch", EDIInterchange.Status.Queued, interchangeOfOtherBranch.EI_Status);
		}

		public void TestReasonForCannotProcessInterchange()
		{
			var otherBranch = CreateBranchAndStaff();

			var interchange1 = CreateAndSaveInterchange(Factory, InboundInterchangeProcessorWithPrerequisiteReceivingConditionsNotMet.AppCode, "SND", "RCV", "header1", "throwexception", "");
			AssertEquals("pre-condition", EDIInterchange.Status.Queued, interchange1.EI_Status);

			var interchange2 = CreateAndSaveInterchange(Factory, InboundInterchangeProcessorWithPrerequisiteReceivingConditionsNotMet.AppCode, "SND", "RCV", "header2", "body2", "");
			AssertEquals(EDIInterchange.Status.Queued, interchange2.EI_Status);

			var interchangeOfOtherBranch = CreateAndSaveInterchange(Factory, InboundInterchangeProcessorWithPrerequisiteReceivingConditionsNotMet.AppCode, "SND", "RCV", "header3", "body3", "");
			interchangeOfOtherBranch.EI_GB = otherBranch.PK;
			Factory.Save();

			AssertEquals(EDIInterchange.Status.Queued, interchangeOfOtherBranch.EI_Status);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var logger = new LoggingInformation();
			var processor = new InboundInterchangeProcessorWithPrerequisiteReceivingConditionsNotMet(logger);
			processor.ExecuteBatch();

			AssertEquals("Two logs should be created", 2, logger.UserLogStrings.Count);
			AssertContains("Log message", "Interchange '1' has not been processed for company 'Eagle Datamation International': Some Reason", logger.UserLogStrings[0]);
			AssertContains("Log message", "Interchange '2' has not been processed for company 'Eagle Datamation International': Some Reason", logger.UserLogStrings[1]);

			AssertEquals(EDIInterchange.Status.Queued, interchange1.EI_Status);
			interchange1.Reload();
			AssertEquals("CAN", interchange1.EI_Status);

			AssertEquals(EDIInterchange.Status.Queued, interchange2.EI_Status);
			interchange2.Reload();
			AssertEquals("CAN", interchange2.EI_Status);

			var ediMessageQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, InboundInterchangeProcessorForTesting.AppCode);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageNum, "1");
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			ediMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			ediMessageQuery.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));

			var msgs = Factory.Load<EDIMessage>(ediMessageQuery);
			AssertEquals("No messages should be created.", 0, msgs.Length);

			interchangeOfOtherBranch.Reload();
			AssertEquals("This interchange should not be processed as it doesn't belong to the current branch", EDIInterchange.Status.Queued, interchangeOfOtherBranch.EI_Status);

			var interchange3 = CreateAndSaveInterchange(Factory, InboundInterchangeProcessorWithPrerequisiteReceivingConditionsNotMet.AppCode, "SND", "RCV", "header3", "body3", "");
			AssertEquals(EDIInterchange.Status.Queued, interchange3.EI_Status);
			Factory.Save();

			processor.reason = ZString.Empty;
			processor.ExecuteBatch();
			AssertEquals("One log should be created", 1, logger.UserLogStrings.Count);
			AssertContains("Log message", "Interchange '4' has been processed successfully.", logger.UserLogStrings[0]);

			AssertEquals(EDIInterchange.Status.Queued, interchange3.EI_Status);
			interchange3.Reload();
			AssertEquals("RCV", interchange3.EI_Status);
		}

		public void TestSupportEnvironmentSwitching()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "K#@";
			company.GC_Name = "TEST COMP";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "B$#";
			branch.GB_BranchName = "BKD NAME";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();

			var interchange1 = CreateAndSaveInterchange(Factory, InboundInterchangeProcessorWithEnvironmentSwitchForTesting.AppCode, "SND", "RCV", "header1", "body1", "footer1");
			interchange1.EI_GB = branch.PK;
			var interchange2 = CreateAndSaveInterchange(Factory, InboundInterchangeProcessorWithEnvironmentSwitchForTesting.AppCode, "SND", "RCV", "header2", "body2", "footer2");
			interchange2.EI_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var logger = new LoggingInformation();
			var processor = new InboundInterchangeProcessorWithEnvironmentSwitchForTesting(logger);
			processor.ExecuteBatch();

			interchange1.Reload();
			AssertEquals(EDIInterchange.Status.Received, interchange1.EI_Status);
			interchange2.Reload();
			AssertEquals(EDIInterchange.Status.Received, interchange2.EI_Status);
		}

		public void TestProcessFailure()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TT";
			staff.GS_EmailAddress = "test@edi.com.au";

			var group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			group.Staff.Add(staff);

			var interchange = CreateAndSaveInterchange(Factory, InboundInterchangeProcessorForTesting.AppCode, "SND", "RCV", "header1", "throwexception", "");
			interchange.EI_InterchangeType = "BOB";
			AssertEquals("pre-condition", EDIInterchange.Status.Queued, interchange.EI_Status);
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var logger = new LoggingInformation();
			var processor = new InboundInterchangeProcessorForTesting2(logger);
			processor.ExecuteBatch();

			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			interchange.Reload();
			AssertEquals(EDIInterchange.Status.Failed, interchange.EI_Status);
		}

		public void TestExecuteBatchProcessTillThereIsNoMore()
		{
			CreateAndSaveInterchange(Factory, InboundInterchangeProcessorForTestingProcessMore.AppCode, "SND", "RCV", "header", "data", "");
			var logger = new LoggingInformation();
			var processor = new InboundInterchangeProcessorForTestingProcessMore(logger);
			processor.ExecuteBatch();
			var factory = new BusinessObjectFactory();
			var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, InboundInterchangeProcessorForTestingProcessMore.AppCode);
			query.AddToFilter(EDIInterchangeSchema.EI_From, "SND");
			query.AddToFilter(EDIInterchangeSchema.EI_To, "RCV");
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, "RCV");
			var interchanges = factory.Load<EDIInterchange>(query);
			AssertEquals(3, interchanges.Length);
			foreach (var interchange in interchanges)
			{
				AssertEquals("interchange.EI_Status", EDIInterchange.Status.Received, interchange.EI_Status);
				AssertEquals("interchange.ContainedMessages.Count", 1, interchange.ContainedMessages.Count);
			}
		}

		public void TestValidBranchesForFilter()
		{
			var branch = CreateBranchAndStaff();

			using (branch.SetAsTemporaryContext())
			{
				var interchangeInCurrentBranch = CreateAndSaveInterchange(Factory, InboundInterchangeProcessorForTesting.AppCode, "SND", "RCV", "header1", "body1", "");
				interchangeInCurrentBranch.EI_GB = branch.PK;
				Factory.Save();

				var logger = new LoggingInformation();
				var processor = new InboundInterchangeProcessorForTesting(logger);
				processor.ExecuteBatch();

				interchangeInCurrentBranch.Reload();
				AssertEquals("Should process the interchange as the branch is created at before.", EDIInterchange.Status.Received, interchangeInCurrentBranch.EI_Status);

				var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };

				var companyInAnotherFactory = anotherFactory.Load<GlbCompany>(branch.GB_GC);
				var newBranchInAnotherFactory = companyInAnotherFactory.Branches.AddNew();
				newBranchInAnotherFactory.FillWithValidTestData();
				newBranchInAnotherFactory.GB_Code = "Z2Z";

				anotherFactory.Save();

				AssertCollectionNotContains("Should not contains the new branch as it's created in a different thread.", newBranchInAnotherFactory.PK, branch.Company.Branches.GetPKs());

				var interchangeInNewBranch = CreateAndSaveInterchange(Factory, InboundInterchangeProcessorForTesting.AppCode, "SND", "RCV", "header2", "body2", "");
				interchangeInNewBranch.EI_GB = newBranchInAnotherFactory.PK;

				Factory.Save();

				processor.ExecuteBatch();

				interchangeInNewBranch.Reload();
				AssertEquals("Should process the interchange as ValidBranchesForFilter should get these latest branches from database.", EDIInterchange.Status.Received, interchangeInNewBranch.EI_Status);
			}
		}

		#region Helper Methods

		public GlbBranch CreateBranchAndStaff()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "Z1Z";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "Z1Z";
			branch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.Branches.Add(branch);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TT";
			staff.GS_EmailAddress = "test@edi.com.au";

			var group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			GlbGroupLink link = Factory.New<GlbGroupLink>();
			link.GK_GG = group.PK;
			link.GK_GS = staff.PK;

			Factory.Save();

			return branch;
		}

		public static EDIInterchange CreateAndSaveInterchange(BusinessObjectFactory factory, string applicationCode, string sender, string receiver, string headerText, string bodyText, string footerText)
		{
			var interchange = factory.New<EDIInterchange>();

			interchange.EI_ApplicationCode = applicationCode;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_From = sender;
			interchange.EI_To = receiver;

			interchange.EI_HeaderText = headerText;
			interchange.EI_BodyText = bodyText;
			interchange.EI_FooterText = footerText;

			factory.Save();

			return interchange;
		}

		#endregion
	}
}
