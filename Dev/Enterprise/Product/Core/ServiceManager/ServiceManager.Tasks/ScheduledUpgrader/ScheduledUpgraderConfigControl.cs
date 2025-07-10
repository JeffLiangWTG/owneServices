using CargoWise.Application;
using Enterprise.ZArchitecture.GUI;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.ServiceManager.Tasks.ScheduledUpgrader
{
	public partial class ScheduledUpgraderConfigControl : ZUserControl
	{
		public ScheduledUpgraderConfigControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				var configObj = new ScheduledUpgraderConfig(ObjectFactory.Get<IServiceTaskAccessor>().GetServiceTask(dataSource));
				base.SetDataBinding(configObj, "");
			}
			else
			{
				base.SetDataBinding(null, "");
			}
		}
	}
}
