
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Security.Provider
{
	abstract class StmMenuItemCheckpointHelper
	{
		internal abstract string ModuleIDPrefix { get; }
		internal abstract string ModuleIDSuffix { get; }
		internal abstract MultilingualString DisplayText { get; }
		internal abstract string FallbackHint { get; }
	}
}
