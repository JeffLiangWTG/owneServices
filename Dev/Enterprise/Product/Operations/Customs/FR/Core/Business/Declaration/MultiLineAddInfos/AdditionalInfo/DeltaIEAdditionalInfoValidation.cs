using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEAdditionalInfoValidation : AdditionalInfoValidation
	{
		public DeltaIEAdditionalInfoValidation(AdditionalInfo parent) : base(parent)
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

		void CheckIfDuplicateTypeAndReferenceInInvoiceAndDeclarationLevel(AdditionalInfo parent, ZPropertyInfo propertyInfo)
		{
			if (!parent.CSI_Code.IsEmpty && !parent.CSI_ReferenceNumber.IsEmpty)
			{
				if (parent.Parent is JobComInvoiceHeader invoiceHeader)
				{
					var declaration = parent.Declaration;
					if (declaration?.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == parent.CSI_Code && x.CSI_ReferenceNumber == parent.CSI_ReferenceNumber) ?? false)
					{
						propertyInfo.AddMessageError(Res.GetString("357947B1-2D22-4681-90CB-14552062FAE9", "The type and reference number of this record is duplicate with records under Misc tab."));
					}
				}
			}
		}
	}
}
