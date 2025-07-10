//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoHCPGAHeaderAddInfoLookups
//
//    This class should be used for overriding collections in AutoHCPGAHeaderAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Globalization;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class HCPGAHeaderAddInfoLookups : AutoHCPGAHeaderAddInfoLookups
	{
		public HCPGAHeaderAddInfoLookups(AutoHCPGAHeaderAddInfo parent) : base(parent)
		{
		}

		public UNDGSubstanceCollection UNDGCodeList => PGAHeaderExtensions.GetCachedUNDGSubstanceCollection(Factory);

		public CodeDescriptionPairList ProgramCodesList
		{
			get { return Factory.GetCachedValue<HCPGADepartmentCodes>(); }
		}

		HCPGAHeader PGAHeader => (HCPGAHeader)((HCPGAHeaderAddInfo)Parent).Parent;

		public CodeDescriptionPairList IntendedUseCodesAPI => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "IntendedUseCodesBasedPgaProgram+API"), () => HCIntendedUseCode.GetIntendedCodeForProgram(HCPGADepartmentCodes.Codes.API));
		public CodeDescriptionPairList IntendedUseCodesBBC => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "IntendedUseCodesBasedPgaProgram+BBC"), () => HCIntendedUseCode.GetIntendedCodeForProgram(HCPGADepartmentCodes.Codes.BBC));
		public CodeDescriptionPairList IntendedUseCodesCTO => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "IntendedUseCodesBasedPgaProgram+CTO"), () => HCIntendedUseCode.GetIntendedCodeForProgram(HCPGADepartmentCodes.Codes.CTO));
		public CodeDescriptionPairList IntendedUseCodesCPR => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "IntendedUseCodesBasedPgaProgram+CPR"), () => HCIntendedUseCode.GetIntendedCodeForProgram(HCPGADepartmentCodes.Codes.CPR));
		public CodeDescriptionPairList IntendedUseCodesDSE => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "IntendedUseCodesBasedPgaProgram+DSE"), () => HCIntendedUseCode.GetIntendedCodeForProgram(HCPGADepartmentCodes.Codes.DSE));
		public CodeDescriptionPairList IntendedUseCodesHDR => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "IntendedUseCodesBasedPgaProgram+HDR"), () => HCIntendedUseCode.GetIntendedCodeForProgram(HCPGADepartmentCodes.Codes.HDR));
		public CodeDescriptionPairList IntendedUseCodesOCS => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "IntendedUseCodesBasedPgaProgram+OCS"), () => HCIntendedUseCode.GetIntendedCodeForProgram(HCPGADepartmentCodes.Codes.OCS));
		public CodeDescriptionPairList IntendedUseCodesMDE => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "IntendedUseCodesBasedPgaProgram+MDE"), () => HCIntendedUseCode.GetIntendedCodeForProgram(HCPGADepartmentCodes.Codes.MDE));
		public CodeDescriptionPairList IntendedUseCodesNHP => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "IntendedUseCodesBasedPgaProgram+NHP"), () => HCIntendedUseCode.GetIntendedCodeForProgram(HCPGADepartmentCodes.Codes.NHP));
		public CodeDescriptionPairList IntendedUseCodesPES => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "IntendedUseCodesBasedPgaProgram+DES"), () => HCIntendedUseCode.GetIntendedCodeForProgram(HCPGADepartmentCodes.Codes.PES));
		public CodeDescriptionPairList IntendedUseCodesRED => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "IntendedUseCodesBasedPgaProgram+RED"), () => HCIntendedUseCode.GetIntendedCodeForProgram(HCPGADepartmentCodes.Codes.RED));
		public CodeDescriptionPairList IntendedUseCodesVET => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "IntendedUseCodesBasedPgaProgram+VET"), () => HCIntendedUseCode.GetIntendedCodeForProgram(HCPGADepartmentCodes.Codes.VET));

		public CodeDescriptionPairList CategoryCodesAPI => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "CategoryCodesAPI+{0}", PGAHeader.CA_IntendedUseCodeAPI), () => GetCategoryCodesList(HCPGADepartmentCodes.Codes.API, PGAHeader.CA_IntendedUseCodeAPI));
		public CodeDescriptionPairList CategoryCodesBBC => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "CategoryCodesBBC+{0}", PGAHeader.CA_IntendedUseCodeBBC), () => GetCategoryCodesList(HCPGADepartmentCodes.Codes.BBC, PGAHeader.CA_IntendedUseCodeBBC));
		public CodeDescriptionPairList CategoryCodesCTO => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "CategoryCodesCTO+{0}", PGAHeader.CA_IntendedUseCodeCTO), () => GetCategoryCodesList(HCPGADepartmentCodes.Codes.CTO, PGAHeader.CA_IntendedUseCodeCTO));
		public CodeDescriptionPairList CategoryCodesCPR => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "CategoryCodesCPR+{0}", PGAHeader.CA_IntendedUseCodeCPR), () => GetCategoryCodesList(HCPGADepartmentCodes.Codes.CPR, PGAHeader.CA_IntendedUseCodeCPR));
		public CodeDescriptionPairList CategoryCodesDSE => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "CategoryCodesDSE+{0}", PGAHeader.CA_IntendedUseCodeDSE), () => GetCategoryCodesList(HCPGADepartmentCodes.Codes.DSE, PGAHeader.CA_IntendedUseCodeDSE));
		public CodeDescriptionPairList CategoryCodesHDR => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "CategoryCodesHDR+{0}", PGAHeader.CA_IntendedUseCodeHDR), () => GetCategoryCodesList(HCPGADepartmentCodes.Codes.HDR, PGAHeader.CA_IntendedUseCodeHDR));
		public CodeDescriptionPairList CategoryCodesOCS => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "CategoryCodesOCS+{0}", PGAHeader.CA_IntendedUseCodeOCS), () => GetCategoryCodesList(HCPGADepartmentCodes.Codes.OCS, PGAHeader.CA_IntendedUseCodeOCS));
		public CodeDescriptionPairList CategoryCodesMDE => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "CategoryCodesMDE+{0}", PGAHeader.CA_IntendedUseCodeMDE), () => GetCategoryCodesList(HCPGADepartmentCodes.Codes.MDE, PGAHeader.CA_IntendedUseCodeMDE));
		public CodeDescriptionPairList CategoryCodesNHP => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "CategoryCodesNHP+{0}", PGAHeader.CA_IntendedUseCodeNHP), () => GetCategoryCodesList(HCPGADepartmentCodes.Codes.NHP, PGAHeader.CA_IntendedUseCodeNHP));
		public CodeDescriptionPairList CategoryCodesPES => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "CategoryCodesPES+{0}", PGAHeader.CA_IntendedUseCodePES), () => GetCategoryCodesList(HCPGADepartmentCodes.Codes.PES, PGAHeader.CA_IntendedUseCodePES));
		public CodeDescriptionPairList CategoryCodesRED => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "CategoryCodesRED+{0}", PGAHeader.CA_IntendedUseCodeRED), () => GetCategoryCodesList(HCPGADepartmentCodes.Codes.RED, PGAHeader.CA_IntendedUseCodeRED));
		public CodeDescriptionPairList CategoryCodesVET => Factory.GetCachedValue(string.Format(CultureInfo.CurrentCulture, "CategoryCodesVET+{0}", PGAHeader.CA_IntendedUseCodeVET), () => GetCategoryCodesList(HCPGADepartmentCodes.Codes.VET, PGAHeader.CA_IntendedUseCodeVET));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		CodeDescriptionPairList GetCategoryCodesList(string programCode, string intendedUseCode)
		{
			var result = new CodeDescriptionPairList();
			result.AddRangeOverwriteIfExists(HCCategories.GetCategoryForProgram(programCode, intendedUseCode));
			result.Sort();
			return result;
		}

		public HCExceptProcessingCodes ExceptProcessingCodes => Factory.GetCachedValue<HCExceptProcessingCodes>();
	}
}
