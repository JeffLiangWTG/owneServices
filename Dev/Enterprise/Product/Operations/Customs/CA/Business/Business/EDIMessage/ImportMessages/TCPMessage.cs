using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class TCPMessage : EDIMessageWithBatchNumber
	{
		public TCPMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			EM_MessageType = MessageTypeList.Codes.TradeChainPartner;
		}

		public override EDIMessage OriginalMessage
		{
			get
			{
				if (IsTransmitMessage)
				{
					throw new InvalidOperationException("Original Message is only available for response messages. This message is a transmit");
				}
				var batchNumber = BatchNumber;
				if (fOriginalMessage == null && !batchNumber.IsEmpty)
				{
					var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAIMP);
					query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.TradeChainPartner);
					query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
					query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Sent);
					query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.LessThan, EM_SystemCreateTimeUtc);
					query.AddToFilter(EDIMessageSchema.EM_MessageNum, batchNumber);
					query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " DESC";
					const string indexName = "NR_RX__EM_MessageNum";
					query.TableIndexHints.Add(new TableIndexHint(indexName));
					fOriginalMessage = Factory.LoadTop1<EDIMessageWithBatchNumber>(query);
				}
				return fOriginalMessage;
			}
		}
		EDIMessage fOriginalMessage;

		protected override bool ShouldUseUnformattedMessageText => false;
	}
}
