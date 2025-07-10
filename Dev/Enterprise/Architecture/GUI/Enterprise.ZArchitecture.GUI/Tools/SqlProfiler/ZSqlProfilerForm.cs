using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.Tools
{
	partial class ZSqlProfilerForm : KForm
	{
		public ZSqlProfilerForm(SqlCommandsManager manager)
		{
			InitializeComponent();
			base.SetDataBinding(manager, "");
		}
	}
}
