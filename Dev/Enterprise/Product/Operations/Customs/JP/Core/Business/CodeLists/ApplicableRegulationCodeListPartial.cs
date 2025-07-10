using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Business
{
	public partial class ApplicableRegulationCodeList
	{
		public static CodeDescriptionPairList EDAPairs
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.AD, Descriptions.AD);
				result.AddPair(Codes.AN, Descriptions.AN);
				result.AddPair(Codes.CA, Descriptions.CA);
				result.AddPair(Codes.CP, Descriptions.CP);
				result.AddPair(Codes.EI, Descriptions.EI);
				result.AddPair(Codes.FO, Descriptions.FO);
				result.AddPair(Codes.HU, Descriptions.HU);
				result.AddPair(Codes.MM, Descriptions.MM);
				result.AddPair(Codes.MS, Descriptions.MS);
				result.AddPair(Codes.NA, Descriptions.NA);
				result.AddPair(Codes.OP, Descriptions.OP);
				result.AddPair(Codes.PL, Descriptions.PL);
				result.AddPair(Codes.RA, Descriptions.RA);
				return result;
			}
		}

		public static CodeDescriptionPairList IDAPairs
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.AC, Descriptions.AC);
				result.AddPair(Codes.AD, Descriptions.AD);
				result.AddPair(Codes.AM, Descriptions.AM);
				result.AddPair(Codes.AN, Descriptions.AN);
				result.AddPair(Codes.CA, Descriptions.CA);
				result.AddPair(Codes.EX, Descriptions.EX);
				result.AddPair(Codes.FD, Descriptions.FD);
				result.AddPair(Codes.FL, Descriptions.FL);
				result.AddPair(Codes.FM, Descriptions.FM);
				result.AddPair(Codes.FR, Descriptions.FR);
				result.AddPair(Codes.FS, Descriptions.FS);
				result.AddPair(Codes.GA, Descriptions.GA);
				result.AddPair(Codes.HU, Descriptions.HU);
				result.AddPair(Codes.IA, Descriptions.IA);
				result.AddPair(Codes.MA, Descriptions.MA);
				result.AddPair(Codes.NA, Descriptions.NA);
				result.AddPair(Codes.OP, Descriptions.OP);
				result.AddPair(Codes.PA, Descriptions.PA);
				result.AddPair(Codes.PD, Descriptions.PD);
				result.AddPair(Codes.PE, Descriptions.PE);
				result.AddPair(Codes.PL, Descriptions.PL);
				result.AddPair(Codes.PM, Descriptions.PM);
				result.AddPair(Codes.PS, Descriptions.PS);
				result.AddPair(Codes.RA, Descriptions.RA);
				result.AddPair(Codes.SH, Descriptions.SH);
				result.AddPair(Codes.SP, Descriptions.SP);
				result.AddPair(Codes.ST, Descriptions.ST);
				return result;
			}
		}

		public static CodeDescriptionPairList BondedPairs
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.AD, Descriptions.AD);
				result.AddPair(Codes.AM, Descriptions.AM);
				result.AddPair(Codes.AN, Descriptions.AN);
				result.AddPair(Codes.CA, Descriptions.CA);
				result.AddPair(Codes.EI, Descriptions.EI);
				result.AddPair(Codes.EX, Descriptions.EX);
				result.AddPair(Codes.FD, Descriptions.FD);
				result.AddPair(Codes.FL, Descriptions.FL);
				result.AddPair(Codes.FM, Descriptions.FM);
				result.AddPair(Codes.FO, Descriptions.FO);
				result.AddPair(Codes.FR, Descriptions.FR);
				result.AddPair(Codes.FS, Descriptions.FS);
				result.AddPair(Codes.GA, Descriptions.GA);
				result.AddPair(Codes.HU, Descriptions.HU);
				result.AddPair(Codes.IA, Descriptions.IA);
				result.AddPair(Codes.MA, Descriptions.MA);
				result.AddPair(Codes.NA, Descriptions.NA);
				result.AddPair(Codes.OP, Descriptions.OP);
				result.AddPair(Codes.PA, Descriptions.PA);
				result.AddPair(Codes.PD, Descriptions.PD);
				result.AddPair(Codes.PE, Descriptions.PE);
				result.AddPair(Codes.PL, Descriptions.PL);
				result.AddPair(Codes.PM, Descriptions.PM);
				result.AddPair(Codes.PS, Descriptions.PS);
				result.AddPair(Codes.RA, Descriptions.RA);
				result.AddPair(Codes.SH, Descriptions.SH);
				result.AddPair(Codes.SP, Descriptions.SP);
				result.AddPair(Codes.ST, Descriptions.ST);
				return result;
			}
		}
	}
}
