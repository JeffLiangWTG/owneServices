using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessHeaderLink : IBusiness
	{
		ZGuid PK { get; }
		[List("Lookups.HeaderFroms")]
		ZGuid FP_FH_HeaderFrom { get; set; }
		[List("Lookups.HeaderTos")]
		ZGuid FP_FH_HeaderTo { get; set; }

		[List("Lookups.Templates")]
		ZGuid FromWorkflowExternalTemplatePK { get; set; }
		[List("Lookups.Templates")]
		ZGuid ToWorkflowExternalTemplatePK { get; set; }

		[List("Lookups.LinkTypeList")]
		ZString FP_LinkType { get; set; }
		ZString LinkTypeDescription { get; }

		ZDecimal FP_TimeDelayFactor { get; set; }
		ZDateTime StaggeredReleaseTimeDelay { get; set; }

		IProcessHeaderLink Clone();
		IProcessHeader HeaderFrom { get; }
		IProcessHeader HeaderTo { get; }
	}
}
