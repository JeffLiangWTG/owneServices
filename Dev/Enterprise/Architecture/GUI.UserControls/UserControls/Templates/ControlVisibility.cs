using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	internal class ControlVisibility<T> : IControlVisibility where T : BusinessObject
	{
		readonly Func<T, bool> isVisible;
		readonly Func<T, ZPropertyInfo>[] dependencies;

		public ControlVisibility(Func<T, bool> isVisible, Func<T, ZPropertyInfo>[] dependencies)
		{
			this.isVisible = isVisible;
			this.dependencies = dependencies;
		}

		public bool IsVisible(BusinessObject bo)
		{
			return bo != null && isVisible((T)bo);
		}

		public IEnumerable<ZPropertyInfo> GetDependencies(BusinessObject bo)
		{
			var tbo = (T)bo;
			return dependencies.Select(d => d(tbo)).Where(p => p != null);
		}
	}
}
