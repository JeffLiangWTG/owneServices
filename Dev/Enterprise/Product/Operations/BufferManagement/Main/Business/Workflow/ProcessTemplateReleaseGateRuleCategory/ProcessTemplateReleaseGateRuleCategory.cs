using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessTemplateReleaseGateRuleCategory : AutoProcessTemplateReleaseGateRuleCategory
	{
		public ProcessTemplateReleaseGateRuleCategory(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject(nameof(Rule))]
		public override ZGuid RGC_RGR_Rule
		{
			get => base.RGC_RGR_Rule;
			set => base.RGC_RGR_Rule = value;
		}

		#endregion

		#region Related Business Objects

		public ProcessTemplateReleaseGateRule Rule => Factory.Load<ProcessTemplateReleaseGateRule>(RGC_RGR_Rule);

		#endregion
	}
}

