using System.Data;
using System.IO;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class ConcreteDataFile : DataFile
	{
		public ConcreteDataFile(string fileRelativePath, params string[] tableNames)
			: base(fileRelativePath, tableNames)
		{
		}

		public override string DefaultDataFileBasePath => TestFileConstants.DefaultTestDataFileBasePath;

		internal int Count;

		protected override DataSet LoadFromXml(string filePath)
		{
			if (Count++ == 0)
			{
				throw new IOException("Stage1!");
			}
			return base.LoadFromXml(filePath);
		}

		protected override DataSet LoadDataSet() => null;
	}
}
