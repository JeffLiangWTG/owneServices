using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business
{
	public static class OrgHeaderExtension
	{
		public static ZString GetNIFCode(this OrgHeader org) => org?.CustomsCodes.GetCustomsRegNo(OrgCusCode.SpainCodeTypes.NIF, Core.Constants.CountryCodes.Spain) ?? ZString.Empty;

		public static ZString GetPASCode(this OrgHeader org) => org?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.PassportID, ZString.Empty) ?? ZString.Empty;

		public static ZString GetIDCode(this OrgHeader org)
		{
			var eorOrnifCode = GetEOROrNIFCode(org);
			return eorOrnifCode.IsEmpty ? org.GetPASCode() : eorOrnifCode;
		}

		public static ZString GetEOROrNIFCode(this OrgHeader org)
		{
			var eorOrnifCode = org.GetEuIdentificationNumber(EconomicGroupList.Codes.EuropeanUnion);
			return eorOrnifCode.IsEmpty ? org.GetEuIdentificationNumber() : eorOrnifCode;
		}

		public static ZString GetEORIForLRNGeneration(this ILRNGenerator header)
		{
			var eoriForLrn = GetIDCode(header.Branch.OrgProxy);
			if (eoriForLrn.IsEmpty)
			{
				eoriForLrn = GetIDCode(header.Branch.Company.OrgProxy);
			}

			if (RefCountry.LoadFromCountryCode(header.Factory, eoriForLrn.SubstringSafe(0, 2)) != null)
			{
				eoriForLrn = eoriForLrn.SubstringSafe(2);
			}

			if (eoriForLrn.Length > 15)
			{
				eoriForLrn = ZString.Empty;
			}

			return eoriForLrn;
		}
	}
}
