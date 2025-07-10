using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIESupportingDocumentValidation : SupportingDocumentValidation
	{
		public DeltaIESupportingDocumentValidation(SupportingDocument parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			CheckIfDuplicateTypeAndReferenceInInvoiceAndDeclarationLevel(Parent, Parent.CSI_CodeInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			CheckIfDuplicateTypeAndReferenceInInvoiceAndDeclarationLevel(Parent, Parent.CSI_ReferenceNumberInfo);
		}

		void CheckIfDuplicateTypeAndReferenceInInvoiceAndDeclarationLevel(SupportingDocument parent, ZPropertyInfo propertyInfo)
		{
			if (!parent.CSI_Code.IsEmpty && !parent.CSI_ReferenceNumber.IsEmpty)
			{
				if (parent.Parent is JobComInvoiceHeader invoiceHeader)
				{
					var declaration = parent.Declaration;
					if (declaration?.SupportingDocuments.Cast<SupportingDocument>().Any(x => x.CSI_Code == parent.CSI_Code && x.CSI_ReferenceNumber == parent.CSI_ReferenceNumber) ?? false)
					{
						propertyInfo.AddMessageError(Res.GetString("E0B1EDA6-B12D-4A28-94C7-5F9CFAB3CFBE", "The type and reference number of this record is duplicate with records under Misc tab."));
					}
				}
			}
		}

		protected override bool ShouldWarnOnCSI_ReferenceNumberLength => false;

		protected override bool ShouldWarnOnCSI_AdditionalDescriptionLength => false;

		protected override bool ShouldWarnOnCSI_DescriptionLength => false;
	}
}
