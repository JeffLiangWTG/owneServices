using System.Xml.Serialization;
using CargoWise.Common;

namespace CargoWise.UniversalCopy
{
	public class RelatedEntityCopyTemplateNode : WrappedCopyTemplateNode
	{
		public RelatedEntityCopyTemplateNode()
		{
		}

		public RelatedEntityCopyTemplateNode(string propertyName, string relatedPropertyName, string relatedEntityTableName, CopyTemplateNode relatedEntityNode)
			: base(relatedEntityNode)
		{
			Argument.NotNull(relatedEntityNode, nameof(relatedEntityNode));
			RelatedPropertyName = relatedPropertyName;
			Name = propertyName;
			RelatedEntityTableName = relatedEntityTableName;
		}

		[XmlAttribute(AttributeName = "RelatedPropertyName")]
		public string RelatedPropertyName { get; set; }

		[XmlAttribute(AttributeName = "RelatedEntityTableName")]
		public string RelatedEntityTableName { get; set; }

		[XmlAttribute(AttributeName = "Do")]
		public RelatedEntityCopyMethod CopyMethod { get; set; }

		internal override void ResetAndUpdateId()
		{
			base.ResetAndUpdateId();
			CopyMethod = default(RelatedEntityCopyMethod);
		}

		public override bool HasData()
		{
			return CopyMethod != RelatedEntityCopyMethod.None;
		}

		internal override void CopyTransientData(CopyTemplateNode sourceNode)
		{
			base.CopyTransientData(sourceNode);

			var sourceRelatedEntityNode = sourceNode as RelatedEntityCopyTemplateNode;
			if (sourceRelatedEntityNode != null)
			{
				DisableCopyMethodCopy = sourceRelatedEntityNode.DisableCopyMethodCopy;
				DisableCopyMethodLink = sourceRelatedEntityNode.DisableCopyMethodLink;
				AllowCopyMethodLinkCopiedWhenLinkIsDisabled = sourceRelatedEntityNode.AllowCopyMethodLinkCopiedWhenLinkIsDisabled;
				CanCopyWithZeroNodes = sourceRelatedEntityNode.CanCopyWithZeroNodes;
			}
		}

		[XmlIgnore]
		public virtual bool CanCopy
		{
			get { return !DisableCopyMethodCopy && CanCopyCore(InnerNode); }
		}

		bool CanCopyCore(CopyTemplateNode node)
		{
			while (node is TemplateCopyTemplateNode)
			{
				var templateNode = (TemplateCopyTemplateNode)node;
				node = templateNode.InnerNode;
			}

			var entityNode = node as EntityCopyTemplateNode;
			return entityNode == null || entityNode.Nodes.Count != 0 || CanCopyWithZeroNodes;
		}

		[XmlIgnore]
		public virtual bool CanLink
		{
			get { return !DisableCopyMethodLink; }
		}

		[XmlIgnore]
		public bool DisableCopyMethodCopy { get; set; }

		[XmlIgnore]
		public bool DisableCopyMethodLink { get; set; }

		[XmlIgnore]
		public bool AllowCopyMethodLinkCopiedWhenLinkIsDisabled { get; set; }

		[XmlIgnore]
		public bool CanCopyWithZeroNodes { get; set; }
	}
}
