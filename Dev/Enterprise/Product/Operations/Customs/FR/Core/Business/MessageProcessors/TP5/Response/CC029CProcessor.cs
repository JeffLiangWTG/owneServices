using System.Globalization;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.TP5;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC029C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CC029CProcessor : TP5BaseProcessor<Cc029CType>
	{
		public CC029CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.ReleasedForTransit;

		protected override ZString GetMRNFromResponseMessage(Cc029CType messageObject) => messageObject.TransitOperation?.Mrn ?? ZString.Empty;

		protected override ZString GetNewDepartureStatus(Cc029CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

		protected override ZDateTime GetNewReleaseDateFromResponseMessage(Cc029CType messageObject) => messageObject.TransitOperation?.ReleaseDate ?? ZDateTime.Empty;

		protected override ZString GetNewMessageStatus(Cc029CType messageObject) => LogicalStatusList.Codes.Accepted;

		protected override ZString GetNewPhase(Cc029CType messageObject) => EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.Declaration;

		protected override void DoExtraProcessing(NctsHeader header, Cc029CType messageObject, EDIMessage inboundMessage)
		{
			base.DoExtraProcessing(header, messageObject, inboundMessage);

			UpdateTemporaryStorageRegisterIfApplicable(header, inboundMessage);
		}

		protected void UpdateTemporaryStorageRegisterIfApplicable(NctsHeader header, EDIMessage inboundMessage)
		{
			if (header?.MovementHeader != null)
			{
				foreach (var bill in header.Bills.Cast<NctsBill>())
				{
					foreach (var goodsItem in bill.GoodsItems.Cast<NctsDepartureCargoDesc>())
					{
						var processor = new CusTempStorageRegisterProcessor<EDIMessage>(goodsItem.TemporaryStorageRegisterTransactionDataProvider, Logger, notifierWhenInsufficient: Notifier);
						try
						{
							processor.CalculateTransactionsAndLockMutexIfNeededForAddingTransaction();
							processor.AddTransactionsWhenSaving(inboundMessage);
						}
						finally
						{
							processor.UnlockRegistersMutexes();
						}

						void Notifier(ZString failingRegister)
						{
							Logger.LogWarning(string.Format(CultureInfo.InvariantCulture, (NoResString)"Warning : Processing received message #{0}, type {1} {2}. Temporary Storage {3} has not been updated by House Consignment #{4}", inboundMessage.EM_MessageNum, inboundMessage.EM_MessageType, inboundMessage.EM_MessageSubType, failingRegister, bill.SequenceNumber));

							header.Logs.AddNew(Events.DeclarationHasErrors, string.Format("Temporary Storage {0} has not been updated by House Consignment #{1}", failingRegister, bill.SequenceNumber), ZDateTimeOffset.UtcNow);

							var body = Res.GetString("A2AD8770-0FEF-4D1F-AAB0-532607D7D11A", "Temporary Storage {0} has not been updated by House Consignment #{1}. There are not enough packages remaining", failingRegister, bill.SequenceNumber);

							var jobReferenceNumber = header.MovementHeader?.BM_PaperlessInbondNum;
							Business.MessageProcessors.ProcessorHelper.SendEmail(header.Factory, header.BH_SystemCreateUser, true, GetEmailBody(jobReferenceNumber, body), GetEmailsubject(jobReferenceNumber), GetEmailGroupRegistryItem());
						}
					}
				}
			}
		}

		protected override void UpdateGuaranteeTransactionsIfNeeded(NctsHeader header, Cc029CType messageObject, EDIMessage inboundMessage)
		{
			var messageGuarantees = messageObject?.Guarantee;
			if (messageGuarantees != null && messageGuarantees.Any())
			{
				var movementHeader = header.MovementHeader;
				var messageGRNs = messageGuarantees.Select(g => g.GuaranteeReference.Grn).ToHashSet();

				foreach (var guarantee in header.GetEffectiveGuarantees().Cast<FRNctsGuarantee>().ToList())
				{
					if (!messageGRNs.Contains(guarantee.GuaranteeReferenceNumber))
					{
						header.GetEffectiveGuarantees().RemoveAndDelete(guarantee);
					}
				}

				var outgoingMessageAppId = FRPermitHelper.GetPermitAppIdForMessage(header.GetOutgoingMessage(inboundMessage));
				movementHeader.GuaranteeTransactionCoordinator.ConfirmValidAndDeleteInvalidTransactions();

				foreach (var messageGuarantee in messageGuarantees)
				{
					if (messageGuarantee?.GuaranteeReference != null)
					{
						var amountToBeCovered = GetGuaranteeAmountToBeCovered(messageGuarantee);
						if (amountToBeCovered != null)
						{
							var nctsGuarantee = header.GetEffectiveGuarantees().Cast<FRNctsGuarantee>().FirstOrDefault(x => x.GuaranteeReferenceNumber == messageGuarantee.GuaranteeReference.Grn);
							movementHeader.GuaranteeTransactionCoordinator.UpdateGuaranteeAmount(nctsGuarantee, outgoingMessageAppId, amountToBeCovered);
						}
					}
				}
			}
		}

		Money GetGuaranteeAmountToBeCovered(GuaranteeType03 guarantee)
		{
			var guaranteeReference = guarantee.GuaranteeReference;
			if (guaranteeReference?.AmountToBeCovered == null || guaranteeReference.AmountToBeCovered == 0)
			{
				return null;
			}
			return new Money(guaranteeReference.AmountToBeCovered, new ZArchitecture.Environment.Currency(guaranteeReference.Currency));
		}
	}
}
