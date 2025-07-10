using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.GUI.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.GUI
{
	[TestedType(typeof(JASShipmentForm))]
	internal class JASShipmentFormBasherTest : ShipmentFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.New<JASForwardingShipment>();
			var line = shipment.OuterPackLines.AddNew();
			line.JL_PackageCount = 10;
			var consol = shipment.Consols.AddNew();
			consol.Containers.AddNew();
			shipment.PickupConfirms.AddNew();
			shipment.DeliveryConfirms.AddNew();

			shipment.OuterPackLines.AddNew();
			var declaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			declaration[JobDeclarationSchema.JE_OverrideFreightDefaults] = true;
			declaration[JobDeclarationSchema.JE_MessageType] = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			((Customs.Business.BaseJobDeclaration)declaration).ApportionmentDirty = false;
			Factory.Save();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var result = new JASShipmentForm(shipment);
				result.ControllerID = ControllerIDs.JobShipment;
				return result;
			}
		}
	}
}
