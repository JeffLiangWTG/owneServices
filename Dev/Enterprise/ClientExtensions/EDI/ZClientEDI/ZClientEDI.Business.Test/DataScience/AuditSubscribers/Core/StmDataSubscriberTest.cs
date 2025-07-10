using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Schema;
using Confluent.Kafka;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using WTG.Serialization.DataScience.Audit.ObjectModel;
using ZClientEDI.Business.Registry;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core.Tests
{
	[TestedType(typeof(StmDataSubscriber))]
	class StmDataSubscriberTest : DataScienceAuditSubscriberTestBase<StmDataSubscriber>
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
					changeRow["SD_PK"] = new Guid("D6878F6B-DD30-492A-AA19-D94DDF21BAE6");
					changeRow["SD_Name"] = "DataScienceBillingKafkaTopic";
					changeTable.Rows.Add(changeRow);
					return changeTable;
				})(),

				["insert 2"] = new Func<DataTable>(() =>
				{
					var changeTable = GetChangeDataTable();
					var changeRow = changeTable.NewRow();
					changeRow[AuditFieldNames.StartLsnFieldName] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
					changeRow[AuditFieldNames.SeqValFieldName] = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02 };
					changeRow[AuditFieldNames.OperationFieldName] = CdcOperation.Insert;
					changeRow[AuditFieldNames.TranEndTimeUtc] = new DateTime(2022, 1, 1);
					changeRow["SD_PK"] = new Guid("D6878F6B-DD30-492A-AA19-D94DDF21BAE6");
					changeRow["SD_Name"] = "DataScienceIssuesKafkaSaslPassword";
					changeTable.Rows.Add(changeRow);
					return changeTable;
				})(),
			}.ToDictionary(kvp => kvp.Key, kvp => (AssertChangeTable: kvp.Value, ActChangeTable: kvp.Value.Copy())));

			set => changeTables = value;
		}
		static Dictionary<string, (DataTable AssertChangeTable, DataTable ActChangeTable)> changeTables;

		StmDataSubscriber Subscriber { get; set; }
		Mock<ITableSchema> TableMock { get; set; }
		Mock<ISubscriberDataSchema> SubscriberDataSchemaMock { get; set; }
		Mock<IProducer<string, string>> KafkaProducerMock { get; set; }
		List<JObject> Produced { get; set; }

		static DataTable GetChangeDataTable()
		{
			var changeTable = new DataTable();
			changeTable.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
			changeTable.Columns.Add(AuditFieldNames.OperationFieldName, typeof(int));
			changeTable.Columns.Add(AuditFieldNames.TranEndTimeUtc, typeof(DateTime));
			changeTable.Columns.Add("SD_PK", typeof(Guid));
			changeTable.Columns.Add("SD_Name", typeof(string));
			return changeTable;
		}

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
					new SchemaPKColumn(TableMock.Object, "SD_PK", false),
					new SchemaIntColumn(TableMock.Object, "SD_Name", 0, 0, false),
				});
			SubscriberDataSchemaMock.Setup(s => s.LegacyColumns)
				.Returns(Array.Empty<SchemaColumn>());
			SubscriberDataSchemaMock.Setup(s => s.NonBizObjColumns).Returns(Enumerable.Empty<ColumnInfo>());

			KafkaProducerMock = new Mock<IProducer<string, string>>();
			Produced = new List<JObject>();
			KafkaProducerMock.Setup(p => p.Produce("test-topic", It.IsAny<Message<string, string>>(), null))
				.Callback((string topic_, Message<string, string> message, Action<DeliveryReport<string, string>> deliveryReports_) =>
					Produced.Add(JObject.Parse(message.Value)));

			Subscriber = new StmDataSubscriberToKafkaForTest(TableMock.Object, SubscriberDataSchemaMock.Object, KafkaProducerMock.Object);
		}

		public void TestDataSchema()
		{
			// Uncomment the line below and debug this test to generate a test from the implementation.
			// var generatedCode = SchemaTestHelper.GenerateTestDataSchemaCode((IDataScienceSubscriberToKafka)TestDataChangeSubscriber);
			// Arrange / Act
			var subscriber = SubscriberUnderTest;

			// Assert
			AssertEquals(1, subscriber.DataSchema.DataSchemaVersion);
			CombineAssertions(
				@"
The schema of the table StmData required by StmDataSubscriber has changed.
The Data Science team will need to adjust their data pipelines before this change can be committed.
Please contact the Data Science team.",
				() =>
				{
					AssertEquals(14, subscriber.ColumnInfos.Count);

					AssertEquals("SD_PK", subscriber.ColumnInfos[0].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[0].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[0].IsNullable);

					AssertEquals("SD_BinaryValue", subscriber.ColumnInfos[1].ColumnName);
					AssertEquals("varbinary(max)", subscriber.ColumnInfos[1].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[1].IsNullable);

					AssertEquals("SD_DepartmentGuid", subscriber.ColumnInfos[2].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[2].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[2].IsNullable);

					AssertEquals("SD_GuidValue", subscriber.ColumnInfos[3].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[3].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[3].IsNullable);

					AssertEquals("SD_IsCancelled", subscriber.ColumnInfos[4].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[4].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[4].IsNullable);

					AssertEquals("SD_IsLogged", subscriber.ColumnInfos[5].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[5].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[5].IsNullable);

					AssertEquals("SD_Name", subscriber.ColumnInfos[6].ColumnName);
					AssertEquals("varchar(300)", subscriber.ColumnInfos[6].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[6].IsNullable);

					AssertEquals("SD_Owner", subscriber.ColumnInfos[7].ColumnName);
					AssertEquals("uniqueidentifier", subscriber.ColumnInfos[7].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[7].IsNullable);

					AssertEquals("SD_PreserveTestValue", subscriber.ColumnInfos[8].ColumnName);
					AssertEquals("bit", subscriber.ColumnInfos[8].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[8].IsNullable);

					AssertEquals("SD_SystemCreateTimeUtc", subscriber.ColumnInfos[9].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[9].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[9].IsNullable);

					AssertEquals("SD_SystemCreateUser", subscriber.ColumnInfos[10].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[10].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[10].IsNullable);

					AssertEquals("SD_SystemLastEditTimeUtc", subscriber.ColumnInfos[11].ColumnName);
					AssertEquals("smalldatetime", subscriber.ColumnInfos[11].SqlType);
					AssertEquals(true, subscriber.ColumnInfos[11].IsNullable);

					AssertEquals("SD_SystemLastEditUser", subscriber.ColumnInfos[12].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[12].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[12].IsNullable);

					AssertEquals("SD_Type", subscriber.ColumnInfos[13].ColumnName);
					AssertEquals("varchar(3)", subscriber.ColumnInfos[13].SqlType);
					AssertEquals(false, subscriber.ColumnInfos[13].IsNullable);
				});
		}

		public void TestSubscriber_ShouldProduceNonSensitiveStmDataChangeRecords()
		{
			// Arrange
			Subscriber = new StmDataSubscriberToKafkaForTest(TableMock.Object, SubscriberDataSchemaMock.Object, KafkaProducerMock.Object);
			var (_, actChangeTable) = ChangeTables["insert 1"];

			// Act
			Subscriber.ProcessChanges(new LoggerForTest(), actChangeTable);

			// Assert
			AssertEquals(1, Produced.Count);
		}

		public void TestSubscriber_ShouldNotProduceSensitiveStmDataChangeRecords()
		{
			// Arrange
			Subscriber = new StmDataSubscriberToKafkaForTest(TableMock.Object, SubscriberDataSchemaMock.Object, KafkaProducerMock.Object);
			var (_, actChangeTable) = ChangeTables["insert 2"];

			// Act
			Subscriber.ProcessChanges(new LoggerForTest(), actChangeTable);

			// Assert
			AssertEquals(0, Produced.Count);
		}

		public void TestShouldSendAuditRowSuccessful()
		{
			// Arrange
			var auditRow = new AuditRow(
				startLsn: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
				sequenceValue: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
				operation: CdcOperation.Insert,
				transactionEndTimeUtc: new DateTime(2022, 6, 1, 12, 0, 1),
				columnValues: new Dictionary<string, object>
				{
					["SD_PK"] = "A077A049-F896-446F-87A4-B55CE728C5E6",
					["SD_Name"] = "DataScienceBillingKafkaTopic"
				});
			Subscriber = new StmDataSubscriberToKafkaForTest(TableMock.Object, SubscriberDataSchemaMock.Object, KafkaProducerMock.Object, useTransactions: true);

			// Act
			var result = Subscriber.ShouldSendAuditRow(auditRow);

			// Assert
			AssertEquals(result, true);
		}

		public void TestSubscriber_ShouldSendAuditRow_ShouldReturnFalseGivenSensitiveAuditRow()
		{
			// Arrange
			var auditRow = new AuditRow(
				startLsn: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
				sequenceValue: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
				operation: CdcOperation.Insert,
				transactionEndTimeUtc: new DateTime(2022, 6, 1, 12, 0, 1),
				columnValues: new Dictionary<string, object>
				{
					["SD_PK"] = "A077A049-F896-446F-87A4-B55CE728C5E6",
					["SD_BinaryValue"] = new byte[] { 1, 2, 3, },
					["SD_Name"] = "DataScienceBillingKafkaSaslPassword"
				});
			Subscriber = new StmDataSubscriberToKafkaForTest(TableMock.Object, SubscriberDataSchemaMock.Object, KafkaProducerMock.Object, useTransactions: true);

			// Act
			var result = Subscriber.ShouldSendAuditRow(auditRow);

			// Assert
			AssertEquals(result, false);
		}

		public void TestSubscriber_SendAuditRow_ShouldReturnFalseGivenInvalidAuditRow()
		{
			// Arrange
			var auditRow = new AuditRow(
				startLsn: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
				sequenceValue: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
				operation: CdcOperation.Insert,
				transactionEndTimeUtc: new DateTime(2022, 6, 1, 12, 0, 1),
				columnValues: new Dictionary<string, object>
				{
					["SD_PK"] = "A077A049-F896-446F-87A4-B55CE728C5E6",
					["SD_BinaryValue"] = new byte[] { 1, 2, 3, },
					["SD_Name"] = "This_Is_An_Invalid_SD_Name"
				});

			Subscriber = new StmDataSubscriberToKafkaForTest(TableMock.Object, SubscriberDataSchemaMock.Object, KafkaProducerMock.Object, useTransactions: true);

			// Act
			var result = Subscriber.ShouldSendAuditRow(auditRow);

			// Assert
			AssertEquals(result, false);
		}

		public void TestGetRegistryItemFromStmDataAuditRow()
		{
			// Arrange
			var instance = EDIDataRegistry.Instance;
			var auditRow = new AuditRow(
				startLsn: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
				sequenceValue: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
				operation: CdcOperation.Insert,
				transactionEndTimeUtc: new DateTime(2022, 6, 1, 12, 0, 1),
				columnValues: new Dictionary<string, object>
				{
					["SD_PK"] = "A077A049-F896-446F-87A4-B55CE728C5E6",
					["SD_Name"] = "DataScienceBillingKafkaTopic"
				});

			// Act
			var registryItemResult = StmDataSubscriber.GetRegistryItemNameFromStmDataAuditRow(auditRow);

			// Assert
			AssertEquals(registryItemResult, EDIDataRegistry.Instance.DataScienceBillingKafkaTopic);
		}

		public void TestThat_GetRegistryItemFromStmDataAuditRow_ShouldReturnNull_WhenSDNameIsNotRegisteredInEDIRegistry()
		{
			// Arrange
			var instance = EDIDataRegistry.Instance;
			var auditRow = new AuditRow(
				startLsn: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
				sequenceValue: new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
				operation: CdcOperation.Insert,
				transactionEndTimeUtc: new DateTime(2022, 6, 1, 12, 0, 1),
				columnValues: new Dictionary<string, object>
				{
					["SD_PK"] = "A077A049-F896-446F-87A4-B55CE728C5E6",
					["SD_Name"] = "This_is_NOT_a_valid_registry_item"
				});

			// Act
			var registryItemResult = StmDataSubscriber.GetRegistryItemNameFromStmDataAuditRow(auditRow);

			// Assert
			AssertEquals(registryItemResult, null);
		}

		public void TestThat_IsRegistryItemSensitiveByMetadata_ShouldReturnsTrue_GivenSensitiveRegistryItems()
		{
			foreach (var registryItem in SensitiveRegistryItems)
			{
				Assert(StmDataSubscriber.IsRegistryItemSensitiveByMetadata(registryItem));
			}
		}

		public void TestThat_IsRegistryItemSensitiveByMetadata_ShouldReturnTrue_GivenRegistryItemsWithNull()
		{
			Assert(!StmDataSubscriber.IsRegistryItemSensitiveByMetadata(null));
		}

		public void TestThat_IsRegistryItemSensitiveByMetadata_ShouldReturnFalse_GivenNonSensitiveRegistryItem()
		{
			// Arrange
			var registryItem = new StringRegistryItem("DataScienceIssuesKafkaSaslUsername",
				(NoResString)"TestCategory",
				(NoResString)"Kafka SASL username",
				(NoResString)"Kafka SASL username for the Issues topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);

			// Act
			var result = StmDataSubscriber.IsRegistryItemSensitiveByMetadata(registryItem);

			// Assert
			AssertEquals(result, false);
		}

		public void Test_IsRegistryItemBlacklisted_ShouldReturnTrueGivenRegistryItemsWithNameThatIsBlacklisted()
		{
			// Arrange
			var registryItems = new List<StringRegistryItem>()
			{
				new StringRegistryItem("DataScienceIssuesKafkaSaslUsername",
				(NoResString)"TestCategory",
				(NoResString)"Kafka SASL Username",
				(NoResString)"Kafka SASL Username for the Issues topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default),

				new StringRegistryItem("DataScienceIssuesKafkaSaslCredential",
				(NoResString)"TestCategory",
				(NoResString)"Kafka SASL Credential",
				(NoResString)"Kafka SASL Credential for the Issues topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default),

				new StringRegistryItem("DataScienceIssuesKafkaSaslAccessToken",
				(NoResString)"TestCategory",
				(NoResString)"Kafka SASL AccessToken",
				(NoResString)"Kafka SASL AccessToken for the Issues topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default),

				new StringRegistryItem("DataScienceIssuesKafkaSaslAuthentication",
				(NoResString)"TestCategory",
				(NoResString)"Kafka SASL Authentication",
				(NoResString)"Kafka SASL Authentication for the Issues topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default),

				new StringRegistryItem("DataScienceIssuesKafkaSaslAuth",
				(NoResString)"TestCategory",
				(NoResString)"Kafka SASL Auth",
				(NoResString)"Kafka SASL Auth for the Issues topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default),

				new StringRegistryItem("DataScienceIssuesKafkaSaslAccessToken",
				(NoResString)"TestCategory",
				(NoResString)"Kafka SASL AccessToken",
				(NoResString)"Kafka SASL AccessToken for the Issues topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default),

				new StringRegistryItem("DataScienceIssuesKafkaSaslLogin",
				(NoResString)"TestCategory",
				(NoResString)"Kafka SASL Login",
				(NoResString)"Kafka SASL Login for the Issues topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default),

				new StringRegistryItem("PasswordForDataScienceIssuesKafkaSasl",
				(NoResString)"TestCategory",
				(NoResString)"Password For Kafka SASL Login",
				(NoResString)"Password For Kafka SASL Login",
				RegistryStorageFlags.System,
				RegistryOptions.Default),
			};

			foreach (var registryItem in registryItems)
			{
				Assert(StmDataSubscriber.IsRegistryItemBlacklisted(registryItem));
			}
		}

		public void TestThat_IsRegistryItemBlacklisted_ReturnFalse_GivenRegistryItemsWithNameNotBlacklisted()
		{
			// Arrange
			var registryItems = new List<StringRegistryItem>()
			{
				new StringRegistryItem("DataScienceIssuesKafkaSaslStuff",
				(NoResString)"TestCategory",
				(NoResString)"Kafka SASL Stuff",
				(NoResString)"Kafka SASL Stuff for the Issues topic. Defaults to Kafka topic.",
				RegistryStorageFlags.System,
				RegistryOptions.Default),

				new StringRegistryItem("DataScienceIssuesKafkaSaslPortAuthority",
				(NoResString)"TestCategory",
				(NoResString)"PortAuthority",
				(NoResString)"PortAuthority",
				RegistryStorageFlags.System,
				RegistryOptions.Default),

				new StringRegistryItem("DataScienceaccreditation",
				(NoResString)"TestCategory",
				(NoResString)"accreditation",
				(NoResString)"accreditation",
				RegistryStorageFlags.System,
				RegistryOptions.Default),

				new StringRegistryItem("DataScienceIssuesKafkaSaslLoginUser",
				(NoResString)"TestCategory",
				(NoResString)"LoginUser",
				(NoResString)"LoginUser",
				RegistryStorageFlags.System,
				RegistryOptions.Default),
			};

			foreach (var registryItem in registryItems)
			{
				Assert(!StmDataSubscriber.IsRegistryItemBlacklisted(registryItem));
			}
		}

		public void TestThat_IsRegistryItemBlacklisted_ReturnFalse_GivenRegistryItemsWithNull()
		{
			Assert(!StmDataSubscriber.IsRegistryItemBlacklisted(null));
		}

		public override void TestCustomFilter()
		{
			var subscriber = SubscriberUnderTest;
			AssertNull(subscriber.CustomFilter);
		}

		protected override IEnumerable<string> IgnoredColumnNames => Enumerable.Empty<string>();

		static readonly List<IRegistryItem> SensitiveRegistryItems = new List<IRegistryItem>()
		{
			new StringRegistryItem("DataScienceProductivityKafkaSample",
				(NoResString)"TestCategory",
				(NoResString)"Kafka SASL Sample",
				(NoResString)"Kafka SASL Sample for the Productivity topic, encoded with TwoWayEncoder.",
				new StringRegistryDataType(isEncrypted: true),
				new TextRegistryEditorInfo(TextEditorType.Password),
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty),

			new StringRegistryItem("DataScienceProductivityKafkaSample",
				(NoResString)"TestCategory",
				(NoResString)"Kafka SASL Sample",
				(NoResString)"Kafka SASL Sample for the Productivity topic, encoded with TwoWayEncoder.",
				new BinaryKeyRegistryDataType(64),
				null,
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty),

			new StringRegistryItem("DataScienceProductivityKafkaSample",
				(NoResString)"TestCategory",
				(NoResString)"Kafka SASL Sample",
				(NoResString)"Kafka SASL Sample for the Productivity topic, encoded with TwoWayEncoder.",
				new SecureStringRegistryDataType(),
				null,
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty),

			new StringRegistryItem("DataScienceProductivityKafkaSample",
				(NoResString)"TestCategory",
				(NoResString)"Kafka SASL Sample",
				(NoResString)"Kafka SASL Sample for the Productivity topic, encoded with TwoWayEncoder.",
				new SecurityIdentifierRegistryDataType(),
				null,
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty),

			new StringRegistryItem("DataScienceProductivityKafkaSample",
				(NoResString)"TestCategory",
				(NoResString)"Kafka SASL Sample",
				(NoResString)"Kafka SASL Sample for the Productivity topic, encoded with TwoWayEncoder.",
				new BinaryKeyRegistryDataType(64),
				null,
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				string.Empty),

			new CodeDescriptionPairListRegistryItem(
				(NoResString)"DataScienceProductivityKafkaSample",
				(NoResString)"Web Services",
				(NoResString)"Web Service Alternative Credentials",
				(NoResString)"Web Service Alternative Credentials",
				50,
				new LoginPasswordPairListEditorInfo(),
				RegistryStorageFlags.System,
				RegistryOptions.PreserveTestValue,
				new ReadOnlyCodeDescriptionPairList(),
				false),

			new BinaryRegistryItem(
				"CW1SupportLoginTokenPrivateKey",
				(NoResString)"TestCategory",
				(NoResString)"CW1Support Account Login Token Private Key",
				(NoResString)"This private key is for signing tokens used for CW1Support account logins into customer systems.",
				new CWSupportLoginTokenPrivateKeyEditorInfo(),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				new byte[] { 1,2,3, }),
			new BinaryRegistryItem(
				"CW1SupportLoginTokenPrivateKey",
				(NoResString)"TestCategory",
				(NoResString)"CWSupport Login Token Certificate",
				(NoResString)"This certificate is used to validate CWSupport account login token.",
				new FileUpLoaderX509CertificateRegistryEditorInfo(),
				RegistryStorageFlags.System,
				RegistryOptions.IsReadOnly,
				new byte[] { 1,2,3, })
		};

		public void Test_TheseNonSensitiveEdiRegistryItemsShouldBeSubscribed()
		{
			IRegistryItem[] nonSensitiveRegistryItems = [
				EDIDataRegistry.Instance.ABMCustomsWareMessagingChargeCode,
				EDIDataRegistry.Instance.ABMCustomsWareMessagingDiscountChargeCode,
				EDIDataRegistry.Instance.ABMFiscalRepInvoiceMessagingChargeCode,
				EDIDataRegistry.Instance.ABMFiscalRepInvoiceMessagingDiscountChargeCode,
				EDIDataRegistry.Instance.ABMMovementMessagingChargeCode,
				EDIDataRegistry.Instance.ABMMovementMessagingDiscountChargeCode,
				EDIDataRegistry.Instance.AchievableBusinessLabel,
				EDIDataRegistry.Instance.ActiveMiddlewareService,
				EDIDataRegistry.Instance.ActivitySubtypeAssignments,
				EDIDataRegistry.Instance.AirlineMessagingDiscountAndRemitChargeCode,
				EDIDataRegistry.Instance.AirlineMessagingFHLChargeCode,
				EDIDataRegistry.Instance.AirlineMessagingFSUChargeCode,
				EDIDataRegistry.Instance.AirlineMessagingFWBChargeCode,
				EDIDataRegistry.Instance.AirlineMessagingTraxonLicenceIdentifier,
				EDIDataRegistry.Instance.AllowedInvoicingBranches,
				EDIDataRegistry.Instance.AllowPayrollMetricsServiceTask,
				EDIDataRegistry.Instance.AllowSaveUpgradePackageToDisk,
				EDIDataRegistry.Instance.AllSystemMessagesViaEhubReleaseBuilds,
				EDIDataRegistry.Instance.AmbiguousContactLoginNotificationMessageTemplate,
				EDIDataRegistry.Instance.AmountOfBusinessWonLabel,
				EDIDataRegistry.Instance.AutoDeployProcessBatchMaxDuration,
				EDIDataRegistry.Instance.AutoDeployProcessBatchRunningInterval,
				EDIDataRegistry.Instance.AvalaraCompanyCode,
				EDIDataRegistry.Instance.AvalaraIntegrationSettingStatus,
				EDIDataRegistry.Instance.AvalaraSalesTaxChargeCode,
				EDIDataRegistry.Instance.AvalaraWebTimeoutSeconds,
				EDIDataRegistry.Instance.AWSPrivateCAListManager,
				EDIDataRegistry.Instance.AYCTriggerTypeSettings,
				EDIDataRegistry.Instance.AzpsApprovedForMyAccount,
				EDIDataRegistry.Instance.AzureApplicationRedirectUrlSyncInterval,
				EDIDataRegistry.Instance.B2CConfigurationRepositoryGitHubAppName,
				EDIDataRegistry.Instance.B2CConfigurationRepositoryOwnerName,
				EDIDataRegistry.Instance.BillingPriceRoundingParams,
				EDIDataRegistry.Instance.BillingStlGlobalPriceLists,
				EDIDataRegistry.Instance.BillingSystemChargeCodeMappings,
				EDIDataRegistry.Instance.BillingTranslationExportSupportedLanguages,
				EDIDataRegistry.Instance.BillingUnitCountAdjustments,
				EDIDataRegistry.Instance.BillingUsageCategoryCodes,
				EDIDataRegistry.Instance.BorderWiseConfirmWebAccessUrl,
				EDIDataRegistry.Instance.BorderWiseMaximumDeviceCount,
				EDIDataRegistry.Instance.BorderWiseMaximumLicenceRelocationsPerMonth,
				EDIDataRegistry.Instance.BorderWiseNewOrganisationEmailNotificationGroup,
				EDIDataRegistry.Instance.BorderWisePurchasedGroups,
				EDIDataRegistry.Instance.BorderWiseRegistrationUrl,
				EDIDataRegistry.Instance.BranchForEDIServiceTasks,
				EDIDataRegistry.Instance.CancellationFeeLabel,
				EDIDataRegistry.Instance.CargoWiseNextBillingARInvoiceOnlyBranches,
				EDIDataRegistry.Instance.CargoWiseNextMinimumVersion,
				EDIDataRegistry.Instance.CertProcessingNotificationGroup,
				EDIDataRegistry.Instance.ClientMappingBillingNames,
				EDIDataRegistry.Instance.ClosedIncidentLastAssigneeNotificationPeriod,
				EDIDataRegistry.Instance.CodingTasks,
				EDIDataRegistry.Instance.CommentChargeCode,
				EDIDataRegistry.Instance.CommissionGeneratorNotificationGroup,
				EDIDataRegistry.Instance.CompetencyLearningTask,
				EDIDataRegistry.Instance.ConsolidatedBillingSettings,
				EDIDataRegistry.Instance.ConsultingRevenueLabel,
				EDIDataRegistry.Instance.ContentFinderUrl,
				EDIDataRegistry.Instance.CorporateAgreementAcknowledgementNotificationMessageTemplate,
				EDIDataRegistry.Instance.CorruptedUpgradePackageFilePath,
				EDIDataRegistry.Instance.CountryTierPriceCodeMappings,
				EDIDataRegistry.Instance.Cr8Cr9ReleaseBuilds,
				EDIDataRegistry.Instance.CurrencyLabel,
				EDIDataRegistry.Instance.CustomerServiceAwaitingResponseNotificationMessageTemplate,
				EDIDataRegistry.Instance.CustomerServiceFinalClosureAutoReplyEmailTemplate,
				EDIDataRegistry.Instance.CustomerServiceHighCriticalityIncidentGroup,
				EDIDataRegistry.Instance.CustomerServiceIncidentClosedNotificationEmailTemplates,
				EDIDataRegistry.Instance.CustomerServiceIncidentWorkItemCompletedEmailTemplates,
				EDIDataRegistry.Instance.CustomerServiceIncidentWorkItemCreatedEmailTemplates,
				EDIDataRegistry.Instance.CustomerServiceResolvedNotificationTemplates,
				EDIDataRegistry.Instance.CustomerServiceResponseNotificationReminderTemplate,
				EDIDataRegistry.Instance.CustomExpiryMessagesReleaseBuilds,
				EDIDataRegistry.Instance.CW1DvdIsoFileDownloadURL,
				EDIDataRegistry.Instance.CW1DvdZipFileDownloadURL,
				EDIDataRegistry.Instance.DatabasesRequiredReleaseBuildTesting,
				EDIDataRegistry.Instance.DataScienceBillingKafkaEnableTransactions,
				EDIDataRegistry.Instance.DataScienceBillingKafkaTopic,
				EDIDataRegistry.Instance.DataScienceCoreKafkaEnableTransactions,
				EDIDataRegistry.Instance.DataScienceCoreKafkaTopic,
				EDIDataRegistry.Instance.DataScienceIncidentRelatedKafkaEnableTransactions,
				EDIDataRegistry.Instance.DataScienceIncidentRelatedKafkaTopic,
				EDIDataRegistry.Instance.DataScienceIssuesKafkaEnableTransactions,
				EDIDataRegistry.Instance.DataScienceIssuesKafkaTopic,
				EDIDataRegistry.Instance.DataScienceProductivityKafkaEnableTransactions,
				EDIDataRegistry.Instance.DataScienceProductivityKafkaTopic,
				EDIDataRegistry.Instance.DbConnectionCrikeyServer,
				EDIDataRegistry.Instance.DefaultFilterLayoutForNotificationRoles,
				EDIDataRegistry.Instance.DefaultFilterLayoutForWebGlbPerson,
				EDIDataRegistry.Instance.DefaultFilterLayoutForWebHRJobApplicant,
				EDIDataRegistry.Instance.DefaultFilterLayoutForWebSecurity,
				EDIDataRegistry.Instance.DefaultOpportunityObjective,
				EDIDataRegistry.Instance.DeniedPartyScreeningListsDbName,
				EDIDataRegistry.Instance.DeniedPartyScreeningListsDbServerName,
				EDIDataRegistry.Instance.DepositChargeCodes,
				EDIDataRegistry.Instance.DisableMyAccountWhitelisting,
				EDIDataRegistry.Instance.DistinctEmailRequiredPageFooterText,
				EDIDataRegistry.Instance.EAdaptorElementNames,
				EDIDataRegistry.Instance.EDIBillingTestingDatabaseName,
				EDIDataRegistry.Instance.EDIBillingTestingDatabaseServerName,
				EDIDataRegistry.Instance.EDIERouterUsageDBName,
				EDIDataRegistry.Instance.EDIERouterUsageDBServerName,
				EDIDataRegistry.Instance.EDIFaxUsageDBName,
				EDIDataRegistry.Instance.EDIFaxUsageDBServerName,
				EDIDataRegistry.Instance.EDIHostingStatsUsageDBName,
				EDIDataRegistry.Instance.EDIHostingStatsUsageDBServerName,
				EDIDataRegistry.Instance.EdiKafkaBootstrapServers,
				EDIDataRegistry.Instance.EditDocumentOrganizationRegistrationMappingModule,
				EDIDataRegistry.Instance.ElasticSearchAdaptorIndex,
				EDIDataRegistry.Instance.ElasticSearchAdaptorRequestStringWithFilter,
				EDIDataRegistry.Instance.ElasticSearchAdaptorRequestUrl,
				EDIDataRegistry.Instance.ELearningDocumentBorderWiseDocumentsApiClientUrl,
				EDIDataRegistry.Instance.ELearningDocumentMyAccountCallTimeout,
				EDIDataRegistry.Instance.ELearningDocumentMyAccountUrl,
				EDIDataRegistry.Instance.ELearningDocumentNetworkSharePdfPathLocationPrefix,
				EDIDataRegistry.Instance.ELearningDocumentNetworkShareUser,
				EDIDataRegistry.Instance.ELearningDocumentRecentPDFUpdatesApiClientUrl,
				EDIDataRegistry.Instance.ELearningDocumentRecentPDFUpdatesForUpdateNotesApiClientUrl,
				EDIDataRegistry.Instance.ELearningDocumentTechnicalAdvisoryDocumentsApiClientUrl,
				EDIDataRegistry.Instance.EmailVerificationNotificationMessageTemplate,
				EDIDataRegistry.Instance.EnableAutoresponder,
				EDIDataRegistry.Instance.EnableCargoWiseNextTransitionVersionFallbackRule,
				EDIDataRegistry.Instance.EnableContentFinder,
				EDIDataRegistry.Instance.EnableDataScienceBillingSubscribers,
				EDIDataRegistry.Instance.EnableDataScienceCoreSubscribers,
				EDIDataRegistry.Instance.EnableDataScienceIncidentRelatedSubscribers,
				EDIDataRegistry.Instance.EnableDataScienceIssuesSubscribers,
				EDIDataRegistry.Instance.EnableDataScienceProductivitySubscribers,
				EDIDataRegistry.Instance.EnableFeatureControlModule,
				EDIDataRegistry.Instance.EnableIncidentAssignedStaffChangedEmailNotification,
				EDIDataRegistry.Instance.EnableIncidentManagementGroupModule,
				EDIDataRegistry.Instance.EnableIncidentSimilarityFunctionality,
				EDIDataRegistry.Instance.EnableIncidentSimilarityPreload,
				EDIDataRegistry.Instance.EnableIncidentSimilarityWebService,
				EDIDataRegistry.Instance.EnableInternalWorkItem,
				EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment,
				EDIDataRegistry.Instance.EnableMyAccountWebConfigOveridden,
				EDIDataRegistry.Instance.EnableTaxProcessorForBilling,
				EDIDataRegistry.Instance.EnableTriageEngineModule,
				EDIDataRegistry.Instance.EnableVerboseModeOnEmailProcessors,
				EDIDataRegistry.Instance.EnableWeeklyBuildCutOff,
				EDIDataRegistry.Instance.ERequestPendingApprovalNotificationMessageTemplate,
				EDIDataRegistry.Instance.ERequestsReleaseBuilds,
				EDIDataRegistry.Instance.ERequestV2ReleaseBuilds,
				EDIDataRegistry.Instance.eRouterLinkedServer,
				EDIDataRegistry.Instance.ErrorLogStackLineExtractorRegexes,
				EDIDataRegistry.Instance.ErrorReportingServiceMaxResults,
				EDIDataRegistry.Instance.ErrorReportingServiceTesting,
				EDIDataRegistry.Instance.ErrorReportingServiceURIs,
				EDIDataRegistry.Instance.EscrowIncidentConfiguration,
				EDIDataRegistry.Instance.EscrowIncidentMessage,
				EDIDataRegistry.Instance.EstExpiryDateLabel,
				EDIDataRegistry.Instance.EstExpressDelCutOffLabel,
				EDIDataRegistry.Instance.EstMaxDevHoursLabel,
				EDIDataRegistry.Instance.EstMaxMonthlyLabel,
				EDIDataRegistry.Instance.EstMaxOneOffLabel,
				EDIDataRegistry.Instance.EstMinDevHoursLabel,
				EDIDataRegistry.Instance.EstMinMonthlyLabel,
				EDIDataRegistry.Instance.EstMinOneOffLabel,
				EDIDataRegistry.Instance.EstRequestDateLabel,
				EDIDataRegistry.Instance.EstSentDateLabel,
				EDIDataRegistry.Instance.EvenLogTraceLevel,
				EDIDataRegistry.Instance.EventLogHighWaterMarkList,
				EDIDataRegistry.Instance.ExceptionKeyMatchingRegexes,
				EDIDataRegistry.Instance.ExceptionKeyRegexes,
				EDIDataRegistry.Instance.ExceptionKeyStacktraceDepths,
				EDIDataRegistry.Instance.ExternalMonitoringSqlCpuUsageExecutionPlanUri,
				EDIDataRegistry.Instance.ExternalMonitoringSqlCpuUsageLowerThreshold,
				EDIDataRegistry.Instance.ExternalMonitoringSqlExecutionPlanQueryUri,
				EDIDataRegistry.Instance.ExternalMonitoringSqlExecutionPlanRetrieveTimeout,
				EDIDataRegistry.Instance.FeatureControlCodeList,
				EDIDataRegistry.Instance.FeatureRequestEstimateAutoExpirePeriod,
				EDIDataRegistry.Instance.FeeBillingDiscountChargeCodes,
				EDIDataRegistry.Instance.GetSimilarIncidentMaxCount,
				EDIDataRegistry.Instance.GithubActionSecretNextCheckDate,
				EDIDataRegistry.Instance.GlowAccreditationPortalUri,
				EDIDataRegistry.Instance.GlowERequestPortalUri,
				EDIDataRegistry.Instance.GlowNewERequestPageUri,
				EDIDataRegistry.Instance.GlowNewInternalIncident,
				EDIDataRegistry.Instance.HandheldDevicePremiumTypes,
				EDIDataRegistry.Instance.HasGlowERequests,
				EDIDataRegistry.Instance.HostingDataStorageChargeCode,
				EDIDataRegistry.Instance.HostingDiscountChargeCode,
				EDIDataRegistry.Instance.HostingDocsStorageChargeCode,
				EDIDataRegistry.Instance.HostingNonProductionStorageChargeCode,
				EDIDataRegistry.Instance.HostingPrintServersChargeCode,
				EDIDataRegistry.Instance.HostingRemoteDevicesChargeCode,
				EDIDataRegistry.Instance.HostingStorageBufferPercentage,
				EDIDataRegistry.Instance.HostingUltraFastStorageChargeCode,
				EDIDataRegistry.Instance.HRNotificationGroup,
				EDIDataRegistry.Instance.HybridLicencingReleaseBuilds,
				EDIDataRegistry.Instance.ImplementationDefaultFromEmailAddress,
				EDIDataRegistry.Instance.ImplementationSchedulingEmailAddress,
				EDIDataRegistry.Instance.IncidentClosePromptServiceTaskLastRunTimeUtc,
				EDIDataRegistry.Instance.IncidentClosureDispositions,
				EDIDataRegistry.Instance.IncidentCriticalityStages,
				EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg,
				EDIDataRegistry.Instance.IncidentFromEmailAddress,
				EDIDataRegistry.Instance.IncidentProductsToIgnoreForceResponseSentViaEHub,
				EDIDataRegistry.Instance.IncidentSimilarityWebServiceDataBatchSize,
				EDIDataRegistry.Instance.IncidentSimilarityWebServiceUrl,
				EDIDataRegistry.Instance.IncidentStaffUnavailableSupportNotificationEnable,
				EDIDataRegistry.Instance.IncidentStaffUnavailableSupportNotificationGroup,
				EDIDataRegistry.Instance.IncidentSupportGroup,
				EDIDataRegistry.Instance.IncidentUpdateNotificationEmailTemplate,
				EDIDataRegistry.Instance.IncludeUpdateNoteUrls,
				EDIDataRegistry.Instance.InternalEnterpriseMasterOrgs,
				EDIDataRegistry.Instance.InternalIncidentLicenceSettings,
				EDIDataRegistry.Instance.InternalNotificationGroup,
				EDIDataRegistry.Instance.InvoiceAttachmentDocType,
				EDIDataRegistry.Instance.InvoicingProcessingFeeLookup,
				EDIDataRegistry.Instance.ISAlertsServiceAccount,
				EDIDataRegistry.Instance.IssueWorkItemCreationThresholdClientVisible,
				EDIDataRegistry.Instance.IssueWorkItemCreationThresholdNonClientVisible,
				EDIDataRegistry.Instance.LastSuccessfulSyncForEDIUTC,
				EDIDataRegistry.Instance.LegacyCr8ModuleMappings,
				EDIDataRegistry.Instance.LegacyCr9ModuleMappings,
				EDIDataRegistry.Instance.LegacyMenuSectionMappings,
				EDIDataRegistry.Instance.LicenceCountryLanguages,
				EDIDataRegistry.Instance.LicenceDatabaseMasterOrgSuggestionBulkUpdateThreshold,
				EDIDataRegistry.Instance.LicenceEditionActiveList,
				EDIDataRegistry.Instance.LicenceUsageBilledPerTransaction,
				EDIDataRegistry.Instance.LinkedIncidentGridRefreshRate,
				EDIDataRegistry.Instance.LogsRequestMaximumZipSize,
				EDIDataRegistry.Instance.MachineHostNameList,
				EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiRequestTimeout,
				EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiUrl,
				EDIDataRegistry.Instance.MainRepositories,
				EDIDataRegistry.Instance.MaxErrorReportSizeInMb,
				EDIDataRegistry.Instance.MaxRowsForDistributedDataSyncRequest,
				EDIDataRegistry.Instance.MinimumAmountToBill,
				EDIDataRegistry.Instance.ModuleToResourceNameList,
				EDIDataRegistry.Instance.MonthlyUsageInvoiceComment,
				EDIDataRegistry.Instance.MonthlyUsageInvoiceDescription,
				EDIDataRegistry.Instance.MonthlyUsageProcessingFeeChargeCode,
				EDIDataRegistry.Instance.MonthlyUsageReportBreakdownComment,
				EDIDataRegistry.Instance.MyAccountCertificateAuthoritySoapRequestTemplate,
				EDIDataRegistry.Instance.MyAccountContactTermsAndConditionsContent,
				EDIDataRegistry.Instance.MyAccountEndpointBaseUrl,
				EDIDataRegistry.Instance.MyAccountHostingSiteLandingPageUrl,
				EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl,
				EDIDataRegistry.Instance.MyAccountIndexPage,
				EDIDataRegistry.Instance.MyAccountKeyRotationIntervalMinutes,
				EDIDataRegistry.Instance.MyAccountLoggerFileTargetPath,
				EDIDataRegistry.Instance.MyAccountLoggerKafkaTargetBrokers,
				EDIDataRegistry.Instance.MyAccountLoggerKafkaTargetTopic,
				EDIDataRegistry.Instance.MyAccountPhysicalServerPath,
				EDIDataRegistry.Instance.MyAccountReportsDownloadNotificationEmailTemplate,
				EDIDataRegistry.Instance.MyAccountReportsDownloadURL,
				EDIDataRegistry.Instance.MyAccountReportsShareFolderPath,
				EDIDataRegistry.Instance.MyAccountSiteRootUrl,
				EDIDataRegistry.Instance.MyAccountTermsAndConditionsContent,
				EDIDataRegistry.Instance.MyAccountTermsAndConditionsNotificationEmailSenderAddress,
				EDIDataRegistry.Instance.MyAccountTermsAndConditionsNotificationEmailSenderName,
				EDIDataRegistry.Instance.MyAccountTermsAndConditionsNotificationEmailTemplate,
				EDIDataRegistry.Instance.MyAccountTrustedServices,
				EDIDataRegistry.Instance.MyAccountUserVerifyAgreementEmailTemplate,
				EDIDataRegistry.Instance.NeoUpgradeLicences,
				EDIDataRegistry.Instance.NewIncidentsDaysToReply,
				EDIDataRegistry.Instance.NotificationGroupForADETask,
				EDIDataRegistry.Instance.NumberOfEmployeesLabel,
				EDIDataRegistry.Instance.NZCustomsJobChargeCode,
				EDIDataRegistry.Instance.NZCustomsMessageChargeCode,
				EDIDataRegistry.Instance.OdplDiscountChargeCode,
				EDIDataRegistry.Instance.OdplHybridDiscountChargeCode,
				EDIDataRegistry.Instance.OdplHybridUsageChargeCode,
				EDIDataRegistry.Instance.OdplSurchargeChargeCode,
				EDIDataRegistry.Instance.OdplUsageChargeCode,
				EDIDataRegistry.Instance.OnboardingNotificationGroup,
				EDIDataRegistry.Instance.OpportunityExchangeRateCompany,
				EDIDataRegistry.Instance.OrgMembershipTypes,
				EDIDataRegistry.Instance.OrgNameChangeNotificationAddresses,
				EDIDataRegistry.Instance.OSNames,
				EDIDataRegistry.Instance.OSVersions,
				EDIDataRegistry.Instance.PaidUpCapitalLabel,
				EDIDataRegistry.Instance.PaymentTermsLabel,
				EDIDataRegistry.Instance.PaymentTypesAndPaymentTerms,
				EDIDataRegistry.Instance.PayrollMetricsCustomerCode,
				EDIDataRegistry.Instance.PayrollMetricsLeaveTypes,
				EDIDataRegistry.Instance.PayrollMetricsPayrollNames,
				EDIDataRegistry.Instance.PayrollMetricsServiceUri,
				EDIDataRegistry.Instance.PayrollMetricsServiceUser,
				EDIDataRegistry.Instance.PrepaidBalanceChargeCode,
				EDIDataRegistry.Instance.PrepaymentInvoiceComment,
				EDIDataRegistry.Instance.PrepaymentMargin,
				EDIDataRegistry.Instance.PriceListDocType,
				EDIDataRegistry.Instance.ProcessedShelfsNotificationGroup,
				EDIDataRegistry.Instance.ProcessingFeeExemptBillingSystems,
				EDIDataRegistry.Instance.ProcessorTypes,
				EDIDataRegistry.Instance.ProductAreaAssignments,
				EDIDataRegistry.Instance.ProductAreas,
				EDIDataRegistry.Instance.ProductDisplayCategories,
				EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID,
				EDIDataRegistry.Instance.ProductRegistrationWebAPINotificationGroup,
				EDIDataRegistry.Instance.ProductsRequiringEnterpriseCode,
				EDIDataRegistry.Instance.ProductsWithThreeDecimalBillingSummary,
				EDIDataRegistry.Instance.ProGetAssetDirectoryPathUrl,
				EDIDataRegistry.Instance.ProjectInvoiceEmailNotificationGroup,
				EDIDataRegistry.Instance.QteAcceptedDateLabel,
				EDIDataRegistry.Instance.QteAmountLabel,
				EDIDataRegistry.Instance.QteDateDeliveredLabel,
				EDIDataRegistry.Instance.QteExpiryDateLabel,
				EDIDataRegistry.Instance.QteExpressDeliveryIncludedLabel,
				EDIDataRegistry.Instance.QteHeadStartIncludedLabel,
				EDIDataRegistry.Instance.QteMaxDevHoursLabel,
				EDIDataRegistry.Instance.QteMinDevHoursLabel,
				EDIDataRegistry.Instance.QteOneOffUpfrontLabel,
				EDIDataRegistry.Instance.QtePaymentTypeLabel,
				EDIDataRegistry.Instance.QteSentDateLabel,
				EDIDataRegistry.Instance.QualityIterationAssignments,
				EDIDataRegistry.Instance.RedirectedEmailDomains,
				EDIDataRegistry.Instance.RedirectedOrganisations,
				EDIDataRegistry.Instance.RegisterPersonalEmailPageFooterText,
				EDIDataRegistry.Instance.RelatedIncidentsDecimalPrecision,
				EDIDataRegistry.Instance.RelatedIncidentsMaxTopToStore,
				EDIDataRegistry.Instance.RelatedIncidentsMinimumSimilarity,
				EDIDataRegistry.Instance.RelatedIncidentsMonthsToStore,
				EDIDataRegistry.Instance.RelatedIncidentsSimilarityMatrixBatchSize,
				EDIDataRegistry.Instance.RelatedIncidentsTfIdfBatchSize,
				EDIDataRegistry.Instance.RelationshipManagerCompanyLookup,
				EDIDataRegistry.Instance.ReleaseBuildTestDbName,
				EDIDataRegistry.Instance.ReleaseBuildTestDbServerName,
				EDIDataRegistry.Instance.ReleaseBuildTestRemoteCommandExecutablePath,
				EDIDataRegistry.Instance.ReleaseBuildTestResultNotificationGroup,
				EDIDataRegistry.Instance.ReleaseBuildTestWebAppHost,
				EDIDataRegistry.Instance.ReleaseBuildTestWebAppUrl,
				EDIDataRegistry.Instance.RequireCompletedCodeReviewForCheckin,
				EDIDataRegistry.Instance.ResolutionAndClosureBehaviour,
				EDIDataRegistry.Instance.RespondedUrlsLimit,
				EDIDataRegistry.Instance.ReviewTasks,
				EDIDataRegistry.Instance.RevisedPrepaymentBalanceDescription,
				EDIDataRegistry.Instance.SalesConductorContentPlaylistBuilderUrl,
				EDIDataRegistry.Instance.SalesConductorDomainUrl,
				EDIDataRegistry.Instance.SalesTaxRates,
				EDIDataRegistry.Instance.ScavengingPurgeSettings,
				EDIDataRegistry.Instance.SelectionCriteriaForUpdatedBookToAutomaticallyProcess,
				EDIDataRegistry.Instance.SelectionCriteriaForUpdatedBookToManuallyProcess,
				EDIDataRegistry.Instance.SelfAssignMessages,
				EDIDataRegistry.Instance.SendOrganizationDataToCertCapture,
				EDIDataRegistry.Instance.SendUpgradeEmailNotificationAutomatically,
				EDIDataRegistry.Instance.ServiceTypeCr8Mappings,
				EDIDataRegistry.Instance.ServiceTypeCr9Mappings,
				EDIDataRegistry.Instance.ServiceTypeMappings,
				EDIDataRegistry.Instance.ServiceTypes,
				EDIDataRegistry.Instance.SourceModules,
				EDIDataRegistry.Instance.SplitterDistance,
				EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier,
				EDIDataRegistry.Instance.StlDiscountSuspensionPolicyDefault,
				EDIDataRegistry.Instance.StlDiscountTypes,
				EDIDataRegistry.Instance.StlFreeTrialDiscounts,
				EDIDataRegistry.Instance.StlInvoiceSupportedLanguages,
				EDIDataRegistry.Instance.StlMonthlyUsageInvoiceDescription,
				EDIDataRegistry.Instance.StlPriceListExchangeRateGroups,
				EDIDataRegistry.Instance.StlRawUsageReportRefCaption,
				EDIDataRegistry.Instance.StlSurchargeChargeCode,
				EDIDataRegistry.Instance.SubscriberUpdateEConversationEmailTemplate,
				EDIDataRegistry.Instance.SurchargeLabel,
				EDIDataRegistry.Instance.SystemExpiredMessages,
				EDIDataRegistry.Instance.SystemExpiryWithinMonthMessages,
				EDIDataRegistry.Instance.SystemExpiryWithinWeekMessages,
				EDIDataRegistry.Instance.SystemManufacturers,
				EDIDataRegistry.Instance.SystemProductMappings,
				EDIDataRegistry.Instance.TasksMandatoryInShelfCreation,
				EDIDataRegistry.Instance.TcaRimEnrollmentSchemeDescriptions,
				EDIDataRegistry.Instance.TelematicsDeviceComponents,
				EDIDataRegistry.Instance.TelematicsDevicesComponentIdentifications,
				EDIDataRegistry.Instance.Tier4CountryCodes,
				EDIDataRegistry.Instance.TokenValidationServiceDiscoveryEndpoint,
				EDIDataRegistry.Instance.TotalClientRevenueLabel,
				EDIDataRegistry.Instance.TrainingSchedulingEmailAddress,
				EDIDataRegistry.Instance.TransactionChargeCodes,
				EDIDataRegistry.Instance.TransactionDiscountChargeCodes,
				EDIDataRegistry.Instance.TranslogixOrgNameChangeNotificationAddresses,
				EDIDataRegistry.Instance.TraverseMyAccountIncidentsDaysBack,
				EDIDataRegistry.Instance.TrustedMessagingUserAgreementCheckBypassProducts,
				EDIDataRegistry.Instance.TXIOutageStartTime,
				EDIDataRegistry.Instance.TXOOutageStartTime,
				EDIDataRegistry.Instance.UnregisteredDevicePremiumTypes,
				EDIDataRegistry.Instance.UnsubscriberEmailTemplate,
				EDIDataRegistry.Instance.UsageBillingSettings,
				EDIDataRegistry.Instance.UseAutoDeployBatchProcessorForUpgrades,
				EDIDataRegistry.Instance.UserAgreementAcknowledgementNotificationEmailSenderAddress,
				EDIDataRegistry.Instance.UserAgreementAcknowledgementNotificationEmailSenderName,
				EDIDataRegistry.Instance.UserAgreementAcknowledgementNotificationMessageTemplate,
				EDIDataRegistry.Instance.UserCreatedCompanyReleaseBuilds,
				EDIDataRegistry.Instance.UserRegistrationCompanySizeList,
				EDIDataRegistry.Instance.UserRegistrationJobRoleList,
				EDIDataRegistry.Instance.UserRegistrationNotificationGroup,
				EDIDataRegistry.Instance.UserRegistrationReasonForRequestingAccessList,
				EDIDataRegistry.Instance.UserRegistrationTypeOfBusinessList,
				EDIDataRegistry.Instance.UseTestRigOriginInIssueWorkItemCreation,
				EDIDataRegistry.Instance.ValidDaysOfGenerateLoginTokenWithOldEntCodeButton,
				EDIDataRegistry.Instance.ValidStlUsageCodesOnOdplPricelists,
				EDIDataRegistry.Instance.VersionSurchargeAdditionalPercent,
				EDIDataRegistry.Instance.VersionSurchargeChargeCode,
				EDIDataRegistry.Instance.VersionSurchargeDescription,
				EDIDataRegistry.Instance.VersionSurchargePercent,
				EDIDataRegistry.Instance.VirtualMachineDetectionKeywords,
				EDIDataRegistry.Instance.WarehouseRevenueLabel,
				EDIDataRegistry.Instance.WebSecurityProductModuleMappings,
				EDIDataRegistry.Instance.WeeklyBuildCutOffDay,
				EDIDataRegistry.Instance.WeeklyBuildCutOffTime,
				EDIDataRegistry.Instance.WinzorLicences,
				EDIDataRegistry.Instance.NetCoreBinaryLicences,
				EDIDataRegistry.Instance.WiseTechAcademyAutoLoginUrl,
				EDIDataRegistry.Instance.WordContextServiceUrl,
				EDIDataRegistry.Instance.WorkItemDefaultFixPriority,
				EDIDataRegistry.Instance.WTGWiseCloudAccessSecurityGroup,

			];

			CombineAssertions(() =>
			{
				foreach (var nonSensitiveRegistryItem in nonSensitiveRegistryItems)
				{
					Assert($"Registry item {nonSensitiveRegistryItem.Name} is not blacklisted", !StmDataSubscriber.IsRegistryItemBlacklisted(nonSensitiveRegistryItem));
					Assert($"Registry item {nonSensitiveRegistryItem.Name} is not sensitive", !StmDataSubscriber.IsRegistryItemSensitiveByMetadata(nonSensitiveRegistryItem));
				}
			});
		}

		public void Test_TheseSensitiveEdiRegistryItemsShouldNotBeSubscribed()
		{
			IRegistryItem[] sensitiveRegistryItems = [
				EDIDataRegistry.Instance.AvalaraAuthenticationProductionPassword,
				EDIDataRegistry.Instance.AvalaraAuthenticationProductionUserId,
				EDIDataRegistry.Instance.AvalaraAuthenticationSandboxPassword,
				EDIDataRegistry.Instance.AvalaraAuthenticationSandboxUserId,
				EDIDataRegistry.Instance.AzureApplicationManagementClientID,
				EDIDataRegistry.Instance.AzureApplicationManagementTenantID,
				EDIDataRegistry.Instance.AzureOpenIDConnectConfiguration,
				EDIDataRegistry.Instance.B2CSecretCheckManagementClientID,
				EDIDataRegistry.Instance.B2CSecretCheckManagementSecretClientID,
				EDIDataRegistry.Instance.B2CSecretCheckManagementTenantID,
				EDIDataRegistry.Instance.BorderWiseUmpDatabaseConnectionString,
				EDIDataRegistry.Instance.CWSupportLoginTokenPrivateKey,
				EDIDataRegistry.Instance.DataScienceBillingKafkaSaslPassword,
				EDIDataRegistry.Instance.DataScienceBillingKafkaSaslUsername,
				EDIDataRegistry.Instance.DataScienceCoreKafkaSaslPassword,
				EDIDataRegistry.Instance.DataScienceCoreKafkaSaslUsername,
				EDIDataRegistry.Instance.DataScienceIncidentRelatedKafkaSaslPassword,
				EDIDataRegistry.Instance.DataScienceIncidentRelatedKafkaSaslUsername,
				EDIDataRegistry.Instance.DataScienceIssuesKafkaSaslPassword,
				EDIDataRegistry.Instance.DataScienceIssuesKafkaSaslUsername,
				EDIDataRegistry.Instance.DataScienceProductivityKafkaSaslPassword,
				EDIDataRegistry.Instance.DataScienceProductivityKafkaSaslUsername,
				EDIDataRegistry.Instance.DeniedPartyScreeningListsDbLogin,
				EDIDataRegistry.Instance.EDIBillingTestingDatabaseLogin,
				EDIDataRegistry.Instance.EDIERouterUsageDBLogin,
				EDIDataRegistry.Instance.EDIFaxUsageDBLogin,
				EDIDataRegistry.Instance.EDIHostingStatsUsageDBLogin,
				EDIDataRegistry.Instance.ElasticSearchAdaptorPassword,
				EDIDataRegistry.Instance.ElasticSearchAdaptorUsername,
				EDIDataRegistry.Instance.ELearningDocumentNetworkSharePassword,
				EDIDataRegistry.Instance.EmailAddressBlockList,
				EDIDataRegistry.Instance.ErrorReportingServiceAccessToken,
				EDIDataRegistry.Instance.ExternalMonitoringWiseGridReportingConnectionString,
				EDIDataRegistry.Instance.InternalCW1ActivationCertificate,
				EDIDataRegistry.Instance.InternalCW1ActivationCertificatePassword,
				EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccount,
				EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword,
				EDIDataRegistry.Instance.MyAccountFormAuthenticationCookieDomain,
				EDIDataRegistry.Instance.MyAccountFormAuthenticationCookieSameSite,
				EDIDataRegistry.Instance.MyAccountFormAuthenticationCookieSSL,
				EDIDataRegistry.Instance.MyAccountGatewaySecret,
				EDIDataRegistry.Instance.MyAccountQueryOIDCClientID,
				EDIDataRegistry.Instance.MyAccountSSOJWTTokenExchangePrivateKey,
				EDIDataRegistry.Instance.PayrollMetricsServicePassword,
				EDIDataRegistry.Instance.ProGetAssetDirectoryApiKey,
				EDIDataRegistry.Instance.ProGetAssetDirectoryPassword,
				EDIDataRegistry.Instance.ProGetAssetDirectoryUserName,
				EDIDataRegistry.Instance.WTGActiveDirectoryCredentials,
			];

			CombineAssertions(() =>
			{
				foreach (var sensitiveRegistryItem in sensitiveRegistryItems)
				{
					Assert($"Registry item {sensitiveRegistryItem.Name} should be sensitive", StmDataSubscriber.IsRegistryItemBlacklisted(sensitiveRegistryItem) || StmDataSubscriber.IsRegistryItemSensitiveByMetadata(sensitiveRegistryItem));
				}
			});
		}
	}

	class StmDataSubscriberToKafkaForTest : StmDataSubscriber
	{
		readonly IProducer<string, string> kafkaProducer;

		internal StmDataSubscriberToKafkaForTest(ITableSchema table, ISubscriberDataSchema dataSchema, IProducer<string, string> kafkaProducer,
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
		public override string Code { get; } = "S~~";
		public override ITableSchema Table { get; }
		public override ISubscriberDataSchema DataSchema { get; }

		internal const string TestTopic = "test-topic";
		readonly string topic;
		protected override string Topic => topic;
		protected override IProducer<string, string> GetOrCreateKafkaProducer(ILogger logger) => kafkaProducer;
		protected override object KafkaTopicLock { get; } = new object();
		protected override int MaxChangesCount { get; }
		protected override bool UseTransactions { get; }

		public override bool IsRequired() => throw new NotImplementedException();

		internal const int MaxKafkaMessageSizeForTest = 1_000_000;
		protected override int MaxKafkaMessageSize => MaxKafkaMessageSizeForTest;
	}
}
