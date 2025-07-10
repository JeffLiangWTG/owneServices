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
	[TestsSubclassesOf(typeof(H7CommonSendMessageWrapper))]
	public abstract class H7CommonSendMessageWrapperBaseTest<TWrapper> : DataProviderTestCase<TWrapper> where TWrapper : H7CommonSendMessageWrapper
	{
		public void TestBusinessObjectReference()
		{
			AssertEquals("Expected filled BusinessObjectReference", bill.ABL_BillNumber, Provider.BusinessObjectReference);
		}

		public void TestIsTestTrue()
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
				var wrapperWithoutTraningEntryChecked = new H7CommonSendMessageWrapperForTest(bill, certificate);
				Assert("Wrapper IsTest should be false when sending from an internal system without testing entry checked", !wrapperWithoutTraningEntryChecked.IsTest);

				header.TrainingEntry = true;
				var wrapperWithTraningEntryChecked = new H7CommonSendMessageWrapperForTest(bill, certificate);
				Assert("Wrapper IsTest should be true when sending from an internal system with testing entry checked", wrapperWithTraningEntryChecked.IsTest);
			}
		}

		public void TestMessages()
		{
			AssertSame("Expected same references", bill.Messages, Provider.Messages);
		}

		public void TestFactory()
		{
			AssertSame("Expected same references", bill.Factory, Provider.Factory);
		}

		public void TestBrokerCode()
		{
			AssertEquals("Expected filled certificate BrokerCode", certificate.BrokerCode, Provider.BrokerCode);
		}

		public void TestCertificateName()
		{
			AssertEquals("Expected filled certificate CertificateName", certificate.CertificateName, Provider.CertificateName);
		}

		public void TestCertificateThumbprint()
		{
			AssertEquals("Expected filled certificate CertificateThumbPrint", certificate.CertificateThumbPrint, Provider.CertificateThumbPrint);
		}

		public void TestCertificateBytes()
		{
			AssertEquals("Expected filled certificate CertificateBytes", certificate.CertificateBytes, Provider.CertificateBytes);
		}

		public void TestDecryptedCertificatePassphrase()
		{
			AssertEquals("Expected filled certificate DecryptedCertificatePassphrase", certificate.DecryptedCertificatePassphrase, Provider.DecryptedCertificatePassphrase);
		}

		public void TestCertificatePK()
		{
			AssertEquals("Expected filled certificate CertificatePK", certificate.CertificatePK, Provider.CertificatePK);
		}

		public void TestCertificateID()
		{
			AssertEquals("Expected filled certificate CertificateID", certificate.CertificateID, Provider.CertificateID);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var broker = Factory.NewWithValidTestData<GlbStaff>();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;
			bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123";
			certificate = new CertificateObject(header.CustomsAgent, header.AMA_CustomsProfile, ZString.Empty);
		}

		protected sealed override TWrapper GetProvider()
		{
			return GetWrapperCore(bill, certificate);
		}

		protected abstract TWrapper GetWrapperCore(AsycudaBill bill, ICertificateProvider certificate);

		protected AsycudaManifestHeader header;
		protected AsycudaBill bill;
		ICertificateProvider certificate;
	}

	class H7CommonSendMessageWrapperForTest : H7CommonSendMessageWrapper
	{
		public H7CommonSendMessageWrapperForTest(AsycudaBill bill, ICertificateProvider certificate)
			: base(bill, certificate)
		{
		}
	}
}
