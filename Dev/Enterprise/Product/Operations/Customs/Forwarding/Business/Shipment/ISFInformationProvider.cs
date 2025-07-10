using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.Forwarding.Business
{
	class ISFInformationProvider
	{
		public ISFInformationProvider(ForwardingShipment shipment)
		{
			Argument.NotNull(shipment, "shipment");

			this.shipment = shipment;
		}

		readonly ForwardingShipment shipment;

		public ZString ISFBillNumber
		{
			get
			{
				var isfBillData = ISFBillData;
				return isfBillData == null ? ZString.Empty : isfBillData.BillNumber;
			}
		}

		public ZString ISFBillStatus
		{
			get
			{
				var isfBillData = ISFBillData;
				return isfBillData == null ? ZString.Empty : isfBillData.Status;
			}
		}

		ISFBillData ISFBillData
		{
			get
			{
				if (iSFBillDataCached == null)
				{
					iSFBillDataCached = new CachedProperty<ISFBillData>(shipment.Factory, () =>
					{
						ISFBillData result = null;
						if (shipment.JS_TransportMode == Constants.TransportModes.Sea || shipment.JS_TransportMode == Constants.TransportModes.SeaAir || shipment.JS_TransportMode == Constants.TransportModes.AirSea)
						{
							var isfBillNums = GetISFBillNums();

							if (isfBillNums.Length > 0)
							{
								result = ISFStatusHelper.GetISFBillData(shipment.Factory, false, isfBillNums, shipment.JS_SystemCreateTimeUtc);
							}
						}

						return result;
					});
				}
				return iSFBillDataCached.Value;
			}
		}
		CachedProperty<ISFBillData> iSFBillDataCached;

		ZString[] GetISFBillNums()
		{
			var result = new List<ZString>();

			var bill = shipment.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS);
			if (bill != null && !bill.CE_EntryNum.IsEmpty)
			{
				result.Add(bill.CE_EntryNum);
			}
			else
			{
				if (!shipment.JS_HouseBill.IsEmpty && !shipment.Consols.Cast<ForwardingConsol>().Any(consol => consol.IsDirect))
				{
					AddISFBillNumsFromBillDetails(shipment, result);

					if (shipment.JS_HouseBill.Length <= 12)
					{
						var orgProxySCACs = shipment.Factory.GetOrgProxySCACs();
						if (orgProxySCACs != null)
						{
							foreach (var scac in orgProxySCACs.Where(s => !s.IsEmpty))
							{
								var number = scac + shipment.JS_HouseBill;

								if (!result.Contains(number))
								{
									result.Add(number);
								}
							}
						}
					}
				}
				else
				{
					foreach (ForwardingConsol consol in shipment.Consols)
					{
						if (consol.IsDirect && consol.JK_TransportMode == Constants.TransportModes.Sea)
						{
							var consolAMSBill = consol.Numbers.GetFirstReferenceNumberByType(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS);
							if (consolAMSBill != null && !consolAMSBill.CE_EntryNum.IsEmpty)
							{
								result.Add(consolAMSBill.CE_EntryNum);
							}
							else
							{
								AddISFBillNumsFromBillDetails(consol, result);
							}
						}
					}
				}
				result.Sort((x, y) => x.CompareTo(y));
			}

			return result.ToArray();
		}

		static void AddISFBillNumsFromBillDetails(IBillDetails billDetails, List<ZString> billNums)
		{
			if (billDetails != null)
			{
				var billNumberInfo = billDetails.BillNumberInfo;
				var billNumber = billNumberInfo != null ? (ZString)billNumberInfo.Value : ZString.Empty;

				if (!billNumber.IsEmpty)
				{
					if (!billNums.Contains(billNumber))
					{
						billNums.Add(billNumber);
					}

					if (billNumber.Length <= 12)
					{
						foreach (var org in billDetails.SCACIssuers)
						{
							var scacs = org.USLocalCustomsCarrierCodes();
							foreach (var scac in scacs)
							{
								var number = scac + billNumber;

								if (!scac.IsEmpty && !billNums.Contains(number))
								{
									billNums.Add(number);
								}
							}
						}
					}
				}
			}
		}

		public ZString ISFBillStatusDescription
		{
			get
			{
				var isfBillData = ISFBillData;
				return isfBillData == null ? ZString.Empty : isfBillData.StatusDescription;
			}
		}
	}
}
