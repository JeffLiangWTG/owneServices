using Enterprise.Core.Modules;

namespace Enterprise.ZArchitecture.Modules
{
	public class ModuleTreeCategories(ModuleCategory jump, ModuleCategory operate, ModuleCategory manage, ModuleCategory maintain)
	{
		public ModuleCategory Jump { get; } = jump;
		public ModuleCategory Operate { get; } = operate;
		public ModuleCategory Manage { get; } = manage;
		public ModuleCategory Maintain { get; } = maintain;
	}
}
