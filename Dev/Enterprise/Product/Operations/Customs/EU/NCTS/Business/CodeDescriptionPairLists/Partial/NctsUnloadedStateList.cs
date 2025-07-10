using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public partial class NctsUnloadedStateList
	{
		public static CodeDescriptionPairList CreateConfigurableList(IEnumerable<string> exclusions) 
		{
			var list = new NctsUnloadedStateList();

			foreach (var exclusion in exclusions) 
			{ 
				list.RemoveCode(exclusion);
			}

			return list;
		}
	}
}
