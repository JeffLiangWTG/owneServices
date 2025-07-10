using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	public class ZHyperLinksColumnItemTemplate : ZItemTemplate
	{
		public ZHyperLinksColumnItemTemplate(ZHyperLinksColumn column)
			: base(column)
		{
		}

		new ZHyperLinksColumn Column
		{
			get { return base.Column as ZHyperLinksColumn; }
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZRepeater repeater = new ZRepeater();
			repeater.HideStatusLabel = true;
			repeater.BindTo = Column.BindTo;

			repeater.DataBinding += new EventHandler(repeater_DataBinding);
			repeater.ItemDataBound += new RepeaterItemEventHandler(repeater_ItemDataBound);
			repeater.ItemCreated += new RepeaterItemEventHandler(repeater_ItemCreated);

			return repeater;
		}

		#region Implementation

		int createdItemIndex;
		readonly string linkID = "lnk";
		readonly string placeHolderID = "ph";

		void repeater_DataBinding(object sender, EventArgs e)
		{
			createdItemIndex = 0;
		}

		void repeater_ItemCreated(object sender, RepeaterItemEventArgs e)
		{
			bool itemsNumberIsLimited = Column.MaxItemsCount != null;

			if (itemsNumberIsLimited)
			{
				if (createdItemIndex < Column.MaxItemsCount)
				{
					AddHyperlink(e.Item);
				}
				else if (createdItemIndex == Column.MaxItemsCount)
				{
					e.Item.Controls.Add(new LiteralControl("..."));
				}
			}
			else
			{
				AddHyperlink(e.Item);
			}

			createdItemIndex++;
		}

		void repeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			ZHyperlink link = e.Item.FindControl(linkID) as ZHyperlink;
			if (link != null)
			{
				PKDescription bizO = e.Item.DataItem as PKDescription;
				if (bizO != null && bizO.DoNotCreateHyperLink)
				{
					link.Visible = false;

					PlaceHolder placeHolderForLabel = e.Item.FindControl(placeHolderID) as PlaceHolder;
					if (placeHolderForLabel != null)
					{
						placeHolderForLabel.Controls.Add(new LiteralControl(bizO.Description));
					}
				}
				else
				{
					link.Bind(e.Item.DataItem);
				}
			}
		}

		void AddHyperlink(RepeaterItem item)
		{
			ZHyperlink link = new ZHyperlink();
			link.ID = linkID;
			link.BindTo = Column.BindToField;
			link.DataTextFields = new string[] { Column.BindToField };
			link.DataNavigateUrlFields = Column.DataNavigateUrlFields;
			link.DataNavigateUrlFormatString = Column.DataNavigateUrlFormatString;
			link.Target = Column.Target;

			if (createdItemIndex > 0)
			{
				item.Controls.Add(new LiteralControl(", <br>"));
			}

			PlaceHolder placeHolderForLabel = new PlaceHolder();
			placeHolderForLabel.ID = placeHolderID;

			item.Controls.Add(link);
			item.Controls.Add(placeHolderForLabel);
		}

		#endregion
	}

	#endregion
}
