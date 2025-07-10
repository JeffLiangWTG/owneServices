using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class G5GenericSendMessageWrapperTest : WrapperHelperTest<G5GenericSendMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if Header is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "tempHeader"), () => new G5GenericSendMessageWrapper(null, Certificate));

				AssertExceptionThrown("Constructor Throws Exception if Certificate is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "certificateData"), () => new G5GenericSendMessageWrapper(header, null));
			});
		}

		public void TestSenderId()
		{
			var orgHeaderDeclarant = Factory.New<OrgHeader>();
			orgHeaderDeclarant.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty SenderId when no declarant is declared", ZString.Empty, wrapper.SenderId);

				header.Declarant.OA_OH = orgHeaderDeclarant.PK;
				AssertEquals("Expected PAS SenderId with country code when no NIF or EORI declared", "GB333333333", wrapper.SenderId);

				orgHeaderDeclarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeaderDeclarant.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected NIF SenderId", "NIF22222222", wrapper.SenderId);

				orgHeaderDeclarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeaderDeclarant.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected Country+NIF SenderId for non NAT Organizations", "ESNIF22222222", wrapper.SenderId);

				var eoriCusCode = orgHeaderDeclarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI SenderId with country code", "FR22222222", wrapper.SenderId);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI SenderId with country code not repeated", "ES22222222", wrapper.SenderId);
			});
		}

		public void TestIsTest()
		{
			CombineAssertions(() =>
			{
				var registrationMock = new Mock<IProductRegistration>();
				registrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Production);
				using (ObjectFactory.Substitute(registrationMock.Object))
				{
					AssertEquals("Declaration is never test when PRD and external environment", false, wrapper.IsTest);
				}

				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Test);
				using (ObjectFactory.Substitute(registrationMock.Object))
				{
					AssertEquals("Declaration is always test when TST and external environment", true, wrapper.IsTest);
				}

				registrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(true);
				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Production);
				using (ObjectFactory.Substitute(registrationMock.Object))
				{
					AssertEquals("Declaration is never test when PRD and internal environment", false, wrapper.IsTest);
				}

				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Test);
				using (ObjectFactory.Substitute(registrationMock.Object))
				{
					header.TrainingEntry = true;
					AssertEquals("Declaration is test when TST and internal environment when flag is checked", true, wrapper.IsTest);

					header.TrainingEntry = false;
					AssertEquals("Declaration is not test when TST and internal environment when flag is not checked", false, wrapper.IsTest);
				}
			});
		}

		public void TestBusinessObjectReference()
		{
			header.AMA_JobReference = "Reference";
			AssertEquals("Expected filled BusinessObjectReference", "Reference", wrapper.BusinessObjectReference);
		}

		public void TestMessages()
		{
			CombineAssertions(() =>
			{
				header.Messages.AddNew();
				AssertEquals("Expected one message", 1, wrapper.Messages.Count);
				AssertSame("Expected same references", header.Messages, wrapper.Messages);
			});
		}

		public void TestFactory()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(wrapper.Factory);
				AssertSame("Expected same references", header.Factory, wrapper.Factory);
			});
		}

		public void TestBrokerCode()
		{
			AssertEquals("Expected filled BrokerCode", Certificate.BrokerCode, wrapper.BrokerCode);
		}

		public void TestCertificateName()
		{
			AssertEquals("Expected filled CertificateName", Certificate.CertificateName, wrapper.CertificateName);
		}

		public void TestCertificateThumbPrint()
		{
			AssertEquals("Expected filled CertificateThumbPrint", Certificate.CertificateThumbPrint, wrapper.CertificateThumbPrint);
		}

		public void TestCertificateBytes()
		{
			AssertEquals("Expected filled CertificateBytes", Certificate.CertificateBytes, wrapper.CertificateBytes);
		}

		public void TestDecryptedCertificatePassphrase()
		{
			AssertEquals("Expected filled DecryptedCertificatePassphrase", Certificate.DecryptedCertificatePassphrase, wrapper.DecryptedCertificatePassphrase);
		}

		public void TestCertificatePK()
		{
			AssertEquals("Expected filled CertificatePK", Certificate.CertificatePK, wrapper.CertificatePK);
		}

		public void TestCertificateID()
		{
			AssertEquals("Expected filled CertificateID", Certificate.CertificateID, wrapper.CertificateID);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<TemporaryStorageHeader>();
			wrapper = new G5GenericSendMessageWrapper(header, Certificate);
		}

		TemporaryStorageHeader header;
		G5GenericSendMessageWrapper wrapper;

		protected override G5GenericSendMessageWrapper GetProvider() => wrapper;
	}
}
