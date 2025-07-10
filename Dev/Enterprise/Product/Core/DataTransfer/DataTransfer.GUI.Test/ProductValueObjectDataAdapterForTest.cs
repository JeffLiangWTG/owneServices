using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.GUI.Testing
{
	sealed class ProductValueObjectDataAdapterForTest : ProductValueObjectDataAdapter
	{
		protected override bool ConfirmUpdateOfExistingBusinessObject(OrgSupplierPart bizObj, CargoWise.ComponentModel.INotifications notifications)
		{
			bool result = base.ConfirmUpdateOfExistingBusinessObject(bizObj, notifications);
			IsConfirmUpdateOfExistingBusinessObjectCalled = true;
			return result;
		}

		public bool IsConfirmUpdateOfExistingBusinessObjectCalled;
	}
}
