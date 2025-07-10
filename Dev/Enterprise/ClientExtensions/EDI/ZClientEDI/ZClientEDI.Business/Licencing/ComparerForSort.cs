using System;
using System.ComponentModel;
using CargoWise.Common.Collections;

namespace ZClientEDI.Business.Licencing;

public class ComparerForSort<TParentType, TComparableType> : ZComparer<TParentType> where TComparableType : IComparable<TComparableType>
{
	public ComparerForSort(Func<TParentType, TComparableType> provider, ListSortDirection direction) : base(
		(lhs, rhs) =>
			provider(lhs).CompareTo(provider(rhs)) * (direction == ListSortDirection.Ascending ? 1 : -1))
	{
	}
}
