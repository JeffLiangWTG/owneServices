using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ExportsOtherMessagesStatusCalculator))]
	sealed class ExportsOtherMessagesStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInterestedMessageTypes()
		{
			AssertEquals("length", 6, Calculator.InterestedMessageTypes.Length);
		}

		public void TestStatusInfo()
		{
			AssertEquals("name", "JE_MessageStatus", Calculator.StatusInfo.Name);
		}

		public void TestGetResponseStatusForWARRELOriginal()
		{
			CMRWARRELMessage message = Factory.New<CMRWARRELMessage>();
			message.EM_MessageText = CMRExportMessagesTestData.WARRELOriginal;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			CMRWARRELRMessage incomingMessage = Factory.New<CMRWARRELRMessage>();
			incomingMessage.EM_MessageText = CMRExportMessagesTestData.WARRELFails;

			Assert("IsWARRELMessage", Calculator.IsWARRELMessage(message));
			Assert("IsOriginal", Calculator.IsOriginal(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearWARRELOriginal.Code, Calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingWARRELOriginal.Code, Calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailWARRELOriginal.Code, Calculator.GetRejectedResponseStatus(message, incomingMessage));
			AssertEquals("GetErrorResponseStatus", CustomsEntryStatus.ClearWARRELOriginal.Code, Calculator.GetAcceptedWithErrorsResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForWARRELReplacement()
		{
			CMRWARRELMessage message = Factory.New<CMRWARRELMessage>();
			message.EM_MessageText = CMRExportMessagesTestData.WARRELReplace;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
			CMRWARRELRMessage incomingMessage = Factory.New<CMRWARRELRMessage>();
			incomingMessage.EM_MessageText = CMRExportMessagesTestData.WARRELFails;

			Assert("IsWARRELMessage", Calculator.IsWARRELMessage(message));
			Assert("IsReplacement", Calculator.IsAmendment(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearWARRELReplacement.Code, Calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingWARRELReplacement.Code, Calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailWARRELReplacement.Code, Calculator.GetRejectedResponseStatus(message, incomingMessage));
			AssertEquals("GetErrorResponseStatus", CustomsEntryStatus.ClearWARRELReplacement.Code, Calculator.GetAcceptedWithErrorsResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForWARRELWithdrawl()
		{
			CMRWARRELMessage message = Factory.New<CMRWARRELMessage>();
			message.EM_MessageText = CMRExportMessagesTestData.WARRELWithdrawl;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
			CMRWARRELRMessage incomingMessage = Factory.New<CMRWARRELRMessage>();
			incomingMessage.EM_MessageText = CMRExportMessagesTestData.WARRELFails;

			Assert("IsWARRELMessage", Calculator.IsWARRELMessage(message));
			Assert("IsWithdrawl", Calculator.IsWithdrawl(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearWARRELWithdrawal.Code, Calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingWARRELWithdrawal.Code, Calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailWARRELWithdrawal.Code, Calculator.GetRejectedResponseStatus(message, incomingMessage));
			AssertEquals("GetErrorResponseStatus", CustomsEntryStatus.ClearWARRELWithdrawal.Code, Calculator.GetAcceptedWithErrorsResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForWARRETOriginal()
		{
			CMRWARRETMessage message = Factory.New<CMRWARRETMessage>();
			message.EM_MessageText = CMRExportMessagesTestData.WARRETOriginal;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			CMRWARRETRMessage incomingMessage = Factory.New<CMRWARRETRMessage>();
			incomingMessage.EM_MessageText = CMRExportMessagesTestData.WARRETFails;

			Assert("IsWARRETMessage", Calculator.IsWARRETMessage(message));
			Assert("IsOriginal", Calculator.IsOriginal(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearWARRETOriginal.Code, Calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingWARRETOriginal.Code, Calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailWARRETOriginal.Code, Calculator.GetRejectedResponseStatus(message, incomingMessage));
			AssertEquals("GetErrorResponseStatus", CustomsEntryStatus.ClearWARRETOriginal.Code, Calculator.GetAcceptedWithErrorsResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForWARRETReplacement()
		{
			CMRWARRETMessage message = Factory.New<CMRWARRETMessage>();
			message.EM_MessageText = CMRExportMessagesTestData.WARRETReplace;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
			CMRWARRETRMessage incomingMessage = Factory.New<CMRWARRETRMessage>();
			incomingMessage.EM_MessageText = CMRExportMessagesTestData.WARRETFails;

			Assert("IsWARRETMessage", Calculator.IsWARRETMessage(message));
			Assert("IsReplacement", Calculator.IsAmendment(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearWARRETReplacement.Code, Calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingWARRETReplacement.Code, Calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailWARRETReplacement.Code, Calculator.GetRejectedResponseStatus(message, incomingMessage));
			AssertEquals("GetErrorResponseStatus", CustomsEntryStatus.ClearWARRETReplacement.Code, Calculator.GetAcceptedWithErrorsResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForDEPRECOriginal()
		{
			CMRDEPRECMessage message = Factory.New<CMRDEPRECMessage>();
			message.EM_MessageText = CMRExportMessagesTestData.DEPRECOriginal;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			CMRDEPRECRMessage incomingMessage = Factory.New<CMRDEPRECRMessage>();
			incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRECFails;

			Assert("IsDEPRECMessage", Calculator.IsDEPRECMessage(message));
			Assert("IsOriginal", Calculator.IsOriginal(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearDEPRECOriginal.Code, Calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingDEPRECOriginal.Code, Calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailDEPRECOriginal.Code, Calculator.GetRejectedResponseStatus(message, incomingMessage));
			AssertEquals("GetErrorResponseStatus", CustomsEntryStatus.ErrorDEPRECOriginal.Code, Calculator.GetAcceptedWithErrorsResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForDEPRECReplacement()
		{
			CMRDEPRECMessage message = Factory.New<CMRDEPRECMessage>();
			message.EM_MessageText = CMRExportMessagesTestData.DEPRECReplace;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
			CMRDEPRECRMessage incomingMessage = Factory.New<CMRDEPRECRMessage>();
			incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRECFails;

			Assert("IsDEPRECMessage", Calculator.IsDEPRECMessage(message));
			Assert("IsReplacement", Calculator.IsAmendment(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearDEPRECReplacement.Code, Calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingDEPRECReplacement.Code, Calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailDEPRECReplacement.Code, Calculator.GetRejectedResponseStatus(message, incomingMessage));
			AssertEquals("GetErrorResponseStatus", CustomsEntryStatus.ErrorDEPRECReplacement.Code, Calculator.GetAcceptedWithErrorsResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForDEPRECWithdrawl()
		{
			CMRDEPRECMessage message = Factory.New<CMRDEPRECMessage>();
			message.EM_MessageText = CMRExportMessagesTestData.DEPRECWithdrawl;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
			CMRDEPRECRMessage incomingMessage = Factory.New<CMRDEPRECRMessage>();
			incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRECFails;

			Assert("IsDEPRECMessage", Calculator.IsDEPRECMessage(message));
			Assert("IsWithdrawl", Calculator.IsWithdrawl(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearDEPRECWithdrawal.Code, Calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingDEPRECWithdrawal.Code, Calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailDEPRECWithdrawal.Code, Calculator.GetRejectedResponseStatus(message, incomingMessage));
			AssertEquals("GetErrorResponseStatus", CustomsEntryStatus.ErrorDEPRECWithdrawal.Code, Calculator.GetAcceptedWithErrorsResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForDEPRELOriginal()
		{
			CMRDEPRELMessage message = Factory.New<CMRDEPRELMessage>();
			message.EM_MessageText = CMRExportMessagesTestData.DEPRELOriginal;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			CMRDEPRELRMessage incomingMessage = Factory.New<CMRDEPRELRMessage>();
			incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRELFails;

			Assert("IsDEPRELMessage", Calculator.IsDEPRELMessage(message));
			Assert("IsOriginal", Calculator.IsOriginal(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearDEPRELOriginal.Code, Calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingDEPRELOriginal.Code, Calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailDEPRELOriginal.Code, Calculator.GetRejectedResponseStatus(message, incomingMessage));
			AssertEquals("GetErrorResponseStatus", CustomsEntryStatus.ErrorDEPRELOriginal.Code, Calculator.GetAcceptedWithErrorsResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForDEPRELReplacement()
		{
			CMRDEPRELMessage message = Factory.New<CMRDEPRELMessage>();
			message.EM_MessageText = CMRExportMessagesTestData.DEPRELReplace;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
			CMRDEPRELRMessage incomingMessage = Factory.New<CMRDEPRELRMessage>();
			incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRELFails;

			Assert("IsDEPRELMessage", Calculator.IsDEPRELMessage(message));
			Assert("IsReplacement", Calculator.IsAmendment(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearDEPRELReplacement.Code, Calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingDEPRELReplacement.Code, Calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailDEPRELReplacement.Code, Calculator.GetRejectedResponseStatus(message, incomingMessage));
			AssertEquals("GetErrorResponseStatus", CustomsEntryStatus.ErrorDEPRELReplacement.Code, Calculator.GetAcceptedWithErrorsResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForDEPRELWithdrawl()
		{
			CMRDEPRELMessage message = Factory.New<CMRDEPRELMessage>();
			message.EM_MessageText = CMRExportMessagesTestData.DEPRELWithdrawl;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
			CMRDEPRELRMessage incomingMessage = Factory.New<CMRDEPRELRMessage>();
			incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRELFails;

			Assert("IsDEPRELMessage", Calculator.IsDEPRELMessage(message));
			Assert("IsWithdrawl", Calculator.IsWithdrawl(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearDEPRELWithdrawal.Code, Calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingDEPRELWithdrawal.Code, Calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailDEPRELWithdrawal.Code, Calculator.GetRejectedResponseStatus(message, incomingMessage));
			AssertEquals("GetErrorResponseStatus", CustomsEntryStatus.ErrorDEPRELWithdrawal.Code, Calculator.GetAcceptedWithErrorsResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForDRWBCKOriginal()
		{
			CMRDRWBCKMessage message = Factory.New<CMRDRWBCKMessage>();
			message.EM_MessageText = CMRDrawbackMessageTestData.DRWBCKOriginal;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			CMRDRWBCKRMessage incomingMessage = Factory.New<CMRDRWBCKRMessage>();
			incomingMessage.EM_MessageText = CMRDrawbackMessageTestData.DRWBCKFails;

			Assert("IsOriginal", Calculator.IsOriginal(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearOriginal.Code, Calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingOriginal.Code, Calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailOriginal.Code, Calculator.GetRejectedResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForDRWBCKAmendment()
		{
			CMRDRWBCKMessage message = Factory.New<CMRDRWBCKMessage>();
			message.EM_MessageText = CMRDrawbackMessageTestData.DRWBCKOriginal;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
			CMRDRWBCKRMessage incomingMessage = Factory.New<CMRDRWBCKRMessage>();
			incomingMessage.EM_MessageText = CMRDrawbackMessageTestData.DRWBCKFails;

			Assert("IsReplacement", Calculator.IsAmendment(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearAmendment.Code, Calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingAmendment.Code, Calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailAmendment.Code, Calculator.GetRejectedResponseStatus(message, incomingMessage));
		}

		public void TestGetResponseStatusForDRWBCKWithdrawl()
		{
			CMRDRWBCKMessage message = Factory.New<CMRDRWBCKMessage>();
			message.EM_MessageText = CMRDrawbackMessageTestData.DRWBCKOriginal;
			message.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
			CMRDRWBCKRMessage incomingMessage = Factory.New<CMRDRWBCKRMessage>();
			incomingMessage.EM_MessageText = CMRDrawbackMessageTestData.DRWBCKFails;

			Assert("IsWithdrawl", Calculator.IsWithdrawl(message));
			AssertEquals("GetAcceptedResponseStatus", CustomsEntryStatus.ClearWithdrawal.Code, Calculator.GetAcceptedResponseStatus(message));
			AssertEquals("GetAwaitingResponseStatus", CustomsEntryStatus.AwaitingWithdrawal.Code, Calculator.GetAwaitingResponseStatus(message));
			AssertEquals("GetRejectedResponseStatus", CustomsEntryStatus.FailWithdrawal.Code, Calculator.GetRejectedResponseStatus(message, incomingMessage));
		}

		protected override BusinessObject GetNewBusinessObject() => new ExportsOtherMessagesStatusCalculator(JobDeclaration.New(Factory));

		ExportsOtherMessagesStatusCalculator calculator;
		ExportsOtherMessagesStatusCalculator Calculator => calculator ?? (calculator = (ExportsOtherMessagesStatusCalculator)GetNewBusinessObject());
	}
}
