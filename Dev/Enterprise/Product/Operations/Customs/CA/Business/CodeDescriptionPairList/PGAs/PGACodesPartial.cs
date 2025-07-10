using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class PGACodes : Common.CA.PGACodes
	{
		public static CodeDescriptionPairList GetProgramCodesList(ZString pgaType, BusinessObjectFactory factory)
		{
			var result = new CodeDescriptionPairList();
			switch (pgaType)
			{
				case Codes.DFO:
					result = factory.GetCachedValue<DFOPGADepartmentCodes>();
					break;
				case Codes.ECCC:
					result = factory.GetCachedValue<ECCCPGADepartmentCodes>();
					break;
				case Codes.HC:
					result = factory.GetCachedValue<HCPGADepartmentCodes>();
					break;
				case Codes.NRCan:
					result = factory.GetCachedValue<NRCanPGADepartmentCodes>();
					break;
				case Codes.TC:
					result = factory.GetCachedValue<TCPGADepartmentCodes>();
					break;
				case Codes.CFIA:
					result = factory.GetCachedValue<CFIAPGADepartmentCodes>();
					break;
				case Codes.PHAC:
					result = factory.GetCachedValue<PHACPGADepartmentCodes>();
					break;
				case Codes.CNSC:
					result = factory.GetCachedValue<CNSCPGADepartmentCodes>();
					break;
				case Codes.GAC:
					result = factory.GetCachedValue<GACPGADepartmentCodes>();
					break;
			}
			return result;
		}

		public static ZString GetPGACodeFromGovAgencyID(ZString govAgencyID)
		{
			var result = govAgencyID;
			switch (govAgencyID)
			{
				case "1":
					result = Codes.CFIA;
					break;
				case "12":
					result = Codes.HC;
					break;
				case "13":
					result = Codes.TC;
					break;
				case "20":
					result = Codes.DFO;
					break;
				case "21":
					result = Codes.NRCan;
					break;
				case "22":
					result = Codes.ECCC;
					break;
				case "23":
					result = Codes.PHAC;
					break;
				case "24":
					result = Codes.CNSC;
					break;
				case "3":
					result = Codes.GAC;
					break;
				case "5":
					result = Codes.CBSA;
					break;
			}
			return result;
		}

		public static CodeDescriptionPairList GetGovernmentAgencyCodesList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<CodeDescriptionPairList>("GovernmentAgencyCodesList", () =>
			{
				var result = new PGACodes();
				result.RemoveCode(PGACodes.Codes.CBSA);
				return result;
			});
		}
	}
}
