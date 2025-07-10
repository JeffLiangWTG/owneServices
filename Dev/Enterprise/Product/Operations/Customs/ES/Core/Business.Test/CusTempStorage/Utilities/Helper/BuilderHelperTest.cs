using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	public static class BuilderHelperTest
	{
		public static void SetValues(this TemporaryStoragePackedItem item, ZShort lineNum, ZString tariff, ZString cusCode, ZString description, ZDecimal grossWeight, bool isMissing = false)
		{
			item.API_LineNo = lineNum;
			item.IsMissing = isMissing;
			item.API_Tariff = tariff;
			item.API_ChemicalSubstanceCode = cusCode;
			item.API_GoodsDescription = description;
			item.API_GrossWeight = grossWeight;
			item.API_GrossWeightUQ = "KG";
			item.UCR = "UCRCode";
			item.PresentationDate = ZDateTime.BrettsBirthday;
		}

		public static void SetValues(this TemporaryStoragePack package, ZString type, ZString marks, ZInt qty)
		{
			package.APA_PackUQ = type;
			package.APA_MarksAndNumbers = marks;
			package.APA_PackQty = qty;
		}

		public static void SetLiabilityCalculationValues(this TemporaryStoragePackedItem item, ZString goodsOrigin, ZDecimal goodsValue, ZString goodsValueCurrency, ZDecimal customsQty, ZString customsUQ, ZDecimal customsQty2, ZString customsUQ2, ZDecimal customsQty3, ZString customsUQ3, ZString[] supplementaryCodes)
		{
			item.API_RN_NKGoodsOrigin = goodsOrigin;
			item.API_GoodsValue = goodsValue;
			item.API_RX_NKGoodsValueCurrency = goodsValueCurrency;
			item.API_CustomsQty = customsQty;
			item.API_CustomsUQ = customsUQ;
			item.API_CustomsQty2 = customsQty2;
			item.API_CustomsUQ2 = customsUQ2;
			item.API_CustomsQty3 = customsQty3;
			item.API_CustomsUQ3 = customsUQ3;

			foreach (var supplementaryCode in supplementaryCodes)
			{
				var newSupplementaryCode = item.AdditionalSupplementaryCodes.AddNew(supplementaryCode);
				newSupplementaryCode.CY_Type = EU.Business.CusCodeDataTypeList.Codes.SupplementaryCode;
				newSupplementaryCode.CY_ParentTableCode = "API";
			}
		}
	}
}
