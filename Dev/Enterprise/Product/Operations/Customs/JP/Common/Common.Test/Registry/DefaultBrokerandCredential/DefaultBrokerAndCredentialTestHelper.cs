using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Common.Testing
{
	public sealed class DefaultBrokerAndCredentialTestHelper
	{
		public void CreateBrokerStaff()
		{
			CreateBrokerStaffAndReturnPK();
		}

		public (ZGuid credentialPK1, ZGuid credentialPK2, ZGuid credentialPK3) CreateBrokerStaffAndReturnPK()
		{
			var factory = new BusinessObjectFactory();

			var currentBranch = GlbBranch.CurrentBranch;
			currentBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			currentBranch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var currentCompanyPk = GlbCompany.CurrentCompany.PK;

			var broker1 = factory.NewWithValidTestData<GlbStaff>();
			GlbStaff.CurrentUser.GS_GB_HomeBranch = currentBranch.PK;
			broker1.GS_GB_HomeBranch = currentBranch.PK;
			broker1.GS_LoginName = "Ayachi";
			broker1.GS_FullName = "Ayachi Ne";
			broker1.GS_Code = "AN";

			var broker2 = factory.NewWithValidTestData<GlbStaff>();
			GlbStaff.CurrentUser.GS_GB_HomeBranch = currentBranch.PK;
			broker2.GS_GB_HomeBranch = currentBranch.PK;
			broker2.GS_LoginName = "Kamisato";
			broker2.GS_FullName = "Kamisato Ayaka";
			broker2.GS_Code = "KA";

			var glbExternalPassword1 = factory.New<GlbExternalPasswordCUS>();
			glbExternalPassword1.GP_GC = currentCompanyPk;
			glbExternalPassword1.GP_GS = broker1.PK;
			glbExternalPassword1.GP_PasswordType = JPPasswordType.Codes.CUS;
			glbExternalPassword1.GP_Transport = UserCodeSpecificTransportModeList.Codes.SEA;
			glbExternalPassword1.GP_MailBoxID = "Test1";
			glbExternalPassword1.GP_UserID = "001";
			glbExternalPassword1.CurrentDecryptedPassword = "12345678";

			var glbExternalPassword2 = factory.New<GlbExternalPasswordCUS>();
			glbExternalPassword2.GP_GC = currentCompanyPk;
			glbExternalPassword2.GP_GS = broker1.PK;
			glbExternalPassword2.GP_PasswordType = JPPasswordType.Codes.CUS;
			glbExternalPassword2.GP_Transport = UserCodeSpecificTransportModeList.Codes.AIR;
			glbExternalPassword2.GP_MailBoxID = "Test2";
			glbExternalPassword2.GP_UserID = "002";

			var glbExternalPassword3 = factory.New<GlbExternalPasswordCUS>();
			glbExternalPassword3.GP_GC = currentCompanyPk;
			glbExternalPassword3.GP_GS = broker1.PK;
			glbExternalPassword3.GP_PasswordType = JPPasswordType.Codes.CUS;
			glbExternalPassword3.GP_Transport = UserCodeSpecificTransportModeList.Codes.BTH;
			glbExternalPassword3.GP_MailBoxID = "Test3";
			glbExternalPassword3.GP_UserID = "003";

			factory.Save();

			return (glbExternalPassword1.PK, glbExternalPassword2.PK, glbExternalPassword3.PK);
		}
	}
}
