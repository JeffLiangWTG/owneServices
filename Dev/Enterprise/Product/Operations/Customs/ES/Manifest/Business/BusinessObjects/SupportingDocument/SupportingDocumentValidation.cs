using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Manifest.Business
{
	public class SupportingDocumentValidation : Customs.Business.CusSupportingInfoValidation
	{
		public SupportingDocumentValidation(SupportingDocument parent)
			: base(parent)
		{
		}
		public new SupportingDocument Parent => (SupportingDocument)base.Parent;
		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}
	}
}
