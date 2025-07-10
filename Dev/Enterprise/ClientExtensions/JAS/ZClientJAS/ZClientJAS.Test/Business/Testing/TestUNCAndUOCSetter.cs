using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.Testing
{
	public static class TestUNCAndUOCSetter
	{
		public static OrgHeader SetCurrentOrgProxy(BusinessObjectFactory factory, ZString orgCode, ZString officeCode, ZString nettingCode)
		{
			OrgHeader originalOrgProxy = GlbCompany.CurrentCompany.OrgProxy;
			if (GlbCompany.CurrentCompany.OrgProxy == null)
			{
				OrgHeader org = factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = orgCode;
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
			}

			SetNettingCode(factory, GlbCompany.CurrentCompany.OrgProxy, nettingCode);
			SetOfficeCode(factory, GlbCompany.CurrentCompany.OrgProxy, officeCode);
			return originalOrgProxy;
		}

		public static void SetNettingCode(BusinessObjectFactory factory, OrgHeader org, ZString nettingCode)
		{
			SetCode(factory, org, OrgCusCode.CodeTypes.UniversalNettingCode, nettingCode);
		}

		public static void SetOfficeCode(BusinessObjectFactory factory, OrgHeader org, ZString officeCode)
		{
			SetCode(factory, org, OrgCusCode.CodeTypes.UniversalOfficeCode, officeCode);
		}

		static void SetCode(BusinessObjectFactory factory, OrgHeader org, ZString codeType, ZString code)
		{
			OrgCusCode cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = codeType;
			cusCode.OK_CustomsRegNo = code;
		}
	}
}
