using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZModuleButtonGridWithCustomButtonTestForm : ZModuleButtonGridTestForm
	{
		public ZModuleButtonGridWithCustomButtonTestForm(IBusiness entity)
			: base(entity)
		{ }

		protected override ZDummyModuleButtonGrid GetModuleButtonGrid()
		{
			return new ZDummyModuleButtonGridWithCustomButton();
		}
	}
}
