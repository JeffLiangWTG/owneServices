using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Registry.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class BranchLocatorObjectWrapper
	{
		public BranchLocatorObjectWrapper(IValueObjectImportContext context, Xsd.Shipment shipmentValueObject)
		{
			OriginOrLoadPort = GetPortCode(context, shipmentValueObject.OriginPort);
			DestinationOrDischargePort = GetPortCode(context, shipmentValueObject.DestinationPort);
			ImportRule = SystemDataRegistry.Instance.ShipmentImportBranchRules.Value;
		}

		ZString GetPortCode(IValueObjectImportContext context, Xsd.UNLOCO unlocoValue)
		{
			var result = ZString.Empty;

			if (unlocoValue != null)
			{
				result = (ZString)context.Converter.ConvertRawStringToZTypeValue(typeof(ZString), ForeignKeyType.PortNK, context.Factory, unlocoValue.Value, context);
			}
			return result;
		}

		public BranchLocatorObjectWrapper(IValueObjectImportContext context, Xsd.Consol consolValueObject)
		{
			OriginOrLoadPort = GetPortCode(context, consolValueObject.LoadPort);
			DestinationOrDischargePort = GetPortCode(context, consolValueObject.DischargePort);
			ImportRule = SystemDataRegistry.Instance.ConsolImportBranchRules.Value;
		}

		public ZString OriginOrLoadPort
		{
			get;
			private set;
		}

		public ZString DestinationOrDischargePort
		{
			get;
			private set;
		}

		public ImportBranchRule ImportRule
		{
			get;
			private set;
		}

#if DEBUG
		public void SetOriginOrLoadPort(ZString portCode)
		{
			OriginOrLoadPort = portCode;
		}

		public void SetDestinationOrDischargePort(ZString portCode)
		{
			DestinationOrDischargePort = portCode;
		}
#endif
	}
}
