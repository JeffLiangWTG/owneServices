using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing.Germany;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Accounting.ElectronicMessaging.Germany.Testing
{
	public class GlobalElectronicInvoiceBuilderForGermanyTest : TestCaseWithFactory
	{
		EInvoicingTestHelperForGermany eInvoicingTestHelperForGermany;

		public void TestCreateB2G()
		{
			var featureControlManager = eInvoicingTestHelperForGermany.GetFeatureControlManagerMock(true, false);
			using (ObjectFactory.Substitute(featureControlManager))
			{
				Helper.CreateCompanyAndBranch("DE1", "BRN", "DE", enableEInvoicing: true);
				var additionalData = CreateAdditionalData(transactionCategory: OrgConstants.Category.Government);
				var transactionBatch = CreateTransactionBatch(withLeitwegID: true);
				var geiBuilder = new GlobalElectronicInvoiceBuilderForGermany("1", transactionBatch, additionalData);
				var (eInvoice, errors, warnings) = geiBuilder.Create();
				AssertNullOrEmpty(errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
				var request = eInvoice.Header.ElectronicInvoiceBatchRequest;

				AssertEquals(EInvoiceAPICommandList.Codes.Request, request.MessageType);
				AssertEquals("Germany electronic invoicing system", request.MessagingSystem);
				AssertNotNullOrEmpty(eInvoice.Payload);

				var expectedPayload =
					"<MissingInXUT>\r\n" +
					"  <BuyersReference>12345</BuyersReference>\r\n" +
					"  <SellersIBANNumber>GB94BARC10201530093459</SellersIBANNumber>\r\n" +
					"</MissingInXUT>";
				AssertEquals(expectedPayload, eInvoice.Payload);

				AssertEquals(null, GetValueFromAdditionalDataItems(request.AdditionalDataItems, GermanyEInvoicingDataItems.TransactionCategory));
			}
		}

		public void TestCreateB2GinXT()
		{
			var featureControlManager = eInvoicingTestHelperForGermany.GetFeatureControlManagerMock(true, true);
			using (ObjectFactory.Substitute(featureControlManager))
			{
				Helper.CreateCompanyAndBranch("DE1", "BRN", "DE", enableEInvoicing: true);
				var additionalData = CreateAdditionalData(transactionCategory: OrgConstants.Category.Government);
				var transactionBatch = CreateTransactionBatch(withLeitwegID: true);
				var geiBuilder = new GlobalElectronicInvoiceBuilderForGermany("1", transactionBatch, additionalData);
				var (eInvoice, errors, warnings) = geiBuilder.Create();
				AssertNullOrEmpty(errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
				var request = eInvoice.Header.ElectronicInvoiceBatchRequest;

				AssertEquals(EInvoiceAPICommandList.Codes.GenerateInvoiceRequest, request.MessageType);
				AssertEquals("Germany electronic invoicing system", request.MessagingSystem);
				AssertEquals("GOV", GetValueFromAdditionalDataItems(request.AdditionalDataItems, GermanyEInvoicingDataItems.TransactionCategory));
				AssertEquals("GB94BARC10201530093459", GetValueFromAdditionalDataItems(request.AdditionalDataItems, GermanyEInvoicingDataItems.SellerIBAN));
				AssertNullOrEmptyOrWhitespace(eInvoice.Payload);
			}
		}

		public void TestCreateB2B()
		{
			var featureControlManager = eInvoicingTestHelperForGermany.GetFeatureControlManagerMock(true, false);
			using (ObjectFactory.Substitute(featureControlManager))
			{
				Helper.CreateCompanyAndBranch("DE1", "BRN", "DE", enableEInvoicing: true);
				var additionalData = CreateAdditionalData(transactionCategory: OrgConstants.Category.Business);
				var transactionBatch = CreateTransactionBatch(withLeitwegID: false);
				var geiBuilder = new GlobalElectronicInvoiceBuilderForGermany("1", transactionBatch, additionalData);
				var (eInvoice, errors, warnings) = geiBuilder.Create();
				AssertNullOrEmpty(errors.ToString());
				AssertNullOrEmpty(warnings.ToString());
				var request = eInvoice.Header.ElectronicInvoiceBatchRequest;

				AssertEquals(EInvoiceAPICommandList.Codes.GenerateInvoiceRequest, request.MessageType);
				AssertEquals("Germany electronic invoicing system", request.MessagingSystem);
				AssertEquals("BUS", GetValueFromAdditionalDataItems(request.AdditionalDataItems, GermanyEInvoicingDataItems.TransactionCategory));
				AssertEquals("GB94BARC10201530093459", GetValueFromAdditionalDataItems(request.AdditionalDataItems, GermanyEInvoicingDataItems.SellerIBAN));
				AssertNullOrEmptyOrWhitespace(eInvoice.Payload);
			}
		}

		public void TestGEIMessageIsProductionSystem()
		{
			// Arrange
			var defaultRegistryValue = AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.Value;
			AssertEquals("Precondition", AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, defaultRegistryValue);

			var createdCompany = Helper.CreateCompanyAndBranch("DE1", "BRN", "DE", enableEInvoicing: true);

			var additionalData = CreateAdditionalData(transactionCategory: OrgConstants.Category.Business);
			var transactionBatch = CreateTransactionBatch(withLeitwegID: false);

			var globalBuilder = new GlobalElectronicInvoiceBuilderForGermany("12345", transactionBatch, additionalData);

			// Assert
			ExecuteWithTemporaryRegistryValue(createdCompany.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysProductionSystem, DatabaseTypes.Codes.Training, true, globalBuilder);
			ExecuteWithTemporaryRegistryValue(createdCompany.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.AlwaysTestSystem, DatabaseTypes.Codes.Production, false, globalBuilder);
			ExecuteWithTemporaryRegistryValue(createdCompany.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, DatabaseTypes.Codes.Production, true, globalBuilder);
			ExecuteWithTemporaryRegistryValue(createdCompany.PK.ToGuid(), AccountingMasterFilesConstants.EReportingGEIMessageSystemTypeCodes.Default, DatabaseTypes.Codes.Test, false, globalBuilder);

			void ExecuteWithTemporaryRegistryValue(Guid companyPk, string registryCode, string licenceType, bool expectedValue, IGlobalElectronicInvoiceBuilder globalBuilder)
			{
				LicenceTypeChanger.SetSystemLicence(licenceType);
				using (AccountingMasterFilesRegistry.Instance.EReportingGEIMessageSystemType.SetTemporaryValue(companyPk, Guid.Empty, Guid.Empty, registryCode))
				{
					var (eInvoice, _, _) = globalBuilder.Create();
					AssertEquals(expectedValue, eInvoice.Header.ElectronicInvoiceBatchRequest.IsProductionSystem);
				}
			}
		}

		AdditionalTransactionInfoForGermanyEInvoice CreateAdditionalData(string transactionCategory = null, bool withIBAN = true)
		{
			var additionalData = new AdditionalTransactionInfoForGermanyEInvoice()
			{
				OriginalTransactionPK = Guid.NewGuid(),
				OriginalTransactionNumber = "1",
				VATRegistrationNum = "DE999999999",
				BankName = "Example Bank",
				AccountNumber = "1234567890",
				SwiftNumber = "ABCDEFG",
				IBANNumber = withIBAN ? "GB94BARC10201530093459" : null,
				TransactionCategory = transactionCategory ?? "GOV",
			};

			return additionalData;
		}

		TransactionBatch CreateTransactionBatch(bool withLeitwegID)
		{
			IDataObjectWriterStrategy allowAllWrites = new AllowAllStrategy();
			var batch = new TransactionBatch(allowAllWrites) { TransactionCollection = [] };

			var transaction = new TransactionInfo(allowAllWrites)
			{
				Branch = new Branch() { Code = "BRN" },
			};

			if (withLeitwegID)
			{
				transaction.OrganizationAddress = new OrganizationAddress(allowAllWrites);
				transaction.OrganizationAddress.SetRegistrationNumberCollection(() =>
					new List<RegistrationNumber>()
					{
						new RegistrationNumber()
						{
							Type = new RegistrationNumberType() { Code = "LID" },
							CountryOfIssue = new Country() { Code = "DE" },
							Value = "12345"
						}
					}
				);
			}

			batch.TransactionCollection.Add(transaction);
			return batch;
		}

		string GetValueFromAdditionalDataItems(GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection items, string name)
		{
			for (int i = 0; i < items.Count; i++)
			{
				var item = items[i];
				if (item != null && string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase))
				{
					return item.Value;
				}
			}
			return null;
		}

		protected EInvoicingTestHelper Helper
		{
			get { return helper ??= new EInvoicingTestHelper(TestObjectCreator); }
		}
		EInvoicingTestHelper helper;

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ??= new TestObjectCreator(Factory); }
		}
		TestObjectCreator testObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();
			eInvoicingTestHelperForGermany = new EInvoicingTestHelperForGermany();
		}

		class AllowAllStrategy : IDataObjectWriterStrategy
		{
			public bool IsAllowSet(string fieldName)
			{
				return true;
			}
		}
	}
}
