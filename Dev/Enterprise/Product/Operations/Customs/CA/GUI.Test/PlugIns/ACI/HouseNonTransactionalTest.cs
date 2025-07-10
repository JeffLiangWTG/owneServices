using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[UseSnapshotProtection]
	sealed class HouseNonTransactionalTest : TestCase
	{
		public void TestMutexLockCorrectlyWithoutIssue01158679()
		{
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;

			var console = factory.New<ForwardingConsol>();
			console.JK_TransportMode = Core.Constants.TransportModes.Sea;
			console.JK_RL_NKDischargePort = "CATOR";
			var shipment2 = console.Shipments.AddNew();
			factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AssertNoExceptionThrown("For ANY reason, the consol's Ocean bill could not be created, the house bill creation should be stopped to prevent duplicated rows.", () =>
			{
				using (var plugin = new ShipmentCargoReportPlugIn(shipment2))
				{
					plugin.InitialiseCusSCAHouse();
					AssertEquals("Someone else is already in the process of creating an ACI job for this shipment.\r\nYou cannot process this ACI job until they save their data. Please try later.", plugin.CoveringLabelText);
				}
				factory.Save();
				var generatedHouseBills = factory.Load<CusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_JS, shipment2.PK));
				AssertEquals("Only one created", 0, generatedHouseBills.Length);
			});
		}

		[ExpectNoExceptions]
		public void TestRaceConditionWithNewHouseBill()
		{
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;

			var console = factory.New<ForwardingConsol>();
			console.JK_TransportMode = Core.Constants.TransportModes.Sea;
			console.JK_RL_NKDischargePort = "CATOR";
			CusSCAOceanBill.GetNewOceanBill(console);
			var shipment = console.Shipments.AddNew();
			factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			CancellationTokenSource cts = new CancellationTokenSource();
			Task task = Task.Run(() =>
			{
				try
				{
					using (Db.DisposableActionForDbConnection())
					{
						ExecuteButDelayPlugin(shipment.PK,cts.Token);
					}
				}
				catch (OperationCanceledException)
				{
					// Task Cancelled.
				}
			}, cts.Token);

			using (var plugin1 = new ShipmentCargoReportPlugIn(shipment))
			{
				plugin1.DelayPluginForTestingMutex = 10000;
				plugin1.InitialiseCusSCAHouse();
				AssertEquals("Someone else is already in the process of creating an ACI job for this shipment.\r\nYou cannot process this ACI job until they save their data. Please try later.", plugin1.CoveringLabelText);
			}
			factory.Save();

			var generatedHouseBills = factory.Load<CusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_JS, shipment.PK));
			AssertEquals("Only one created", 1, generatedHouseBills.Length);

			if (!task.IsCompleted)
			{
				cts.Cancel();
				task.Wait();
				Fail("Task did not complete");
			}
		}
		void ExecuteButDelayPlugin(ZGuid pk, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			Db.Connection.EnsureIsOpen();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var house = factory2.Load<ForwardingShipment>(pk);

			token.ThrowIfCancellationRequested();

			using (var plugin = new ShipmentCargoReportPlugIn(house))
			{
				plugin.DelayPluginForTestingMutex = 5000;
				plugin.InitialiseCusSCAHouse();
			}
			token.ThrowIfCancellationRequested();

			factory2.Save();
		}
	}
}
