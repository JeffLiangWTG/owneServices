using System;
using System.Text;
using System.Xml.Serialization;
using CargoWise.Common;

namespace CargoWise.UniversalCopy
{
	public abstract class WrappedCopyTemplateNode : CopyTemplateNode
	{
		protected WrappedCopyTemplateNode()
		{
		}

		protected WrappedCopyTemplateNode(string caption)
			: base(caption)
		{
		}

		protected WrappedCopyTemplateNode(CopyTemplateNode innerNode)
			: this(innerNode.Name)
		{
			Argument.NotNull(innerNode, nameof(innerNode));
			InnerNode = innerNode;
		}

		[XmlElement(typeof(PropertyCopyTemplateNode), ElementName = "P")
		, XmlElement(typeof(EntityCopyTemplateNode), ElementName = "E")
		, XmlElement(typeof(CollectionCopyTemplateNode), ElementName = "C")
		, XmlElement(typeof(RelatedEntityCopyTemplateNode), ElementName = "R")
		, XmlElement(typeof(TemplateCopyTemplateNode), ElementName = "T")]
		public CopyTemplateNode InnerNode
		{
			get { return innerNode; }
			set
			{
				if (value == this)
				{
					throw new InvalidOperationException("InnerNode cannot reference same node\r\nNode name: " + Name);
				}
				innerNode = value;
			}
		}
		CopyTemplateNode innerNode;

		public override CopyTemplateNode FindNode(string id)
		{
			return base.FindNode(id) ?? (InnerNode != null ? InnerNode.FindNode(id) : null);
		}

		internal override void ResetAndUpdateId()
		{
			base.ResetAndUpdateId();

			if (InnerNode != null)
			{
				InnerNode.ResetAndUpdateId();
			}
		}

		public override bool HasData()
		{
			return InnerNode != null && InnerNode.HasData();
		}

		public override bool FindPathToNode(CopyTemplateNode node, StringBuilder pathBuilder, bool includeType)
		{
			if (base.FindPathToNode(node, pathBuilder, includeType))
			{
				return true;
			}

			if (InnerNode != null && InnerNode.FindPathToNode(node, pathBuilder, includeType))
			{
				InsertNameToPath(pathBuilder, includeType);
				return true;
			}

			return false;
		}
	}
}
