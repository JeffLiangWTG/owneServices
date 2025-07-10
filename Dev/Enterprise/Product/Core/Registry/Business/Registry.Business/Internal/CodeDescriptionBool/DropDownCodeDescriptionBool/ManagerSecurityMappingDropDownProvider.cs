using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public static class ManagerSecurityMappingDropDownProvider
	{
		public static ReadOnlyCodeDescriptionPairList MangerMappingCodeLookup
		{
			get
			{
				if (mangerMappingCodeLookup == null)
				{
					mangerMappingCodeLookup = new CodeDescriptionPairList();
					//code and description must be switched as there is no drop down control which supports showing only description
					mangerMappingCodeLookup.AddPair(ResString.GetMultilingualString("FF511D2E-1FF8-4C2E-BBEA-846F75BFE46D", "MANAGERSECURITY1"), "");
					mangerMappingCodeLookup.AddPair(ResString.GetMultilingualString("1D58F6DD-381A-4A6C-98A8-4EE5D4609C09", "MANAGERSECURITY2"), "");
					mangerMappingCodeLookup.AddPair(ResString.GetMultilingualString("2F1C7205-6FB3-4E63-8E6D-538077A5D063", "MANAGERSECURITY3"), "");
				}
				return mangerMappingCodeLookup;
			}
		}
		public static ReadOnlyCodeDescriptionPairList MangerMappingDescriptionLookup
		{
			get
			{
				if (mangerMappingDescriptionLookup == null)
				{
					mangerMappingDescriptionLookup = new CodeDescriptionPairList();

					var reportingRoleCodes = SystemDataRegistry.Instance.StaffReportingRoles.Value.ToArray<StaffReportingRole>().Select(x => x.Code);
					foreach (var code in reportingRoleCodes)
					{
						mangerMappingDescriptionLookup.AddPair(code, "");
					}
				}
				return mangerMappingDescriptionLookup;
			}
		}

		[ThreadStatic]
		static CodeDescriptionPairList mangerMappingCodeLookup;

		[ThreadStatic]
		static CodeDescriptionPairList mangerMappingDescriptionLookup;
	}
}
