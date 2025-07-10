using CargoWise.Common;

namespace CargoWise.BuildTools
{
	public class BuildXmlAddInfoEntry
	{
		public BuildXmlAddInfoEntry(string viewName, string solutionName, string oldPrefix, string parentSchema, string childTableName, string childForeignKey)
		{
			Argument.NotNull(viewName, nameof(viewName));
			Argument.NotNull(solutionName, nameof(solutionName));
			ViewName = viewName;
			SolutionName = solutionName;
			OldPrefix = oldPrefix;
			ParentSchema = parentSchema;
			ChildTableName = childTableName;
			ChildForeignKey = childForeignKey;
		}

		public string ViewName { get; private set; }
		public string SolutionName { get; private set; }
		public string OldPrefix { get; private set; }
		public string ParentSchema { get; private set; }
		public string ChildTableName { get; private set; }
		public string ChildForeignKey { get; private set; }
	}
}
