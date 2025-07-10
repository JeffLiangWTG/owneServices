using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.Module.Testing
{
	internal interface IPrintJobModuleForTest : IDisposable
	{
		FilterBusinessObject NewFilterBusinessObject { get; }
		IFilterControl NewFilterControl { get; }
		IBusinessObjectCollection NewGridCollection { get; }
		List<StmPrintJob> SelectedJobs { get; }

		MenuItem[] GetNewActionMenuItems();
		MenuItem[] GetNewStandardMenuItems();
		BusinessObject[] GetSelectedBusinessObjects();
		void PublicPerformSearch();
	}
}
