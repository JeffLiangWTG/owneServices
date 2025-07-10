namespace Enterprise.DbUpgrader.Data.BaseData.Common
{
	public abstract class SystemInstallDataUpgradeTask : BaseDataUpgradeTask
	{
		public SystemInstallDataUpgradeTask(BaseDataFile resourceFile)
			: base(resourceFile)
		{
		}

		public sealed override bool IsRequired
		{
			get
			{
				return (base.IsRequired && ResourceFile.VersionInDatabase == 0);
			}
		}
	}
}
