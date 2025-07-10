using System.ComponentModel;
using System.Web.UI;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Label to display Date/Time values as estimated or actual if available
	/// </summary>
	[DefaultProperty("Text"), ToolboxData("<{0}:ZEstimatedActualDateTimeLabel runat=server></{0}:ZEstimatedActualDateTimeLabel>")]
	public class ZEstimatedActualDateTimeLabel : ZDateTimeLabel
	{
		protected override IZType GetPropertyValueObject(object dataSource)
		{
			IZType result = (IZType)ZPropertyAccessor.Get(dataSource, BindTo);
			if (result.IsEmpty && !string.IsNullOrEmpty(BindToEstimated))
			{
				result = (IZType)ZPropertyAccessor.Get(dataSource, BindToEstimated);
				CssClass = ZCssHelper.Join(CssClass, TimelineHelper.TimeLineEstimateClass);
			}
			return result;
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindToEstimated))]
		public virtual string BindToEstimated
		{
			get { return fBindToEstimated; }
			set { fBindToEstimated = value; }
		}
		string fBindToEstimated = "";
	}
}
