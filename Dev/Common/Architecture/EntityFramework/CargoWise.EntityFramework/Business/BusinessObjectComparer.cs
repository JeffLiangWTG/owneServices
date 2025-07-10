using System.Collections;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public abstract class BusinessObjectEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer
		where T : BusinessObject
	{
		BusinessObjectEqualityComparer() { }

		#region PKOnlyComparer

		/// <summary>
		/// Compare only the PK.
		/// Type and Factory are ignored.
		/// </summary>
		public static BusinessObjectEqualityComparer<T> PKOnlyComparer
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return new _PKOnlyComparer(); }
		}

		class _PKOnlyComparer : BusinessObjectEqualityComparer<T>
		{
			public override bool Equals(T x, T y)
			{
				if (x == null && y == null)
				{
					return true;
				}

				if (x == null || y == null)
				{
					return false;
				}

				return x.PK.Equals(y.PK);
			}

			public override int GetHashCode(T obj)
			{
				return obj == null ? 0 : obj.PK.GetHashCode();
			}
		}

		#endregion

		#region IgnoreFactoryComparer

		/// <summary>
		/// Compare Type and PK.
		/// Factory is ignored.
		/// </summary>
		public static BusinessObjectEqualityComparer<T> IgnoreFactoryComparer
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return new _IgnoreFactoryComparer(); }
		}

		class _IgnoreFactoryComparer : BusinessObjectEqualityComparer<T>
		{
			public override bool Equals(T x, T y)
			{
				if (x == null && y == null)
				{
					return true;
				}

				if (x == null || y == null)
				{
					return false;
				}

				return x.PK.Equals(y.PK) && x.GetType().Equals(y.GetType());
			}

			public override int GetHashCode(T obj)
			{
				return obj == null ? 0 : (obj.PK.GetHashCode() ^ obj.GetType().GetHashCode());
			}
		}

		#endregion

		#region InstanceComparer

		/// <summary>
		/// Compare business object instances (PK, Type and Factory).
		/// </summary>
		public static BusinessObjectEqualityComparer<T> InstanceComparer
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return new _InstanceComparer(); }
		}

		class _InstanceComparer : BusinessObjectEqualityComparer<T>
		{
			public override bool Equals(T x, T y)
			{
				if (x == null && y == null)
				{
					return true;
				}

				if (x == null || y == null)
				{
					return false;
				}

				return x.Equals(y);
			}

			public override int GetHashCode(T obj)
			{
				return obj == null ? 0 : obj.GetHashCode();
			}
		}

		#endregion

		#region IEqualityComparer<T> Members

		public abstract bool Equals(T x, T y);
		public abstract int GetHashCode(T obj);

		#endregion

		#region IEqualityComparer Members

		bool IEqualityComparer.Equals(object x, object y)
		{
			return Equals((T)x, (T)y);
		}

		int IEqualityComparer.GetHashCode(object obj)
		{
			return GetHashCode((T)obj);
		}

		#endregion
	}
}
