using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEPreviousDocumentValidation : PreviousDocumentValidation
	{
		public DeltaIEPreviousDocumentValidation(PreviousDocument parent) : base(parent)
		{
		}

		protected override bool IsSubTypeMandatory => false;

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

		void CheckIfDuplicateTypeAndReferenceInInvoiceAndDeclarationLevel(PreviousDocument parent, ZPropertyInfo propertyInfo)
		{
			if (!parent.CSI_Code.IsEmpty && !parent.CSI_ReferenceNumber.IsEmpty)
			{
				if (parent.Parent is JobComInvoiceHeader invoiceHeader)
				{
					var declaration = parent.Declaration;
					if (declaration?.PreviousDocuments.Cast<PreviousDocument>().Any(x => x.CSI_Code == parent.CSI_Code && x.CSI_ReferenceNumber == parent.CSI_ReferenceNumber) ?? false)
					{
						propertyInfo.AddMessageError(Res.GetString("27E56BF7-DC93-4ABA-9C70-AF3D6221EC57", "The type and reference number of this record is duplicate with records under Misc tab."));
					}
				}
			}
		}
	}
}
