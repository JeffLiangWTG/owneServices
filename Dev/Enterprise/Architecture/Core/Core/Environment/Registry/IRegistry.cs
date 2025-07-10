using System.Collections.Generic;

namespace Enterprise.ZArchitecture.Core.Environment.Registry
{
	public interface IRegistry
	{
		IEnumerable<RegistryCategoryRef> GetSortedTopLevelCategories();
		RegistryCategoryContent GetSortedContent(object categoryKey);
	}
}
