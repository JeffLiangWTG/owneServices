using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	static class ArgentinaConstants
	{
		#region SuppressResourceStringsCheckRegion

		public const int UnitOfMeasure = 7;
		public const int Quantity = 1;
		public const int UnitOfRefrence = 1;
		public const string IdIsAnnulment = "22";
		public const string IdTransferModality = "27";
		public const string IdCBU = "2101";
		public const string IdCBUItemDetail = "21";
		public const string MoneyFormat = "F2";

		#region TaxGroupsMessageCode

		public static HashSet<string> GetTaxGroupsCodeList
		{
			get
			{
				return new HashSet<string>()
				{
					ArgentinaComplianceInfo.TaxMessageGroupCodes.N3,
					ArgentinaComplianceInfo.TaxMessageGroupCodes.N4,
					ArgentinaComplianceInfo.TaxMessageGroupCodes.N5,
					ArgentinaComplianceInfo.TaxMessageGroupCodes.N6
				};
			}
		}

		#endregion

		#region CbtesAsocComplianceSubTypes

		public static readonly ImmutableHashSet<string> DebitOrCreditNoteComplianceSubTypeList = new HashSet<string>()
		{
			ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDA,
			ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCA,
			ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDB,
			ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCB,
			ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDC,
			ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCC,
			ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDM,
			ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCM,
			ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDA,
			ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCA,
			ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDB,
			ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB,
			ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDC,
			ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCC,
			ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCE,
			ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDE,
		}.ToImmutableHashSet();

		#endregion

		public const string TypeOfExpo = "2";

		public static readonly ImmutableDictionary<string, short> ArcaIvaCategories = new Dictionary<string, short>
		{
			{ "IVX", 9 },
			{ "IVF", 5 },
			{ "IVE", 4 },
			{ "IVP", 8 },
			{ "IVI", 1 },
			{ "IVM", 6 },
			{ "IVS", 7 }
		}.ToImmutableDictionary();

		#region AfipCodesCountries

		public static class AfipCodesCountries
		{
			public static readonly ImmutableDictionary<string, string> AfipCodes = new Dictionary<string, string>()
			{
				{ "AF", "301" },
				{ "AX", "497" },
				{ "AL", "401" },
				{ "DZ", "102" },
				{ "AS", "695" },
				{ "AD", "404" },
				{ "AO", "149" },
				{ "AI", "652" },
				{ "AQ", "265" },
				{ "AG", "237" },
				{ "AR", "200" },
				{ "AM", "349" },
				{ "AW", "653" },
				{ "AU", "501" },
				{ "AT", "405" },
				{ "AZ", "350" },
				{ "BS", "239" },
				{ "BH", "303" },
				{ "BD", "345" },
				{ "BB", "201" },
				{ "BY", "439" },
				{ "BE", "406" },
				{ "BZ", "236" },
				{ "BJ", "112" },
				{ "BM", "663" },
				{ "BT", "305" },
				{ "BO", "202" },
				{ "BQ", "230" },
				{ "BA", "446" },
				{ "BW", "103" },
				{ "BR", "203" },
				{ "IO", "508" },
				{ "BN", "346" },
				{ "BG", "407" },
				{ "BF", "101" },
				{ "BI", "104" },
				{ "KH", "306" },
				{ "CM", "105" },
				{ "CA", "204" },
				{ "CV", "150" },
				{ "KY", "671" },
				{ "CF", "107" },
				{ "TD", "111" },
				{ "CL", "208" },
				{ "CN", "310" },
				{ "CX", "672" },
				{ "CC", "673" },
				{ "CO", "205" },
				{ "KM", "155" },
				{ "CG", "108" },
				{ "CD", "109" },
				{ "CK", "654" },
				{ "CR", "206" },
				{ "CI", "110" },
				{ "HR", "447" },
				{ "CU", "207" },
				{ "CW", "230" },
				{ "CY", "435" },
				{ "CZ", "451" },
				{ "DK", "409" },
				{ "DJ", "153" },
				{ "DM", "233" },
				{ "DO", "209" },
				{ "EC", "210" },
				{ "EG", "113" },
				{ "SV", "211" },
				{ "GQ", "119" },
				{ "ER", "160" },
				{ "EE", "440" },
				{ "ET", "161" },
				{ "FK", "254" },
				{ "FO", "228" },
				{ "FJ", "512" },
				{ "FI", "411" },
				{ "FR", "412" },
				{ "GF", "229" },
				{ "PF", "656" },
				{ "TF", "509" },
				{ "GA", "115" },
				{ "GM", "116" },
				{ "GE", "351" },
				{ "DE", "438" },
				{ "GH", "117" },
				{ "GI", "665" },
				{ "GR", "413" },
				{ "GL", "666" },
				{ "GD", "240" },
				{ "GP", "509" },
				{ "GU", "667" },
				{ "GT", "213" },
				{ "GG", "670" },
				{ "GN", "118" },
				{ "GW", "156" },
				{ "GY", "214" },
				{ "HT", "215" },
				{ "HM", "507" },
				{ "VA", "431" },
				{ "HN", "216" },
				{ "HK", "668" },
				{ "HU", "414" },
				{ "IS", "416" },
				{ "IN", "315" },
				{ "ID", "316" },
				{ "XZ", "997" },
				{ "IR", "318" },
				{ "IQ", "317" },
				{ "IE", "415" },
				{ "IM", "676" },
				{ "IL", "319" },
				{ "IT", "417" },
				{ "JM", "217" },
				{ "JP", "320" },
				{ "JE", "508" },
				{ "JO", "321" },
				{ "KZ", "352" },
				{ "KE", "120" },
				{ "KI", "514" },
				{ "KP", "308" },
				{ "KR", "309" },
				{ "XK", "497" },
				{ "KW", "323" },
				{ "KG", "353" },
				{ "LA", "324" },
				{ "LV", "441" },
				{ "LB", "325" },
				{ "LS", "121" },
				{ "LR", "122" },
				{ "LY", "123" },
				{ "LI", "418" },
				{ "LT", "442" },
				{ "LU", "419" },
				{ "MO", "344" },
				{ "MG", "124" },
				{ "MW", "125" },
				{ "MY", "326" },
				{ "MV", "327" },
				{ "ML", "126" },
				{ "MT", "420" },
				{ "MH", "520" },
				{ "MQ", "509" },
				{ "MR", "129" },
				{ "MU", "128" },
				{ "YT", "509" },
				{ "MX", "218" },
				{ "FM", "515" },
				{ "MD", "443" },
				{ "MC", "421" },
				{ "MN", "329" },
				{ "ME", "453" },
				{ "MS", "686" },
				{ "MA", "127" },
				{ "MZ", "151" },
				{ "MM", "304" },
				{ "NA", "158" },
				{ "NR", "503" },
				{ "NP", "330" },
				{ "NL", "423" },
				{ "NC", "509" },
				{ "NZ", "504" },
				{ "NI", "219" },
				{ "NE", "130" },
				{ "NG", "131" },
				{ "NU", "687" },
				{ "NF", "677" },
				{ "MK", "450" },
				{ "MP", "521" },
				{ "NO", "422" },
				{ "OM", "328" },
				{ "PK", "332" },
				{ "PW", "516" },
				{ "PS", "357" },
				{ "PA", "220" },
				{ "PG", "513" },
				{ "PY", "221" },
				{ "PE", "222" },
				{ "PH", "312" },
				{ "PN", "690" },
				{ "PL", "424" },
				{ "PT", "425" },
				{ "PR", "223" },
				{ "QA", "322" },
				{ "RE", "509" },
				{ "RO", "427" },
				{ "RU", "444" },
				{ "RW", "133" },
				{ "BL", "509" },
				{ "SH", "694" },
				{ "KN", "238" },
				{ "LC", "234" },
				{ "MF", "509" },
				{ "PM", "680" },
				{ "VC", "235" },
				{ "WS", "506" },
				{ "SM", "428" },
				{ "ST", "157" },
				{ "SA", "302" },
				{ "SN", "134" },
				{ "RS", "454" },
				{ "SC", "152" },
				{ "SL", "135" },
				{ "SG", "333" },
				{ "SX", "230" },
				{ "SK", "448" },
				{ "SI", "449" },
				{ "SB", "518" },
				{ "SO", "136" },
				{ "ZA", "159" },
				{ "GS", "297" },
				{ "SS", "163" },
				{ "ES", "410" },
				{ "LK", "307" },
				{ "SD", "162" },
				{ "SR", "232" },
				{ "SJ", "696" },
				{ "SZ", "137" },
				{ "SE", "429" },
				{ "CH", "430" },
				{ "SY", "334" },
				{ "TW", "313" },
				{ "TJ", "354" },
				{ "TZ", "139" },
				{ "TH", "335" },
				{ "TL", "397" },
				{ "TG", "140" },
				{ "TK", "699" },
				{ "TO", "519" },
				{ "TT", "224" },
				{ "TN", "141" },
				{ "TR", "436" },
				{ "TM", "355" },
				{ "TC", "678" },
				{ "TV", "517" },
				{ "UG", "142" },
				{ "UA", "445" },
				{ "AE", "331" },
				{ "GB", "426" },
				{ "US", "212" },
				{ "UM", "511" },
				{ "UY", "225" },
				{ "UZ", "356" },
				{ "VU", "505" },
				{ "VE", "226" },
				{ "VN", "337" },
				{ "VG", "682" },
				{ "VI", "683" },
				{ "WF", "509" },
				{ "EH", "197" },
				{ "YE", "348" },
				{ "ZM", "144" },
				{ "ZW", "132" }
			}.ToImmutableDictionary();
		}

		#endregion

		#region MiPymeComplianceSubType

		public static HashSet<string> MiPymeComplianceSubTypeList
		{
			get
			{
				return new HashSet<string>
				{
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXA,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXB,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXC,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDA,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDB,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDC,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCA,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCC
				};
			}
		}

		public static HashSet<string> MiPymeDebitOrCreditNoteComplianceSubTypeList
		{
			get
			{
				return new HashSet<string>
				{
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDA,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDB,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDC,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCA,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCC
				};
			}
		}

		#endregion

		public static ZString[] MiPymeDebitOrCreditNoteComplianceSubTypeMustIncludeCuitInComprobantesAsociadosList
		{
			get
			{
				return new ZString[]
				{
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDA,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCA,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDB,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCB,
				};
			}
		}

		public static HashSet<string> ClassAMComplianceSubTypeList
		{
			get
			{
				return new HashSet<string>()
				{
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDA,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCA,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PXA,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PDA,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.PCA,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXM,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDM,
					ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCM,
				};
			}
		}

		public const string MiPymeTransferModality = "ADC";

		#endregion
		}
}
