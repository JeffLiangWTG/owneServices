using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	public interface ISupportSubAccountCollection
	{
		void Sort(string propertyName);

		ISupportSubAccount AddNew();

		void RemoveAndDeleteAll();

		ZString SortPropertyName { get; }

		IEnumerable<ISupportSubAccount> SubAccountElements { get; }
	}
}
