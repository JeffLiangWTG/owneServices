using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public static class CommonLookups
	{
		public static CodeDescriptionPairList EORILookup(JobDeclaration declaration, Func<OrgHeader, ZString> getIdentificationNumber, Func<OrgHeader, IEnumerable<ZString>> getIdentificationNumbers = null)
		{
			return EORILookup(declaration, Array.Empty<string>(), getIdentificationNumber, getIdentificationNumbers);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string Declarant = "Declarant";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string Importer = "Importer";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string Representative = "Representative";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string Buyer = "Buyer";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string Warehouse = "Warehouse";

		public static CodeDescriptionPairList EORILookup(JobDeclaration declaration, string[] filter, Func<OrgHeader, ZString> getIdentificationNumber, Func<OrgHeader, IEnumerable<ZString>> getIdentificationNumbers = null)
		{
			var result = new CodeDescriptionPairList();
			if (declaration != null)
			{
				AddEORIToListFromOrgHeader(result, Declarant, declaration.Declarant?.Header, filter, getIdentificationNumber, getIdentificationNumbers);
				AddEORIToListFromOrgHeader(result, Representative, declaration.Representative?.Header, filter, getIdentificationNumber, getIdentificationNumbers);
				AddEORIToListFromOrgHeader(result, Importer, declaration.Importer, filter, getIdentificationNumber, getIdentificationNumbers);
				AddEORIToListFromOrgHeader(result, Buyer, declaration.Buyer, filter, getIdentificationNumber, getIdentificationNumbers);

				foreach (CusEntryInstruction csi in declaration.CustomsEntryInstructions)
				{
					AddEORIToListFromOrgHeader(result, Warehouse, csi.Warehouse2?.Header, filter, getIdentificationNumber, getIdentificationNumbers);
					AddEORIToListFromOrgHeader(result, Warehouse, csi.Warehouse?.Header, filter, getIdentificationNumber, getIdentificationNumbers);
				}
			}
			return result;
		}

		public static void AddEORIToListFromOrgPk(CodeDescriptionPairList list, string prefix, ZGuid? orgPK, BusinessObjectFactory factory, Func<OrgHeader, ZString> getIdentificationNumber, Func<OrgHeader, IEnumerable<ZString>> getIdentificationNumbers = null)
		{
			AddEORIToListFromOrgPk(list, prefix, orgPK, factory, Array.Empty<string>(), getIdentificationNumber, getIdentificationNumbers);
		}

		static void AddEORIToListFromOrgPk(CodeDescriptionPairList list, string prefix, ZGuid? orgPK, BusinessObjectFactory factory, string[] filter, Func<OrgHeader, ZString> getIdentificationNumber, Func<OrgHeader, IEnumerable<ZString>> getIdentificationNumbers = null)
		{
			var org = orgPK.HasValue ? factory.Load<OrgHeader>(orgPK.Value) : null;

			AddEORIToListFromOrgHeader(list, prefix, org, filter, getIdentificationNumber, getIdentificationNumbers);
		}

		static void AddEORIToListFromOrgHeader(CodeDescriptionPairList list, string prefix, OrgHeader org, string[] filter, Func<OrgHeader, ZString> getIdentificationNumber, Func<OrgHeader, IEnumerable<ZString>> getIdentificationNumbers = null)
		{
			if (org != null && (filter.Length == 0 || filter.Contains(prefix)))
			{
				if (getIdentificationNumbers != null)
				{
					var codes = getIdentificationNumbers?.Invoke(org);
					foreach (var code in codes)
					{
						if (!string.IsNullOrWhiteSpace(code) && !list.ContainsCode(code))
						{
							list.AddPair(code, string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} - {1}", prefix, org.OH_FullName));
						}
					}
				}
				else
				{
					var code = getIdentificationNumber?.Invoke(org) ?? org.GetEuIdentificationNumber();
					if (!string.IsNullOrWhiteSpace(code) && !list.ContainsCode(code))
					{
						list.AddPair(code, string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} - {1}", prefix, org.OH_FullName));
					}
				}
			}
		}
	}
}
