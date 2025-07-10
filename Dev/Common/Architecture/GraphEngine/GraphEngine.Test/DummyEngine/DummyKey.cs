using System;

namespace CargoWise.GraphEngine.Test
{
	public class DummyKey : IEquatable<DummyKey>
	{
		public DummyKey(string key)
		{
			inner = key;
		}

		readonly string inner;

		public override int GetHashCode()
		{
			return inner.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return (obj as DummyKey)?.Equals(this) ?? base.Equals(obj);
		}

		public bool Equals(DummyKey other)
		{
			return inner == other.inner;
		}

		public override string ToString()
		{
			return inner;
		}
	}
}
