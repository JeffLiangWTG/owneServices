using System.ComponentModel;
using System.Web.UI;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Label to display Code or Description based on Guid
	/// </summary>
	[DefaultProperty("Text"), ToolboxData("<{0}:ZFindBoxLabel runat=server></{0}:ZFindBoxLabel>")]
	public class ZFindBoxLabel : ZFindBoxLabelBase
	{
		protected override string GetCode(object list, IZType value)
		{
			ZString result = ZString.Empty;
			if (list is IFindBoxListProvider)
			{
				result = ((IFindBoxListProvider)list).CodeFromPrimaryKey((ZGuid)value);
			}
			else if (list is CodeDescriptionPairList)
			{
				result = ((CodeDescriptionPairList)list).CodeFromPrimaryKey((ZGuid)value);
			}
			return result;
		}

		protected override ZCodeFindBoxFetchHintHandler GetFetchHintHandler(IBindToList control, object dataSource)
		{
			return new WebGuidFindBoxFetchHintHandler(control, dataSource);
		}

		class WebGuidFindBoxFetchHintHandler : ZGuidFindBoxFetchHintHandler
		{
			public WebGuidFindBoxFetchHintHandler(IBindToList control, object dataSource)
				: base(control, dataSource)
			{
			}

			protected override string BindingMember
			{
				get { return ((IBindTo)Control).BindTo; }
			}
		}
	}

	static class CodeDescriptionPairListExtender
	{
		public static ZString CodeFromPrimaryKey(this CodeDescriptionPairList list, ZGuid pK)
		{
			foreach (ICodeDescription pair in list)
			{
				if (pair.PK.Equals(pK))
				{
					return pair.Code;
				}
			}
			return ZString.Empty;
		}
	}
}
