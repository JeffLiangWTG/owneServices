using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.ErrorReporting.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable IDE0005 // Res.GetString for the .Net 8 build
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005 // Res.GetString for the .Net 8 build

namespace Enterprise.ErrorReporting.GUI
{
	[SuppressMessage("CargoWiseOne", "CW1178:Use source generated static methods", Justification = "Baseline")]
	public partial class ErrorDetailsForm : ZForm
	{
		public ErrorDetailsForm(StmErrorReport errorReport)
			: base(errorReport)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, zPostingButtonsUserControl);
		}

		public override string FormCaption
		{
			get { return BusinessEntity.HumanReadableName; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			PopulateTreeView(zTreeView, ErrorReport.QER_ReportXml);
		}

		StmErrorReport ErrorReport
		{
			get { return (StmErrorReport)BusinessEntity; }
		}

#if DEBUG
		internal TreeView TreeView { get { return zTreeView; } }
#endif

		static void PopulateTreeView(ZTreeView treeView, ZString reportXml)
		{
			treeView.Nodes.Clear();
			XDocument document;

			try
			{
				document = XDocument.Parse(reportXml);
			}
			catch (XmlException ex)
			{
				treeView.Nodes.Add(Res.GetString("2e382dc6-31cd-45b1-9a7b-974c31de39af", "An error occurred while parsing XML: {0}", ex.Message));
				return;
			}

			treeView.Nodes.Add(BuildTreeNode(document.Root));
		}

		static TreeNode BuildTreeNode(XElement element)
		{
			var treeNode = new TreeNode();

			if (element.HasAttributes)
			{
				var attributesNode = new TreeNode(Res.GetString("7A3C8442-D649-4BFC-BE7A-5828115624AA", "Attributes"));
				attributesNode.Nodes.AddRange(element.Attributes().Select(BuildTreeNode).ToArray());
				treeNode.Nodes.Add(attributesNode);
			}

			if (element.HasElements)
			{
				var elementsNode = new TreeNode(Res.GetString("9737EAB7-4C9F-423E-AA0A-2C9F46E352E8", "Elements"));
				elementsNode.Nodes.AddRange(element.Elements().Select(BuildTreeNode).ToArray());
				treeNode.Nodes.Add(elementsNode);
			}

			treeNode.Text = BuildNameForTreeNode(element);

			return treeNode;
		}

		static string BuildNameForTreeNode(XElement element)
		{
			var textNodes = element.Nodes().Where(x => x.NodeType == XmlNodeType.Text).Cast<XText>();
			var innerText = string.Join(string.Empty, textNodes.Select(x => x.Value).ToArray());

			if (string.IsNullOrEmpty(innerText))
			{
				return element.Name.LocalName;
			}
			else
			{
				return Res.GetString("D4E66B60-71E9-4494-9F72-74FAF090C80D", "{0}: {1}", element.Name.LocalName, innerText);
			}
		}

		static TreeNode BuildTreeNode(XAttribute attribute)
		{
			return new TreeNode(Res.GetString("D4E66B60-71E9-4494-9F72-74FAF090C80D", "{0}: {1}", attribute.Name.LocalName, attribute.Value));
		}

		protected override void SetReadOnlyIncludingChildren()
		{
			this.SetReadOnlyIncludingChildren(new List<string>() { "CurrentRecordNumberCalcEdit" });
		}
	}
}
