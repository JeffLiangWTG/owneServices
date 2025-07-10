using System.Xml.Serialization;
using CargoWise.Common;
using WTG.Glow.Data.Annotations;

namespace CargoWise.UniversalCopy
{
	public class CollectionCopyTemplateNode : WrappedCopyTemplateNode
	{
		public CollectionCopyTemplateNode()
		{
		}

		public CollectionCopyTemplateNode(CollectionRelationPropertyAttribute relationAttribute, string itemsTableName, CopyTemplateNode itemEntityNode)
			: base(itemEntityNode)
		{
			Argument.NotNull(itemEntityNode, nameof(itemEntityNode));
			Argument.NotNull(relationAttribute, nameof(relationAttribute));
			ItemPropertyName = relationAttribute.ItemPropertyName;
			ItemsTableName = itemsTableName;

			Name = relationAttribute.CollectionName;
		}

		public override string GetTableName()
		{
			return ItemsTableName;
		}

		[XmlAttribute(AttributeName = "ItemPropertyName")]
		public string ItemPropertyName { get; set; }

		[XmlAttribute(AttributeName = "ItemParentTablePropertyName")]
		public string ItemParentTablePropertyName { get; set; }

		[XmlAttribute(AttributeName = "ItemTableName")]
		public string ItemsTableName { get; set; }

		[XmlAttribute(AttributeName = "Do")]
		public CollectionCopyMethod CopyMethod { get; set; }

		[XmlAttribute(AttributeName = "IsSplitCollection")]
		public bool IsSplitCollection { get; set; }

		public EntityFilter Filter { get; set; }

		[XmlAttribute(AttributeName = "Order")]
		public int Order { get; set; }

		public string SplitOwner { get; set; }

		public string CollectionId { get; set; }

		internal override void ResetAndUpdateId()
		{
			base.ResetAndUpdateId();
			CopyMethod = default;
			if (Filter == null || Filter.FilterTypeId != EntityFilterTypeIds.MandatoryExpressionFilter)
			{
				Filter = null;
				Order = 0;
			}
		}

		public override bool HasData()
		{
			return CopyMethod != CollectionCopyMethod.None || CollectionId != null;
		}

		internal override void CopyTransientData(CopyTemplateNode sourceNode)
		{
			base.CopyTransientData(sourceNode);

			if (sourceNode is CollectionCopyTemplateNode sourceCollectionNode)
			{
				if (Filter == null && sourceCollectionNode.Filter != null && sourceCollectionNode.Filter.FilterTypeId == EntityFilterTypeIds.MandatoryExpressionFilter)
				{
					Filter = new EntityFilter
					{
						FilterTypeId = sourceCollectionNode.Filter.FilterTypeId,
						FilterData = sourceCollectionNode.Filter.FilterData,
					};
					Order = sourceCollectionNode.Order;
				}
			}
		}
	}
}
