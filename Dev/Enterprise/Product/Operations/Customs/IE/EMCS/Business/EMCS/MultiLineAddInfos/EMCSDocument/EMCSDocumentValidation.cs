namespace Enterprise.Customs.IE.EMCS.Business
{
	public class EMCSDocumentValidation : EU.EMCS.Business.EMCSDocumentValidation
	{
		public EMCSDocumentValidation(EMCSDocument parent)
			: base(parent)
		{
		}

		protected new EMCSDocument Parent => (EMCSDocument)base.Parent;

		protected override bool IsDescriptionRequired => Parent.CSI_SubType.IsEmpty || Parent.CSI_ReferenceNumber.IsEmpty;
	}
}
