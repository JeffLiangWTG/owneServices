using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Universal;
using Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing
{
	public class GlobalEInvoicingBuilderTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var countryFactoryMock = CountryObjectFactoryMock(transactionBatch);
			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: CreateMockBatchDataLoader().Object);
			var (eInvoice, validationErrors, validationWarnings) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();

			AssertEquals("MessagingSystem", "Hehe Electronic invoicing system_MySystem", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);
			AssertEquals("MessageType", "REQ_MyType", eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
			AssertEquals("BatchNumber", "12345_MyNumber", eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber);
			AssertEquals("Payload", "PHhtbD5IZWxsbyE8L3htbD4=", eInvoice.Payload);
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		public void TestCreate_WithManyTransactionsInBatch_WithoutXUT()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance));
			transactionBatch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance));
			transactionBatch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance));

			var countryFactoryMock = CountryObjectFactoryMock(transactionBatch);
			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: CreateMockBatchDataLoader().Object);
			var (eInvoice, _, _) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();
			var expectedPayload = TextStream + GetManyTransactionContent(3);
			var actualPayload = Encoding.UTF8.GetString(Convert.FromBase64String(eInvoice.Payload));
			AssertEquals(expectedPayload, actualPayload);
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		public void TestCreate_Throws_WhenMoreThanOneTransactionInBatch_AndXUT_SingleTransactionStrategy()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance));
			transactionBatch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance));

			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.GEIMessageShouldIncludeUniversalTransaction(
				It.IsAny<TransactionBatch>(),
				It.IsAny<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest>()))
				  .Returns(IncludeUniversalTransactionStrategy.SingleTransaction);
			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: CreateMockBatchDataLoader().Object);
			AssertExceptionThrown<ArgumentException>(
				"It should throw when multiple transaction present for single transaction strategy",
				GlobalEInvoicingBuilder.ExceptionMessageForSingleTransactionOnly,
				() => (globalBuilder as IGlobalElectronicInvoiceBuilder).Create()
			);
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		public void TestCreate_WhenMoreThanOneTransactionInBatch_AndXUT_BatchedTransactionsStrategy()
		{
			var testTransactionNames = new string[] { "T1", "T2", "T3", "T4 Long name" };
			var eInvoice = CreateFromBatch(
				IncludeUniversalTransactionStrategy.BatchedTransactions,
				testTransactionNames);

			AssertNotNull(eInvoice);
			AssertNotNull(eInvoice.TransactionBatch);
			AssertNotNull(eInvoice.TransactionBatch.Transactions);
			AssertEquals(testTransactionNames.Length, eInvoice.TransactionBatch.Transactions.Length);
			AssertNullOrEmpty(eInvoice.Transaction.ToString());

			for (int i = 0; i < testTransactionNames.Length; i++)
			{
				var expectedXML = $@"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Description>{testTransactionNames[i]}</Description>
  </TransactionInfo>
</UniversalTransaction>";
				var actualXML = eInvoice.TransactionBatch.Transactions[i].ToUTF8FromBase64();
				this.AssertXMLEqualsIgnoreChildOrder("XML not matched", expectedXML, actualXML);
			}
		}

		public void TestCreate_SingleTransactionInBatch_AndXUT_BatchedTransactionsStrategy()
		{
			var eInvoice = CreateFromBatch(
				IncludeUniversalTransactionStrategy.BatchedTransactions,
				new string[] { "T1" });

			AssertNotNull(eInvoice);
			AssertNotNull(eInvoice.TransactionBatch);
			AssertNotNull(eInvoice.TransactionBatch.Transactions);
			AssertEquals(1, eInvoice.TransactionBatch.Transactions.Length);
			AssertNullOrEmpty(eInvoice.Transaction.ToString());

			var expectedXML = $@"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Description>T1</Description>
  </TransactionInfo>
</UniversalTransaction>";
			var actualXML = eInvoice.TransactionBatch.Transactions[0].ToUTF8FromBase64();
			this.AssertXMLEqualsIgnoreChildOrder("XML not matched", expectedXML, actualXML);
		}

		public void TestCreate_WhenMoreThanOneTransactionInBatch_AndXUT_NoTransactionStrategy()
		{
			var testTransactionNames = new string[] { "T1", "T2", "T3", "T4 Long name" };
			var eInvoice = CreateFromBatch(
				IncludeUniversalTransactionStrategy.NoTransaction,
				testTransactionNames);

			AssertNotNull(eInvoice);
			AssertNull(eInvoice.TransactionBatch?.Transactions);
			AssertNullOrEmpty(eInvoice.Transaction.ToString());
		}

		public void TestCreate_WhenOneTransactionInBatch_AndXUT_SingleTransactionStrategy()
		{
			var testTransactionNames = new string[] { "T1" };
			var eInvoice = CreateFromBatch(
				IncludeUniversalTransactionStrategy.SingleTransaction,
				testTransactionNames);

			AssertNotNull(eInvoice);
			AssertNull(eInvoice?.TransactionBatch?.Transactions);
			AssertNotNullOrEmpty(eInvoice.Transaction.ToString());

			var expectedXML = $@"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Description>T1</Description>
  </TransactionInfo>
</UniversalTransaction>";
			var actualXML = eInvoice.Transaction.ToUTF8FromBase64();
			this.AssertXMLEqualsIgnoreChildOrder("XML not matched", expectedXML, actualXML);
		}

		GlobalElectronicInvoicing CreateFromBatch(IncludeUniversalTransactionStrategy strategy,
			string[] transactionDescriptions = null)
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			transactionDescriptions = transactionDescriptions ?? new string[] { "T1", "T2" };
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			foreach (var desc in transactionDescriptions)
			{
				transactionBatch.TransactionCollection.Add(
					new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
					{
						Description = desc,
					});
			}

			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.GEIMessageShouldIncludeUniversalTransaction(
				It.IsAny<TransactionBatch>(),
				It.IsAny<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest>()))
				  .Returns(strategy);

			var globalBuilder = new GlobalEInvoicingBuilder(
				countryFactoryMock.Object,
				"12345",
				transactionBatch,
				loggerCreattorMock.Object.CreateLogger,
				dataLoader: CreateMockBatchDataLoader().Object);
			(var eInvoice, _, _) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();

			AssertLoggerCreatorMock(loggerCreattorMock);

			return eInvoice;
		}

		#region TestModifyUniversalTransactionBeforeSettingGEI

		public void TestModifyUniversalTransactionBeforeGEI_IsCalled_WhenSettingUniversalTransaction_ForSingleTransaction()
		{
			// Arrange
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var universalStrategy = IncludeUniversalTransactionStrategy.SingleTransaction;
			countryFactoryMock.Setup(x => x.GEIMessageShouldIncludeUniversalTransaction(It.IsAny<TransactionBatch>(), It.IsAny<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest>())).Returns(universalStrategy);

			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, () => new Logger(), dataLoader: CreateMockBatchDataLoader().Object);

			// Act
			var (eInvoice, validationErrors, validationWarnings) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();

			// Assert
			AssertNotNull(eInvoice);
			AssertEquals(string.Empty, validationErrors.ToString());
			AssertEquals(string.Empty, validationWarnings.ToString());
			countryFactoryMock.Verify(
				x => x.ModifyUniversalTransactionBeforeGEI(transactionBatch, It.IsAny<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest>()),
				Times.Once()
			);
		}

		public void TestModifyUniversalTransactionBeforeGEI_IsCalled_WhenSettingUniversalTransaction_ForBatchedTransactions()
		{
			// Arrange
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var universalStrategy = IncludeUniversalTransactionStrategy.BatchedTransactions;
			countryFactoryMock.Setup(x => x.GEIMessageShouldIncludeUniversalTransaction(It.IsAny<TransactionBatch>(), It.IsAny<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest>())).Returns(universalStrategy);

			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, () => new Logger(), dataLoader: CreateMockBatchDataLoader().Object);

			// Act
			var (eInvoice, validationErrors, validationWarnings) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();

			// Assert
			AssertNotNull(eInvoice);
			AssertEquals(string.Empty, validationErrors.ToString());
			AssertEquals(string.Empty, validationWarnings.ToString());
			countryFactoryMock.Verify(
				x => x.ModifyUniversalTransactionBeforeGEI(transactionBatch, It.IsAny<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest>()),
				Times.Once()
			);
		}

		public void TestModifyUniversalTransactionBeforeGEI_IsNotCalled_WhenSettingUniversalTransaction_ForNoTransaction()
		{
			// Arrange
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var universalStrategy = IncludeUniversalTransactionStrategy.NoTransaction;
			countryFactoryMock.Setup(x => x.GEIMessageShouldIncludeUniversalTransaction(It.IsAny<TransactionBatch>(), It.IsAny<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest>())).Returns(universalStrategy);

			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, () => new Logger(), dataLoader: CreateMockBatchDataLoader().Object);

			// Act
			var (eInvoice, validationErrors, validationWarnings) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();

			// Assert
			AssertNotNull(eInvoice);
			AssertEquals(string.Empty, validationErrors.ToString());
			AssertEquals(string.Empty, validationWarnings.ToString());
			countryFactoryMock.Verify(
				x => x.ModifyUniversalTransactionBeforeGEI(transactionBatch, It.IsAny<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest>()),
				Times.Never()
			);
		}

		public void TestModifyUniversalTransactionBeforeGEI_RemovesShipments_WhenSettingUniversalTransaction_ForBatchedTransactions()
		{
			// Arrange
			var transactionWithManyShipments = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { Number = "0001" };
			transactionWithManyShipments.SetShipmentCollection(() => new List<UniversalDataBuss.DataObjects.Universal.Shipment>()
				{
					new UniversalDataBuss.DataObjects.Universal.Shipment() { VesselName = "Abigale" },
					new UniversalDataBuss.DataObjects.Universal.Shipment() { VesselName = "Brooke" },
				}
			);
			var transactionWithOneShipment = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { Number = "0002" };
			transactionWithOneShipment.SetShipmentCollection(() => new List<UniversalDataBuss.DataObjects.Universal.Shipment>()
				{
					new UniversalDataBuss.DataObjects.Universal.Shipment() { VesselName = "Charlie" },
				}
			);
			var transactionWithNoShipments = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance) { Number = "0003" };

			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionWithManyShipments);
			transactionBatch.TransactionCollection.Add(transactionWithOneShipment);
			transactionBatch.TransactionCollection.Add(transactionWithNoShipments);

			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var universalStrategy = IncludeUniversalTransactionStrategy.BatchedTransactions;
			countryFactoryMock.Setup(x => x.GEIMessageShouldIncludeUniversalTransaction(It.IsAny<TransactionBatch>(), It.IsAny<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest>())).Returns(universalStrategy);
			countryFactoryMock
				.Setup(x => x.ModifyUniversalTransactionBeforeGEI(transactionBatch, It.IsAny<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest>()))
				.Callback((TransactionBatch b, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest h) => GlobalEInvoicingBuilder.RemoveShipmentCollection(b, h));

			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, () => new Logger(), dataLoader: CreateMockBatchDataLoader().Object);

			// Act
			var (eInvoice, validationErrors, validationWarnings) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();

			// Assert
			AssertEquals(string.Empty, validationErrors.ToString());
			AssertEquals(string.Empty, validationWarnings.ToString());

			CombineAssertions(() =>
			{
				AssertEquals(3, eInvoice.TransactionBatch.Transactions.Length);
				AssertMultilineASCIIEquals(@"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Number>0001</Number>
  </TransactionInfo>
</UniversalTransaction>", eInvoice.TransactionBatch.Transactions[0].ToUTF8FromBase64());
				AssertMultilineASCIIEquals(@"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Number>0002</Number>
  </TransactionInfo>
</UniversalTransaction>", eInvoice.TransactionBatch.Transactions[1].ToUTF8FromBase64());
				AssertMultilineASCIIEquals(@"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <Number>0003</Number>
  </TransactionInfo>
</UniversalTransaction>", eInvoice.TransactionBatch.Transactions[2].ToUTF8FromBase64());
			});
		}

		public void TestRemoveShipmentCollection_DoesNotThrow_ForNullParameters()
		{
			AssertNoExceptionThrown(() => GlobalEInvoicingBuilder.RemoveShipmentCollection(null, null));
			AssertNoExceptionThrown(() => GlobalEInvoicingBuilder.RemoveShipmentCollection(new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance), null));
			AssertNoExceptionThrown(() => GlobalEInvoicingBuilder.RemoveShipmentCollection(null, new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest()));
		}

		#endregion

		public void TestGEIMessageType()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var xmlWriterMock = new Mock<ITransactionBatchToPayloadWriter>();

			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();

			countryFactoryMock.Setup(x => x.GetTransactionBatchToPayloadWriter()).Returns(xmlWriterMock.Object);
			countryFactoryMock.Setup(x => x.GetMessageType(transactionBatch, It.IsAny<AccEInvoicingBatch>())).Returns<TransactionBatch, AccEInvoicingBatch>((t, y) =>
			{
				return "GEX";
			});

			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: CreateMockBatchDataLoader().Object);

			var (eInvoice, validationErrors, validationWarnings) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();

			xmlWriterMock.Verify(x => x.WritePayloadToStream(transactionBatch, It.IsNotNull<Stream>(), "GEX", It.IsNotNull<AccEInvoicingBatch>(), It.IsNotNull<INotifications>(), It.IsNotNull<INotifications>()), Times.Once);

			AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType), "GEX", eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		public void TestGEIMessageNullPayloadWriter()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var transactionBatch = CreateTestTransactionBatchMatchingMockBranchCode();
			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: CreateMockBatchDataLoader().Object);

			var (eInvoice, validationErrors, _) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();

			AssertNotNull(nameof(eInvoice), eInvoice);
			AssertEquals(nameof(eInvoice.Payload), string.Empty, eInvoice.Payload);
			AssertEquals(nameof(validationErrors), string.Empty, validationErrors.ToString());
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		public void TestGEIMessageBranchAndCompany_WithNonNullBranch()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var transactionBatch = CreateTestTransactionBatchMatchingMockBranchCode();
			var dataLoaderMock = CreateMockBatchDataLoader();
			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: dataLoaderMock.Object);

			var (eInvoice, validationErrors, _) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();

			AssertNotNull(nameof(eInvoice), eInvoice);
			AssertEquals(nameof(validationErrors), string.Empty, validationErrors.ToString());
			AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.BranchCode), "B99", eInvoice.Header.ElectronicInvoiceBatchRequest.BranchCode);
			AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.CompanyCode), "C99", eInvoice.Header.ElectronicInvoiceBatchRequest.CompanyCode);
			dataLoaderMock.Verify(x => x.LoadBranchAndCompany(transactionBatch), Times.Once);
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		public void TestGEIMessageBranchAndCompany_WithNullBranchAndError()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var transactionBatch = CreateTestTransactionBatchMatchingMockBranchCode();
			var dataLoaderMock = new Mock<ITransactionBatchDataLoader>();
			dataLoaderMock.Setup(x => x.LoadBranchAndCompany(It.IsAny<TransactionBatch>())).Returns<TransactionBatch>(_ => (null, "Some failure loading branch from transaction batch"));

			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: dataLoaderMock.Object);

			var (eInvoice, validationErrors, _) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();

			AssertNull(nameof(eInvoice), eInvoice);
			AssertEquals(nameof(validationErrors), "Some failure loading branch from transaction batch", validationErrors.ToString());
			dataLoaderMock.Verify(x => x.LoadBranchAndCompany(transactionBatch), Times.Once);
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		public void TestGEIMessageIsProductionSystem()
		{
			// Arrange
			var loggerCreatorMock = GetLoggerCreatorMock();
			var defaultRegistryValue = AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.Value;
			AssertEquals("Precondition", AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, defaultRegistryValue);

			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance));
			transactionBatch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance));

			var dataLoaderMock = new Mock<ITransactionBatchDataLoader>();
			dataLoaderMock.Setup(x => x.LoadBranchAndCompany(It.IsAny<TransactionBatch>())).Returns<TransactionBatch>(_ => (GlbBranch.CurrentBranch, null));

			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreatorMock.Object.CreateLogger, dataLoader: dataLoaderMock.Object);

			// Assert
			ExecuteWithTemporaryRegistryValue(GlbCompany.CurrentCompany.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysProductionSystem, DatabaseTypes.Codes.Training, true, globalBuilder);
			AssertLoggerCreatorMock(loggerCreatorMock);
			ExecuteWithTemporaryRegistryValue(GlbCompany.CurrentCompany.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysTestSystem, (DatabaseTypes.Codes.Production), false, globalBuilder);
			ExecuteWithTemporaryRegistryValue(GlbCompany.CurrentCompany.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, DatabaseTypes.Codes.Production, true, globalBuilder);
			ExecuteWithTemporaryRegistryValue(GlbCompany.CurrentCompany.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, DatabaseTypes.Codes.Test, false, globalBuilder);

			void ExecuteWithTemporaryRegistryValue(Guid companyPk, string code, string licenceType, bool expectedValue, IGlobalElectronicInvoiceBuilder globalBuilder)
			{
				LicenceTypeChanger.SetSystemLicence(licenceType);
				using (AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, code))
				{
					var (eInvoice, _, _) = globalBuilder.Create();
					AssertEquals(expectedValue, eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystem);
				}
			}

			AssertLoggerCreatorMockAtLeastTwoInvocations(loggerCreatorMock);
		}

		public void TestUniversalTransaction_IsNotSet_WhenObjectFactoryReturnsFalse()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.GEIMessageShouldIncludeUniversalTransaction(
				It.IsAny<TransactionBatch>(),
				It.IsAny<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest>())).Returns(IncludeUniversalTransactionStrategy.NoTransaction);
			var transactionBatch = CreateTestTransactionBatchMatchingMockBranchCode();
			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: CreateMockBatchDataLoader().Object);

			var (eInvoice, _, _) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();

			AssertEquals(nameof(eInvoice.Transaction), string.Empty, eInvoice.Transaction);
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		public void TestUniversalTransaction_IsSet_WhenObjectFactoryReturnsTrue()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.GEIMessageShouldIncludeUniversalTransaction(
				It.IsAny<TransactionBatch>(),
				It.IsAny<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest>())).Returns(IncludeUniversalTransactionStrategy.SingleTransaction);
			var transactionBatch = CreateTestTransactionBatchMatchingMockBranchCode();
			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: CreateMockBatchDataLoader().Object);

			var (eInvoice, _, _) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();

			AssertNotNullOrEmpty(nameof(eInvoice.Transaction), eInvoice.Transaction);
			var deserialisedTransaction = UniversalDataBussExtensionsTestHelper.AssertBase64UniversalXMLIsParsableWithoutErrors<TransactionInfo>(eInvoice.Transaction, nameof(eInvoice.Transaction));
			AssertEquals("Univeral Transaction can be deserialised", transactionBatch.TransactionCollection[0].TransactionReference, deserialisedTransaction.TransactionReference);
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		public void TestGEIHeaderCredentials_AreEmpty_WhenNullCredentialLoader()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.GetCredentialsLoader()).Returns(() => null);
			var transactionBatch = CreateTestTransactionBatchMatchingMockBranchCode();
			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: CreateMockBatchDataLoader().Object);

			var (eInvoice, _, _) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();
			AssertNotNull(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials), eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials);
			AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials.Count), 0, eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials.Count);
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		public void TestGEIHeaderCredentials_AreEmpty_WhenCredentialLoaderReturnsNull()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var credentialLoaderMock = new Mock<ICredentialsLoader>();
			credentialLoaderMock.Setup(x => x.LoadForGEIRequest(It.IsAny<GlbBranch>(), It.IsAny<TransactionBatch>(), It.IsAny<ICountryEInvoicingObjectFactory>())).Returns(() => null);
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.GetCredentialsLoader()).Returns(() => credentialLoaderMock.Object);
			var transactionBatch = CreateTestTransactionBatchMatchingMockBranchCode();
			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: CreateMockBatchDataLoader().Object);

			var (eInvoice, _, _) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();
			AssertNotNull(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials), eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials);
			AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials.Count), 0, eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials.Count);
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		public void TestGEIHeaderCredentials_AreEmpty_WhenCredentialLoaderReturnsEmpty()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var credentialLoaderMock = new Mock<ICredentialsLoader>();
			credentialLoaderMock.Setup(x => x.LoadForGEIRequest(It.IsAny<GlbBranch>(), It.IsAny<TransactionBatch>(), It.IsAny<ICountryEInvoicingObjectFactory>())).Returns(() => Array.Empty<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>());
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.GetCredentialsLoader()).Returns(() => credentialLoaderMock.Object);
			var transactionBatch = CreateTestTransactionBatchMatchingMockBranchCode();
			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: CreateMockBatchDataLoader().Object);

			var (eInvoice, _, _) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();
			AssertNotNull(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials), eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials);
			AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials.Count), 0, eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials.Count);
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		public void TestGEIHeaderCredentials_AreIncluded_WhenCredentialLoaderReturnsOneCredential()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var credentialLoaderMock = new Mock<ICredentialsLoader>();
			credentialLoaderMock.Setup(x => x.LoadForGEIRequest(It.IsAny<GlbBranch>(), It.IsAny<TransactionBatch>(), It.IsAny<ICountryEInvoicingObjectFactory>()))
								.Returns(() => new[]
								{
									new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
									{
										Key = "SomeCredential",
										Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue() { Value = "S3cRe1Pa$$w0rD" }
									}
								});
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.GetCredentialsLoader()).Returns(() => credentialLoaderMock.Object);
			var transactionBatch = CreateTestTransactionBatchMatchingMockBranchCode();
			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: CreateMockBatchDataLoader().Object);

			var (eInvoice, _, _) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();
			AssertNotNull(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials), eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials);
			AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials.Count), 1, eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials.Count);
			AssertEquals(nameof(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential.Key), "SomeCredential", eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials[0].Key);
			AssertEquals(nameof(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue.Value), "S3cRe1Pa$$w0rD", eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials[0].Value.Value);
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		public void TestGEIHeaderCredentials_AreIncluded_WhenCredentialLoaderReturnsManyCredentials()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var credentialLoaderMock = new Mock<ICredentialsLoader>();
			credentialLoaderMock.Setup(x => x.LoadForGEIRequest(It.IsAny<GlbBranch>(), It.IsAny<TransactionBatch>(), It.IsAny<ICountryEInvoicingObjectFactory>()))
								.Returns(() => new[]
								{
									new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
									{
										Key = "SomeCredential",
										Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue() { Value = "S3cRe1Pa$$w0rD" }
									},
									new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential()
									{
										Key = "APassword",
										Value = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue() { Value = "UzNjUmUxUGEkJHcwckQ=", Encrypted = true, EncryptedSpecified = true }
									},
								});
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.GetCredentialsLoader()).Returns(() => credentialLoaderMock.Object);
			var transactionBatch = CreateTestTransactionBatchMatchingMockBranchCode();
			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: CreateMockBatchDataLoader().Object);

			var (eInvoice, _, _) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();
			AssertNotNull(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials), eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials);
			AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials.Count), 2, eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials.Count);
			CombineAssertions(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials) + "[0]", () =>
			{
				AssertEquals(nameof(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential.Key), "SomeCredential", eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials[0].Key);
				AssertEquals(nameof(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue.Value), "S3cRe1Pa$$w0rD", eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials[0].Value.Value);
			});
			CombineAssertions(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials) + "[1]", () =>
			{
				AssertEquals(nameof(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential.Key), "APassword", eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials[1].Key);
				AssertEquals(nameof(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue.Value), "UzNjUmUxUGEkJHcwckQ=", eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials[1].Value.Value);
				AssertEquals(nameof(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue.Encrypted), true, eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials[1].Value.Encrypted);
				AssertEquals(nameof(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialValue.EncryptedSpecified), true, eInvoice.Header.ElectronicInvoiceBatchRequest.Credentials[1].Value.EncryptedSpecified);
			});
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		#region AdditionalHeaderDataItems

		public void TestGEIHeaderAdditionalDataItems_AreNull_WhenLoadAdditionalHeaderDataItemsReturnsNull()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.GetIAdditionalDataItemsProvider()).Returns(() => null);
			var transactionBatch = CreateTestTransactionBatchMatchingMockBranchCode();
			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: CreateMockBatchDataLoader().Object);

			var (eInvoice, _, _) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();
			AssertNull(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems), eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems);
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		public void TestGEIHeaderAdditionalDataItems_AreEmpty_WhenLoadAdditionalHeaderDataItemsReturnsEmpty()
		{
			var mockIAdditionalDataItems = new Mock<IAdditionalDataItemsProvider>();
			mockIAdditionalDataItems.Setup(x => x.GetAdditionalHeaderDataItems(It.IsAny<AccEInvoicingBatch>(), It.IsAny<GlbBranch>(), It.IsAny<TransactionBatch>(), It.IsAny<ICountryEInvoicingObjectFactory>(), It.IsAny<INotifications>())).Returns(() => new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection());

			var loggerCreattorMock = GetLoggerCreatorMock();
			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.GetIAdditionalDataItemsProvider()).Returns(mockIAdditionalDataItems.Object);
			var transactionBatch = CreateTestTransactionBatchMatchingMockBranchCode();
			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: CreateMockBatchDataLoader().Object);

			var (eInvoice, _, _) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();
			AssertNotNull(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems), eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems);
			AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems.Count), 0, eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems.Count);
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		public void TestGEIHeaderAdditionalDataItems_AreIncluded_WhenLoadAdditionalHeaderDataItemsReturnsOneItem()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var mockIAdditionalDataItems = new Mock<IAdditionalDataItemsProvider>();
			mockIAdditionalDataItems.Setup(x => x.GetAdditionalHeaderDataItems(It.IsAny<AccEInvoicingBatch>(), It.IsAny<GlbBranch>(), It.IsAny<TransactionBatch>(), It.IsAny<ICountryEInvoicingObjectFactory>(), It.IsAny<INotifications>())).Returns(() => {
				var col = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection();
				var item = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem()
				{
					Key = "keyOne",
					Value = "valueOne"
				};
				col.Add(item);
				return col;
			});

			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.GetIAdditionalDataItemsProvider()).Returns(mockIAdditionalDataItems.Object);

			var transactionBatch = CreateTestTransactionBatchMatchingMockBranchCode();
			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: CreateMockBatchDataLoader().Object);

			var (eInvoice, _, _) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();
			AssertNotNull(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems), eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems);
			AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems.Count), 1, eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems.Count);
			AssertEquals(nameof(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem.Key), "keyOne", eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems[0].Key);
			AssertEquals(nameof(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem.Value), "valueOne", eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems[0].Value);
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		public void TestGEIHeaderAdditionalDataItems_AreIncluded_WhenLoadAdditionalHeaderDataItemsReturnsManyItems()
		{
			var loggerCreattorMock = GetLoggerCreatorMock();
			var mockIAdditionalDataItems = new Mock<IAdditionalDataItemsProvider>();
			mockIAdditionalDataItems.Setup(x => x.GetAdditionalHeaderDataItems(It.IsAny<AccEInvoicingBatch>(), It.IsAny<GlbBranch>(), It.IsAny<TransactionBatch>(), It.IsAny<ICountryEInvoicingObjectFactory>(), It.IsAny<INotifications>())).Returns(() =>
			{
				var col = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection();
				var item1 = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem()
				{
					Key = "keyOne",
					Value = "valueOne"
				};
				col.Add(item1);
				var item2 = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem()
				{
					Key = "keyTwo",
					Value = "valueTwo"
				};
				col.Add(item2);
				return col;
			});

			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.GetIAdditionalDataItemsProvider()).Returns(mockIAdditionalDataItems.Object);
			var transactionBatch = CreateTestTransactionBatchMatchingMockBranchCode();
			var globalBuilder = new GlobalEInvoicingBuilder(countryFactoryMock.Object, "12345", transactionBatch, loggerCreattorMock.Object.CreateLogger, dataLoader: CreateMockBatchDataLoader().Object);

			var (eInvoice, _, _) = (globalBuilder as IGlobalElectronicInvoiceBuilder).Create();
			AssertNotNull(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems), eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems);
			AssertEquals(nameof(eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems.Count), 2, eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems.Count);
			AssertEquals(nameof(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem.Key), "keyOne", eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems[0].Key);
			AssertEquals(nameof(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem.Value), "valueOne", eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems[0].Value);
			AssertEquals(nameof(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem.Key), "keyTwo", eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems[1].Key);
			AssertEquals(nameof(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem.Value), "valueTwo", eInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems[1].Value);
			AssertLoggerCreatorMock(loggerCreattorMock);
		}

		#endregion

		#region Implementation

		string TextStream => @"<xml>Hello!</xml>";
		static string GetManyTransactionContent(int count) => $"<count>{count}</count>";

		Mock<ICountryEInvoicingObjectFactory> CountryObjectFactoryMock(TransactionBatch transaction)
		{
			var xmlWriterMock = new Mock<ITransactionBatchToPayloadWriter>();
			xmlWriterMock.Setup(x => x.WritePayloadToStream(transaction, It.IsAny<Stream>(), It.IsAny<ZString>(), It.IsAny<AccEInvoicingBatch>(), It.IsAny<INotifications>(), It.IsAny<INotifications>())).Callback<TransactionBatch, Stream, ZString, AccEInvoicingBatch, INotifications, INotifications>((batch, stream, messageType, accBatch, errors, warnings) =>
			{
				var writer = new StreamWriter(stream);
				writer.Write(TextStream);
				if (batch.TransactionCollection.Count > 1)
				{
					writer.Write(GetManyTransactionContent(batch.TransactionCollection.Count));
				}
				writer.Flush();
				stream.Position = 0;
			});

			var countryFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			countryFactoryMock.Setup(x => x.GetTransactionBatchToPayloadWriter()).Returns(xmlWriterMock.Object);
			countryFactoryMock.Setup(x => x.CountryCode).Returns("Hehe");

			countryFactoryMock
				.Setup(x => x.UpdateGEIBatchRequest(It.IsAny<AccEInvoicingBatch>(), It.IsAny<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest>()))
				.Callback<AccEInvoicingBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest>((y, x) =>
			{
				x.MessagingSystem += "_MySystem";
				x.BatchNumber += "_MyNumber";
				x.MessageType += "_MyType";
			});

			return countryFactoryMock;
		}

		Mock<ITransactionBatchDataLoader> CreateMockBatchDataLoader()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "C99";
			var branchAndCompany = Factory.New<GlbBranch>();
			branchAndCompany.GB_Code = "B99";
			branchAndCompany.GB_GC = company.PK;
			var batch = Factory.New<AccEInvoicingBatch>();

			var result = new Mock<ITransactionBatchDataLoader>();
			result.Setup(x => x.LoadBranchAndCompany(It.IsAny<TransactionBatch>())).Returns<TransactionBatch>(_ => (branchAndCompany, string.Empty));
			result.Setup(x => x.LoadAccBatch(It.IsAny<GlbCompany>(), It.IsAny<string>())).Returns<GlbCompany, string>((c, bn) => (batch, string.Empty));
			return result;
		}

		TransactionBatch CreateTestTransactionBatchMatchingMockBranchCode()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch() { Code = "B99" },
				TransactionReference = TestObjectCreator.GetRandomString(10),
			};
			transaction.SetPostingJournalCollection(() => new List<PostingJournal>() { new PostingJournal(DefaultDataObjectWriterStrategy.TestInstance) });

			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transaction);
			return transactionBatch;
		}

		#endregion

		void AssertLoggerCreatorMock(Mock<ILoggerCreatorFortesting> mockLoggerCreatorFortesting)
		{
			mockLoggerCreatorFortesting.Verify(x => x.CreateLogger(), Times.Exactly(2), "should create logger twice at Create()");
		}

		void AssertLoggerCreatorMockAtLeastTwoInvocations(Mock<ILoggerCreatorFortesting> mockLoggerCreatorFortesting)
		{
			mockLoggerCreatorFortesting.Verify(x => x.CreateLogger(), Times.AtLeast(2), "should create logger At least twice at Create()");
		}

		Mock<ILoggerCreatorFortesting> GetLoggerCreatorMock()
		{
			var mock = new Mock<ILoggerCreatorFortesting>();
			mock.Setup(x => x.CreateLogger()).Returns(new Logger());
			return mock;
		}

		public interface ILoggerCreatorFortesting
		{
			INotifications CreateLogger();
		}
	}
}
