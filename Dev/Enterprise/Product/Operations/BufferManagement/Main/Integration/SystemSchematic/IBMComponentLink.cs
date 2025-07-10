using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMComponentLink
	{
		ZGuid PK { get; }
		ZGuid FL_FC_ComponentFrom { get; set; }
		ZGuid FL_FC_ComponentTo { get; set; }
		ZBool FL_TransferRulesEnabled { get; set; }
		ZBool FL_IsReleaseGateRuleApplied { get; set; }
		bool HasNoFilterWhenRequired { get; }
		ZString DisplayText { get; }

		StmModuleFilter FilterRule { get; }
	}
}
