namespace Enterprise.DbUpgrader.Data
{
	public class ExceptionTypesUpgradeTask : EmbeddedUpgradeTask
	{
		public ExceptionTypesUpgradeTask()
			: base(new ExceptionTypesDataFile())
		{
		}
	}
}
