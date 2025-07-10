using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[UseSnapshotProtection]
	sealed class ConsolNonTransactionalTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestRaceConditionWithNewOceanBill()
		{
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;

			ValidationTestHelper.AddCarrierCodeToCurrentCompany(factory, "8080");

			var console1 = factory.New<ForwardingConsol>();
			console1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			console1.JK_RL_NKDischargePort = "CATOR";
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
						ExecuteButDelayPlugin(console1.PK, cts.Token);
					}
				}
				catch (OperationCanceledException)
				{
					// Task Cancelled.
				}
			}, cts.Token);

			using (var plugin1 = new CAConsolACIPlugIn(console1))
			{
				plugin1.DelayPluginForTestingMutex = 10000;
				plugin1.InitialiseBusinessObject();
				AssertEquals("Someone else is already in the process of creating ACI jobs for this consol.\r\nYou cannot process ACI jobs on this consol until they save their data. Please try later.", plugin1.CoveringLabelText);
			}
			factory.Save();
			if(!task.IsCompleted)
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
			var console = factory2.Load<ForwardingConsol>(pk);

			var factory3 = new BusinessObjectFactory();
			factory3.RefreshEnabled = false;
			ValidationTestHelper.AddCarrierCodeToCurrentCompany(factory3, "8080");

			token.ThrowIfCancellationRequested();

			using (var plugin = new CAConsolACIPlugIn(console))
			{
				plugin.DelayPluginForTestingMutex = 5000;
				plugin.InitialiseBusinessObject();
			}

			token.ThrowIfCancellationRequested();
			factory2.Save();
		}
	}
}
