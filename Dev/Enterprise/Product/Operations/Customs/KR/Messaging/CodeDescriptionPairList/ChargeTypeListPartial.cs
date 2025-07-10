using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public partial class ChargeTypeList
	{
		public static string GetCorrespondingChargeType(ZString customsDutyTaxType)
		{
			var result = ZString.Empty;
			switch (customsDutyTaxType)
			{
				case EntryTaxTypeList.Codes.CUD:
					result = Codes.Duty;
					break;
				case EntryTaxTypeList.Codes.IND:
				case EntryTaxTypeList.Codes.CST:
					result = Codes.SpecialConsumptionTax;
					break;
				case EntryTaxTypeList.Codes.ENV:
				case EntryTaxTypeList.Codes._5AA:
					result = Codes.TransportationTax;
					break;
				case EntryTaxTypeList.Codes.ACT:
				case EntryTaxTypeList.Codes.TAC:
					result = Codes.LiquorTax;
					break;
				case EntryTaxTypeList.Codes._5AB:
					result = Codes.EducationTax;
					break;
				case EntryTaxTypeList.Codes.VAT:
					result = Codes.VAT;
					break;
				case EntryTaxTypeList.Codes.CAP:
				case EntryTaxTypeList.Codes._5DC:
				case EntryTaxTypeList.Codes._5CL:
					result = Codes.AgricultureTax;
					break;
				case EntryTaxTypeList.Codes._5AT:
				case EntryTaxTypeList.Codes._5AD:
				case EntryTaxTypeList.Codes._5AE:
				case EntryTaxTypeList.Codes._5AF:
				case EntryTaxTypeList.Codes._5AG:
				case EntryTaxTypeList.Codes._5AH:
				case EntryTaxTypeList.Codes._5AI:
				case EntryTaxTypeList.Codes._5AJ:
				case EntryTaxTypeList.Codes._5AU:
				case EntryTaxTypeList.Codes._5AV:
				case EntryTaxTypeList.Codes._5AW:
				case EntryTaxTypeList.Codes._5AX:
				case EntryTaxTypeList.Codes._5AS:
				case EntryTaxTypeList.Codes._5AZ:
				case EntryTaxTypeList.Codes._5BA:
				case EntryTaxTypeList.Codes._5BB:
					result = Codes.PenaltyAndInterest;
					break;
				case EntryTaxTypeList.Codes._5AK:
				case EntryTaxTypeList.Codes._5CT:
				case EntryTaxTypeList.Codes._5AC:
				case EntryTaxTypeList.Codes._5AY:
				case EntryTaxTypeList.Codes._5CS:
					result = Codes.PenaltyForLateOrMissedDeclaration;
					break;
			}
			return result;
		}

		public static ZString[] GetDomesticTaxTypes()
		{
			return new ZString[]
			{
				Codes.LiquorTax,
				Codes.AgricultureTax,
				Codes.TransportationTax,
				Codes.SpecialConsumptionTax,
				Codes.EducationTax,
				Codes.VAT,
			};
		}
	}
}
