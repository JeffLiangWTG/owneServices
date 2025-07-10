// This data is stored in data sets not the db
// This reads a number text files and put data into db

using System.Collections;
using System.Data;
using System.IO;

using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class ReferenceFileBuilder
	{
		public ReferenceFileBuilder()
		{
		}

		#region StructureDataSet

#pragma warning disable CW1108 // Do Not Use DataSet
		public DataSet StructureDataSet // loading data from stream, not database
		{
			get
			{
				if (fStructureDataSet == null)
				{
					fStructureDataSet = new DataSet(); // loading data from stream, not database
					fStructureDataSet.ReadXml(GetTableStructure());
				}

				return fStructureDataSet;
			}
		}
		DataSet fStructureDataSet; // loading data from stream, not database
#pragma warning restore CW1108 // Do Not Use DataSet

		public abstract Stream GetTableStructure();
		#endregion

		#region Mappings Data Set

#pragma warning disable CW1108 // Do Not Use DataSet
		public DataSet MappingsDataSet // loading data from stream, not database
		{
			get
			{
				if (fMappingsDataSet == null)
				{
					fMappingsDataSet = new DataSet(); // loading data from stream, not database
					fMappingsDataSet.ReadXml(GetTableMappings());
				}

				return fMappingsDataSet;
			}
		}
		DataSet fMappingsDataSet; // loading data from stream, not database
#pragma warning restore CW1108 // Do Not Use DataSet

		public abstract Stream GetTableMappings();

		#endregion

		#region File Supporter

		public FileSupporter FileSupporter
		{
			get
			{
				if (fFileSupporter == null)
				{
					fFileSupporter = new FileSupporter();
				}

				return fFileSupporter;
			}
		}
		FileSupporter fFileSupporter;

		#endregion

		#region Mapping Data Row

		public DataRow MappingDataRow
		{
			get { return fMappingDataRow; }
			set { fMappingDataRow = value; }
		}
		DataRow fMappingDataRow;

		#endregion

		#region Data Item

		public ZString GetDataItemColumnValue(DataRow row)
		{
			return row[CMRReferenceFileBuilderConstants.TablesStructure.DataItemColumn].ToString();
		}

		#endregion

		#region Format

		public ZString GetFormatColumnValue(DataRow row)
		{
			return row[CMRReferenceFileBuilderConstants.TablesStructure.FormatColumn].ToString();
		}

		#endregion

		#region Start Position

		public ZString GetStartPositionColumnValue(DataRow row)
		{
			return row[CMRReferenceFileBuilderConstants.TablesStructure.StartPositionColumn].ToString();
		}

		#endregion

		#region Length

		public ZString GetLengthColumnValue(DataRow row)
		{
			return row[CMRReferenceFileBuilderConstants.TablesStructure.LengthColumn].ToString();
		}

		#endregion

		#region Unique Index

		public ZString GetUniqueIndexColumnValue(DataRow row)
		{
			return row[CMRReferenceFileBuilderConstants.TablesStructure.UniqueIndexColumn].ToString();
		}

		#endregion

		#region Table Prefix

		public ZString TablePrefix
		{
			get { return MappingDataRow[CMRReferenceFileBuilderConstants.TablesMappings.ReferenceFilePrefixColumn].ToString() + "_"; }
		}

		#endregion

		#region Table Name

		public ZString TableName
		{
			get
			{
				return RefDbTableNameResolver.GetRefDbTableSynonym(RefDbTypeEnum.Customs, "AU", MappingDataRow[CMRReferenceFileBuilderConstants.TablesMappings.ReferenceFileTableNameColumn].ToString());
			}
		}

		#endregion

		#region PK Column Name

		public ZString PKColumnName
		{
			get { return TablePrefix + "PK"; }
		}

		#endregion

		#region Mappings Table

		public DataTable MappingsTable
		{
			get
			{
				if (fMappingsTable == null && MappingsDataSet != null)
				{
					fMappingsTable = MappingsDataSet.Tables[CMRReferenceFileBuilderConstants.TablesMappings.TableName];
				}
				return fMappingsTable;
			}
		}
		DataTable fMappingsTable;

		#endregion

		#region Filter For Structure Data Set

		public DataRow[] GetFilteredAndSortedDataSet()
		{
			DataTable structureDataTable = StructureDataSet.Tables[CMRReferenceFileBuilderConstants.TablesStructure.TableName];

			ZString filter = structureDataTable.Columns[CMRReferenceFileBuilderConstants.TablesStructure.CustomsTableNameColumn]
				+ " = '" + MappingDataRow[CMRReferenceFileBuilderConstants.TablesMappings.CustomsTableNameColumn] + "'";

			DataRow[] filteredRows = structureDataTable.Select(filter);

			ArrayList listOfRows = new ArrayList(filteredRows);
			listOfRows.Sort(new StartPositionComparer());

			for (int i = 0; i < listOfRows.Count; i++)
			{
				filteredRows[i] = (DataRow)listOfRows[i];
			}

			return filteredRows;
		}

		#region Comparer

		internal class StartPositionComparer : IComparer
		{
			#region IComparer Members

			public int Compare(object x, object y)
			{
				DataRow row1 = (DataRow)x;
				DataRow row2 = (DataRow)y;

				int value1 = int.Parse(row1[CMRReferenceFileBuilderConstants.TablesStructure.StartPositionColumn].ToString());
				int value2 = int.Parse(row2[CMRReferenceFileBuilderConstants.TablesStructure.StartPositionColumn].ToString());

				return value1.CompareTo(value2);
			}

			#endregion

		}

		#endregion

		#endregion

		#region Precsion and Scale

		public ZInt GetLengthWtihPrecisionAndScale(ZString precisionAndScale)
		{
			ZString[] precisionAndScaleSplit = precisionAndScale.Split('.');
			return int.Parse(precisionAndScaleSplit[0].ToString().TrimEnd('.')) + int.Parse(precisionAndScaleSplit[1]);
		}

		#endregion

		#region Factory

		protected void SaveAndCreateNewFactory()
		{
			Factory.Save();
			fFactory = null;
		}

		protected BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}
				return fFactory;
			}
		}
		BusinessObjectFactory fFactory;

		#endregion
	}
}
