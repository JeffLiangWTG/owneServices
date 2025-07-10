using System.Windows.Forms;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module
{
	public partial class AccountingOnFormFilterStrip : WorkflowFilterStrip, IAccountingOnFormFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			if (currentModuleFilter is ReferenceNumberFilter)
			{
				return ReferenceNumberFilterGUIProvider.GetReferenceNumberFilterControls(this, FilterControlBindingSource);
			}
			else
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}
		}
	}
}
