using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public class CodeDescriptionPairListHolder : IService
	{
		#region Instance Stuff

		internal CodeDescriptionPairListHolder()
		{
			lists = new Dictionary<string, CodeDescriptionPairList>();
		}

		public CodeDescriptionPairList this[string key]
		{
			get { return lists[key]; }
			set { lists[key] = value; }
		}

		public CodeDescriptionPairList this[ZQuery filter]
		{
			get { return this[GetKeyForFilter(filter)]; }
			set { this[GetKeyForFilter(filter)] = value; }
		}

		string GetKeyForFilter(ZQuery filter)
		{
			return filter.LiteralTextADO;
		}

		readonly Dictionary<string, CodeDescriptionPairList> lists;

		#endregion
	}
}
