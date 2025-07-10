using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business.Business.EventManagement
{
	public class CascadingLink
	{
		public IStmALogParent Parent { get; set; }
		public IBaseTrigger[] Triggers { get; set; }
	}

	public abstract class CascadingLinkComparer : IEqualityComparer<CascadingLink>
	{
		#region Comparers

		/// <summary>
		///		Gets a comparer which compares the the <see cref="CascadingLink"/> instances by PKs on <see cref="CascadingLink.Parent"/> and <see cref="CascadingLink.Triggers"/>s.
		/// </summary>
		public static CascadingLinkComparer ByPKs
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

		public abstract bool Equals(CascadingLink x, CascadingLink y);

		public abstract int GetHashCode(CascadingLink obj);

		#endregion

		#region Concrete Implementations

		/// <summary>
		///		Compares the <see cref="CascadingLink"/> instances by PKs on <see cref="CascadingLink.Parent"/> and <see cref="CascadingLink.Triggers"/>s.
		/// </summary>
		public class PKsComparer : CascadingLinkComparer
		{
			/// <summary>
			///		Checks whether the <paramref name="x"/> and <paramref name="y"/> are equal.
			/// </summary>			
			/// <returns>
			///		<c>true</c>, if PKs of <see cref="CascadingLink.Parent"/> and <see cref="CascadingLink.Triggers"/>s on <paramref name="x"/> equal to correspondent 
			///		PKs on <paramref name="y"/>; otherwise, <c>false</c>.
			/// </returns>
			public override bool Equals(CascadingLink x, CascadingLink y)
			{
				if (x == y)
				{
					return true;
				}

				if (x == null || y == null)
				{
					return false;
				}

				if (x.Triggers.Length != y.Triggers.Length)
				{
					return false;
				}

				if (x.Parent.LogsParentPK == y.Parent.LogsParentPK)
				{
					var expectedPKs = y.Triggers.Select(p => p.Identifier).ToList();
					if (x.Triggers.All(p => expectedPKs.Contains(p.Identifier)))
					{
						return true;
					}
				}

				return false;
			}

			public override int GetHashCode(CascadingLink obj)
			{
				var hash = 17;

				if (obj.Parent != null)
				{
					hash = (hash * 3) + obj.Parent.GetHashCode();
				}

				if (obj.Triggers != null)
				{
					hash = (hash * 3) + obj.Triggers.Sum(t => t.GetHashCode());
				}

				return hash;
			}
		}

		#endregion
	}
}
