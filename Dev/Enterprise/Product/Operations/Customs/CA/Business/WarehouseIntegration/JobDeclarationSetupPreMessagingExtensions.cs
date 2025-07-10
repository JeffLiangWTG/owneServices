using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.CA.Business
{
	public static class JobDeclarationSetupPreMessagingExtensions
	{
		public static PreMessagingActionResult PreMessagingAction(this JobDeclaration declaration, MessageSubTypes messageType)
		{
			bool isBondedWarehouse = false;
			var errorMessages = ZString.Empty;
			Func<PublishToUniversalResult> preMessagingAction = null;
			Action restoreToPreMessagingState = null;

			if (declaration.IsOutwardBondedWarehousingEnabled)
			{
				isBondedWarehouse = declaration.IsWHSUniversalXMLActive && (declaration.HasWHSTransaction || declaration.IsExBondAutomationEnabled);
				if (isBondedWarehouse)
				{
					errorMessages = CheckRequiredFieldsForBondedWarehousing(declaration, messageType);
					ActionForOutward(declaration, ref preMessagingAction, ref restoreToPreMessagingState, messageType);
				}
			}
			if (declaration.IsInwardBondedWarehousingEnabled)
			{
				isBondedWarehouse = declaration.IsWHSUniversalXMLActive && (declaration.HasWHSTransaction || declaration.HasLineGoingIntoAnAutomatedBondedWarehouse);
				if (isBondedWarehouse)
				{
					errorMessages = CheckRequiredFieldsForBondedWarehousing(declaration, messageType);
					ActionForInward(declaration, ref preMessagingAction, ref restoreToPreMessagingState, messageType);
				}
			}
			return new PreMessagingActionResult()
			{
				IsBondedWarehouse = isBondedWarehouse,
				ErrorMessageForCheckFieldsForBondedWarehous = errorMessages,
				PreMessagingAction = preMessagingAction,
				RestoreToPreMessagingState = restoreToPreMessagingState
			};
		}

		static ZString CheckRequiredFieldsForBondedWarehousing(this JobDeclaration declaration, MessageSubTypes messageType)
		{
			var isCreateOrChange = messageType == MessageSubTypes.Create || messageType == MessageSubTypes.Change;
			return declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing(
				checkProduct: isCreateOrChange, checkQuantity: isCreateOrChange, checkEntryDetails: isCreateOrChange);
		}

		static void ActionForOutward(this JobDeclaration declaration, ref Func<PublishToUniversalResult> preMessagingAction, ref Action restoreToPreMessagingState, MessageSubTypes messageType)
		{
			switch (messageType)
			{
				case MessageSubTypes.Withdraw:
					if (declaration.HasWHSTransaction)
					{
						preMessagingAction = () => declaration.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);
						restoreToPreMessagingState = declaration.PublishAcceptEventForWHSOutwardInADifferentFactory;
					}
					break;
				case MessageSubTypes.Create:
				case MessageSubTypes.Change:
					if (declaration.HasWHSTransaction)
					{
						preMessagingAction = declaration.PublishShipmentForWHSOutwardWithPreAmendmentData;
						restoreToPreMessagingState = declaration.RestoreLatestClearedBondedWarehouseOutwardInADifferentFactory;
					}
					else
					{
						preMessagingAction = () => declaration.PublishShipmentForWHSOutward(true);
						restoreToPreMessagingState = declaration.PublishCancelEventForWHSOutwardInADifferentFactory;
					}
					break;
			}
		}

		static void ActionForInward(this JobDeclaration declaration, ref Func<PublishToUniversalResult> preMessagingAction, ref Action restoreToPreMessagingState, MessageSubTypes messageType)
		{
			switch (messageType)
			{
				case MessageSubTypes.Withdraw:
					if (declaration.HasWHSTransaction && declaration.WarehouseTransactionStatus != WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal)
					{
						preMessagingAction = () => declaration.PublishCancelEventForWHSInwardAndSaveIfNeeded(true, WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal);
						restoreToPreMessagingState = declaration.RestoreLatestClearedBondedWarehouseInwardInADifferentFactory;
					}
					break;
				case MessageSubTypes.Create:
				case MessageSubTypes.Change:
					if (declaration.HasWHSInwardTransactionAndNotCreatedPending())
					{
						preMessagingAction = () => declaration.PublishShipmentForWHSInward(true);
						restoreToPreMessagingState = declaration.RestoreLatestClearedBondedWarehouseInwardInADifferentFactory;
					}
					else
					{
						preMessagingAction = () => declaration.PublishShipmentForWHSInward(true);
						restoreToPreMessagingState = () => declaration.PublishCancelEventForWHSInwardAndSaveIfNeeded(true, WarehouseTransactionStatusList.Codes.InwardCanceled);
					}
					break;
			}
		}
	}
}
