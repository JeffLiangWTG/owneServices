using System;
using System.Collections;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Summary description for ZMultiTextListBox.
	/// </summary>
	[DefaultProperty("Text"),
	ToolboxData("<{0}:ZMultiTextListBox runat=server></{0}:ZMultiTextListBox>")]
	public class ZMultiTextListBox : ListBox
	{
		public ZMultiTextListBox() : base()
		{
			CachedSelectedIndex = -1;
		}

		public ZBool ShowDescription
		{
			get { return fShowDescription; }
			set { fShowDescription = value; }
		}
		ZBool fShowDescription;

		protected override void OnDataBinding(EventArgs e)
		{
			IList list = DataSource as IList;
			if (list != null)
			{
				Items.Clear();
				Items.Capacity = list.Count;

				foreach (object obj in list)
				{
					ListItem item = new ListItem();

					ICodeDescription codeDescBizO = obj as ICodeDescription;
					BusinessObject bizO = obj as BusinessObject;

					if (codeDescBizO != null || bizO != null)
					{
						string code = codeDescBizO != null ?  codeDescBizO.Code : CodePropertyAttribute.CodeFromBusinessObject(bizO).ToString();
						string desc = codeDescBizO != null ? codeDescBizO.Description : DescriptionPropertyAttribute.DescriptionFromBusinessObject(bizO).ToString();
						item.Value = code;
						item.Text = ShowDescription ? String.Format("{0} - {1}", code, desc) : code;
						Items.Add(item);
					}
				}
			}
			if (CachedSelectedValue != null)
			{
				int index = FindItemByValueInternal(Items, CachedSelectedValue);
				if (index < 0)
				{
					throw new InvalidOperationException("Invalid value: index < 0");
				}
				else
				{
					if (CachedSelectedIndex != -1 && index != CachedSelectedIndex)
					{
						throw new ArgumentException("SelectedValue and SelectedIndex are mutually exclusive. Unable to set both.");
					}
					else
					{
						this.SelectedIndex = index;
						this.CachedSelectedValue = null;
						this.CachedSelectedIndex = -1;
					}
				}
			}
			else if (CachedSelectedIndex != -1)
			{
				this.SelectedIndex = CachedSelectedIndex;
				this.CachedSelectedIndex = -1;
			}
		}

		int FindItemByValueInternal(ListItemCollection itemCollection, string value)
		{
			int foundIndex = -1;
			for (int i = 0 ; i < itemCollection.Count && foundIndex < 0 ; i++)
			{
				if (itemCollection[i].Value.Equals(value))
				{
					foundIndex = i;
				}
			}
			return foundIndex;
		}

		public override string SelectedValue
		{
			get { return base.SelectedValue; }
			set
			{
				if (Items.Count == 0)
				{
					this.CachedSelectedValue = value;
				}
				base.SelectedValue = value;
			}
		}
		string CachedSelectedValue;

		public override int SelectedIndex
		{
			get
			{
				return base.SelectedIndex;
			}
			set
			{
				if (Items.Count == 0)
				{
					this.CachedSelectedIndex = value;
				}
				base.SelectedIndex = value;
			}
		}
		int CachedSelectedIndex;
	}
}
