using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	[DefaultProperty("Text"),
	ToolboxData("<{0}:ZTimelinePanel runat=server></{0}:ZTimelinePanel>")]
	public class ZTimelinePanel : Panel, ISelfBindingWebControl
	{
		#region ISelfBindingWebControl Members

		public bool IsBindable(object dataSource)
		{
			return !string.IsNullOrEmpty(BindTo) && dataSource != null;
		}

		public void Bind(object dataSource)
		{
			var actualValue = ZTimelineColumn.AsOffset(ZPropertyAccessor.Get(dataSource, BindTo));
			var estimatedValue = ZTimelineColumn.AsOffset(ZPropertyAccessor.Get(dataSource, BindToEstimated));

			string status = TimelineHelper.GetProcessTaskWebStatus(actualValue, estimatedValue);

			if ((WebDataRegistry.Instance.MilestoneVisibility.Value != MilestoneVisibilityList.Codes.CompletedMilestonesOnly &&
			WebDataRegistry.Instance.MilestoneVisibility.Value != MilestoneVisibilityList.Codes.LastCompletedMilestoneOnly) ||
			status == ProcessTask.Completed || status == ProcessTask.CompletedLate)
			{
				ContentsLabel.Bind(dataSource);

				if (string.IsNullOrEmpty(ContentsLabel.Text))
				{
					ContentsLabel.Text = "&nbsp;"; //otherwise hint doesn't come up in IE
				}
				ContentsLabel.ToolTip = string.Empty;

				ToolTip = TimelineHelper.GetComment(actualValue, estimatedValue, DateTimeFormat);

				string timeLineClass = TimelineHelper.GetCSS(actualValue, estimatedValue);

				CssClass = ZCssHelper.Join(CssClass, timeLineClass);
			}
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
		public string BindToEstimated
		{
			get { return bindToEstimated; }
			set { bindToEstimated = value; }
		}
		string bindToEstimated = "";

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
				return fContentsLabel ?? (fContentsLabel = new ZEstimatedActualDateTimeLabel
				{
					BindTo = BindTo,
					BindToEstimated = BindToEstimated,
					DateTimeFormat = DateTimeFormat
				});
			}
		}
		ZEstimatedActualDateTimeLabel fContentsLabel;
	}
}
