using System;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public class DynamicBusinessObjectPropertyComparer : IComparer<DynamicBusinessObjectProperty>
	{
		public int Compare(DynamicBusinessObjectProperty x, DynamicBusinessObjectProperty y)
		{
			var result = (x != null).CompareTo(y != null);
			if (result == 0 && x != null)
			{
				var xPos = x.GetPosition();
				var yPos = y.GetPosition();
				result = xPos.HasValue.CompareTo(yPos.HasValue);

				if (result == 0 && xPos.HasValue)
				{
					result = xPos.Value.CompareTo(yPos.Value);
				}

				if (result == 0)
				{
					var xCaption = x.GetCaption();
					var yCaption = y.GetCaption();
					result = (xCaption != null).CompareTo(yCaption != null);

					if (result == 0 && xCaption != null)
					{
						result = string.Compare(xCaption, yCaption, StringComparison.CurrentCulture);
					}
				}

				if (result == 0)
				{
					result = string.Compare(x.Type?.Name, y.Type?.Name, StringComparison.CurrentCulture);
				}
			}

			return result;
		}
	}
}
