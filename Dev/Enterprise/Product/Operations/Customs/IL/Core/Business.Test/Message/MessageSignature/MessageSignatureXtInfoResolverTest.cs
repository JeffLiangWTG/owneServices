using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(MessageSignatureXtInfoResolver))]
	sealed class MessageSignatureXtInfoResolverTest : TestCaseWithFactory
	{
		public void TestGetMessageAttributes_ShouldReturnNoneSignature_WhenNoneSignatureType()
		{
			using (ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var ediMessage = CreateEDIMessage(MessageSignatureType.NoneSignatureType);
				ediMessage.EM_MessageSubType = ILEDIMessageSubTypeList.Codes.CurrencyRateRequest;
				var resolver = new MessageSignatureXtInfoResolver(ediMessage);

				var (messageError, externalPassword, _) = resolver.GetMessageAttributes();
				AssertNull(messageError);
				AssertNull("externalPassword should be null", externalPassword);
			}
		}

		public void TestGetMessageAttributes_ShouldReturnNoneSignatureWithMessageNote_WhenPersonalSignatureTypeAndWithoutDigitalSignatureRegistry()
		{
			using (ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var ediMessage = CreateEDIMessage(MessageSignatureType.PersonalSignature);
				var resolver = new MessageSignatureXtInfoResolver(ediMessage);

				var (messageError, externalPassword, messageNote) = resolver.GetMessageAttributes();
				AssertNull(messageError);
				AssertEquals("Digital Signature is disabled by Registry", messageNote);
				AssertNull("externalPassword should be null", externalPassword);
			}
		}

		public void TestGetMessageAttributes_ShouldReturnNoneSignatureWithMessageNote_WhenCompanySignatureTypeAndWithoutDigitalSignatureRegistry()
		{
			using (ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var ediMessage = CreateEDIMessage(MessageSignatureType.CompanySignature);
				var resolver = new MessageSignatureXtInfoResolver(ediMessage);

				var (messageError, externalPassword, messageNote) = resolver.GetMessageAttributes();
				AssertNull(messageError);
				AssertEquals("Digital Signature is disabled by Registry", messageNote);
				AssertNull("externalPassword should be null", externalPassword);
			}
		}

		public void TestGetMessageAttributes_ShouldReturnMessageError_WhenSignTypeIsPersonalStaffAndUserLacksValidCertificate()
		{
			using (ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (ILCustomsDataRegistry.Instance.PersonalDigitalSignatureFallbackConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DigitalSignatureFallbackList.Codes.StaffOnly))
			{
				var ediMessage = CreateEDIMessage(MessageSignatureType.PersonalSignature);
				ediMessage.EM_SystemCreateUser = "US1";
				var resolver = new MessageSignatureXtInfoResolver(ediMessage);
				var (messageError, externalPassword, _) = resolver.GetMessageAttributes();

				AssertNull("externalPassword should be null", externalPassword);
				AssertEquals("No Valid digital sign certificate found for signing this message – please review your staff or company configuration", messageError);
			}
		}

		public void TestGetMessageAttributes_ShouldReturnStaffSignature_WhenSignTypeIsPersonalStaffAndUserHasValidCertificate()
		{
			using (ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (ILCustomsDataRegistry.Instance.PersonalDigitalSignatureFallbackConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DigitalSignatureFallbackList.Codes.StaffOnly))
			{
				var ediMessage = CreateEDIMessage(MessageSignatureType.PersonalSignature);
				ediMessage.EM_SystemCreateUser = "US2";
				var resolver = new MessageSignatureXtInfoResolver(ediMessage);
				var (messageError, externalPassword, _) = resolver.GetMessageAttributes();

				AssertNull(messageError);
				AssertNotNull("externalPassword should not be null", externalPassword);
				AssertEquals("COM", externalPassword.GP_CertificateAuthority);
				AssertEquals("usr.Name", externalPassword.GP_UserID);
			}
		}

		public void TestGetMessageAttributes_ShouldReturnStaffSignature_WhenSignTypeIsPersonalStaffDirectReportAndUserHasValidCertificate()
		{
			using (ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (ILCustomsDataRegistry.Instance.PersonalDigitalSignatureFallbackConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DigitalSignatureFallbackList.Codes.StaffDirectReport))
			{
				var ediMessage = CreateEDIMessage(MessageSignatureType.PersonalSignature);
				ediMessage.EM_SystemCreateUser = "US2";
				var resolver = new MessageSignatureXtInfoResolver(ediMessage);
				var (messageError, externalPassword, _) = resolver.GetMessageAttributes();

				AssertNull(messageError);
				AssertNotNull("externalPassword should not be null", externalPassword);
				AssertEquals("COM", externalPassword.GP_CertificateAuthority);
				AssertEquals("usr.Name", externalPassword.GP_UserID);
			}
		}

		public void TestGetMessageAttributes_ShouldReturnManagerSignature_WhenSignTypeIsPersonalStaffDirectReportAndUserManagerHasValidCertificate()
		{
			using (ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (ILCustomsDataRegistry.Instance.PersonalDigitalSignatureFallbackConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DigitalSignatureFallbackList.Codes.StaffDirectReport))
			{
				var ediMessage = CreateEDIMessage(MessageSignatureType.PersonalSignature);
				ediMessage.EM_SystemCreateUser = "US3";
				var resolver = new MessageSignatureXtInfoResolver(ediMessage);
				var (messageError, externalPassword, _) = resolver.GetMessageAttributes();

				AssertNull(messageError);
				AssertNotNull("externalPassword should not be null", externalPassword);
				AssertEquals("PER", externalPassword.GP_CertificateAuthority);
				AssertEquals("mngr.Name", externalPassword.GP_UserID);
			}
		}

		public void TestGetMessageAttributes_ShouldReturnUserSignature_WhenMessageSignTypeIsCompanyAndUserHasValidCertificate()
		{
			using (ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var ediMessage = CreateEDIMessage(MessageSignatureType.CompanySignature);
				ediMessage.EM_SystemCreateUser = "US2";
				var resolver = new MessageSignatureXtInfoResolver(ediMessage);
				var (messageError, externalPassword, _) = resolver.GetMessageAttributes();

				AssertNull(messageError);
				AssertNotNull("externalPassword should not be null", externalPassword);
				AssertEquals("COM", externalPassword.GP_CertificateAuthority);
				AssertEquals("usr.Name", externalPassword.GP_UserID);
			}
		}

		public void TestGetMessageAttributes_ShouldReturnUserManagerSignature_WhenMessageSignTypeIsCompanyAndUserManagerHasValidCertificate()
		{
			using (ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var ediMessage = CreateEDIMessage(MessageSignatureType.CompanySignature);
				ediMessage.EM_SystemCreateUser = "US3";
				var resolver = new MessageSignatureXtInfoResolver(ediMessage);
				var (messageError, externalPassword, _) = resolver.GetMessageAttributes();

				AssertNull(messageError);
				AssertNotNull("externalPassword should not be null", externalPassword);
				AssertEquals("PER", externalPassword.GP_CertificateAuthority);
				AssertEquals("mngr.Name", externalPassword.GP_UserID);
			}
		}

		public void TestGetMessageAttributes_ShouldReturnCompanyUserSignature_WhenMessageSignTypeIsCompanyAndValidCompanyUser()
		{
			using (ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var ediMessage = CreateEDIMessage(MessageSignatureType.CompanySignature);
				ediMessage.EM_SystemCreateUser = "US1";
				var resolver = new MessageSignatureXtInfoResolver(ediMessage);
				var (messageError, externalPassword, _) = resolver.GetMessageAttributes();

				AssertNull(messageError);
				AssertNotNull("externalPassword should not be null", externalPassword);
				AssertEquals("PER", externalPassword.GP_CertificateAuthority);
				AssertEquals("mngr.Name", externalPassword.GP_UserID);
			}
		}

		public void TestGetMessageAttributes_ShouldReturnMessageError_WhenMessageSignTypeIsCompanyAndNoValid()
		{
			using (ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var ediMessage = CreateEDIMessage(MessageSignatureType.CompanySignature, false);
				ediMessage.EM_SystemCreateUser = "US1";
				var resolver = new MessageSignatureXtInfoResolver(ediMessage);
				var (messageError, externalPassword, _) = resolver.GetMessageAttributes();

				AssertNull("externalPassword should be null", externalPassword);
				AssertEquals("No Valid digital sign certificate found for signing this message – please review your staff or company configuration", messageError);
			}
		}

		EDIMessage CreateEDIMessage(string messageSignType, bool setupCompanyUserId = true)
		{
			var factory = Factory;
			var usrStaffWithoutCertificateNoManager = factory.NewWithValidTestData<GlbStaff>();
			usrStaffWithoutCertificateNoManager.GS_Code = "US1";

			var usrStaffWithCertificate = factory.NewWithValidTestData<GlbStaff>();
			usrStaffWithCertificate.GS_Code = "US2";
			usrStaffWithCertificate.GS_LoginName = "aaa";
			var wrapper = GlbStaffWrapper.Get(usrStaffWithCertificate);
			var usrExternalPassword = wrapper.PasswordCollection.AddNew();
			usrExternalPassword.GP_CertificateAuthority = CertificateAuthoritiesList.Codes.Comsign;
			usrExternalPassword.GP_UserID = "usr.Name";
			usrExternalPassword.CurrentDecryptedPassword = "usrPass";

			SetupStaffManagerRelationship(factory);

			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "HMZ";
			company.GC_Name = "HMY";
			var credential = CreateCredential(factory, company);
			var ediMessage = messageSignType == MessageSignatureType.PersonalSignature
					? factory.New<ILDEC275RequestMessage>()
					: messageSignType == MessageSignatureType.CompanySignature
						? factory.New<ILGPM130RequestMessage>()
						: factory.New<ILEDIMessage>();

			ediMessage.EM_SystemCreateUser = "US1";
			ediMessage.EM_GP = credential.PK;
			var glbCompanyWrapper = new GlbCompanyWrapper(ediMessage.Company);
			glbCompanyWrapper.GlbExternalPassword.GP_UserID = setupCompanyUserId ? "MGR" : ZString.Empty;

			factory.Save();
			return ediMessage;
		}

		void SetupStaffManagerRelationship(BusinessObjectFactory factory)
		{
			var usrStaffWithValidManager = factory.NewWithValidTestData<GlbStaff>();
			usrStaffWithValidManager.GS_Code = "US3";
			usrStaffWithValidManager.GS_LoginName = "XR1";
			usrStaffWithValidManager.GS_EmailAddress = "stf@mail.com";
			usrStaffWithValidManager.GS_IsActive = true;

			var mngrStaff = factory.NewWithValidTestData<GlbStaff>();
			mngrStaff.GS_Code = "MGR";
			mngrStaff.GS_IsActive = true;

			var mngrWrapper = GlbStaffWrapper.Get(mngrStaff);
			var mngrExternalPassword = mngrWrapper.PasswordCollection.AddNew();
			mngrExternalPassword.GP_CertificateAuthority = CertificateAuthoritiesList.Codes.PersonalId;
			mngrExternalPassword.GP_UserID = "mngr.Name";
			mngrExternalPassword.CurrentDecryptedPassword = "mngrPass";

			var managerLink = Factory.NewWithValidTestData<GlbStaffManager>();
			managerLink.GSM_GS_Manager = mngrStaff.PK;
			managerLink.GSM_GS_Staff = usrStaffWithValidManager.PK;
			managerLink.GSM_ManagerType = DefaultStaffReportingRoles.Codes.DirectManager;
			managerLink.GSM_IsApproved = true;
			managerLink.GSM_EffectiveDate = new ZDateTime(2025, 1, 1);
			managerLink.GSM_EndDate = ZDateTime.MaxSmallDateTime;
			usrStaffWithValidManager.Managers.Add(managerLink);
		}

		GlbCompanyCredentialICS2 CreateCredential(BusinessObjectFactory factory, GlbCompany glbCompany)
		{
			var result = factory.New<GlbCompanyCredentialICS2>();
			result.GP_GC = glbCompany.PK;
			result.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			result.GP_ExpiryDate = ZDateTime.Today.AddDays(2);
			result.GP_MailBoxID = "560038416";

			return result;
		}
	}
}
