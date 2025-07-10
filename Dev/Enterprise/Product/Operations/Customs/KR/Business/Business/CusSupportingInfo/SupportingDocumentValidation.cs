using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class SupportingDocumentValidation : CusSupportingInfoValidation
	{
		public SupportingDocumentValidation(SupportingDocument parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}

		public new SupportingDocument Parent => (SupportingDocument)base.Parent;
	}
}
