using System;
using System.Collections.Generic;
using Enterprise.Client.UPE.Business.Asycuda;
using Enterprise.Environment;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class BISIUploadRecord
	{
		public int LineCount
		{
			get
			{
				int result = _200000Lines.Count + _300000Lines.Count + _400000Lines.Count + _500000Lines.Count;
				return (_100000Line != null) ? result + 1 : result;
			}
		}

		public void SetShipmentDetailsLine(IShipmentData shipmentData)
		{
			_100000Line = new ShipmentDetailsLine(shipmentData);
			PopulateShipmentCountryDataLinesFromData(shipmentData);
			PopulateCommodityDetailLinesFromData(shipmentData);
			PopulateShipmentChargeLinesFromData(shipmentData);
		}

		public bool HasShipmentDetailLine()
		{
			return _100000Line != null;
		}

		public void AddShipmentStatusLine(IShipmentStatusData shipmentStatusData)
		{
			_200000Lines.Add(new ShipmentStatusLine(shipmentStatusData));
		}

		#region Implementation

		public List<ShipmentStatusLine> _200000Lines
		{
			get
			{
				if (f200000Lines == null)
				{
					f200000Lines = new List<ShipmentStatusLine>();
				}
				return f200000Lines;
			}
		}
		List<ShipmentStatusLine> f200000Lines;

		public List<ShipmentReceiptLine> _300000Lines
		{
			get
			{
				if (f300000Lines == null)
				{
					f300000Lines = new List<ShipmentReceiptLine>();
				}
				return f300000Lines;
			}
		}
		List<ShipmentReceiptLine> f300000Lines;

		public List<CommodityDetailLine> _400000Lines
		{
			get
			{
				if (f400000Lines == null)
				{
					f400000Lines = new List<CommodityDetailLine>();
				}
				return f400000Lines;
			}
		}
		List<CommodityDetailLine> f400000Lines;

		public List<ShipmentChargeLine> _500000Lines
		{
			get
			{
				if (f500000Lines == null)
				{
					f500000Lines = new List<ShipmentChargeLine>();
				}
				return f500000Lines;
			}
		}
		List<ShipmentChargeLine> f500000Lines;

		void PopulateShipmentCountryDataLinesFromData(IShipmentData shipmentData)
		{
			_300000Lines.Clear();

			foreach (var receiptData in shipmentData.ReceiptsData)
			{
				_300000Lines.Add(new ShipmentReceiptLine(shipmentData, receiptData));
			}
		}

		void PopulateCommodityDetailLinesFromData(IShipmentData shipmentData)
		{
			_400000Lines.Clear();
			foreach (CommodityDetailData commodityData in shipmentData.CommoditiesData)
			{
				_400000Lines.Add(new CommodityDetailLine(shipmentData, commodityData, _400000Lines.Count));
			}
		}

		void PopulateShipmentChargeLinesFromData(IShipmentData shipmentData)
		{
			_500000Lines.Clear();
			foreach (ShipmentChargeData chargeData in shipmentData.ChargesData)
			{
				var line = new ShipmentChargeLine(shipmentData, chargeData);
				line.IsCustomCharge = false;
				_500000Lines.Add(line);
			}

			var countryCode = Env.CurrentCompany.Country.Code;
			if (countryCode == Core.Constants.CountryCodes.Singapore)
			{
				var tradeNet = shipmentData as TradeNetIShipmentDataProxy;
				if (tradeNet != null)
				{
					var helper = tradeNet.ShipmentDataProxyHelper;
					PopulateCustomCharge(shipmentData, helper);
				}
				else
				{
					var asycuda = shipmentData as AsycudaIShipmentDataProxy;
					if (asycuda != null)
					{
						var helper = asycuda.ShipmentDataProxyHelper;
						PopulateCustomCharge(shipmentData, helper);
					}
				}
			}
		}

		void PopulateCustomCharge(IShipmentData shipmentData, IShipmentDataProxyHelper helper)
		{
			if (helper.CustomCharge1 > 0)
			{
				var chargeData = new ShipmentChargeData((ShipmentChargeTypeCode)Enum.Parse(typeof(ShipmentChargeTypeCode), helper.CustomCode1), helper.CustomCharge1, Core.Constants.CurrencyCodes.Singapore);
				var line = new ShipmentChargeLine(shipmentData, chargeData);
				line.IsCustomCharge = true;
				_500000Lines.Add(line);
			}
			if (helper.CustomCharge2 > 0)
			{
				var chargeData = new ShipmentChargeData((ShipmentChargeTypeCode)Enum.Parse(typeof(ShipmentChargeTypeCode), helper.CustomCode2), helper.CustomCharge2, Core.Constants.CurrencyCodes.Singapore);
				var line = new ShipmentChargeLine(shipmentData, chargeData);
				line.IsCustomCharge = true;
				_500000Lines.Add(line);
			}
			if (helper.CustomCharge3 > 0)
			{
				var chargeData = new ShipmentChargeData((ShipmentChargeTypeCode)Enum.Parse(typeof(ShipmentChargeTypeCode), helper.CustomCode3), helper.CustomCharge3, Core.Constants.CurrencyCodes.Singapore);
				var line = new ShipmentChargeLine(shipmentData, chargeData);
				line.IsCustomCharge = true;
				_500000Lines.Add(line);
			}
		}

		public ShipmentDetailsLine _100000Line;

		#endregion
	}
}
