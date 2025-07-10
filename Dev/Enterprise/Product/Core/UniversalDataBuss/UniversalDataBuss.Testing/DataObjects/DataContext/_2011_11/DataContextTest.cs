using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11.Testing
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

			dataContext.EnterpriseID = "ENT";
			dataContext.ServerID = "SER";
			dataContext.Company = new Company() { Code = "COM", Name = "Company" };

			var dataSources = new List<DataSource>();
			dataSources.Add(new DataSource() { Type = "ForwardingConsol", Key = "C00001000" });
			dataSources.Add(new DataSource() { Type = "ForwardingShipment", Key = "S00001000" });
			dataContext.DataSourceCollection = dataSources;

			AssertEquals("Key should only include the primary Data Source Reference", "ENT|SER|COM|ForwardingConsol|C00001000", dataContext.GetDataSourceKey());
		}

		public void TestGetEnterpriseCode()
		{
			var dataContext = new DataContext() { DataProvider = "ABCDEFGHI" };
			var dataContextWithEmptyProvider = new DataContext() { DataProvider = "" };
			AssertEquals("ABC", dataContext.GetEnterpriseCode());
			AssertEquals("", dataContextWithEmptyProvider.GetEnterpriseCode());
		}

		public void TestGetServerCode()
		{
			var dataContext = new DataContext() { DataProvider = "ABCDEFGHI" };
			var dataContextWithEmptyProvider = new DataContext() { DataProvider = "" };
			AssertEquals("DEF", dataContext.GetServerCode());
			AssertEquals("", dataContextWithEmptyProvider.GetServerCode());
		}

		public void TestGetCompanyCode()
		{
			var dataContext = new DataContext() { DataProvider = "ABCDEFGHI" };
			var dataContextWithEmptyProvider = new DataContext() { DataProvider = "" };
			AssertEquals("GHI", dataContext.GetCompanyCode());
			AssertEquals("", dataContextWithEmptyProvider.GetCompanyCode());
		}

		public void TestGetDataSources()
		{
			DataContext dataContext = null;

			AssertEquals("Should return empty string if DataContext is null.", "", dataContext.GetDataSources());

			dataContext = new DataContext();
			AssertEquals("Should be null proof on all properties read.", "", dataContext.GetDataSources());

			dataContext.EnterpriseID = "ENT";
			dataContext.ServerID = "SER";
			dataContext.Company = new Company() { Code = "COM", Name = "Company" };

			var dataSources = new List<DataSource>();
			dataSources.Add(new DataSource() { Type = "ForwardingConsol", Key = "C00001000" });
			dataSources.Add(new DataSource() { Type = "ForwardingShipment", Key = "S00001000" });
			dataContext.DataSourceCollection = dataSources;

			AssertEquals("Here's what a full key should look like...", "ForwardingConsol [C00001000], ForwardingShipment [S00001000]", dataContext.GetDataSources());
		}

		public void TestLazyGetDataSources()
		{
			var dataContext = new DataContext();
			dataContext.EnterpriseID = "ENT";
			dataContext.ServerID = "SER";
			dataContext.Company = new Company() { Code = "COM", Name = "Company" };

			var testIdentity = new TestIdentity();
			((IDataContextDataObject)dataContext).AddDataSource(testIdentity);

			testIdentity.DataContextKey = "key";
			testIdentity.DataContextType = DataContextType.BatchNumber;
			AssertEquals("Key should have value", "key", ((IDataContextDataObject)dataContext).DataSourceCollection.Single().Key.Value);
			AssertEquals("Type should have value", nameof(DataContextType.BatchNumber), ((IDataContextDataObject)dataContext).DataSourceCollection.Single().Type.Value);
		}

		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(DataContext.TriggerReference), ProcessTask.MacroTriggerConditionValueMaxLength },
				{ nameof(DataContext.ServerID), 3 },
				{ nameof(DataContext.EnterpriseID), 3 },
				{ nameof(DataContext.DataProvider), 50 },
				{ nameof(DataContext.EventReference), StmALogSchema.SL_Reference.MaxLength },
				{ nameof(DataContext.TriggerDescription), ProcessTasksSchema.P9_Description.MaxLength },
			};
		}

		class TestIdentity : IEntityID
		{
			public DataContextType DataContextType { get; set; }

			public string DataContextKey { get; set; }
		}
	}
}

