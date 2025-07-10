using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.Management
{
	public class EventContextValuesHelper
	{
		public EventContextValuesHelper(bool isAir, List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			this.isAir = isAir;
			this.contextValues = contextValues;
		}

		readonly bool isAir;
		readonly List<KeyValuePair<TypeWithDescription, IZType>> contextValues;

		public void AddMasterBillNumberAndPortCodes(ZString masterBill, IRefUNLOCO loadPort, IRefUNLOCO dischargePort)
		{
			if (isAir)
			{
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.MAWBNumber, masterBill.FormatAirMAWB());
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.MAWBOriginIATAAirportCode, loadPort.GetIATACode());
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.MAWBDestinationIATAAirportCode, dischargePort.GetIATACode());
			}
			else
			{
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.MBOLNumber, masterBill);
			}

			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.MBOLOriginUNLOCO, loadPort.GetUNLOCO());
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.MBOLDestinationUNLOCO, dischargePort.GetUNLOCO());
		}

		public void AddHouseBillNumberAndPortCodes(ZString houseBill, IRefUNLOCO originPort, IRefUNLOCO destinationPort)
		{
			if (isAir)
			{
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.HAWBNumber, houseBill);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.HAWBOriginIATAAirportCode, originPort.GetIATACode());
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.HAWBDestinationIATAAirportCode, destinationPort.GetIATACode());
			}
			else
			{
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.HBOLNumber, houseBill);
			}

			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.HBOLOriginUNLOCO, originPort.GetUNLOCO());
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.HBOLDestinationUNLOCO, destinationPort.GetUNLOCO());
		}

		public void AddCustomsStatus(ZString customsStatus)
		{
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.ComplianceStatus, customsStatus);
		}
	}
}
