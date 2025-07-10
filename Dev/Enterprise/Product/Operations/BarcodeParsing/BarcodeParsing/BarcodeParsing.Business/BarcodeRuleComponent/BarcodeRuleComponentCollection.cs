using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeRuleComponentCollection : ActiveBusinessObjectCollection<BarcodeRuleComponent>
	{
		public BarcodeRuleComponentCollection(BarcodeRule rule)
			: base(rule.Factory, rule, null, BarcodeRuleComponentSchema.BRC_BRU_Rule)
		{
			ApplySort(BarcodeRuleComponentSchema.BRC_Sequence.Name, ListSortDirection.Ascending);
		}

		protected override void SetDefaultsForNewElementCore(BarcodeRuleComponent newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			var rule = (BarcodeRule)Relationship.Master;
			if (!rule.IsPartialRule)
			{
				var maxSequenceNo = Count > 0 ? this.Max(c => c.BRC_Sequence) : ZShort.Zero;
				newElement.BRC_Sequence = maxSequenceNo + 1;
			}
		}

		protected override bool AllowNew
		{
			get { return Count == 0 || !((BarcodeRule)Relationship.Master).IsPartialRule; }
		}
	}
}
