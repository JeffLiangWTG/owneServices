namespace Enterprise.DbUpgrader.Data.BaseData.Common
{
	public abstract class BaseDataFile : EmbeddedDataFile
	{
		public BaseDataFile(string fileRelativePath, params string[] tableNames)
			: base(fileRelativePath, tableNames)
		{
		}

		#if DEBUG

		public override bool MustCleanDataBeforeSetup
		{
			get { return true; }
		}

		#endif
	}
}
