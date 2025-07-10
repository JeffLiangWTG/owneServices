using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ConsolRevenue
{
	[TestedType(typeof(ConsolRevenueMaster))]
	public class ConsolRevenueMasterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ConsolRevenueMaster(Factory.New<ForwardingConsol>(), Factory);
		}

		public void TestRevenuesIsRegisteredEditableChildObject()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ConsolRevenueMaster master = new ConsolRevenueMaster(consol, Factory);
			AssertEquals("Revenues IsRegisteredEditableChildObject", true, master.IsRegisteredEditableChildObject(master.Revenues));
		}

		public void TestConsolRevenueMasterWithMutex()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUABC", "KRABC", "C00001");
			var shipment = creator.CreateShipment("S00001", consol);
			shipment.JS_ActualWeight = 100m;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			Factory.Save();
			//user click the 'Apportion Revenue To Shipments' menu
			var newFactory = new BusinessObjectFactory();
			var consolRevMaster = new ConsolRevenueMaster(consol, newFactory);
			string mutexExceptionMessage = null;
			consolRevMaster.JobCreationExceptionEvent += (sender, e) =>
			{
				mutexExceptionMessage = e.Message;
			};

			var consolRevenue = new ConsolRevenue(consolRevMaster);
			consolRevenue.ChargeCode = creator.CC1.PK;
			var splitCharges = consolRevenue.SplitCharges;
			AssertNull(mutexExceptionMessage); //mutex created
			//user opens the billing tab
			var newFactory2 = new BusinessObjectFactory();
			var shipment2 = newFactory2.Load<ForwardingShipment>(shipment.PK);
			var loader = new JobHeader.Loader(newFactory2, shipment2);
			var job = loader.TryLoadOrCreateWithMutex();
			AssertNull(job); //can not acquire mutex now.
			consolRevMaster.ReleaseMutexes();
			job = loader.TryLoadOrCreateWithMutex();
			AssertNotNull(job); //can acquire mutex now.
			job.Dispose();
		}

		public void TestSetIsJobMutexDisposed()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var shipment1 = TestObjectCreator.CreateShipment("S0000010");
			var shipment2 = TestObjectCreator.CreateShipment("S0000011");
			var shipments = new List<IJobInvoicingPlugIn>() { shipment1, shipment2 };
			var newFactory = new BusinessObjectFactory();
			var loader = new Job.Loader(newFactory, shipment2);
			var jobForShipment2 = loader.TryLoadOrCreateWithMutex();
			AssertNotNull(jobForShipment2);

			var consolRevenueMaster = CreateConsolRevenueMaster(shipments);
			var consolRevenue = new ConsolRevenue(consolRevenueMaster);
			AssertEquals(false, Factory.HasContext(BusinessContext.ShouldSkipConsolRevenueApportionFormClosing));
			AssertNotNull(consolRevenue.SplitCharges);
			AssertEquals(true, Factory.HasContext(BusinessContext.ShouldSkipConsolRevenueApportionFormClosing));

			jobForShipment2.Dispose();
		}

		public void TestNotSetIsJobMutexDisposed()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var shipment = TestObjectCreator.CreateShipment("S0000010");
			var shipments = new List<IJobInvoicingPlugIn>() { shipment };
			var newFactory = new BusinessObjectFactory();
			var loader = new Job.Loader(newFactory, shipment);
			var job = loader.TryLoadOrCreateWithMutex();
			AssertNotNull(job);

			var consolRevenueMaster = CreateConsolRevenueMaster(shipments);
			var consolRevenue = new ConsolRevenue(consolRevenueMaster);
			AssertEquals(false, Factory.HasContext(BusinessContext.ShouldSkipConsolRevenueApportionFormClosing));
			AssertNotNull(consolRevenue.SplitCharges);
			AssertEquals(false, Factory.HasContext(BusinessContext.ShouldSkipConsolRevenueApportionFormClosing));

			job.Dispose();
		}

		ConsolRevenueMaster CreateConsolRevenueMaster(List<IJobInvoicingPlugIn> shipments)
		{
			var costSupporter = new Mock<IGenericJobCostSupporter>();
			costSupporter.Setup(x => x.ShipmentsList).Returns(shipments.ToArray());

			var consol = new Mock<IJobCostingPlugIn>();
			consol.Setup(x => x.CostSupporter).Returns(costSupporter.Object);

			var consolRevenueMaster = new ConsolRevenueMaster(consol.Object, Factory);
			return consolRevenueMaster;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
