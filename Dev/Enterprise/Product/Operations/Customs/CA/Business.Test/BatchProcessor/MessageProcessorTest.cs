using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class MessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageProcessorProcessesMessage()
		{
			processor.ExecuteBatch();
			aciMessage = new BusinessObjectFactory().Load<EDIMessage>(aciMessage.PK);
			AssertEquals(EDIMessage.Status.Failed, aciMessage.EM_Status);
			expMessage = new BusinessObjectFactory().Load<EDIMessage>(expMessage.PK);
			AssertEquals(EDIMessage.Status.Failed, expMessage.EM_Status);
			impMessage = new BusinessObjectFactory().Load<EDIMessage>(impMessage.PK);
			AssertEquals(EDIMessage.Status.Failed, impMessage.EM_Status);
			cacMessage = new BusinessObjectFactory().Load<EDIMessage>(cacMessage.PK);
			AssertEquals(EDIMessage.Status.Failed, cacMessage.EM_Status);
			ErrorReporter.Clear();
		}

		public void TestACIMessageFilter()
		{
			BatchProcessorUtilities.ResetValidACIBranchesForTesting();
			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Egypt;
			newCompany.GC_Code = "EGY";
			var newBranch = Factory.New<GlbBranch>();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			newBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
			newBranch.GB_Code = "XXX";
			var newBranchForNewCompany = newCompany.Branches.AddNew();
			newBranchForNewCompany.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Constants.CountryCodes.Egypt)).RL_Code;
			newBranchForNewCompany.GB_Code = "EG1";

			Factory.Save();

			var message1 = GetNewMessage(EDIMessage.ApplicationCodes.CAEXP, GlbBranch.CurrentBranch.PK);
			var message2 = GetNewMessage(EDIMessage.ApplicationCodes.CAEXP, newBranch.PK);
			var message3 = GetNewMessage(EDIMessage.ApplicationCodes.CAEXP, newBranchForNewCompany.PK);
			var message4 = GetNewMessage(EDIMessage.ApplicationCodes.CAACI, GlbBranch.CurrentBranch.PK);
			var message5 = GetNewMessage(EDIMessage.ApplicationCodes.CAACI, newBranch.PK);
			var message6 = GetNewMessage(EDIMessage.ApplicationCodes.CAACI, newBranchForNewCompany.PK);

			Factory.Save();

			processor.ExecuteBatch();

			AssertProcessed(message1, true);
			AssertProcessed(message2, true);
			AssertProcessed(message3, true);
			AssertProcessed(message4, true);
			AssertProcessed(message5, true);
			AssertProcessed(message6, true);
			ErrorReporter.Clear();
		}

		EDIMessage GetNewMessage(ZString appCode, ZGuid branchPK)
		{
			var result = Factory.New<EDIMessage>();
			result.EM_ApplicationCode = appCode;
			result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			result.EM_GB = branchPK;
			result.EM_Status = EDIMessage.Status.Queued;
			return result;
		}

		void AssertProcessed(EDIMessage message, bool shouldHaveBeenProcessed)
		{
			var messageInNewFactory = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			if (shouldHaveBeenProcessed)
			{
				AssertNotEquals("Message should have been selected and processed", EDIMessage.Status.Queued, messageInNewFactory.EM_Status);
			}
			else
			{
				AssertEquals("Message should NOT have been selected and processed", EDIMessage.Status.Queued, messageInNewFactory.EM_Status);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbGroup group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_FullName = "blah";
			staff.GS_EmailAddress = "blah@blah.com";
			staff.GS_Code = "ZAC";
			processor = new CAMessageProcessor();

			aciMessage = Factory.New<EDIMessage>();
			aciMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			aciMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			aciMessage.EM_Status = EDIMessage.Status.Queued;

			expMessage = Factory.New<EDIMessage>();
			expMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAEXP;
			expMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			expMessage.EM_Status = EDIMessage.Status.Queued;

			impMessage = Factory.New<EDIMessage>();
			impMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			impMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			impMessage.EM_Status = EDIMessage.Status.Queued;

			cacMessage = Factory.New<EDIMessage>();
			cacMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CACustoms;
			cacMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cacMessage.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();
		}

		CAMessageProcessor processor;
		EDIMessage aciMessage;
		EDIMessage expMessage;
		EDIMessage impMessage;
		EDIMessage cacMessage;
	}
}
