using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// DropDownList to be bound to numeric fields
	/// </summary>
	public class ZNumericDropDownList : ZDropDownList
	{
		protected override object GetSelectedValueForSavingIntoBizObject()
		{
			Type propertyType = ZPropertyAccessor.GetPropertyType(BusinessEntity, BindTo);

			INumericZType value;
			if (propertyType == typeof(ZDecimal))
			{
				value = ZDecimal.ParseSafe(CachedSelectedValue, 0);
			}
			else if (propertyType == typeof(ZInt))
			{
				value = ZInt.ParseSafe(CachedSelectedValue, 0);
			}
			else if (propertyType == typeof(ZShort))
			{
				value = ZShort.ParseSafe(CachedSelectedValue, 0);
			}
			else if (propertyType == typeof(ZByte))
			{
				value = ZByte.ParseSafe(CachedSelectedValue, 0);
			}
			else
			{
				throw new NotSupportedException("The type you are binding to is not supported by the ZNumericDropDownList: <" + propertyType.FullName + ">.");
			}
			return value;
		}
	}
}
