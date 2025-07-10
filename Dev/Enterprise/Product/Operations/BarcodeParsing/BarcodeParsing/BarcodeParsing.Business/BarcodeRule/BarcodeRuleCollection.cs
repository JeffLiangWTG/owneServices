using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeRuleCollection : ActiveBusinessObjectCollection<BarcodeRule>
	{
		public BarcodeRuleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public BarcodeRuleCollection(BarcodeRuleSet ruleSet)
			: base(ruleSet.Factory, ruleSet, null, BarcodeRuleSchema.BRU_BRS_RuleSet)
		{
		}

		protected override bool AllowNew
		{
			get { return base.AllowNew && (Relationship.Master == null || !Relationship.Master.ReadOnly); }
		}

		protected override void SetDefaultsForNewElementCore(BarcodeRule newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			var maxRuleNo = Count > 0 ? this.Max(r => r.BRU_RuleNumber) : ZShort.Zero;
			newElement.BRU_RuleNumber = maxRuleNo + 1;
		}
	}
}
