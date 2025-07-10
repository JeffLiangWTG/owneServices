using CargoWise.Types;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(MessageSendingObjectValidation))]
	sealed class MessageSendingObjectValidationTest : Customs.Business.Testing.MessageSendingObjectValidationTest
	{
		public void TestCheckECRAction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			var messageSendingObject = new MessageSendingObject(cusEntryHeader);
			messageSendingObject.ShouldSend = true;
			messageSendingObject.ProcedureCode = JPProcedureCodeList.Codes.ECR;

			var info = messageSendingObject.ActionInfo;

			var errorMessage = "Enter a valid Action.";
			messageSendingObject.Action = "X";
			AssertHasError(info, errorMessage);

			messageSendingObject.ShouldSend = false;
			messageSendingObject.Validation.ValidateAction();
			AssertNoError(info, errorMessage);

			messageSendingObject.ShouldSend = true;
			messageSendingObject.Action = ActionList.Codes.Nine;
			AssertNoError(info, errorMessage);

			errorMessage = "Please enter an Action.";
			messageSendingObject.Action = "";
			AssertHasError(info, errorMessage);
			messageSendingObject.Action = ActionList.Codes.Nine;
			AssertNoError(info, errorMessage);
		}

		public void TestCheckDeclarationCorrectionCopyRequest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			var messageSendingObject = new MessageSendingObject(cusEntryHeader);
			var targetInfo = messageSendingObject.DeclarationCorrectionCopyRequestInfo;

			var expectedMessageError = "Declaration Correction Copy Request can only be true when Procedure Code is IDC or EDC or EAC.";
			CombineAssertions(() =>
			{
				SetValueAndAssert(JPProcedureCodeList.Codes.IDC, true, false);
				SetValueAndAssert(JPProcedureCodeList.Codes.EDC, true, false);
				SetValueAndAssert(JPProcedureCodeList.Codes.EAC, true, false);

				SetValueAndAssert(JPProcedureCodeList.Codes.IDE, true, true);
				SetValueAndAssert(JPProcedureCodeList.Codes.IDE, false, false);

				SetValueAndAssert(JPProcedureCodeList.Codes.EDE, true, true);
				SetValueAndAssert(JPProcedureCodeList.Codes.EDE, false, false);
			});

			void SetValueAndAssert(ZString action, ZBool declarationCorrectionCopyRequest, bool shouldHasMessageError)
			{
				messageSendingObject.ProcedureCode = action;
				messageSendingObject.DeclarationCorrectionCopyRequest = declarationCorrectionCopyRequest;
				if (shouldHasMessageError)
				{
					AssertHasMessageError(
						string.Format("Should has message error when Action is {0} and DeclarationCorrectionCopyRequest is {1}.", messageSendingObject.ProcedureCode, messageSendingObject.DeclarationCorrectionCopyRequest),
						targetInfo, expectedMessageError);
				}
				else
				{
					AssertNoMessageError(
						string.Format("Should not has message error when Action is {0} and DeclarationCorrectionCopyRequest is {1}.", messageSendingObject.ProcedureCode, messageSendingObject.DeclarationCorrectionCopyRequest),
						targetInfo, expectedMessageError);
				}
			}
		}
	}
}
