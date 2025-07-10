using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Schema;
using Confluent.Kafka;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.EConversation.Testing;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.Serialization.DataScience.Audit.ObjectModel;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests
{
	class DataScienceSubscriberToKafkaBaseTest : TestCase
	{
		static Dictionary<string, (DataTable AssertChangeTable, DataTable ActChangeTable)> ChangeTables
		{
			get => changeTables ?? (changeTables = new Dictionary<string, DataTable>
			{
				["insert 1"] = new Func<DataTable>(() =>
				{
					var changeTable = GetChangeDataTable();
					var changeRow = changeTable.NewRow();
					changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
					changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02 };
					changeRow[AuditFieldNames.OperationFieldName] = CdcOperation.Insert;
					changeRow[AuditFieldNames.TranEndTimeUtc] = new DateTime(2022, 1, 1);
					changeRow["TT_PK"] = new Guid("D6878F6B-DD30-492A-AA19-D94DDF21BAE6");
					changeRow["TT_Int"] = 0;
					changeTable.Rows.Add(changeRow);
					return changeTable;
				})(),
				["insert 3"] = new Func<DataTable>(() =>
				{
					var changeTable = GetChangeDataTable();
					var changeRow = changeTable.NewRow();
					changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
					changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02 };
					changeRow[AuditFieldNames.OperationFieldName] = CdcOperation.Insert;
					changeRow[AuditFieldNames.TranEndTimeUtc] = new DateTime(2022, 1, 1, 0, 0, 0);
					changeRow["TT_PK"] = new Guid("89EB010B-DCCB-40CF-9777-D5CD91BF608C");
					changeRow["TT_Int"] = 0;
					changeTable.Rows.Add(changeRow);
					changeRow = changeTable.NewRow();
					changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03 };
					changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04 };
					changeRow[AuditFieldNames.OperationFieldName] = CdcOperation.Insert;
					changeRow[AuditFieldNames.TranEndTimeUtc] = new DateTime(2022, 1, 1, 0, 0, 1);
					changeRow["TT_PK"] = new Guid("5E9384E1-460B-4156-9E44-7BBAE1F3CCDC");
					changeRow["TT_Int"] = 1;
					changeTable.Rows.Add(changeRow);
					changeRow = changeTable.NewRow();
					changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05 };
					changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06 };
					changeRow[AuditFieldNames.OperationFieldName] = CdcOperation.Insert;
					changeRow[AuditFieldNames.TranEndTimeUtc] = new DateTime(2022, 1, 1, 0, 0, 2);
					changeRow["TT_PK"] = new Guid("0B03FB9B-8C90-474F-AFC5-96836A374056");
					changeRow["TT_Int"] = 1;
					changeTable.Rows.Add(changeRow);
					return changeTable;
				})(),
				["delete 1"] = new Func<DataTable>(() =>
				{
					var changeTable = GetChangeDataTable();
					var changeRow = changeTable.NewRow();
					changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
					changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02 };
					changeRow[AuditFieldNames.OperationFieldName] = CdcOperation.Delete;
					changeRow[AuditFieldNames.TranEndTimeUtc] = new DateTime(2022, 1, 1);
					changeRow["TT_PK"] = new Guid("D6878F6B-DD30-492A-AA19-D94DDF21BAE6");
					changeRow["TT_Int"] = 0;
					changeTable.Rows.Add(changeRow);
					changeTable.AcceptChanges();
					changeRow.Delete();
					return changeTable;
				})(),
				["update 1"] = new Func<DataTable>(() =>
				{
					var changeTable = GetChangeDataTable();
					var changeRow = changeTable.NewRow();
					changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
					changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02 };
					changeRow[AuditFieldNames.OperationFieldName] = CdcOperation.PreUpdate;
					changeRow[AuditFieldNames.TranEndTimeUtc] = new DateTime(2022, 1, 1);
					changeRow["TT_PK"] = new Guid("D6878F6B-DD30-492A-AA19-D94DDF21BAE6");
					changeRow["TT_Int"] = 0;
					changeTable.Rows.Add(changeRow);
					changeTable.AcceptChanges();
					changeRow["TT_Int"] = 1;
					return changeTable;
				})(),
			}.ToDictionary(kvp => kvp.Key, kvp => (AssertChangeTable: kvp.Value, ActChangeTable: kvp.Value.Copy())));

			set => changeTables = value;
		}
		static Dictionary<string, (DataTable AssertChangeTable, DataTable ActChangeTable)> changeTables;

		DataScienceSubscriberToKafkaBase Subscriber { get; set; }

		Mock<ITableSchema> TableMock { get; set; }
		Mock<ISubscriberDataSchema> SubscriberDataSchemaMock { get; set; }
		Mock<IProducer<string, string>> KafkaProducerMock { get; set; }
		List<JObject> Produced { get; set; }

		protected override void FinalTearDown()
		{
			base.FinalTearDown();

			ChangeTables = null;
		}

		protected override void SetUp()
		{
			base.SetUp();

			TableMock = new Mock<ITableSchema>();
			TableMock.Setup(t => t.TableName).Returns("TestTable");
			TableMock.Setup(t => t.SqlSchemaName).Returns("dbo");

			SubscriberDataSchemaMock = new Mock<ISubscriberDataSchema>();
			SubscriberDataSchemaMock.Setup(s => s.BizObjColumns)
				.Returns(new SchemaColumn[]
				{
					new SchemaPKColumn(TableMock.Object, "TT_PK", false),
					new SchemaIntColumn(TableMock.Object, "TT_Int", 0, 0, false),
				});
			SubscriberDataSchemaMock.Setup(s => s.LegacyColumns)
				.Returns(Array.Empty<SchemaColumn>());
			SubscriberDataSchemaMock.Setup(s => s.NonBizObjColumns).Returns(Enumerable.Empty<ColumnInfo>());

			KafkaProducerMock = new Mock<IProducer<string, string>>();
			Produced = new List<JObject>();
			KafkaProducerMock.Setup(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null))
				.Callback((string topic_, Message<string, string> message, Action<DeliveryReport<string, string>> deliveryReports_) =>
					Produced.Add(JObject.Parse(message.Value)));
			KafkaProducerMock.Setup(p => p.Flush(It.IsAny<TimeSpan>())).Returns(0);
			Subscriber = new SubscriberToKafkaForTest(TableMock.Object, SubscriberDataSchemaMock.Object, KafkaProducerMock.Object);
		}

		public void TestSpecificColumnsIsNull()
		{
			AssertNull(
				"Specific columns should be null, otherwise we might miss legacy column data.\n" +
					"In the scenario where a DB transformation deletes a column before all changes are processed,\n" +
					"the deleted column cannot be part of SpecificColumns.\n" +
					"Changes in the deleted column can then be missed as they are not tracked by ActualDataChangesAuditSubscriber.\n" +
					"(`SpecificColumns == null` has a consequence that a subscriber with a narrow column interest may receive\n" +
					"updates that don't affect the columns of interest, however, this is better than\n" +
					"potentially losing data and Data Science subscribers are generally designed to track\n" +
					"the vast majority of columns.)",
				Subscriber.SpecificColumns);
		}

		public void TestColumnInfos_NoLegacyColumns()
		{
			CombineAssertions(() =>
			{
				AssertEquals(2, Subscriber.ColumnInfos.Count);
				AssertEquals("TT_PK", Subscriber.ColumnInfos[0].ColumnName);
				AssertEquals("uniqueidentifier", Subscriber.ColumnInfos[0].SqlType);
				AssertEquals(false, Subscriber.ColumnInfos[0].IsNullable);
				AssertEquals("TT_Int", Subscriber.ColumnInfos[1].ColumnName);
				AssertEquals("int", Subscriber.ColumnInfos[1].SqlType);
				AssertEquals(false, Subscriber.ColumnInfos[1].IsNullable);
			});
		}
		public void TestColumnInfos_WithLegacyColumns()
		{
			// Arrange
			SubscriberDataSchemaMock.Setup(s => s.BizObjColumns)
				.Returns(new SchemaColumn[]
				{
					new SchemaPKColumn(TableMock.Object, "TT_PK", false),
				});
			SubscriberDataSchemaMock.Setup(s => s.LegacyColumns)
				.Returns(new SchemaColumn[]
				{
					new SchemaIntColumn(TableMock.Object, "TT_Int", 0, 0, false),
				});

			// Act / Assert
			CombineAssertions(() =>
			{
				AssertEquals(2, Subscriber.ColumnInfos.Count);
				AssertEquals("TT_PK", Subscriber.ColumnInfos[0].ColumnName);
				AssertEquals("uniqueidentifier", Subscriber.ColumnInfos[0].SqlType);
				AssertEquals(false, Subscriber.ColumnInfos[0].IsNullable);
				AssertEquals("TT_Int", Subscriber.ColumnInfos[1].ColumnName);
				AssertEquals("int", Subscriber.ColumnInfos[1].SqlType);
				AssertEquals(false, Subscriber.ColumnInfos[1].IsNullable);
			});
		}

		public void TestColumnInfos_WithNonBizObjColumns()
		{
			// Arrange
			SubscriberDataSchemaMock.Setup(s => s.NonBizObjColumns).Returns(
				new ColumnInfo[]
				{
					new ColumnInfo("TT_NonBizObjColumn1", "smallint", isNullable: true),
					new ColumnInfo("TT_NonBizObjColumn2", "bit", isNullable: false),
				});

			// Act
			var columnInfos = Subscriber.ColumnInfos;

			// Asserts
			var nonBizObjColumn1Info = columnInfos.Single(ci => ci.ColumnName == "TT_NonBizObjColumn1");
			AssertEquals(expected: "smallint", actual: nonBizObjColumn1Info.SqlType);
			AssertEquals(expected: true, actual: nonBizObjColumn1Info.IsNullable);

			var nonBizObjColumn2Info = columnInfos.Single(ci => ci.ColumnName == "TT_NonBizObjColumn2");
			AssertEquals(expected: "bit", actual: nonBizObjColumn2Info.SqlType);
			AssertEquals(expected: false, actual: nonBizObjColumn2Info.IsNullable);
		}

		public void TestTransactionCommitted()
		{
			// Arrange
			Subscriber = new SubscriberToKafkaForTest(TableMock.Object, SubscriberDataSchemaMock.Object, KafkaProducerMock.Object, useTransactions: true);
			var (_, actChangeTable) = ChangeTables["insert 1"];

			var sequence = new MockSequence();
			KafkaProducerMock.InSequence(sequence).Setup(p => p.BeginTransaction());
			KafkaProducerMock.InSequence(sequence).Setup(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null));
			KafkaProducerMock.InSequence(sequence).Setup(p => p.CommitTransaction());

			// Act
			Subscriber.ProcessChanges(new LoggerForTest(), actChangeTable);

			// Assert
			KafkaProducerMock.Verify(p => p.BeginTransaction(), Times.Once());
			KafkaProducerMock.Verify(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null), Times.Once());
			KafkaProducerMock.Verify(p => p.CommitTransaction(), Times.Once());

			KafkaProducerMock.Verify(p => p.Flush(It.IsAny<TimeSpan>()), Times.Once());

			KafkaProducerMock.Verify(p => p.AbortTransaction(), Times.Never());
			Assert(true);
		}

		public void TestKafkaFlushTimeoutUsingTransaction()
		{
			// Arrange
			KafkaProducerMock.Setup(p => p.Flush(It.IsAny<TimeSpan>())).Returns(1);
			Subscriber = new SubscriberToKafkaForTest(TableMock.Object, SubscriberDataSchemaMock.Object, KafkaProducerMock.Object, useTransactions: true);
			var (_, actChangeTable) = ChangeTables["insert 1"];

			var sequence = new MockSequence();
			KafkaProducerMock.InSequence(sequence).Setup(p => p.BeginTransaction());
			KafkaProducerMock.InSequence(sequence).Setup(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null));
			KafkaProducerMock.InSequence(sequence).Setup(p => p.CommitTransaction());

			// Act
			AssertExceptionThrown(typeof(KafkaFlushTimeoutException), () => Subscriber.ProcessChanges(new LoggerForTest(), actChangeTable));

			// Assert
			KafkaProducerMock.Verify(p => p.BeginTransaction(), Times.Once());
			KafkaProducerMock.Verify(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null), Times.Once());
			KafkaProducerMock.Verify(p => p.CommitTransaction(), Times.Once());

			KafkaProducerMock.Verify(p => p.Flush(It.IsAny<TimeSpan>()), Times.Once());

			KafkaProducerMock.Verify(p => p.AbortTransaction(), Times.Never());
			Assert(true);
		}

		public void TestKafkaFlushTimeoutUsingTransactionWhenCommitFails()
		{
			// Arrange
			KafkaProducerMock.Setup(p => p.Flush(It.IsAny<TimeSpan>())).Returns(1);
			Subscriber = new SubscriberToKafkaForTest(TableMock.Object, SubscriberDataSchemaMock.Object, KafkaProducerMock.Object, useTransactions: true);
			var (_, actChangeTable) = ChangeTables["insert 1"];

			var sequence = new MockSequence();
			KafkaProducerMock.InSequence(sequence).Setup(p => p.BeginTransaction());
			KafkaProducerMock.InSequence(sequence).Setup(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null));
			KafkaProducerMock.InSequence(sequence).Setup(p => p.CommitTransaction()).Throws(() => new KafkaException(ErrorCode.UnsupportedForMessageFormat));

			// Act
			AssertExceptionThrown(typeof(KafkaException), () => Subscriber.ProcessChanges(new LoggerForTest(), actChangeTable));

			// Assert
			KafkaProducerMock.Verify(p => p.BeginTransaction(), Times.Once());
			KafkaProducerMock.Verify(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null), Times.Once());
			KafkaProducerMock.Verify(p => p.CommitTransaction(), Times.Once());
			KafkaProducerMock.Verify(p => p.Flush(It.IsAny<TimeSpan>()), Times.Never());
			KafkaProducerMock.Verify(p => p.AbortTransaction(), Times.Never());
			Assert(true);
		}

		public void TestKafkaFlushTimeoutUsingTransactionWithNothingProduced()
		{
			// Arrange
			KafkaProducerMock.Setup(p => p.Flush(It.IsAny<TimeSpan>())).Returns(1);
			Subscriber = new SubscriberToKafkaForTest(TableMock.Object, SubscriberDataSchemaMock.Object, KafkaProducerMock.Object, useTransactions: true);
			var actChangeTable = GetChangeDataTable();

			var sequence = new MockSequence();
			KafkaProducerMock.InSequence(sequence).Setup(p => p.BeginTransaction());
			KafkaProducerMock.InSequence(sequence).Setup(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null));
			KafkaProducerMock.InSequence(sequence).Setup(p => p.AbortTransaction());

			// Act
			AssertExceptionThrown(typeof(KafkaFlushTimeoutException), () => Subscriber.ProcessChanges(new LoggerForTest(), actChangeTable));

			// Assert
			KafkaProducerMock.Verify(p => p.BeginTransaction(), Times.Once());
			KafkaProducerMock.Verify(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null), Times.Never());
			KafkaProducerMock.Verify(p => p.AbortTransaction(), Times.Once());
			KafkaProducerMock.Verify(p => p.Flush(It.IsAny<TimeSpan>()), Times.Once());
			Assert(true);
		}

		public void TestKafkaFlushTimeoutNotUsingTransaction()
		{
			// Arrange
			KafkaProducerMock.Setup(p => p.Flush(It.IsAny<TimeSpan>())).Returns(1);
			Subscriber = new SubscriberToKafkaForTest(TableMock.Object, SubscriberDataSchemaMock.Object, KafkaProducerMock.Object, useTransactions: false);
			var (_, actChangeTable) = ChangeTables["insert 1"];
			KafkaProducerMock.Setup(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null));

			// Act
			AssertExceptionThrown(typeof(KafkaFlushTimeoutException), () => Subscriber.ProcessChanges(new LoggerForTest(), actChangeTable));

			// Assert
			KafkaProducerMock.Verify(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null), Times.Once());
			KafkaProducerMock.Verify(p => p.Flush(It.IsAny<TimeSpan>()), Times.Once());
			Assert(true);
		}

		public void TestTransactionAborted_WhenError()
		{
			// Arrange
			Subscriber = new SubscriberToKafkaForTest(TableMock.Object, SubscriberDataSchemaMock.Object, KafkaProducerMock.Object, useTransactions: true);
			var (_, actChangeTable) = ChangeTables["insert 1"];

			var sequence = new MockSequence();
			KafkaProducerMock.InSequence(sequence).Setup(p => p.BeginTransaction());
			KafkaProducerMock.InSequence(sequence).Setup(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null))
				.Callback(() => throw new InvalidOperationException());
			KafkaProducerMock.InSequence(sequence).Setup(p => p.AbortTransaction());

			// Act / Assert
			AssertExceptionThrown(typeof(InvalidOperationException), () => Subscriber.ProcessChanges(new LoggerForTest(), actChangeTable));

			// Assert
			KafkaProducerMock.Verify(p => p.BeginTransaction(), Times.Once());
			KafkaProducerMock.Verify(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null), Times.Once());
			KafkaProducerMock.Verify(p => p.AbortTransaction(), Times.Once());

			KafkaProducerMock.Verify(p => p.CommitTransaction(), Times.Never());
			KafkaProducerMock.Verify(p => p.Flush(It.IsAny<TimeSpan>()), Times.Never());
		}

		public void TestTransactionsNotUsed_WhenTransactionsOff()
		{
			// Arrange
			var (_, actChangeTable) = ChangeTables["insert 1"];

			var sequence = new MockSequence();
			KafkaProducerMock.InSequence(sequence).Setup(p => p.BeginTransaction());
			KafkaProducerMock.InSequence(sequence).Setup(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null));
			KafkaProducerMock.InSequence(sequence).Setup(p => p.CommitTransaction());

			// Act
			Subscriber.ProcessChanges(new LoggerForTest(), actChangeTable);

			// Assert
			KafkaProducerMock.Verify(p => p.BeginTransaction(), Times.Never());
			KafkaProducerMock.Verify(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null), Times.Once());
			KafkaProducerMock.Verify(p => p.CommitTransaction(), Times.Never());

			KafkaProducerMock.Verify(p => p.AbortTransaction(), Times.Never());
			KafkaProducerMock.Verify(p => p.Flush(It.IsAny<TimeSpan>()), Times.Once());
			Assert(true);
		}

		public void TestTransactionsNotUsed_WhenTransactionsOff_Error()
		{
			// Arrange
			var (_, actChangeTable) = ChangeTables["insert 1"];
			KafkaProducerMock.Setup(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null))
				.Callback(() => throw new InvalidOperationException());

			// Act / Assert
			AssertExceptionThrown(typeof(InvalidOperationException), () => Subscriber.ProcessChanges(new LoggerForTest(), actChangeTable));

			// Assert
			KafkaProducerMock.Verify(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null), Times.Once());

			KafkaProducerMock.Verify(p => p.BeginTransaction(), Times.Never());
			KafkaProducerMock.Verify(p => p.CommitTransaction(), Times.Never());
			KafkaProducerMock.Verify(p => p.AbortTransaction(), Times.Never());
			KafkaProducerMock.Verify(p => p.Flush(It.IsAny<TimeSpan>()), Times.Never());
		}

		public void TestProduce2BatchesAndStringWriterIsReset()
		{
			// Arrange
			var subscriber = new SubscriberToKafkaForTest(TableMock.Object, SubscriberDataSchemaMock.Object, KafkaProducerMock.Object, maxChangesCount: 2);
			var (_, actChangeTable) = ChangeTables["insert 3"];

			// Act
			subscriber.ProcessChanges(new LoggerForTest(), actChangeTable);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(2, Produced.Count);
				AssertEquals(2, (Produced[0]["changes"] as JArray).Count);
				AssertEquals("0x00000000000000000001", (string)(Produced[0]["changes"] as JArray)[0]["startLsn"]);
				AssertEquals("0x00000000000000000002", (string)(Produced[0]["changes"] as JArray)[0]["sequenceValue"]);
				AssertEquals("Insert", (string)(Produced[0]["changes"] as JArray)[0]["operation"]);
				AssertEquals("0x00000000000000000003", (string)(Produced[0]["changes"] as JArray)[1]["startLsn"]);
				AssertEquals("0x00000000000000000004", (string)(Produced[0]["changes"] as JArray)[1]["sequenceValue"]);
				AssertEquals("Insert", (string)(Produced[0]["changes"] as JArray)[0]["operation"]);
				AssertEquals(1, (Produced[1]["changes"] as JArray).Count);
				AssertEquals("0x00000000000000000005", (string)(Produced[1]["changes"] as JArray)[0]["startLsn"]);
				AssertEquals("0x00000000000000000006", (string)(Produced[1]["changes"] as JArray)[0]["sequenceValue"]);
				AssertEquals("Insert", (string)(Produced[0]["changes"] as JArray)[0]["operation"]);
			});
		}

		public void TestDeletedRow()
		{
			// Arrange
			var (_, actChangeTable) = ChangeTables["delete 1"];

			// Act
			Subscriber.ProcessChanges(new LoggerForTest(), actChangeTable);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(1, Produced.Count);
				AssertEquals(1, (Produced[0]["changes"] as JArray).Count);
				AssertEquals("0x00000000000000000001", (string)(Produced[0]["changes"] as JArray)[0]["startLsn"]);
				AssertEquals("0x00000000000000000002", (string)(Produced[0]["changes"] as JArray)[0]["sequenceValue"]);
				AssertEquals("Delete", (string)(Produced[0]["changes"] as JArray)[0]["operation"]);
			});
		}

		public void TestUpdatedRow()
		{
			// Arrange
			var (_, actChangeTable) = ChangeTables["update 1"];

			// Act
			Subscriber.ProcessChanges(new LoggerForTest(), actChangeTable);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(1, Produced.Count);
				AssertEquals(2, (Produced[0]["changes"] as JArray).Count);
				AssertEquals("0x00000000000000000001", (string)(Produced[0]["changes"] as JArray)[0]["startLsn"]);
				AssertEquals("0x00000000000000000002", (string)(Produced[0]["changes"] as JArray)[0]["sequenceValue"]);
				AssertEquals("PreUpdate", (string)(Produced[0]["changes"] as JArray)[0]["operation"]);
				AssertEquals(0, (int)(Produced[0]["changes"] as JArray)[0]["columnValues"]["TT_Int"]);
				AssertEquals("0x00000000000000000001", (string)(Produced[0]["changes"] as JArray)[1]["startLsn"]);
				AssertEquals("0x00000000000000000002", (string)(Produced[0]["changes"] as JArray)[1]["sequenceValue"]);
				AssertEquals("PostUpdate", (string)(Produced[0]["changes"] as JArray)[1]["operation"]);
				AssertEquals(1, (int)(Produced[0]["changes"] as JArray)[1]["columnValues"]["TT_Int"]);
			});
		}

		public void TestKafkaRegistryConfigInstancesShareSingletonKafkaProducer()
		{
			// Arrange
			var logger = new TestServiceLogger();
			var configs1 = new TestKafkaConfigs();
			var configs2 = new TestKafkaConfigs();

			var producer1 = configs1.GetOrCreateProducer(logger);
			var producer2 = configs2.GetOrCreateProducer(logger);

			// Assert
			Assert(!ReferenceEquals(configs1, configs2));
			Assert(ReferenceEquals(producer1, producer2));
		}

		public void TestKafkaRegistryConfigInstancesShareSingletonKafkaProducerMultiThreaded()
		{
			// Arrange
			var logger = new TestServiceLogger();
			var producerSource1 = new TaskCompletionSource<IProducer<string, string>>();
			var producerSource2 = new TaskCompletionSource<IProducer<string, string>>();

			var thread1 = new Thread(() => producerSource1.SetResult(new TestKafkaConfigs().GetOrCreateProducer(logger)));
			var thread2 = new Thread(() => producerSource2.SetResult(new TestKafkaConfigs().GetOrCreateProducer(logger)));

			// Act
			thread1.Start();
			thread2.Start();
			thread1.Join();
			thread2.Join();

			var producer1 = producerSource1.Task.Result;
			var producer2 = producerSource2.Task.Result;

			// Assert
			Assert(ReferenceEquals(producer1, producer2));
		}

		public void TestKafkaRegistryConfigInstanceCreatesNewKafkaProducerAfterConfigChange()
		{
			// Arrange
			var logger = new TestServiceLogger();
			var config1 = new TestKafkaConfigs();
			var config2 = new TestKafkaConfigs("different-bootstrap-servers");

			var producer1 = config1.GetOrCreateProducer(logger);
			var producer2 = config2.GetOrCreateProducer(logger);

			// Assert
			Assert(config1.PeekBootstrapServers() != config2.PeekBootstrapServers());
			Assert(!ReferenceEquals(producer1, producer2));
		}

