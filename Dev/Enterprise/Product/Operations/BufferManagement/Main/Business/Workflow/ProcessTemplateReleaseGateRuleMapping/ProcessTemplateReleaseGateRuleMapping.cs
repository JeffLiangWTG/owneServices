using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessTemplateReleaseGateRuleMapping : AutoProcessTemplateReleaseGateRuleMapping
	{
		public ProcessTemplateReleaseGateRuleMapping(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject(nameof(Rule))]
		public override ZGuid RGM_RGR_Rule
		{
			get => base.RGM_RGR_Rule;
			set => base.RGM_RGR_Rule = value;
		}

		#endregion

		#region Related Business Objects

		public ProcessTemplateReleaseGateRule Rule => Factory.Load<ProcessTemplateReleaseGateRule>(RGM_RGR_Rule);

		#endregion
	}
}

