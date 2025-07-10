using System;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZSelectionCheckBox : CheckBox, IContainResources
	{
		public ZSelectionCheckBox() : this(false)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "javascript code should not be translated")]
		public ZSelectionCheckBox(bool selectAll) : base()
		{
			if (selectAll)
			{
				Attributes["onclick"] = "javascript:SelectAllCheckboxes(this);";
				ToolTip = Res.GetString("ec926575-685d-4309-bb3c-2ebf7f111d73", "Select/Deselect All");
			}
			else
			{
				ToolTip = Res.GetString("f7902069-fbbd-4715-8df7-07b49759b7d4", "Select/Deselect");
			}
		}

		public new ZPage Page
		{
			get { return base.Page as ZPage; }
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			RenderScriptBlocks();
		}

		#region IContainResources Members

		public ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection fResources = new ZWebResourceCollection();
				fResources.Add(SelectionCheckBoxScriptResource);
				return fResources;
			}
		}

		#region SelectionCheckBoxScriptResource

		public ZWebResource SelectionCheckBoxScriptResource
		{
			get
			{
				if (fSelectionCheckBoxScriptResource == null)
				{
					fSelectionCheckBoxScriptResource = new ZWebResource(typeof(ZSelectionCheckBox), "SelectionCheckBox.js", Page);
				}
				return fSelectionCheckBoxScriptResource;
			}
		}

		ZWebResource fSelectionCheckBoxScriptResource;

		#endregion

		protected virtual void RenderScriptBlocks()
		{
			if (Page != null && !Page.ZClientScript.IsClientScriptIncludeRegistered("ZSelectionCheckBox_Scripts"))
			{
				Page.ZClientScript.RegisterClientScriptInclude("ZSelectionCheckBox_Scripts", SelectionCheckBoxScriptResource.FileName);
			}
		}

		#endregion
	}
}
