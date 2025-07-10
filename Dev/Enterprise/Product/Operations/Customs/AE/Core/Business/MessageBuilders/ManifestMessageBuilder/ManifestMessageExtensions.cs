using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.AE.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AE.Business;

public static class ManifestMessageExtensions
{
	public static OrgHeader AeOrgProxyForManifestMessage(BusinessObjectFactory factory)
	{
		if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedArabEmirates)
		{
			return GlbCompany.CurrentCompany.OrgProxy;
		}

		OrgHeader result = null;
		var branch = AeBranchForManifestMessageFromRegistry(factory);
		if (branch != null)
		{
			var cusCodes = AECustomsRegistry.GetActiveAgentOrgCusCodes(branch);
			if (cusCodes.Any())
			{
				result = branch.OrgProxy;
			}
		}
		return result;
	}

	public static GlbBranch AeBranchForManifestMessageFromRegistry(BusinessObjectFactory factory)
	{
		var registry = AECustomsRegistry.Instance.DefaultBranchForManifestSubmission.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
		if (string.IsNullOrEmpty(registry))
		{
			return null;
		}

		return factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, registry));
	}
}
