using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace WTG.Serialization.DataScience.Audit.ObjectModel
{
	public class AuditMessage
	{
		[JsonConstructor]
		protected AuditMessage(DateTimeOffset messageCreatedTime, ProducerDetails producerDetails, DataSource dataSource, IList<AuditRow> changes, string messageFormatVersion)
			: this(messageCreatedTime, producerDetails, dataSource, changes)
		{
			Debug.Assert(messageFormatVersion == MessageFormatVersion);
		}

		public AuditMessage(DateTimeOffset messageCreatedTime, ProducerDetails producerDetails, DataSource dataSource, IList<AuditRow> changes)
		{
			MessageCreatedTime = messageCreatedTime;
			ProducerDetails = producerDetails ?? throw new ArgumentNullException(nameof(producerDetails));
			DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
			Changes = changes ?? throw new ArgumentNullException(nameof(changes));
		}
		public AuditMessage(ProducerDetails producerDetails, DataSource dataSource, IList<AuditRow> changes) : this(DateTimeOffset.Now, producerDetails, dataSource, changes) { }

		public AuditMessage(ProducerDetails producerDetails, DataSource dataSource) : this(producerDetails, dataSource, new List<AuditRow>()) { }

		public string MessageFormatVersion { get; } = AuditMessageJsonSerializerFactory.MessageFormatVersion;
		public DateTimeOffset MessageCreatedTime { get; }
		public ProducerDetails ProducerDetails { get; }
		public DataSource DataSource { get; }
		public IList<AuditRow> Changes { get; }
	}

	public class ProducerDetails
	{
		public ProducerDetails(string producerName, string producerAssemblyVersion, string producerHost)
		{
			ProducerName = producerName ?? throw new ArgumentNullException(nameof(producerName));
			ProducerAssemblyVersion = producerAssemblyVersion ?? throw new ArgumentNullException(nameof(producerAssemblyVersion));
			ProducerHost = producerHost ?? throw new ArgumentNullException(nameof(producerHost));
		}

		public string ProducerName { get; }
		public string ProducerAssemblyVersion { get; }
		public string ProducerHost { get; }
	}

	public class DataSource
	{
		public DataSource(int dataSchemaVersion, string database, string server, string table, IReadOnlyList<ColumnInfo> columns)
		{
			DataSchemaVersion = dataSchemaVersion;
			Database = database ?? throw new ArgumentNullException(nameof(database));
			Server = server ?? throw new ArgumentNullException(nameof(server));
			Table = table ?? throw new ArgumentNullException(nameof(table));
			Columns = columns ?? throw new ArgumentNullException(nameof(columns));
		}

		public int DataSchemaVersion { get; }
		public string Database { get; }
		public string Server { get; }
		public string Table { get; }
		public IReadOnlyList<ColumnInfo> Columns { get; }
	}

	public class ColumnInfo
	{
		public ColumnInfo(string columnName, string sqlType, bool isNullable)
		{
			ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
			SqlType = sqlType ?? throw new ArgumentNullException(nameof(sqlType));
			IsNullable = isNullable;
		}

		public string ColumnName { get; }
		public string SqlType { get; }
		public bool IsNullable { get; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Json Converter requires a simple type")]
	public class AuditRow
	{
		public AuditRow(byte[] startLsn, byte[] sequenceValue, CdcOperation operation, DateTime transactionEndTimeUtc, IReadOnlyDictionary<string, object> columnValues)
		{
			StartLsn = startLsn ?? throw new ArgumentNullException(nameof(startLsn));
			SequenceValue = sequenceValue ?? throw new ArgumentNullException(nameof(sequenceValue));
			Operation = operation;
			TransactionEndTimeUtc = transactionEndTimeUtc;
			ColumnValues = columnValues ?? throw new ArgumentNullException(nameof(columnValues));
		}

		[JsonConverter(typeof(HexByteArrayConverter))]
		public byte[] StartLsn { get; }

		[JsonConverter(typeof(HexByteArrayConverter))]
		public byte[] SequenceValue { get; }

		public CdcOperation Operation { get; }
		public DateTime TransactionEndTimeUtc { get; }
		public IReadOnlyDictionary<string, object> ColumnValues { get; }
	}
}
