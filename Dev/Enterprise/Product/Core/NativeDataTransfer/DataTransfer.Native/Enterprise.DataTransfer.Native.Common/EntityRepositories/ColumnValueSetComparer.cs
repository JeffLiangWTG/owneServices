namespace Enterprise.DataTransfer.Native.Common
{
	static class ColumnValueSetComparer
	{
		public static bool IsEqual(IColumnValueSet a, IColumnValueSet b)
		{
			if (ReferenceEquals(a, b))
			{
				return true;
			}

			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
			{
				return false;
			}

			int numColumns = a.ColumnCount;
			if (numColumns != b.ColumnCount)
			{
				return false;
			}

			for (int i = 0; i < numColumns; ++i)
			{
				var aVal = a.GetValue(i);
				var bVal = b.GetValue(i);
				if (!object.Equals(aVal, bVal))
				{
					return false;
				}
			}
			return true;
		}

		public static int AppendHashCode(int hash, object val)
		{
			unchecked
			{
				hash = (hash * 397) ^ (val != null ? val.GetHashCode() : 0);
			}
			return hash;
		}
	}
}
