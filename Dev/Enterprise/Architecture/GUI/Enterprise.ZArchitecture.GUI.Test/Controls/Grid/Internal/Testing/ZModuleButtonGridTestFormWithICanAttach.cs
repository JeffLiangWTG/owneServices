using CargoWise.EntityFramework;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[SuppressFormDesignerAnalysis]
	sealed class ZModuleButtonGridTestFormWithICanAttach : ZModuleButtonGridTestForm, ICanAttachWithoutSecurity
	{
		public ZModuleButtonGridTestFormWithICanAttach(IBusiness entity)
			: base(entity)
		{ }
	}
}
