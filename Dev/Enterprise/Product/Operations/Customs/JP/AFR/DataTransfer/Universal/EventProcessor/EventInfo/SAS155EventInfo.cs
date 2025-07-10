using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class SAS155EventInfo : EventInfo
	{
		public SAS155EventInfo(IXmlEventValueObject eventDataObject) : base(eventDataObject)
		{
		}

		public ZBool IsNON => (isNON ?? (isNON = BillsToUpdate.Count == 1 && billsToUpdate.All(x => x.ProcessResultCode.IsEmpty && x.BillNo == NONBillNO))).Value;
		public ZString DischargeCode => dischargePortCode ?? (dischargePortCode = ContextCollection.GetZStringValue(Constants.PortOfDischargeCode) ?? ZString.Empty);
		public ZString NewCarrierCode => newCarrierCode ?? (newCarrierCode = ContextCollection.GetZStringValue(Constants.CarrierCodeNew) ?? ZString.Empty);
		public ZString NewVesselCallSign => newVesselCallSign ?? (newVesselCallSign = ContextCollection.GetZStringValue(Constants.VesselCallSignNew) ?? ZString.Empty);
		public ZString NewVoyageNumber => newVoyageNumber ?? (newVoyageNumber = ContextCollection.GetZStringValue(Constants.VoyageNumberNew) ?? ZString.Empty);
		public ZString NewLoadingPortCode => newLoadingPortCode ?? (newLoadingPortCode = ContextCollection.GetZStringValue(Constants.PortOfLoadingUNLOCONew) ?? ZString.Empty);
		public ZString NewLoadingPortSuffix => newLoadingPortSuffix ?? (newLoadingPortSuffix = ContextCollection.GetZStringValue(Constants.PortOfLoadingSuffixNew) ?? ZString.Empty);

		BillInfo[] billsToUpdate;
		public IReadOnlyList<BillInfo> BillsToUpdate
		{
			get
			{
				if (billsToUpdate == null)
				{
					billsToUpdate = BillInfoContexts
						.Select(x => new BillInfo
						{
							BillNo = x.SubContextCollection?.GetZStringValue(Constants.BillNumber) ??
									 ZString.Empty,
							ProcessResultCode =
							x.SubContextCollection?.GetZStringValue(Constants.ProcessResultCode) ??
							ZString.Empty
						})
						.Where(x => !x.BillNo.IsEmpty)
						.ToArray();
				}
				return billsToUpdate;
			}
		}

		ZBool? isNON;
		string dischargePortCode;
		string newCarrierCode;
		string newVesselCallSign;
		string newVoyageNumber;
		string newLoadingPortCode;
		string newLoadingPortSuffix;

		const string NONBillNO = "NON";

		public class BillInfo
		{
			public ZString BillNo { get; set; }
			public ZString ProcessResultCode { get; set; }
		}
	}
}
