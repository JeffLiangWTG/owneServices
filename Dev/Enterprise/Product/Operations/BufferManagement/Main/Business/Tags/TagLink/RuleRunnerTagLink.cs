using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class RuleRunnerTagLink : TagLink
	{
		public RuleRunnerTagLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool InOperationalScope
		{
			get { return Definition == null || Definition.CanRuleUseTags; }
		}

		protected override TagLinkLookups GetNewLookups()
		{
			return new RuleTagLinkLookups(this);
		}
	}
}
