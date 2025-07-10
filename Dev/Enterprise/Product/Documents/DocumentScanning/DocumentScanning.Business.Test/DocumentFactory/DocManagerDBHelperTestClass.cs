using System.Collections.Generic;

namespace Enterprise.DocumentScanning.Business.Testing
{
	public class DocManagerDBHelperTestClass : DocManagerDBHelper
	{
		public DocManagerDBHelperTestClass()
		{
			SkipRefreshDbReaderRolePermissionsForTest = true;
		}

		public new void DropDatabase(string dBName)
		{
			base.DropDatabase(dBName);
		}

		public new int CreateDatabase(int newDBNumber)
		{
			return base.CreateDatabase(newDBNumber);
		}

		public new IEnumerable<int> GetStorageDocDbNumbersIncludingMainDb()
		{
			return base.GetStorageDocDbNumbersIncludingMainDb();
		}

		protected override int DatabaseFileSizeThresholdInMb
		{
			get
			{
				return TrimThresholdtoZero ? 0 : base.DatabaseFileSizeThresholdInMb;
			}
		}
		public void RefreshDbReaderRolePermissionsOnCreationOfDatabase() => SkipRefreshDbReaderRolePermissionsForTest = false;

		public bool TrimThresholdtoZero { get; set; }

		protected override int? DbInitialSizeMb
		{
			get { return null; }
		}
	}
}
