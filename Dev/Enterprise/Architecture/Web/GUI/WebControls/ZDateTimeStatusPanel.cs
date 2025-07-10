using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	[DefaultProperty("Text"),
	ToolboxData("<{0}:ZDateTimeStatusPanel runat=server></{0}:ZDateTimeStatusPanel>")]
	public class ZDateTimeStatusPanel : Panel, ISelfBindingWebControl
	{
		#region ISelfBindingWebControl Members

		public bool IsBindable(object dataSource)
		{
			return !string.IsNullOrEmpty(BindTo) && dataSource != null;
		}

		public void Bind(object dataSource)
		{
			ContentsLabel.Bind(dataSource);
			if (string.IsNullOrEmpty(ContentsLabel.Text))
			{
				ContentsLabel.Text = "&nbsp;";//otherwise hint doesn't come up in IE
			}

			ZDateTime dateValue = (ZDateTime)ZPropertyAccessor.Get(dataSource, BindTo);
			ZString statusValue = !string.IsNullOrEmpty(BindToStatus) ? (ZString)ZPropertyAccessor.Get(dataSource, BindToStatus) : ZString.Empty;

			string timeLineClass = TimelineHelper.NoTimeLineEstimateClass;

			if (dateValue != SuppressUtil.SuppressedDateTime)
			{
				switch (statusValue.ToString())
				{
					case Constants.DateTimeStatus.Overdue: timeLineClass = TimelineHelper.OverdueClass; break;
					case Constants.DateTimeStatus.Late: timeLineClass = TimelineHelper.CompletedLateClass; break;
					case Constants.DateTimeStatus.OnTime: timeLineClass = TimelineHelper.CompletedClass; break;
				}

				ToolTip = !statusValue.IsEmpty ? statusValue.ToString() : ContentsLabel.ToolTip;
			}
			ContentsLabel.ToolTip = "";

			CssClass = ZCssHelper.Join(CssClass, timeLineClass);
		}

		public void UnBind()
		{
			// TODO:  Add ZTimelinePanel.UnBind implementation
		}

		#endregion

		#region IBindTo Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindTo
		{
			get { return bindTo; }
			set { bindTo = value; }
		}
		string bindTo = "";

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindToStatus
		{
			get { return bindToStatus; }
			set { bindToStatus = value; }
		}
		string bindToStatus = "";

		#endregion

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			Controls.Add(ContentsLabel);
		}

		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(ZDateTimePickerFormat.Short)]
		public ZDateTimePickerFormat DateTimeFormat
		{
			get { return dateTimeFormat; }
			set { dateTimeFormat = value; }
		}
		ZDateTimePickerFormat dateTimeFormat;

		internal ZEstimatedActualDateTimeLabel ContentsLabel
		{
			get
			{
				if (contentsLabel == null)
				{
					contentsLabel = new ZEstimatedActualDateTimeLabel();
					contentsLabel.BindTo = BindTo;
					contentsLabel.DateTimeFormat = DateTimeFormat;
				}
				return contentsLabel;
			}
		}
		ZEstimatedActualDateTimeLabel contentsLabel;
	}
}
