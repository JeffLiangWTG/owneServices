using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.Declaration.GUI;

public class MessagingActionsController : SendsMessagesToCustomsGUI
{
	public MessagingActionsController(JobDeclaration declaration)
	{
		this.declaration = declaration;
	}

	readonly JobDeclaration declaration;

	protected override ContinueWithSave DoActionsBeforeSendingRequiredMessages(Customs.Business.IMessageManager manager, Customs.Business.RequiredMessagesInformation detectionResult)
	{
		ContinueWithSave result = base.DoActionsBeforeSendingRequiredMessages(manager, detectionResult);

		if (result == ContinueWithSave.Yes && detectionResult.IsThereAmendmentOrWithdrawal)
		{
			var errorMessages = declaration.IsBondedWarehousingDisabled ? ZString.Empty : declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing(checkEntryDetails: declaration.IsExWarehouse);
			if (errorMessages.IsEmpty)
			{
				result = GenerateDeclarationQuestionIfRequiredAndShowCPQAForm(declaration, new TypedEnumerable<CusEntryHeader>(detectionResult.BizObjsToSendMessageForForAmendmentOrWithdrawal));

				if (result == ContinueWithSave.Yes)
				{
					CMRAmendmentWithdrawalReason reason = (CMRAmendmentWithdrawalReason)detectionResult.AmendmentWithdrawalReason;
					reason.ReasonText = declaration.OutstandingAmendmentLogManger.AllOustandingAmendmentReferences;
					result = GetAmendmentWithdrawalReason(reason);
				}
			}
			else
			{
				result = ContinueWithSave.No;
				NotifyUserOfAnInvalidOperation(errorMessages);
			}
		}

		return result;
	}
}
