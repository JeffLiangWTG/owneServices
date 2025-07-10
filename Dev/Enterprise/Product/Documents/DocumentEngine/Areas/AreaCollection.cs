using System.Collections.Generic;

namespace Enterprise.DocumentEngine.Areas
{
	internal class AreaCollection : List<Area>
	{
		internal int Height
		{
			get
			{
				var result = 0;

				foreach (var area in this)
				{
					result += area.HeightInXls;
				}

				return result;
			}
		}
	}
}
