using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using CargoWise.Common;

namespace CargoWise.UniversalCopy
{
	public class EntityCopyTemplateNode : CopyTemplateNode
	{
		public EntityCopyTemplateNode()
		{
		}

		public EntityCopyTemplateNode(Type type)
			: base(RemoveIFromInterfaceName(type.Name))
		{
			Argument.NotNull(type, nameof(type)); // Suggested By ReviewBot 
		}

		static string RemoveIFromInterfaceName(string name)
		{
			Argument.NotNull(name, nameof(name)); // Suggested By ReviewBot 
			return name.Length > 2 && name[0] == 'I' && char.IsUpper(name[1]) && char.IsLower(name[2]) ? name.Substring(1) : name;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		[XmlElement(typeof(PropertyCopyTemplateNode), ElementName = "P")
		, XmlElement(typeof(CollectionCopyTemplateNode), ElementName = "C")
		, XmlElement(typeof(RelatedEntityCopyTemplateNode), ElementName = "R")]
		public List<CopyTemplateNode> Nodes
		{
			get { return nodes; }
		}
		readonly List<CopyTemplateNode> nodes = new List<CopyTemplateNode>();

		public override CopyTemplateNode FindNode(string id)
		{
			return base.FindNode(id) ?? Nodes.Select(node => node.FindNode(id)).FirstOrDefault(node => node != null);
		}

		internal override void ResetAndUpdateId()
		{
			base.ResetAndUpdateId();
			foreach (CopyTemplateNode node in Nodes)
			{
				node.ResetAndUpdateId();
			}
		}

		[XmlElement(typeof(PropertyCopyTemplateNode), ElementName = "V")]
		public List<PropertyCopyTemplateNode> ValueOnlyNodes
		{
			get { return valueOnlyNodes ?? (valueOnlyNodes = new List<PropertyCopyTemplateNode>()); }
		}
		List<PropertyCopyTemplateNode> valueOnlyNodes;

		[XmlIgnore]
		internal bool FullyBuilt { get; set; }

		public override bool HasData()
		{
			return Nodes.Any(node => node.HasData());
		}

		public static EntityCopyTemplateNode GetEntityTemplateNodeFromTopLevelCopyTemplateNode(CopyTemplateNode parentCopyNode)
		{
			WrappedCopyTemplateNode wrappedCopyNode = parentCopyNode as WrappedCopyTemplateNode;
			if (wrappedCopyNode != null)
			{
				return GetEntityTemplateNodeFromTopLevelCopyTemplateNode(wrappedCopyNode.InnerNode);
			}

			EntityCopyTemplateNode entityCopyNode = parentCopyNode as EntityCopyTemplateNode;
			if (entityCopyNode != null)
			{
				return entityCopyNode;
			}

			return null;
		}

		public override bool FindPathToNode(CopyTemplateNode node, StringBuilder pathBuilder, bool includeType)
		{
			if (base.FindPathToNode(node, pathBuilder, includeType))
			{
				return true;
			}

			if (Nodes.Any(n => n.FindPathToNode(node, pathBuilder, includeType)))
			{
				InsertNameToPath(pathBuilder, includeType);
				return true;
			}

			return false;
		}
	}
}
