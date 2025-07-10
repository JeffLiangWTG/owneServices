
namespace Enterprise.Client.ECU.ConsolExport
{
	public static class Constants
	{
		#region Header

		public static class Header
		{
			public const string GVER = "[GVER]";
			public const string GSTS = "[GSTS]";
			public const string GDOC = "[GDOC]";
			public const string GREF = "[GREF]";
			public const string GRCV = "[GRCV]";
			public const string GSND = "[GSND]";
			public const string GDAT = "[GDAT]";
			public const string VVES = "[VVES]";
			public const string VOYN = "[VOYN]";
			public const string VCAR = "[VCAR]";
			public const string VPOL = "[VPOL]";
			public const string VPOD = "[VPOD]";
			public const string VETD = "[VETD]";
			public const string VETA = "[VETA]";
			public const string CNTR = "[CNTR]";
			public const string CTYP = "[CTYP]";
			public const string CSLN = "[CSLN]";
			public const string BWGT = "[BWGT]";
			public const string BFRT = "[BFRT]";
			public const string BCON = "[BCON]";
			public const string BMEA = "[BMEA]";
			public const string BBPD = "[BBPD]";
			public const string BDSC = "[BDSC]";
			public const string BHBL = "[BHBL]";
			public const string BBPL = "[BBPL]";
			public const string BSET = "[BSET]";
			public const string BMKN = "[BMKN]";
			public const string BSHP = "[BSHP]";
			public const string BNOT = "[BNOT]";
			public const string BDES = "[BDES]";
			public const string BPKG = "[BPKG]";
			public const string CPKG = "[CPKG]";
			public const string CWGT = "[CWGT]";
			public const string CMEA = "[CMEA]";
			public const string TPKG = "[TPKG]";
			public const string TWGT = "[TWGT]";
			public const string TMEA = "[TMEA]";
			public const string VLAG = "[VLAG]";
			public const string BNBL = "[BNBL]";
			public const string DONE = "[DONE]";
		}

		#endregion

		#region SubHeader

		public static class SubHeader
		{
			public const string Code = "<CODE>";
			public const string Date = "<DATE>";
			public const string Time = "<TIME>";
			public const string Name = "<NAME>";
			public const string Adr1 = "<ADR1>";
			public const string Adr2 = "<ADR2>";
			public const string Adr3 = "<ADR3>";
			public const string Faxn = "<FAXN>";
			public const string Mail = "<MAIL>";
			public const string Stop = "<STOP>";
			public const string Numb = "<NUMB>";
			public const string Unit = "<UNIT>";
			public const string Strt = "<STRT>";
			public const string Full = "<FULL>";
			public const string Ln = "<LN";
			public const string Curr = "<CURR>";
		}

		#endregion

		public const string VersionNumber = "001500";
		public const string Production = "P";
		public const string Manifest = "M";
		public const string WeightUQ = "KGS";
		public const string VolUQ = "CBM";
	}
}
