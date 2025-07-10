using System;
using System.Data;
using Enterprise.DbUpgrader.Data;

namespace Enterprise.Builder.DataUpgradeSetup.Testing
{
	public class DataTaskSetupForTest : DataTaskSetup
	{
		public DataTaskSetupForTest(UpgradeTask task, DataUpgradeSetupController controller)
					: base(task, controller)
		{
		}

		public DataTaskSetupForTest(DataUpgradeSetupController controller)
				: this(new UpgradeTaskForTest(), controller)
		{
		}

		public int ExposedVersionInXmlSourceFile
		{
			get { return Convert.ToInt32(VersionInXmlSourceFile); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public DataSet ExposedLoadDataFromSourceFile()
		{
			return LoadDataFromSourceFile();
		}

		public DataFile TestDataFile
		{
			get { return fTask.ResourceFile; }
		}

		public UpgradeTask TestUpgradeTask
		{
			get { return fTask; }
		}
	}
}
