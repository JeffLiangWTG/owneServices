using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.EU.H7.Business.UniversalDataTransfer
{
	public class AsycudaBillEventContextReader : ASYCUDA.Business.UniversalDataTransfer.AsycudaBillEventContextReader
	{
		public AsycudaBillEventContextReader(ASYCUDA.Business.AsycudaBill bill) : base(bill)
		{
		}

		protected override void AddEventContextValuesCore(List<KeyValuePair<TypeWithDescription, IZType>> values)
		{
			values.AddIfNotEmpty(Event.ContextTypes.MAWBNumber, bill.Header.MasterBill.ABL_BillNumber);
			values.AddIfNotEmpty(Event.ContextTypes.HAWBNumber, bill.ABL_BillNumber);
			values.AddIfNotEmpty(Event.ContextTypes.ComplianceStatus, bill.ABL_BillStatus);
		}
	}
}
