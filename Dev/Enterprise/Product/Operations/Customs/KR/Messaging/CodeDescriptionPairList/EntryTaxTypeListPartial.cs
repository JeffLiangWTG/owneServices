using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public partial class EntryTaxTypeList
	{
		public static string GetCorrespondingChargeTypeCW1ToKRCFor5UL(ZString dutyTaxTypeCW1)
		{
			var result = ZString.Empty;
			switch (dutyTaxTypeCW1)
			{
				case ChargeTypeList.Codes.Duty:
					result = Codes.CUD;
					break;
				case ChargeTypeList.Codes.EducationTax:
					result = Codes._5AB;
					break;
				case ChargeTypeList.Codes.AgricultureTax:
					result = Codes.CAP;
					break;
				case ChargeTypeList.Codes.VAT:
					result = Codes.VAT;
					break;
				case ChargeTypeList.Codes.LiquorTax:
					result = Codes.ACT;
					break;
				case ChargeTypeList.Codes.SpecialConsumptionTax:
					result = Codes.IND;
					break;
				case ChargeTypeList.Codes.TransportationTax:
					result = Codes.ENV; // 5AA Same (929)
					break;
				case ChargeTypeList.Codes.PenaltyForLateDeclaration:
					result = Codes._5AC;
					break;
				case ChargeTypeList.Codes.PenaltyForMissedDeclaration:
					result = Codes._5AY;
					break;
			}
			return result;
		}
		public static bool Is5ULPenaltyChargeType(string code)
		{
			return code == EntryTaxTypeList.Codes._5AD
				|| code == EntryTaxTypeList.Codes._5AE
				|| code == EntryTaxTypeList.Codes._5AF
				|| code == EntryTaxTypeList.Codes._5AG
				|| code == EntryTaxTypeList.Codes._5AH
				|| code == EntryTaxTypeList.Codes._5AI
				|| code == EntryTaxTypeList.Codes._5AJ;
		}
		public static string GetCustomsChargeTypeByPenaltyType(ZString dutyPenaltyType)
		{
			var result = ZString.Empty;
			switch (dutyPenaltyType)
			{
				case Codes._5AD:
					result = Codes.CUD;
					break;
				case Codes._5AE:
					result = Codes.IND;
					break;
				case Codes._5AF:
					result = Codes.ACT;
					break;
				case Codes._5AG:
					result = Codes.ENV; 
					break;
				case Codes._5AH:
					result = Codes.VAT;
					break;
				case Codes._5AI:
					result = Codes._5AB;
					break;
				case Codes._5AJ:
					result = Codes.CAP;
					break;
			}
			return result;
		}
	}
}
