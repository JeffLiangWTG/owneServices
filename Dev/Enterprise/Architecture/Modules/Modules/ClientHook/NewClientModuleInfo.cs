namespace Enterprise.ZArchitecture.Modules
{
	public class NewClientModuleInfo
	{
		public NewClientModuleInfo(ModuleInfo newModuleInfo) : this("", "", newModuleInfo)
		{
		}

		public NewClientModuleInfo(ModuleTreeLoaderConstant.Entry category, ModuleTreeLoaderConstant.Entry section, ModuleInfo newModuleInfo)
			: this(category.Name, section.Name, newModuleInfo)
		{ }

		public NewClientModuleInfo(ModuleTreeLoaderConstant.Entry category, string sectionName, ModuleInfo newModuleInfo)
			: this(category.Name, sectionName, newModuleInfo)
		{ }

		public NewClientModuleInfo(string categoryName, string sectionName, ModuleInfo newModuleInfo)
		{
			this.CategoryName = categoryName;
			this.SectionName = sectionName;
			Info = newModuleInfo;
		}

		public ModuleIdentifier ID
		{
			get { return Info.ID; }
		}

		public readonly string CategoryName;
		public readonly string SectionName;
		public readonly ModuleInfo Info;
	}
}
