using System;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using CargoWise.Common;

namespace CargoWise.UniversalCopy
{
	public class TemplateCopyTemplateNode : WrappedCopyTemplateNode
	{
		public TemplateCopyTemplateNode()
		{
		}

		public TemplateCopyTemplateNode(CopyTemplateNode templateNode)
			: base(templateNode.Name)
		{
			Argument.NotNull(templateNode, nameof(templateNode)); // Suggested By ReviewBot 
			TemplateNode = templateNode;
		}

		[XmlIgnore]
		public CopyTemplateNode TemplateNode
		{
			get { return templateNode; }
			private set
			{
				if (value == this)
				{
					throw new InvalidOperationException("TemplateNode cannot reference same node.\r\nNode name: " + Name);
				}

				var wrappedNode = value as WrappedCopyTemplateNode;
				if (wrappedNode != null && wrappedNode.InnerNode == this)
				{
					throw new InvalidOperationException(
						string.Format(CultureInfo.InvariantCulture,
							"TemplateNode cannot reference same node via WrappedNode.InnerNode.\r\nNode name: {0}, TemplateNode name and type: {1} ({2})",
							Name, value.Name, value.GetType().Name));
				}

				templateNode = value;
				if (value != null)
				{
					TemplateNodeId = value.Id;
				}
			}
		}
		CopyTemplateNode templateNode;

		[XmlIgnore]
		public string TemplateNodeId
		{
			get { return templateNodeId; }
			set
			{
				if (value == Id)
				{
					throw new InvalidOperationException("TemplateNodeId cannot reference same node.");
				}
				templateNodeId = value;
			}
		}
		string templateNodeId;

		public void FindTemplateAndInitializeInnerNode(CopyTemplateNode root)
		{
			if (InnerNode == null)
			{
				if (TemplateNode == null && !string.IsNullOrEmpty(TemplateNodeId) && root != null)
				{
					TemplateNode = root.FindNode(TemplateNodeId);
				}
				if (TemplateNode != null)
				{
					InnerNode = CloneNode(TemplateNode);
				}
			}
		}

		internal static CopyTemplateNode CloneNode(CopyTemplateNode sourceNode, bool resetAndUpdateId = true)
		{
			if (sourceNode == null)
			{
				return null;
			}

			return CloneNodeForNotNullSource(sourceNode, resetAndUpdateId);
		}

		internal static CopyTemplateNode CloneNodeForNotNullSource(CopyTemplateNode sourceNode, bool resetAndUpdateId = true)
		{
			using (var stream = new MemoryStream())
			{
				CopyTemplateTree templateTree = sourceNode as CopyTemplateTree;
				bool shouldWrap = templateTree == null;
				if (shouldWrap)
				{
					templateTree = new CopyTemplateTree { InnerNode = sourceNode };
				}

				templateTree.Serialize(stream);
				stream.Seek(0, SeekOrigin.Begin);

				CopyTemplateTree clonedTree = CopyTemplateTree.Deserialize(stream);

				CopyTemplateNode result = shouldWrap ? clonedTree.InnerNode : clonedTree;
				CopyTemplateTree.ExtendNode(result, sourceNode, false);

				if (resetAndUpdateId)
				{
					result.ResetAndUpdateId();
				}

				return result;
			}
		}

		internal override void ResetAndUpdateId()
		{
			InnerNode = null;
			base.ResetAndUpdateId();
		}
	}
}
