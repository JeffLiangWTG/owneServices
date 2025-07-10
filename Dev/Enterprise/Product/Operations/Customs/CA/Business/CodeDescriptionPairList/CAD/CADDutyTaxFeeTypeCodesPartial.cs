using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	partial class CADDutyTaxFeeTypeCodes
	{
		public static bool IsTaxCode(ZString code)
		{
			var result = false;
			switch (code)
			{
				case Codes.AAD:
				case Codes.AAI:
				case Codes.FET:
				case Codes.GST:
				case Codes.OTH:
				case Codes.PAT:
				case Codes.SUR:
				case Codes.TAC:
					result = true;
					break;
				default:
					break;
			}
			return result;
		}

		public static ZString ConvertDutyAndTaxType(ZString type)
		{
			var result = type;
			switch (type)
			{
				case DutyAndTaxTypes.Codes.ADD:
					result = CADDutyTaxFeeTypeCodes.Codes.ADD;
					break;
				case DutyAndTaxTypes.Codes.CVD:
					result = CADDutyTaxFeeTypeCodes.Codes.CVD;
					break;
				case DutyAndTaxTypes.Codes.GST:
					result = CADDutyTaxFeeTypeCodes.Codes.GST;
					break;
				case DutyAndTaxTypes.Codes.SUR:
					result = CADDutyTaxFeeTypeCodes.Codes.SUR;
					break;
				case DutyAndTaxTypes.Codes.SIMADuty:
				case DutyAndTaxTypes.Codes.CTA:
					break;
				case DutyAndTaxTypes.Codes.ExciseTax:
					result = CADDutyTaxFeeTypeCodes.Codes.FET;
					break;
				case DutyAndTaxTypes.Codes.CPT:
					result = CADDutyTaxFeeTypeCodes.Codes.AAI;
					break;
				case DutyAndTaxTypes.Codes.CustomsDuty:
					result = CADDutyTaxFeeTypeCodes.Codes.CUD;
					break;
				case DutyAndTaxTypes.Codes.SAF:
					result = CADDutyTaxFeeTypeCodes.Codes.OTH;
					break;
				default:
					break;
			}
			return result;
		}
	}
}
