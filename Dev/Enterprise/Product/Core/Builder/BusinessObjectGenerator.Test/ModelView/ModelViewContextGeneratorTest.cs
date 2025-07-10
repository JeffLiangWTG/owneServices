using System.Collections.Generic;
using System.IO;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.BusinessObjectGenerator.ModelView.Testing
{
	sealed class ModelViewContextGeneratorTest : TestCase
	{
		public void TestGetCodeGeneratorContext()
		{
			var filePath = embeddedResourceRetriever.SaveResourceToFile(TestResources.DummyBizO.Xml, $"{TestResources.DummyBizO.Name}.model.xml", Path.Combine(ModelViewConstants.ScriptsDefinitionsNamespace, "CountryFolder"));
			_ = embeddedResourceRetriever.SaveResourceToFile(TestResources.DummyBizO.Xml, $"{TestResources.DummyBizO.Name}.model.notxml");
			var codeGeneratorContext = contextGenerator.GetCodeGeneratorContext(filePath);

			AssertNotNull(codeGeneratorContext);
			AssertEquals(TestResources.DummyBizO.Name, codeGeneratorContext.ModelName);
			AssertEquals(Path.Combine(embeddedResourceRetriever.DirectoryPath, ModelViewConstants.ScriptsDefinitionsNamespace, "CountryFolder"), codeGeneratorContext.ParentDirectory);
			AssertEquals("DevelopmentOnly", codeGeneratorContext.DevelopmentAttributeName);
			AssertEquals($"CargoWise.DbUpgrader.Scripts.Definitions.CountryFolder", codeGeneratorContext.DefinitionNamespace);
			AssertEquals("Z0_AddInfo", codeGeneratorContext.AddInfoColumnName);
			AssertNull(codeGeneratorContext.ClusterKeyColumnName);
			AssertEquals("Z0_PK", codeGeneratorContext.PKColumnName);

			var modelView = codeGeneratorContext.ModelView;
			AssertNotNull(modelView);
			AssertEquals("DummyBizo", modelView.Table);
			AssertEquals("[Z0_Code]='ZZ'", modelView.SearchCondition);

			var columns = codeGeneratorContext.Columns;
			AssertNotNull(columns);
			AssertEquals(1, columns.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Z0_PK" }, columns);

			var addInfos = modelView.AddInfos;
			AssertNotNull(addInfos);
			AssertEquals(37, addInfos.Count);
			var expectedAddInfos = GetExpectedAddInfos();
			AssertContainsExactElementsInAnyOrder(new AddInfoComparerForTest(), expectedAddInfos, addInfos);
		}

		public void TestGetAllCodeGeneratorContexts()
		{
			_ = embeddedResourceRetriever.SaveResourceToFile(TestResources.DummyBizO.Xml, $"{TestResources.DummyBizO.Name}.model.xml", Path.Combine(ModelViewConstants.ScriptsDefinitionsNamespace, "CountryFolder"));
			_ = embeddedResourceRetriever.SaveResourceToFile(TestResources.DummyBizO.Xml, $"{TestResources.DummyBizO.Name}2.model.xml", Path.Combine(ModelViewConstants.ScriptsDefinitionsNamespace, "CountryFolder"));
			_ = embeddedResourceRetriever.SaveResourceToFile(TestResources.DummyBizO.Xml, $"{TestResources.DummyBizO.Name}.model.notxml");
			var allContexts = contextGenerator.GetAllCodeGeneratorContexts(embeddedResourceRetriever.DirectoryPath);

			AssertEquals(2, allContexts.Count);
			AssertEquals(1, allContexts.FindAll(c => c.ModelName == TestResources.DummyBizO.Name).Count);
			AssertEquals(1, allContexts.FindAll(c => c.ModelName == $"{TestResources.DummyBizO.Name}2").Count);
		}

		public void TestIsAddInfo()
		{
			CombineAssertions(() =>
			{
				Assert(!contextGenerator.IsAddInfo("Test"));
				Assert(contextGenerator.IsAddInfo("Test_AddInfo"));
				Assert(!contextGenerator.IsAddInfo("Test_ClusterKey"));
				Assert(!contextGenerator.IsAddInfo("Test_PK"));
			});
		}

		public void TestIsClusterKey()
		{
			Assert(!contextGenerator.IsClusterKey("Test"));
			Assert(!contextGenerator.IsClusterKey("Test_AddInfo"));
			Assert(contextGenerator.IsClusterKey("Test_ClusterKey"));
			Assert(!contextGenerator.IsClusterKey("Test_PK"));
		}

		public void TestIsPrimaryKey()
		{
			Assert(!contextGenerator.IsPrimaryKey("Test"));
			Assert(!contextGenerator.IsPrimaryKey("Test_AddInfo"));
			Assert(!contextGenerator.IsPrimaryKey("Test_ClusterKey"));
			Assert(contextGenerator.IsPrimaryKey("Test_PK"));
		}

		public void TestGetColumns()
		{
			_ = embeddedResourceRetriever.SaveResourceToFile(TestResources.SchemaMain.Resource, TestResources.SchemaMain.FileName, TestResources.SchemaMain.DestinationSubFolder);

			var expectedColumns = new List<string>
			{
				"D5_PK",
				"D5_E0",
				"D5_Type",
				"D5_Value",
				"D5_SystemCreateTimeUtc",
				"D5_SystemCreateUser",
				"D5_SystemLastEditTimeUtc",
				"D5_SystemLastEditUser",
			};

			var columns = contextGenerator.GetColumns("JobSlotAllocationAspect");

			AssertNotNull(columns);
			AssertEquals(8, columns.Count);
			AssertContainsExactElementsInAnyOrder(expectedColumns, columns);
		}

		public void TestGetColumns_DummyBizOSpecialCase()
		{
			var expectedColumns = new List<string>
			{
				"Z0_PK",
			};

			var columns = contextGenerator.GetColumns("DummyBizo");

			AssertNotNull(columns);
			AssertEquals(1, columns.Count);
			AssertContainsExactElementsInAnyOrder(expectedColumns, columns);
		}

		List<AddInfo> GetExpectedAddInfos()
		{
			return new List<AddInfo>
			{
				new AddInfo
				{
					Name = "Z0_AddInfoString35",
					DataType = "String",
					Precision = null,
					Scale = null,
					MaxLength = 35,
					Indexed = false,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_AddInfoString3",
					DataType = "String",
					Precision = null,
					Scale = null,
					MaxLength = 3,
					Indexed = false,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_AddInfoDecimal122",
					DataType = "Decimal",
					Precision = 12,
					Scale = 2,
					MaxLength = null,
					Indexed = false,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_AddInfoDecimal073",
					DataType = "Decimal",
					Precision = 7,
					Scale = 3,
					MaxLength = null,
					Indexed = false,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_AddInfoInt16",
					DataType = "Int16",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = false,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_AddInfoInt32",
					DataType = "Int32",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = false,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_AddInfoInt32NotNullable",
					DataType = "Int32",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = false,
					IsUnicode = false,
					IsNullable = false
				},
				new AddInfo
				{
					Name = "Z0_AddInfoBool",
					DataType = "Boolean",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = false,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_AddInfoGuid",
					DataType = "Guid",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = false,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_AddInfoDateTime",
					DataType = "DateTime",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = false,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_AddInfoDate",
					DataType = "Date",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = false,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_NAddInfoString35",
					DataType = "String",
					Precision = null,
					Scale = null,
					MaxLength = 35,
					Indexed = false,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_NAddInfoString3",
					DataType = "String",
					Precision = null,
					Scale = null,
					MaxLength = 3,
					Indexed = false,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_NAddInfoDecimal122",
					DataType = "Decimal",
					Precision = 12,
					Scale = 2,
					MaxLength = null,
					Indexed = false,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_NAddInfoDecimal073",
					DataType = "Decimal",
					Precision = 7,
					Scale = 3,
					MaxLength = null,
					Indexed = false,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_NAddInfoInt16",
					DataType = "Int16",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = false,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_NAddInfoInt32",
					DataType = "Int32",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = false,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_NAddInfoBool",
					DataType = "Boolean",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = false,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_NAddInfoGuid",
					DataType = "Guid",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = false,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_NAddInfoDateTime",
					DataType = "DateTime",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = false,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_NAddInfoDate",
					DataType = "Date",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = false,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_IAddInfoString35",
					DataType = "String",
					Precision = null,
					Scale = null,
					MaxLength = 35,
					Indexed = true,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_IAddInfoDecimal122",
					DataType = "Decimal",
					Precision = 12,
					Scale = 2,
					MaxLength = null,
					Indexed = true,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_IAddInfoInt16",
					DataType = "Int16",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = true,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_IAddInfoInt32",
					DataType = "Int32",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = true,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_IAddInfoBool",
					DataType = "Boolean",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = true,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_IAddInfoGuid",
					DataType = "Guid",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = true,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_IAddInfoDateTime",
					DataType = "DateTime",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = true,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_IAddInfoDate",
					DataType = "Date",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = true,
					IsUnicode = false
				},
				new AddInfo
				{
					Name = "Z0_INAddInfoString35",
					DataType = "String",
					Precision = null,
					Scale = null,
					MaxLength = 35,
					Indexed = true,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_INAddInfoDecimal122",
					DataType = "Decimal",
					Precision = 12,
					Scale = 2,
					MaxLength = null,
					Indexed = true,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_INAddInfoInt16",
					DataType = "Int16",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = true,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_INAddInfoInt32",
					DataType = "Int32",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = true,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_INAddInfoBool",
					DataType = "Boolean",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = true,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_INAddInfoGuid",
					DataType = "Guid",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = true,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_INAddInfoDateTime",
					DataType = "DateTime",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = true,
					IsUnicode = true
				},
				new AddInfo
				{
					Name = "Z0_INAddInfoDate",
					DataType = "Date",
					Precision = null,
					Scale = null,
					MaxLength = null,
					Indexed = true,
					IsUnicode = true
				},
			};
		}

		protected override void SetUp()
		{
			base.SetUp();

			embeddedResourceRetriever = new EmbeddedResourceRetriever();
			contextGenerator = new ModelViewContextGenerator(embeddedResourceRetriever.DirectoryPath);
		}

		protected override void TearDown()
		{
			embeddedResourceRetriever?.Dispose();
			embeddedResourceRetriever = null;

			base.TearDown();
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;
		ModelViewContextGenerator contextGenerator;

		class AddInfoComparerForTest : IEqualityComparer<AddInfo>
		{
			public bool Equals(AddInfo x, AddInfo y)
			{
				if (ReferenceEquals(x, y))
				{
					return true;
				}

				if (ReferenceEquals(x, null))
				{
					return false;
				}

				if (ReferenceEquals(y, null))
				{
					return false;
				}

				if (x.GetType() != y.GetType())
				{
					return false;
				}

				return x.Name == y.Name &&
					   x.DataType == y.DataType &&
					   x.Precision == y.Precision &&
					   x.Scale == y.Scale &&
					   x.MaxLength == y.MaxLength &&
					   x.Indexed == y.Indexed &&
					   x.IsUnicode == y.IsUnicode;
			}

			public int GetHashCode(AddInfo obj)
			{
				unchecked
				{
					var hashCode = (obj.Name != null ? obj.Name.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.DataType != null ? obj.DataType.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ obj.Precision.GetHashCode();
					hashCode = (hashCode * 397) ^ obj.Scale.GetHashCode();
					hashCode = (hashCode * 397) ^ obj.MaxLength.GetHashCode();
					hashCode = (hashCode * 397) ^ obj.Indexed.GetHashCode();
					hashCode = (hashCode * 397) ^ obj.IsUnicode.GetHashCode();
					return hashCode;
				}
			}
		}
	}
}
