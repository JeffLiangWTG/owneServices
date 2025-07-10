using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZActivityLoggingTabPage : ZBindingTabPage
	{
		public ZActivityLoggingTabPage()
		{
			ActivityLoggingUserControl = new ZActivityLoggingUserControl();
			ActivityLoggingUserControl.Dock = DockStyle.Fill;
			Controls.Add(ActivityLoggingUserControl);
			ExcludeFromBindingOnSave = true;
		}

		[DefaultValue(true)]
		public override bool ExcludeFromBindingOnSave
		{
			get { return base.ExcludeFromBindingOnSave; }
			set { base.ExcludeFromBindingOnSave = value; }
		}

		#region Implementation

		StmActivityLogCollection ActivityLogs;
		StmActivityLogFilterProvider ActivityLogFilterProvider;
		readonly ZActivityLoggingUserControl ActivityLoggingUserControl;

		protected override void SetDataBindingCore(object dataSource, string dataMember)
		{
			ActivityLogs = new StmActivityLogCollectionByParent((BusinessObject)dataSource);
			ActivityLogFilterProvider = new StmActivityLogFilterProvider(ActivityLogs);
			ActivityLogs.Load();
			((IDataBoundControl)ActivityLoggingUserControl).SetDataBinding(ActivityLogFilterProvider, "");
			base.SetDataBindingCore(ActivityLogFilterProvider, "");
		}

		#endregion
	}
}
