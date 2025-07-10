using System.Collections;

namespace CargoWise.EntityFramework
{
	public interface IFreezeSortOnElementModifyComparer
	{
		IComparer Comparer { get; }
	}

	public sealed class FreezeSortOnElementModifyComparer : IComparer, IFreezeSortOnElementModifyComparer
	{
		FreezeSortOnElementModifyComparer(IComparer comparer)
		{
			this.Comparer = comparer;
		}

		public static FreezeSortOnElementModifyComparer Wrap(IComparer comparer)
		{
			return comparer == null ? null : comparer as FreezeSortOnElementModifyComparer ?? new FreezeSortOnElementModifyComparer(comparer);
		}

		public int Compare(object x, object y)
		{
			return Comparer.Compare(x, y);
		}

		public IComparer Comparer { get; private set; }

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			FreezeSortOnElementModifyComparer rhs = obj as FreezeSortOnElementModifyComparer;
			return rhs != null && object.Equals(Comparer, rhs.Comparer);
		}

		public override int GetHashCode()
		{
			return Comparer.GetHashCode() + 1;
		}

		#endregion
	}
}
