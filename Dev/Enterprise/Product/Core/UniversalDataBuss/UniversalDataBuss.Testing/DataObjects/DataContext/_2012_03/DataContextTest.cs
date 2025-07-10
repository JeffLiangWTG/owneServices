using System.Linq;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11.Testing
{
	[TestedType(typeof(DataContext))]
	class DataContextTest : DataObjectTestCase<DataContext>
	{
		public void TestGetDataSourceKey()
		{
			DataContext dataContext = null;

			AssertEquals("Should return empty string if DataContext is null.", "", dataContext.GetDataSourceKey());

			dataContext = new DataContext();
			AssertEquals("Should be null proof on all properties read.", "||", dataContext.GetDataSourceKey());

			dataContext.DataSource = new DataSource()
			{
				DataProvider = new DataProvider() { Code = "ENTSERCOM", Type = DataProviderType.EnterpriseID },
				Type = "ForwardingConsol",
				Key = "C00001000"
			};

			AssertEquals("Key should only include the primary Data Source Reference", "ENT|SER|COM|ForwardingConsol|C00001000", dataContext.GetDataSourceKey());
		}

		public void TestLazyGetDataSources()
		{
			var dataContext = new DataContext();
			var testIdentity = new TestIdentity();
			((IDataContextDataObject)dataContext).AddDataSource(testIdentity);

			testIdentity.DataContextKey = "key";
			testIdentity.DataContextType = DataContextType.BatchNumber;
			dataContext.DataSource.DataProvider = new DataProvider() { Code = "ENTSERCOM", Type = DataProviderType.EnterpriseID };
			AssertEquals("Key should have value", "key", ((IDataContextDataObject)dataContext).DataSourceCollection.Single().Key.Value);
			AssertEquals("Type should have value", nameof(DataContextType.BatchNumber), ((IDataContextDataObject)dataContext).DataSourceCollection.Single().Type.Value);
			AssertEquals("Key should only include the primary Data Source Reference", "ENT|SER|COM|BatchNumber|key", dataContext.GetDataSourceKey());
		}

		class TestIdentity : IEntityID
		{
			public DataContextType DataContextType { get; set; }

			public string DataContextKey { get; set; }
		}
	}
}

