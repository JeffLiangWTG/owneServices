using System;
using System.Diagnostics;
using System.Text;
using System.Xml.Serialization;
using CargoWise.Common;

namespace CargoWise.UniversalCopy
{
	[DebuggerDisplay("{GetType().Name}: {Name}, {Id}")]
	[XmlInclude(typeof(PropertyCopyTemplateNode))]
	[XmlInclude(typeof(EntityCopyTemplateNode))]
	[XmlInclude(typeof(CollectionCopyTemplateNode))]
	[XmlInclude(typeof(RelatedEntityCopyTemplateNode))]
	[XmlInclude(typeof(TemplateCopyTemplateNode))]
	public abstract class CopyTemplateNode
	{
		protected CopyTemplateNode()
		{
		}

		protected CopyTemplateNode(string caption)
		{
			Id = Guid.NewGuid().ToString();
			Name = caption;
		}

		[XmlIgnore]
		public string Id { get; set; }

		[XmlAttribute(AttributeName = "N")]
		public string Name { get; set; }

		[XmlAttribute(AttributeName = "Desc")]
		public string Description { get; set; }

		[XmlIgnore]
		public bool IsMandatory { get; set; }

		[XmlIgnore]
		public int Priority { get; set; }

		public virtual CopyTemplateNode FindNode(string id)
		{
			return id == Id ? this : null;
		}

		public virtual string GetTableName()
		{
			return Name;
		}

		internal virtual void ResetAndUpdateId()
		{
			Id = Guid.NewGuid().ToString();
		}

		public abstract bool HasData();

		internal virtual void CopyTransientData(CopyTemplateNode sourceNode)
		{
			Argument.NotNull(sourceNode, nameof(sourceNode));
			if (string.IsNullOrEmpty(Description))
			{
				Description = sourceNode.Description;
			}
			Priority = sourceNode.Priority;
			IsMandatory = sourceNode.IsMandatory;
		}

		public virtual bool FindPathToNode(CopyTemplateNode node, StringBuilder pathBuilder, bool includeType)
		{
			Argument.NotNull(pathBuilder, nameof(pathBuilder));
			if (node == this)
			{
				InsertNameToPath(pathBuilder, includeType);
				return true;
			}
			return false;
		}

		protected void InsertNameToPath(StringBuilder pathBuilder, bool includeType)
		{
			Argument.NotNull(pathBuilder, nameof(pathBuilder));
			if (pathBuilder.Length > 0)
			{
				pathBuilder.Insert(0, '\\');
			}
			pathBuilder.Insert(0, (includeType ? GetType().Name + " " : "") + Name);
		}
	}
}
