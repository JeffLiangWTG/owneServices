using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.eNett;
using Enterprise.Accounting.DataTransfer.com.enett991;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration.Testing
{
	public class eNettWebServiceWrapperTest : TestCaseWithFactory
	{
		public void TestConstructor_Error()
		{
			var constructorError = AssertExceptionThrown<ArgumentException>(() => new eNettWebServiceWrapper(eNettWebServiceWrapper.CreateNewWebService(), false));
			AssertContains("It is invalid to set 'isAutoFactorySave' false when no external Factory has been passed to 'factory' as it will lead to losing unsaved data", constructorError.Message);

			constructorError = AssertExceptionThrown<ArgumentException>(() => new eNettWebServiceWrapper(eNettWebServiceWrapper.CreateNewWebService(), true, Factory));
			AssertContains("'factory' must be null when 'isAutoFactorySave' is true as it will be ignored.", constructorError.Message);
		}

		public void TestConstructor_Ok()
		{
			AssertNoExceptionThrown(() =>
			{
				_ = new eNettWebServiceWrapper();
				_ = new eNettWebServiceWrapper(eNettWebServiceWrapper.CreateNewWebService(), true);
				_ = new eNettWebServiceWrapper(eNettWebServiceWrapper.CreateNewWebService(), false, Factory);
			});
		}

		public void TestProcessCreditCard_AutoSave()
			=> TestProcessCreditCard(true);

		public void TestProcessCreditCard_NonAutoSave()
			=> TestProcessCreditCard(false);

		void TestProcessCreditCard(bool isAutoSaved)
		{
			FactorySaveAlerterTestFailureAlternatives(
				(wrapper) => wrapper.ProcessCreditCard(Factory.NewWithValidTestData<APPayment>(), "mock_cardSecurityCode")
				, isAutoSaved);
		}

		public void TestProcessDirectDebit_AutoSave()
			=> TestProcessDirectDebit(true);

		public void TestProcessDirectDebit_NonAutoSave()
			=> TestProcessDirectDebit(false);

		void TestProcessDirectDebit(bool isAutoSaved)
		{
			FactorySaveAlerterTestFailureAlternatives(
				(wrapper) =>
				{
					APPayment transaction = Factory.NewWithValidTestData<APPayment>();
					transaction.AH_OH = TestObjectCreator.AALSHI.PK;
					transaction.AH_AB = TestObjectCreator.AUDBankAccount.PK;
					wrapper.ProcessDirectDebit(transaction);
				}
				, isAutoSaved);
		}

		void FactorySaveAlerterTestFailureAlternatives(Action<eNettWebServiceWrapper> tester, bool isAutoSaved)
		{
			var wrapper = isAutoSaved
				? new eNettWebServiceWrapper(eNettWebServiceWrapper.CreateNewWebService(), true)
				: new eNettWebServiceWrapper(eNettWebServiceWrapper.CreateNewWebService(), false, Factory);

			var factoryInWrapper = wrapper.Factory_ForTest;
			var factoryLastSaveCount = factoryInWrapper.SaveCount;

			AssertNoExceptionThrown(() => tester(wrapper));

			if (isAutoSaved)
			{
				AssertNotEquals(wrapper.Factory_ForTest, Factory);
			}
			else
			{
				AssertEquals(wrapper.Factory_ForTest, Factory);
				AssertEquals(0, factoryInWrapper.SaveCount - factoryLastSaveCount);
				factoryInWrapper.Save();
			}
			Assert((factoryInWrapper.SaveCount - factoryLastSaveCount) > 0);
		}

		[ExpectNoExceptions]
		public void TestAPAccountDetailsFallback()
		{
			var mockService = new Mock<IeNettWebServiceClient>();
			eNettWebServiceWrapper wrapper = new eNettWebServiceWrapper(mockService.Object, false, Factory);
			APPayment transaction = Factory.NewWithValidTestData<APPayment>();
			transaction.AH_OH = TestObjectCreator.AALSHI.PK;
			transaction.AH_AB = TestObjectCreator.AUDBankAccount.PK;

			wrapper.ProcessDirectDebitFx(transaction, 1);
			mockService.Verify(x => x.ProcessDirectDebitFx(
				It.Is<ProcessDirectDebitFxDTO>(v =>
					v.integrator == "CARGOWISE" &&
					v.integratorKey == "key" &&
					v.version == "1" &&
					v.sourceDescription == transaction.AH_Desc &&
					v.integratorRef == transaction.AH_TransactionNum &&
					v.fromClient == string.Empty &&
					v.fromAccountBSB == TestObjectCreator.AUDBankAccount.AB_BSB &&
					v.fromAccountNo == TestObjectCreator.AUDBankAccount.AB_AccountNum &&
					v.toClient == string.Empty &&
					v.toAccountName == string.Empty &&
					v.toAccountNo == string.Empty &&
					v.swiftCode == string.Empty &&
					v.invoiceNo == string.Empty &&
					v.invoiceID == string.Empty &&
					v.amount == 0m &&
					v.currency == Constants.CurrencyCodes.Australia &&
					!v.sendRemittance &&
					!string.IsNullOrEmpty(v.integratorXML) &&       // Serialised XML string; assert it has non-empty content.
					v.lineItems == null &&
					v.authorised &&
					v.adviceEmail == string.Empty &&
					v.createdBy == "CargoWise Support" &&
					v.notes == string.Empty &&
					v.paymentDate == transaction.AH_PostDate.ToDateTime() &&
					v.integrationAuthCode == string.Empty &&
					v.fxQuoteID == 1 &&
					v.localAmount == 0m &&
					v.localCurrency == Constants.CurrencyCodes.Australia &&
					v.fxRate == 1m &&
					v.fxUsername == string.Empty &&
					v.fxPassword == string.Empty
					)
				), Times.Once
			);
		}

		public void TestRelatedOrganisationsDisplayClientNotRepeated()
		{
			var cusCode = TestObjectCreator.ABIGAS.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.eNettRegistrationNumber;
			cusCode.OK_CustomsRegNo = "1";
			cusCode.OK_RN_NKCodeCountry = "AU";

			var regCode = new EnettRegistrationCode { RegistrationCode = "1", OrganisationPK = TestObjectCreator.AALSHI.PK };
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regCode);
			Factory.Save();

			var eNettWebServiceMock = new Mock<IeNettWebServiceClient>();
			eNettWebServiceMock
				.Setup(m => m.DisplayClientList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
				.Returns(new Response_GetClientList[] { new Response_GetClientList { ECN = 1, TerminalCode = "X" } });

			var wrapper = new eNettWebServiceWrapper();
			var registeredOrganisations = new ComPayRegisteredOrganisationCollection(Factory);
			wrapper.DisplayClientList(eNettWebServiceMock.Object, registeredOrganisations);

			AssertEquals("Only one registered organisation", 1, registeredOrganisations.Count);
			AssertEquals("The OrgHeader code is not repeated", "ABIGAS", registeredOrganisations[0].RelatedOrganisations);
			eNettWebServiceMock.VerifyAll();
		}

		public void TestRelatedOrganisationsDisplayClientFilter()
		{
			var cusCode = TestObjectCreator.ABIGAS.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.eNettRegistrationNumber;
			cusCode.OK_CustomsRegNo = "1";
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode = TestObjectCreator.AALSHI.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.eNettRegistrationNumber;
			cusCode.OK_CustomsRegNo = "1";
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode = TestObjectCreator.ZECTRA.CustomsCodes.AddNew(); // Different RegNo
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.eNettRegistrationNumber;
			cusCode.OK_CustomsRegNo = "2";
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode = TestObjectCreator.XLINDU.CustomsCodes.AddNew(); // Different Country
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.eNettRegistrationNumber;
			cusCode.OK_CustomsRegNo = "1";
			cusCode.OK_RN_NKCodeCountry = "US";
			cusCode = TestObjectCreator.LocalClient.CustomsCodes.AddNew(); // Different Code Type
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.BondHolderCode;
			cusCode.OK_CustomsRegNo = "1";
			cusCode.OK_RN_NKCodeCountry = "AU";

			var regCode = new EnettRegistrationCode { RegistrationCode = "1", OrganisationPK = TestObjectCreator.AALSHI.PK };
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regCode);
			Factory.Save();

			var eNettWebServiceMock = new Mock<IeNettWebServiceClient>();
			eNettWebServiceMock
				.Setup(m => m.DisplayClientList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
				.Returns(new Response_GetClientList[] { new Response_GetClientList { ECN = 1, TerminalCode = "X" } });

			var wrapper = new eNettWebServiceWrapper();
			var registeredOrganisations = new ComPayRegisteredOrganisationCollection(Factory);
			wrapper.DisplayClientList(eNettWebServiceMock.Object, registeredOrganisations);

			AssertEquals("Only one registered organisation", 1, registeredOrganisations.Count);
			AssertEquals("RelatedOrganisations has filtered our irrelevant OrgHeaders", "AALSHI, ABIGAS", registeredOrganisations[0].RelatedOrganisations);
			eNettWebServiceMock.VerifyAll();
		}

		public void TestRelatedOrganisationsTruncation()
		{
			for (int i = 0; i < 126; i++)
			{
				var orgHeaderNew = Factory.New<OrgHeader>();
				orgHeaderNew.OH_Code = String.Format("ORG{0:000}", i);
				var cusCode = orgHeaderNew.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.CodeTypes.eNettRegistrationNumber;
				cusCode.OK_CustomsRegNo = "1";
				cusCode.OK_RN_NKCodeCountry = "AU";
			}

			var regCode = new EnettRegistrationCode { RegistrationCode = "1", OrganisationPK = TestObjectCreator.AALSHI.PK };
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regCode);
			Factory.Save();

			var eNettWebServiceMock = new Mock<IeNettWebServiceClient>();
			eNettWebServiceMock
				.Setup(m => m.DisplayClientList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
				.Returns(new Response_GetClientList[] { new Response_GetClientList { ECN = 1, TerminalCode = "X" } });

			var wrapper = new eNettWebServiceWrapper();
			var registeredOrganisations = new ComPayRegisteredOrganisationCollection(Factory);
			wrapper.DisplayClientList(eNettWebServiceMock.Object, registeredOrganisations);

			AssertEquals("Only one registered organisation", 1, registeredOrganisations.Count);
			AssertEquals("RelatedOrganisations truncated to 1000 characters", 1000, registeredOrganisations[0].RelatedOrganisations.Length);
			AssertEquals("RelatedOrganisations showing correct organisations", "ORG000, ORG001, ORG002, ORG003, ORG004, ORG005, ORG006, ORG007, ORG008, ORG009, ORG010, ORG011, ORG012, ORG013, ORG014, ORG015, ORG016, ORG017, ORG018, ORG019, ORG020, ORG021, ORG022, ORG023, ORG024, ORG025, ORG026, ORG027, ORG028, ORG029, ORG030, ORG031, ORG032, ORG033, ORG034, ORG035, ORG036, ORG037, ORG038, ORG039, ORG040, ORG041, ORG042, ORG043, ORG044, ORG045, ORG046, ORG047, ORG048, ORG049, ORG050, ORG051, ORG052, ORG053, ORG054, ORG055, ORG056, ORG057, ORG058, ORG059, ORG060, ORG061, ORG062, ORG063, ORG064, ORG065, ORG066, ORG067, ORG068, ORG069, ORG070, ORG071, ORG072, ORG073, ORG074, ORG075, ORG076, ORG077, ORG078, ORG079, ORG080, ORG081, ORG082, ORG083, ORG084, ORG085, ORG086, ORG087, ORG088, ORG089, ORG090, ORG091, ORG092, ORG093, ORG094, ORG095, ORG096, ORG097, ORG098, ORG099, ORG100, ORG101, ORG102, ORG103, ORG104, ORG105, ORG106, ORG107, ORG108, ORG109, ORG110, ORG111, ORG112, ORG113, ORG114, ORG115, ORG116, ORG117, ORG118, ORG119, ORG120, ORG121, ORG122, ORG123, ORG12...", registeredOrganisations[0].RelatedOrganisations);
			eNettWebServiceMock.VerifyAll();
		}

		public void TesteNettWebServiceTimeoutValue()
		{
			eNettWebServiceWrapper.UseRealWebService_ForTesting = true;
			var service = (IntegrationService)eNettWebServiceWrapper.CreateNewWebService();
			AssertEquals(300000, service.Timeout);
		}

		#region Implementation

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion
	}
}
