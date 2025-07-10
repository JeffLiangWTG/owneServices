using System;
using System.Collections.Generic;

namespace Enterprise.DocumentEngine
{
	public class ValueProviderList : List<ValueProvider>
	{
		public ValueProviderList()
		{
		}

		public ValueProviderList(IEnumerable<ValueProvider> value)
			: base(value)
		{
		}

		public ValueProvider this[string responsibility]
		{
			get
			{
				int index = IndexOfResponsibleProvider(responsibility);
				if (index > -1)
				{
					return this[index];
				}
				else
				{
					throw new Exception("Can't find " + responsibility + " provider in list");
				}
			}
		}

		public bool ContainsProviderResponsibleFor(string responsibility)
		{
			return IndexOfResponsibleProvider(responsibility) > -1;
		}

		int IndexOfResponsibleProvider(string responsibility)
		{
			int result = -1;
			for (int counter = 0; counter < this.Count && result == -1; counter++)
			{
				if (this[counter].IsResponsibleForReplacing(responsibility, Passes.FirstPass))
				{
					result = counter;
					break;
				}
			}
			return result;
		}
	}
}
