using System;
using System.Collections;
using System.Data;
using Enterprise.BusinessObjectGenerator;

namespace Enterprise.Builder.Generator.Testing
{
	sealed class TestSingleBizObjGenerator : SingleBizObjGenerator
	{
		public TestSingleBizObjGenerator(string fileNameOfBusinessObject, GeneratorOutputDirectory outputDirectory)
			: base(fileNameOfBusinessObject, outputDirectory)
		{
		}

		public TestSingleBizObjGenerator(string fileNameOfBusinessObject, GeneratorOutputDirectory outputDirectory, bool masterFilesRef)
			: base(fileNameOfBusinessObject, outputDirectory, masterFilesRef)
		{
		}

		public bool ThrowDuringGenerateAll;
		public IList TablesNamesInDbDuringGeneration;
		public IList IndexesInDbDuringGeneration;
		public string tableNameToGetIndexesFor;

		internal override void GenerateAll(GeneratorOutputDirectory outputDirectory)
		{
			TablesNamesInDbDuringGeneration = SingleBizObjGeneratorTest.GetTableNamesFromDb();
			if (!string.IsNullOrEmpty(tableNameToGetIndexesFor))
			{
				IndexesInDbDuringGeneration = GetUniqueIndexesForTable_ForTest(tableNameToGetIndexesFor);
			}
			if (ThrowDuringGenerateAll)
			{
				throw new InvalidOperationException("C U L 8 R");
			}
		}

		public DataTable TableFromDatabaseSchema_Exposed()
		{
			return TableFromDatabaseSchema();
		}

		public BusinessObjectInfo Info_Exposed
		{
			get
			{
				return base.Info;
			}
		}

		protected override string ActualDatabaseNameDuringRegen
		{
			get
			{
				if (string.IsNullOrWhiteSpace(ActualDatabaseNameDuringRegen_ForTest))
				{
					return base.ActualDatabaseNameDuringRegen;
				}

				return ActualDatabaseNameDuringRegen_ForTest;
			}
		}

		public string ActualDatabaseNameDuringRegen_Exposed
		{
			get { return ActualDatabaseNameDuringRegen; }
		}

		public string ActualDatabaseNameDuringRegen_ForTest { get; set; }
	}
}
