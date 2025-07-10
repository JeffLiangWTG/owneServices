using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ClusterKeyMappingsProviderTest : TestCase
	{
		public void TestGetAllClusterKeyMappings()
		{
			var clusterKeyMetaDataAttributes = new[]
			{
				new ClusterKeyMetaDataAttribute(AssemblyLoader.LoadAssembly("Enterprise.Customs.Business").GetType("Enterprise.Customs.Business.CusEntryHeader"))
				{
					ParentTableName = "JobDeclaration",
					ParentFkColumnName = "CH_JE",
				},
				new ClusterKeyMetaDataAttribute(AssemblyLoader.LoadAssembly("Enterprise.Customs.Business").GetType("Enterprise.Customs.Business.CommonJobComInvoiceHeader"))
				{
					ParentTableName = "JobDeclaration",
					ParentFkColumnName = "JZ_JE",
					TableName = "JobComInvoiceHeader",
				},
			};
			var assemblyMetaDataReaderMock = new Mock<IAssemblyMetaDataReader>();
			assemblyMetaDataReaderMock.Setup(m => m.GetAttributes<ClusterKeyMetaDataAttribute>(true)).Returns(clusterKeyMetaDataAttributes);

			using (ObjectFactory.Substitute(assemblyMetaDataReaderMock.Object))
			{
				var clusterKeyMappingsProvider = new ClusterKeyMappingsProvider();
				var mappings = clusterKeyMappingsProvider.GetAllClusterKeyMappings();

				AssertNotNull("MappingData", mappings);
				AssertMappingInfo(mappings, "CusEntryHeader", "JobDeclaration", "CH_JE");
				AssertMappingInfo(mappings, "JobComInvoiceHeader", "JobDeclaration", "JZ_JE", true);
			}
			void AssertMappingInfo(IClusterKeyMappingData[] mappingItems, string tableName, string parentEntityName, string parentFkColumnName, bool canActAsTopmostTable = false, string secondaryParentTable = null, string secondaryFkColumn = null)
			{
				var mapping = mappingItems.FirstOrDefault(m => m.TableName == tableName);
				AssertNotNull($"Mapping for {tableName}", mapping);
				CombineAssertions($"Table {tableName}", () =>
				{
					AssertEquals("Parent Table Name", parentEntityName, mapping.ParentTableName);
					AssertEquals("FK Column Name", parentFkColumnName, mapping.ParentFkColumnName);
					AssertEquals("Can Act As Topmost Table", canActAsTopmostTable, mapping.CanBeTopmostTable);
					AssertEquals("Secondary Parent Table Name", secondaryParentTable, mapping.SecondaryParentTableName);
					AssertEquals("Secondary Parent FK Column Name", secondaryFkColumn, mapping.SecondaryFkColumnName);
				});
			}
		}

		public void TestClusterKeyMappingProviderRegistry()
		{
			var clusterKeyMappingsProvider = ObjectFactory.Get<IClusterKeyMappingsProvider>();
			AssertType<ClusterKeyMappingsProvider>(clusterKeyMappingsProvider);
			AssertSame("ClusterKeyMappingsProvider is registered as singleton", clusterKeyMappingsProvider, ObjectFactory.Get<IClusterKeyMappingsProvider>());
		}
	}
}
