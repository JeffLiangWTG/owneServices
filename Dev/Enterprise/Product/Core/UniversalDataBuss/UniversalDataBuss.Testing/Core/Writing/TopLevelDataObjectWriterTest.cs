using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.UniversalDataBuss.Testing.Core.Writing
{
	class TopLevelDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestJobCostingOnlyExportsWhenSentToOrgProxyOrRegistryItemIsSet()
		{
			var forwardingShipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			forwardingShipment.JS_TransportMode = "SEA"; // Sea Freight
			forwardingShipment.JS_PackingMode = "LCL";
			forwardingShipment.JS_ShipmentType = "STD"; // Standard House

			var consignor = Factory.New<IOrgHeader>();
			consignor.OH_FullName = "I'LL SEND IT";
			consignor.OH_RL_NKClosestPort = "NZAKL";

			var consignee = Factory.New<IOrgHeader>();
			consignee.OH_FullName = "GIMME GIMME";
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.OH_IsDebtor = true;

			forwardingShipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			forwardingShipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			var shipmentBO = forwardingShipment as BusinessObject;
			var creator = ObjectFactory.New<IAccountingTestDataCreator>();
			creator.CreateJobHeader(shipmentBO, consignee.PK);
			creator.AddChargeLineToCreatedJobHeader("FRT", "FAT RICH TRUNKS", 123.45m, "AUD");

			Factory.Save();

			var contextManager = shipmentBO.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var shipmentData = contextManager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO))).GetDataObject(shipmentBO) as UniversalShipment;

			CombineAssertions(delegate
			{
				AssertNotNull("shipmentData.JobCosting for ORP", shipmentData.JobCosting);

				shipmentData = contextManager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.RAG, shipmentBO))).GetDataObject(shipmentBO) as UniversalShipment;

				AssertNull("shipmentData.JobCosting for RAG", shipmentData.JobCosting);

				eAdaptorRegistry.Instance.UniversalXMLAlwaysIncludeJobCostingInUniversalShipment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				shipmentData = contextManager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.RAG, shipmentBO))).GetDataObject(shipmentBO) as UniversalShipment;

				AssertNotNull("shipmentData.JobCosting for RAG with Registry Item Set", shipmentData.JobCosting);

				shipmentData = contextManager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO))).GetDataObject(shipmentBO) as UniversalShipment;

				AssertNotNull("shipmentData.JobCosting for ORP with Registry Item Set", shipmentData.JobCosting);
			});
		}

		public void TestExportWithSpecifiedSchema()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
				shipment[JobShipmentSchema.JS_TransportMode] = "SEA";
				shipment[JobShipmentSchema.JS_PackingMode] = "LCL";
				shipment[JobShipmentSchema.JS_ShipmentType] = "STD";

				Factory.Save();

				var contextManager = shipment.GetUniversalDataContextManager() as IShipmentDataContextManager;

				var manager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipment), null, UniversalXmlSchema.Version_2012_11_DO_NOT_USE);

				var writer = contextManager.GetShipmentDataObjectWriter(manager);

				var dataObject = (UniversalShipment)writer.GetDataObject(shipment);

				var dataContext = dataObject.DataContext;

				Assert("shipment has correct namespace dependent data context", dataContext is DataObjects.Universal._2012_11.DataContext);
			}
		}
	}
}
