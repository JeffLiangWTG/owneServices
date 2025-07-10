using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessTemplateReleaseGroupRuleMapping : IBusiness
	{
		ZString PTM_Value { get; set; }

		ZGuid PTM_GG_Group { get; set; }

		IGlbGroup ReleaseGroup { get; }
	}
}
