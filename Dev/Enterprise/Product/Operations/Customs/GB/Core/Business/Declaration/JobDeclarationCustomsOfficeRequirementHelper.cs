using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business
{
	public class JobDeclarationCustomsOfficeRequirementHelper : EU.Business.JobDeclarationCustomsOfficeRequirementHelper
	{
		public JobDeclarationCustomsOfficeRequirementHelper(JobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override string GetCacheKeyCombination()
		{
			return string.Join(",", base.GetCacheKeyCombination(), Declaration.JE_ApplicationCode);
		}

		protected override CustomsOfficeRequirement GetMainOffice()
		{
			return Declaration.ApplicationExtender.GetMainOffice(Declaration);
		}

		protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements()
		{
			return Declaration.ApplicationExtender.GetOtherCustomsOfficeRequirements(Declaration);
		}
	}
}
