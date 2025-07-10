using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class SetDefaultsForJobTest : TestCaseWithFactory
	{
		public void TestOverseasAgentDefaultingFromDeliveryAgentForExport()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var creator = new TestObjectCreator(Factory);

			var rag = creator.CreateOrgHeader("CONSOLRC", true, false, "NZAKL");
			var cnr = creator.CreateOrgHeader("SHIPMCNR", true, false, "AUSYD");
			var cne = creator.CreateOrgHeader("SHIPMCNE", true, false, "NZAKL");
			var dag = creator.CreateOrgHeader("SHIPMDAG", true, false, "NZAKL");

			rag.OH_Code = "CONSOLRC";
			cnr.OH_Code = "SHIPMCNR";
			cne.OH_Code = "SHIPMCNE";
			dag.OH_Code = "SHIPMDAG";

			var consol = creator.CreateConsol("AUSYD", "NZAKL", saveIt: false);
			consol.JK_OA_ReceivingForwarderAddress = rag.MainAddress.PK;

			Factory.Save();

			var shipment = consol.Shipments.AddNew();

			try
			{
				shipment.CreateJobHeaderWithMutex();
				shipment.ConsignorPK = cnr.PK;
				shipment.ConsigneePK = cne.PK;

				AssertEquals("CONSOLRC", shipment.Job.AgentCollect.OH_Code);

				shipment.JS_OH_DeliveryAgent = dag.PK;

				AssertEquals("Overseas Agent has to re-default to Delivery Agent as first receiving agent and should override previously defaulted value", "SHIPMDAG", shipment.Job.AgentCollect.OH_Code);
			}
			finally
			{
				shipment.Job?.Dispose();
			}
		}

		public void TestOverseasAgentDefaultingFromPickupAgentForImport()
		{
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var creator = new TestObjectCreator(Factory);

			var sag = creator.CreateOrgHeader("CONSOLSA", true, false, "NZAKL");
			var cnr = creator.CreateOrgHeader("SHIPMCNR", true, false, "NZAKL");
			var cne = creator.CreateOrgHeader("SHIPMCNE", true, false, "AUSYD");
			var pag = creator.CreateOrgHeader("SHIPMPAG", true, false, "NZAKL");

			sag.OH_Code = "CONSOLSA";
			cnr.OH_Code = "SHIPMCNR";
			cne.OH_Code = "SHIPMCNE";
			pag.OH_Code = "SHIPMPAG";

			var consol = creator.CreateConsol("NZAKL", "AUSYD", saveIt: false);
			consol.JK_OA_SendingForwarderAddress = sag.MainAddress.PK;

			Factory.Save();

			var shipment = consol.Shipments.AddNew();
			try
			{
				shipment.CreateJobHeaderWithMutex();
				shipment.ConsignorPK = cnr.PK;
				shipment.ConsigneePK = cne.PK;

				AssertEquals("CONSOLSA", shipment.Job.AgentCollect.OH_Code);

				shipment.PickupAgentPK = pag.PK;

				AssertEquals("Overseas Agent has to re-default to Pickup Agent as first sending agent and should override previously defaulted value", "SHIPMPAG", shipment.Job.AgentCollect.OH_Code);
			}
			finally
			{
				shipment.Job?.Dispose();
			}
		}
	}
}
