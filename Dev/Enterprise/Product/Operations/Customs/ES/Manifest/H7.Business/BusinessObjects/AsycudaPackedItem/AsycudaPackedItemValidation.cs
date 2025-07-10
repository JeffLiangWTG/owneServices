using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.Business.Extensions;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class AsycudaPackedItemValidation : EU.H7.Business.AsycudaPackedItemValidation
	{
		public AsycudaPackedItemValidation(AsycudaPackedItem parent)
			: base(parent)
		{
		}

		protected new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

		protected override bool CheckTotalIntrinsicValueOfGoodsIsExceed(out string messageOfValueExceed)
		{
			var bill = Parent.Bill;
			if (bill.ABL_Procedure == ESH7AdditionalProcedureCodeList.Codes.C08)
			{
				if (Parent.Header != null && Parent.Header.LodgementCustomsOfficeInCanaryIsland)
				{
					bool exporterInEUOrCeutaOrMelilla = Parent.Factory.IsMemberOfEU(bill.ABL_RN_NKShipperCountry) || bill.ABL_RN_NKShipperCountry == Core.Constants.NonStandardCountryCodes.Codes.XC || bill.ABL_RN_NKShipperCountry == Core.Constants.NonStandardCountryCodes.Codes.XL;

					if (exporterInEUOrCeutaOrMelilla && bill.SumOfGoodsValue > 110m)
					{
						messageOfValueExceed = Res.GetString("5a3e5733-594a-4624-8226-37aa5609a351", "Sum of Intrinsic Value (Items) must not exceed EUR 110 when Add. Procedure(s) is C08 and Customs Office (Lodgement) is in the Canary Islands and Exporter Country/Region is in the EU, Ceuta or Melilla.");
						return true;
					}
					else if (!exporterInEUOrCeutaOrMelilla && bill.SumOfGoodsValue > 45m)
					{
						messageOfValueExceed = Res.GetString("be387ade-832c-41a7-9b22-64cd1b1840d5", "Sum of Intrinsic Value (Items) must not exceed EUR 45 when Add. Procedure(s) is C08 and Customs Office (Lodgement) is in the Canary Islands and Exporter Country/Region is not in the EU, Ceuta or Melilla.");
						return true;
					}
				}
				else
				{
					if (bill.SumOfGoodsValue > 45m)
					{
						messageOfValueExceed = Res.GetString("eb745af5-ef0c-4058-b065-ac8a664146a0", "Sum of Intrinsic Value (Items) must not exceed EUR 45 when Add. Procedure(s) is C08 and Customs Office (Lodgement) is not in the Canary Islands.");
						return true;
					}
				}

				messageOfValueExceed = string.Empty;
				return false;
			}

			return base.CheckTotalIntrinsicValueOfGoodsIsExceed(out messageOfValueExceed);
		}

		protected override void CheckAPI_GoodsValue()
		{
			base.CheckAPI_GoodsValue();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.API_GoodsValueInfo);
		}

		protected override void CheckAPI_CustomsQty2()
		{
			base.CheckAPI_CustomsQty2();
			if (Parent.Bill.ABL_Procedure == ESH7AdditionalProcedureCodeList.Codes.C08 && TariffPrefixes.Any(Parent.API_Tariff.StartsWith))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.API_CustomsQty2Info);
			}
		}

		protected override void CheckAPI_GrossWeight()
		{
			base.CheckAPI_GrossWeight();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.API_GrossWeightInfo);
			if (Procedures07.Contains(Parent.Bill.ABL_Procedure) && !Parent.API_GrossWeight.IsEmpty && !Parent.API_GrossWeightUQ.IsEmpty)
			{
				var convertedWeight = Parent.API_GrossWeight;
				if (Parent.API_GrossWeightUQ != Constants.Weight.Kilograms)
				{
					convertedWeight = Constants.Weight.Convert(convertedWeight, Parent.API_GrossWeightUQ, Constants.Weight.Kilograms);
				}

				if (convertedWeight > 100)
				{
					Parent.API_GrossWeightInfo.AddMessageError(GrossWeightNotExceeding100KgMessage);
				}
			}
		}

		protected string GrossWeightNotExceeding100KgMessage => Res.GetString("06829475-f097-4b1f-ad3e-c7e3b4d217a7", "Gross Weight must not exceed 100 KG.");

		protected readonly List<string> TariffPrefixes = ["2204", "2205", "2206", "2207", "2208", "240210", "240220", "2401", "2403", "3303"];

		protected readonly List<string> Procedures07 = [ESH7AdditionalProcedureCodeList.Codes.C07, ESH7AdditionalProcedureCodeList.Codes.C07F48, ESH7AdditionalProcedureCodeList.Codes.C07F49];
	}
}
