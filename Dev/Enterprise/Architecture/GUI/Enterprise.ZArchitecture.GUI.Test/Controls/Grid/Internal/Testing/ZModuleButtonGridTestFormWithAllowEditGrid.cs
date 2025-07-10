using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZModuleButtonGridTestFormWithAllowEditGrid : ZModuleButtonGridTestForm
	{
		public ZModuleButtonGridTestFormWithAllowEditGrid(IBusiness entity)
			: base(entity)
		{ }

		protected override ZDummyModuleButtonGrid GetModuleButtonGrid()
		{
			return new ZDummyModuleButtonGridWithAllowEdit();
		}
	}
}
