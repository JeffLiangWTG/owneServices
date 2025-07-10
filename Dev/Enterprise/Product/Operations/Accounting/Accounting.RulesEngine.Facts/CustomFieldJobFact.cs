using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting;
using WTG.ProductionRules.Core;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class CustomFieldJobFact : JobFact, IInputFactWithCustomFields
	{
		public CustomFieldJobFact(JobHeader job, IEnvironmentFact environmentFact, IOrganisationWithMainAddressFact localClientFact, IStaffFact salesRepFact)
			: base(job, environmentFact, localClientFact, salesRepFact)
		{
		}

		public string GetCustomFieldById(string customFieldId)
		{
			return GetCustomFieldByIdCore(customFieldId);
		}

		protected virtual string GetCustomFieldByIdCore(string customFieldId)
		{
			//TODO: to be implemented in the WI00808404
			return string.Empty;
		}
	}
}
