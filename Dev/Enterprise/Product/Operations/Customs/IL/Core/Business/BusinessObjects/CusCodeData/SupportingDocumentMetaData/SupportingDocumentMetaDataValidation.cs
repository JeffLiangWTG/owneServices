using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class SupportingDocumentMetaDataValidation : CusCodeDataValidation
	{
		public SupportingDocumentMetaDataValidation(SupportingDocumentMetaData parent) : base(parent)
		{
		}

		protected new SupportingDocumentMetaData Parent => (SupportingDocumentMetaData)base.Parent;

		protected override void CheckCY_CodeList()
		{
			ListValidation.ErrorIfInvalidCode(Parent.CY_CodeInfo, Parent.Lookups.CY_CodeList);
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();

			var parent = Parent;
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(parent.CY_DataInfo, parent.MandatoryInfo, ZBool.True, ValidationCaptions.SupportingDocumentMetaData.ValueIsMandatory);
		}
	}
}
