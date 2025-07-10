using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessTemplateReleaseGroupRuleMapping : AutoProcessTemplateReleaseGroupRuleMapping, IProcessTemplateReleaseGroupRuleMapping
	{
		public ProcessTemplateReleaseGroupRuleMapping(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject(nameof(ReleaseGroup))]
		public override ZGuid PTM_GG_Group { get => base.PTM_GG_Group; }

		#endregion

		#region Related Business Objects

		public ProcessTemplateReleaseGroupRule Rule => Factory.Load<ProcessTemplateReleaseGroupRule>(PTM_PTR_Rule);

		public GlbGroup ReleaseGroup => Factory.Load<GlbGroup>(PTM_GG_Group);

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region IProcessTemplateReleaseGroupRuleMapping Members

		IGlbGroup IProcessTemplateReleaseGroupRuleMapping.ReleaseGroup => ReleaseGroup;

		#endregion
	}
}
