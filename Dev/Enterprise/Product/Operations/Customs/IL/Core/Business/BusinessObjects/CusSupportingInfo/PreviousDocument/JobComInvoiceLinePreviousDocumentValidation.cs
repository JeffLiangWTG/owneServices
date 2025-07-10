using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.Business
{
	public class JobComInvoiceLinePreviousDocumentValidation : PreviousDocumentValidation
	{
		public JobComInvoiceLinePreviousDocumentValidation(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
		}

		protected override void CheckCSI_ReferenceNumber2()
		{
			base.CheckCSI_ReferenceNumber2();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumber2Info);
		}

		protected override void CheckCSI_ItemNumber()
		{
			base.CheckCSI_ItemNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ItemNumberInfo);
		}

		protected override void CheckCSI_Quantity()
		{
			base.CheckCSI_Quantity();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_QuantityInfo);
		}

		protected override void CheckCSI_UnitOfQuantity()
		{
			base.CheckCSI_UnitOfQuantity();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_UnitOfQuantityInfo);
		}
	}
}
