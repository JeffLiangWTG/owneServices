using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(AESMessageSendingAction))]
	class AESMessageSendingActionTest : CusEntryHeaderMessageSendingActionTest<AESMessageSendingAction>
	{
		public void TestAnnotationIsClearedWhenReadOnly()
		{
			(var company, var branch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var sendingAction = new AESMessageSendingAction(entryHeader);

			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportCancellation;
			sendingAction.Annotation = "An annotation";
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportOriginal;
			AssertEquals("sendingAction.AnnotationInfo.ReadOnly", true, sendingAction.AnnotationInfo.ReadOnly);
			AssertEquals("Annotation", ZString.Empty, sendingAction.Annotation);

			sendingAction.Annotation = "Another annotation";
			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportCancellation;
			AssertEquals("sendingAction.AnnotationInfo.ReadOnly", false, sendingAction.AnnotationInfo.ReadOnly);
			AssertEquals("Annotation", "Another annotation", sendingAction.Annotation);

			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExitCancellation;
			AssertEquals("sendingAction.AnnotationInfo.ReadOnly", false, sendingAction.AnnotationInfo.ReadOnly);
		}

		protected override Type ExpectedLookupsType => typeof(AESMessageSendingActionLookups);

		protected override Type ExpectedSenderType => typeof(AESMessageSender);

		protected override Type ExpectedValidationType => typeof(AESMessageSendingActionValidation);

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AESMessageSendingAction((CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew());
		}
	}
}
