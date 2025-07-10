using Enterprise.ZArchitecture.Modules;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Business
{
	public class CodeDescriptionModuleFilterPair
	{
		public CodeDescriptionModuleFilterPair(string code, string description, ModuleIdentifier module)
		{
			Code = code;
			Description = description;
			Module = module;
		}

		public readonly ModuleIdentifier Module;
		public readonly string Code;
		public readonly string Description;

		[ThreadSafe]
		static CodeDescriptionModuleFilterPair defaultCodeDescriptionModule;
		public static CodeDescriptionModuleFilterPair DefaultCodeDescriptionModule => defaultCodeDescriptionModule ?? (defaultCodeDescriptionModule = new CodeDescriptionModuleFilterPair(string.Empty, string.Empty, ModuleIDs.NotAssigned));
	}
}
