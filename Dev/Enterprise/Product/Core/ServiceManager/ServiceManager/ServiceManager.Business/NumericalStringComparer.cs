using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.Types;

namespace Enterprise.ServiceManager.Business
{
	class NumericalStringComparer<TParentType> : ZComparer<TParentType>
	{
		public NumericalStringComparer(Func<TParentType, ZString> provider, ListSortDirection direction) : base(
			(lhs, rhs) =>
			{
				var directionMultiplier = (direction == ListSortDirection.Ascending ? 1 : -1);

				var lhsString = provider(lhs);
				var rhsString = provider(rhs);

				if (string.IsNullOrEmpty(lhsString) && string.IsNullOrEmpty(rhsString))
				{
					return 0;
				}
				else if (string.IsNullOrEmpty(lhsString))
				{
					return 1;
				}
				else if (string.IsNullOrEmpty(rhsString))
				{
					return -1;
				}

				var lhsMatches = GetLineNums(lhsString, direction);
				var rhsMatches = GetLineNums(rhsString, direction);

				var index = 0;
				var comparison = 0;

				while (comparison == 0 && (index < lhsMatches.Count && index < rhsMatches.Count))
				{
					comparison = lhsMatches[index].CompareTo(rhsMatches[index]);
					index++;
				}

				return directionMultiplier * (
					comparison == 0
						? lhsMatches.Count.CompareTo(rhsMatches.Count)
						: comparison);
			})
		{
		}

		static List<int> GetLineNums(string line, ListSortDirection direction)
		{
			var nums = line.Split(';')
				.SelectMany(section => section.Split(':').Last().Replace("]", string.Empty).Split(','))
				.Select(int.Parse);

			return direction switch
			{
				ListSortDirection.Ascending => nums.OrderBy(i => i).ToList(),
				ListSortDirection.Descending => nums.OrderByDescending(i => i).ToList(),
				_ => nums.ToList()
			};
		}
	}
}
