using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class CertificateOfOriginValidation : Customs.Business.CusSupportingInfoValidation
	{
		public CertificateOfOriginValidation(CertificateOfOrigin parent) : base(parent)
		{
		}

		public new CertificateOfOrigin Parent => (CertificateOfOrigin)base.Parent;

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
		}
	}
}
