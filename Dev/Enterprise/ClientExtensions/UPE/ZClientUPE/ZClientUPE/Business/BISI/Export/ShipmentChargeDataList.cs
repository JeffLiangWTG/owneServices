using System.Collections;
using System.Linq;

using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class ShipmentChargeDataList
	{
		public int Count
		{
			get { return Charges.Count; }
		}

		public void AddCharge(string chargeDescription, ZDecimal grossAmount)
		{
			AddCharge(ConvertToUPEChargeTypeCode(chargeDescription), grossAmount, Core.Constants.CurrencyCodes.Australia);
		}

		public void AddCharge(ShipmentChargeTypeCode typeCode, ZDecimal grossAmount, ZString currencyCode)
		{
			if (typeCode != ShipmentChargeTypeCode.Unknown)
			{
				if (Charges.ContainsKey(typeCode))
				{
					ShipmentChargeData charge = Charges[typeCode] as ShipmentChargeData;
					charge.AddAmount(grossAmount);
				}
				else
				{
					Charges.Add(typeCode, new ShipmentChargeData(typeCode, grossAmount, currencyCode));
				}
			}
		}

		public void AddCharge(params ShipmentChargeData[] chargesData)
		{
			foreach (ShipmentChargeData chargeData in chargesData)
			{
				AddCharge(chargeData.TypeCodeEnum, chargeData.GrossAmount, Core.Constants.CurrencyCodes.Australia);
			}
		}

		public ShipmentChargeData[] ToArray()
		{
			return Charges.Values.OfType<ShipmentChargeData>().OrderBy(x => x.TypeCode).ToArray();
		}

		#region Implementation

		ShipmentChargeTypeCode ConvertToUPEChargeTypeCode(string description)
		{
			ShipmentChargeTypeCode result;

			switch (description)
			{
				case CusEntryChargeTypeList.Descriptions.DutyAmount:
					result = ShipmentChargeTypeCode.Duty;
					break;

				case CusEntryChargeTypeList.Descriptions.GSTAmount:
					result = ShipmentChargeTypeCode.GST;
					break;

				case CusEntryChargeTypeList.Descriptions.LCTAmount:
				case CusEntryChargeTypeList.Descriptions.WetAmount:
				case CusEntryChargeTypeList.Descriptions.Woodlevy:
				case CusEntryChargeTypeList.Descriptions.TotalPayableAdmin:
					result = ShipmentChargeTypeCode.Other;
					break;

				case CusEntryChargeTypeList.Descriptions.DeclarationProcessingCharge:
				case CusEntryChargeTypeList.Descriptions.AQISProcessingCharge:
					result = ShipmentChargeTypeCode.Tradegate;
					break;

				case ShipmentChargeDescription.VAT:
					result = ShipmentChargeTypeCode.VAT;
					break;

				default:
					result = ShipmentChargeTypeCode.Unknown;
					break;
			}

			return result;
		}

		Hashtable Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new Hashtable();
				}
				return fCharges;
			}
		}

		Hashtable fCharges;

		#endregion
	}
}