#nullable enable
		public void TestMultipleKafkaSubscribersShareExactlyOneKafkaProducer()
		{
			// Arrange
			var logger = new TestServiceLogger();
			IProducer<string, string>? previousKafkaProducer = null;
			var (_, actChangeTable) = ChangeTables["insert 1"];

			for (int i = 0; i < 5; i++)
			{
				// Act
				var config = new TestKafkaConfigs();
				var kafkaProducer = config.GetOrCreateProducer(logger);
				var subscriber = new SubscriberToKafkaForTest(TableMock.Object, SubscriberDataSchemaMock.Object, kafkaProducer);
				subscriber.ProcessChanges(new LoggerForTest(), actChangeTable);

				//	Assert
				Assert(kafkaProducer is not null);
				if (i > 0)
				{
					Assert(ReferenceEquals(kafkaProducer, previousKafkaProducer));
				}

				previousKafkaProducer = kafkaProducer;
			}
		}
#nullable disable

		static DataTable GetChangeDataTable()
		{
			var changeTable = new DataTable();
			changeTable.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.OperationFieldName, typeof(int));
			changeTable.Columns.Add(AuditFieldNames.TranEndTimeUtc, typeof(DateTime));
			changeTable.Columns.Add("TT_PK", typeof(Guid));
			changeTable.Columns.Add("TT_Int", typeof(int));
			return changeTable;
		}
	}

	class DataScienceSubscriberToKafkaBaseMessagingTest : TestCase
	{
		[Immutable]
		class TestTableSchema : ITableSchema
		{
			public string SqlSchemaName => "dbo";
			public string TableName => "TestTable";

			public static readonly TestTableSchema Instance = new ();
			public static readonly SchemaPKColumn PK = new (Instance, "TT_PK", true);
			public static readonly SchemaStringColumn TT_LongText = new (Instance, "TT_LongText", 1, SqlDbType.VarChar, null, true, int.MaxValue);

			SchemaPKColumn ITableSchema.PK => PK;
			string ITableSchema.PkIndexName => string.Empty;
			SchemaColumnCollection ITableSchema.All { get; } = new SchemaColumnCollection(PK, new SchemaColumn[] { TT_LongText });
			SchemaColumn ITableSchema.GetSchemaColumn(string columnName) => columnName switch
			{
				"TT_PK" => PK,
				"TT_LongText" => TT_LongText,
				_ => null
			};
		}

		class TestSubscriberDataSchema : ISubscriberDataSchema
		{
			public int DataSchemaVersion => 1;
			public IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[] { TestTableSchema.PK, TestTableSchema.TT_LongText };
			public IEnumerable<SchemaColumn> LegacyColumns => Enumerable.Empty<SchemaColumn>();
			public IEnumerable<ColumnInfo> NonBizObjColumns => Enumerable.Empty<ColumnInfo>();
		}

		public void TestChunkedKafkaMessageShallNotExceedMaxSize()
		{
			// ARRANGE
			var changes = GenerateTestData(500_000, 500_000, 500_000, 500_000);
			var messageOverhead = 100_000;

			var producedMessages = new List<Message<string, string>>();
			var kafkaProducerMock = new Mock<IProducer<string, string>>();
			kafkaProducerMock
				.Setup(p => p.Produce(SubscriberToKafkaForTest.TestTopic, It.IsAny<Message<string, string>>(), null))
				.Callback((string topic_, Message<string, string> message, Action<DeliveryReport<string, string>> deliveryReports_) => producedMessages.Add(message));

			var subscriber = new SubscriberToKafkaForTest(
				new TestTableSchema(),
				new TestSubscriberDataSchema(),
				kafkaProducerMock.Object,
				maxChangesCount: 100);

			var logger = new LoggerForTest();

			// ACT
			subscriber.ProcessChanges(logger, changes);

			// ASSERTS
			kafkaProducerMock.Verify(
				p => p.Produce(
					SubscriberToKafkaForTest.TestTopic,
					It.Is<Message<string, string>>(
						msg => Encoding.UTF8.GetByteCount(msg.Value) + messageOverhead > SubscriberToKafkaForTest.MaxKafkaMessageSizeForTest),
					It.IsAny<Action<DeliveryReport<string, string>>>()),
				Times.Never,
				"Calling Kafka producer with an oversize message should never have happened.");

			var auditMessages = ReadAuditMessagesFromKafkaMessages(producedMessages).ToList();
			ValidateProducedMessages(changes, auditMessages, 4);
		}

		[DeveloperOnlyTest]
		public void TestChunkedKafkaMessageShallNotExceedMaxSize_WithTestTopic()
		{
			Assert("For this test you will need to launch a local Kafka instance servicing port 9092.", CheckIfLocalPortIsServiced(9092));

			// ARRANGE
			const string kafkaTopic = "test8ec8aae85e3f437b94b5d25d27315840";

			var clientConfig = new ClientConfig()
			{
				BootstrapServers = "localhost:9092",
			};

			var producerConfig = new ProducerConfig(clientConfig)
			{
				CompressionType = CompressionType.Snappy,
				EnableIdempotence = true,
				LingerMs = 20,
			};

			var consumerConfig = new ConsumerConfig(clientConfig)
			{
				GroupId = $"{nameof(DataScienceSubscriberToKafkaBaseMessagingTest)}.{nameof(TestChunkedKafkaMessageShallNotExceedMaxSize_WithTestTopic)}",
				AutoOffsetReset = AutoOffsetReset.Earliest,
			};

			var changes = GenerateTestData(500_000, 500_000, 500_000, 500_000);
			using var kafkaProducer = new ProducerBuilder<string, string>(producerConfig).Build();
			using var kafkaConsumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

			var subscriber = new SubscriberToKafkaForTest(
				new TestTableSchema(),
				new TestSubscriberDataSchema(),
				kafkaProducer,
				maxChangesCount: 100,
				topic: kafkaTopic);

			var logger = new LoggerForTest();

			// ACT
			subscriber.ProcessChanges(logger, changes);
			kafkaProducer.Flush();

			Thread.Sleep(1000);

			// ASSERTS
			var producedMessages = new List<Message<string, string>>();
			var numPolls = 0;
			var numMsgConsumed = 0;
			kafkaConsumer.Subscribe(kafkaTopic);
			while (numPolls < 100 && numMsgConsumed < changes.Rows.Count)
			{
				var consumeResult = kafkaConsumer.Consume(timeout: TimeSpan.FromSeconds(1.0));
				if (consumeResult?.Message is Message<string, string> msg)
				{
					producedMessages.Add(msg);
					kafkaConsumer.Commit();
					++numMsgConsumed;
					numPolls = 0;
				}
				else
				{
					numPolls += 1;
					continue;
				}
			}

			var auditMessages = ReadAuditMessagesFromKafkaMessages(producedMessages).ToList();
			ValidateProducedMessages(changes, auditMessages, 4);
		}

		public void TestChunkedKafkaMessageShallNotExceedMessageLimit()
		{
			// ARRANGE
			var maxChangesCount = 100;
			var changes = GenerateTestData(Enumerable.Repeat(100, 200).ToArray());
			var producedMessages = new List<Message<string, string>>();
			var kafkaProducerMock = new Mock<IProducer<string, string>>();
			kafkaProducerMock
				.Setup(p => p.Produce(SubscriberToKafkaForTest.TestTopic, It.IsAny<Message<string, string>>(), null))
				.Callback((string topic_, Message<string, string> message, Action<DeliveryReport<string, string>> deliveryReports_) => producedMessages.Add(message));

			var subscriber = new SubscriberToKafkaForTest(
				new TestTableSchema(),
				new TestSubscriberDataSchema(),
				kafkaProducerMock.Object,
				maxChangesCount);

			var logger = new LoggerForTest();

			// ACT
			subscriber.ProcessChanges(logger, changes);

			// ASSERT
			var auditMessages = ReadAuditMessagesFromKafkaMessages(producedMessages).ToList();
			ValidateProducedMessages(changes, auditMessages, 200);
			foreach (var auditMessage in auditMessages)
			{
				AssertLessThanOrEqualTo(auditMessage.Changes.Count, maxChangesCount);
			}
		}

		public void TestSingleOversizedChangeShallResultInMultiPartMessagesProduced()
		{
			// ARRANGE
			var messageOverhead = 100_000;
			var changes = GenerateTestData(1_500_000);
			var kafkaProducerMock = new Mock<IProducer<string, string>>();
			var cachedKafkaMessages = new List<Message<string, string>>();
			kafkaProducerMock
				.Setup(p => p.Produce(
					It.IsAny<string>(),
					It.IsAny<Message<string, string>>(),
					It.IsAny<Action<DeliveryReport<string, string>>>()))
				.Callback((string topic_, Message<string, string> message, Action<DeliveryReport<string, string>> deliveryReportHandler_) =>
				{
					cachedKafkaMessages.Add(message);
				});

			var subscriber = new SubscriberToKafkaForTest(
				new TestTableSchema(),
				new TestSubscriberDataSchema(),
				kafkaProducerMock.Object,
				maxChangesCount: 100);

			var logger = new LoggerForTest();

			// ACT
			subscriber.ProcessChanges(logger, changes);

			// ASSERT
			kafkaProducerMock.Verify(producer => producer.Produce(
				SubscriberToKafkaForTest.TestTopic,
				It.IsAny<Message<string, string>>(),
				null),
				Times.AtLeast(2));

			kafkaProducerMock.Verify(producer => producer.Produce(
				SubscriberToKafkaForTest.TestTopic,
				It.Is<Message<string, string>>(msg => Encoding.UTF8.GetByteCount(msg.Value) + messageOverhead > SubscriberToKafkaForTest.MaxKafkaMessageSizeForTest),
				null),
				Times.Never);

			var auditMessages = ReadAuditMessagesFromKafkaMessages(cachedKafkaMessages).ToList();
			ValidateProducedMessages(changes, auditMessages, 1);
		}

		static IEnumerable<AuditMessage> ReadAuditMessagesFromKafkaMessages(IEnumerable<Message<string, string>> kafkaMessages)
		{
			var messagePartsById = new Dictionary<Guid, string[]>();
			foreach (var kafkaMessage in kafkaMessages)
			{
				var kafkaMessageJO = JObject.Parse(kafkaMessage.Value);
				if (kafkaMessageJO.TryGetValue("multipartTrackingInfo", out var multipartTrackingInfoToken))
				{
					Assert(multipartTrackingInfoToken is JObject);
					var multipartTrackingInfo = (JObject)multipartTrackingInfoToken;
					var multipartMessageId = Guid.Parse(multipartTrackingInfo.Value<string>("messageId"));
					var partIndex = multipartTrackingInfo.Value<int>("partIndex");
					var partsCount = multipartTrackingInfo.Value<int>("partsCount");
					Assert(partIndex >= 0);
					Assert(partsCount > 0);
					Assert(partIndex < partsCount);

					var payloadString = kafkaMessageJO.Value<string>("payloadString");
					AssertNotNullOrEmpty(payloadString);

					if (partIndex == 0)
					{
						Assert(!messagePartsById.ContainsKey(multipartMessageId));
						var parts = new string[partsCount];
						parts[0] = payloadString;
						messagePartsById[multipartMessageId] = parts;
					}
					else
					{
						Assert(messagePartsById.ContainsKey(multipartMessageId));
						var parts = messagePartsById[multipartMessageId];
						AssertNotNull(parts);
						AssertNull(parts[partIndex]);
						parts[partIndex] = payloadString;

						if (partIndex == partsCount - 1)
						{
							Assert(parts.All(part => part != null));
							var auditMessage = JsonConvert.DeserializeObject<AuditMessage>(string.Concat(parts));
							AssertNotNull(auditMessage);
							yield return auditMessage;
							messagePartsById.Remove(multipartMessageId);
						}
					}
				}
				else
				{
					var auditMessage = JsonConvert.DeserializeObject<AuditMessage>(kafkaMessage.Value);
					AssertNotNull(auditMessage);
					yield return auditMessage;
				}
			}
		}

		public void TestRechunkAfterMessageSizeUnderestimation()
		{
			// ARRANGE
			var changes = GenerateTestData(400_000, 200_000, 200_000, 200_000);
			var messageOverhead = 100_000;

			var producedMessages = new List<Message<string, string>>();
			var kafkaProducerMock = new Mock<IProducer<string, string>>();
			var oversizeErrorCount = 0;
			kafkaProducerMock
				.Setup(p => p.Produce(SubscriberToKafkaForTest.TestTopic, It.IsAny<Message<string, string>>(), null))
				.Callback((string topic_, Message<string, string> message, Action<DeliveryReport<string, string>> deliveryReports_) =>
				{
					var messageSize = Encoding.UTF8.GetByteCount(message.Key) + Encoding.UTF8.GetByteCount(message.Value) + messageOverhead;
					if (messageSize > 800_000)
					{
						++oversizeErrorCount;
						throw new ProduceException<string, string>(
							new Error(ErrorCode.MsgSizeTooLarge),
							new DeliveryResult<string, string>()
							{
								Message = message
							});
					}

					producedMessages.Add(message);
				});

			var subscriber = new SubscriberToKafkaForTest(
				new TestTableSchema(),
				new TestSubscriberDataSchema(),
				kafkaProducerMock.Object,
				maxChangesCount: 100);

			var logger = new LoggerForTest();

			// ACT
			subscriber.ProcessChanges(logger, changes);

			// ASSERTS
			AssertEquals(1, oversizeErrorCount);

			var producedAuditMessages = ReadAuditMessagesFromKafkaMessages(producedMessages).ToList();
			ValidateProducedMessages(changes, producedAuditMessages, 4);

			var changesPerMessage = producedAuditMessages.Select(m => m.Changes.Count).ToList();
			AssertSequencesEqual(new int[] { 2, 2 }, changesPerMessage);
		}

		public void TestProduceRandomChanges_100k_200k() =>
			RunNTimes(10, random => RunTestProduceRandomChanges(numChanges: 100, maxChangesPerMessage: 100, minTextLength: 100_000, maxTextLength: 200_000, random: random));

		public void TestProduceRandomChanges_100k_700k() =>
			RunNTimes(10, random => RunTestProduceRandomChanges(numChanges: 100, maxChangesPerMessage: 100, minTextLength: 100_000, maxTextLength: 700_000, random: random));

		public void TestProduceRandomChanges_500k_700k() =>
			RunNTimes(10, random => RunTestProduceRandomChanges(numChanges: 100, maxChangesPerMessage: 100, minTextLength: 500_000, maxTextLength: 700_000, random: random));

		public void TestProduceRandomChanges_100k_200k_Max3PerMessage() =>
			RunNTimes(10, random => RunTestProduceRandomChanges(numChanges: 100, maxChangesPerMessage: 3, minTextLength: 100_000, maxTextLength: 200_000, random: random));

		public void TestProduceRandomChanges_100k_5m_WithMultiPart() =>
			RunNTimes(10, random => RunTestProduceRandomChanges(numChanges: 20, maxChangesPerMessage: 100, minTextLength: 100_000, maxTextLength: 5_000_000, random: random));

		public void TestProduceRandomChanges_1m_5m_WithMultiPart() =>
			RunNTimes(10, random => RunTestProduceRandomChanges(numChanges: 20, maxChangesPerMessage: 100, minTextLength: 1_000_000, maxTextLength: 5_000_000, random: random));

		[DeveloperOnlyTest]
		public void TestProduceRandomChanges_100k_200k_WithTestTopic() =>
			RunNTimes(2, random => RunTestProduceRandomChanges_WithTestTopic(numChanges: 100, maxChangesPerMessage: 100, minTextLength: 100_000, maxTextLength: 200_000, random: random));

		[DeveloperOnlyTest]
		public void TestProduceRandomChanges_100k_700k_WithTestTopic() =>
			RunNTimes(2, random => RunTestProduceRandomChanges_WithTestTopic(numChanges: 100, maxChangesPerMessage: 100, minTextLength: 100_000, maxTextLength: 700_000, random: random));

		[DeveloperOnlyTest]
		public void TestProduceRandomChanges_500k_700k_WithTestTopic() =>
			RunNTimes(2, random => RunTestProduceRandomChanges_WithTestTopic(numChanges: 100, maxChangesPerMessage: 100, minTextLength: 500_000, maxTextLength: 700_000, random: random));

		[DeveloperOnlyTest]
		public void TestProduceRandomChanges_100k_200k_Max3PerMessage_WithTestTopic() =>
			RunNTimes(2, random => RunTestProduceRandomChanges_WithTestTopic(numChanges: 100, maxChangesPerMessage: 3, minTextLength: 100_000, maxTextLength: 200_000, random: random));

		[DeveloperOnlyTest]
		public void TestProduceRandomChanges_100k_5m_WithMultiPart_WithTestTopic() =>
			RunNTimes(2, random => RunTestProduceRandomChanges_WithTestTopic(numChanges: 20, maxChangesPerMessage: 100, minTextLength: 100_000, maxTextLength: 5_000_000, random: random));

		[DeveloperOnlyTest]
		public void TestProduceRandomChanges_1m_3m_WithMultiPart_WithTestTopic() =>
			RunNTimes(2, random => RunTestProduceRandomChanges_WithTestTopic(numChanges: 20, maxChangesPerMessage: 100, minTextLength: 1_000_000, maxTextLength: 3_000_000, random: random));

		// [DeveloperOnlyTest]
		// public void TestGenerateSampleDataForConsumerTests()
		// {
		// 	var numChanges = 50;
		// 	var maxChangesPerMessage = 100;
		// 	var minTextLength = 2000;
		// 	var maxTextLength = 3_000_000;
		// 	var random = new Random((int)(DateTime.UtcNow.Ticks % (long)int.MaxValue));
		// 	var testName = $"{nameof(TestGenerateSampleDataForConsumerTests)}_NumChanges{numChanges}_MinTextLength{minTextLength}_MaxTextLength{maxTextLength}";

		// 	// ARRANGE
		// 	var changes = GenerateRandomTestData(numChanges, minTextLength, maxTextLength, random, out var numChangeRecords);
		// 	var producedMessages = new List<Message<string, string>>();
		// 	var kafkaProducerMock = new Mock<IProducer<string, string>>();
		// 	kafkaProducerMock
		// 		.Setup(p => p.Produce(SubscriberToKafkaForTest.TestTopic, It.IsAny<Message<string, string>>(), null))
		// 		.Callback((string topic_, Message<string, string> message, Action<DeliveryReport<string, string>> deliveryReports_) => producedMessages.Add(message));

		// 	var subscriber = new SubscriberToKafkaForTest(
		// 		new TestTableSchema(),
		// 		new TestSubscriberDataSchema(),
		// 		kafkaProducerMock.Object,
		// 		maxChangesCount: maxChangesPerMessage);

		// 	var logger = new LoggerForTest();

		// 	// ACT
		// 	subscriber.ProcessChanges(logger, changes);

		// 	// ASSERT
		// 	var messageOverhead = 100_000;
		// 	var auditMessages = ReadAuditMessagesFromKafkaMessages(producedMessages).ToList();
		// 	ValidateProducedMessages(changes, auditMessages, numChangeRecords);
		// 	foreach (var kafkaMessage in producedMessages)
		// 	{
		// 		AssertLessThanOrEqualTo(
		// 			Encoding.UTF8.GetByteCount(kafkaMessage.Value) + messageOverhead,
		// 			SubscriberToKafkaForTest.MaxKafkaMessageSizeForTest
		// 		);
		// 	}

		// 	foreach (var auditMessage in auditMessages)
		// 	{
		// 		AssertLessThanOrEqualTo(auditMessage.Changes.Count, maxChangesPerMessage);
		// 	}

		// 	var serialiser = AuditMessageJsonSerializerFactory.CreateJsonSerializer();
		// 	using (var stream = File.Create(Path.Combine(GetCurrentDirectory(), $"{testName}.KafkaMessages.json.gz")))
		// 	{
		// 		using var compression = new GZipStream(stream, mode: CompressionMode.Compress);
		// 		using var writer = new StreamWriter(compression, Encoding.UTF8);
		// 		using var jsonWriter = new JsonTextWriter(writer);
		// 		serialiser.Serialize(jsonWriter, producedMessages);
		// 	}

		// 	using (var stream = File.Create(Path.Combine(GetCurrentDirectory(), $"{testName}.AuditMessages.json.gz")))
		// 	{
		// 		using var compression = new GZipStream(stream, mode: CompressionMode.Compress);
		// 		using var writer = new StreamWriter(compression, Encoding.UTF8);
		// 		using var jsonWriter = new JsonTextWriter(writer);
		// 		serialiser.Serialize(jsonWriter, auditMessages);
		// 	}
		// }

		static string GetCurrentDirectory([CallerFilePath] string callerFile = null)
		{
			AssertNotNull(callerFile);
			var dir = Path.GetDirectoryName(callerFile);
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			return dir;
		}

		static void RunNTimes(int runTimes, Action<Random> runAction)
		{
			var random = new Random((int)(DateTime.UtcNow.Ticks % int.MaxValue));
			for (var i = 0; i < runTimes; ++i)
			{
				runAction(random);
			}
		}

		void RunTestProduceRandomChanges(int numChanges, int maxChangesPerMessage, int minTextLength, int maxTextLength, Random random)
		{
			// ARRANGE
			var changes = GenerateRandomTestData(numChanges, minTextLength, maxTextLength, random, out var numChangeRecords);
			var producedMessages = new List<Message<string, string>>();
			var kafkaProducerMock = new Mock<IProducer<string, string>>();
			kafkaProducerMock
				.Setup(p => p.Produce(SubscriberToKafkaForTest.TestTopic, It.IsAny<Message<string, string>>(), null))
				.Callback((string topic_, Message<string, string> message, Action<DeliveryReport<string, string>> deliveryReports_) => producedMessages.Add(message));

			var subscriber = new SubscriberToKafkaForTest(
				new TestTableSchema(),
				new TestSubscriberDataSchema(),
				kafkaProducerMock.Object,
				maxChangesCount: maxChangesPerMessage);

			var logger = new LoggerForTest();

			// ACT
			subscriber.ProcessChanges(logger, changes);

			// ASSERT
			var messageOverhead = 100_000;
			var auditMessages = ReadAuditMessagesFromKafkaMessages(producedMessages).ToList();
			ValidateProducedMessages(changes, auditMessages, numChangeRecords);
			foreach (var kafkaMessage in producedMessages)
			{
				AssertLessThanOrEqualTo(
					Encoding.UTF8.GetByteCount(kafkaMessage.Value) + messageOverhead,
					SubscriberToKafkaForTest.MaxKafkaMessageSizeForTest
				);
			}

			foreach (var auditMessage in auditMessages)
			{
				AssertLessThanOrEqualTo(auditMessage.Changes.Count, maxChangesPerMessage);
			}
		}

		void RunTestProduceRandomChanges_WithTestTopic(int numChanges, int maxChangesPerMessage, int minTextLength, int maxTextLength, Random random)
		{
			// ARRANGE
			Assert("For this test you will need to launch a local Kafka instance servicing port 9092", CheckIfLocalPortIsServiced(9092));
			var kafkaTopic = $"test{random.Next(1, 9999)}";

			var clientConfig = new ClientConfig()
			{
				BootstrapServers = "localhost:9092",
			};

			var producerConfig = new ProducerConfig(clientConfig)
			{
				CompressionType = CompressionType.Snappy,
				EnableIdempotence = true,
				LingerMs = 20,
			};

			var consumerConfig = new ConsumerConfig(clientConfig)
			{
				GroupId = $"{nameof(DataScienceSubscriberToKafkaBaseMessagingTest)}.{nameof(RunTestProduceRandomChanges_WithTestTopic)}",
				AutoOffsetReset = AutoOffsetReset.Earliest,
			};

			var changes = GenerateRandomTestData(numChanges, minTextLength, maxTextLength, random, out var numChangeRecords);
			using var kafkaProducer = new ProducerBuilder<string, string>(producerConfig).Build();
			using var kafkaConsumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

			var subscriber = new SubscriberToKafkaForTest(
				new TestTableSchema(),
				new TestSubscriberDataSchema(),
				kafkaProducer,
				maxChangesCount: maxChangesPerMessage,
				topic: kafkaTopic);

			var logger = new LoggerForTest();

			// ACT
			subscriber.ProcessChanges(logger, changes);
			kafkaProducer.Flush();
			Thread.Sleep(1000);

			// ASSERT
			var messageOverhead = 100_000;
			var producedMessages = new List<Message<string, string>>();
			var numPolls = 0;
			var numMsgConsumed = 0;
			kafkaConsumer.Subscribe(kafkaTopic);
			while (numPolls < 100)
			{
				var consumeResult = kafkaConsumer.Consume(timeout: TimeSpan.FromSeconds(1.0));
				if (consumeResult?.IsPartitionEOF ?? false)
				{
					break;
				}

				if (consumeResult?.Message is Message<string, string> msg)
				{
					producedMessages.Add(msg);
					kafkaConsumer.Commit();
					++numMsgConsumed;
					numPolls = 0;
				}
				else
				{
					numPolls += 1;
					continue;
				}
			}

			var auditMessages = ReadAuditMessagesFromKafkaMessages(producedMessages).ToList();
			ValidateProducedMessages(changes, auditMessages, numChangeRecords);
			foreach (var kafkaMessage in producedMessages)
			{
				AssertLessThanOrEqualTo(
					Encoding.UTF8.GetByteCount(kafkaMessage.Value) + messageOverhead,
					SubscriberToKafkaForTest.MaxKafkaMessageSizeForTest
				);
			}

			foreach (var auditMessage in auditMessages)
			{
				AssertLessThanOrEqualTo(auditMessage.Changes.Count, maxChangesPerMessage);
			}
		}

		static DataTable GenerateTestData(params int[] longTextLengths)
		{
			var changes = new DataTable();
			_ = changes.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			_ = changes.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			_ = changes.Columns.Add(AuditFieldNames.OperationFieldName, typeof(int));
			_ = changes.Columns.Add(AuditFieldNames.TranEndTimeUtc, typeof(DateTime));
			_ = changes.Columns.Add(TestTableSchema.PK.Name, TestTableSchema.PK.DotNetType);
			_ = changes.Columns.Add(TestTableSchema.TT_LongText.Name, TestTableSchema.TT_LongText.DotNetType);

			var longTextBuilder = new StringBuilder(2_000_000);
			for (int i = 0, n = longTextLengths.Length; i < n; ++i)
			{
				var longTextLength = longTextLengths[i];
				var row = changes.NewRow();
				var startLsn = GetLsnForChangeRecordIndex(i);
				var seqVal = GetSeqValForChangeRecordIndex(i);
				var pk = Guid.NewGuid();
				row[AuditFieldNames.StartLsnFieldName] = startLsn;
				row[AuditFieldNames.SeqValFieldName] = seqVal;
				row[AuditFieldNames.OperationFieldName] = CdcOperation.Insert;
				row[AuditFieldNames.TranEndTimeUtc] = new DateTime(2024, 3, 14);
				row[TestTableSchema.PK.Name] = pk;
				row[TestTableSchema.TT_LongText.Name] = CreateLongTextFromEmbeddedResourceSample(longTextLength, longTextBuilder);
				changes.Rows.Add(row);
			}

			return changes;
		}

		static DataTable GenerateRandomTestData(int numChanges, int minTextLength, int maxTextLength, Random random, out int numChangeRecords)
		{
			numChangeRecords = 0;
			var changes = new DataTable();
			_ = changes.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			_ = changes.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			_ = changes.Columns.Add(AuditFieldNames.OperationFieldName, typeof(int));
			_ = changes.Columns.Add(AuditFieldNames.TranEndTimeUtc, typeof(DateTime));
			_ = changes.Columns.Add(TestTableSchema.PK.Name, TestTableSchema.PK.DotNetType);
			_ = changes.Columns.Add(TestTableSchema.TT_LongText.Name, TestTableSchema.TT_LongText.DotNetType);

			var pks = new HashSet<Guid>(numChanges);
			var longTextBuilder = new StringBuilder(maxTextLength);
			for (var i = 0; i < numChanges; ++i)
			{
				var startLsn = GetLsnForChangeRecordIndex(i);
				var seqVal = GetSeqValForChangeRecordIndex(i);
				var opChoice = (CdcOperation)random.Next(0, 3);
				if (opChoice == CdcOperation.Insert || pks.Count == 0)
				{
					var insertRow = changes.NewRow();
					var pk = Guid.NewGuid();
					insertRow[AuditFieldNames.StartLsnFieldName] = startLsn;
					insertRow[AuditFieldNames.SeqValFieldName] = seqVal;
					insertRow[AuditFieldNames.OperationFieldName] = CdcOperation.Insert;
					insertRow[AuditFieldNames.TranEndTimeUtc] = DateTime.UtcNow;
					insertRow[TestTableSchema.PK.Name] = pk;
					insertRow[TestTableSchema.TT_LongText.Name] = CreateLongTextFromEmbeddedResourceSample(random.Next(minTextLength, maxTextLength), longTextBuilder);
					changes.Rows.Add(insertRow);
					AssertEquals(DataRowState.Added, insertRow.RowState);
					pks.Add(pk);
					++numChangeRecords;
				}
				else if (opChoice == CdcOperation.Delete)
				{
					var pk = pks.ElementAt(random.Next(0, pks.Count));
					var sourceRow = changes.Rows.Cast<DataRow>().Last(r => r.RowState != DataRowState.Deleted && r[TestTableSchema.PK.Name].Equals(pk));
					var deleteRow = changes.NewRow();
					deleteRow[AuditFieldNames.StartLsnFieldName] = startLsn;
					deleteRow[AuditFieldNames.SeqValFieldName] = seqVal;
					deleteRow[AuditFieldNames.OperationFieldName] = CdcOperation.Delete;
					deleteRow[AuditFieldNames.TranEndTimeUtc] = DateTime.UtcNow;
					deleteRow[TestTableSchema.PK.Name] = pk;
					deleteRow[TestTableSchema.TT_LongText.Name] = sourceRow[TestTableSchema.TT_LongText.Name];

					changes.Rows.Add(deleteRow);
					deleteRow.AcceptChanges();
					deleteRow.Delete();
					AssertEquals(DataRowState.Deleted, deleteRow.RowState);
					pks.Remove(pk);
					++numChangeRecords;
				}
				else  // update
				{
					var pk = pks.ElementAt(random.Next(0, pks.Count));
					var sourceRow = changes.Rows.Cast<DataRow>().Last(r => r.RowState != DataRowState.Deleted && r[TestTableSchema.PK.Name].Equals(pk));
					var updateRow = changes.NewRow();
					updateRow[AuditFieldNames.StartLsnFieldName] = startLsn;
					updateRow[AuditFieldNames.SeqValFieldName] = seqVal;
					updateRow[AuditFieldNames.OperationFieldName] = CdcOperation.PreUpdate;
					updateRow[AuditFieldNames.TranEndTimeUtc] = DateTime.UtcNow;
					updateRow[TestTableSchema.PK.Name] = pk;
					updateRow[TestTableSchema.TT_LongText.Name] = sourceRow[TestTableSchema.TT_LongText.Name];

					changes.Rows.Add(updateRow);
					updateRow.AcceptChanges();
					updateRow.SetModified();
					AssertEquals(DataRowState.Modified, updateRow.RowState);
					updateRow[TestTableSchema.TT_LongText.Name] = CreateLongTextFromEmbeddedResourceSample(random.Next(minTextLength, maxTextLength), longTextBuilder);
					numChangeRecords += 2;
				}
			}

			return changes;
		}

		static string CreateLongTextFromEmbeddedResourceSample(int minLength, StringBuilder buffer)
		{
			buffer.Clear();
			var testAssembly = typeof(DataScienceSubscriberToKafkaBaseMessagingTest).Assembly;
			using var sampleTextStream = testAssembly.GetManifestResourceStream("ZClientEDI.Business.Test.DataScience.SampleText.html.gz");
			AssertNotNull(sampleTextStream);
			while (buffer.Length < minLength)
			{
				sampleTextStream.Seek(0L, SeekOrigin.Begin);
				using var decompressionStream = new GZipStream(sampleTextStream, mode: CompressionMode.Decompress, leaveOpen: true);
				using var sampleTextReader = new StreamReader(decompressionStream, Encoding.UTF8);

				while (buffer.Length < minLength)
				{
					var line = sampleTextReader.ReadLine();
					if (line == null)
					{
						break;
					}

					buffer.AppendLine(line);
				}
			}

			return buffer.ToString();
		}

		void ValidateProducedMessages(DataTable sourceData, List<AuditMessage> messages, int expectedNumChanges)
		{
			var auditRows = messages.SelectMany(message => message.Changes).ToList();
			var sourceChanges = sourceData.Rows.Cast<DataRow>().SelectMany(row =>
			{
				switch (row.RowState)
				{
					case DataRowState.Deleted:
						return new[] { (ChangeRow: row, RowVersion: DataRowVersion.Original, Operation: CdcOperation.Delete) };
					case DataRowState.Added:
						return new[] { (ChangeRow: row, RowVersion: DataRowVersion.Current, Operation: CdcOperation.Insert) };
					case DataRowState.Modified:
						return new[] {
							(ChangeRow: row, RowVersion: DataRowVersion.Original, Operation: CdcOperation.PreUpdate),
							(ChangeRow: row, RowVersion: DataRowVersion.Current, Operation: CdcOperation.PostUpdate)
						};

					default:
						Fail("Invalid row state");
						return Array.Empty<(DataRow, DataRowVersion, CdcOperation)>();
				}
			}).ToList();

			AssertEquals(expectedNumChanges, sourceChanges.Count);
			AssertEquals(expectedNumChanges, auditRows.Count);
			foreach (var ((sourceDataRow, rowVersion, operation), auditRow) in sourceChanges.Zip(auditRows, (x, y) => (x, y)))
			{
				var sourceDataStartLsn = (byte[])sourceDataRow[AuditFieldNames.StartLsnFieldName, rowVersion];
				var sourceDataSeqVal = (byte[])sourceDataRow[AuditFieldNames.SeqValFieldName, rowVersion];
				var sourceDataTranEndTimeUtc = (DateTime)sourceDataRow[AuditFieldNames.TranEndTimeUtc, rowVersion];
				var sourceDataPk = (Guid)sourceDataRow[TestTableSchema.PK.Name, rowVersion];
				var sourceDataLongText = (string)sourceDataRow[TestTableSchema.TT_LongText.Name, rowVersion];

				var messageDataStartLsn = auditRow.StartLsn;
				var messageDataSeqVal = auditRow.SequenceValue;
				var messageDataOperation = auditRow.Operation;
				var messageDataTranEndTimeUtc = auditRow.TransactionEndTimeUtc;
				var messageDataPk = Guid.Parse((string)auditRow.ColumnValues[TestTableSchema.PK.Name]);
				var messageDataLongText = (string)auditRow.ColumnValues[TestTableSchema.TT_LongText.Name];

				AssertEquals(operation, auditRow.Operation);
				AssertSequencesEqual("Mismatch between source and message data (StartLsn)", sourceDataStartLsn, messageDataStartLsn);
				AssertSequencesEqual("Mismatch between source and message data (SeqVal)", sourceDataSeqVal, messageDataSeqVal);
				AssertEquals("Mismatch between source and message data (Operation)", operation, messageDataOperation);
				AssertEquals("Mismatch between source and message data (TranEndTimeUtc)", sourceDataTranEndTimeUtc, messageDataTranEndTimeUtc);
				AssertEquals("Mismatch between source and message data (TT_PK)", sourceDataPk, messageDataPk);
				AssertEquals("Mismatch between source and message data (TT_LongText)", sourceDataLongText, messageDataLongText);
			}
		}

		static readonly byte[] baseLsn = new byte[]
		{
			0x00, 0x06, 0x8C, 0xB5, 0x00, 0x3C, 0x23, 0x7F, 0x00, 0x07
		};

		static byte[] GetLsnForChangeRecordIndex(int index)
		{
			var outLsn = new byte[10];
			for (int i = 0, n = baseLsn.Length; i < n; ++i)
			{
				outLsn[i] = baseLsn[i];
			}

			var lowBytes = outLsn.AsSpan().Slice(0, 8);
			var x = BinaryPrimitives.ReadUInt64BigEndian(lowBytes);
			BinaryPrimitives.WriteUInt64BigEndian(lowBytes, x + (ulong)(5 * (index / 5)));
			return outLsn;
		}

		static byte[] GetSeqValForChangeRecordIndex(int index)
		{
			var outSeqVal = new byte[10];
			for (int i = 0, n = baseLsn.Length; i < n; ++i)
			{
				outSeqVal[i] = baseLsn[i];
			}

			var lowBytes = outSeqVal.AsSpan().Slice(0, 8);
			var x = BinaryPrimitives.ReadUInt64BigEndian(lowBytes);
			BinaryPrimitives.WriteUInt64BigEndian(lowBytes, x + (ulong)index);
			return outSeqVal;
		}

		static bool CheckIfLocalPortIsServiced(int portNumber) =>
			IPGlobalProperties.GetIPGlobalProperties()?.GetActiveTcpListeners() is System.Net.IPEndPoint[] endPoints &&
			endPoints.Any(ep => ep.Port == portNumber);
	}

	class DataScienceSubscriberToKafkaGenericBaseTest : TestCase
	{
		public void TestActualDataChangesAuditSubscriberOverrides()
		{
			// Arrange / Act
			var subscribers = new DataScienceSubscriberToKafkaBase[] {
				new SubscriberForTest<KafkaRegistryConfigs.Core>(),
				new SubscriberForTest<KafkaRegistryConfigs.Billing>(),
				new SubscriberForTest<KafkaRegistryConfigs.Productivity>(),
				new SubscriberForTest<KafkaRegistryConfigs.IncidentRelated>(),
			};

			// Assert
			foreach (var subscriber in subscribers)
			{
				Assert(subscriber.NotifyInsert);
				Assert(subscriber.NotifyUpdate);
				Assert(subscriber.NotifyDelete);
			}
		}

		public void TestSingleKafkaLock()
		{
			var subscribersGroup1 = new IWantThatKafkaTopicLock[] {
				new SubscriberForTest<KafkaRegistryConfigs.Core>(),
				new SubscriberForTest<KafkaRegistryConfigs.Billing>(),
				new SubscriberForTest<KafkaRegistryConfigs.Productivity>(),
				new SubscriberForTest<KafkaRegistryConfigs.IncidentRelated>(),
				new SubscriberForTest<KafkaRegistryConfigs.Issues>(),
			};

			var subscribersGroup2 = new IWantThatKafkaTopicLock[] {
				new SubscriberForTest<KafkaRegistryConfigs.Core>(),
				new SubscriberForTest<KafkaRegistryConfigs.Billing>(),
				new SubscriberForTest<KafkaRegistryConfigs.Productivity>(),
				new SubscriberForTest<KafkaRegistryConfigs.IncidentRelated>(),
				new SubscriberForTest<KafkaRegistryConfigs.Issues>(),
			};

			Assert(ReferenceEquals(new KafkaRegistryConfigs.Core().KafkaTopicLock, new KafkaRegistryConfigs.Core().KafkaTopicLock));
			Assert(ReferenceEquals(new KafkaRegistryConfigs.Billing().KafkaTopicLock, new KafkaRegistryConfigs.Billing().KafkaTopicLock));
			Assert(ReferenceEquals(new KafkaRegistryConfigs.Productivity().KafkaTopicLock, new KafkaRegistryConfigs.Productivity().KafkaTopicLock));
			Assert(ReferenceEquals(new KafkaRegistryConfigs.IncidentRelated().KafkaTopicLock, new KafkaRegistryConfigs.IncidentRelated().KafkaTopicLock));
			Assert(ReferenceEquals(new KafkaRegistryConfigs.Issues().KafkaTopicLock, new KafkaRegistryConfigs.Issues().KafkaTopicLock));

			Assert(!ReferenceEquals(new KafkaRegistryConfigs.Core().KafkaTopicLock, new KafkaRegistryConfigs.Billing().KafkaTopicLock));
			Assert(!ReferenceEquals(new KafkaRegistryConfigs.Billing().KafkaTopicLock, new KafkaRegistryConfigs.Productivity().KafkaTopicLock));
			Assert(!ReferenceEquals(new KafkaRegistryConfigs.Productivity().KafkaTopicLock, new KafkaRegistryConfigs.IncidentRelated().KafkaTopicLock));
			Assert(!ReferenceEquals(new KafkaRegistryConfigs.IncidentRelated().KafkaTopicLock, new KafkaRegistryConfigs.Issues().KafkaTopicLock));
			Assert(!ReferenceEquals(new KafkaRegistryConfigs.Issues().KafkaTopicLock, new KafkaRegistryConfigs.Core().KafkaTopicLock));

			Assert(ReferenceEquals(subscribersGroup1[0].GetThatKafkaTopicLock(), new KafkaRegistryConfigs.Core().KafkaTopicLock));
			Assert(ReferenceEquals(subscribersGroup1[1].GetThatKafkaTopicLock(), new KafkaRegistryConfigs.Billing().KafkaTopicLock));
			Assert(ReferenceEquals(subscribersGroup1[2].GetThatKafkaTopicLock(), new KafkaRegistryConfigs.Productivity().KafkaTopicLock));
			Assert(ReferenceEquals(subscribersGroup1[3].GetThatKafkaTopicLock(), new KafkaRegistryConfigs.IncidentRelated().KafkaTopicLock));
			Assert(ReferenceEquals(subscribersGroup1[4].GetThatKafkaTopicLock(), new KafkaRegistryConfigs.Issues().KafkaTopicLock));

			Assert(ReferenceEquals(subscribersGroup2[0].GetThatKafkaTopicLock(), new KafkaRegistryConfigs.Core().KafkaTopicLock));
			Assert(ReferenceEquals(subscribersGroup2[1].GetThatKafkaTopicLock(), new KafkaRegistryConfigs.Billing().KafkaTopicLock));
			Assert(ReferenceEquals(subscribersGroup2[2].GetThatKafkaTopicLock(), new KafkaRegistryConfigs.Productivity().KafkaTopicLock));
			Assert(ReferenceEquals(subscribersGroup2[3].GetThatKafkaTopicLock(), new KafkaRegistryConfigs.IncidentRelated().KafkaTopicLock));
			Assert(ReferenceEquals(subscribersGroup2[4].GetThatKafkaTopicLock(), new KafkaRegistryConfigs.Issues().KafkaTopicLock));

			for (var i = 0; i < subscribersGroup1.Length; ++i)
			{
				var subscriber1 = subscribersGroup1[i];
				for (var j = 0; j < subscribersGroup2.Length; ++j)
				{
					var subscriber2 = subscribersGroup2[j];
					AssertEquals(i == j, ReferenceEquals(subscriber1.GetThatKafkaTopicLock(), subscriber2.GetThatKafkaTopicLock()));
				}
			}
		}

		interface IWantThatKafkaTopicLock
		{
			object GetThatKafkaTopicLock();
		}

		class SubscriberForTest<TConfig> : DataScienceSubscriberToKafkaBase<TConfig>, IWantThatKafkaTopicLock
			where TConfig : KafkaRegistryConfigsBase<TConfig>, new()
		{
			public override string Code => throw new NotImplementedException();
			public override ITableSchema Table => throw new NotImplementedException();
			public override ISubscriberDataSchema DataSchema => throw new NotImplementedException();
			public override int DataSchemaVersion => throw new NotImplementedException();
			public object GetThatKafkaTopicLock() => KafkaTopicLock;
			public override IEnumerable<SchemaColumn> BizObjColumns => throw new NotImplementedException();
		}
	}

	class SubscriberToKafkaForTest : DataScienceSubscriberToKafkaBase
	{
		readonly IProducer<string, string> kafkaProducer;

		internal SubscriberToKafkaForTest(ITableSchema table, ISubscriberDataSchema dataSchema, IProducer<string, string> kafkaProducer,
			int maxChangesCount = 100, bool useTransactions = false, string topic = TestTopic)
		{
			Table = table;
			DataSchema = dataSchema;
			this.kafkaProducer = kafkaProducer;
			MaxChangesCount = maxChangesCount;
			UseTransactions = useTransactions;
			this.topic = topic;
		}

		public override bool NotifyInsert { get; } = true;
		public override bool NotifyUpdate { get; } = true;
		public override bool NotifyDelete { get; } = true;
		public override string Code { get; } = "T~~";
		public override ITableSchema Table { get; }

		public override ISubscriberDataSchema DataSchema { get; }

		internal const string TestTopic = "test-topic";
		readonly string topic;
		protected override string Topic => topic;
		protected override IProducer<string, string> GetOrCreateKafkaProducer(Integration.ILogger logger) => kafkaProducer;
		protected override object KafkaTopicLock { get; } = new object();
		protected override int MaxChangesCount { get; }
		protected override bool UseTransactions { get; }

		public override bool IsRequired() => throw new NotImplementedException();

		internal const int MaxKafkaMessageSizeForTest = 1_000_000;
		protected override int MaxKafkaMessageSize => MaxKafkaMessageSizeForTest;
	}

	public sealed class TestKafkaConfigs(string ediKafkaBootstrapServers) : KafkaRegistryConfigsBase<TestKafkaConfigs>
	{
		public TestKafkaConfigs() : this("default-bootstrap-servers") { }
		public override bool EnableSubscriber { get; } = true;
		public override bool UseTransaction { get; } = true;
		public override string KafkaTopic { get; } = "test-topic";
		protected override string EdiKafkaBootstrapServers { get; } = ediKafkaBootstrapServers;
		protected override string KafkaSaslPassword { get; } = "test-username";
		protected override string KafkaSaslUsername { get; } = "test-password";

		protected override IProducer<string, string> CreateProducer(Integration.ILogger logger)
		{
			var mockedKafkaProducer = new Mock<IProducer<string, string>>();
			return mockedKafkaProducer.Object;
		}

		public string PeekBootstrapServers() => EdiKafkaBootstrapServers;
	}
}
