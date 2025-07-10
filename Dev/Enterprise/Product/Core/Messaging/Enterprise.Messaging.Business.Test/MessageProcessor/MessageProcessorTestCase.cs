using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.MessageProcessors.Testing
{
	public class MessageProcessorTestCase : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		[NUnit.Framework.ExpectException(typeof(ArgumentNullException))]
		public void TestPreProcessNullMessageThrowsArgumentNullException()
		{
			EDIMessage message = null;
			MessageProcessor.PreProcessMessage(message);
		}

		[NUnit.Framework.ExpectException(typeof(ArgumentNullException))]
		public void TestProcessNullMessageThrowsArgumentNullException()
		{
			EDIMessage message = null;
			MessageProcessor.ProcessMessage(message);
		}

		public void TestGetUserToNotify()
		{
			var batchProcessorStaffMember = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.ServiceUserCode);
			var postmasterStaffMember = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, "CWPostMaster");
			var currentStaffMember = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);

			var parent = Factory.New<TestHelperEDIMessage>();

			var result = MessageProcessor.GetLastNonBatchProcessorStaffToSendMessage(parent);
			AssertNull(result);

			SimulateMessage(parent, postmasterStaffMember, ZDateTime.Now.AddMinutes(-15), EDIMessage.Direction.Transmit);
			result = MessageProcessor.GetLastNonBatchProcessorStaffToSendMessage(parent);
			AssertEquals("LastUser", postmasterStaffMember, result);

			SimulateMessage(parent, batchProcessorStaffMember, ZDateTime.Now.AddMinutes(-10), EDIMessage.Direction.Receive);
			result = MessageProcessor.GetLastNonBatchProcessorStaffToSendMessage(parent);
			AssertEquals("LastUser", postmasterStaffMember, result);

			SimulateMessage(parent, currentStaffMember, ZDateTime.Now.AddMinutes(-5), EDIMessage.Direction.Transmit);
			result = MessageProcessor.GetLastNonBatchProcessorStaffToSendMessage(parent);
			AssertEquals("LastUser", currentStaffMember, result);

			SimulateMessage(parent, batchProcessorStaffMember, ZDateTime.Now, EDIMessage.Direction.Transmit);
			result = MessageProcessor.GetLastNonBatchProcessorStaffToSendMessage(parent);
			AssertEquals("LastUser", currentStaffMember, result);
		}

		public void TestSendAcknowledgementReport()
		{
			EDIMessage parent = (EDIMessage)Factory.New(typeof(TestHelperEDIMessage));
			SimulateMessage(parent, PostMasterStaffMember, ZDateTime.Now.AddMinutes(-15), EDIMessage.Direction.Transmit);

			MessageProcessor.AcknowledgementEmailModePublic = Core.Constants.EmailTo.NoEmails;
			TestAcknowledgement(MessageProcessor, parent, 0, 0);

			MessageProcessor.AcknowledgementEmailModePublic = Core.Constants.EmailTo.StaffMember;
			TestAcknowledgement(MessageProcessor, parent, 1, 0);

			MessageProcessor.AcknowledgementEmailModePublic = Core.Constants.EmailTo.NominatedGroup;
			TestAcknowledgement(MessageProcessor, parent, 0, 1);

			MessageProcessor.AcknowledgementEmailModePublic = Core.Constants.EmailTo.StaffMemberAndNominatedGroup;
			TestAcknowledgement(MessageProcessor, parent, 1, 1);
		}

		public void TestSendImpedimentReport()
		{
			EDIMessage parent = (EDIMessage)Factory.New(typeof(TestHelperEDIMessage));
			SimulateMessage(parent, PostMasterStaffMember, ZDateTime.Now.AddMinutes(-15), EDIMessage.Direction.Transmit);

			MessageProcessor.ImpedimentEmailModePublic = Core.Constants.EmailTo.NoEmails;
			TestImpediment(MessageProcessor, parent, 0, 0);

			MessageProcessor.ImpedimentEmailModePublic = Core.Constants.EmailTo.StaffMember;
			TestImpediment(MessageProcessor, parent, 1, 0);

			MessageProcessor.ImpedimentEmailModePublic = Core.Constants.EmailTo.NominatedGroup;
			TestImpediment(MessageProcessor, parent, 0, 1);

			MessageProcessor.ImpedimentEmailModePublic = Core.Constants.EmailTo.StaffMemberAndNominatedGroup;
			TestImpediment(MessageProcessor, parent, 1, 1);
		}

		public void TestSendErrorReport()
		{
			EDIMessage parent = (EDIMessage)Factory.New(typeof(TestHelperEDIMessage));
			SimulateMessage(parent, PostMasterStaffMember, ZDateTime.Now.AddMinutes(-15), EDIMessage.Direction.Transmit);

			MessageProcessor.ErrorEmailModePublic = Core.Constants.EmailTo.NoEmails;
			TestError(MessageProcessor, parent, 0, 0);

			MessageProcessor.ErrorEmailModePublic = Core.Constants.EmailTo.StaffMember;
			TestError(MessageProcessor, parent, 1, 0);

			MessageProcessor.ErrorEmailModePublic = Core.Constants.EmailTo.NominatedGroup;
			TestError(MessageProcessor, parent, 0, 1);

			MessageProcessor.ErrorEmailModePublic = Core.Constants.EmailTo.StaffMemberAndNominatedGroup;
			TestError(MessageProcessor, parent, 1, 1);

			parent = null;

			MessageProcessor.ErrorEmailModePublic = Core.Constants.EmailTo.NoEmails;
			TestError(MessageProcessor, parent, 0, 0);

			MessageProcessor.ErrorEmailModePublic = Core.Constants.EmailTo.StaffMember;
			TestError(MessageProcessor, parent, 0, 0);

			MessageProcessor.ErrorEmailModePublic = Core.Constants.EmailTo.NominatedGroup;
			TestError(MessageProcessor, parent, 0, 1);

			MessageProcessor.ErrorEmailModePublic = Core.Constants.EmailTo.StaffMemberAndNominatedGroup;
			TestError(MessageProcessor, parent, 0, 1);

			MessageProcessor.ReportWhenNoParentExposed = true;

			MessageProcessor.ErrorEmailModePublic = Core.Constants.EmailTo.NoEmails;
			TestError(MessageProcessor, parent, 0, 1);

			MessageProcessor.ErrorEmailModePublic = Core.Constants.EmailTo.StaffMember;
			TestError(MessageProcessor, parent, 0, 1);

			MessageProcessor.ErrorEmailModePublic = Core.Constants.EmailTo.NominatedGroup;
			TestError(MessageProcessor, parent, 0, 1);

			MessageProcessor.ErrorEmailModePublic = Core.Constants.EmailTo.StaffMemberAndNominatedGroup;
			TestError(MessageProcessor, parent, 0, 1);
		}

		#region Implementation

		void TestAcknowledgement(TestHelperMessageProcessor messageProcessor, BusinessObject parent, int recipients, int cCRecipients)
		{
			EmailDef email = new EmailDef();
			messageProcessor.SendAcknowledgementReport(parent, email);
			AssertEquals("Recipients", recipients, email.Recipients.Count);
			AssertEquals("CCRecipients", cCRecipients, email.CCRecipients.Count);
		}

		void TestImpediment(TestHelperMessageProcessor messageProcessor, BusinessObject parent, int recipients, int cCRecipients)
		{
			EmailDef email = new EmailDef();
			messageProcessor.SendImpedimentReport(parent, email);
			AssertEquals("Recipients", recipients, email.Recipients.Count);
			AssertEquals("CCRecipients", cCRecipients, email.CCRecipients.Count);
		}

		void TestError(TestHelperMessageProcessor messageProcessor, BusinessObject parent, int recipients, int cCRecipients)
		{
			EmailDef email = new EmailDef();
			messageProcessor.SendErrorReport(parent, email);
			AssertEquals("Recipients", recipients, email.Recipients.Count);
			AssertEquals("CCRecipients", cCRecipients, email.CCRecipients.Count);
		}

		void SimulateMessage(BusinessObject parent, GlbStaff sender, ZDateTime addedTime, ZString direction)
		{
			var newMessage = (EDIMessage)parent.Factory.New(typeof(TestHelperEDIMessage));
			newMessage.EM_LinkedObject = parent;
			newMessage.EM_ReceiveTransmit = direction;
			parent.Factory.Save();

			// These tests might depend on the log being changed outside the main factory.
			newMessage.EM_SystemCreateUser = sender.GS_Code;
			newMessage.EM_SystemCreateTimeUtc = addedTime;
			parent.Factory.Save();
		}

		public void TestPreProcessMessage()
		{
			var logger = new LoggingInformation();
			var processor = new TestHelperMessageProcessor(logger);

			AssertExceptionThrown<ArgumentNullException>(() => processor.PreProcessMessage(null));

			var message = EDIMessageTestFactory.New(Factory);
			message.EM_Status = EDIMessage.Status.Queued;
			processor.PreProcessMessage(message);

			AssertEquals(message.EM_Status, EDIMessage.Status.PreProcessedOK);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Logger = new LoggingInformation();
			MessageProcessor = new TestHelperMessageProcessor(Logger);

			PostMasterStaffMember = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, "CWPostMaster");
			PostMasterStaffMember.GS_EmailAddress = "SYSADMIN@EDI.COM.AU";
			Factory.Save();
		}

		GlbStaff PostMasterStaffMember;

		LoggingInformation Logger;

		TestHelperMessageProcessor MessageProcessor;

		#region TestHelperMessageProcessor

		protected class TestHelperMessageProcessor : MessageProcessor
		{
			public TestHelperMessageProcessor(LoggingInformation logger)
				: base(logger, "ABC", "Name")
			{
				ReportWhenNoParentExposed = false;
			}

			public bool ReportWhenNoParentExposed;
			protected override bool ReportWhenNoParent
			{
				get { return ReportWhenNoParentExposed; }
			}

			protected override ZGuid AcknowledgementEmailGroup
			{
				get
				{
					if (fAcknowledgementEmailGroup.IsEmpty)
					{
						var factory = new BusinessObjectFactory();
						var group = factory.New<GlbGroup>();
						var currentStaffMember = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GlbStaff.CurrentUser.GS_Code);

						group.Staff.Add(currentStaffMember);
						currentStaffMember.GS_EmailAddress = "TEST@EDI.COM.AU";
						var anotherStaff = factory.New<GlbStaff>();
						anotherStaff.GS_EmailAddress = "Hosting.Notifications@cargowise.com";
						group.Staff.Add(anotherStaff);
						factory.Save();
						fAcknowledgementEmailGroup = group.PK;
					}
					return fAcknowledgementEmailGroup;
				}
			}
			protected ZGuid fAcknowledgementEmailGroup;

			protected override ZString AcknowledgementEmailMode
			{
				get { return AcknowledgementEmailModePublic; }
			}
			public ZString AcknowledgementEmailModePublic;
			public ZString ImpedimentEmailModePublic;
			public ZString ErrorEmailModePublic;

			protected override ZGuid ImpedimentEmailGroup { get { return AcknowledgementEmailGroup; } }
			protected override ZString ImpedimentEmailMode { get { return ImpedimentEmailModePublic; } }
			protected override ZGuid ErrorEmailGroup { get { return AcknowledgementEmailGroup; } }
			protected override ZString ErrorEmailMode { get { return ErrorEmailModePublic; } }

			protected override string DoProcessingReturningStatus(EDIMessage message)
			{
				return EDIMessage.Status.Error;
			}
		}

		#endregion

		#region TestHelperEDIMessage

		public class TestHelperEDIMessage : EDIMessage
		{
			public TestHelperEDIMessage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
			{
				//dont do anything
			}
		}

		#endregion

		#endregion

	}
}
