using System.Data;
using System.Reflection;

namespace Enterprise.DbUpgrader.Data
{
	/// <summary>
	/// Synchronises between the tables in the database and an embedded resource file
	/// </summary>
	public class EmbeddedDataFile : DataFile
	{
		/// <summary>
		/// Creates a DataFile to synchronise the data between the FileFullPath and a list of tables in database.
		/// </summary>
		/// <param name="FileFullPath">The source file that's used to store the contents of the data tables</param>
		/// <param name="TableNames">List of table names in database to synchronise</param>
		public EmbeddedDataFile(string fileRelativePath, params string[] tableNames) : base(fileRelativePath, tableNames)
		{
		}

		public int Version
		{
			get { return GetVersion(DataSet); }
		}

		protected override DataSet LoadDataSet()
		{
			return ReadXml(ResourceAssembly.GetManifestResourceStream(FileResourceName), FileResourceName);
		}

		protected virtual Assembly ResourceAssembly
		{
			get { return GetType().Assembly; }
		}
	}
}
