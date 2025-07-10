using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC028C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC028CMessageProcessor : NCTSMessageProcessor<ICC028CDataProvider>
	{
		public CC028CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.CC028C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC028C };

		protected override Type MessageInterpreterType => typeof(CC028CMessageInterpreter);

		protected override ICC028CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc028CType, CC028CDataProvider>();

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC028CDataProvider messageDataProvider) => NctsMessageHelper.LocateHeaderByLRN(message.Factory, messageDataProvider)?.MovementHeader;

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC028CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

			if (moveHeader.BM_CustomsStatus != NCTS5DepartureCustomsStatusList.Codes.PreLodged && moveHeader.BM_CustomsStatus != NCTS5DepartureCustomsStatusList.Codes.Acknowledged)
			{
				DiscardMessage(message, Res.GetString("6EE9CDB5-2F4F-462C-B9AB-559055C1DF9C", "The message was discarded, because the Status at Customs of the declaration is different from PRE and ACK. (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to {3}.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType, EDIMessage.Status.Discarded));
			}
		}

		protected override void ProcessMessageCore(BEMessage message, ICC028CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;

			var customsStatus = moveHeader.BM_CustomsStatus;
			if (string.IsNullOrEmpty(customsStatus)
				|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.Acknowledged
				|| customsStatus == NCTS5DepartureCustomsStatusList.Codes.PreLodged)
			{
				moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
				moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
				nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
				if (moveHeader.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.D)
				{
					moveHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
				}

				var movementReferenceNumber = nctsHeader.MovementReferenceEntryNumber;
				movementReferenceNumber.CE_EntryNum = messageDataProvider.MRN;
				moveHeader.BM_EntryDate = messageDataProvider.EntryDate;
			}
			message.EM_Status = EDIMessage.Status.ProcessedOK;

			if (nctsHeader.MovementHeader.IsSimplifiedNctsProcedure)
			{
				CreateCustomsRegistryNumber(nctsHeader);
			}

			if (moveHeader.BM_GONumber.IsEmpty)
			{
				NctsMessageHelper.RequestFOLFromCustoms(nctsHeader);
			}
		}

		static void CreateCustomsRegistryNumber(NctsHeader nctsHeader)
		{
			var representativeOrganisationPK = nctsHeader.MovementHeader.Representative.OrganisationPK;
			var principalOrganisationPK = nctsHeader.Principal.OrganisationPK;
			var organisationCode = ZString.Empty;

			CustomsRegistry customsRegistryItem = null;
			if (!representativeOrganisationPK.IsEmpty)
			{
				customsRegistryItem = RegistryHelper.GetValidCustomsRegistryForCompany(BECustomsRegistry.Instance.CustomsRegistry.Value, BERegistryDeclarationTypeList.Codes.TransitDeparture, representativeOrganisationPK);
				organisationCode = nctsHeader.MovementHeader.Representative.Organisation.OH_Code;
			}
			else
			{
				customsRegistryItem = RegistryHelper.GetValidCustomsRegistryForCompany(BECustomsRegistry.Instance.CustomsRegistry.Value, BERegistryDeclarationTypeList.Codes.TransitDeparture, principalOrganisationPK);
				organisationCode = nctsHeader.Principal.Organisation.OH_Code;
			}

			if (customsRegistryItem != null)
			{
				var registryNumber = nctsHeader.Factory.New<CusEntryNumberForCC028CMessage>();
				registryNumber.Parent = nctsHeader.MovementHeader;
				registryNumber.CE_EntryType = CusEntryNumberTypes.EU.CustomsRegistry;
				registryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
				registryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
				registryNumber.CE_EntryLineReference = BERegistryDeclarationTypeList.Codes.TransitDeparture + "-" + organisationCode;
				registryNumber.CE_IssueDate = customsRegistryItem.StartingDate;
			}
		}

		protected override bool CheckMessageSequenceIsValidCore(BEMessage message) => !(message.EM_LinkedObject is NctsDepartureMovementHeader nctsMovementHeader)
			|| (nctsMovementHeader.BM_CustomsStatus != ZString.Empty || nctsMovementHeader.BM_Phase != NctsMovementHeaderTransactionStatusList.Codes.Declaration);
	}
}
