using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.ClientSharedComponents.Registry
{
	partial class ServiceTaskDataTransferRegistryControl : DataTransferRegistryControl
	{
		public ServiceTaskDataTransferRegistryControl() : base()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			AutomaticProcessRegistryBusinessObject parentRegistryObject = CurrentDataItem as AutomaticProcessRegistryBusinessObject;
			if (parentRegistryObject != null)
			{
				parentRegistryObject.ValidateIntervalTypeAction = null;
				parentRegistryObject.ValidateIntervalAction = null;
				parentRegistryObject.ValidateNextRunDateAction = null;
			}
		}
	}
}
