using System;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZCheckBoxForSelect : CheckBox, IContainResources
	{
		public ZCheckBoxForSelect()
			: this(false)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "javascript code should not be translated")]
		public ZCheckBoxForSelect(bool selectAll)
			: base()
		{
			if (selectAll)
			{
				Attributes["onclick"] = "javascript:SelectAllCheckBoxesForThisColumn(this);";
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
				var resources = new ZWebResourceCollection();
				resources.Add(CheckBoxForSelectScriptResource);
				return resources;
			}
		}

		#region CheckBoxForSelectScriptResource

		public ZWebResource CheckBoxForSelectScriptResource
		{
			get
			{
				if (checkBoxForSelectScriptResource == null)
				{
					checkBoxForSelectScriptResource = new ZWebResource(typeof(ZCheckBoxForSelect), "ZCheckBoxForSelect.js", Page);
				}
				return checkBoxForSelectScriptResource;
			}
		}
		ZWebResource checkBoxForSelectScriptResource;

		#endregion

		protected virtual void RenderScriptBlocks()
		{
			if (Page != null && !Page.ZClientScript.IsClientScriptIncludeRegistered("ZCheckBoxForSelect_Scripts"))
			{
				Page.ZClientScript.RegisterClientScriptInclude("ZCheckBoxForSelect_Scripts", CheckBoxForSelectScriptResource.FileName);
			}
		}

		#endregion
	}
}
