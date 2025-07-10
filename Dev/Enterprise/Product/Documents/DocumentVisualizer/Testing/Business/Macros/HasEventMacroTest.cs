using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class HasEventMacroTest : TestCaseWithFactory
	{
		public void TestHasEvent()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_OH_DeliveryAgent = orgHeader.PK;

			var expr = $"@data.HasEvent(\"{Events.StatusUpdated.Code}\")".With<FilterLibrary>().CreateExpression();
			AssertEquals(false, (bool)expr.Evaluate(shipment));

			(shipment as IStmALogParent).Logs.AddNew(Events.StatusUpdated, "Status A", ZDateTimeOffset.Now);
			AssertEquals(true, (bool)expr.Evaluate(shipment));
			AssertEquals(false, expr.HasErrors());

			expr = $"HasEvent(\"{Events.StatusUpdated.Code}\")".With<FilterLibrary>().CreateExpression();
			AssertEquals(true, (bool)expr.Evaluate(shipment));
			AssertEquals(false, expr.HasErrors());

			expr = $"@data.HasEvent(DeliveryAgent,\"{Events.StorageCommenced.Code}\")".With<FilterLibrary>().CreateExpression();
			AssertEquals(false, (bool)expr.Evaluate(shipment));

			orgHeader.Logs.AddNew(Events.StorageCommenced, "Status B", ZDateTimeOffset.Now);
			AssertEquals(true, (bool)expr.Evaluate(shipment));
			AssertEquals(false, expr.HasErrors());

			expr = $"HasEvent(DeliveryAgent,\"{Events.StorageCommenced.Code}\")".With<FilterLibrary>().CreateExpression();
			AssertEquals(true, (bool)expr.Evaluate(shipment));
			AssertEquals(false, expr.HasErrors());

			expr = $"DeliveryAgent.HasEvent(\"{Events.StorageCommenced.Code}\")".With<FilterLibrary>().CreateExpression();
			AssertEquals(true, (bool)expr.Evaluate(shipment));
			AssertEquals(false, expr.HasErrors());

			expr = $"@data.DeliveryAgent.HasEvent(\"{Events.StorageCommenced.Code}\")".With<FilterLibrary>().CreateExpression();
			AssertEquals(true, (bool)expr.Evaluate(shipment));
			AssertEquals(false, expr.HasErrors());

			expr = $"@data.HasEvent(\"{Events.StatusUpdated.Code}\")".With<FilterLibrary>().CreateExpression();
			expr.Evaluate(Factory.New<DummyBusinessObject>());
			AssertEquals(true, expr.HasErrors());
			AssertEquals("The current data does not support Events.", expr.Errors.Single().Message);
		}

		public void TestHasEvent_NotIncludeIsEstimateOrIsCancelledEvent()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var log = orgHeader.Logs.AddNew(Events.StorageCommenced, "Status A", ZDateTimeOffset.Now);
			var expr = $"HasEvent(\"{Events.StorageCommenced.Code}\")".With<FilterLibrary>().CreateExpression();

			AssertEquals("Precondition", true, (bool)expr.Evaluate(orgHeader));
			AssertEquals("Precondition", false, log.SL_IsEstimate);
			AssertEquals("Precondition", false, log.SL_IsCancelled);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_IsEstimate = true;
				AssertEquals(false, (bool)expr.Evaluate(orgHeader));

				log.SL_IsEstimate = false;
				AssertEquals(true, (bool)expr.Evaluate(orgHeader));

				log.Cancel();
				AssertEquals(true, log.SL_IsCancelled);
				AssertEquals(false, (bool)expr.Evaluate(orgHeader));
			}
		}
	}
}
