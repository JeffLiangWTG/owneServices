using System;
using System.ComponentModel;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Business;
using TreeNode = Enterprise.ZArchitecture.Web.Business.TreeNode;
using TreeNodeCollection = Enterprise.ZArchitecture.Web.Business.TreeNodeCollection;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	/// <summary>
	/// Summary description for ZTreeView.
	/// </summary>
	[ToolboxData("<{0}:ZTreeView runat=server />"),
	Designer(typeof(Design.ZTreeViewDesigner))]
	public class ZTreeView : WebControl, ISelfBindingWebControl, IContainResources, INamingContainer, IPostBackEventHandler
	{
		protected const string ClientScriptBlockID = "ZTreeViewClientScriptBlock";
		protected const string ExpandCollapseFunction = "Toggle(this)";

		public ZTreeView()
			: base()
		{
			Width = 300;
			Height = 300;
		}

		protected override void OnLoad(EventArgs e)
		{
			if (!Page.ClientScript.IsClientScriptBlockRegistered(GetType(), ClientScriptBlockID))
			{
				Page.ClientScript.RegisterClientScriptBlock(GetType(), ClientScriptBlockID, ClientScriptBlock);
			}
			if (Page.IsPostBack)
			{
				UpdateSelectedPath();
			}
			base.OnLoad(e);
		}

		#region Properties

		#region DataSource
		protected object fDataSource;

		protected object DataSource
		{
			get { return fDataSource; }
			set { fDataSource = value; }
		}

		protected TreeNodeCollection fNodes;

		public TreeNodeCollection Nodes
		{
			get
			{
				if (fNodes == null)
				{
					fNodes = new TreeNodeCollection(null);
				}
				return fNodes;
			}
			set { fNodes = value; }
		}

		#endregion DataSource

		#region Selection

		Color fSelectedColor;

		[Category("Appearance")]
		[DefaultValue("Gray")]
		public Color SelectedColor
		{
			get { return fSelectedColor; }
			set { fSelectedColor = value; }
		}

		[Browsable(false)]
		public IFamilyMember SelectedItem
		{
			get
			{
				if ((SelectedPath != null) && (SelectedPath.Length > 0))
				{
					string delimStr = ".";
					char[] delimiter = delimStr.ToCharArray();

					string[] pathComponents = SelectedPath.Split(delimiter);

					fSelectedItem = Nodes.GetItemAtPath(pathComponents);
				}
				return fSelectedItem;
			}
			set { fSelectedItem = value; }
		}
		IFamilyMember fSelectedItem;

		string ExpandedPath
		{
			get
			{
				return fExpandedPath ?? "";
			}
			set
			{
				fExpandedPath = value;
			}
		}
		string fExpandedPath;

		#endregion Selection

		protected ZPage BasePage
		{
			get { return Page as ZPage; }
		}

		public bool IsDesigning;

		#endregion Properties

		#region Implementation

		void UpdateSelectedPath()
		{
			string path = Page.Request.Form.Get("SelectedItem");
			if (path != null)
			{
				SelectedPath = path;
			}
		}

		protected string SelectedPath
		{
			get { return fSelectedPath; }
			set { fSelectedPath = value; }
		}
		string fSelectedPath;

		protected string RuntimeDirectory
		{
			get
			{
				if (fRuntimeDirectory == null)
				{
					fRuntimeDirectory = VirtualPathUtility.AppendTrailingSlash(VirtualPathUtility.GetDirectory(PlusImageFile.FileName));
				}
				return fRuntimeDirectory;
			}
		}
		string fRuntimeDirectory;

		#region Rendering

		protected override void Render(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "ScrollDiv");
			RenderTreeViewStyleAttributes(writer);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			RenderAllNodes(writer);
			if (!IsDesigning)
			{
				Page.ClientScript.RegisterHiddenField("SelectedItem", SelectedPath);
				Page.ClientScript.RegisterHiddenField("ExpandedPath", ExpandedPath);
				Page.ClientScript.RegisterHiddenField("RuntimeDirectory", RuntimeDirectory);
			}
			writer.RenderEndTag();
		}

		protected void RenderTreeViewStyleAttributes(HtmlTextWriter writer)
		{
			writer.AddStyleAttribute("overflow", "auto");
			RenderTreeViewDimensionStyleAttributes(writer);
			if (!BackColor.IsEmpty)
			{
				writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, BackColor.Name);
			}
			if (!BorderColor.IsEmpty)
			{
				writer.AddStyleAttribute(HtmlTextWriterStyle.BorderColor, BorderColor.Name);
				writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, BorderStyle.ToString());
			}
			if (!BorderWidth.IsEmpty)
			{
				writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, BorderWidth.Value.ToString());
			}
		}

		protected virtual void RenderTreeViewDimensionStyleAttributes(HtmlTextWriter writer)
		{
			if (Width.Value > 0)
			{
				writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Width.Value.ToString());
			}
			if (Height.Value > 0)
			{
				writer.AddStyleAttribute(HtmlTextWriterStyle.Height, Height.Value.ToString());
			}
		}

		protected void RenderAllNodes(HtmlTextWriter writer)
		{
			int nodeId = 0;

			foreach (TreeNode node in Nodes)
			{
				RenderNode(writer, node, String.Format("{0}", nodeId++));
			}
		}

		protected void RenderNode(HtmlTextWriter writer, TreeNode node, string id)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
			writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
			writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
			writer.RenderBeginTag(HtmlTextWriterTag.Table);
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);
			RenderNodeDetails(writer, node, id);
			writer.RenderEndTag();

			if (node.IsExpanded)
			{
				node.LoadChildNodes();

				writer.RenderBeginTag(HtmlTextWriterTag.Tr);
				if (node.Parent != null)
				{
					if (!node.IsLastChild)
					{
						writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundImage, VerticalImageFile.FileName);
						writer.AddStyleAttribute("background-repeat", "repeat-y");
					}
					writer.RenderBeginTag(HtmlTextWriterTag.Td);
					writer.RenderEndTag();
				}
				writer.RenderBeginTag(HtmlTextWriterTag.Td);

				int childId = 0;

				foreach (TreeNode childNode in node.Nodes)
				{
					RenderNode(writer, childNode, String.Format("{0}.{1}", id, childId++));
				}
				writer.RenderEndTag();
				writer.RenderEndTag();
			}
			writer.RenderEndTag();
			writer.WriteLine();
		}

		protected void RenderNodeDetails(HtmlTextWriter writer, TreeNode node, string id)
		{
			RenderTreeLinkCell(writer, node);
			RenderNodeCell(writer, node, id);
		}

		protected void RenderTreeLinkCell(HtmlTextWriter writer, TreeNode node)
		{
			if (node.Parent != null)
			{
				if (!node.IsLastChild)
				{
					writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundImage, VerticalImageFile.FileName);
					writer.AddStyleAttribute("background-repeat", "repeat-y");
				}
				writer.AddAttribute(HtmlTextWriterAttribute.Valign, "TOP");
				writer.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
				writer.RenderBeginTag(HtmlTextWriterTag.Td);
				string linksrc = node.IsLastChild ? HRLImageFile.FileName : HRImageFile.FileName;
				writer.AddAttribute(HtmlTextWriterAttribute.Src, linksrc);
				writer.RenderBeginTag(HtmlTextWriterTag.Img);
				writer.RenderEndTag();
				writer.RenderEndTag();
			}
		}

		protected void RenderNodeCell(HtmlTextWriter writer, TreeNode node, string id)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Nowrap, "true");
			writer.AddAttribute(HtmlTextWriterAttribute.Id, id);
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			if (node.HasChildren)
			{
				if (node.IsExpanded)
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Onclick, ExpandCollapseFunction);
				}
				else
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Onclick, LoadExpandFunction(id));
				}
			}
			writer.AddStyleAttribute("cursor", "hand");
			writer.RenderBeginTag(HtmlTextWriterTag.A);
			RenderImageTag(writer, node);
			writer.RenderEndTag();
			RenderNodeCellDetails(writer, node, id);
			writer.RenderEndTag();
		}

		protected void RenderImageTag(HtmlTextWriter writer, TreeNode node)
		{
			if (node.HasChildren)
			{
				string imgsrc = node.IsExpanded ? MinusImageFile.FileName : PlusImageFile.FileName;
				writer.AddAttribute(HtmlTextWriterAttribute.Src, imgsrc);
				writer.RenderBeginTag(HtmlTextWriterTag.Img);
				writer.RenderEndTag();
			}
		}

		protected virtual void RenderNodeCellDetails(HtmlTextWriter writer, TreeNode node, string id)
		{
			writer.Write(node.Text.Replace("'", "\\'").Replace("\r", "").Replace("\n", ""));
		}

		#endregion Rendering

		#region ClientScriptBlock

		public string LoadExpandFunction(string arg)
		{
			return "javascript:" + Page.ClientScript.GetPostBackEventReference(this, arg);
		}

		public string ClientScriptBlock
		{
			get
			{
				return string.Format("<SCRIPT src='{0}'></SCRIPT>", ScriptFile.FileName);
			}
		}

		#endregion

		#endregion Implementation

		#region ISelfBindingWebControl Members

		public bool IsBindable(object dataSource)
		{
			if (!string.IsNullOrEmpty(BindTo) && dataSource != null)
			{
				PropertyDescriptor bindToProperty = ZTypeDescriptor.GetProperties(dataSource, BindTo, false);
				if (bindToProperty != null)
				{
					return true;
				}
			}
			else
			{
				return dataSource is TreeNodeCollection;
			}
			return false;
		}

		public void Bind(object dataSource)
		{
			if (!string.IsNullOrEmpty(BindTo))
			{
				Nodes = ZPropertyAccessor.Get(dataSource, BindTo) as TreeNodeCollection;
			}
			else if (dataSource is TreeNodeCollection)
			{
				Nodes = dataSource as TreeNodeCollection;
			}
		}

		public void UnBind()
		{
			BindTo = null;
			DataSource = null;
		}

		#endregion

		#region IBindTo Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindTo
		{
			get { return fBindTo; }
			set { fBindTo = value; }
		}
		string fBindTo = "";

		#endregion

		#region IPostBackEventHandler Members

		public void RaisePostBackEvent(string eventArgument)
		{
			string delimStr = ".";
			char[] delimiter = delimStr.ToCharArray();

			ExpandedPath = eventArgument;
			string[] pathComponents = eventArgument.Split(delimiter);

			Nodes.ExpandPathToTarget(pathComponents);
		}

		#endregion

		#region IContainResources Members

		public ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = new ZWebResourceCollection();
				result.Add(ScriptFile);
				result.Add(HRImageFile);
				result.Add(HRLImageFile);
				result.Add(MinusImageFile);
				result.Add(PlusImageFile);
				result.Add(VerticalImageFile);
				return result;
			}
		}

		#region ScriptFile

		protected ZWebResource ScriptFile
		{
			get
			{
				if (fScriptFile == null)
				{
					fScriptFile = new ZWebResource(typeof(ZTreeView), "TreeViewScriptBlock.js", BasePage);
				}

				return fScriptFile;
			}
		}
		ZWebResource fScriptFile;

		#endregion

		#region HRImageFile

		protected ZWebResource HRImageFile
		{
			get
			{
				if (fHRImageFile == null)
				{
					fHRImageFile = new ZWebResource(typeof(ZTreeView), "hr.gif", BasePage);
				}

				return fHRImageFile;
			}
		}
		ZWebResource fHRImageFile;

		#endregion

		#region HRLImageFile

		protected ZWebResource HRLImageFile
		{
			get
			{
				if (fHRLImageFile == null)
				{
					fHRLImageFile = new ZWebResource(typeof(ZTreeView), "hr_l.gif", BasePage);
				}

				return fHRLImageFile;
			}
		}
		ZWebResource fHRLImageFile;

		#endregion

		#region MinusImageFile

		protected ZWebResource MinusImageFile
		{
			get
			{
				if (fMinusImageFile == null)
				{
					fMinusImageFile = new ZWebResource(typeof(ZTreeView), "minus.gif", BasePage);
				}

				return fMinusImageFile;
			}
		}
		ZWebResource fMinusImageFile;

		#endregion

		#region PlusImageFile

		protected ZWebResource PlusImageFile
		{
			get
			{
				if (fPlusImageFile == null)
				{
					fPlusImageFile = new ZWebResource(typeof(ZTreeView), "plus.gif", BasePage);
				}

				return fPlusImageFile;
			}
		}
		ZWebResource fPlusImageFile;

		#endregion

		#region VerticalImageFile

		protected ZWebResource VerticalImageFile
		{
			get
			{
				if (fVerticalImageFile == null)
				{
					fVerticalImageFile = new ZWebResource(typeof(ZTreeView), "vertical.gif", BasePage);
				}

				return fVerticalImageFile;
			}
		}
		ZWebResource fVerticalImageFile;

		#endregion

		#endregion
	}

	#endregion
}

#region ZTreeViewDesigner

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Design
{
	using System.IO;
	using System.Web.UI.Design;

	public class ZTreeViewDesigner : ControlDesigner
	{
		public override string GetDesignTimeHtml()
		{
			using (StringWriter stringWriter = new StringWriter())
			{
				HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter);
				ZTreeView control = this.Component as ZTreeView;
				control.IsDesigning = true;
				control.RenderControl(htmlWriter);
				return stringWriter.ToString();
			}
		}
	}
}

#endregion ZTreeViewDesigner
