using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	sealed class AFRHeaderDataEventContextReader
	{
		public AFRHeaderDataEventContextReader(JPAFRHeader header)
		{
			this.header = Argument.NotNull(header, "header");
		}

		public void AddEventContextValues(List<KeyValuePair<TypeWithDescription, IZType>> values, bool addFirstBill = true)
		{
			var helper = new EventContextValuesHelper(false, values);
			helper.AddMasterBillNumberAndPortCodes(header.JPH_MasterBillNumber, header.Loading, header.Discharge);

			values.AddIfNotEmpty(UniversalEvent.ContextTypes.VesselName, header.JPH_VesselName);
			values.AddIfNotEmpty(UniversalEvent.ContextTypes.VesselCallSign, header.JPH_RadioCallSign);
			values.AddIfNotEmpty(UniversalEvent.ContextTypes.LloydsNumber, header.Vessel?.RV_LloydsNumber ?? string.Empty);
			values.AddIfNotEmpty(UniversalEvent.ContextTypes.VoyageNumber, header.JPH_Voyage);

			if (addFirstBill)
			{
				var bill = header.Bills.FirstOrDefault();
				if (bill != null)
				{
					helper.AddHouseBillNumberAndPortCodes(bill.JPB_BillNumber, bill.Origin, bill.FinalDestination);
				}
			}
		}

		readonly JPAFRHeader header;
	}
}
