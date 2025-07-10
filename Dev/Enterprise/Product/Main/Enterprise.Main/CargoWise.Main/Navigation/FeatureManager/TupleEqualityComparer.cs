using System;
using System.Collections.Generic;

namespace CargoWise.Main.Navigation;

public class TupleEqualityComparer : IEqualityComparer<(string, string)>
{
	public bool Equals((string, string) x, (string, string) y)
	{
		return string.Equals(x.Item1, y.Item1, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(x.Item2, y.Item2, StringComparison.OrdinalIgnoreCase);
	}

	public int GetHashCode((string, string) obj)
	{
		int hash1 = StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item1 ?? string.Empty);
		int hash2 = StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Item2 ?? string.Empty);
		return hash1 ^ hash2;
	}
}
