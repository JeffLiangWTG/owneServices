
namespace Enterprise.Customs.GB.Chief
{
	public static class ChiefConstants
	{
		public const string DutyCode = "A00";
		public const string VatCode = "B00";
		public const string DeferredTaxCode = "F";
		public const string CusDecTypeXTC = "XTC";
		public const string CusResTypeRPA = "RPA";
		public const string CusResTypeRPB = "RPB";
		public const string GoodCusresResponse29 = "29";
		public const string BadCusresResponse27 = "27";
		public const string GoodRequestResponse11 = "11";
		public const string CusDecTypeDLU = "DLU";
		public const string CusDecTypeDES = "DES";
		public const string ChiefCcsukPimaPrefix = "CUKCTM98CHF";
		public const string ChiefCcsukPrintServerPima_Test = "CUKCTM98PRT001";
		public const string ChiefCcsukPrintServerPima_Live = "CUKCTM98000PRT";

		public const string UNH = "UNH";
		public const string UNB = "UNB";
		public const string E9AccountingFailureAdvice = "E9";
		public const string E2ImportAcceptance = "E2";
		public const string MessageTypeSeparatorInApplicationReference = "~";

		public const string ImmediateTaxPaymentMethodFlexibleAccountingSystem = "D";
		public const string ImmediateTaxPaymentMethodCash = "A";
		public const string PostponedVatAccounting = "G";

		public static class CountryCodes
		{
			public const string Serbia = "XS";
		}

		// Edifact GEI segments are identified by.....
		public static class GeiSegmentTypes
		{
			public const string ICS_ImportCustomsStatus = "ICS";
			public const string ROE_RouteOfEntry = "ROE";
			public const string SOE_StyleOfEntry = "SOE";
			public const string TYP_TypeOfEntry = "TYP";
			public const string CRC_CustomsResponseCode = "CRC";
			public const string OPN_OpenIndicator = "OPN";
		}

		public static class RffSegmentIdentifiers
		{
			/// <summary>
			/// Security reference
			/// </summary>
			public const string COF = "COF";

			/// <summary>
			/// Security turn
			/// </summary>
			public const string PB = "PB";

			/// <summary>
			/// Items deleted
			/// </summary>
			public const string ABX = "ABX";

			/// <summary>
			/// Previous RoE
			/// </summary>
			public const string ABR = "ABR";

			/// <summary>
			/// Items added
			/// </summary>
			public const string AFD = "AFD";

			/// User ref
			public const string TN = "TN";
			/// <summary>
			/// Inventory system
			/// </summary>
			public const string ACF = "ACF";
			/// <summary>
			/// Entry key
			/// </summary>
			public const string AFM = "AFM";
			/// <summary>
			/// Entry type
			/// </summary>
			public const string ACD = "ACD";

			/// <summary>
			/// Current MUCR (for results of enqiries only)
			/// </summary>
			public const string FF = "FF";

			/// <summary>
			/// DUCR and part
			/// </summary>
			public const string ABO = "ABO";

			/// <summary>
			/// Entry number and version
			/// </summary>
			public const string ABT = "ABT";

			/// <summary>
			/// Movement reference number, MRN
			/// </summary>
			public const string AAE = "AAE";

			/// <summary>
			/// MUCR
			/// </summary>
			public const string UCN = "UCN";

			/// <summary>
			/// ICS - import clearnace status
			/// </summary>
			public const string ABS = "ABS";

			/// <summary>
			/// Route & style of entry - ROE & SOE
			/// </summary>
			public const string AHZ = "AHZ";

			/// <summary>
			/// Movement number.  
			/// </summary>
			public const string AES = "AES";

			/// <summary>
			/// First DAN
			/// </summary>
			public const string ABI = "ABI";

			/// <summary>
			/// Second DAN
			/// </summary>
			public const string DA = "DA";

			/// <summary>
			/// DLU - LI-REF Licence reference
			/// </summary>
			public const string EX = "EX";

			/// <summary>
			/// DLU - LI-TDR-ID - LIcence trader ID
			/// </summary>
			public const string ASM = "ASM";

			/// <summary>
			/// Specific Circumstance Indicator
			/// </summary>
			public const string AJK = "AJK";

			/// <summary>
			/// Container number
			/// </summary>
			public const string AAQ = "AAQ";

			/// <summary>
			/// United Nations Dangerous Goods - UNDG
			/// </summary>
			public const string UN = "UN";

			/// <summary>
			/// Misc description of goods
			/// </summary>
			public const string ZZZ = "ZZZ";
		}

		public static class MiscSegmentIdentifiers
		{
			/// <summary>
			/// Export control result
			/// </summary>
			public const string AJA = "AJA";

			/// <summary>
			/// Additional info 
			/// </summary>
			public const string ACB = "ACB";

			/// <summary>
			/// Gross mass
			/// </summary>
			public const string AAH = "AAH";

			/// <summary>
			/// Net mass
			/// </summary>
			public const string AAR = "AAR";
		}
	}
}
