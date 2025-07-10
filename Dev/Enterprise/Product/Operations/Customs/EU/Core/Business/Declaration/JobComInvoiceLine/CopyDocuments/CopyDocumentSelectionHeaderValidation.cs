using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration;

public class CopyDocumentsSelectionHeaderValidation(AutoCopyDocumentsSelectionHeader parent) : AutoCopyDocumentsSelectionHeaderValidation(parent)
{
	protected override void CheckInvoiceNumber()
	{
		base.CheckInvoiceNumber();
		MandatoryValidation.CheckEntered(Parent.InvoiceNumberInfo);
		ListValidation.ErrorIfInvalidCode(Parent.InvoiceNumberInfo);
	}
}
