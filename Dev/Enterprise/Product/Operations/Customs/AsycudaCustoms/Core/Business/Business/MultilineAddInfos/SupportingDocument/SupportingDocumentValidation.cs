using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class SupportingDocumentValidation : CusSupportingInfoValidation
	{
		public SupportingDocumentValidation(SupportingDocument parent) : base(parent) { }

		new SupportingDocument Parent => (SupportingDocument)base.Parent;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
		}
	}
}
