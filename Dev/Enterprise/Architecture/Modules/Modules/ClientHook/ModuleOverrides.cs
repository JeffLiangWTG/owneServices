namespace Enterprise.ZArchitecture.Modules
{
	public class ModuleOverrides : RegistrationList<ModuleIdentifier, ModuleInfo>
	{
		public void AddModuleOverride(ClientOverrideModuleInfo moduleInfo)
		{
			Add(moduleInfo);
		}
	}
}
