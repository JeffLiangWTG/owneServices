using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class SAS148EventInfo : EventInfo
	{
		public SAS148EventInfo(IXmlEventValueObject eventDataObject) : base(eventDataObject)
		{
		}

		public ZString MasterBillIdentifier => masterBillIdentifier ?? (masterBillIdentifier = ContextCollection.GetZStringValue(Constants.MasterBillIdentifier) ?? ZString.Empty);
		public ZString DateTimeOfDeletion => dateTimeOfDeletion ?? (dateTimeOfDeletion = ContextCollection.GetZStringValue(Constants.DateTimeOfDeletion) ?? ZString.Empty);
		public ZString NewVesselName => VesselName;
		public ZString NewCarrierCode => CarrierCode;
		public ZString NewVesselCallSign => VesselCallSign;
		public ZString NewVoyageNumber => VoyageNumber;
		public ZString NewLoadingPortCode => LoadingPortCode;
		public ZString NewLoadingPortSuffix => LoadingPortSuffix;

		public ZDateTime ETA
		{
			get
			{
				if (eta == null)
				{
					var etaString = ContextCollection.GetZStringValue(Constants.TimeOfArrival) ?? ZString.Empty;
					eta = !etaString.IsEmpty && ZDateTime.TryParseISO8601Date(etaString, out var e) ? e : ZDateTime.Empty;
				}

				return eta.Value;
			}
		}

		public ZDateTime ETD
		{
			get
			{
				if (etd == null)
				{
					var etaString = ContextCollection.GetZStringValue(Constants.TimeOfDeparture) ?? ZString.Empty;
					etd = !etaString.IsEmpty && ZDateTime.TryParseISO8601Date(etaString, out var e) ? e : ZDateTime.Empty;
				}

				return etd.Value;
			}
		}

		public ZBool AreAllVesselDetailEmpty => (areAllVesselDetailEmpty ?? (areAllVesselDetailEmpty = NewCarrierCode.IsEmpty
																									  && NewVesselCallSign.IsEmpty
																									  && NewVoyageNumber.IsEmpty
																									  && NewLoadingPortCode.IsEmpty
																									  && NewLoadingPortSuffix.IsEmpty)).Value;
		public ZBool AreAllHouseBillDiscrepancyCodeEmpty => (areAllHouseBillDiscrepancyCodeEmpty ?? (areAllHouseBillDiscrepancyCodeEmpty = BillsToUpdate.All(x => x.DiscrepancyCode.IsEmpty))).Value;

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
							DiscrepancyCode =
								x.SubContextCollection?.GetZStringValue(Constants.DiscrepancyCode) ??
								ZString.Empty
						})
						.Where(x => !x.BillNo.IsEmpty)
						.ToArray();
				}
				return billsToUpdate;
			}
		}

		ZDateTime? eta, etd;
		string masterBillIdentifier;
		string dateTimeOfDeletion;
		ZBool? areAllVesselDetailEmpty;
		ZBool? areAllHouseBillDiscrepancyCodeEmpty;

		new ZString CarrierCode => base.CarrierCode;
		new ZString VesselCallSign => base.VesselCallSign;
		new ZString VoyageNumber => base.VoyageNumber;
		ZString VesselName => VesselDetailContext.VesselName ?? ZString.Empty;
		new ZString LoadingPortCode => base.LoadingPortCode;
		new ZString LoadingPortSuffix => base.LoadingPortSuffix;

		public class BillInfo
		{
			public ZString BillNo { get; set; }
			public ZString DiscrepancyCode { get; set; }
		}
	}
}
