using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public static class AFRReporterHelper
	{
		public static AFRReporterID GetReporter(this GlbCompany company)
		{
			return JPAFRRegistry.Instance.AFRReporterIDForDocument.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
		}

		public static void SetReporter(this GlbCompany company, AFRReporterID newReporter)
		{
			JPAFRRegistry.Instance.AFRReporterIDForDocument.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, newReporter);
		}

		public const string ConfigurationName = "JPAFR";
	}
}
