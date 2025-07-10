using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using eAdaptorRegistry = Enterprise.Registry.Business.eServices.eAdaptorRegistry;

namespace Enterprise.UniversalDataBuss.Testing.Management
{
	class ExtensionsTests : TestCaseWithFactory
	{
		public void TestAddDataTargetToUniversalEvent_2011()
		{
			TestAddDataTargetToUniversalEventCore(
				() => new DataObjects.Universal._2011_11.DataContext { DataSourceCollection = new List<DataObjects.Universal._2011_11.DataSource>() },
				() => new DataObjects.Universal._2011_11.DataContext { DataSourceCollection = new List<DataObjects.Universal._2011_11.DataSource> { new DataObjects.Universal._2011_11.DataSource { Key = "KEY", Type = nameof(DataContextType.Organization) } } });
		}

		public void TestAddDataTargetToUniversalEvent_2012()
		{
			TestAddDataTargetToUniversalEventCore(
				() => new DataObjects.Universal._2012_11.DataContext { DataTargetCollection = new List<DataObjects.Universal._2012_11.DataTarget>() },
				() => new DataObjects.Universal._2012_11.DataContext { DataSource = new DataObjects.Universal._2012_11.DataSource { Key = "KEY", Type = nameof(DataContextType.Organization) } });
		}

		public void TestAddDataTargetToUniversalEventCore<TContext>(Func<TContext> setupEmpty, Func<TContext> setupPopulated) where TContext : IDataContextDataObject, new()
		{
			var uEvent = new Event();
			uEvent.AddDataTargetToUniversalEvent(null);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			uEvent.AddDataTargetToUniversalEvent(shipment);

			shipment.DataContext = new TContext();
			uEvent.AddDataTargetToUniversalEvent(shipment);

			shipment.DataContext = setupEmpty();
			uEvent.AddDataTargetToUniversalEvent(shipment);

			shipment.DataContext = setupPopulated();
			uEvent.AddDataTargetToUniversalEvent(shipment);

			var dataTarget = uEvent.DataContext.DataTargetCollection.Single();
			AssertEquals("KEY", dataTarget.Key);
			AssertEquals("Organization", dataTarget.Type);
		}

		public void TestShouldProduceFalseWhenRegistryItemSetToNotFailOnInactive()
		{
			var department = Factory.New<IGlbDepartment>();
			(false, department).ReportActiveState(new XmlSessionTracker(new Integration.DummyLogger()), "test", out var result);
			Assert("result should be false", !result);
		}

		public void TestShouldProduceTrueWhenRegistryItemSetToFailOnInactive()
		{
			eAdaptorRegistry.Instance.UniversalXMLInactiveDepartmentFailsMessage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var department = Factory.New<IGlbDepartment>();
			(false, department).ReportActiveState(new XmlSessionTracker(new Integration.DummyLogger()), "test", out var result);
			Assert("result should be true", result);
		}

		public void TestShouldProduceFalseWhenTupleActiveValueIsTrue()
		{
			var department = Factory.New<IGlbDepartment>();
			(true, department).ReportActiveState(new XmlSessionTracker(new Integration.DummyLogger()), "test", out var result);
			Assert("result should be false", !result);
		}
	}
}
