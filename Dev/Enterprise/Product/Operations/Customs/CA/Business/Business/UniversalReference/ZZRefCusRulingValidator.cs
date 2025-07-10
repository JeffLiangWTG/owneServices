using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class ZZRefCusRulingValidator
	{
		public static void ValidateSpecialAuthorityNumber(BusinessObjectFactory factory, ZPropertyInfo numberPropertyInfo, params ZGuid[] orgPKs)
		{
			var validOrgPKs = orgPKs?.Where(x => x.IsValid).ToList();
			var number = (ZString)numberPropertyInfo.Value;
			if (!number.IsEmpty)
			{
				var rulings = factory.Load<ZZRefCusRulingCombined>(new ZQuery(ZZRefCusRulingCombinedSchema.ZZX_RulingNumber, number));
				if (rulings.Length == 0)
				{
					numberPropertyInfo.AddWarning(Res.GetString("e3cea1f9-5724-4fc6-aaa6-6037fff4a5b1", "Special Authority Number not found in Rulings table. Either add a new Ruling (F3) or select a valid Ruling (F4)."));
				}
				else if (validOrgPKs != null && validOrgPKs.Count > 0)
				{
					var rulingOrgs = rulings.Select(x => x.AppliesToAddress?.OA_OH);
					if (rulingOrgs.All(pk => pk != null && !validOrgPKs.Contains(pk.Value)))
					{
						numberPropertyInfo.AddError(Res.GetString("29b59015-8d0a-45bb-95eb-847453abe443", "Special Authority Number is found but is associated with another organization and cannot be used with this organization."));
					}
				}
			}
		}

		public static void ValidateAuthorityNumberMatchesRemissionType(ZPropertyInfo numberPropertyInfo, ZString remissionType, ZZRefCusRulingCombined ruling)
		{
			if (ruling != null && ruling.ZZX_RulingType != remissionType)
			{
				numberPropertyInfo.AddMessageError(AuthorityNumberDoesNotMatchRemissionType);
			}
		}

		internal static string AuthorityNumberDoesNotMatchRemissionType => Res.GetString("09c876e8-5470-47e9-8a09-48446d85137d", "Special Authority Number does not have the same remission type with the Invoice Line.");
	}
}
