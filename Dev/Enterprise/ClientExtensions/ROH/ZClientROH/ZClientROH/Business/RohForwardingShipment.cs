using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.Rohlig
{
	public class RohForwardingShipment : ForwardingShipment
	{
		public RohForwardingShipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static new RohForwardingShipment New(BusinessObjectFactory factory)
		{
			return (RohForwardingShipment)factory.New(typeof(RohForwardingShipment));
		}

		public override DocumentSupporter DocumentSupporter
		{
			get { return new RohForwardingShipmentDocumentSupporter(this); }
		}
	}

	public class RohForwardingShipmentDocumentSupporter : ForwardingShipmentDocumentSupporter
	{
		public RohForwardingShipmentDocumentSupporter(RohForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docWrapper)
		{
			string result;

			if (CurrentCommand != null && CurrentCommand.SU_MenuName.EndsWith("Cartage Advice With Receipt") && filterType == MenuTemplateFilterType.PrintStandard)
			{
				result = ZBool.False.ToString();
			}
			else
			{
				result = base.GetMenuTemplateFilterValue(filterType, docWrapper);
			}

			return result;
		}

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			CurrentCommand = commandAboutToBeRun; // need this to determine the selected menu.
			return base.GetDataStateBeforeRun(commandAboutToBeRun);
		}

		public
		IStmMenuItem CurrentCommand;
	}
}
