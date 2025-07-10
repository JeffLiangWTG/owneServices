using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.IE;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusExitHeaderValidation : EU.ExitControl.Business.CusExitHeaderValidation
	{
		public CusExitHeaderValidation(CusExitHeader parent)
			: base(parent)
		{
		}

		protected new CusExitHeader Parent => (CusExitHeader)base.Parent;

		protected override void ValidateCarrierEORI(ZString carrierEORI)
		{
			if (carrierEORI.IsEmpty)
			{
				Parent.CXH_OA_CarrierInfo.AddMessageError(TheSelectedOrganizationMustHaveAValidEORIMessage);
			}
			else if (Parent.CusExitReports.Any(r => r.CER_Type == ExitReportTypeList.Codes.ExitNotification)
					&& Parent.Company is GlbCompany company
					&& IE.Business.GlbCompanyWrapper.Get(company) is IIEGlbCompanyWrapper wrapper
					&& wrapper.GetGlbExternalPassword() is GlbCompanyCredential companyCredential
					&& companyCredential.GP_MailBoxID != carrierEORI)
			{
				Parent.CXH_OA_CarrierInfo.AddMessageError(Res.GetString("477CF38F-80C2-4F9E-B681-227C2FE708D3", "Carrier is the Declarant on a Exit Notification (IE590). Carrier EORI must match the Message Sender EORI associated with your company's Revenue Online Services Credentials. This is entered on the company record."));
			}
		}

		protected override void CheckCXH_GS_NKCustomsAgent()
		{
			base.CheckCXH_GS_NKCustomsAgent();
			var info = Parent.CXH_GS_NKCustomsAgentInfo;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(info);
		}
	}
}
