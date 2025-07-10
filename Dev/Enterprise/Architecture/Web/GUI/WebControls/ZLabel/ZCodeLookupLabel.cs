using System;
using System.ComponentModel;
using System.Reflection;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Label that has Natural key field and a lookup list
	/// </summary>
	[DefaultProperty("Text"), ToolboxData("<{0}:ZCodeLookupLabel runat=server></{0}:ZCodeLookupLabel>")]
	public class ZCodeLookupLabel : ZLookupLabelBase
	{
		protected override string GetCode(object list, IZType value)
		{
			if (list is ICodeDescriptionPairList)
			{
				return ((ICodeDescriptionPairList)list).ContainsCode(value.ToString()) ? value.ToString() : string.Empty;
			}
			MethodInfo containsCodeMethod = list.GetType().GetMethod("ContainsCode", BindingFlags.Public | BindingFlags.Instance);
			if (containsCodeMethod != null)
			{
				try
				{
					object containsCodeResult = containsCodeMethod.Invoke(list, new object[] { value.ToString() });
					if (containsCodeResult is bool)
					{
						return (bool)containsCodeResult ? value.ToString() : string.Empty;
					}
				}
				catch (Exception e) when (!e.IsCriticalException()) { }
			}
			return string.Empty;
		}

		protected override string GetDescription(object list, IZType value)
		{
			if (list is ICodeDescriptionPairList)
			{
				return ((ICodeDescriptionPairList)list).GetDescriptionFromCode(GetCode(list, value));
			}
			MethodInfo descriptionFromCodeMethod = list.GetType().GetMethod("GetDescriptionFromCode", BindingFlags.Public | BindingFlags.Instance);
			if (descriptionFromCodeMethod != null)
			{
				try
				{
					return descriptionFromCodeMethod.Invoke(list, new object[] { GetCode(list, value) }).ToString();
				}
				catch (Exception e) when (!e.IsCriticalException()) { }
			}
			return string.Empty;
		}

		#region IBindToList Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindToList))]
		public override string BindToList
		{
			get { return fBindToList; }
			set { fBindToList = value; }
		}
		string fBindToList = "";

		#endregion
	}
}
