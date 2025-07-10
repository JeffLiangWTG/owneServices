using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Registry.Business
{
	public class InvoiceRollupAndGroupDescriptionValidation
	{
		public InvoiceRollupAndGroupDescriptionValidation(InvoiceRollupAndGroupDescription parent)
		{
			Parent = parent;
		}

		readonly InvoiceRollupAndGroupDescription Parent;

		public void ValidateEnglishDescription()
		{
			Parent.EnglishDescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(Parent.EnglishDescriptionInfo);
		}
	}
}