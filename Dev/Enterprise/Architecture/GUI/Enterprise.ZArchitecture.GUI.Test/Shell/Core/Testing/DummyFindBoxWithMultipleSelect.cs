using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyFindBoxWithMultipleSelect : DummyFindBox, IFindBoxWithMultipleSelect
	{
		public DummyFindBoxWithMultipleSelect()
			: this("DummyWithMultipleSelect", "It's DummyWithMultipleSelect", true)
		{ }

		public DummyFindBoxWithMultipleSelect(string code, string description, bool allowModuleMultiSelect)
		{
			this.Code = Code;
			this.Description = description;
			this.AllowModuleMultiSelect = allowModuleMultiSelect;
		}

		public bool AllowModuleMultiSelect { get; set; }
	}
}
