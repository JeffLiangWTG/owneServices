using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC025C;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC025CMessageProcessor : NCTSMessageProcessor<ICC025CDataProvider>
	{
		public CC025CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.CC025C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC025C };

		protected override ICC025CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc025CType, CC025CDataProvider>();

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC025CDataProvider messageDataProvider) => NctsMessageHelper.LocateHeaderByMRN(message.Factory, messageDataProvider, NctsMoveHeaderType.Codes.Arrival);

		protected override void ProcessMessageCore(BEMessage message, ICC025CDataProvider messageDataProvider)
		{
			var nctsHeader = NctsMessageHelper.GetNctsHeaderFromLinkedObject(message);
			var moveHeader = nctsHeader.ArrivalMovementHeader;

			var customsStatus = string.Empty;
			switch (messageDataProvider.ReleaseIndicator)
			{
				case Constants.ReleaseIndicator.FullRelease:
					customsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
					break;
				case Constants.ReleaseIndicator.PartialRelease:
					customsStatus = NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionPartialRelease;
					break;
				case Constants.ReleaseIndicator.NoRelease:
					customsStatus = NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
					break;
				case Constants.ReleaseIndicator.PartialReleaseClosed:
					customsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease;
					break;
			}

			foreach (var houseConsignmentProvider in messageDataProvider.HouseConsignments)
			{
				foreach (var consignmentItemProvider in houseConsignmentProvider.ConsignmentItems)
				{
					foreach (var packageProvider in consignmentItemProvider.Packaging)
					{
						var package = NctsMessageHelper.RetrievePackageBySequencesForArrivalDeclaration(nctsHeader, houseConsignmentProvider.SequenceNumber, consignmentItemProvider.DeclarationSequenceNumber, packageProvider.SequenceNumber);
						if (package != null)
						{
							package.B5_UnitsReleased = packageProvider.NumberOfPackages;
						}
					}
				}
			}

			nctsHeader.MovementReferenceEntryNumber.CE_IssueDate = messageDataProvider.ReleaseDate;
			if (!customsStatus.IsEmpty())
			{
				moveHeader.CustomsEntryStatusLogAdded += MovementHeader_CustomsEntryStatusLogAdded;
				moveHeader.BM_CustomsStatus = customsStatus;
			}
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}

		void MovementHeader_CustomsEntryStatusLogAdded(object sender, EventArgs e)
		{
			if (sender is NctsArrivalMovementHeader moveHeader)
			{
				moveHeader.Header.LockFileIfEnabledByConfiguration(Res.GetString("5B9CCEBE-7648-41CD-B2E7-E19FAEEBE908", "The tab 'Unloading Remarks' and its depending tabs were locked when message 'Goods Release Notification' was received."), EUJobMessageTypeList.Codes.NctsArrivalUnloadingRemarks);
				moveHeader.CustomsEntryStatusLogAdded -= MovementHeader_CustomsEntryStatusLogAdded;
			}
		}

		protected override Type MessageInterpreterType => typeof(CC025CMessageInterpreter);
	}
}
