using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public partial class CARMStatementOfAccountStatementTypeList
	{
		public static class ShortCodes
		{
			public const string LegalEntiry = "L";
			public const string ProgramAccount = "A";
			public const string ProgramType = "T";
		}

		public static ZString GetLongCode(ZString shortCode)
		{
			switch (shortCode)
			{
				case ShortCodes.LegalEntiry:
					return Codes.LegalEntiry;
				case ShortCodes.ProgramAccount:
					return Codes.ProgramAccount;
				case ShortCodes.ProgramType:
					return Codes.ProgramType;
				default:
					return shortCode;
			}
		}

		public static ZString GetShortCode(ZString longCode)
		{
			switch (longCode)
			{
				case Codes.LegalEntiry:
					return ShortCodes.LegalEntiry;
				case Codes.ProgramAccount:
					return ShortCodes.ProgramAccount;
				case Codes.ProgramType:
					return ShortCodes.ProgramType;
				default:
					return longCode;
			}
		}
	}
}
