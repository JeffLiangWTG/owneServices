using System;
using System.Collections.Generic;

namespace Enterprise.Integration
{
	public interface IRegistryItemVisibility
	{
		bool IsVisible(IRegistryItem item);

		bool IsVisible(Guid companyPk, Guid branchPk, IEnumerable<Guid> countryFilterPKs);
	}
}
