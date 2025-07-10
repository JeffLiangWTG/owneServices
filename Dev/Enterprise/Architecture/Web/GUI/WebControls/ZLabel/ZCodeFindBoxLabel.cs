using System.ComponentModel;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Label that has Natural key field and a lookup list
	/// </summary>
	[DefaultProperty("Text"), ToolboxData("<{0}:ZCodeFindBoxLabel runat=server></{0}:ZCodeFindBoxLabel>")]
	public class ZCodeFindBoxLabel : ZFindBoxLabelBase
	{
		protected override string GetCode(object list, IZType value)
		{
			ZString result = ZString.Empty;
			if (list is IFindBoxListProvider)
			{
				result = ((IFindBoxListProvider)list).GetBusinessObjectFromCode((ZString)value) != null ? (ZString)value : ZString.Empty;
			}
			else if (list is CodeDescriptionPairList)
			{
				result = ((CodeDescriptionPairList)list).ContainsCode((ZString)value) ? (ZString)value : ZString.Empty;
			}
			return result;
		}
	}
}
