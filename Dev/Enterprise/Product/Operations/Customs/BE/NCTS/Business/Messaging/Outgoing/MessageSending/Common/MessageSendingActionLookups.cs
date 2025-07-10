using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class MessageSendingActionLookups : NctsHeaderMessageSendingObjectLookups
	{
		public MessageSendingActionLookups(MessageSendingAction parent)
			: base(parent)
		{
		}

		new MessageSendingAction Parent => (MessageSendingAction)base.Parent;

		public CodeDescriptionPairList EntryTypeList
		{
			get
			{
				var parent = Parent;
				var movementHeader = parent.MovementHeader;
				var subApplicationCode = movementHeader.BM_SubApplicationCode;
				var additionalDeclarationType = movementHeader.BM_AdditionalDeclarationType;
				var customsStatus = movementHeader.BM_CustomsStatus;
				var phase = movementHeader.BM_Phase;
				var messageStatus = parent.Header.EffectiveMessageStatus;
				var invalidErrorFailedMessageStatus = new ZString[] { LogicalStatusList.Codes.Invalid, LogicalStatusList.Codes.Error, LogicalStatusList.Codes.Failed };

				return Factory.GetCachedValue($"BE.NCTS.Business.MessageSendingConfiguration.MessageTypeList_{subApplicationCode}.{additionalDeclarationType}.{customsStatus}.{phase}.{messageStatus}", () =>
				{
					var result = new CodeDescriptionPairList();
					if (subApplicationCode == NctsMoveHeaderType.Codes.Departure)
					{
						if ((additionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.A || additionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.D)
							&& customsStatus.IsEmpty
							&& (phase.IsEmpty || phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration))
						{
							result.AddPair(BEExportEntryTypeList.Codes.ExportDeclaration, BEExportEntryTypeList.Descriptions.ExportDeclaration);
						}
						else if (additionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.D
							&& customsStatus == NCTS5DepartureCustomsStatusList.Codes.PreLodged
							&& (phase == NctsMovementHeaderTransactionStatusList.Codes.Amendment
								|| phase == NctsMovementHeaderTransactionStatusList.Codes.Cancellation
								|| phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration
								|| (phase == NctsMovementHeaderTransactionStatusList.Codes.Presentation && messageStatus.In(invalidErrorFailedMessageStatus))))
						{
							result.AddPair(NctsMessageTypeList.Codes.Amendment, NctsMessageTypeList.Descriptions.Amendment);
							result.AddPair(NctsMessageTypeList.Codes.InvalidationCancellation, NctsMessageTypeList.Descriptions.InvalidationCancellation);
							result.AddPair(NctsMessageTypeList.Codes.PresentationNotification, NctsMessageTypeList.Descriptions.PresentationNotification);
						}
						else if (additionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.A)
						{
							if ((phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration
								|| phase == NctsMovementHeaderTransactionStatusList.Codes.Amendment)
									&& (customsStatus == NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid
									|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested))
							{
								result.AddPair(NctsMessageTypeList.Codes.Amendment, NctsMessageTypeList.Descriptions.Amendment);
							}
							else if ((phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration
								|| phase == NctsMovementHeaderTransactionStatusList.Codes.Amendment
								|| phase == NctsMovementHeaderTransactionStatusList.Codes.Cancellation)
									&& customsStatus == NCTS5DepartureCustomsStatusList.Codes.MrnAllocated)
							{
								result.AddPair(NctsMessageTypeList.Codes.Amendment, NctsMessageTypeList.Descriptions.Amendment);
								result.AddPair(NctsMessageTypeList.Codes.InvalidationCancellation, NctsMessageTypeList.Descriptions.InvalidationCancellation);
							}
							else if ((phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration
								|| phase == NctsMovementHeaderTransactionStatusList.Codes.NonArrivedMovement)
									&& customsStatus == NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry)
							{
								result.AddPair(NctsMessageTypeList.Codes.ResponseOnRequestForNonArrivedMovement, NctsMessageTypeList.Descriptions.ResponseOnRequestForNonArrivedMovement);
							}
							else if (customsStatus == NCTS5DepartureCustomsStatusList.Codes.Acknowledged
								&& (phase == NctsMovementHeaderTransactionStatusList.Codes.Amendment
									|| phase == NctsMovementHeaderTransactionStatusList.Codes.Cancellation
									|| phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration))
							{
								result.AddPair(NctsMessageTypeList.Codes.Amendment, NctsMessageTypeList.Descriptions.Amendment);
								result.AddPair(NctsMessageTypeList.Codes.InvalidationCancellation, NctsMessageTypeList.Descriptions.InvalidationCancellation);
							}
							else if (phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration)
							{
								if (customsStatus == NctsMovementHeaderTransactionStatusList.Codes.NoFullReleaseOfGoodsMovementRemainsOpen
									|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit)
								{
									result.AddPair(NctsMessageTypeList.Codes.InvalidationCancellation, NctsMessageTypeList.Descriptions.InvalidationCancellation);
								}
								else if (customsStatus == NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest
									|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.DecisionToControl
									|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.DiscrepanciesAtDestination)
								{
									result.AddPair(NctsMessageTypeList.Codes.RequestARelease, NctsMessageTypeList.Descriptions.RequestARelease);
								}
							}
						}
					}
					else if (subApplicationCode == NctsMoveHeaderType.Codes.Arrival && additionalDeclarationType.IsEmpty)
					{
						if (customsStatus.IsEmpty && (phase.IsEmpty || phase == NctsMovementHeaderTransactionStatusList.Codes.Arrival))
						{
							result.AddPair(NctsMessageTypeList.Codes.ArrivalNotification, NctsMessageTypeList.Descriptions.ArrivalNotification);
						}
						else if ((customsStatus == NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted
								&& (phase == NctsMovementHeaderTransactionStatusList.Codes.Arrival
								|| phase == NctsMovementHeaderTransactionStatusList.Codes.UnloadingPermission
								|| phase == NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks))
							|| (customsStatus == NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks
								&& (phase == NctsMovementHeaderTransactionStatusList.Codes.UnloadingPermission
								|| phase == NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks)))
						{
							result.AddPair(NctsMessageTypeList.Codes.UnloadingRemarks, NctsMessageTypeList.Descriptions.UnloadingRemarks);
						}
					}

					return result;
				});
			}
		}
	}
}
