using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestsSubclassesOf(typeof(G3CommonSendMessageWrapper))]
	public abstract class G3CommonSendMessageWrapperTest<TWrapper> : DataProviderTestCase<TWrapper> where TWrapper : G3CommonSendMessageWrapper
	{
		public void TestIsTest()
		{
			var registrationMock = new Mock<IProductRegistration>();
			using (ObjectFactory.Substitute(registrationMock.Object))
			{
				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Production);
				Assert("Wrapper IsTest should be false when sending from a production client", !Provider.IsTest);

				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Test);
				Assert("Wrapper IsTest should be false when sending from a testing client", Provider.IsTest);

				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Training);
				Assert("Wrapper IsTest should be false when sending from a training client", Provider.IsTest);

				registrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(true);
				header.TrainingEntry = false;
				var wrapperWithoutTrainingEntryChecked = GetProviderCore();
				Assert("Wrapper IsTest should be false when sending from an internal system without testing entry checked", !wrapperWithoutTrainingEntryChecked.IsTest);

				header.TrainingEntry = true;
				var wrapperWithTrainingEntryChecked = GetProviderCore();
				Assert("Wrapper IsTest should be true when sending from an internal system with testing entry checked", wrapperWithTrainingEntryChecked.IsTest);
			}
		}

		public void TestBusinessObjectReference()
		{
			header.AMA_JobReference = "AMA0001234";
			AssertEquals("Expected filled BusinessObjectReference", "AMA0001234", Provider.BusinessObjectReference);
		}

		public void TestMessages()
		{
			CombineAssertions(() =>
			{
				var messages = Provider.Messages;
				AssertNotNull("Expected filled Messages", messages);
				AssertSame("Cached Messages", messages, Provider.Messages);
			});
		}

		public void TestFactory()
		{
			CombineAssertions(() =>
			{
				AssertNotNull("Expected not null Factory", Provider.Factory);
				AssertSame("Expected same references", header.Factory, Provider.Factory);
			});
		}

		public void TestBrokerCode()
		{
			AssertEquals("Expected filled BrokerCode", certificate.BrokerCode, Provider.BrokerCode);
		}

		public void TestCertificateName()
		{
			AssertEquals("Expected filled CertificateName", certificate.CertificateName, Provider.CertificateName);
		}

		public void TestCertificateThumbPrint()
		{
			AssertEquals("Expected filled CertificateThumbPrint", certificate.CertificateThumbPrint, Provider.CertificateThumbPrint);
		}

		public void TestCertificateBytes()
		{
			AssertEquals("Expected filled CertificateBytes", certificate.CertificateBytes, Provider.CertificateBytes);
		}

		public void TestDecryptedCertificatePassphrase()
		{
			AssertEquals("Expected filled DecryptedCertificatePassphrase", certificate.DecryptedCertificatePassphrase, Provider.DecryptedCertificatePassphrase);
		}

		public void TestCertificatePK()
		{
			AssertEquals("Expected filled CertificatePK", certificate.CertificatePK, Provider.CertificatePK);
		}

		public void TestCertificateID()
		{
			AssertEquals("Expected filled CertificateID", certificate.CertificateID, Provider.CertificateID);
		}

		public void TestMessage()
		{
			CombineAssertions(() =>
			{
				var message = Provider.Message;
				AssertNotNull("Expected filled Message", message);
				AssertSame("Cached Message", message, Provider.Message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var broker = Factory.NewWithValidTestData<GlbStaff>();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;

			header.Bills.AddNew();
			certificate = new CertificateObject(header.CustomsAgent, header.AMA_CustomsProfile, ZString.Empty);
		}

		protected abstract TWrapper GetProviderCore();

		protected sealed override TWrapper GetProvider()
		{
			return GetProviderCore();
		}

		protected ICertificateProvider certificate;
		protected AsycudaManifestHeader header;
	}
}
