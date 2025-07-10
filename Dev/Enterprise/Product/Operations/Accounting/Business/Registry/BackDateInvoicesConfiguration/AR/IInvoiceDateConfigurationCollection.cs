using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Registry.Business
{
	public interface IInvoiceDateConfigurationCollection
	{
		BusinessObject AddNew();
		void RunPreSaveValidation();
	}
}
