using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class CertificateOfOriginValidation : CusSupportingInfoValidation
	{
		public CertificateOfOriginValidation(CertificateOfOrigin parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_SubTypeInfo);
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}

		protected override void CheckCSI_Procedure()
		{
			base.CheckCSI_Procedure();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_ProcedureInfo);
		}

		protected override void CheckCSI_RN_NKCountryCode()
		{
			base.CheckCSI_RN_NKCountryCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_RN_NKCountryCodeInfo);
		}

		protected override void CheckCSI_Status()
		{
			base.CheckCSI_Status();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_StatusInfo);
		}

		public new CertificateOfOrigin Parent => (CertificateOfOrigin)base.Parent;
	}
}
