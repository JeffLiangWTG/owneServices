using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Universal;
using Enterprise.Accounting.Utility.Testing.eInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Moq;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing
{
	[TestsSubclassesOf(typeof(CountryEInvoicingObjectFactory))]
	abstract class CountryEInvoicingObjectFactoryTest : TestCaseWithFactory
	{
		protected abstract CountryEInvoicingObjectFactory GetTestCountryFactory();
		protected ICountryEInvoicingObjectFactory GetCountryFactory() => GetTestCountryFactory();

		protected virtual Type GetExpectedTransactionBatchToPayloadWriterType() => null;

		protected virtual Type GetExpectedEInvoicingDataValidatorType() => typeof(NullEInvoicingDataValidator);

		protected virtual bool IsJsonValidation => false;

		protected virtual Type GetExpectedPayloadValidationType() => null;

		protected virtual Type GetExpectedCredentialsInterfaceType() => typeof(IEInvoicingCredentialSettings);

		protected virtual Type GetExpectedCredentialsLoaderType() => null;

		protected virtual Type GetExpectedBatchCreatorType() => typeof(NoGroupingEInvoicingBatchCreator);

		protected virtual IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction => IncludeUniversalTransactionStrategy.NoTransaction;

		protected virtual PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting => new PopulateOptionalXUTFieldsSetting(false, false);

		protected virtual string ExpectedApTransactionListRequestBatchId => null;

		protected virtual ZString MessageTypeAssignedInCountryObjectFactory => ZString.Empty;

		protected virtual ZString ExpectedCommunicationTransport => EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;

		protected virtual bool? ExpectedSupportsWaitForOriginalTransactionForAmending => null;

		protected virtual Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(GlobalXUEFunctionalityProvider);

		protected virtual Type GetElectronicMessagingNotificationEmailCreatorType() => null;

		public void TestElectronicMessagingNotificationEmailCreatorType()
		{
			var expectedType = GetElectronicMessagingNotificationEmailCreatorType();

			var emailCreator = GetCountryFactory().GetElectronicMessagingNotificationEmailCreator();

			AssertType(expectedType, emailCreator);
		}

		protected virtual bool IsIEInvoicingCredentialXUEBehaviorProviderImplemented => false;

		public void TestIsIEInvoicingCredentialXUEBehaviorProviderImplemented()
		{
			var xueProvider = GetCountryFactory().GetGlobalXUEFunctionalityProvider();
			var expectedGlobalXUEFunctionalityProvider = GetExpectedGlobalXUEFunctionalityProviderInterfaceType();

			AssertType(expectedGlobalXUEFunctionalityProvider, xueProvider);
		}

		public void TestGetTransactionBatchToXmlWriter()
		{
			var writer = GetCountryFactory().GetTransactionBatchToPayloadWriter();
			var expectedWriterType = GetExpectedTransactionBatchToPayloadWriterType();

			if (expectedWriterType == null)
			{
				AssertNull(writer);
			}
			else
			{
				AssertType(expectedWriterType, writer);
			}
		}

		public void TestGetExpectedEInvoicingDataValidator()
		{
			var validator = GetCountryFactory().GetDataValidator(GlbCompany.CurrentCompany);
			var expectedValidatorType = GetExpectedEInvoicingDataValidatorType();

			if (expectedValidatorType == null)
			{
				AssertNull(validator);
			}
			else
			{
				AssertType(expectedValidatorType, validator);
			}
		}

		public void TestCountryCode()
		{
			var countryCode = GetCountryFactory().CountryCode;
			AssertNotNullOrEmpty(countryCode);
		}

		public virtual void TestEInvoicingServicePoint()
		{
			var countryFactory = GetCountryFactory();
			var countryCode = countryFactory.CountryCode;
			AssertEInvoicingServicePoint(ZString.Empty, $"XHUB_{countryCode}_EINVOICING");
			AssertEInvoicingServicePoint("SUFFIX", $"XHUB_{countryCode}_EINVOICING_SUFFIX");

			void AssertEInvoicingServicePoint(ZString suffix, ZString expectedValue)
			{
				var servicePoint = countryFactory.GetEInvoicingServicePoint(suffix);
				AssertEquals(expectedValue, servicePoint);
			}
		}

		public void TestEInvoicingServicePoint_IsOverridableViaFeatureControl()
		{
			new EInvoicingFeatureSettingsBuilder(countryCode)
				.WithTransportDestination("DIFFERNT_ENDPOINT")
				.BuildAndRegisterFeatureControlMock();
			var countryFactory = GetCountryFactory();

			var servicePointWithNoSuffix = countryFactory.GetEInvoicingServicePoint(ZString.Empty);
			AssertEquals("DIFFERNT_ENDPOINT", servicePointWithNoSuffix);

			var servicePointWithSuffix = countryFactory.GetEInvoicingServicePoint("UNUSED");
			AssertEquals("When overriden via feature control, suffix should have no effect", "DIFFERNT_ENDPOINT", servicePointWithSuffix);
		}

		public void TestCommunicationTransport()
		{
			var countryFactory = GetCountryFactory();
			AssertEquals(ExpectedCommunicationTransport, countryFactory.CommunicationTransport);
		}

		public void TestCommunicationTransport_IsOverridableViaFeatureControl()
		{
			new EInvoicingFeatureSettingsBuilder(countryCode)
				.WithTransportDelivery("USB")
				.BuildAndRegisterFeatureControlMock();
			var countryFactory = GetCountryFactory();

			AssertEquals("USB", countryFactory.CommunicationTransport);
		}

		public void TestIsPayloadValidationImplemented()
		{
			var payloadWriter = GetCountryFactory().GetTransactionBatchToPayloadWriter();
			var payloadValidation = payloadWriter?.GetPayloadValidation("");
			var payloadValidationType = GetExpectedPayloadValidationType();

			if (payloadValidation == null)
			{
				AssertNull(payloadValidationType);
			}
			else
			{
				AssertType(payloadValidationType, payloadValidation);
			}
		}

		public void TestCredentialTypes()
		{
			var credentials = GetCountryFactory().Credentials;
			var noCredentials = credentials.IsNoCredential();
			var certificateCredentials = credentials.IsCertificate();
			var passwordCredentials = credentials.IsPassword();
			var countOfCredentialTypes = new[] { noCredentials, certificateCredentials, passwordCredentials }.Count(x => x);

			AssertEquals("A country may have no credentials, certificate credentials OR password credentials. But not multiple.", 1, countOfCredentialTypes);
			Assert("A country may NOT have password and certificate credentials (this is unsupported, but logically possible in type system and business scenarios)", !(certificateCredentials && passwordCredentials));

			var baseMessage = $"{nameof(GetExpectedCredentialsInterfaceType)} must be the interface {nameof(IEInvoicingCredentialSettings)} (for no credentials) or {nameof(IEInvoicingCertificateCredentialSettings)} / {nameof(IEInvoicingPasswordCredentialSettings)}. ";
			var expectedCredentialsType = GetExpectedCredentialsInterfaceType();
			Assert(baseMessage + $"{expectedCredentialsType.FullName} is not an interface.", expectedCredentialsType.IsInterface);
			Assert(baseMessage + $"{expectedCredentialsType.FullName} is not assignable as a {nameof(IEInvoicingCredentialSettings)}.", typeof(IEInvoicingCredentialSettings).IsAssignableFrom(expectedCredentialsType));
		}

		public virtual void TestAuthorizationRecordType_CountryDoesNotImplement_IComplianceInfoElectronicInvoicing()
		{
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();
			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();

			mockICountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>())).Returns(mockICountryComplianceInfoBase.Object);

			using (ObjectFactory.Substitute(mockICountryComplianceFactory.Object))
			{
				AssertEquals(ZString.Empty, GetCountryFactory().AuthorizationRecordType);
			}
		}

		public void TestAuthorizationRecordType_CountryImplement_IComplianceInfoElectronicInvoicing()
		{
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();
			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();

			AssertTestAuthorizationRecordType_ReturnValue("XXX", "XXX");
			AssertTestAuthorizationRecordType_ReturnValue(ZString.Empty, ZString.Empty);
			AssertTestAuthorizationRecordType_ReturnValue(null, ZString.Empty);

			void AssertTestAuthorizationRecordType_ReturnValue(string recordType, string expectedRecordType)
			{
				mockICountryComplianceInfoBase.As<IComplianceInfoElectronicInvoicing>().Setup(x => x.GetAccTransactionHeaderAuthorisationRecordType()).Returns(recordType);
				mockICountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>())).Returns(mockICountryComplianceInfoBase.Object);

				using (ObjectFactory.Substitute(mockICountryComplianceFactory.Object))
				{
					AssertEquals(expectedRecordType, GetCountryFactory().AuthorizationRecordType);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestAuthorizationRecordType_GetICountryComplianceInfoBase_Parameters()
		{
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();
			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();

			mockICountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>())).Returns(mockICountryComplianceInfoBase.Object);

			using (ObjectFactory.Substitute(mockICountryComplianceFactory.Object))
			{
				var result = GetCountryFactory().AuthorizationRecordType;
				mockICountryComplianceFactory.Verify(x => x.GetICountryComplianceInfoBase(countryCode), Times.Once);
			}
		}

		public virtual void TestGetInvoiceEventMessageTargetQuery()
		{
			var ediMessage = Factory.New<EDIMessage>();
			var universalEvent = ediMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var getInvoiceEventMessageTargetQuery = GetCountryFactory().GetInvoiceEventMessageTargetQuery(universalEvent);
			Assert(getInvoiceEventMessageTargetQuery.IsEmpty);
		}

		public void TestGetBatchCreator()
		{
			var actualBatchCreator = GetCountryFactory().GetBatchCreator(GlbCompany.CurrentCompany);
			var expectedBatchCreatorType = GetExpectedBatchCreatorType();

			AssertNotNull("A batch creator is required for all E-Invoicing countries.", actualBatchCreator);
			AssertType(expectedBatchCreatorType, actualBatchCreator);
		}

		public virtual void TestGEIMessageShouldIncludeUniversalTransaction()
		{
			var expected = GEIMessageShouldIncludeUniversalTransaction;
			var actual = GetCountryFactory().GEIMessageShouldIncludeUniversalTransaction(null, null);
			AssertEquals($"Universal Transaction should have {expected} be included in GEI Messages.", expected, actual);
		}

		public virtual void TestGEIMessagePopulateOptionalXUTFieldsSetting()
		{
			var expected = GEIMessagePopulateOptionalXUTFieldsSetting;
			AssertEquals(
				$"Universal Transaction should populate ({expected}) in GEI Messages.",
				expected,
				GetCountryFactory().GEIMessagePopulateOptionalXUTFieldsSetting());
		}

		public virtual void TestModifyUniversalTransactionBeforeGEI()
		{
			var transactionBatch = CreateTransactionBatch();
			var xmlBefore = transactionBatch.ToXmlFragment();

			var gei = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest();
			GetCountryFactory().ModifyUniversalTransactionBeforeGEI(transactionBatch, gei);
			var xmlAfter = transactionBatch.ToXmlFragment();

			AssertMultilineASCIIEquals("No changes should be made to the transaction batch", xmlBefore, xmlAfter);
		}

		public virtual void TestGetMessageType()
		{
			var countryFactory = GetCountryFactory();
			var defaultMessageType = countryFactory.GetMessageType(new TransactionBatch(), null);
			AssertEquals("Precondition: defaultRequest.MessageType is empty", MessageTypeAssignedInCountryObjectFactory, defaultMessageType);
		}

		public void TestGetMessageType_IsOverridableViaFeatureControl()
		{
			new EInvoicingFeatureSettingsBuilder(countryCode)
				.WithTransportMessageType("123")
				.BuildAndRegisterFeatureControlMock();
			var countryFactory = GetCountryFactory();

			var messageType = countryFactory.GetMessageType(CreateTransactionBatchWithOneTransaction(), Factory.New<AccEInvoicingBatch>());
			AssertEquals("123", messageType);
		}

		public void TestGetExpectedCredentialsLoaderType()
		{
			var actualLoader = GetCountryFactory().GetCredentialsLoader();
			var expectedLoaderType = GetExpectedCredentialsLoaderType();

			if (expectedLoaderType == null)
			{
				AssertNull(actualLoader);
			}
			else
			{
				AssertType(expectedLoaderType, actualLoader);
			}
		}

		protected virtual Type GetExpectedAdditionalDataItemsProviderType() => null;

		public void TestGetIAdditionalDataItemsProvider()
		{
			var expectedAdditionalDataItemsType = GetExpectedAdditionalDataItemsProviderType();
			var actualAdditionalDataItemsType = GetCountryFactory().GetIAdditionalDataItemsProvider();

			if (expectedAdditionalDataItemsType == null)
			{
				AssertNull(actualAdditionalDataItemsType);
			}
			else
			{
				AssertType(expectedAdditionalDataItemsType, actualAdditionalDataItemsType);
			}
		}

		public virtual void TestApTransactionListRequestBatchId()
		{
			AssertEquals(ExpectedApTransactionListRequestBatchId, GetCountryFactory().ApTransactionListRequestBatchId);
		}

		public virtual void TestIEInvoicingCredentialXUEBehaviorProvider()
		{
			var countryFactory = GetCountryFactory();
			AssertEquals(IsIEInvoicingCredentialXUEBehaviorProviderImplemented, countryFactory.Credentials is IEInvoicingCredentialXUEBehaviorProvider);
		}

		public void TestBatchStrategy()
		{
			var countryFactory = GetTestCountryFactory() as IBatchCreatorStrategy;
			AssertEquals(ExpectedSupportsWaitForOriginalTransactionForAmending, countryFactory?.SupportsWaitForOriginalTransactionForAmending);
		}

		public void TestFeatureSettings_ReturnsEmptyObject_WhenNotConfigured()
		{
			var countryFactory = GetCountryFactory();

			CombineAssertions(() =>
			{
				AssertSequencesEqual(Enumerable.Empty<string>(), countryFactory.FeatureSettings.Features);
				AssertNullOrEmpty(countryFactory.FeatureSettings.Transport.Delivery);
				AssertNullOrEmpty(countryFactory.FeatureSettings.Transport.Destination);
				AssertNullOrEmpty(countryFactory.FeatureSettings.Transport.MessageType);
				AssertSequencesEqual(Enumerable.Empty<KeyValuePair<string, Newtonsoft.Json.Linq.JToken>>(), countryFactory.FeatureSettings.CountrySpecific);
			});
		}

		public void TestFeatureSettings_ReturnsThisCountryObject_WhenConfigured()
		{
			var featureSettings = new[]
			{
				new EInvoicingFeatureSettingsBuilder(countryCode)
					.WithFeatures(new[] { "Feature1", "ThirdFeature" })
					.WithTransportMessageType("TYP")
					.WithTransportDestination("NHUB_NN_EINVOICINGv3.11")
					.WithTransportDelivery("BUS")
					.WithCountrySpecificObject(new Newtonsoft.Json.Linq.JObject(new Newtonsoft.Json.Linq.JProperty("Value", "This")))
				,
				new EInvoicingFeatureSettingsBuilder("__")
					.WithFeatures(new[] { "FeatureTheSecond" })
					.WithTransportMessageType("PYT")
					.WithTransportDestination("SOME_OTHER_DESTINATION")
					.WithTransportDelivery("TRK")
					.WithCountrySpecificObject(new Newtonsoft.Json.Linq.JObject(new Newtonsoft.Json.Linq.JProperty("Value", "Other")))
			};
			featureSettings.BuildAllAndRegisterFeatureControlMock();
			var countryFactory = GetCountryFactory();

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "Feature1", "ThirdFeature" }, countryFactory.FeatureSettings.Features);
				AssertEquals("BUS", countryFactory.FeatureSettings.Transport.Delivery);
				AssertEquals("NHUB_NN_EINVOICINGv3.11", countryFactory.FeatureSettings.Transport.Destination);
				AssertEquals("TYP", countryFactory.FeatureSettings.Transport.MessageType);
				AssertEquals("This", countryFactory.FeatureSettings.CountrySpecific.Value<string>("Value"));
			});
		}

		public void TestFeatureSettings_ReturnsNewObject_WhenFeatureControlChanges()
		{
			var firstSettings = new EInvoicingFeatureSettingsBuilder(countryCode)
					.WithFeatures(new[] { "Feature1", "ThirdFeature" })
					.WithTransportMessageType("TYP")
					.WithTransportDestination("NHUB_NN_EINVOICINGv3.11")
					.WithTransportDelivery("BUS")
					.WithCountrySpecificObject(new Newtonsoft.Json.Linq.JObject(new Newtonsoft.Json.Linq.JProperty("Value", "This")));

			var mockSettings1 = firstSettings.BuildAsMock();
			var mock = new Mock<IFeatureControlManager>();
			mock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.AccountingEInvoicingConfiguration, CancellationToken.None)).Returns(System.Threading.Tasks.Task.FromResult(mockSettings1.Object));
			ObjectFactory.Substitute(mock.Object);

			var countryFactory = GetCountryFactory();
			AssertEquals("Precondition: FeatureSettings is working for first config object", "BUS", countryFactory.FeatureSettings.Transport.Delivery);

			var secondSettings = new EInvoicingFeatureSettingsBuilder(countryCode)
					.WithFeatures(new[] { "FeatureTheSecond" })
					.WithTransportMessageType("PYT")
					.WithTransportDestination("SOME_OTHER_DESTINATION")
					.WithTransportDelivery("TRK")
					.WithCountrySpecificObject(new Newtonsoft.Json.Linq.JObject(new Newtonsoft.Json.Linq.JProperty("Value", "Other")));
			var mockSettings2 = secondSettings.BuildAsMock();
			mock.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.AccountingEInvoicingConfiguration, CancellationToken.None)).Returns(System.Threading.Tasks.Task.FromResult(mockSettings2.Object));

			CombineAssertions("After feature data changes, the ObjectFactory should switch to new settings.", () =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "FeatureTheSecond" }, countryFactory.FeatureSettings.Features);
				AssertEquals("TRK", countryFactory.FeatureSettings.Transport.Delivery);
				AssertEquals("SOME_OTHER_DESTINATION", countryFactory.FeatureSettings.Transport.Destination);
				AssertEquals("PYT", countryFactory.FeatureSettings.Transport.MessageType);
				AssertEquals("Other", countryFactory.FeatureSettings.CountrySpecific.Value<string>("Value"));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			invoice = Factory.NewWithValidTestData<ARInvoice>();
			countryCode = GetCountryFactory().CountryCode;
		}

		protected ZString countryCode;
		protected ARInvoice invoice;

		protected TransactionBatch CreateTransactionBatchWithOneTransaction()
		{
			var t1 = new TransactionInfo(DefaultDataObjectWriterStrategy.Instance) { Ledger = "AR", TransactionType = TransactionType.INV, Number = "12345", LocalExVATAmount = 100m, OSTotal = 150m };

			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.Instance);
			transactionBatch.TransactionCollection = [t1];
			return transactionBatch;
		}

		protected TransactionBatch CreateTransactionBatch()
		{
			var t1 = new TransactionInfo(DefaultDataObjectWriterStrategy.Instance) { Ledger = "AR", TransactionType = TransactionType.INV, Number = "12345", LocalExVATAmount = 100m, OSTotal = 150m };
			t1.SetShipmentCollection(() => new List<Shipment>()
			{
				new Shipment() { VesselName = "George", GoodsDescription = "Goods", },
				new Shipment() { VesselName = "Henry", GoodsDescription = "Things", },
				new Shipment() { VesselName = "Indiana", GoodsDescription = "Stuff", },
			});

			var t2 = new TransactionInfo(DefaultDataObjectWriterStrategy.Instance) { Ledger = "AP", TransactionType = TransactionType.CRD, Number = "67890", LocalExVATAmount = -10m, OSTotal = -15m };

			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.Instance);
			transactionBatch.TransactionCollection = new List<TransactionInfo>() { t1, t2 };
			return transactionBatch;
		}
	}
}
