using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class SuspensionDrawbackImportEntryDocumentValidation : Customs.Business.CusSupportingInfoValidation
	{
		public SuspensionDrawbackImportEntryDocumentValidation(SuspensionDrawbackImportEntryDocument parent) : base(parent)
		{
		}
		public new SuspensionDrawbackImportEntryDocument Parent => (SuspensionDrawbackImportEntryDocument)base.Parent;

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_SubTypeInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
		}
	}
}
