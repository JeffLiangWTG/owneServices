using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.LogWalker
{
	sealed class StmJobQueueCustomComparer : IEqualityComparer<StmJobQueue>
	{
		public StmJobQueueCustomComparer(params Func<StmJobQueue, IComparable>[] propertiesToCompare)
		{
			this.propertiesToCompare = Argument.NotNull(propertiesToCompare, "propertiesToCompare");

			if (propertiesToCompare.Length == 0)
			{
				throw new ArgumentException("There are no properties specified. Please specify at least one property to compare.", nameof(propertiesToCompare));
			}
		}

		public bool Equals(StmJobQueue x, StmJobQueue y)
		{
			return propertiesToCompare.All(getProperty => getProperty(x).CompareTo(getProperty(y)) == 0);
		}

		public int GetHashCode(StmJobQueue obj)
		{
			var hash = 17;

			foreach (var getProperty in propertiesToCompare)
			{
				hash = (hash * 7) + getProperty(obj).GetHashCode();
			}

			return hash;
		}

		readonly Func<StmJobQueue, IComparable>[] propertiesToCompare;
	}
}
