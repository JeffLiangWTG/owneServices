using System.ComponentModel;
using System.Web.UI;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Label that has Natural key field and a lookup list
	/// </summary>
	[DefaultProperty("Text"), ToolboxData("<{0}:ZGuidFindBoxLabel runat=server></{0}:ZGuidFindBoxLabel>")]
	public class ZGuidFindBoxLabel : ZFindBoxLabelBase
	{
		protected override string GetCode(object list, IZType value)
		{
			var result = ZString.Empty;
			var codeDescriptionPairList = list as CodeDescriptionPairList;
			var iFindBoxListProviderList = list as IFindBoxListProvider;
			if (iFindBoxListProviderList != null)
			{
				result = iFindBoxListProviderList.GetBusinessObjectFromCode((ZString)value) != null ? (ZString)value : ZString.Empty;
			}
			else if (codeDescriptionPairList != null)
			{
				result = codeDescriptionPairList.ContainsCode((ZString)value) ? (ZString)value : ZString.Empty;
			}
			return result;
		}

		protected override ZCodeFindBoxFetchHintHandler GetFetchHintHandler(IBindToList control, object dataSource)
		{
			return new WebGuiFindBoxFetchHintHandler(control, dataSource);
		}

		class WebGuiFindBoxFetchHintHandler : ZGuidFindBoxFetchHintHandler
		{
			public WebGuiFindBoxFetchHintHandler(IBindToList control, object dataSource)
				: base(control, dataSource)
			{
			}

			protected override string BindingMember
			{
				get { return ((IBindTo)Control).BindTo; }
			}
		}
	}
}
