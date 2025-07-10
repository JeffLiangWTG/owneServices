using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class CusPollingTransactionMessageGenerator
	{
		public readonly LoggingInformation Logger;
		readonly BusinessObjectFactory factory;
		public CusPollingTransactionMessageGenerator(LoggingInformation logger, BusinessObjectFactory factory)
		{
			Logger = logger;
			this.factory = factory;
		}

		public void Create(CancellationToken token, ZGuid companyPk)
		{
			var openTransactions = GetQueuedCusPollingTransactions(companyPk);
			var messageCnt = ZInt.Zero;
			if (openTransactions.Length != 0)
			{
				foreach (var docTransaction in openTransactions)
				{
					token.ThrowIfCancellationRequested();

					var incomingDLTMessage = docTransaction.ParentDLTMessage;
					if (incomingDLTMessage != null)
					{
						var message = factory.New<EDIMessage>();
						message.EM_GB = incomingDLTMessage.EM_GB;
						message.EM_MessageType = Constants.EDIInterchangeType.DOC;
						message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
						message.EM_Status = EDIMessage.Status.Queued;
						message.EM_MessageData = MessageEncoding.UTF8WithoutBOM.GetBytes(docTransaction.CPT_TransactionID);
						message.EM_LinkedObject = docTransaction;
						messageCnt++;

						docTransaction.CPT_Status = Messaging.CusPollingTransactionStatusList.Codes.Closed;
					}
				}

				var inboundDLTPKs = openTransactions.Select(x => x.CPT_ParentID).Distinct();
				foreach (var inboundDLTPK in inboundDLTPKs)
				{
					SetMessageAndInterchangeStatusAsDiscardedIfRequired(inboundDLTPK);
				}
			}

			if (messageCnt > ZInt.Zero)
			{
				factory.Save();
				Logger.Log(Res.GetString("AFFC6610-0012-43D3-AC56-BD1DDB2F97BA", "{0} DOC message(s) has been created successfully.", messageCnt));
			}
		}

		void SetMessageAndInterchangeStatusAsDiscardedIfRequired(ZGuid inboundDLTMessagePK)
		{
			if (AreAllCusPollingTransactionsClosed(inboundDLTMessagePK))
			{
				var inDLTMessage = factory.Load<EDIMessage>(inboundDLTMessagePK);
				if (inDLTMessage != null)
				{
					var inDLTInterchange = inDLTMessage.Interchange;
					if (inDLTInterchange != null)
					{
						inDLTInterchange.SetMessageAndInterchangeStatusAsDiscarded();
						inDLTInterchange.GetOutgoingInterchange().SetMessageAndInterchangeStatusAsDiscarded();
					}
				}
			}
		}

		bool AreAllCusPollingTransactionsClosed(ZGuid parentID)
		{
			var transactions = factory.Load<CusPollingTransaction>(new ZQuery(CusPollingTransactionSchema.CPT_ParentID, parentID));
			return !transactions.Any(x => x.CPT_Status != CusPollingTransactionStatusList.Codes.Closed);
		}

		CusPollingTransaction[] GetQueuedCusPollingTransactions(ZGuid companyPk)
		{
			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
			branchQuery.AddToFilter(GlbBranchSchema.GB_GC, companyPk);

			var messageQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.PK);
			messageQuery.AddSubQuery(EDIMessageSchema.EM_GB, GlbBranchSchema.PK, branchQuery, JoinCondition.And);

			var zQuery = new ZDBOnlyQuery(typeof(CusPollingTransaction));
			zQuery.AddToFilter(CusPollingTransactionSchema.CPT_Status, Messaging.CusPollingTransactionStatusList.Codes.Opened);
			zQuery.AddToFilter(CusPollingTransactionSchema.CPT_ApplicationCode, EDIMessage.ApplicationCodes.KRCustoms);
			zQuery.AddSubQuery(CusPollingTransactionSchema.CPT_ParentID, EDIMessageSchema.PK, messageQuery, JoinCondition.And);

			return factory.Load<CusPollingTransaction>(zQuery);
		}
	}
}
