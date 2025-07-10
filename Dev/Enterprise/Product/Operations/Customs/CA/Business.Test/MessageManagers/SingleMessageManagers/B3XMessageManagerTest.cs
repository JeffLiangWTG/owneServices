using System;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(B3XMessageManager))]
	sealed class B3XMessageManagerTest : CAMessageManagerTestCase
	{
		public void TestMessageGetActionCodeDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			declaration.JE_DeclarationReference = "B00001111";
			Factory.Save();

			var manager = new B3XMessageManagerForTesting(new B3XMessageWrapper(declaration));
			Factory.Save();

			AssertEquals("Original X Type Entry Message for Declaration B00001111 has been generated.", manager.LastNotification(MessageSubTypes.Create));
		}

		public void TestSubmissionDateOnCustomsCommenced()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			Factory.Save();

			Assert(declaration.CA_B2SubmissionDate.IsEmpty);

			var manager = new B3XMessageManagerForTesting(new B3XMessageWrapper(declaration));
			manager.OverrideCanSendThisMessage = true;
			manager.SendMessage(MessageSubTypes.Create, false);

			Assert(!declaration.CA_B2SubmissionDate.IsEmpty);
		}

		public override void TestGetMessageBuilder()
		{
			AssertEquals("GetMessageBuilder", typeof(B3CusdecMessageBuilder<B3XMessage>), ((B3XMessageManagerForTesting)messageManager).GetMessageBuilder_Exposed(MessageSubTypes.Undefined).GetType());
		}

		public override void TestMessageFriendlyName()
		{
			AssertEquals("MessageFriendlyName", "X Type Entry Message for Declaration B00001000", messageManager.MessageFriendlyName);
		}

		public override void TestPopulateMessages()
		{
			((B3XMessageManagerForTesting)messageManager).PopulateMessage_Exposed(MessageSubTypes.Create);
			AssertEquals("1 message", 1, declaration.Messages.Count);
			AssertEquals("EM_MessageSubType", MessageSubTypeCodes.Codes.Original, declaration.Messages[0].EM_MessageSubType);
			AssertEquals("Message status", MessageStatusList.Codes.AwaitingOriginal, declaration.JE_MessageStatus);
		}

		protected override IEDIFACTMessageAttachee GetDataWrapper()
		{
			return new B3XMessageWrapper(declaration);
		}

		protected override EDIFACTMessageManager GetMessageManager()
		{
			return new B3XMessageManagerForTesting((IB3Header)dataWrapper);
		}

		JobDeclaration declaration;

		IDisposable asecSetup;

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			Factory.Save();
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest("12345");
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345");

			var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var customsDefaultToCurrentLoginDeptRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
			customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		protected override void AssertCanSendThisMessage(CusEntryHeader entryHeader, ZString expectedMessage)
		{
			Assert(true);
		}

		public override void TestCanSendThisMessage()
		{
			var originalJob = Factory.New<JobDeclaration>();
			originalJob.JE_DeclarationReference = "B00000001";
			originalJob.TransactionNumber.AccountSecurityCode = "12345";
			originalJob.TransactionNumber.SequentialNumber = "00006789";
			originalJob.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ccn = originalJob.CargoControlNumbers.AddNew();
			ccn.CY_CargoControlNumber = "8036X557";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			declaration.CA_B2AcceptedDate = new ZDateTime(2020, 12, 28);
			declaration.CA_AmendmentTo = AmendmentToList.Codes.B2;
			declaration.CA_OriginalTransactionNo = "12345000067897";
			Factory.Save();

			var manager = new B3XMessageManagerForTesting(new B3XMessageWrapper(declaration));
			manager.Notification.NextAnswer = false;
			manager.SendMessage(MessageSubTypes.Create, false);

			AssertEquals("MessageText", @"This B3X has been already submitted for Review – hence no changes are allowed to the data that is printed on the B3X.
If the current changes are of the administrative nature, something that would NOT cause the reprint of the B3X to produce different results, please proceed with sending, otherwise please do not send this message.", manager.Notification.LastMessage);
		}

		protected override void AssertResetDeclaration(CAMessageManager manager, Action resetDeclaration, bool securityAllowed)
		{
			Assert(true);
		}
	}
}
