using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.EventManagement
{
	public class PropagationLink
	{
		public PropagationLink(IStmALogParent target, IEnumerable<BusinessObject> siblings, string siblingsDescription, ElementGetter firstItemGetter = null, ElementGetter lastItemGetter = null)
		{
			this.Target = target;
			this.Siblings = siblings;
			this.SiblingsDescription = siblingsDescription;
			this.firstItemGetter = firstItemGetter;
			this.lastItemGetter = lastItemGetter;
		}

		readonly public IStmALogParent Target;
		readonly internal IEnumerable<BusinessObject> Siblings;
		readonly internal string SiblingsDescription;
		readonly ElementGetter firstItemGetter;
		readonly ElementGetter lastItemGetter;

		internal BusinessObject GetFirstWhereThereIsAnOrder()
		{
			if (firstItemGetter != null)
			{
				return firstItemGetter();
			}

			return Siblings == null ? null : Siblings.FirstOrDefault();
		}

		internal BusinessObject GetLastWhereThereIsAnOrder()
		{
			if (lastItemGetter != null)
			{
				return lastItemGetter();
			}

			return Siblings == null ? null : Siblings.LastOrDefault();
		}
	}

	public delegate BusinessObject ElementGetter();

	public abstract class PropagationLinkComparer : IEqualityComparer<PropagationLink>
	{
		#region Comparers

		/// <summary>
		///		Gets a comparer which compares the the <see cref="PropagationLink"/> instances by PKs on <see cref="PropagationLink.Target"/> and <see cref="PropagationLink.Siblings"/>s.
		/// </summary>
		public static PropagationLinkComparer ByPKs
		{
			get
			{
				if (pksComparer == null)
				{
					pksComparer = new PKsComparer();
				}

				return pksComparer;
			}
		}

		[ThreadStatic]
		static PKsComparer pksComparer;

		#endregion

		#region IEqualityComparer

		public abstract bool Equals(PropagationLink x, PropagationLink y);

		public abstract int GetHashCode(PropagationLink obj);

		#endregion

		#region Concrete Implementations

		/// <summary>
		///		Compares the <see cref="PropagationLink"/> instances by PKs on <see cref="PropagationLink.Target"/> and <see cref="PropagationLink.Siblings"/>s.
		/// </summary>
		public class PKsComparer : PropagationLinkComparer
		{
			/// <summary>
			///		Checks whether the <paramref name="x"/> and <paramref name="y"/> are equal.
			/// </summary>			
			/// <returns>
			///		<c>true</c>, if PKs of <see cref="PropagationLink.Target"/> and <see cref="PropagationLink.Siblings"/>s on <paramref name="x"/> equal to correspondent 
			///		PKs on <paramref name="y"/>; otherwise, <c>false</c>.
			/// </returns>
			public override bool Equals(PropagationLink x, PropagationLink y)
			{
				if (x == y)
				{
					return true;
				}

				if (x == null || y == null)
				{
					return false;
				}

				if (x.Siblings.Count() != y.Siblings.Count())
				{
					return false;
				}

				if (x.Target.LogsParentPK == y.Target.LogsParentPK)
				{
					var expectedPKs = y.Siblings.Select(p => p.PK).ToList();
					if (x.Siblings.All(p => expectedPKs.Contains(p.PK)))
					{
						return true;
					}
				}

				return false;
			}

			public override int GetHashCode(PropagationLink obj)
			{
				var hash = 17;

				if (obj.Target != null)
				{
					hash = (hash * 3) + obj.Target.GetHashCode();
				}

				if (obj.Siblings != null)
				{
					hash = (hash * 3) + obj.Siblings.Sum(s => s.GetHashCode());
				}

				return hash;
			}
		}

		#endregion
	}
}
