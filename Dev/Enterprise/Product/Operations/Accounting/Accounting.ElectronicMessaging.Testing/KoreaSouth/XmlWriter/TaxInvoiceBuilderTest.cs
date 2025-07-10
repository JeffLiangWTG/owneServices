using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	class TaxInvoiceBuilderTest : TestCaseWithFactory
	{
		[TestDate(2022, 03, 03, 15, 15, 00)]
		public void TestBuildTaxInvoice()
		{
			var xml = @"<TaxInvoice xmlns=""urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0"" xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">
  <ExchangedDocument>
    <ID>1234</ID>
    <IssueDateTime>20220303000000</IssueDateTime>
    <ReferencedDocument>
      <ID>1028142299</ID>
    </ReferencedDocument>
  </ExchangedDocument>
  <TaxInvoiceDocument>
    <IssueID>IssueIDAAA</IssueID>
    <TypeCode>0101</TypeCode>
    <IssueDateTime>20220303</IssueDateTime>
    <PurposeCode>02</PurposeCode>
  </TaxInvoiceDocument>
  <TaxInvoiceTradeSettlement>
    <InvoicerParty>
      <ID>InvoicerIDAAA</ID>
      <TypeCode>InvoicerTypeCodeAAA</TypeCode>
      <NameText>InvoicerNameTextAAA</NameText>
      <ClassificationCode>InvoicerClassificationCodeAAA</ClassificationCode>
      <SpecifiedOrganization>
        <TaxRegistrationID>InvoicerTaxRegistrationIDAAA</TaxRegistrationID>
      </SpecifiedOrganization>
      <SpecifiedPerson>
        <NameText>InvoicerSpecifiedPersonNameTextAAA</NameText>
      </SpecifiedPerson>
      <DefinedContact>
        <PersonNameText>InvoicerDefinedContactPersonNameAAA</PersonNameText>
        <TelephoneCommunication>InvoicerDefinedContactTelAAA</TelephoneCommunication>
        <URICommunication>InvoicerDefinedContactURICommunicationAAA</URICommunication>
      </DefinedContact>
      <SpecifiedAddress>
        <LineOneText>InvoicerSpecifiedAddressLineOneTextAAA</LineOneText>
      </SpecifiedAddress>
    </InvoicerParty>
    <InvoiceeParty>
      <ID>InvoiceeIDAAA</ID>
      <TypeCode>InvoiceeTypeCodeAAA</TypeCode>
      <NameText>InvoiceeNameTextAAA</NameText>
      <ClassificationCode>InvoiceeClassificationCodeAAA</ClassificationCode>
      <SpecifiedOrganization>
        <TaxRegistrationID>InvoiceeTaxRegistrationIDAAA</TaxRegistrationID>
        <BusinessTypeCode>InvoiceeBusinessTypeCodeAAA</BusinessTypeCode>
      </SpecifiedOrganization>
      <SpecifiedPerson>
        <NameText>InvoiceeSpecifiedPersonNameTextAAA</NameText>
      </SpecifiedPerson>
      <PrimaryDefinedContact>
        <PersonNameText>InvoiceePrimaryDefinedContactPersonNameAAA</PersonNameText>
        <TelephoneCommunication>InvoiceePrimaryDefinedContactTelAAA</TelephoneCommunication>
        <URICommunication>InvoiceePrimaryDefinedContactURICommunicationAAA</URICommunication>
      </PrimaryDefinedContact>
      <SpecifiedAddress>
        <LineOneText>InvoiceeSpecifiedAddressLineOneTextAAA</LineOneText>
      </SpecifiedAddress>
    </InvoiceeParty>
    <SpecifiedPaymentMeans>
      <TypeCode>40</TypeCode>
      <PaidAmount>1010</PaidAmount>
    </SpecifiedPaymentMeans>
    <SpecifiedMonetarySummation>
      <ChargeTotalAmount>1000</ChargeTotalAmount>
      <TaxTotalAmount>10</TaxTotalAmount>
      <GrandTotalAmount>1010</GrandTotalAmount>
    </SpecifiedMonetarySummation>
  </TaxInvoiceTradeSettlement>
  <TaxInvoiceTradeLineItem>
    <SequenceNumeric>1</SequenceNumeric>
    <InvoiceAmount>505</InvoiceAmount>
    <NameText>BBB_1</NameText>
    <PurchaseExpiryDateTime>20220304</PurchaseExpiryDateTime>
    <TotalTax>
      <CalculatedAmount>5</CalculatedAmount>
    </TotalTax>
  </TaxInvoiceTradeLineItem>
  <TaxInvoiceTradeLineItem>
    <SequenceNumeric>2</SequenceNumeric>
    <InvoiceAmount>505</InvoiceAmount>
    <NameText>BBB_2</NameText>
    <PurchaseExpiryDateTime>20220305</PurchaseExpiryDateTime>
    <TotalTax>
      <CalculatedAmount>5</CalculatedAmount>
    </TotalTax>
  </TaxInvoiceTradeLineItem>
  <TaxInvoiceTradeLineItem>
    <SequenceNumeric>1</SequenceNumeric>
    <InvoiceAmount>505</InvoiceAmount>
    <NameText>BBB_3</NameText>
    <PurchaseExpiryDateTime>20220306</PurchaseExpiryDateTime>
    <TotalTax>
      <CalculatedAmount>5</CalculatedAmount>
    </TotalTax>
  </TaxInvoiceTradeLineItem>
</TaxInvoice>";

			var validator = new Mock<TaxInvoiceValidation>();

			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_BatchNumber = 1234;
			batch.AIB_SystemCreateTimeUtc = new ZDateTime(2022, 03, 03);
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LocalTotal = 1010,
				LocalVATAmount = 10,
				LocalExVATAmount = 1000,
				FullyPaidDate = null,
				Description = "AAA",
				TransactionDate = new ZDateTime(2022, 03, 03, 12, 12, 12),
				OriginalReference = null,
			};
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal> {
				new PostingJournal {
					VATTaxID = new TaxID() { TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "CAP" } },
				},
				new PostingJournal {
					VATTaxID = new TaxID() { TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
				},
				new PostingJournal {
					VATTaxID = new TaxID() { TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "EXT" } },
				},
			});

			var additionalInfoConverter = new Mock<AdditionalInfoConverter>();
			additionalInfoConverter
				.Setup(x => x.ConvertTaxInvoiceAdditionalInfo(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>()))
				.Returns(() => new AdditionalInfo()
				{
					IssueID = "IssueIDAAA",

					InvoiceeID = "InvoiceeIDAAA",
					InvoiceeTypeCode = "InvoiceeTypeCodeAAA",
					InvoiceeNameText = "InvoiceeNameTextAAA",
					InvoiceeClassificationCode = "InvoiceeClassificationCodeAAA",
					InvoiceeTaxRegistrationID = "InvoiceeTaxRegistrationIDAAA",
					InvoiceeSpecifiedPersonNameText = "InvoiceeSpecifiedPersonNameTextAAA",
					InvoiceePrimaryDefinedContactPersonName = "InvoiceePrimaryDefinedContactPersonNameAAA",
					InvoiceePrimaryDefinedContactTel = "InvoiceePrimaryDefinedContactTelAAA",
					InvoiceePrimaryDefinedContactURICommunication = "InvoiceePrimaryDefinedContactURICommunicationAAA",
					InvoiceeSpecifiedAddressLineOneText = "InvoiceeSpecifiedAddressLineOneTextAAA",
					InvoiceeBusinessTypeCode = "InvoiceeBusinessTypeCodeAAA",

					InvoicerID = "InvoicerIDAAA",
					InvoicerTypeCode = "InvoicerTypeCodeAAA",
					InvoicerNameText = "InvoicerNameTextAAA",
					InvoicerClassificationCode = "InvoicerClassificationCodeAAA",
					InvoicerTaxRegistrationID = "InvoicerTaxRegistrationIDAAA",
					InvoicerSpecifiedPersonNameText = "InvoicerSpecifiedPersonNameTextAAA",
					InvoicerDefinedContactPersonName = "InvoicerDefinedContactPersonNameAAA",
					InvoicerDefinedContactTel = "InvoicerDefinedContactTelAAA",
					InvoicerDefinedContactURICommunication = "InvoicerDefinedContactURICommunicationAAA",
					InvoicerSpecifiedAddressLineOneText = "InvoicerSpecifiedAddressLineOneTextAAA",
					Lines = new List<TaxInvoiceTradeLineItem>()
					{
						new TaxInvoiceTradeLineItem()
						{
							JobNumber = "Job1",
							Sequence = 1,
							ReverseDate = new ZDateTime(2022,03,04),
							DescriptionText = "BBB_1",
							InvoiceAmount = "505",
							CalculatedAmount = "5",
						},
						new TaxInvoiceTradeLineItem()
						{
							JobNumber = "Job1",
							Sequence = 2,
							ReverseDate = new ZDateTime(2022,03,05),
							DescriptionText = "BBB_2",
							InvoiceAmount = "505",
							CalculatedAmount = "5",
						},
						new TaxInvoiceTradeLineItem()
						{
							JobNumber = "Job2",
							Sequence = 1,
							ReverseDate = new ZDateTime(2022,03,06),
							DescriptionText = "BBB_3",
							InvoiceAmount = "505",
							CalculatedAmount = "5",
						}
					},
					FullTypeCode = GetFullTypeCode(transactionInfo)
				});

			var processor = new TaxInvoiceBuilder(validator.Object, additionalInfoConverter.Object);
			AssertEquals(xml, processor.BuildTaxInvoice(transactionInfo, batch, new Logger()).ToString());

			AssertNoExceptionThrown(() => validator.Verify(x => x.ValidateTaxInvoice(It.IsAny<INotifications>(), It.IsAny<AdditionalInfo>(), transactionInfo), Times.Once));
			AssertNoExceptionThrown(() => additionalInfoConverter.Verify(x => x.ConvertTaxInvoiceAdditionalInfo(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>()), Times.Once));
		}

		[TestDate(2023, 01, 30, 00, 00, 00)]
		public void TestBuildTaxInvoiceWithMiscellaneousInvoice()
		{
			var xml = @"<TaxInvoice xmlns=""urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0"" xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">
  <ExchangedDocument>
    <ID>456</ID>
    <IssueDateTime>20230130000000</IssueDateTime>
    <ReferencedDocument>
      <ID>1028142299</ID>
    </ReferencedDocument>
  </ExchangedDocument>
  <TaxInvoiceDocument>
    <IssueID>IssueIDAAA</IssueID>
    <TypeCode>0101</TypeCode>
    <IssueDateTime>20220303</IssueDateTime>
    <PurposeCode>02</PurposeCode>
  </TaxInvoiceDocument>
  <TaxInvoiceTradeSettlement>
    <InvoicerParty>
      <ID>InvoicerIDAAA</ID>
      <TypeCode>InvoicerTypeCodeAAA</TypeCode>
      <NameText>InvoicerNameTextAAA</NameText>
      <ClassificationCode>InvoicerClassificationCodeAAA</ClassificationCode>
      <SpecifiedOrganization>
        <TaxRegistrationID>InvoicerTaxRegistrationIDAAA</TaxRegistrationID>
      </SpecifiedOrganization>
      <SpecifiedPerson>
        <NameText>InvoicerSpecifiedPersonNameTextAAA</NameText>
      </SpecifiedPerson>
      <DefinedContact>
        <PersonNameText>InvoicerDefinedContactPersonNameAAA</PersonNameText>
        <TelephoneCommunication>InvoicerDefinedContactTelAAA</TelephoneCommunication>
        <URICommunication>InvoicerDefinedContactURICommunicationAAA</URICommunication>
      </DefinedContact>
      <SpecifiedAddress>
        <LineOneText>InvoicerSpecifiedAddressLineOneTextAAA</LineOneText>
      </SpecifiedAddress>
    </InvoicerParty>
    <InvoiceeParty>
      <ID>InvoiceeIDAAA</ID>
      <TypeCode>InvoiceeTypeCodeAAA</TypeCode>
      <NameText>InvoiceeNameTextAAA</NameText>
      <ClassificationCode>InvoiceeClassificationCodeAAA</ClassificationCode>
      <SpecifiedOrganization>
        <TaxRegistrationID>InvoiceeTaxRegistrationIDAAA</TaxRegistrationID>
        <BusinessTypeCode>InvoiceeBusinessTypeCodeAAA</BusinessTypeCode>
      </SpecifiedOrganization>
      <SpecifiedPerson>
        <NameText>InvoiceeSpecifiedPersonNameTextAAA</NameText>
      </SpecifiedPerson>
      <PrimaryDefinedContact>
        <PersonNameText>InvoiceePrimaryDefinedContactPersonNameAAA</PersonNameText>
        <TelephoneCommunication>InvoiceePrimaryDefinedContactTelAAA</TelephoneCommunication>
        <URICommunication>InvoiceePrimaryDefinedContactURICommunicationAAA</URICommunication>
      </PrimaryDefinedContact>
      <SpecifiedAddress>
        <LineOneText>InvoiceeSpecifiedAddressLineOneTextAAA</LineOneText>
      </SpecifiedAddress>
    </InvoiceeParty>
    <SpecifiedPaymentMeans>
      <TypeCode>40</TypeCode>
      <PaidAmount>1010</PaidAmount>
    </SpecifiedPaymentMeans>
    <SpecifiedMonetarySummation>
      <ChargeTotalAmount>1000</ChargeTotalAmount>
      <TaxTotalAmount>10</TaxTotalAmount>
      <GrandTotalAmount>1010</GrandTotalAmount>
    </SpecifiedMonetarySummation>
  </TaxInvoiceTradeSettlement>
  <TaxInvoiceTradeLineItem>
    <SequenceNumeric>1</SequenceNumeric>
    <InvoiceAmount>505</InvoiceAmount>
    <NameText>BBB_1</NameText>
    <PurchaseExpiryDateTime>20220304</PurchaseExpiryDateTime>
    <TotalTax>
      <CalculatedAmount>5</CalculatedAmount>
    </TotalTax>
  </TaxInvoiceTradeLineItem>
  <TaxInvoiceTradeLineItem>
    <SequenceNumeric>2</SequenceNumeric>
    <InvoiceAmount>505</InvoiceAmount>
    <NameText>BBB_2</NameText>
    <PurchaseExpiryDateTime>20220305</PurchaseExpiryDateTime>
    <TotalTax>
      <CalculatedAmount>5</CalculatedAmount>
    </TotalTax>
  </TaxInvoiceTradeLineItem>
  <TaxInvoiceTradeLineItem>
    <SequenceNumeric>1</SequenceNumeric>
    <InvoiceAmount>505</InvoiceAmount>
    <NameText>BBB_3</NameText>
    <PurchaseExpiryDateTime>20220306</PurchaseExpiryDateTime>
    <TotalTax>
      <CalculatedAmount>5</CalculatedAmount>
    </TotalTax>
  </TaxInvoiceTradeLineItem>
</TaxInvoice>";

			var validator = new Mock<TaxInvoiceValidation>();

			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_BatchNumber = 456;
			batch.AIB_SystemCreateTimeUtc = new ZDateTime(2022, 03, 03);
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				LocalTotal = 1010,
				LocalVATAmount = 10,
				LocalExVATAmount = 1000,
				FullyPaidDate = null,
				Description = "AAA",
				TransactionDate = new ZDateTime(2022, 03, 03, 12, 12, 12),
				OriginalReference = null,
			};
			transactionInfo.SetPostingJournalCollection(() => new List<PostingJournal> {
				new PostingJournal {
					VATTaxID = new TaxID() { TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "CAP" } },
				},
				new PostingJournal {
					VATTaxID = new TaxID() { TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "RAT" } },
				},
				new PostingJournal {
					VATTaxID = new TaxID() { TaxType = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = "EXT" } },
				},
			});

			var additionalInfoConverter = new Mock<AdditionalInfoConverter>();
			additionalInfoConverter
				.Setup(x => x.ConvertTaxInvoiceAdditionalInfo(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>()))
				.Returns(() => new AdditionalInfo()
				{
					IssueID = "IssueIDAAA",

					InvoiceeID = "InvoiceeIDAAA",
					InvoiceeTypeCode = "InvoiceeTypeCodeAAA",
					InvoiceeNameText = "InvoiceeNameTextAAA",
					InvoiceeClassificationCode = "InvoiceeClassificationCodeAAA",
					InvoiceeTaxRegistrationID = "InvoiceeTaxRegistrationIDAAA",
					InvoiceeSpecifiedPersonNameText = "InvoiceeSpecifiedPersonNameTextAAA",
					InvoiceePrimaryDefinedContactPersonName = "InvoiceePrimaryDefinedContactPersonNameAAA",
					InvoiceePrimaryDefinedContactTel = "InvoiceePrimaryDefinedContactTelAAA",
					InvoiceePrimaryDefinedContactURICommunication = "InvoiceePrimaryDefinedContactURICommunicationAAA",
					InvoiceeSpecifiedAddressLineOneText = "InvoiceeSpecifiedAddressLineOneTextAAA",
					InvoiceeBusinessTypeCode = "InvoiceeBusinessTypeCodeAAA",

					InvoicerID = "InvoicerIDAAA",
					InvoicerTypeCode = "InvoicerTypeCodeAAA",
					InvoicerNameText = "InvoicerNameTextAAA",
					InvoicerClassificationCode = "InvoicerClassificationCodeAAA",
					InvoicerTaxRegistrationID = "InvoicerTaxRegistrationIDAAA",
					InvoicerSpecifiedPersonNameText = "InvoicerSpecifiedPersonNameTextAAA",
					InvoicerDefinedContactPersonName = "InvoicerDefinedContactPersonNameAAA",
					InvoicerDefinedContactTel = "InvoicerDefinedContactTelAAA",
					InvoicerDefinedContactURICommunication = "InvoicerDefinedContactURICommunicationAAA",
					InvoicerSpecifiedAddressLineOneText = "InvoicerSpecifiedAddressLineOneTextAAA",
					Lines = new List<TaxInvoiceTradeLineItem>()
					{
						new TaxInvoiceTradeLineItem()
						{
							JobNumber = "Job1",
							Sequence = 1,
							ReverseDate = new ZDateTime(2022,03,04),
							DescriptionText = "BBB_1",
							InvoiceAmount = "505",
							CalculatedAmount = "5",
						},
						new TaxInvoiceTradeLineItem()
						{
							JobNumber = "",
							Sequence = 2,
							ReverseDate = new ZDateTime(2022,03,05),
							DescriptionText = "BBB_2",
							InvoiceAmount = "505",
							CalculatedAmount = "5",
						},
						new TaxInvoiceTradeLineItem()
						{
							JobNumber = "",
							Sequence = 1,
							ReverseDate = new ZDateTime(2022,03,06),
							DescriptionText = "BBB_3",
							InvoiceAmount = "505",
							CalculatedAmount = "5",
						}
					},
					FullTypeCode = GetFullTypeCode(transactionInfo)
				});

			var processor = new TaxInvoiceBuilder(validator.Object, additionalInfoConverter.Object);
			AssertEquals(xml, processor.BuildTaxInvoice(transactionInfo, batch, new Logger()).ToString());

			AssertNoExceptionThrown(() => validator.Verify(x => x.ValidateTaxInvoice(It.IsAny<INotifications>(), It.IsAny<AdditionalInfo>(), transactionInfo), Times.Once));
			AssertNoExceptionThrown(() => additionalInfoConverter.Verify(x => x.ConvertTaxInvoiceAdditionalInfo(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>()), Times.Once));
		}

		[TestDate(2022, 03, 03, 15, 15, 00)]
		public void TestBuildTaxInvoiceEmpty()
		{
			var xml = @"<TaxInvoice xmlns=""urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0"" xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">
  <ExchangedDocument>
    <ID>0</ID>
    <IssueDateTime>20220303000000</IssueDateTime>
    <ReferencedDocument>
      <ID>1028142299</ID>
    </ReferencedDocument>
  </ExchangedDocument>
  <TaxInvoiceDocument>
    <IssueID></IssueID>
    <TypeCode></TypeCode>
    <IssueDateTime>20220303</IssueDateTime>
    <PurposeCode>02</PurposeCode>
  </TaxInvoiceDocument>
  <TaxInvoiceTradeSettlement>
    <InvoicerParty>
      <ID></ID>
      <NameText></NameText>
      <SpecifiedPerson>
        <NameText></NameText>
      </SpecifiedPerson>
    </InvoicerParty>
    <InvoiceeParty>
      <ID></ID>
      <NameText></NameText>
      <SpecifiedOrganization>
        <BusinessTypeCode></BusinessTypeCode>
      </SpecifiedOrganization>
      <SpecifiedPerson>
        <NameText></NameText>
      </SpecifiedPerson>
    </InvoiceeParty>
    <SpecifiedPaymentMeans>
      <TypeCode>40</TypeCode>
      <PaidAmount>0</PaidAmount>
    </SpecifiedPaymentMeans>
    <SpecifiedMonetarySummation>
      <ChargeTotalAmount>0</ChargeTotalAmount>
      <GrandTotalAmount>0</GrandTotalAmount>
    </SpecifiedMonetarySummation>
  </TaxInvoiceTradeSettlement>
</TaxInvoice>";

			var validator = new Mock<TaxInvoiceValidation>();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionDate = new ZDateTime(2022, 03, 03, 12, 12, 12),
				LocalExVATAmount = 0,
				LocalTotal = 0,
			};

			var additionalInfoConverter = new Mock<AdditionalInfoConverter>();
			additionalInfoConverter
				.Setup(x => x.ConvertTaxInvoiceAdditionalInfo(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>()))
				.Returns(() => new AdditionalInfo() { FullTypeCode = GetFullTypeCode(transactionInfo) });

			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_BatchNumber = 0;
			batch.AIB_SystemCreateTimeUtc = new ZDateTime(2022, 03, 03);

			var processor = new TaxInvoiceBuilder(validator.Object, additionalInfoConverter.Object);
			AssertEquals(xml, processor.BuildTaxInvoice(transactionInfo, batch, new Logger()).ToString());

			AssertNoExceptionThrown(() => validator.Verify(x => x.ValidateTaxInvoice(It.IsAny<INotifications>(), It.IsAny<AdditionalInfo>(), transactionInfo), Times.Once));
			AssertNoExceptionThrown(() => additionalInfoConverter.Verify(x => x.ConvertTaxInvoiceAdditionalInfo(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>()), Times.Once));
		}

		public void TestBuildTaxInvoiceInvalid()
		{
			var additionalInfoConverter = new Mock<AdditionalInfoConverter>();
			additionalInfoConverter
				.Setup(x => x.ConvertTaxInvoiceAdditionalInfo(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>()))
				.Returns(() => new AdditionalInfo());

			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_BatchNumber = 0;
			batch.AIB_SystemCreateTimeUtc = new ZDateTime(2022, 03, 03);
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransactionDate = new ZDateTime(2022, 03, 03, 12, 12, 12),
				LocalExVATAmount = 0,
				LocalTotal = 0,
			};

			var processor = new TaxInvoiceBuilder(new TaxInvoiceValidation(), additionalInfoConverter.Object);
			AssertEquals("<Invalid />", processor.BuildTaxInvoice(transactionInfo, batch, new Logger()).ToString());
		}

		public void TestBuildTaxInvoice_NotificationIsAlwaysAddedAsNotificationWithGroupKey()
		{
			var notificationError = new Mock<INotification>();
			notificationError.Setup(x => x.Type).Returns(CargoWise.ComponentModel.NotificationType.Error);
			notificationError.Setup(x => x.Message).Returns("ErrorNoficationMessage");

			var notificationWarnning = new Mock<INotification>();
			notificationWarnning.Setup(x => x.Type).Returns(CargoWise.ComponentModel.NotificationType.Warning);
			notificationWarnning.Setup(x => x.Message).Returns("WarningNoficationMessage");

			var notificationInfo = new Mock<INotification>();
			notificationInfo.Setup(x => x.Type).Returns(CargoWise.ComponentModel.NotificationType.Information);
			notificationInfo.Setup(x => x.Message).Returns("InfoNoficationMessage");

			var validator = new Mock<TaxInvoiceValidation>();
			validator
				.Setup(x => x.ValidateTaxInvoice(It.IsAny<INotifications>(), It.IsAny<AdditionalInfo>(), It.IsAny<TransactionInfo>()))
				.Callback<INotifications, AdditionalInfo, TransactionInfo>((validationNotifier, b, c) => {
					validationNotifier.Add(notificationError.Object);
					validationNotifier.Add(notificationWarnning.Object);
					validationNotifier.Add(notificationInfo.Object);
				});

			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ComplianceSubType = "TransactionPK",
			};

			var additionalInfoConverter = new Mock<AdditionalInfoConverter>();
			additionalInfoConverter
				.Setup(x => x.ConvertTaxInvoiceAdditionalInfo(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>()))
				.Returns(() => new AdditionalInfo() { FullTypeCode = GetFullTypeCode(transactionInfo) });

			var logger = new LoggerForTesting();
			new TaxInvoiceBuilder(validator.Object, additionalInfoConverter.Object)
				.BuildTaxInvoice(
					transactionInfo,
					Factory.NewWithValidTestData<AccEInvoicingBatch>(),
					logger
				);

			AssertEquals("PreCondition", "TransactionPK", transactionInfo.ComplianceSubType);
			AssertEquals("PreCondition", 3, logger.Notifications.Count);

			AssertNotification(logger, CargoWise.ComponentModel.NotificationType.Error, "ErrorNoficationMessage");
			AssertNotification(logger, CargoWise.ComponentModel.NotificationType.Warning, "WarningNoficationMessage");
			AssertNotification(logger, CargoWise.ComponentModel.NotificationType.Information, "InfoNoficationMessage");

			foreach (var notification in logger.Notifications)
			{
				var notificationWithGroupKey = notification as NotificationWithGroupKey;
				AssertNotNull(notificationWithGroupKey);
				AssertEquals(transactionInfo.ComplianceSubType, notificationWithGroupKey.GroupKey);
			}
		}

		[TestDate(2023, 06, 14)]
		public void TestElectronicInvoiceDataElementsConfiguration()
		{
			var xml = @"<TaxInvoice xmlns=""urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0"" xmlns:ds=""http://www.w3.org/2000/09/xmldsig#"">
  <ExchangedDocument>
    <ID>0</ID>
    <IssueDateTime>20230614000000</IssueDateTime>
    <ReferencedDocument>
      <ID>1028142299</ID>
    </ReferencedDocument>
  </ExchangedDocument>
  <TaxInvoiceDocument>
    <IssueID></IssueID>
    <TypeCode></TypeCode>
    <DescriptionText>AAA: AAAAAA</DescriptionText>
    <DescriptionText>BBB: AlienNo01</DescriptionText>
    <DescriptionText>CCC: PassportNo01</DescriptionText>
    <IssueDateTime>20220304</IssueDateTime>
    <PurposeCode>02</PurposeCode>
  </TaxInvoiceDocument>
  <TaxInvoiceTradeSettlement>
    <InvoicerParty>
      <ID></ID>
      <NameText></NameText>
      <SpecifiedPerson>
        <NameText></NameText>
      </SpecifiedPerson>
    </InvoicerParty>
    <InvoiceeParty>
      <ID></ID>
      <NameText></NameText>
      <SpecifiedOrganization>
        <BusinessTypeCode></BusinessTypeCode>
      </SpecifiedOrganization>
      <SpecifiedPerson>
        <NameText></NameText>
      </SpecifiedPerson>
    </InvoiceeParty>
    <SpecifiedPaymentMeans>
      <TypeCode>40</TypeCode>
      <PaidAmount>10</PaidAmount>
    </SpecifiedPaymentMeans>
    <SpecifiedMonetarySummation>
      <ChargeTotalAmount>10</ChargeTotalAmount>
      <GrandTotalAmount>10</GrandTotalAmount>
    </SpecifiedMonetarySummation>
  </TaxInvoiceTradeSettlement>
</TaxInvoice>";

			var invoiceType = EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice;
			var collection = new KoreaSouthEInvoicingDataElementConfigurationCollection()
			{
				new KoreaSouthEInvoicingDataElementConfiguration(invoiceType, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1, "[AAA: <InvoiceHeaderDescription>]"),
				new KoreaSouthEInvoicingDataElementConfiguration(invoiceType, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine2, "[BBB: <ForeignerRegistrationNumber>]"),
				new KoreaSouthEInvoicingDataElementConfiguration(invoiceType, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine3, "[CCC: <PassportNumber>]")
			};

			using (AccountingConfigurationRegistry.Instance.ElectronicInvoiceDataElementsConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
				{
					TransactionType = TransactionType.INV,
					TransactionDate = new ZDateTime(2022, 03, 04),
					Description = "AAAAAA",
					LocalExVATAmount = 10,
					LocalTotal = 10,
				};

				var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
				batch.AIB_BatchNumber = 0;
				batch.AIB_SystemCreateTimeUtc = new ZDateTime(2023, 06, 14);

				var additionalInfo = new AdditionalInfo()
				{
					InvoiceeAlienRegistrationNo = "AlienNo01",
					InvoiceePassportNo = "PassportNo01",
					FullTypeCode = GetFullTypeCode(transactionInfo)
				};

				var additionalInfoConverter = new Mock<AdditionalInfoConverter>();
				additionalInfoConverter
					.Setup(x => x.ConvertTaxInvoiceAdditionalInfo(It.IsAny<TransactionInfo>(), It.IsAny<AccEInvoicingBatch>()))
					.Returns(() => additionalInfo);

				var validator = new Mock<TaxInvoiceValidation>();
				var processor = new TaxInvoiceBuilder(validator.Object, additionalInfoConverter.Object);
				AssertEquals(xml, processor.BuildTaxInvoice(transactionInfo, batch, new Logger()).ToString());
			}
		}

		void AssertNotification(LoggerForTesting loggerForTesting, INotificationType expectedType, string expectedMessage)
		{
			var notification = loggerForTesting.Notifications.FirstOrDefault(x => x.Type == expectedType);
			AssertNotNull($"Expeced Notification Type should be existed[{expectedType.EnumValueName}]", notification);
			AssertEquals("Notification Message", expectedMessage, notification.Message);
		}

		ZString GetFullTypeCode(TransactionInfo transactionInfo)
		{
			return KoreaSouthEInvoicingHelper.GetTaxInvoiceDocumentTypeCode(
					"",
					transactionInfo.PostingJournalCollection?.Select(x => x.VATTaxID?.TaxType?.Code).WhereNotNull() ?? Array.Empty<ZString>(),
					transactionInfo.IsAmendment,
					transactionInfo.LocalVATAmount);
		}

		class LoggerForTesting : INotifications
		{
			public LoggerForTesting()
			{
				Notifications = new List<INotification>();
			}

			void INotifications.Add(INotification notification)
			{
				Notifications.Add(notification);
			}

			public override string ToString()
			{
				return string.Join("\r\n", Notifications.Select(x => x.Message).ToArray());
			}

			public List<INotification> Notifications { get; }
		}
	}
}
