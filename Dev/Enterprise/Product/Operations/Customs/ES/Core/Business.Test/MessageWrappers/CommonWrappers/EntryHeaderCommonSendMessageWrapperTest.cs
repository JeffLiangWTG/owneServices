using System;
using CargoWise.Application;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Customs.ES.Business.Testing
{
	class EntryHeaderCommonSendMessageWrapperTest : WrapperHelperTest<EntryHeaderCommonSendMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if entryHeader is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryHeader"), () => GetWrapper(null, Certificate));

				var emptyEntryHeader = Factory.New<CusEntryHeader>();
				AssertExceptionThrown("Constructor Throws Exception if certificate is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","certificate"), () => GetWrapper(emptyEntryHeader, null));

				AssertExceptionThrown("Constructor Throws Exception if Declaration is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","Declaration"), () => GetWrapper(emptyEntryHeader, Certificate));
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
					declaration.ZG_IsTrainingDeclaration = true;
					AssertEquals("Declaration is test when TST and internal environment when flag is checked", true, wrapper.IsTest);

					declaration.ZG_IsTrainingDeclaration = false;
					AssertEquals("Declaration is not test when TST and internal environment when flag is not checked", false, wrapper.IsTest);
				}
			});
		}

		public void TestBusinessObjectReference()
		{
			entryHeader.CH_BGMReference = "Reference";
			AssertEquals("Expected filled BusinessObjectReference", "Reference", wrapper.BusinessObjectReference);
		}

		public void TestMessages()
		{
			CombineAssertions(() =>
			{
				entryHeader.Messages.AddNew();
				AssertEquals("Expected one message", 1, wrapper.Messages.Count);
				AssertSame("Expected same references", entryHeader.Messages, wrapper.Messages);
			});
		}

		public void TestFactory()
		{
			CombineAssertions(() =>
			{
				AssertNotNull("Expected not null Factory", wrapper.Factory);
				AssertSame("Expected same references", entryHeader.Factory, wrapper.Factory);
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

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = GetWrapper(entryHeader, Certificate);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		EntryHeaderCommonSendMessageWrapper wrapper;

		EntryHeaderCommonSendMessageWrapper GetWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) => new EntryHeaderCommonSendMessageWrapper(cusEntryHeader, certificateData);

		protected override EntryHeaderCommonSendMessageWrapper GetProvider() => wrapper;
	}
}
