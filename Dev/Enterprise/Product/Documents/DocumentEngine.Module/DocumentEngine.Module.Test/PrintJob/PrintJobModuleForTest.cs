using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.Module.Testing
{
	sealed class PrintJobModuleForTest : PrintJobModule, IPrintJobModuleForTest
	{
		public PrintJobModuleForTest()
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

		public new MenuItem[] GetNewStandardMenuItems()
		{
			return base.GetNewStandardMenuItems();
		}

		public new MenuItem[] GetNewActionMenuItems()
		{
			return base.GetNewActionMenuItems();
		}

		public void PublicPerformSearch()
		{
			PerformSearch();
		}

		public override BusinessObject[] GetSelectedBusinessObjects()
		{
			return SelectedJobs.ToArray();
		}

		public List<StmPrintJob> SelectedJobs { get; } = new List<StmPrintJob>();
	}
}
