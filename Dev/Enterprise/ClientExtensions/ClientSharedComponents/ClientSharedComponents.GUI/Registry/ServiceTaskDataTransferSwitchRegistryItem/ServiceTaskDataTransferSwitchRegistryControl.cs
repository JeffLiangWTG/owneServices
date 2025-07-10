using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.ClientSharedComponents.Registry
{
	partial class ServiceTaskDataTransferSwitchRegistryControl : DataTransferSwitchRegistryControl
	{
		public ServiceTaskDataTransferSwitchRegistryControl() : base()
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
