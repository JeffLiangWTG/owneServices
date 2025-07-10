using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.Module.Testing
{
	sealed class PrintQueueModuleForTest : PrintQueueModule
	{
		public PrintQueueModuleForTest()
		{
		}

		public IFilterControl NewFilterControl
		{
			get { return GetNewFilterControl(); }
		}

		public IBusinessObjectCollection NewGridCollection
		{
			get { return GetNewGridCollection(); }
		}

		public FilterBusinessObject NewFilterBusinessObject
		{
			get { return GetNewFilterBusinessObject(); }
		}

		public void ShowDeleteFormForTesting(BusinessObject selectedBizO)
		{
			base.ShowDeleteForm(selectedBizO);
		}

		public void PerformSearchForTest()
		{
			base.PerformSearch();
		}

		public void ReplacePrintQueueForTesting()
		{
			base.ReplacePrintQueue_Click(null, null);
		}
	}
}
