using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.MasterFiles.Business;

public class EDIOrgStaffAssignmentsLookups : OrgStaffAssignmentsLookups
{
	public const string PrimaryKAM = "RM1";
	public const string SecondaryKAM = "RM2";

	public EDIOrgStaffAssignmentsLookups(AutoOrgStaffAssignments parent)
		: base(parent)
	{
	}

	public CodeDescriptionPairList ProductList => Factory.GetCachedValue("EDIOrgStaffAssignmentsLookups.ProductList",
					() =>
					{
						return IncidentDetailsLookupsHelper.ProductList;
					});

	public override ReadOnlyCodeDescriptionPairList StaffRoles
	{
		get
		{
			var result = new CodeDescriptionPairList();

			result.AddRange(base.StaffRoles);

			if (!result.ContainsCode(PrimaryKAM))
			{
				result.AddPair(PrimaryKAM, ResString.GetMultilingualString("b52259d5-ce1a-4028-b363-087e90c25461", "Key Account Manager - Primary"));
			}

			if (!result.ContainsCode(SecondaryKAM))
			{
				result.AddPair(SecondaryKAM, ResString.GetMultilingualString("f5d680cb-764f-4455-9125-1c6bb1b9d249", "Key Account Manager - Secondary"));
			}
			return result;
		}
	}
}

