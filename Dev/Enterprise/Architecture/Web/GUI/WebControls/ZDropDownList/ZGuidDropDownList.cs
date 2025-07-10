using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// DropDownList to be bound to Guid fields
	/// </summary>
	public class ZGuidDropDownList : ZDropDownList
	{
		protected override object GetSelectedValueForSavingIntoBizObject()
		{
			object result;
			try
			{
				result = new ZGuid(CachedSelectedValue);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				result = null;
			}
			return result;
		}

		protected override bool ValuesEqual(string newValue, string oldValue)
		{
			return base.ValuesEqual(newValue, oldValue)
				|| string.IsNullOrEmpty(newValue) && Guid.Empty.ToString().Equals(oldValue)
				|| string.IsNullOrEmpty(oldValue) && Guid.Empty.ToString().Equals(newValue);
		}
	}
}
