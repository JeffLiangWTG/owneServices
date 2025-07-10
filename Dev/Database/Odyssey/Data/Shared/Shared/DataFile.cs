using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.DbUpgrader.Data
{
	/// <summary>
	/// Synchronises between the tables in the database and an embedded resource file
	/// </summary>
	public abstract class DataFile
	{
		static DataFile()
		{
			var extraAllowedTypes = new[]
			{
				typeof(Microsoft.SqlServer.Types.SqlGeography),
			};
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			AppDomain.CurrentDomain.SetData("System.Data.DataSetDefaultAllowedTypes", extraAllowedTypes);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		/// <summary>
		/// Creates a DataFile to synchronise the data between the FileFullPath and a list of tables in database.
		/// </summary>
		/// <param name="FileFullPath">The source file that's used to store the contents of the data tables</param>
		/// <param name="TableNames">List of table names in database to synchronise</param>
		protected DataFile(string fileRelativePath, params string[] tableNames)
		{
			this.fileRelativePath = fileRelativePath;
			this.fTableNames = tableNames;
			CreateVersionRegistryItem();
		}

		public string[] TableNames
		{
			get { return fTableNames; }
		}

#if DEBUG
		virtual
#endif
		public string FileResourceName => ResourceNameHeader + ResourceRelativeName;

		virtual public string ResourceRelativeName => fileRelativePath.Replace('\\', '.');

		public string FileRelativePath
		{
			get { return fileRelativePath; }
		}

		public virtual int VersionInDatabase
		{
			get => VersionRegistryItem.LoadValue(Db.Connection);
			set => VersionRegistryItem.SaveValue(value, Db.Connection);
		}

		public int GetVersion(DataSet data)
		{
			return GetVersionCore(data);
		}

		protected virtual int GetVersionCore(DataSet data)
		{
			var version = 0;

			if (data != null)
			{
				try
				{
					version = Convert.ToInt32(data.DataSetName);
				}
				catch (FormatException)
				{
					version = 0;
				}
			}

			return version;
		}

		public DataSet LoadDataFromDatabase()
		{
			var dataSet = new DataSet();
			DataRetriever.LoadDataFromDatabase(dataSet, SelectQuery, TableNames);
			return dataSet;
		}

#if DEBUG

		public virtual string FileFullPath
		{
			get { return Path.Combine(DefaultDataFileBasePath, fileRelativePath); }
		}

		const string dataUpgraderBasePath = @"Database\Odyssey\Data\";

		public virtual string DefaultDataFileBasePath
		{
			get { return Path.Combine(DefaultBaseSourcePath, dataUpgraderBasePath); }
		}

		static string defaultBaseSourcePath;

		static string DefaultBaseSourcePath
		{
			get
			{
				if (defaultBaseSourcePath == null)
				{
					var assembly = Assembly.Load("BuildTools");
					var type = assembly.GetType("CargoWise.BuildTools.BuildConstants");
					var propertyInfo = type.GetProperty("LocalEnterprisePath", BindingFlags.Static | BindingFlags.Public);
					defaultBaseSourcePath = (string)propertyInfo.GetValue(null, null);
				}
				return defaultBaseSourcePath;
			}
		}

		public DataSet LoadDataFromFile()
		{
			return LoadDataFromFile(FileFullPath);
		}

		public virtual bool MustCleanDataBeforeSetup
		{
			get { return false; }
		}

#endif

		protected DataSet LoadDataFromFile(string filePath)
		{
			int keepTryingCount = 2;
			DataSet result = null;
			while (keepTryingCount > 0)
			{
				try
				{
					result = LoadFromXml(filePath);
					keepTryingCount = 0;
				}
				catch (IOException)
				{
					System.Threading.Thread.Sleep(1000);
					keepTryingCount--;
					if (keepTryingCount == 0)
					{
						throw;
					}
				}
			}
			return result;
		}

		protected virtual DataSet LoadFromXml(string filePath)
		{
			using (Stream dataSetStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return ReadXml(dataSetStream, filePath);
			}
		}

		protected DataSet ReadXml(Stream dataSetStream, string streamName)
		{
			DataSet result = new DataSet();
			try
			{
				if (dataSetStream != null && streamName.ToLower().EndsWith(".gz"))
				{
					dataSetStream = new GZipStream(dataSetStream, CompressionMode.Decompress);
				}
				if (dataSetStream == null)
				{
					throw new ApplicationException("Failed to extract DataSet from stream: " + streamName);
				}

				result.ReadXml(dataSetStream);
			}
			finally
			{
				if (dataSetStream != null)
				{
					dataSetStream.Close();
				}
			}

			return result;
		}

		public virtual void WriteXml(DataSet dataSet, string fileName, XmlWriteMode mode)
		{
			AdjustDateTimeColumnsMode(dataSet);
			Stream dataSetStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
			try
			{
				if (dataSetStream != null && fileName.ToLower().EndsWith(".gz"))
				{
					dataSetStream = new GZipStream(dataSetStream, CompressionMode.Compress);
				}

				dataSet.WriteXml(dataSetStream, mode);
			}
			finally
			{
				if (dataSetStream != null)
				{
					dataSetStream.Close();
				}
			}
		}

		void AdjustDateTimeColumnsMode(DataSet data)
		{
			foreach (DataTable table in data.Tables)
			{
				foreach (DataColumn column in table.Columns)
				{
					if (column.DataType == typeof(DateTime))
					{
						column.DateTimeMode = DataSetDateTime.Unspecified;
					}
				}
			}
		}

		public DataSet DataSet
		{
			get
			{
				if (fDataSet == null)
				{
					fDataSet = LoadDataSet();
				}
				return fDataSet;
			}
		}

		protected abstract DataSet LoadDataSet();

		/// <summary>
		/// Posts changes in a given DataSet to the database.
		/// Note1:
		///   TransactionalDataAdapter.Update throws an exception if call not in a transaction.
		///   Therefore this method MUST be called within a transaction.
		/// Note2:
		///   No need to put EnableConstraint in a try/finally pattern as transaction will be rollback in case of an exception.
		/// </summary>
		internal void SaveDataToDatabase(DataSet data)
		{
			DisableConstraintsAndDropUniqueIndexesIfNecessary(data.Tables);
			ApplyDatasetChangesToDatabase(data);
			EnableConstraintsAndRecreateUniqueIndexes(data.Tables);
		}

		#region Implementation

		protected virtual void CreateVersionRegistryItem()
		{
			VersionRegistryItem = new DatabaseVersionRegistryItem("XsdVersion-" + FileResourceName);
		}

		void DisableConstraintsAndDropUniqueIndexesIfNecessary(DataTableCollection tables)
		{
			if (this is IFixReferencesAndDuplicates)
			{
				DisableConstraints(tables);
				DropUniqueIndexes();
			}
		}

		void ApplyDatasetChangesToDatabase(DataSet data)
		{
			foreach (DataTable table in data.Tables)
			{
				if (!string.IsNullOrEmpty(TableNamePrefix) && !table.TableName.StartsWith(TableNamePrefix))
				{
					table.TableName = TableNamePrefix + table.TableName;
				}
			}

			var serviceProvider = GlobalServiceProvider.Instance;
			var saverFactory = serviceProvider.GetRequiredService<IZSqlSaverFactory>();
			var schemaResolver = serviceProvider.GetRequiredService<IApplicationSchemaResolver>();
			var saver = saverFactory.GetSqlSaver(data, Db.Connection, schemaResolver);
			saver.Save(false);

			if (!string.IsNullOrEmpty(TableNamePrefix))
			{
				foreach (DataTable table in data.Tables)
				{
					table.TableName = table.TableName.Replace(TableNamePrefix, "");
				}
			}
		}

		void EnableConstraintsAndRecreateUniqueIndexes(DataTableCollection tables)
		{
			if (this is IFixReferencesAndDuplicates)
			{
				(this as IFixReferencesAndDuplicates).PerformExtraDataManipulationBeforeEnablingConstraints();
				RecreateUniqueIndexes();
				EnableConstraints(tables);
			}
		}

		protected void DisableConstraints(DataTableCollection tables)
		{
			foreach (DataTable table in tables)
			{
				DisableTableConstraints(table.TableName);
			}
		}

		void DropUniqueIndexes()
		{
			foreach (UniqueIndexInfo index in UniqueIndexesToDropBeforeSaveAndRecreateAfterwards)
			{
				Db.Connection.ExecuteNonQuery(index.DropStatement);
			}
		}

		void RecreateUniqueIndexes()
		{
			foreach (UniqueIndexInfo index in UniqueIndexesToDropBeforeSaveAndRecreateAfterwards)
			{
				Db.Connection.ExecuteNonQuery(index.CreateStatement);
			}
		}

		/// <summary>
		/// List of unique indexes to drop before save and recreate after some data manipulation (if any)
		/// </summary>
		List<UniqueIndexInfo> UniqueIndexesToDropBeforeSaveAndRecreateAfterwards
		{
			get
			{
				if (uniqueIndexesToDropBeforeSaveAndRecreateAfterwards == null)
				{
					uniqueIndexesToDropBeforeSaveAndRecreateAfterwards = GetUniqueIndexesToDropBeforeSaveAndRecreateAfterwards();
				}

				return uniqueIndexesToDropBeforeSaveAndRecreateAfterwards;
			}
		}

		/// <summary>
		/// Initialisation method for UniqueIndexesToDropBeforeSaveAndRecreateAfterwards
		/// (List of unique indexes to drop before save and recreate after some data manipulation)
		/// </summary>
		protected virtual List<UniqueIndexInfo> GetUniqueIndexesToDropBeforeSaveAndRecreateAfterwards()
		{
			return new List<UniqueIndexInfo>();
		}

		List<UniqueIndexInfo> uniqueIndexesToDropBeforeSaveAndRecreateAfterwards;

		protected void EnableConstraints(DataTableCollection tables)
		{
			foreach (DataTable table in tables)
			{
				EnableTableConstraints(table.TableName);
			}
		}

		protected void DisableTableConstraints(string tableName)
		{
			string sqlQuery = "alter table " + TableNamePrefix + tableName + " nocheck constraint all";
			Db.Connection.ExecuteNonQuery(sqlQuery);
		}

		protected void EnableTableConstraints(string tableName)
		{
			string sqlQuery = "alter table " + TableNamePrefix + tableName + " with check check constraint all";
			Db.Connection.ExecuteNonQuery(sqlQuery);
		}

		protected virtual string TableNamePrefix
		{
			get { return ""; }
		}

		protected virtual string ResourceNameHeader
		{
			get { return "Enterprise.DbUpgrader.Data."; }
		}

		protected virtual string SelectQuery
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				foreach (string tableName in TableNames)
				{
					if (builder.Length > 0)
					{
						builder.Append(';');
					}

					BuildQueryForTable(builder, tableName);
				}

				return builder.ToString();
			}
		}

		protected virtual void BuildQueryForTable(StringBuilder builder, string tableName)
		{
			builder.Append("SELECT * FROM ");
			builder.Append(tableName);
			builder.Append(" ORDER BY 1");
		}

		DataSet fDataSet;
		protected string[] fTableNames;
		public DatabaseVersionRegistryItem VersionRegistryItem { get; protected set; }
		readonly string fileRelativePath;

		#endregion
	}
}
