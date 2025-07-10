using System;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class SalesRelationDirectionRule : RegistryBusinessObjectTemplate
	{
		public SalesRelationDirectionRule()
			: base(null, null)
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string NodeSequenceAsString = "NodeSequenceAsString";
		}

		#endregion

		#region Nodes

		public SalesRelationRuleNodeCollection Nodes
		{
			get
			{
				if (nodes == null)
				{
					nodes = new SalesRelationRuleNodeCollection();
					((IBindingList)nodes).ListChanged += Nodes_ListChanged;
					RegisterEditableChildObject(nodes);
				}
				return nodes;
			}
		}
		SalesRelationRuleNodeCollection nodes;

		#region NodeSequenceAsString

		public ZString NodeSequenceAsString
		{
			get { return string.Join(pathSeperator, Nodes.Cast<SalesRelationRuleNode>().Select(node => node.Type)); }
		}

		public ZPropertyInfo NodeSequenceAsStringInfo
		{
			get { return GetZPropertyInfo(nameof(NodeSequenceAsString), "Sequence"); }
		}

		#endregion

		#endregion

		#region Xml Serialization

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			var nodeSequenceAsString = reader.ReadElementString(Schema.NodeSequenceAsString).Split(new[] { pathSeperator }, StringSplitOptions.None);
			foreach (var nodeAsString in nodeSequenceAsString)
			{
				var node = Nodes.AddNew();
				node.Type = nodeAsString;
			}
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.NodeSequenceAsString, NodeSequenceAsString);
		}

		const string pathSeperator = " > ";

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SalesRelationDirectionRule();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var clonedRule = (SalesRelationDirectionRule)clone;
			using (clonedRule.GetValidationSuspender())
			{
				foreach (SalesRelationRuleNode node in Nodes)
				{
					var clonedNode = clonedRule.Nodes.AddNew();
					clonedNode.Type = node.Type;
				}
			}
		}

		#endregion

		#region Validation

		void Nodes_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (!IsValidationSuspended)
			{
				ValidationNodeSequenceAsString();
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidationNodeSequenceAsString();
		}

		public void ValidationNodeSequenceAsString()
		{
			NodeSequenceAsStringInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(NodeSequenceAsStringInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(NodeSequenceAsStringInfo);

			if (Nodes.Count > 0 && Nodes.Count < minimumSequenceLength)
			{
				NodeSequenceAsStringInfo.AddError(ResString.GetMultilingualString("004ccf0b-b6bb-421d-b4bd-6e81304f5a6d", "Must contain at least {0} types.", minimumSequenceLength));
			}
		}

		const int minimumSequenceLength = 2;

		#endregion
	}
}
