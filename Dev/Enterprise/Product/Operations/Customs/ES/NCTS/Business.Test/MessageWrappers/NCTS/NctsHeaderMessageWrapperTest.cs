using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NctsHeaderMessageWrapperTest : WrapperHelperTest<NctsHeaderMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("NctsHeader", () => new NctsHeaderMessageWrapper(null, Certificate));
				AssertExceptionThrown<ArgumentNullException>("Certificate", () => new NctsHeaderMessageWrapper(nctsHeader, null));
			});
		}

		public void TestLocalReferenceNumber()
		{
			nctsHeader.LocalReferenceNumber = "NCTS00000001";
			nctsHeader.BH_JobReference = "TestReference";
			AssertEquals("Expected filled LocalReferenceNumber", "NCTS00000001", wrapper.LocalReferenceNumber);
		}

		public void TestCountryOfDestination()
		{
			nctsHeader.MovementHeader.BM_RL_NKDestinationPort = HeaderDataNCTS.DestinationCountry;
			AssertEquals("Expected filled CountryOfDestination", HeaderDataNCTS.DestinationCountry, wrapper.CountryOfDestination);
		}

		public void TestNullConsignor()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Consignor.ToString());
		}

		public void TestConsignor()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				nctsHeader.Consignor.OrganisationPK = orgHeader.PK;
				wrapper = new NctsHeaderMessageWrapper(nctsHeader, Certificate);
				var consignor = wrapper.Consignor;

				AssertNotNull("Expected filled Consignor", consignor);
				AssertSame("Cached Consignor", wrapper.Consignor, consignor);
			});
		}

		public void TestNullConsignee()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Consignee.ToString());
		}

		public void TestConsignee()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				nctsHeader.Consignee.OrganisationPK = orgHeader.PK;
				wrapper = new NctsHeaderMessageWrapper(nctsHeader, Certificate);
				var consignee = wrapper.Consignee;

				AssertNotNull("Expected filled Consignee", consignee);
				AssertSame("Cached Consignee", wrapper.Consignee, consignee);
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

		public void TestCertificateID()
		{
			AssertEquals("Expected filled CertificateID", Certificate.CertificateID, wrapper.CertificateID);
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
					AssertEquals("Declaration is never test when TST and internal environment", false, wrapper.IsTest);
				}
			});
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

		public void TestDeclarantIdForUNBSegment_DeclarantIsNull()
		{
			nctsHeader.DeclarantOrgPK = ZGuid.Empty;
			AssertEquals("Expected empty Id when declarant is not declared", ZString.Empty, wrapper.DeclarantIdForUNBSegment);
		}

		public void TestDeclarantIdForUNBSegment_EmptyId()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderDataNCTS.DeclarantCode;
			orgHeader.Addresses.AddNew();
			nctsHeader.DeclarantOrgPK = orgHeader.PK;
			AssertEquals("Expected empty Id when declarant declared but has no id", ZString.Empty, wrapper.DeclarantIdForUNBSegment);
		}

		public void TestDeclarantIdForUNBSegment_NIF()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderDataNCTS.DeclarantCode;
			orgHeader.Addresses.AddNew();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			nctsHeader.DeclarantOrgPK = orgHeader.PK;
			AssertEquals("Expected NIF Id when category is NAT and NIF is declared", "NIF22222222", wrapper.DeclarantIdForUNBSegment);
		}

		public void TestDeclarantIdForUNBSegment_CountryNIF()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderDataNCTS.DeclarantCode;
			orgHeader.Addresses.AddNew();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			orgHeader.OH_Category = OrgConstants.Category.Government;
			nctsHeader.DeclarantOrgPK = orgHeader.PK;
			AssertEquals("Expected Country+NIF Id when category is not NAT and there is no EORI declared", "ESNIF22222222", wrapper.DeclarantIdForUNBSegment);
		}

		public void TestDeclarantIdForUNBSegment_EORI()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderDataNCTS.DeclarantCode;
			orgHeader.Addresses.AddNew();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
			nctsHeader.DeclarantOrgPK = orgHeader.PK;
			AssertEquals("Expected EORI Id with country code when category is not NAT and EORI is declared", "FR22222222", wrapper.DeclarantIdForUNBSegment);
		}

		public void TestDeclarantIdForUNBSegment_EORINoDuplicatedCountryCode()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderDataNCTS.DeclarantCode;
			orgHeader.Addresses.AddNew();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "ES22222222", "ES");
			nctsHeader.DeclarantOrgPK = orgHeader.PK;
			AssertEquals("Expected EORI Id with country code not repeated", "ES22222222", wrapper.DeclarantIdForUNBSegment);
		}

		public void TestDeclarantIdForUNBSegment_PAS()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderDataNCTS.DeclarantCode;
			orgHeader.Addresses.AddNew();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333");
			nctsHeader.DeclarantOrgPK = orgHeader.PK;
			AssertEquals("Expected PAS when there is no NIF or EORI declared", "GB333333333", wrapper.DeclarantIdForUNBSegment);
		}

		public void TestBusinessObjectReference()
		{
			nctsHeader.BH_JobReference = "Reference";
			AssertEquals("Expected filled BusinessObjectReference", "Reference", wrapper.BusinessObjectReference);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.DepartureAndArrival);
			wrapper = new NctsHeaderMessageWrapper(nctsHeader, Certificate);
		}

		NctsHeader nctsHeader;
		NctsHeaderMessageWrapper wrapper;

		protected override NctsHeaderMessageWrapper GetProvider() => wrapper;
	}
}
