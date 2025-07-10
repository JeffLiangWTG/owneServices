using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class CustomPropertyComparer : IComparer<ICustomProperty>
	{
		readonly DynamicBusinessObjectPropertyComparer inner = new DynamicBusinessObjectPropertyComparer();
		public int Compare(ICustomProperty x, ICustomProperty y)
		{
			var result = inner.Compare(x.Info, y.Info);
			return result == 0 ? string.Compare(x.Identifier, y.Identifier, StringComparison.OrdinalIgnoreCase) : result;
		}
	}
}
