using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZModuleButtonGridTestFormAlwaysShowFormInReadOnlyMode : ZModuleButtonGridTestForm
	{
		public ZModuleButtonGridTestFormAlwaysShowFormInReadOnlyMode(IBusiness entity)
			: base(entity)
		{ }

		protected override ZDummyModuleButtonGrid GetModuleButtonGrid()
		{
			return new ZDummyModuleButtonGridAlwaysShowFormInReadOnlyMode();
		}
	}
}
