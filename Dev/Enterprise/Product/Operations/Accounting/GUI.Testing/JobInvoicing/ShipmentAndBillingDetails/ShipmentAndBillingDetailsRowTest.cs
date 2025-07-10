using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(ShipmentAndBillingDetailsRow))]
	internal class ShipmentAndBillingDetailsRowTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPropertiesReadOnly()
		{
			var shipment = ObjectCreator.CreateShipment("S123");
			var consol = ObjectCreator.CreateGatewayConsol(sendingGatewayCompany: GlbCompany.CurrentCompany);
			using (ObjectCreator.CreateJob(consol))
			{
				var details = new ShipmentAndBillingDetailsRow(shipment, consol);

				Assert(details.TargetJobNumInfo.ReadOnly);
				Assert(details.RelatedJobNumInfo.ReadOnly);
				Assert(details.RelatedJobOriginInfo.ReadOnly);
				Assert(details.RelatedJobDestinationInfo.ReadOnly);
				Assert(details.PickUpAgentCodeInfo.ReadOnly);
				Assert(details.PickUpAgentNameInfo.ReadOnly);
				Assert(details.DeliveryAgentCodeInfo.ReadOnly);
				Assert(details.DeliveryAgentNameInfo.ReadOnly);
				Assert(details.PreviousSendingAgentCodeInfo.ReadOnly);
				Assert(details.PreviousSendingAgentNameInfo.ReadOnly);
				Assert(details.DebtorCodeInfo.ReadOnly);
				Assert(details.DebtorNameInfo.ReadOnly);
				Assert(details.TargetJobRouteInfo.ReadOnly);
				Assert(details.TargetJobNumInfo.ReadOnly);
				Assert(!details.ShowRelatedChargesInfo.ReadOnly);
			}
		}

		public void TestRelatedJobNum()
		{
			var consol = ObjectCreator.CreateGatewayConsolsAndShipments().gC0001;
			Factory.Save();
			consol.Shipments[0].JS_UniqueConsignRef = ZString.Empty;
			using (var consolJob = ObjectCreator.CreateJob(consol, false))
			{
				foreach (ForwardingShipment shipment in consol.Shipments)
				{
					var detail = new ShipmentAndBillingDetailsRow(shipment, consol);
					AssertEquals(shipment.JobNumber, detail.RelatedJobNum);
					AssertEquals(shipment.InvoicingSupporter.Origin.Code, detail.RelatedJobOrigin);
					AssertEquals(shipment.InvoicingSupporter.Destination.Code, detail.RelatedJobDestination);

					AssertEquals(shipment.PickupAgent?.OH_Code ?? ZString.Empty, detail.PickUpAgentCode);
					AssertEquals(shipment.PickupAgent?.OH_FullName ?? ZString.Empty, detail.PickUpAgentName);

					AssertEquals(shipment.DeliveryAgent?.OH_Code ?? ZString.Empty, detail.DeliveryAgentCode);
					AssertEquals(shipment.DeliveryAgent?.OH_FullName ?? ZString.Empty, detail.DeliveryAgentName);

					var previousSendingAgent = (consolJob.GetInvoicingSupporter().GetPreviousConsol(shipment.JobNumber) as ForwardingConsol)?.SendingForwarder;
					AssertEquals(previousSendingAgent?.OH_Code ?? ZString.Empty, detail.PreviousSendingAgentCode);
					AssertEquals(previousSendingAgent?.OH_FullName ?? ZString.Empty, detail.PreviousSendingAgentName);
				}
			}
		}

		public void TestDebtor()
		{
			var newRegValue = new GatewayChargeDefaultDebtorConfigurationCollection();
			newRegValue.Add(new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ORG",
				ConsolPaymentTerm = "ALL",
				RelatedJob = "ALL",
				PreviousSendingAgent = "ALL",
				Debtor = "SGT"
			});
			AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newRegValue);

			var setting3 = TestObjectCreator.SetupDebtorDefaultingRegistry(("ALL", "ALL", "ALL", "ALL", "SHP", "GTT", "PSA"));      // 3rd priority
			var setting2 = TestObjectCreator.SetupDebtorDefaultingRegistry(("ALL", "ALL", "ALL", "PPD", "ALL", "ALL", "RGT"));      // 2nd priority
			var setting1 = TestObjectCreator.SetupDebtorDefaultingRegistry(("ALL", "ALL", "ALL", "PPD", "SHP", "ALL", "SPA"));      // 1st priority
			TestObjectCreator.SetupDebtorDefaultingRegistry(("ALL", "ALL", "ALL", "ALL", "SHP", "GTA", "RGT"));      // NA (GTA)
			TestObjectCreator.SetupDebtorDefaultingRegistry(("ALL", "ALL", "ALL", "CCX", "ALL", "ALL", "RGT"));      // NA (CCX)

			var setup = ObjectCreator.CreateGatewayConsolsAndShipments("PPD");
			setup.gC0001.JK_SendingForwarderHandlingType = "GTT";
			var consol = setup.gC0002;
			ObjectCreator.CreateJob(consol, false);

			setup.shPicAg.OH_IsDebtor = true;
			var details = new ShipmentAndBillingDetailsRow(setup.s0002, consol);
			AssertEquals("Matches with Most specific registry setting if org is debtor", setup.shPicAg.OH_Code, details.DebtorCode);       //1st priority
			AssertEquals("DebtorName matches debtor's name", setup.shPicAg.OH_FullName, details.DebtorName);
			AssertEquals("DebtorAddress matches debtor's AR Document (default) address", setup.shPicAg.AddressForSendingARDocuments.Address1, details.DebtorAddress);

			setup.shPicAg.OH_IsDebtor = false;
			details = new ShipmentAndBillingDetailsRow(setup.s0002, consol);
			AssertEquals("Doesn't set a debtor if the default org is not a debtor", ZString.Empty, details.DebtorCode);       //1st priority false debtor
			AssertEquals("DebtorName is empty if debtor is empty", ZString.Empty, details.DebtorName);
			AssertEquals("DebtorAddress is empty if debtor is empty", ZString.Empty, details.DebtorAddress);

			setting1.Dispose();
			setup.recAg.OH_IsDebtor = true;
			details = new ShipmentAndBillingDetailsRow(setup.s0002, consol);
			AssertEquals("Matches with most specific registry setting", setup.recAg.OH_Code, details.DebtorCode);       //2nd priority
			AssertEquals("DebtorName matches debtor's name", setup.recAg.OH_FullName, details.DebtorName);
			AssertEquals("DebtorAddress matches debtor's AR Document (default) address", setup.recAg.AddressForSendingARDocuments.Address1, details.DebtorAddress);

			setting2.Dispose();
			setup.prevSenAg.OH_IsDebtor = true;
			details = new ShipmentAndBillingDetailsRow(setup.s0002, consol);
			AssertEquals("Matches with most specific registry setting", setup.prevSenAg.OH_Code, details.DebtorCode);       //3rd priority
			AssertEquals("DebtorName matches debtor's name", setup.prevSenAg.OH_FullName, details.DebtorName);
			AssertEquals("DebtorAddress matches debtor's AR Document (default) address", setup.prevSenAg.AddressForSendingARDocuments.Address1, details.DebtorAddress);

			setting3.Dispose();
			details = new ShipmentAndBillingDetailsRow(setup.s0002, consol);
			AssertEquals("Should not match to anything if no registry settings with ALL as charge group", ZString.Empty, details.DebtorCode);       //Default
			AssertEquals("DebtorName is empty if debtor is empty", ZString.Empty, details.DebtorName);
			AssertEquals("DebtorAddress is empty if debtor is empty", ZString.Empty, details.DebtorAddress);
		}

		public void TestInvoiceTarget()
		{
			TestObjectCreator.SetupDebtorDefaultingRegistry(("ALL", "ALL", "ALL", "PPD", "SHP", "ALL", "SDA"));
			var dataSetup = ObjectCreator.CreateGatewayConsolsAndShipments();
			var consol = dataSetup.gC0002;
			Factory.Save();

			var collection = new GatewayChargeDefaultInvoiceTargetJobConfigurationCollection();
			var setting1 = collection.AddNew();
			setting1.ConsolDirection = "ALL";
			setting1.ConsolTransportMode = "ALL";
			setting1.PreviousSendingAgentType = "SGT";
			setting1.InvoiceTargetJobType = "REL";
			var setting2 = collection.AddNew();
			setting2.ConsolDirection = "ALL";
			setting2.ConsolTransportMode = "ALL";
			setting2.PreviousSendingAgentType = "GTA";
			setting2.InvoiceTargetJobType = "REL";
			var setting3 = collection.AddNew();
			setting3.ConsolDirection = "ALL";
			setting3.ConsolTransportMode = "ALL";
			setting3.PreviousSendingAgentType = "GTT";
			setting3.InvoiceTargetJobType = "PCL";
			var setting4 = collection.AddNew();
			setting4.ConsolDirection = "ALL";
			setting4.ConsolTransportMode = "ALL";
			setting4.PreviousSendingAgentType = "NON";
			setting4.InvoiceTargetJobType = "SCL";
			var setting5 = collection.AddNew();
			setting5.ConsolDirection = "ALL";
			setting5.ConsolTransportMode = "ALL";
			setting5.PreviousSendingAgentType = "ALL";
			setting5.InvoiceTargetJobType = "SCL";
			AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultInvoiceTargetJobConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			using (var consolJob = ObjectCreator.CreateJob(dataSetup.gC0002))
			{
				var relatedShipment = dataSetup.s0002;
				relatedShipment.JS_OH_DeliveryAgent = ObjectCreator.DebtorSisterOrgProxy.PK;

				//Test 1 - invalid related job number
				relatedShipment.JS_UniqueConsignRef = ZString.Empty;
				AssertEquals("Pre-requisite: need invalid job number for this case", ZString.Empty, relatedShipment.JobNumber);
				var detailRow = new ShipmentAndBillingDetailsRow(relatedShipment, consol);
				AssertEquals(string.Empty, detailRow.TargetJobNum);

				relatedShipment.JS_UniqueConsignRef = "S0002";
				var previousConsol = consolJob.GetInvoicingSupporter().GetPreviousConsol("S0002");

				//Test 2 - previous consol exists, sending agent does not exists, NON => SCL
				AssertEquals("C0001", previousConsol.InvoicingSupporter.ConsolNumber);
				dataSetup.gC0001.JK_OA_SendingForwarderAddress = ZGuid.Empty;
				AssertNull("previous consol sending agent is null", dataSetup.gC0001.SendingForwarder);
				detailRow = new ShipmentAndBillingDetailsRow(relatedShipment, consol);
				AssertEquals("C0002", detailRow.TargetJobNum);
				AssertEquals($"{dataSetup.gC0002.Transports[0].JW_RL_NKLoadPort} - {dataSetup.gC0002.Transports[0].JW_RL_NKDiscPort}", detailRow.TargetJobRoute);

				//Test 3 - previous consol exists, sending agent exists AND type is blank, SGT => REL
				dataSetup.gC0001.JK_OA_SendingForwarderAddress = ObjectCreator.DebtorSisterOrgProxy.MainAddress.PK;
				AssertNotNull("previous consol sending agent is NOT null", dataSetup.gC0001.SendingForwarder);
				AssertEquals(ZString.Empty, dataSetup.gC0001.JK_SendingForwarderHandlingType);
				detailRow = new ShipmentAndBillingDetailsRow(relatedShipment, consol);
				AssertEquals("S0002", detailRow.TargetJobNum);
				AssertEquals($"{relatedShipment.Origin.Code} - {relatedShipment.Destination.Code}", detailRow.TargetJobRoute);

				//Test 4 - previous consol exists, sending agent exists AND type is GTA, GTA => REL
				dataSetup.gC0001.JK_SendingForwarderHandlingType = "GTA";
				detailRow = new ShipmentAndBillingDetailsRow(relatedShipment, consol);
				AssertEquals("S0002", detailRow.TargetJobNum);
				AssertEquals($"{relatedShipment.Origin.Code} - {relatedShipment.Destination.Code}", detailRow.TargetJobRoute);

				//Test 5 - previous consol exists, sending agent exists AND type is GTT, GTT => PCL
				dataSetup.gC0001.JK_SendingForwarderHandlingType = "GTT";
				detailRow = new ShipmentAndBillingDetailsRow(relatedShipment, consol);
				AssertEquals("C0001", detailRow.TargetJobNum);
				AssertEquals($"{dataSetup.gC0001.Transports[0].JW_RL_NKLoadPort} - {dataSetup.gC0001.Transports[0].JW_RL_NKDiscPort}", detailRow.TargetJobRoute);

				//Test 6 - previous consol does NOT exist, NON => SCL
				dataSetup.s0002.Consols.Remove(dataSetup.gC0001);
				previousConsol = consolJob.GetInvoicingSupporter().GetPreviousConsol("S0002");
				AssertNull(previousConsol);
				detailRow = new ShipmentAndBillingDetailsRow(relatedShipment, consol);
				AssertEquals("C0002", detailRow.TargetJobNum);
				AssertEquals($"{dataSetup.gC0002.Transports[0].JW_RL_NKLoadPort} - {dataSetup.gC0002.Transports[0].JW_RL_NKDiscPort}", detailRow.TargetJobRoute);
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ShipmentAndBillingDetailsRow();
		}

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;

		#endregion
	}
}
