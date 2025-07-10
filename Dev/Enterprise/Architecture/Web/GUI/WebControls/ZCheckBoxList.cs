using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	[ToolboxData("<{0}:ZCheckBoxList runat=server></{0}:ZCheckBoxList>")]
	public class ZCheckBoxList : WebControl, ISelfBindingWebControl, IRepeatInfoUser
	{
		public RepeatDirection RepeatDirection
		{
			get
			{
				return (ViewState["RepeatDirection"] is RepeatDirection)
					? (RepeatDirection)ViewState["RepeatDirection"]
					: RepeatDirection.Vertical;
			}
			set { ViewState["RepeatDirection"] = value; }
		}

		public RepeatLayout RepeatLayout
		{
			get
			{
				return (ViewState["RepeatLayout"] is RepeatLayout)
					? (RepeatLayout)ViewState["RepeatLayout"]
					: RepeatLayout.Flow;
			}
			set { ViewState["RepeatLayout"] = value; }
		}

		public bool AutoPostBack
		{
			get { return fAutoPostBack; }
			set { fAutoPostBack = value; }
		}

		protected override void Render(HtmlTextWriter writer)
		{
			RepeatInfo.RepeatDirection = RepeatDirection;
			RepeatInfo.RepeatLayout = RepeatLayout;
			RepeatInfo.RenderRepeater(writer, this, ControlStyle, this);
		}

		readonly RepeatInfo RepeatInfo = new RepeatInfo();
		bool fAutoPostBack;

		#region IBindTo Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindTo
		{
			get { return fBindTo; }
			set { fBindTo = value; }
		}

		string fBindTo = "";

		#endregion

		#region ISelfBindingWebControl Members

		public bool IsBindable(object topLevelBizO)
		{
			bool result = false;

			if (!string.IsNullOrEmpty(BindTo))
			{
				result = ZPropertyAccessor.Get(topLevelBizO, BindTo) is IEnumerable<IBindableBooleanItem>;
			}
			else if (topLevelBizO is IEnumerable<IBindableBooleanItem>)
			{
				result = true;
			}

			return result;
		}

		public void Bind(object topLevelBizO)
		{
			ControlsToBeRendered.Clear();

			IEnumerable<IBindableBooleanItem> dataSource = (string.IsNullOrEmpty(BindTo))
				? (IEnumerable<IBindableBooleanItem>)topLevelBizO
				: (IEnumerable<IBindableBooleanItem>)ZPropertyAccessor.Get(topLevelBizO, BindTo);

			foreach (IBindableBooleanItem element in dataSource)
			{
				CheckBox childControl = GetNewControlToBeRendered();
				childControl.ID = element.Identifier.ToString();
				childControl.Text = element.Text;
				childControl.AutoPostBack = AutoPostBack;

				BindChildControl(childControl as ISelfBindingWebControl, element);
				Controls.Add(childControl);
				ControlsToBeRendered.Add(childControl);
			}
		}

		public void UnBind()
		{
			BindTo = "";
			ControlsToBeRendered.Clear();
		}

		protected virtual CheckBox GetNewControlToBeRendered()
		{
			return new ZCheckBox();
		}

		internal List<CheckBox> ControlsToBeRendered
		{
			get
			{
				if (fControlsToBeRendered == null)
				{
					fControlsToBeRendered = new List<CheckBox>();
				}
				return fControlsToBeRendered;
			}
		}

		void BindChildControl(ISelfBindingWebControl childControl, IBindableBooleanItem dataSource)
		{
			if (childControl != null)
			{
				childControl.BindTo = dataSource.BoolValueInfo.Name;
				childControl.Bind(dataSource);
			}
		}

		List<CheckBox> fControlsToBeRendered;

		#endregion

		#region IRepeatInfoUser Members

		Style IRepeatInfoUser.GetItemStyle(ListItemType itemType, int repeatIndex)
		{
			return ControlsToBeRendered[repeatIndex].ControlStyle;
		}

		bool IRepeatInfoUser.HasFooter
		{
			get { return false; }
		}

		bool IRepeatInfoUser.HasHeader
		{
			get { return false; }
		}

		bool IRepeatInfoUser.HasSeparators
		{
			get { return false; }
		}

		void IRepeatInfoUser.RenderItem(ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
		{
			ControlsToBeRendered[repeatIndex].RenderControl(writer);
		}

		int IRepeatInfoUser.RepeatedItemCount
		{
			get { return ControlsToBeRendered.Count; }
		}

		#endregion

		#region Internal Properties

		internal StateBag ViewStateInternal => ViewState;
		internal void RenderInternal(HtmlTextWriter writer) => Render(writer);

		#endregion
	}
}
