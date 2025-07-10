using System;
using System.Text;
using WTG.Serialization.DataScience.Audit.ObjectModel;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers
{
	[Serializable]
	public class OversizeChangeRecordException : Exception
	{
		public OversizeChangeRecordException(AuditRow auditRow, IDataScienceSubscriberToKafka subscriber, int actualMessageSize, int maxMessageSize, Exception innerException)
			: base(FormatExceptionMessage(auditRow, subscriber, actualMessageSize, maxMessageSize), innerException)
		{
		}

		public OversizeChangeRecordException(AuditRow auditRow, IDataScienceSubscriberToKafka subscriber, int actualMessageSize, int maxMessageSize)
			: base(FormatExceptionMessage(auditRow, subscriber, actualMessageSize,maxMessageSize))
		{
		}

#if NETFRAMEWORK
		protected OversizeChangeRecordException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
			: base(serializationInfo, streamingContext) { }
#endif

		static string FormatExceptionMessage(AuditRow auditRow, IDataScienceSubscriberToKafka subscriber, int actualMessageSize, int maxMessageSize)
		{
			var buffer = new StringBuilder();
			buffer.Append($"A single change record exceeded the maximum Kafka message size.");
			buffer.Append($" Expected message to be within {maxMessageSize} bytes, ");
			buffer.AppendLine($" message to produce is at least {actualMessageSize} bytes plus overheads).");
			buffer.AppendLine($"  Subscriber = [{subscriber.Table.SqlSchemaName}].[{subscriber.Table.TableName}]");
			buffer.AppendLine($"  Subscriber Code = {subscriber.Code}");
			buffer.AppendLine($"  LSN = {BitConverter.ToString(auditRow.StartLsn)}");
			buffer.AppendLine($"  SeqVal = {BitConverter.ToString(auditRow.SequenceValue)}");
			buffer.AppendLine($"  Operation = {auditRow.Operation}");
			if (auditRow.ColumnValues.TryGetValue(subscriber.Table.PK.Name, out var pk) && pk != null)
			{
				buffer.AppendLine($"  PK = {pk}");
			}

			return buffer.ToString();
		}
	}
}
