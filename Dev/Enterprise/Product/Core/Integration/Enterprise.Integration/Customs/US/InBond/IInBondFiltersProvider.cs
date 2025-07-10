using System;
using Enterprise.Integration.ZArchitecture;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public static partial class InBond
			{
				public interface IInBondFiltersProvider
				{
					void AddFilters(IModuleFilterCollection filters, Type parent);
				}
			}
		}
	}
}