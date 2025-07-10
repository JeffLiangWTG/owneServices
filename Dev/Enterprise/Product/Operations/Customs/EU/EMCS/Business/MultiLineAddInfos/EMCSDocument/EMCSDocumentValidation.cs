using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSDocumentValidation : CusSupportingInfoValidation
	{
		public EMCSDocumentValidation(EMCSDocument parent)
			: base(parent)
		{
			declaration = parent.Declaration;
		}
		readonly EMCSJobDeclaration declaration;

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			if (IsDescriptionRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo);
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
			RelatedObjectValidation.MaxLengthValidation(Parent.CSI_ReferenceNumberInfo, Parent.CSI_ReferenceNumberInfo.MaxLength);
			CheckDocumentIsUnique(Parent.CSI_ReferenceNumberInfo);
		}

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_SubTypeInfo);
		}

		protected new EMCSDocument Parent => (EMCSDocument)base.Parent;

		void CheckDocumentIsUnique(ZPropertyInfo info)
		{
			if (declaration != null && !(Parent.CSI_Description.IsEmpty || Parent.CSI_ReferenceNumber.IsEmpty))
			{
				var documents = declaration.Documents.Cast<EMCSDocument>();
				if (documents.Any(d => d != Parent && d.KeyToDetermineUniqueness == Parent.KeyToDetermineUniqueness))
				{
					info.AddMessageError(Res.GetString("a4499c4a-de29-47c9-925c-152d2d27c8d8", "Description and Reference combination must be unique."));
				}
			}
		}
		protected virtual bool IsDescriptionRequired => true;
	}
}
