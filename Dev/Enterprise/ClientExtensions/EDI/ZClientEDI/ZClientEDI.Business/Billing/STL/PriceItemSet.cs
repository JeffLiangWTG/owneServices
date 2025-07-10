using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Client.EDI.Billing.Business
{
	public sealed class PriceItemSet
	{
		public PriceItemSet(IEnumerable<ClientLicencePriceItem> items)
		{
			AllItems = items;
			KeyToHighestBreakPriceNode = new Dictionary<UsageCodeKey, PriceNode>();
			CountryTierCodeToPriceNode = new Dictionary<string, PriceNode>();
			allNodes = new List<PriceNode>(items.Count());

			List<PriceNode> parentStack = new List<PriceNode>();
			foreach (var priceItem in items.OrderBy(x => x.L7_Order))
			{
				int indent = priceItem.DescriptionIndentLevel;

				while (parentStack.Count > 0 && parentStack[parentStack.Count - 1].Item.DescriptionIndentLevel >= indent)
				{
					parentStack.RemoveAt(parentStack.Count - 1);
				}

				var node = new PriceNode(priceItem, parentStack.Count > 0 ? parentStack[parentStack.Count - 1] : null);
				parentStack.Add(node);
				allNodes.Add(node);

				if (!priceItem.L7_Code.IsEmpty)
				{
					PriceNode other;
					string code = priceItem.L7_Code;
					if (!priceItem.L7_Ref4.IsEmpty)
					{
						code += '.' + priceItem.L7_Ref4;
					}
					var key = new UsageCodeKey(priceItem.L7_Category, code);

					if (priceItem.L7_FeeType == BillingConstants.FeeType.CountryTier)
					{
						CountryTierCodeToPriceNode[priceItem.L7_CountryTierCode] = node;
					}
					else if (!KeyToHighestBreakPriceNode.TryGetValue(key, out other) || other.Item.L7_UnitBreak < priceItem.L7_UnitBreak)
					{
						KeyToHighestBreakPriceNode[key] = node;
					}
				}
			}
		}

		readonly List<PriceNode> allNodes;
		public IEnumerable<PriceNode> AllNodesWithCode { get { return allNodes.Where(x => !x.Item.L7_Code.IsEmpty); } }
		public IEnumerable<ClientLicencePriceItem> AllItems { get; private set; }
		public Dictionary<UsageCodeKey, PriceNode> KeyToHighestBreakPriceNode { get; private set; }
		public Dictionary<string, PriceNode> CountryTierCodeToPriceNode { get; private set; }
	}

	public sealed class PriceNode
	{
		public PriceNode(ClientLicencePriceItem priceItem, PriceNode categoryParent)
		{
			this.Item = priceItem;
			this.CategoryParent = categoryParent;
		}

		public ClientLicencePriceItem Item { get; private set; }
		public PriceNode CategoryParent { get; private set; }

		public ClientLicencePriceItem FindCategoryItemByIndentLevel(int indentLevel)
		{
			var node = FindCategoryNodeByIndentLevel(indentLevel);
			return node != null ? node.Item : null;
		}

		public PriceNode FindCategoryNodeByIndentLevel(int indentLevel)
		{
			var node = CategoryParent;
			while (node != null)
			{
				int itemLevel = node.Item.DescriptionIndentLevel;
				if (itemLevel == indentLevel)
				{
					return node;
				}
				else
				{
					node = node.CategoryParent;
				}
			}

			return null;
		}
	}
}
