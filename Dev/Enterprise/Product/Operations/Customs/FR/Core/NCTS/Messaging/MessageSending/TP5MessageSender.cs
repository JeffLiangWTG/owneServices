using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class TP5MessageSender : MessageSender<TP5MessageSendingObject>, ITP5MessageSender
	{
		public TP5MessageSender(TP5MessageSendingObject tP5MessageSendingObject, ErrorCollector errorCollector) : base(tP5MessageSendingObject, errorCollector)
		{
		}

		protected override MessageBuilderManager<TP5MessageSendingObject> GetBuilderManager()
		{
			return new TP5MessageBuilderManager();
		}

		protected override bool PreSend()
		{
			var nctsHeader = objectToSend.NctsHeader;

			if (NctsHeaderDeclarationGoodsItemNumbersHelper.IsDeclarationGoodsItemNumbersNonSequential(nctsHeader))
			{
				NctsHeaderDeclarationGoodsItemNumbersHelper.SetDeclarationGoodsItemNumbersToZero(nctsHeader);
			}

			NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(nctsHeader);

			if (nctsHeader.IsPhase5Departure)
			{
				if (objectToSend.MessageType == TP5MessageTypeList.Codes.CC015C)
				{
					NctsMovementHeaderValuationDateHelper.SetValuationDate(nctsHeader.MovementHeader, false);
				}
				else if (objectToSend.MessageType == TP5MessageTypeList.Codes.CC013C)
				{
					NctsMovementHeaderValuationDateHelper.SetValuationDate(nctsHeader.MovementHeader, true);
				}
			}

			return base.PreSend();
		}

		protected override void PostCreateEdiMessage(EDIMessage message)
		{
			base.PostCreateEdiMessage(message);
			AddPermitsIfApplicable(message);
		}

		void AddPermitsIfApplicable(EDIMessage message)
		{
			var nctsHeader = (Business.NCTS.NctsHeader)objectToSend.NctsHeader;
			var permitProcessor = new NCTSCusPermitCusDecProcessor(nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader : nctsHeader, message);
			try
			{
				permitProcessor.AddPermitRecordsAndLockMutexIfNeeded();
				permitProcessor.AddPermitTransactions(message, msg => FRPermitHelper.GetPermitAppIdForMessage(message));
			}
			finally
			{
				permitProcessor?.UnlockPermitMutexes();
			}
		}
	}
}
