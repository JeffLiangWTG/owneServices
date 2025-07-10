using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TestDropEdit : ZDropEdit
	{
		public ZFilterModule FilterModuleForTesting { get; set; }

		protected override ZFilterModule NewModuleFromModuleID()
		{
			return FilterModuleForTesting ?? base.NewModuleFromModuleID();
		}
	}
}
