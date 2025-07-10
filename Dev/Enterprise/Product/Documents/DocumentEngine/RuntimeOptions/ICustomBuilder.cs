namespace Enterprise.DocumentEngine.RuntimeOptions
{
	interface ICustomBuilder
	{
		string TemplateFileName { get; set; }
		void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField);
		void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField);
	}
}
