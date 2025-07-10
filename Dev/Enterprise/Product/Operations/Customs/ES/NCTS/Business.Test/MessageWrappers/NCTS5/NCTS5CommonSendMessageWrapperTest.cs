using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonSendMessageWrapperTest : WrapperHelperTest<NCTS5CommonSendMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("NctsHeader", () => new NCTS5CommonSendMessageWrapper(null, Certificate));
				AssertExceptionThrown<ArgumentNullException>("Certificate", () => new NCTS5CommonSendMessageWrapper(nctsHeader, null));
			});
		}

		public void TestMessageSender()
		{
			var orgHeaderPrincipal = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderPrincipal.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF44444444");
			orgHeaderPrincipal.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

			var orgHeaderRepresentative = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderRepresentative.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");

			var orgHeaderRepresentativeArrival = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderRepresentativeArrival.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB444444444", "GB");

			var orgHeaderDestinationTraderArrival = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderDestinationTraderArrival.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB555555555", "GB");

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty MessageSender when no representative or principal are declared", ZString.Empty, wrapper.MessageSender);

				nctsHeader.Principal.OrganisationPK = orgHeaderPrincipal.PK;
				AssertEquals("Expected principal NIF in MessageSender when only principal declared", "NIF44444444", wrapper.MessageSender);

				nctsHeader.MovementHeader.Representative.OrganisationPK = orgHeaderRepresentative.PK;
				AssertEquals("Expected representatice PAS in MessageSender with country code when no NIF or EORI declared when representative declared in departure", "GB333333333", wrapper.MessageSender);

				var nctsHeader1 = Factory.New<NctsHeader>();
				nctsHeader1.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
				nctsHeader1.ArrivalMovementHeader.Representative.OrganisationPK = orgHeaderRepresentativeArrival.PK;
				var wrapperArrival = new NCTS5CommonSendMessageWrapper(nctsHeader1, Certificate);
				AssertEquals("Expected representatice PAS in MessageSender with country code when no NIF or EORI declared when representative declared in arrival", "GB444444444", wrapperArrival.MessageSender);

				nctsHeader1.ArrivalMovementHeader.Representative.OrganisationPK = ZGuid.Empty;
				nctsHeader1.DestinationTrader.OrganisationPK = orgHeaderDestinationTraderArrival.PK;
				wrapperArrival = new NCTS5CommonSendMessageWrapper(nctsHeader1, Certificate);
				AssertEquals("Expected representatice PAS in MessageSender when representative not declared in arrival (destination trader)", "GB555555555", wrapperArrival.MessageSender);

				orgHeaderRepresentative.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeaderRepresentative.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected representative NIF in MessageSender", "NIF22222222", wrapper.MessageSender);

				orgHeaderRepresentative.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected Country+NIF for non NAT Organizations MessageSender", "ESNIF22222222", wrapper.MessageSender);

				OrgCusCode eoriCusCode = orgHeaderRepresentative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI MessageSender with country code", "FR22222222", wrapper.MessageSender);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI MessageSender with country code not repeated", "ES22222222", wrapper.MessageSender);
			});
		}

		public void TestMessageIdentification()
		{
			AssertEquals("Expected filled MessageIdentification", "<<MSGNO PLACEHOLDER>>", wrapper.MessageIdentification);
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
					nctsHeader.TrainingEntry = true;
					AssertEquals("Declaration is test when TST and internal environment when flag is checked", true, wrapper.IsTest);

					nctsHeader.TrainingEntry = false;
					AssertEquals("Declaration is not test when TST and internal environment when flag is not checked", false, wrapper.IsTest);
				}
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
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
					{
						AssertEquals("IsFinalPeriod is false when TST and external environment (IsTest=true)", false, wrapper.IsFinalPeriod);
					}

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
					{
						AssertEquals("IsFinalPeriod is true when TST and external environment (IsTest=true)", true, wrapper.IsFinalPeriod);
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
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
					{
						nctsHeader.TrainingEntry = true;
						AssertEquals("IsFinalPeriod is false when TST and internal environment and flag is checked (IsTest=true)", false, wrapper.IsFinalPeriod);
					}

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
					{
						AssertEquals("IsFinalPeriod is true when TST and internal environment and flag is checked (IsTest=true)", true, wrapper.IsFinalPeriod);

						nctsHeader.TrainingEntry = false;
						AssertEquals("IsFinalPeriod is false when TST and internal environment and flag is not checked (IsTest=false)", false, wrapper.IsFinalPeriod);
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
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
					{
						AssertEquals("PhaseIDSpecified is false when TST and external environment (IsTest=true) but registry is AES", false, wrapper.PhaseIDSpecified);
					}

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
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
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
					{
						nctsHeader.TrainingEntry = true;
						AssertEquals("PhaseIDSpecified is false when TST and internal environment and flag is checked (IsTest=true)", false, wrapper.PhaseIDSpecified);
					}

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
					{
						AssertEquals("PhaseIDSpecified is true when TST and internal environment and flag is checked (IsTest=true)", true, wrapper.PhaseIDSpecified);

						nctsHeader.TrainingEntry = false;
						AssertEquals("PhaseIDSpecified is false when TST and internal environment and flag is not checked (IsTest=false)", false, wrapper.PhaseIDSpecified);
					}
				}
			});
		}

		public void TestBusinessObjectReference()
		{
			nctsHeader.BH_JobReference = "Reference";
			AssertEquals("Expected filled BusinessObjectReference", "Reference", wrapper.BusinessObjectReference);
		}

		public void TestMessages()
		{
			CombineAssertions(() =>
			{
				nctsHeader.Messages.AddNew();
				AssertEquals("Expected one message", 1, wrapper.Messages.Count);
				AssertSame("Expected same references", nctsHeader.Messages, wrapper.Messages);
			});
		}

		public void TestFactory()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(wrapper.Factory);
				AssertSame("Expected same references", nctsHeader.Factory, wrapper.Factory);
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

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			wrapper = new NCTS5CommonSendMessageWrapper(nctsHeader, Certificate);
		}

		NctsHeader nctsHeader;
		NCTS5CommonSendMessageWrapper wrapper;

		protected override NCTS5CommonSendMessageWrapper GetProvider() => wrapper;
	}
}
