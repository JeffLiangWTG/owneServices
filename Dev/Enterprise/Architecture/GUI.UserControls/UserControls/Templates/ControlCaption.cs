using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.GUI
{
	internal class ControlCaption<T> : IControlCaption where T : BusinessObject
	{
		readonly Func<T, IDictionary<string, ResourceStringData>> getCaptionData;
		readonly Func<T, ZPropertyInfo>[] dependencies;

		public ControlCaption(Func<T, IDictionary<string, ResourceStringData>> getCaptionData, Func<T, ZPropertyInfo>[] dependencies)
		{
			this.getCaptionData = getCaptionData;
			this.dependencies = dependencies;
		}

		public bool TryGetCaption(BusinessObject bo, out IDictionary<string, ResourceStringData> captionData)
		{
			if ((dependencies?.Any() ?? false) && bo == null)
			{
				captionData = new Dictionary<string, ResourceStringData>();
				return false;
			}

			captionData = getCaptionData((T)bo);
			return captionData != null;
		}

		public IEnumerable<ZPropertyInfo> GetDependencies(BusinessObject bo)
		{
			var tbo = (T)bo;
			return dependencies.Select(d => d(tbo)).Where(p => p != null);
		}
	}
}
