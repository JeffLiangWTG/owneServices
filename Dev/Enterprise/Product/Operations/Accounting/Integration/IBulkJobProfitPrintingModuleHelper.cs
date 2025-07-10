using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Integration
{
	public delegate void JobProfitPrintingDelegate();

	public interface IBulkJobProfitPrintingModuleHelper
	{
		IMenuItem GetMenuItem(JobProfitPrintingDelegate jobProfitPrintingDelegate);
		void PrintJobProfitDocument(BusinessObjectFactory factory, BusinessObject[] selectedElements);
	}
}
