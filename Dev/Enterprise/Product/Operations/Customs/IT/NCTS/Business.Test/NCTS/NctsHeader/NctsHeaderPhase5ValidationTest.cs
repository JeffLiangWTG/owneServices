using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsHeaderPhase5ValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckRepresentationTypeDoesNotAddAnyNotification()
	{
		nctsHeader.RepresentationType = "~";
		AssertNoNotifications(nctsHeader.RepresentationTypeInfo);
	}

	public void TestCheckDeclarantAddressPKDoesNotAddAnyNotification()
	{
		nctsHeader.RepresentationType = EU.Business.RepresentationTypeList.Codes._2Direct;
		SimulateUserToEnterAndEmptyDeclarantAddress();
		AssertNoNotifications(nctsHeader.DeclarantAddressPKInfo);

		void SimulateUserToEnterAndEmptyDeclarantAddress()
		{
			nctsHeader.DeclarantAddressPK = ZGuid.BrettsGuid;
			nctsHeader.DeclarantAddressPK = ZGuid.Empty;
		}
	}

	public void TestCheckAuthorizationDoesNotAddAnyNotification()
	{
		nctsHeader.Authorization = "yyyyyyy";
		AssertNoNotifications(nctsHeader.AuthorizationInfo);
	}

	public void TestCheckSubscriberDoesNotAddAnyNotification()
	{
		nctsHeader.Subscriber = ZString.Empty;
		AssertNoNotifications(nctsHeader.SubscriberInfo);
	}

	public void TestValidateConsignorMustNotBeDeclaredAtHeaderLevelWhenParticipantsTypeIsGroupageDoesNotAddAnyNotification()
	{
		var organization = Factory.New<OrgHeader>();
		organization.MainAddress.Postcode = "12345";
		organization.CustomsCodes.AddNew("IVA", "1234567890");

		nctsHeader.MovementHeader.ParticipantType = "GRP";
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		goodsItem.Consignor.OrganisationPK = organization.PK;
		nctsHeader.Consignor.OrganisationPK = organization.PK;
		AssertNoNotifications(nctsHeader.Consignor.OrganisationPKInfo);
	}

	public void TestValidateConsigneeMustNotBeDeclaredAtHeaderLevelWhenParticipantsTypeIsGroupageDoesNotAddAnyNotification()
	{
		var organization = Factory.New<OrgHeader>();
		organization.MainAddress.Postcode = "12345";

		nctsHeader.MovementHeader.ParticipantType = "GRP";
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		goodsItem.Consignee.OrganisationPK = organization.PK;
		nctsHeader.Consignee.OrganisationPK = organization.PK;
		AssertNoNotifications(nctsHeader.Consignee.OrganisationPKInfo);
	}

	public void TestCheckBH_CustomsProfile_ValueNotEntered()
	{
		nctsHeader.BH_CustomsProfile = "1234";
		AssertNoMessageErrorContaining("Empty Node", nctsHeader.BH_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);

		nctsHeader.BH_CustomsProfile = ZString.Empty;
		AssertHasMessageErrorContaining("Empty Node", nctsHeader.BH_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckBH_CustomsProfile_ListValidation()
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var companyWrapper = IT.Business.GlbCompanyWrapper.Get(currentCompany);

			var accountDetail = companyWrapper.PasswordCollection.AddNew();
			accountDetail.GP_UserID = "1234";
			accountDetail.GP_Name = "TestCert1";
			accountDetail.GP_MailBoxID = "11111111111-001";

			accountDetail.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			accountDetail.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			currentCompany.Factory.Save();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			CombineAssertions(() =>
			{
				nctsHeader.BH_CustomsProfile = "TestCert1";
				AssertNoMessageErrorContaining(nctsHeader.BH_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);

				nctsHeader.BH_CustomsProfile = ZString.Empty;
				AssertHasMessageErrorContaining(nctsHeader.BH_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_ApplicationCode = "NC5";
	}

	NctsHeader nctsHeader;
}
