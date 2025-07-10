using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport;

public static class AddressHelper
{
	public static OrgAddress GetReportAddress(OrgHeader orgProxy)
	{
		if (orgProxy?.AddressesActive == null)
		{
			return null;
		}

		var mainOfficeAddresses = orgProxy.AddressesActive.Where(a =>
			a.AddressCapability.GetCapabilityEnabledMain(OrgConstants.AddressType.Office)).Take(2).ToList();

		return mainOfficeAddresses.Count == 1 ? mainOfficeAddresses.Single() : null;
	}
}
