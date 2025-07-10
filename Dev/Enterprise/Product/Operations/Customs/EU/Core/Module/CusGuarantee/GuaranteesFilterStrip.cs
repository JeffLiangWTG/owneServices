using System.Windows.Forms;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Module
{
	public partial class GuaranteesFilterStrip : ZFilterStrip
	{
		public GuaranteesFilterStrip()
		{
			InitializeComponent();
		}

		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			if (currentModuleFilter is CusAuthorisationsRuleModuleFilter)
			{
				var control = new CusAuthorisationsRuleModuleFilterStrip { TabIndex = 1 };
				Controls.Find("FilterCategoriesToolStrip", searchAllChildren: false)[0].AllowOverlap(control);
				FilterDescriptionDropEdit.AllowOverlap(control);
				FilterPropertyLockButton.AllowOverlap(control);
				return new Control[] { control };
			}
			return base.GetCurrentFilterControls(currentModuleFilter);
		}
	}
}
