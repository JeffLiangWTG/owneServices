using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.KR;

namespace Enterprise.Customs.KR.Business
{
	public class GlbCompanyWrapper : MasterFiles.Business.GlbCompanyWrapper, IKRGlbCompanyWrapper
	{
		protected GlbCompanyWrapper(GlbCompany company)
			: base(company)
		{
		}

		public GlbCompanyCredential CertificateForUnipass
		{
			get
			{
				if (certificateForUnipass == null)
				{
					certificateForUnipass = GetGlbExternalPasswordOrCreateNew<GlbCompanyCredential>(PasswordTypesList.Codes.KRB);
					RegisterEditableChildObject(certificateForUnipass);
				}

				return certificateForUnipass;
			}
		}
		GlbCompanyCredential certificateForUnipass;

		public override bool IsValidWrapper => true;

		IGlbExternalPassword IKRGlbCompanyWrapper.CertificateForUnipass => CertificateForUnipass;

		public bool IsCertError
		{
			get
			{
				var result = false;
				var now = ZDateTime.Now;
				if (now > CertificateForUnipass.GP_ExpiryDate || now < CertificateForUnipass.GP_IssueDate)
				{
					result = true;
				}
				return result;
			}
		}

		public bool IsValidForMessaging
		{
			get
			{
				var result = false;
				var unipassDeclarantID = KRCustomsRegistry.Instance.UNIPASSDeclarantID.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (!string.IsNullOrEmpty(unipassDeclarantID) && CertificateForUnipass.GP_Certificate != null && !IsCertError)
				{
					result = true;
				}
				return result;
			}
		}
	}
}
