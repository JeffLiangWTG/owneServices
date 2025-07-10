using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.Module.Testing
{
	sealed class UPEJobDeclarationModuleForTest : UPEJobDeclarationModule
	{
		public void PerformOnMoveToClassifierQueue_Click()
		{
			MoveToClassifierQueue_Click(null, new EventArgs());
		}

		public new ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return base.GetNewController(selectedBusinessObject);
		}

		public new IFilterControl GetNewFilterControl()
		{
			return base.GetNewFilterControl();
		}

		public new IBusinessObjectCollection GetNewGridCollection()
		{
			return base.GetNewGridCollection();
		}

		public new FilterBusinessObject GetNewFilterBusinessObject()
		{
			return base.GetNewFilterBusinessObject();
		}

		public new MenuItem[] GetNewActionMenuItems()
		{
			return base.GetNewActionMenuItems();
		}

		public new BusinessObjectFactory Factory
		{
			get { return base.Factory; }
		}

		public TestBulkStatusUpdatingForm LastCreatedBulkStatusUpdatingForm;

		public BusinessObject[] SelectedGridElements;

		protected override BulkStatusUpdatingForm NewBulkStatusUpdatingForm(BusinessObject[] selectedItems)
		{
			LastCreatedBulkStatusUpdatingForm = new TestBulkStatusUpdatingForm(FilterBusinessObject, SelectedGridElements);
			return LastCreatedBulkStatusUpdatingForm;
		}

		public BusinessObject[] fSelectedElements;
		protected override IReadOnlyList<BusinessObject> SelectedElements
		{
			get { return fSelectedElements ?? (Array.Empty<BusinessObject>()); }
		}
	}
}
