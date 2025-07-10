using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.SG
{
	public class CustomsEntryTypeList : CodeDescriptionPairList
	{
		#region Constants

		public static class Singapore
		{
			public const string Permit = "PMT";
			public const string Certificate = "CER";

			public static class SGExemption
			{
				public static class Codes
				{
					public const string TS = "TS";
					public const string UA = "UA";
					public const string PP = "PP";
					public const string DP = "DP";
					public const string MD = "MD";
					public const string MF = "MF";
					public const string CA = "CA";
					public const string AT = "AT";
					public const string SP = "SP";
					public const string CD = "CD";
					public const string PM = "PM";
					public const string HC = "HC";
					public const string HT = "HT";
					public const string PT = "PT";
					public const string ZZ = "ZZ";
				}

				public static class Description
				{
					public static MultilingualString TS { get { return ResString.GetMultilingualString("CustomsEntryTypeList|TS", "Transhipment (includes re-documentation cargo)"); } }
					public static MultilingualString UA { get { return ResString.GetMultilingualString("CustomsEntryTypeList|UA", "Unaccompanied, non-controlled goods with total value not exceeding $400"); } }
					public static MultilingualString PP { get { return ResString.GetMultilingualString("CustomsEntryTypeList|PP", "Not prohibited under regulation 6 of the Imports and Exports Regulations 1995"); } }
					public static MultilingualString DP { get { return ResString.GetMultilingualString("CustomsEntryTypeList|DP", "Diplomatic correspondence"); } }
					public static MultilingualString MD { get { return ResString.GetMultilingualString("CustomsEntryTypeList|MD", "By joint defense force, excluding civilian motor vehicles"); } }
					public static MultilingualString MF { get { return ResString.GetMultilingualString("CustomsEntryTypeList|MF", "By the MFA, excluding motor vehicles"); } }
					public static MultilingualString CA { get { return ResString.GetMultilingualString("CustomsEntryTypeList|CA", "Used motor vehicles covered by Carnet de Passage endorsed by the Automobile Association of Singapore"); } }
					public static MultilingualString AT { get { return ResString.GetMultilingualString("CustomsEntryTypeList|AT", "Goods covered with an ATA Carnet"); } }
					public static MultilingualString SP { get { return ResString.GetMultilingualString("CustomsEntryTypeList|SP", "Bona fide trade samples not exceeding $400"); } }
					public static MultilingualString CD { get { return ResString.GetMultilingualString("CustomsEntryTypeList|CD", "Commercial, shipping or airline documents"); } }
					public static MultilingualString PM { get { return ResString.GetMultilingualString("CustomsEntryTypeList|PM", "Press photographs or negatives, news write-ups, news clippings, news films or news transcription tapes"); } }
					public static MultilingualString HC { get { return ResString.GetMultilingualString("CustomsEntryTypeList|HC", "Human corpses, human remains, human bones or cremated ashes"); } }
					public static MultilingualString HT { get { return ResString.GetMultilingualString("CustomsEntryTypeList|HT", "Human transplant materials"); } }
					public static MultilingualString PT { get { return ResString.GetMultilingualString("CustomsEntryTypeList|PT", "Pets"); } }
					public static MultilingualString ZZ { get { return ResString.GetMultilingualString("CustomsEntryTypeList|ZZ", "Others"); } }
				}

				public static bool IsTDBExemption(string code)
				{
					return Codes.AT == code
						   || Codes.CA == code
						   || Codes.CD == code
						   || Codes.DP == code
						   || Codes.HC == code
						   || Codes.HT == code
						   || Codes.MD == code
						   || Codes.MF == code
						   || Codes.PM == code
						   || Codes.PP == code
						   || Codes.PT == code
						   || Codes.SP == code
						   || Codes.TS == code
						   || Codes.UA == code
						   || Codes.ZZ == code;
				}
			}
		}

		#endregion

		public CustomsEntryTypeList()
		{
			AddPair(Singapore.Permit, ResString.GetMultilingualString("92CC5BF9-C122-4DCD-BE94-CEA314DFE76C", "Permit"));
			AddPair(Singapore.Certificate, ResString.GetMultilingualString("3ACD7A1B-C2CF-4347-A52B-C93FCF01DD68", "Certificate"));
			AddPair(Singapore.SGExemption.Codes.TS, Singapore.SGExemption.Description.TS);
			AddPair(Singapore.SGExemption.Codes.UA, Singapore.SGExemption.Description.UA);
			AddPair(Singapore.SGExemption.Codes.PP, Singapore.SGExemption.Description.PP);
			AddPair(Singapore.SGExemption.Codes.DP, Singapore.SGExemption.Description.DP);
			AddPair(Singapore.SGExemption.Codes.MD, Singapore.SGExemption.Description.MD);
			AddPair(Singapore.SGExemption.Codes.MF, Singapore.SGExemption.Description.MF);
			AddPair(Singapore.SGExemption.Codes.CA, Singapore.SGExemption.Description.CA);
			AddPair(Singapore.SGExemption.Codes.AT, Singapore.SGExemption.Description.AT);
			AddPair(Singapore.SGExemption.Codes.SP, Singapore.SGExemption.Description.SP);
			AddPair(Singapore.SGExemption.Codes.CD, Singapore.SGExemption.Description.CD);
			AddPair(Singapore.SGExemption.Codes.PM, Singapore.SGExemption.Description.PM);
			AddPair(Singapore.SGExemption.Codes.HC, Singapore.SGExemption.Description.HC);
			AddPair(Singapore.SGExemption.Codes.HT, Singapore.SGExemption.Description.HT);
			AddPair(Singapore.SGExemption.Codes.PT, Singapore.SGExemption.Description.PT);
			AddPair(Singapore.SGExemption.Codes.ZZ, Singapore.SGExemption.Description.ZZ);
		}
	}
}
