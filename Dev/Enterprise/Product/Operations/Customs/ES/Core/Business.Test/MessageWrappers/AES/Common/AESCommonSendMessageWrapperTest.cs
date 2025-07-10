using CargoWise.Application;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AESCommonSendMessageWrapperTest : WrapperHelperTest<AESCommonSendMessageWrapper>
	{
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
						declaration.ZG_IsTrainingDeclaration = true;
						AssertEquals("IsFinalPeriod is false when TST and internal environment and flag is checked (IsTest=true) but registry is AES", false, wrapper.IsFinalPeriod);
					}

					using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
					{
						AssertEquals("IsFinalPeriod is true when TST and internal environment and flag is checked (IsTest=true) and registry is AES1.1", true, wrapper.IsFinalPeriod);

						declaration.ZG_IsTrainingDeclaration = false;
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
						declaration.ZG_IsTrainingDeclaration = true;
						AssertEquals("PhaseIDSpecified is false when TST and internal environment and flag is checked (IsTest=true) but registry is AES", false, wrapper.PhaseIDSpecified);
					}

					using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
					{
						AssertEquals("PhaseIDSpecified is true when TST and internal environment and flag is checked (IsTest=true) and registry is AES1.1", true, wrapper.PhaseIDSpecified);

						declaration.ZG_IsTrainingDeclaration = false;
						AssertEquals("PhaseIDSpecified is false when TST and internal environment and flag is not checked (IsTest=false) and registry is AES1.1", false, wrapper.PhaseIDSpecified);
					}
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = new AESCommonSendMessageWrapper(entryHeader, Certificate);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		CusEntryHeader entryHeader;
		AESCommonSendMessageWrapper wrapper;

		protected override AESCommonSendMessageWrapper GetProvider() => wrapper;
	}
}
