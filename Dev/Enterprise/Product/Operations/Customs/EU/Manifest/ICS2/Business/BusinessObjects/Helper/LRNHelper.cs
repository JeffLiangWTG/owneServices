using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business;

public static class LRNHelper
{
	public static string GenerateLRN(BusinessObjectFactory factory, GlbCompany company, GlbBranch branch, ZString specificCircumstanceIndicator)
	{
		return string.Format("{0:3}{1:3}{2:3}{3:2}{4:11}",
			company.GC_Code,
			branch.GB_Code,
			specificCircumstanceIndicator,
			ZDateTime.UtcNow.ToString("yy"),
			Env.NumberFountains.EUICS2LocalReferenceNumber(company.PK.ToGuid()).GetNextFormatted(factory));
	}
}
