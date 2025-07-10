using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	public partial class FtpJobConfigControl : ZUserControl
	{
		public FtpJobConfigControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				var configObj = new FtpJobConfig((BusinessObject)dataSource);
				base.SetDataBinding(configObj, "");
			}
			else
			{
				base.SetDataBinding(null, "");
			}
		}
	}
}
