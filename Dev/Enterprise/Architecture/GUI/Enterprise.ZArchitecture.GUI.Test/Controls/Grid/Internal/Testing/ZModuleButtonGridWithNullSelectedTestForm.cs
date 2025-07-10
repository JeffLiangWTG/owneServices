using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZModuleButtonGridWithNullSelectedTestForm : ZModuleButtonGridTestForm
	{
		public ZModuleButtonGridWithNullSelectedTestForm(IBusiness entity)
			: base(entity)
		{
		}

		protected override ZDummyModuleButtonGrid GetModuleButtonGrid()
		{
			return new ZDummyNullSelectedModuleButtonGrid();
		}
	}
}
