using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using TreeNode = Enterprise.ZArchitecture.Web.Business.TreeNode;
using TreeNodeCollection = Enterprise.ZArchitecture.Web.Business.TreeNodeCollection;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	/// <summary>
	/// TreeView that displays the description within a text box below the TreeView.
	/// </summary>
	[ToolboxData("<{0}:ZTreeViewWithDescription runat=server />"),
	Designer(typeof(Design.ZTreeViewWithDescriptionDesigner))]
	public class ZTreeViewWithDescription : ZTreeView
	{
		protected const string DisplayDescriptionFunctionName = "DisplayDescriptionAndQuantity";
		protected const int MarginBetweenTreeViewAndDescriptionBox = 20;

		public ZTreeViewWithDescription() : base()
		{
		}

		#region Properties

		public override Unit Height
		{
			get
			{
				return base.Height;
			}
			set
			{
				if (value.Value < (HeightOfAdditionalComponents.Value * 2))
				{
					base.Height = new Unit(HeightOfAdditionalComponents.Value * 2);
				}
				else
				{
					base.Height = value;
				}
			}
		}

		protected Unit fDescriptionHeight;
		[Category("Layout"), DefaultValue(100), Description("Height of Description TextBox")]
		public Unit DescriptionHeight
		{
			get { return fDescriptionHeight; }
			set
			{
				if (value.Value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}
				fDescriptionHeight = value;
			}
		}

		protected Unit fQuantityHeight;
		[Category("Layout"), DefaultValue(50), Description("Height of Quantity TextBox")]
		public Unit QuantityHeight
		{
			get { return fQuantityHeight; }
			set
			{
				if (value.Value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}
				fQuantityHeight = value;
			}
		}

		Unit HeightOfAdditionalComponents
		{
			get
			{
				return (new Unit((MarginBetweenTreeViewAndDescriptionBox * 2) + DescriptionHeight.Value + QuantityHeight.Value));
			}
		}

		#endregion

		#region Implementation

		#region RenderOverrides

		protected override void Render(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);
			writer.AddStyleAttribute(HtmlTextWriterStyle.Height, Height.Value.ToString());
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Width.Value.ToString());
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			base.Render(writer);
			RenderDescriptionAndQuantity(writer);
			writer.RenderEndTag();
		}

		#region TreeView Appearance

		protected override void RenderTreeViewDimensionStyleAttributes(HtmlTextWriter writer)
		{
			if (!Height.IsEmpty)
			{
				double treeViewHeight = Height.Value - ((MarginBetweenTreeViewAndDescriptionBox * 2) + DescriptionHeight.Value + QuantityHeight.Value);
				writer.AddStyleAttribute(HtmlTextWriterStyle.Height, treeViewHeight.ToString());
			}
			if (!Width.IsEmpty)
			{
				writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Width.Value.ToString());
			}
		}

		#endregion TreeView Appearance

		#region NodeCellDetail Overrides

		protected override void RenderNodeCellDetails(HtmlTextWriter writer, TreeNode node, string id)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, SelectFunction(DescriptionBoxID, id, Code(node.Tag), Description(node.Tag), QuantityBoxID, Quantity(node.Tag)));
			if (node.Tag.Equals(SelectedItem))
			{
				SelectedPath = id;
			}
			writer.AddStyleAttribute("cursor", "hand");
			writer.AddStyleAttribute("text-overflow", "ellipsis");
			writer.RenderBeginTag(HtmlTextWriterTag.Span);
			writer.Write(node.Text);
			writer.RenderEndTag();
		}

		#endregion NodeCellDetail Overrides

		#endregion RenderOverrides

		#region RenderDescriptionAndQuantity

		protected void RenderDescriptionAndQuantity(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
			writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
			writer.RenderBeginTag(HtmlTextWriterTag.Table);
			RenderDescriptionDiv(writer);
			RenderQuantityDiv(writer);
			writer.RenderEndTag();
			Page.ClientScript.RegisterHiddenField("Code", Code(SelectedItem));
			Page.ClientScript.RegisterHiddenField("Description", Description(SelectedItem));
			Page.ClientScript.RegisterHiddenField("DescriptionControl", DescriptionBoxID);
			Page.ClientScript.RegisterHiddenField("Quantity", Quantity(SelectedItem));
			Page.ClientScript.RegisterHiddenField("QuantityControl", QuantityBoxID);
		}

		#endregion RenderDescriptionAndQuantity

		#region RenderDescription

		protected void RenderDescriptionDiv(HtmlTextWriter writer)
		{
			// Render the spacer div between the description div and the div above it
			writer.AddStyleAttribute(HtmlTextWriterStyle.Height, MarginBetweenTreeViewAndDescriptionBox.ToString());
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.RenderEndTag();

			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			// Render the label for the Description
			writer.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
			writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			RenderDescriptionText(writer);
			writer.RenderEndTag();

			//Render to table cell to contain the description div 
			writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			RenderDescriptionBox(writer);
			writer.RenderEndTag();
			writer.RenderEndTag();
		}

		protected void RenderDescriptionText(HtmlTextWriter writer)
		{
			writer.Write(Res.GetString("b5f74a00-52d1-43a8-a292-58b4c35c98c0", "Full Description"));
		}
		protected void RenderDescriptionBox(HtmlTextWriter writer)
		{
			RenderDescriptionBoxStyle(writer);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.RenderEndTag();
		}

		protected void RenderDescriptionBoxStyle(HtmlTextWriter writer)
		{
			RenderTreeViewStyleAttributes(writer);
			writer.AddStyleAttribute(HtmlTextWriterStyle.Height, DescriptionHeight.Value.ToString());
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "");
			writer.AddAttribute(HtmlTextWriterAttribute.Id, DescriptionBoxID);
		}

		protected string DescriptionBoxID
		{
			get { return this.ID + ".DescriptionBox"; }
		}

		protected void RenderQuantityBoxStyle(HtmlTextWriter writer)
		{
			RenderTreeViewStyleAttributes(writer);
			writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
			writer.AddAttribute(HtmlTextWriterAttribute.Id, QuantityBoxID);
		}

		protected string QuantityBoxID
		{
			get { return this.ID + ".Quantity"; }
		}

		#endregion RenderDescription

		#region RenderQuantity

		protected void RenderQuantityDiv(HtmlTextWriter writer)
		{
			// Render the Quantity Label and Text Box 

			writer.AddStyleAttribute(HtmlTextWriterStyle.Height, QuantityHeight.Value.ToString());
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);
			writer.RenderBeginTag(HtmlTextWriterTag.Td);

			writer.RenderBeginTag(HtmlTextWriterTag.Span);
			writer.Write(Res.GetString("f13f3f4f-89f1-4c09-8635-dbef36859f1b", "Quantity"));
			writer.RenderEndTag();
			writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			RenderQuantityBoxStyle(writer);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.RenderEndTag();
			writer.RenderEndTag();
		}

		#endregion RenderQuantity

		#region Helper Methods

		#region Select Function

		public string SelectFunction(string descTarget, string id, string code, string description, string quantityTarget, string quantity)
		{
			string encCode = ConvertToJavascriptFriendlyString(code);
			string encDescription = ConvertToJavascriptFriendlyString(description);
			return String.Format("{0}('{1}', '{2}', '{3}', '{4}', '{5}', '{6}')", DisplayDescriptionFunctionName, id, descTarget, encCode, encDescription, quantityTarget, quantity);
		}

		static string ConvertToJavascriptFriendlyString(string stringToEncode)
		{
			return stringToEncode.Replace("'", "\'").Replace("\n", "").Replace("\r", "");
		}

		#endregion Select Function

		public delegate string IFamilyMemberHelper(IFamilyMember node);
		public delegate bool IFamilyMemberBoolHelper(IFamilyMember node);

		#region Code Helper

		IFamilyMemberHelper fCodeMethod;
		public IFamilyMemberHelper CodeMethod
		{
			get { return fCodeMethod; }
			set { fCodeMethod = value; }
		}

		public string Code(IFamilyMember node)
		{
			IFamilyMemberHelper helper = CodeMethod;
			if (helper != null)
			{
				return helper(node);
			}
			else
			{
				return String.Empty;
			}
		}

		#endregion Code Helper

		#region Quantity Helper

		IFamilyMemberHelper fQtyMethod;

		public IFamilyMemberHelper QuantityMethod
		{
			get { return fQtyMethod; }
			set { fQtyMethod = value; }
		}

		public string Quantity(IFamilyMember node)
		{
			IFamilyMemberHelper helper = QuantityMethod;
			if (helper != null)
			{
				return helper(node);
			}
			else
			{
				return String.Empty;
			}
		}

		#endregion Quantity Helper

		#region Description Helper

		IFamilyMemberHelper fDescMethod;

		public IFamilyMemberHelper DescriptionMethod
		{
			get { return fDescMethod; }
			set { fDescMethod = value; }
		}

		public string Description(IFamilyMember node)
		{
			IFamilyMemberHelper helper = DescriptionMethod;
			if (helper != null)
			{
				return helper(node).Replace("'", "\\'").Replace("\r", "").Replace("\n", "");
			}
			else
			{
				return String.Empty;
			}
		}

		#endregion Description Helper

		#region IsSelectable Helper

		IFamilyMemberBoolHelper fIsSelectableMethod;

		public IFamilyMemberBoolHelper IsSelectableMethod
		{
			get { return fIsSelectableMethod; }
			set { fIsSelectableMethod = value; }
		}

		public bool IsSelectable(IFamilyMember node)
		{
			IFamilyMemberBoolHelper helper = IsSelectableMethod;
			if (helper != null)
			{
				return helper(node);
			}
			else
			{
				return true;
			}
		}

		#endregion Description Helper

		#endregion Helper Methods

		#endregion Implementation
	}

	#endregion
}

