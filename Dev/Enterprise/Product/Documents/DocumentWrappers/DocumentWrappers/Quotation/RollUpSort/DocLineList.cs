using System;
using System.Collections;
using System.Collections.Generic;
using Enterprise.DocumentEngineIntegration.RollUpSort;

namespace Enterprise.DocumentWrappers.Quotation.RollUpSort
{
	sealed class DocLineList : List<ISortableDocLine>, ISortableDocLineList
	{
		void ISortableDocLineList.Sort(IComparer comparer)
		{
			var array = this.ToArray();
			Array.Sort(array, comparer);

			for (var i = 0; i < Count; i++)
			{
				this[i] = array[i];
			}
		}
	}
}
