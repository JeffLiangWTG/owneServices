using System.Drawing;
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	#region SuppressResourceStringsCheckRegion

	public class ZFilterStripDropDownList : ZDropDownList
	{
		#region IsDescriptionsList

		public bool IsDescriptionsList
		{
			get { return isDescriptionsList; }
			set { isDescriptionsList = value; }
		}

		bool isDescriptionsList;

		#endregion

		#region Rendering

		protected internal void InternalRender(HtmlTextWriter w) => this.Render(w);

		protected override void RenderContents(HtmlTextWriter writer)
		{
			if (IsDescriptionsList)
			{
				bool isFirstNonEmptyItem = true;
				if (ItemList != null)
				{
					foreach (ListItem item in Items)
					{
						ICodeDescription codeDescription = ItemList[item.Value];
						if (codeDescription != null)
						{
							if (!(string.IsNullOrEmpty(codeDescription.Description) && string.IsNullOrEmpty(codeDescription.Code)))
							{
								if (isFirstNonEmptyItem)
								{
									if (ShowEmptyItem)
									{
										RenderListItem(new ListItem(ZString.Empty), writer);
									}
									isFirstNonEmptyItem = false;
								}
								if (Helper.IsCategoryItem(codeDescription))
								{
									CheckToCloseCategory(writer);
									RenderOptionGroupBeginTag((string.IsNullOrEmpty(codeDescription.Description) ? codeDescription.Code : codeDescription.Description), writer);
									categoryOpened = true;
								}
								else if (string.IsNullOrEmpty(codeDescription.Description))
								{
									CheckToCloseCategory(writer);
									RenderListItem(item, writer);
								}
								else if (Helper.IsExclusiveItem(codeDescription))
								{
									RenderListItem(item, writer, KnownColor.Blue);
								}
								else
								{
									RenderListItem(item, writer);
								}
							}
						}
					}
					CheckToCloseCategory(writer);
				}
			}
			else
			{
				base.RenderContents(writer);
			}
		}

		public override void RenderControl(HtmlTextWriter writer)
		{
			if ((Info != null && Info.ReadOnly))
			{
				DropDownList readOnlyControl = new DropDownList();
				readOnlyControl.CssClass = CssClass;
				readOnlyControl.Width = Width;
				readOnlyControl.Items.Add(Text);
				readOnlyControl.Text = Text;
				readOnlyControl.Visible = Visible;
				readOnlyControl.Enabled = false;
				readOnlyControl.RenderControl(writer);
			}
			else
			{
				base.RenderControl(writer);
			}
		}

		protected override string GetDataTextField(object data)
		{
			return data is ModuleFilter ? "LocalizedDescription" : base.GetDataTextField(data);
		}

		#region Category

		void RenderOptionGroupBeginTag(string name, HtmlTextWriter writer)
		{
			writer.WriteBeginTag("optgroup");
			writer.WriteAttribute("label", name);
			writer.Write(HtmlTextWriter.TagRightChar);
			writer.WriteLine();
		}

		void RenderOptionGroupEndTag(HtmlTextWriter writer)
		{
			writer.WriteEndTag("optgroup");
			writer.WriteLine();
		}

		void CheckToCloseCategory(HtmlTextWriter writer)
		{
			if (categoryOpened)
			{
				RenderOptionGroupEndTag(writer);
				categoryOpened = false;
			}
		}

		bool categoryOpened;

		#endregion

		#region ListItem

		void RenderListItem(ListItem item, HtmlTextWriter writer)
		{
			RenderListItem(item, writer, KnownColor.Black);
		}

		void RenderListItem(ListItem item, HtmlTextWriter writer, KnownColor color)
		{
			writer.WriteBeginTag("option");
			writer.WriteAttribute("value", item.Value, true);
			if (item.Selected)
			{
				writer.WriteAttribute("selected", "selected", false);
			}
			if (color != KnownColor.Black)
			{
				writer.WriteAttribute("style", "color:" + color);
			}
			writer.Write(HtmlTextWriter.TagRightChar);
			WebUtility.HtmlEncode(item.Text, writer);
			writer.WriteEndTag("option");
			writer.WriteLine();
		}

		#endregion
		#endregion
		#region Business entities

		ZFilterStripDropHelper Helper
		{
			get { return fHelper ?? (fHelper = new ZFilterStripDropHelper()); }
		}

		ZFilterStripDropHelper fHelper;

		CodeDescriptionPairList ItemList
		{
			get
			{
				CodeDescriptionPairList list = null;
				if (BusinessEntity != null && !new ZString(BindToList).IsEmpty)
				{
					list = ZPropertyAccessor.Get(BusinessEntity, BindToList) as CodeDescriptionPairList;
				}
				return list;
			}
		}

		#endregion

		protected override bool GetDisplayNotifications()
		{
			return false;
		}
	}

	#endregion
}
