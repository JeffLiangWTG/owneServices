using System;
using System.Collections.Generic;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	partial class HCCategories
	{
		public static CodeDescriptionPairList GetCategoryForProgram(string programCode, string intendedUseCode)
		{
			var result = new CodeDescriptionPairList();
			foreach (var relation in list)
			{
				if (relation.Item1 == programCode && relation.Item2 == intendedUseCode)
				{
					result.AddPair(relation.Item3, relation.Item4);
				}
			}
			return result;
		}

		static readonly List<Tuple<string, string, string, string>> list = new List<Tuple<string, string, string, string>>
		{
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.API, HCIntendedUseCode.Codes.HC13, Codes.HC01, Descriptions.HC01),

			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.BBC, HCIntendedUseCode.Codes.HC01, Codes.HC02, Descriptions.HC02),

			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CTO, HCIntendedUseCode.Codes.HC01, Codes.HC26, Descriptions.HC26),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CTO, HCIntendedUseCode.Codes.HC01, Codes.HC27, Descriptions.HC27),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CTO, HCIntendedUseCode.Codes.HC01, Codes.HC28, Descriptions.HC28),

			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CPR, HCIntendedUseCode.Codes.HC21, Codes.HC29, Descriptions.HC29),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CPR, HCIntendedUseCode.Codes.HC21, Codes.HC30, Descriptions.HC30),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CPR, HCIntendedUseCode.Codes.HC21, Codes.HC31, Descriptions.HC31),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CPR, HCIntendedUseCode.Codes.HC21, Codes.HC32, Descriptions.HC32),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CPR, HCIntendedUseCode.Codes.HC21, Codes.HC33, Descriptions.HC33),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CPR, HCIntendedUseCode.Codes.HC21, Codes.HC34, Descriptions.HC34),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CPR, HCIntendedUseCode.Codes.HC21, Codes.HC35, Descriptions.HC35),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CPR, HCIntendedUseCode.Codes.HC21, Codes.HC36, Descriptions.HC36),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CPR, HCIntendedUseCode.Codes.HC21, Codes.HC37, Descriptions.HC37),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CPR, HCIntendedUseCode.Codes.HC22, Codes.HC37, Descriptions.HC37),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CPR, HCIntendedUseCode.Codes.HC23, Codes.HC37, Descriptions.HC37),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CPR, HCIntendedUseCode.Codes.HC24, Codes.HC37, Descriptions.HC37),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CPR, HCIntendedUseCode.Codes.HC26, Codes.HC37, Descriptions.HC37),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.CPR, HCIntendedUseCode.Codes.HC27, Codes.HC37, Descriptions.HC37),

			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.DSE, HCIntendedUseCode.Codes.HC01, Codes.HC04, Descriptions.HC04),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.DSE, HCIntendedUseCode.Codes.HC02, Codes.HC04, Descriptions.HC04),

			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.HDR, HCIntendedUseCode.Codes.HC01, Codes.HC05, Descriptions.HC05),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.HDR, HCIntendedUseCode.Codes.HC01, Codes.HC06, Descriptions.HC06),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.HDR, HCIntendedUseCode.Codes.HC02, Codes.HC05, Descriptions.HC05),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.HDR, HCIntendedUseCode.Codes.HC05, Codes.HC07, Descriptions.HC07),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.HDR, HCIntendedUseCode.Codes.HC05, Codes.HC08, Descriptions.HC08),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.HDR, HCIntendedUseCode.Codes.HC05, Codes.HC09, Descriptions.HC09),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.HDR, HCIntendedUseCode.Codes.HC05, Codes.HC10, Descriptions.HC10),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.HDR, HCIntendedUseCode.Codes.HC07, Codes.HC05, Descriptions.HC05),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.HDR, HCIntendedUseCode.Codes.HC07, Codes.HC06, Descriptions.HC06),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.HDR, HCIntendedUseCode.Codes.HC29, Codes.HC05, Descriptions.HC05),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.HDR, HCIntendedUseCode.Codes.HC29, Codes.HC06, Descriptions.HC06),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.HDR, HCIntendedUseCode.Codes.HC31, Codes.HC05, Descriptions.HC05),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.HDR, HCIntendedUseCode.Codes.HC31, Codes.HC06, Descriptions.HC06),

			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.NHP, HCIntendedUseCode.Codes.HC01, Codes.HC15, Descriptions.HC15),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.NHP, HCIntendedUseCode.Codes.HC05, Codes.HC15, Descriptions.HC15),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.NHP, HCIntendedUseCode.Codes.HC02, Codes.HC15, Descriptions.HC15),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.NHP, HCIntendedUseCode.Codes.HC07, Codes.HC15, Descriptions.HC15),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.NHP, HCIntendedUseCode.Codes.HC29, Codes.HC15, Descriptions.HC15),

			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.PES, HCIntendedUseCode.Codes.HC06, Codes.HC38, Descriptions.HC38),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.PES, HCIntendedUseCode.Codes.HC06, Codes.HC39, Descriptions.HC39),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.PES, HCIntendedUseCode.Codes.HC07, Codes.HC38, Descriptions.HC38),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.PES, HCIntendedUseCode.Codes.HC07, Codes.HC39, Descriptions.HC39),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.PES, HCIntendedUseCode.Codes.HC08, Codes.HC38, Descriptions.HC38),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.PES, HCIntendedUseCode.Codes.HC08, Codes.HC39, Descriptions.HC39),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.PES, HCIntendedUseCode.Codes.HC09, Codes.HC38, Descriptions.HC38),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.PES, HCIntendedUseCode.Codes.HC09, Codes.HC39, Descriptions.HC39),

			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.VET, HCIntendedUseCode.Codes.HC10, Codes.HC16, Descriptions.HC16),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.VET, HCIntendedUseCode.Codes.HC10, Codes.HC17, Descriptions.HC17),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.VET, HCIntendedUseCode.Codes.HC11, Codes.HC16, Descriptions.HC16),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.VET, HCIntendedUseCode.Codes.HC14, Codes.HC16, Descriptions.HC16),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.VET, HCIntendedUseCode.Codes.HC12, Codes.HC16, Descriptions.HC16),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.VET, HCIntendedUseCode.Codes.HC07, Codes.HC16, Descriptions.HC16),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.VET, HCIntendedUseCode.Codes.HC29, Codes.HC16, Descriptions.HC16),

			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC01, Codes.HC18, Descriptions.HC18),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC01, Codes.HC19, Descriptions.HC19),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC01, Codes.HC20, Descriptions.HC20),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC01, Codes.HC22, Descriptions.HC22),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC01, Codes.HC24, Descriptions.HC24),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC01, Codes.HC25, Descriptions.HC25),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC05, Codes.HC18, Descriptions.HC18),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC05, Codes.HC19, Descriptions.HC19),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC05, Codes.HC20, Descriptions.HC20),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC05, Codes.HC21, Descriptions.HC21),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC05, Codes.HC22, Descriptions.HC22),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC05, Codes.HC24, Descriptions.HC24),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC05, Codes.HC25, Descriptions.HC25),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC15, Codes.HC18, Descriptions.HC18),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC15, Codes.HC19, Descriptions.HC19),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC15, Codes.HC20, Descriptions.HC20),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC15, Codes.HC21, Descriptions.HC21),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC15, Codes.HC22, Descriptions.HC22),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC15, Codes.HC23, Descriptions.HC23),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC15, Codes.HC24, Descriptions.HC24),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC15, Codes.HC25, Descriptions.HC25),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC02, Codes.HC19, Descriptions.HC19),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC02, Codes.HC20, Descriptions.HC20),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC02, Codes.HC22, Descriptions.HC22),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC10, Codes.HC19, Descriptions.HC19),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC10, Codes.HC20, Descriptions.HC20),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC10, Codes.HC22, Descriptions.HC22),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC10, Codes.HC24, Descriptions.HC24),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC16, Codes.HC23, Descriptions.HC23),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC17, Codes.HC23, Descriptions.HC23),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC18, Codes.HC23, Descriptions.HC23),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC19, Codes.HC23, Descriptions.HC23),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC19, Codes.HC24, Descriptions.HC24),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC13, Codes.HC24, Descriptions.HC24),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC13, Codes.HC25, Descriptions.HC25),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC20, Codes.HC24, Descriptions.HC24),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC28, Codes.HC18, Descriptions.HC18),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC28, Codes.HC19, Descriptions.HC19),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC28, Codes.HC20, Descriptions.HC20),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC28, Codes.HC21, Descriptions.HC21),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC28, Codes.HC22, Descriptions.HC22),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC28, Codes.HC23, Descriptions.HC23),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC28, Codes.HC24, Descriptions.HC24),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.OCS, HCIntendedUseCode.Codes.HC28, Codes.HC25, Descriptions.HC25),

			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.RED, "", Codes.HC40, Descriptions.HC40),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.RED, "", Codes.HC41, Descriptions.HC41),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.RED, "", Codes.HC42, Descriptions.HC42),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.RED, "", Codes.HC43, Descriptions.HC43),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.RED, "", Codes.HC44, Descriptions.HC44),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.RED, "", Codes.HC45, Descriptions.HC45),

			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC01, Codes.HC11, Descriptions.HC11),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC01, Codes.HC12, Descriptions.HC12),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC01, Codes.HC13, Descriptions.HC13),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC01, Codes.HC14, Descriptions.HC14),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC03, Codes.HC11, Descriptions.HC11),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC03, Codes.HC12, Descriptions.HC12),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC03, Codes.HC13, Descriptions.HC13),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC03, Codes.HC14, Descriptions.HC14),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC02, Codes.HC11, Descriptions.HC11),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC02, Codes.HC12, Descriptions.HC12),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC02, Codes.HC13, Descriptions.HC13),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC02, Codes.HC14, Descriptions.HC14),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC04, Codes.HC11, Descriptions.HC11),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC04, Codes.HC12, Descriptions.HC12),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC04, Codes.HC13, Descriptions.HC13),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC04, Codes.HC14, Descriptions.HC14),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC07, Codes.HC11, Descriptions.HC11),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC07, Codes.HC12, Descriptions.HC12),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC07, Codes.HC13, Descriptions.HC13),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC07, Codes.HC14, Descriptions.HC14),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC30, Codes.HC11, Descriptions.HC11),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC30, Codes.HC12, Descriptions.HC12),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC30, Codes.HC13, Descriptions.HC13),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC30, Codes.HC14, Descriptions.HC14),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC29, Codes.HC11, Descriptions.HC11),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC29, Codes.HC12, Descriptions.HC12),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC29, Codes.HC13, Descriptions.HC13),
			new Tuple<string, string, string, string>(HCPGADepartmentCodes.Codes.MDE, HCIntendedUseCode.Codes.HC29, Codes.HC14, Descriptions.HC14)
		};
	}
}
