using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE410;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class IE410Processor : DeltaIEBaseProcessor<CC410BType>
	{
		public IE410Processor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.InvalidationApprovalNotificationFeedback;

		protected override ZString GetNewMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetLRNFromResponseMessage(CC410BType messageObject) => messageObject.ImportOperation.FirstOrDefault()?.LRN;

		protected override ZString GetNewEntryStatus() => DeltaIEImportCusEntryStatusList.Codes.Invalidated;

		protected override void DoExtraProcessing(CusEntryHeader entryHeader, EDIMessage inboundMessage, bool isFirstTimeProcessing)
		{
			base.DoExtraProcessing(entryHeader, inboundMessage, isFirstTimeProcessing);

			UpdateTemporaryStorageRegisterIfApplicable(entryHeader, inboundMessage);
		}

		protected void UpdateTemporaryStorageRegisterIfApplicable(CusEntryHeader entryHeader, EDIMessage inboundMessage)
		{
			var processor = new CusTempStorageRegisterProcessor<EDIMessage>(entryHeader, Logger);
			try
			{
				processor.CalculateTransactionsAndLockMutexIfNeededForRollingBackTransaction();
				processor.AddTransactionsWhenSaving(inboundMessage);
			}
			finally
			{
				processor.UnlockRegistersMutexes();
			}
		}
	}
}
