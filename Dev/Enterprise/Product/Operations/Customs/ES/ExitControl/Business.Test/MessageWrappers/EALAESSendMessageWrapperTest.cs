using System;
using CargoWise.Application;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Registry;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	class EALAESSendMessageWrapperTest : WrapperHelperTest<EALAESSendMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if Exit Report is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "exitReport"), () => GetWrapper(null, Certificate));

				var exitReportTest = Factory.New<CusExitReport>();
				AssertExceptionThrown("Constructor Throws Exception if Consignment is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "Consignment"), () => GetWrapper(exitReportTest, Certificate));

				AssertExceptionThrown("Constructor Throws Exception if certificate is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "certificateData"), () => GetWrapper(exitReport, null));
			});
		}

		public void TestMessage()
		{
			var message = wrapper.Message;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Message", message);
				AssertSame("Cached Message", wrapper.Message, message);
			});
		}

		public void TestIsFinalPeriod()
		{
			CombineAssertions(() =>
			{
				var registrationMock = new Mock<IProductRegistration>();
				registrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Production);
				using (ObjectFactory.Substitute(registrationMock.Object))
				{
					AssertEquals("IsFinalPeriod is false when PRD and external environment (IsTest=false)", false, wrapper.IsFinalPeriod);
				}

				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Test);
				using (ObjectFactory.Substitute(registrationMock.Object))
				{
					using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
					{
						AssertEquals("IsFinalPeriod is false when TST and external environment (IsTest=true) but registry is AES", false, wrapper.IsFinalPeriod);
					}

					using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
					{
						AssertEquals("IsFinalPeriod is true when TST and external environment (IsTest=true) and registry is AES1.1", true, wrapper.IsFinalPeriod);
					}
				}

				registrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(true);
				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Production);
				using (ObjectFactory.Substitute(registrationMock.Object))
				{
					AssertEquals("IsFinalPeriod is false when PRD and internal environment (IsTest=false)", false, wrapper.IsFinalPeriod);
				}

				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Test);
				using (ObjectFactory.Substitute(registrationMock.Object))
				{
					using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
					{
						exitHeader.TrainingEntry = true;
						AssertEquals("IsFinalPeriod is false when TST and internal environment and flag is checked (IsTest=true) but registry is AES", false, wrapper.IsFinalPeriod);
					}

					using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
					{
						AssertEquals("IsFinalPeriod is true when TST and internal environment and flag is checked (IsTest=true) and registry is AES1.1", true, wrapper.IsFinalPeriod);

						exitHeader.TrainingEntry = false;
						AssertEquals("IsFinalPeriod is false when TST and internal environment and flag is not checked (IsTest=false) and registry is AES1.1", false, wrapper.IsFinalPeriod);
					}
				}
			});
		}

		public void TestPhaseIDSpecified()
		{
			CombineAssertions(() =>
			{
				var registrationMock = new Mock<IProductRegistration>();
				registrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Production);
				using (ObjectFactory.Substitute(registrationMock.Object))
				{
					AssertEquals("PhaseIDSpecified is false when PRD and external environment (IsTest=false)", false, wrapper.PhaseIDSpecified);
				}

				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Test);
				using (ObjectFactory.Substitute(registrationMock.Object))
				{
					using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
					{
						AssertEquals("PhaseIDSpecified is false when TST and external environment (IsTest=true) but registry is AES", false, wrapper.PhaseIDSpecified);
					}

					using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
					{
						AssertEquals("PhaseIDSpecified is true when TST and external environment (IsTest=true) and registry is AES1.1", true, wrapper.PhaseIDSpecified);
					}
				}

				registrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(true);
				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Production);
				using (ObjectFactory.Substitute(registrationMock.Object))
				{
					AssertEquals("PhaseIDSpecified is false when PRD and internal environment (IsTest=false)", false, wrapper.PhaseIDSpecified);
				}

				registrationMock.Setup(r => r.Key.DatabaseType).Returns(DatabaseTypes.Codes.Test);
				using (ObjectFactory.Substitute(registrationMock.Object))
				{
					using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
					{
						exitHeader.TrainingEntry = true;
						AssertEquals("PhaseIDSpecified is false when TST and internal environment and flag is checked (IsTest=true) but registry is AES", false, wrapper.PhaseIDSpecified);
					}

					using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
					{
						AssertEquals("PhaseIDSpecified is true when TST and internal environment and flag is checked (IsTest=true) and registry is AES1.1", true, wrapper.PhaseIDSpecified);

						exitHeader.TrainingEntry = false;
						AssertEquals("PhaseIDSpecified is false when TST and internal environment and flag is not checked (IsTest=false) and registry is AES1.1", false, wrapper.PhaseIDSpecified);
					}
				}
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
					exitHeader.TrainingEntry = true;
					AssertEquals("Declaration is test when TST and internal environment when flag is checked", true, wrapper.IsTest);

					exitHeader.TrainingEntry = false;
					AssertEquals("Declaration is not test when TST and internal environment when flag is not checked", false, wrapper.IsTest);
				}
			});
		}

		public void TestBusinessObjectReference()
		{
			exitConsignment.CXC_MovementReference = "Reference";
			AssertEquals("Expected filled BusinessObjectReference", "Reference", wrapper.BusinessObjectReference);
		}

		public void TestMessages()
		{
			CombineAssertions(() =>
			{
				exitReport.Messages.AddNew();
				AssertEquals("Expected one message", 1, wrapper.Messages.Count);
				AssertSame("Expected same references", exitReport.Messages, wrapper.Messages);
			});
		}

		public void TestFactory()
		{
			CombineAssertions(() =>
			{
				AssertNotNull("Expected not null Factory", wrapper.Factory);
				AssertSame("Expected same references", exitReport.Factory, wrapper.Factory);
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

		public void TestExportOperation()
		{
			var exportOperation = wrapper.ExportOperation;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled ExportOperation", exportOperation);
				AssertSame("Cached ExportOperation", wrapper.ExportOperation, exportOperation);
			});
		}

		public void TestCustomsOfficeOfExitActual()
		{
			CombineAssertions(() =>
			{
				exitReport.CER_OfficeOfExit = "ES009999";
				wrapper = GetWrapper(exitReport, Certificate);
				AssertEquals("Expected filled CustomOfficeOfExitActual with OfficeOfExit", "ES009999", wrapper.CustomsOfficeOfExitActual);
			});
		}

		public void TestGoodsShipment()
		{
			var goodsShipment = wrapper.GoodsShipment;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled GoodsShipment", goodsShipment);
				AssertSame("Cached GoodsShipment", wrapper.GoodsShipment, goodsShipment);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitConsignment = exitHeader.CusExitConsignments.AddNew();
			exitReport = exitHeader.CusExitReports.AddNew();
			exitReport.CER_CXC_Consignment = exitConsignment.PK;

			wrapper = GetWrapper(exitReport, Certificate);
		}
		CusExitHeader exitHeader;
		CusExitConsignment exitConsignment;
		CusExitReport exitReport;
		EALAESSendMessageWrapper wrapper;

		EALAESSendMessageWrapper GetWrapper(CusExitReport exitReport, ICertificateProvider certificateData) => new EALAESSendMessageWrapper(exitReport, certificateData);

		protected override EALAESSendMessageWrapper GetProvider() => wrapper;
	}
}
