using CargoWise.ComponentModel;
using Enterprise.Core;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class BatchForwardingShipmentValueObjectDataAdapter : ForwardingShipmentValueObjectDataAdapter
	{
		public override ForwardingShipment CreateOrUpdateFromValueObject(Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			ForwardingShipment shipment = null;

			var branchLocatorWrapper = new BranchLocatorObjectWrapper(context, shipmentValue);
			BranchFinderResult branchFinderResult = BranchLocator.Find((Xsd.XmlInterchange)context.Interchange, branchLocatorWrapper);

			if (!branchFinderResult.IsCreationAllowed)
			{
				context.Notify(new InfoNotification(Res.GetString("a9fd78f8-1b6d-47a4-bc82-a58672f058af", "Shipment file has not been imported because branch could not be located. Refer to registry settings {0}", SystemDataRegistry.Instance.ShipmentImportBranchRules.Inner.Location)));
			}
			else
			{
				if (branchFinderResult.Branch != null)
				{
					if (BranchImportContextService.GetInstance(context.Factory).IsInBranchContext &&
						BranchImportContextService.GetInstance(context.Factory).BranchContext.PK != branchFinderResult.Branch.PK)
					{
						context.FactoryProvider.SaveCurrentAndCreateNew();
					}

					BranchImportContextService.GetInstance(context.Factory).Set(branchFinderResult.Branch);
				}

				shipment = base.CreateOrUpdateFromValueObject(shipmentValue, context);
			}

			return shipment;
		}

		protected override ForwardingShipment FindBusinessObject(Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			ForwardingShipment result = base.FindBusinessObject(shipmentValue, context);

			if (result != null)
			{
				if (SystemDataRegistry.Instance.UpdateShipmentsDuringAutomaticImport.Value == Constants.ShipmentAutomaticImportOptions.Code.Create)
				{
					result = null;
				}
				else if (SystemDataRegistry.Instance.UpdateShipmentsDuringAutomaticImport.Value == Constants.ShipmentAutomaticImportOptions.Code.NoImport)
				{
					context.Notify(new ErrorNotification(ErrorType.ImportingDataError, Res.GetString("0b9c0e3b-597d-4c6c-99d7-3b7fa1ef141e", "House bill number '{0}' already exists. Shipment file has not been imported.", result.JS_HouseBill)));
				}
			}

			return result;
		}

		protected override bool IncludeInactiveForMatchingOnAgentReference
		{
			get { return false; }
		}

		protected override bool ShouldUpdateExistingObject(ForwardingShipment bizObj, INotifications notifications)
		{
			return SystemDataRegistry.Instance.UpdateShipmentsDuringAutomaticImport.Value == Constants.ShipmentAutomaticImportOptions.Code.Update;
		}

		protected override ShipmentOrdersDataAdapterHelper GetShipmentOrdersDataAdapterHelper(IValueObjectDataAdapter orderValueObjectDataAdapter = null)
		{
			return new BatchShipmentOrdersDataAdapterHelper(TriggeredByEvents, GetOrderValueObjectDataAdapter(TriggeredByEvents), SystemDataRegistry.Instance.UpdateShipmentOrdersDuringShipmentAutomaticImport.Value);
		}

		protected override IValueObjectDataAdapter GetOrderValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
		{
			return new OrderValueObjectDataAdapterForBatchShipment(triggeredByEvents, SystemDataRegistry.Instance.UpdateShipmentOrdersDuringShipmentAutomaticImport.Value);
		}
	}
}
