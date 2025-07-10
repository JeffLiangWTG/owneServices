using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.Scheduler.Module.Testing
{
	sealed class ScheduledReportsModuleForTest : ScheduledReportsModule
	{
		public ScheduledReportsModuleForTest()
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

		public new MenuItem[] GetNewAdditionalMenuItems()
		{
			return base.GetNewAdditionalMenuItems();
		}

		public void SetSelectedGridElements(BusinessObject[] selectedGridElements)
		{
			fSelectedBusinessObjects = selectedGridElements;
		}

		protected override BusinessObject[] SelectedBusinessObjects
		{
			get { return fSelectedBusinessObjects; }
		}
		public MenuItem[] GetNewActionMenuItemsExposed()
		{
			return base.GetNewActionMenuItems();
		}

		public BusinessObject[] fSelectedBusinessObjects;

		protected override BusinessObject CurrentBusinessObjectInGrid => CurrentBusinessObjectInGrid_Exposed;

		public BusinessObject CurrentBusinessObjectInGrid_Exposed;
	}
}
