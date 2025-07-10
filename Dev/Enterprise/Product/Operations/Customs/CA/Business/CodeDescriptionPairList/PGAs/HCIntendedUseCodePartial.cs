
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	partial class HCIntendedUseCode
	{
		public static CodeDescriptionPairList GetIntendedCodeForProgram(string programCode)
		{
			var result = new CodeDescriptionPairList();
			switch (programCode)
			{
				case HCPGADepartmentCodes.Codes.API:
					result.AddPairIfNotExist(Codes.HC13, Descriptions.HC13);
					break;
				case HCPGADepartmentCodes.Codes.BBC:
					result.AddPairIfNotExist(Codes.HC01, Descriptions.HC01);
					break;
				case HCPGADepartmentCodes.Codes.CTO:
					result.AddPairIfNotExist(Codes.HC01, Descriptions.HC01);
					break;
				case HCPGADepartmentCodes.Codes.CPR:
					result.AddPairIfNotExist(Codes.HC21, Descriptions.HC21);
					result.AddPairIfNotExist(Codes.HC22, Descriptions.HC22);
					result.AddPairIfNotExist(Codes.HC23, Descriptions.HC23);
					result.AddPairIfNotExist(Codes.HC24, Descriptions.HC24);
					result.AddPairIfNotExist(Codes.HC26, Descriptions.HC26);
					result.AddPairIfNotExist(Codes.HC27, Descriptions.HC27);
					break;
				case HCPGADepartmentCodes.Codes.DSE:
					result.AddPairIfNotExist(Codes.HC01, Descriptions.HC01);
					result.AddPairIfNotExist(Codes.HC02, Descriptions.HC02);
					break;
				case HCPGADepartmentCodes.Codes.HDR:
					result.AddPairIfNotExist(Codes.HC01, Descriptions.HC01);
					result.AddPairIfNotExist(Codes.HC02, Descriptions.HC02);
					result.AddPairIfNotExist(Codes.HC05, Descriptions.HC05);
					result.AddPairIfNotExist(Codes.HC07, Descriptions.HC07);
					result.AddPairIfNotExist(Codes.HC29, Descriptions.HC29);
					result.AddPairIfNotExist(Codes.HC31, Descriptions.HC31);
					break;
				case HCPGADepartmentCodes.Codes.NHP:
					result.AddPairIfNotExist(Codes.HC01, Descriptions.HC01);
					result.AddPairIfNotExist(Codes.HC02, Descriptions.HC02);
					result.AddPairIfNotExist(Codes.HC05, Descriptions.HC05);
					result.AddPairIfNotExist(Codes.HC07, Descriptions.HC07);
					result.AddPairIfNotExist(Codes.HC29, Descriptions.HC29);
					break;
				case HCPGADepartmentCodes.Codes.OCS:
					result.AddPairIfNotExist(Codes.HC01, Descriptions.HC01);
					result.AddPairIfNotExist(Codes.HC02, Descriptions.HC02);
					result.AddPairIfNotExist(Codes.HC05, Descriptions.HC05);
					result.AddPairIfNotExist(Codes.HC10, Descriptions.HC10);
					result.AddPairIfNotExist(Codes.HC13, Descriptions.HC13);
					result.AddPairIfNotExist(Codes.HC15, Descriptions.HC15);
					result.AddPairIfNotExist(Codes.HC16, Descriptions.HC16);
					result.AddPairIfNotExist(Codes.HC17, Descriptions.HC17);
					result.AddPairIfNotExist(Codes.HC18, Descriptions.HC18);
					result.AddPairIfNotExist(Codes.HC19, Descriptions.HC19);
					result.AddPairIfNotExist(Codes.HC20, Descriptions.HC20);
					result.AddPairIfNotExist(Codes.HC28, Descriptions.HC28);
					break;
				case HCPGADepartmentCodes.Codes.MDE:
					result.AddPairIfNotExist(Codes.HC01, Descriptions.HC01);
					result.AddPairIfNotExist(Codes.HC02, Descriptions.HC02);
					result.AddPairIfNotExist(Codes.HC03, Descriptions.HC03);
					result.AddPairIfNotExist(Codes.HC04, Descriptions.HC04);
					result.AddPairIfNotExist(Codes.HC07, Descriptions.HC07);
					result.AddPairIfNotExist(Codes.HC29, Descriptions.HC29);
					result.AddPairIfNotExist(Codes.HC30, Descriptions.HC30);
					break;
				case HCPGADepartmentCodes.Codes.PES:
					result.AddPairIfNotExist(Codes.HC06, Descriptions.HC06);
					result.AddPairIfNotExist(Codes.HC07, Descriptions.HC07);
					result.AddPairIfNotExist(Codes.HC08, Descriptions.HC08);
					result.AddPairIfNotExist(Codes.HC09, Descriptions.HC09);
					break;
				case HCPGADepartmentCodes.Codes.VET:
					result.AddPairIfNotExist(Codes.HC07, Descriptions.HC07);
					result.AddPairIfNotExist(Codes.HC10, Descriptions.HC10);
					result.AddPairIfNotExist(Codes.HC11, Descriptions.HC11);
					result.AddPairIfNotExist(Codes.HC12, Descriptions.HC12);
					result.AddPairIfNotExist(Codes.HC14, Descriptions.HC14);
					result.AddPairIfNotExist(Codes.HC29, Descriptions.HC29);
					break;
			}
			return result;
		}
	}
}
