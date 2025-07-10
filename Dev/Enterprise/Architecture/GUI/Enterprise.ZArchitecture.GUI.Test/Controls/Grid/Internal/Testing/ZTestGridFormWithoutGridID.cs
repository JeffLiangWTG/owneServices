using CargoWise.EntityFramework;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[SuppressFormDesignerAnalysis]
	sealed class ZTestGridFormWithoutGridID : ZTestGridForm
	{
		public ZTestGridFormWithoutGridID(IBusiness entity)
			: base(entity)
		{
		}
	}
}
