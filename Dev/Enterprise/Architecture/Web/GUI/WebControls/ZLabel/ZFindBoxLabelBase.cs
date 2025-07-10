using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Base class for labels with look up lists
	/// </summary>
	public abstract class ZFindBoxLabelBase : ZLookupLabelBase
	{
		protected override string GetDescription(object list, IZType value)
		{
			string result = string.Empty;
			if (list is IFindBoxListProvider)
			{
				result = ((IFindBoxListProvider)list).DescriptionFromCode(GetCode(list, value));
			}
			if (list is CodeDescriptionPairList)
			{
				result = ((CodeDescriptionPairList)list).GetDescriptionFromCode(GetCode(list, value));
			}
			return result;
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindToList))]
		[RefreshProperties(RefreshProperties.All)] // this is to force the .ReadOnly value to update in the designer
		public override string BindToList
		{
			get { return fBindToList; }
			set { fBindToList = value; }
		}
		string fBindToList = "";
	}
}
