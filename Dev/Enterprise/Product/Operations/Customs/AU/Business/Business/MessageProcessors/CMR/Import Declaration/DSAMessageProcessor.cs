using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DSAMessageProcessor : BaseImportDeclarationMessageProcessor
	{
		public DSAMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.DSA, "Declaration Status Advice Message (DSA)")
		{
		}

		protected override bool IsUnsolicitedMessage
		{
			get { return true; }
		}

		#region IsDisabled
		bool IsDisabled => AUCustomsDataRegistry.Instance.DisableDSAMessages.Value;

		protected override string DoPreProcessingReturningStatus(EDIMessage message)
		{
			return IsDisabled ? EDIMessage.Status.Discarded : base.DoPreProcessingReturningStatus(message);
		}

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			return IsDisabled ? EDIMessage.Status.Discarded : base.DoProcessingReturningStatus(message);
		}

		protected override string MessageErrorCode
		{
			get { return IsDisabled ? EDIMessage.Status.Discarded : base.MessageErrorCode; }
		}
		#endregion

		protected override bool DoAdditionalProcessing()
		{
			var result = base.DoAdditionalProcessing();
			if (!result)
			{
				declaration = incomingMessage.EM_LinkedObject as JobDeclaration;
				if (declaration == null)
				{
					Logger.LogWarning("The incoming DSA message is not responding to either an entry header or declaration.  Can't continue.");
					result = false;
				}
				else
				{
					result = true;
					SetDeclarationNumber();
					SetDeclarationStatus();
				}
			}
			else
			{
				if (entryHeader != null && incomingMessage is CMRDSAMessage responseMessage)
				{
					var packingGroups = entryHeader.Packages.Cast<Package>().Select(x => x.PackingGroup).Distinct();
					if (consolidatedDeclaration != null)
					{
						foreach (JobDeclaration dec in consolidatedDeclaration.JobDeclarations)
						{
							if (dec.PK != declaration.PK)
							{
								var packingGroupsFromOtherDeclaration = dec.EntryHeader.Packages.Cast<Package>().Select(x => x.PackingGroup).Distinct();
								packingGroups = packingGroups.Union(packingGroupsFromOtherDeclaration);
							}
						}
					}
					foreach (var packingGroup in packingGroups)
					{
						packingGroup.StatusCalculator.DeriveStatusNow();
						var mostRecentMessage = packingGroup.ResetCacheAndGetMostRecentCARSTorDSAMessage();
						if (mostRecentMessage != null)
						{
							var abbreviatedCargoStatusDescriptionForTransportLine = packingGroup.GetAbbreviatedStatusDescriptionForTransportLine();
							var cargoStatusForTransportLine = packingGroup.GetCargoStatusFromLatestMessage();
							mostRecentMessage.ResetCUSRESCache();
						}
					}

					consolidatedDeclaration?.SyncStatusAfterMessageProcessing(responseMessage);
				}
			}

			return result;
		}

		protected override void SetEntryHeaderStatus()
		{
			if (cUSRES != null && entryHeader != null)
			{
				ZString newEntryStatus = GetStatus(incomingMessage.GetStatus());
				if (newEntryStatus != CMRImportEntryAdvice.Finalised.Code || entryHeader.CH_EntryStatus != CMRImportEntryAdvice.ATDReceived.Code)
				{
					entryHeader.CH_EntryStatus = newEntryStatus;
				}
				var declaration = entryHeader.Declaration;
				declaration.JE_EntryStatus = entryHeader.Declaration.SummaryEntryStatusCalculator.SummaryEntryStatus;
				if (declaration.IsWHSUniversalXMLActive && declaration.IsInwardBondedWarehousingEnabled
					&& newEntryStatus == CMRImportEntryAdvice.Finalised.Code && ShouldUpdateInward(declaration.WarehouseTransactionStatus))
				{
					incomingMessage.Factory.Saved -= Factory_Saved;
					incomingMessage.Factory.Saved += Factory_Saved;
					delayEmailReport = true;
				}
			}
		}

		protected override void SetMessageSubType()
		{
			incomingMessage.EM_MessageSubType = GetStatus(incomingMessage.GetStatus());
		}

		void SetDeclarationNumber()
		{
			CMRCUSRESMessage message = incomingMessage;
			if (message != null)
			{
				ZString declarationNumber = message.EntryNumber;
				if (!declarationNumber.IsEmpty)
				{
					declaration.DeclarationNumber = declarationNumber;
				}
			}
		}

		void SetDeclarationStatus()
		{
			if (!statusType.IsEmpty)
			{
				DeclarationStatusProcessor declarationStatusProcessor = new DeclarationStatusProcessor(declaration);
				declarationStatusProcessor.SetDeclarationStatus(statusType);
			}
		}
	}
}
