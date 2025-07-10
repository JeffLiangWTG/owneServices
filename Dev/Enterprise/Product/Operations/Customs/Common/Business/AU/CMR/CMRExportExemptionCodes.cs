using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public class CMRExportExemptionCodes : CodeDescriptionPair
	{
		protected CMRExportExemptionCodes(object code, MultilingualString description)
			: base(code, description)
		{
		}

		public static readonly CMRExportExemptionCodes EXPE = new CMRExportExemptionCodes("EXPE", ResString.GetMultilingualString("CMRExportExemptionCodes|EXPE", "Unaccompanied Personal Effects"));
		public static readonly CMRExportExemptionCodes EXLV = new CMRExportExemptionCodes("EXLV", ResString.GetMultilingualString("CMRExportExemptionCodes|EXLV", "Goods under the value of $2000 (AUD)"));
		public static readonly CMRExportExemptionCodes EXTI = new CMRExportExemptionCodes("EXTI", ResString.GetMultilingualString("CMRExportExemptionCodes|EXTI", "Carnet or Tryptique. Temporary imports"));
		public static readonly CMRExportExemptionCodes EXML = new CMRExportExemptionCodes("EXML", ResString.GetMultilingualString("CMRExportExemptionCodes|EXML", "Australia Post or Diplomatic Mail Bags"));
		public static readonly CMRExportExemptionCodes EXDC = new CMRExportExemptionCodes("EXDC", ResString.GetMultilingualString("CMRExportExemptionCodes|EXDC", "Australian Domestic Cargo"));
		public static readonly CMRExportExemptionCodes EXSP = new CMRExportExemptionCodes("EXSP", ResString.GetMultilingualString("CMRExportExemptionCodes|EXSP", "Australian Aircraft Spares"));
		public static readonly CMRExportExemptionCodes EXDD = new CMRExportExemptionCodes("EXDD", ResString.GetMultilingualString("CMRExportExemptionCodes|EXDD", "Military goods. Owned by Australian Government"));

		public const int Char4CodeMaxLength = 4;

		public static ZString GetFromExit2Exemption(ZString code)
		{
			ZString result = ZString.Empty;
			switch (code)
			{
				case CusEntryNumberTypes.Australia.EX1:
					result = "EXPE";
					break;
				case CusEntryNumberTypes.Australia.EX2:
					result = "EXLV";
					break;
				case CusEntryNumberTypes.Australia.EX3:
					result = "EXTI";
					break;
				case CusEntryNumberTypes.Australia.EX5:
					result = "EXTI";
					break;
				case CusEntryNumberTypes.Australia.EXA:
					result = "EXML";
					break;
				case CusEntryNumberTypes.Australia.EXB:
					result = "EXDC";
					break;
				case CusEntryNumberTypes.Australia.EXC:
					result = "EXSP";
					break;
				default:
					result = code;
					break;
			}
			return result;
		}

		public static ZString Get3CharCode(ZString code)
		{
			ZString result = code;
			switch (code)
			{
				case "EXDC":
					result = "XDC";
					break;
				case "EXDD":
					result = "XDD";
					break;
				case "EXLV":
					result = "XLV";
					break;
				case "EXML":
					result = "XML";
					break;
				case "EXPE":
					result = "XPE";
					break;
				case "EXSP":
					result = "XSP";
					break;
				case "EXTI":
					result = "XTI";
					break;
				default:
					result = code;
					break;
			}
			return result;
		}

		public static ZString Get4CharCode(ZString code)
		{
			ZString result = code;
			switch (code)
			{
				case "XDC":
					result = "EXDC";
					break;
				case "XDD":
					result = "EXDD";
					break;
				case "XLV":
					result = "EXLV";
					break;
				case "XML":
					result = "EXML";
					break;
				case "XPE":
					result = "EXPE";
					break;
				case "XSP":
					result = "EXSP";
					break;
				case "XTI":
					result = "EXTI";
					break;
				default:
					result = code;
					break;
			}
			return result;
		}
	}
}

