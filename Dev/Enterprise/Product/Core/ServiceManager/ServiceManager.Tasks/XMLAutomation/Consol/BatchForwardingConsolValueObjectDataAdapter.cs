using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class BatchForwardingConsolValueObjectDataAdapter : ForwardingConsolValueObjectDataAdapter
	{
		public override ForwardingConsol CreateOrUpdateFromValueObject(Xsd.Consol consolValue, IValueObjectImportContext context)
		{
			ForwardingConsol consol = null;

			var branchLocatorWrapper = new BranchLocatorObjectWrapper(context, consolValue);
			BranchFinderResult branchFinderResult = BranchLocator.Find((Xsd.XmlInterchange)context.Interchange, branchLocatorWrapper);

			if (!branchFinderResult.IsCreationAllowed)
			{
				context.Notify(new InfoNotification(Res.GetString("dab2d38c-718e-41df-90b3-0daf7e7894a9", "Consol file has not been imported because branch could not be located. Refer to registry settings {0}", SystemDataRegistry.Instance.ConsolImportBranchRules.Inner.Location)));
			}
			else if (branchFinderResult.Branch != null)
			{
				if (BranchImportContextService.GetInstance(context.Factory).IsInBranchContext &&
					BranchImportContextService.GetInstance(context.Factory).BranchContext.PK != branchFinderResult.Branch.PK)
				{
					context.FactoryProvider.SaveCurrentAndCreateNew();
				}
				BranchImportContextService.GetInstance(context.Factory).Set(branchFinderResult.Branch);
				consol = base.CreateOrUpdateFromValueObject(consolValue, context);
			}
			else
			{
				consol = base.CreateOrUpdateFromValueObject(consolValue, context);
			}

			return consol;
		}

		protected override ShipmentValueObjectDataAdapter<ForwardingShipment> GetNewShipmentValueObjectDataAdapter(ForwardingConsol existingConsol)
		{
			return new ForwardingShipmentValueObjectDataAdapterForBatchConsol(existingConsol, TriggeredByEvents);
		}
	}
}
