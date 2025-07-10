//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	partial class ECCCProductCategories
	{
		public static CodeDescriptionPairList GetProductCategoriesForProgram(string programCode)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			switch (programCode)
			{
				case ECCCPGADepartmentCodes.Codes.WEN:
					result.AddPair(Codes.EC35, Descriptions.EC35);
					result.AddPair(Codes.EC36, Descriptions.EC36);
					result.AddPair(Codes.EC37, Descriptions.EC37);
					result.AddPair(Codes.EC38, Descriptions.EC38);
					result.AddPair(Codes.EC39, Descriptions.EC39);
					result.AddPair(Codes.EC40, Descriptions.EC40);
					result.AddPair(Codes.EC41, Descriptions.EC41);
					result.AddPair(Codes.EC42, Descriptions.EC42);
					result.AddPair(Codes.EC43, Descriptions.EC43);
					break;
			}
			return result;
		}

		public static CodeDescriptionPairList GetVehicleClassList(string processCode)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			switch (processCode)
			{
				case ProcessCodes.Codes.EC01:
				case ProcessCodes.Codes.XE01:
					result.AddPair(Codes.EC05, Descriptions.EC05);
					result.AddPair(Codes.EC06, Descriptions.EC06);
					result.AddPair(Codes.EC07, Descriptions.EC07);
					result.AddPair(Codes.EC08, Descriptions.EC08);
					result.AddPair(Codes.EC09, Descriptions.EC09);
					result.AddPair(Codes.EC10, Descriptions.EC10);
					result.AddPair(Codes.EC11, Descriptions.EC11);
					result.AddPair(Codes.EC12, Descriptions.EC12);
					break;
				case ProcessCodes.Codes.EC04:
					result.AddPair(Codes.EC25, Descriptions.EC25);
					result.AddPair(Codes.EC26, Descriptions.EC26);
					result.AddPair(Codes.EC27, Descriptions.EC27);
					result.AddPair(Codes.EC28, Descriptions.EC28);
					result.AddPair(Codes.EC29, Descriptions.EC29);
					result.AddPair(Codes.EC31, Descriptions.EC31);
					result.AddPair(Codes.EC32, Descriptions.EC32);
					break;
				case ProcessCodes.Codes.XE04:
					result.AddPair(Codes.EC25, Descriptions.EC25);
					result.AddPair(Codes.EC26, Descriptions.EC26);
					result.AddPair(Codes.EC27, Descriptions.EC27);
					result.AddPair(Codes.EC28, Descriptions.EC28);
					result.AddPair(Codes.EC29, Descriptions.EC29);
					result.AddPair(Codes.EC31, Descriptions.EC31);
					break;
			}
			return result;
		}

		public static CodeDescriptionPairList GetEngineClassList(string processCode)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			switch (processCode)
			{
				case ProcessCodes.Codes.EC01:
				case ProcessCodes.Codes.XE01:
					result.AddPair(Codes.EC14, Descriptions.EC14);
					result.AddPair(Codes.EC34, Descriptions.EC34);
					break;
				case ProcessCodes.Codes.EC02:
				case ProcessCodes.Codes.XE02:
					result.AddPair(Codes.EC15_New, Descriptions.EC15_New);
					result.AddPair(Codes.EC16_New, Descriptions.EC16_New);
					result.AddPair(Codes.EC0A, Descriptions.EC0A);
					result.AddPair(Codes.EC0B, Descriptions.EC0B);
					result.AddPair(Codes.EC0C, Descriptions.EC0C);
					result.AddPair(Codes.EC0D, Descriptions.EC0D);
					break;
				case ProcessCodes.Codes.EC03:
				case ProcessCodes.Codes.XE03:
					result.AddPair(Codes.EC22, Descriptions.EC22);
					result.AddPair(Codes.EC23, Descriptions.EC23);
					result.AddPair(Codes.EC24, Descriptions.EC24);
					break;
				case ProcessCodes.Codes.EC04:
					result.AddPair(Codes.EC30, Descriptions.EC30);
					result.AddPair(Codes.EC33, Descriptions.EC33);
					break;
				case ProcessCodes.Codes.XE04:
					result.AddPair(Codes.EC44, Descriptions.EC44);
					result.AddPair(Codes.EC45, Descriptions.EC45);
					result.AddPair(Codes.EC46, Descriptions.EC46);
					break;
			}
			return result;
		}
	}
}