#region ZTreeViewWithDescriptionDesigner

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Design
{
	using System.IO;
	using System.Web.UI.Design;

	public class ZTreeViewWithDescriptionDesigner : ControlDesigner
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "design time only text")]
		public override string GetDesignTimeHtml()
		{
			using (StringWriter stringWriter = new StringWriter())
			{
				HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter);
				ZTreeView control = this.Component as ZTreeViewWithDescription;
				control.IsDesigning = true;
				if (control.Nodes.Count == 0)
				{
					CreateSampleNodeHierarchy(control.Nodes, "Node {0}", 5, 4);
					control.RenderControl(htmlWriter);
					control.Nodes.Clear();
				}
				else
				{
					control.RenderControl(htmlWriter);
				}
				return stringWriter.ToString();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "design time only text")]
		void CreateSampleNodeHierarchy(TreeNodeCollection nodes, string textFormat, int numNodes, int numDescendants)
		{
			for (int i = 0 ; i < numNodes - 1 ; i++)
			{
				string shortDesc = String.Format(textFormat, i);
				string longDesc = String.Format("I am the Top Level Node {0}", i);
				TreeNode node = new TreeNode(new DesignerTreeElement(shortDesc, longDesc));
				nodes.Add(node);
			}
		}
	}

	public class DesignerTreeElement : IFamilyMember
	{
		readonly string fShortDescription;
		readonly string fLongDescription;

		public DesignerTreeElement(string shortDescription, string longDescription)
		{
			fShortDescription = shortDescription;
			fLongDescription = longDescription;
		}

		#region IFamilyMember Members

		public bool HasChildren
		{
			get { return false; }
		}

		public IFamilyMember[] Children
		{
			get { return Array.Empty<DesignerTreeElement>(); }
		}

		public ZPropertyInfo LongDescriptionInfo
		{
			get { return null; }
		}

		public string ShortDescription
		{
			get { return fShortDescription; }
		}

		public CargoWise.Types.ZString LongDescription
		{
			get	{ return fLongDescription; }
		}

		#endregion
	}
}

#endregion ZTreeViewWithDescriptionDesigner
