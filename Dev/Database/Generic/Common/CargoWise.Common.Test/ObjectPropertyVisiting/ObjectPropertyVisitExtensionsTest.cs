using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Testing.NameClashC;

namespace CargoWise.Common.Testing
{
	interface IA
	{
		string A { get; }
	}

	interface IB : IA
	{
		int B { get; }
	}

	interface IC
	{
		double C { get; }
		IB CB { get; }
	}

	struct BImpl : IB
	{
		public string A => "Bs A";

		int IB.B => 41;
	}

	struct BusinessObject
	{
		public string IgnoreMe => "Ignore Me";
	}

	class CustomEnumeration : IEnumerable<string>
	{
		readonly List<string> someList = new List<string>() { "a", "b", "c", "d" };

		public IEnumerator<string> GetEnumerator()
		{
			return ((IEnumerable<string>)this.someList).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this.someList).GetEnumerator();
		}
	}

	class CustomEnumerationWithProperty : IEnumerable<string>
	{
		readonly List<string> someList = new List<string>() { "a", "b", "c", "d" };

		public string AProperty => "My Enum Prop";

		public IEnumerator<string> GetEnumerator()
		{
			return ((IEnumerable<string>)this.someList).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this.someList).GetEnumerator();
		}
	}

	class Recursive
	{
		public string Property => "A Property";
		public Recursive Next => this;
	}

	class HasNull
	{
		public string SomeNull => null;
		public IB[] Array => null;
		public IEnumerable<IB> TypedEnumerable => null;
		public IEnumerable UntypedEnumerable => null;
		public Dictionary<string, int> Dictionary => null;
		public IEnumerable<IB> TypedEnumerableWithNull => new IB[] { new BImpl(), null, new BImpl() };
	}

	namespace NameClashA
	{
		interface INameClash
		{
			string NameClash { get; }
		}
	}

	namespace NameClashB
	{
		interface INameClash
		{
			string NameClash { get; }
		}
	}

	namespace NameClashC
	{
		interface INameClash
		{
			string NameClash { get; }
		}
	}

	namespace C.D
	{
		interface INameClash
		{
			string NameClash { get; }
		}
	}

	namespace B.C.D
	{
		interface INameClash
		{
			string NameClash { get; }
		}
	}

	namespace A.B.C.D
	{
		interface INameClash
		{
			string NameClash { get; }
		}
	}

	class NameClash : NameClashA.INameClash, NameClashB.INameClash, INameClash, A.B.C.D.INameClash, B.C.D.INameClash, C.D.INameClash
	{
		string NameClashA.INameClash.NameClash => "NameClash A";

		string NameClashB.INameClash.NameClash => "NameClash B";

		string INameClash.NameClash => "NameClash C";

		string A.B.C.D.INameClash.NameClash => "NameClash A.B.C.D";

		string B.C.D.INameClash.NameClash => "NameClash B.C.D";

		string C.D.INameClash.NameClash => "NameClash C.D";
	}

	class BaseA
	{
		public int A { get; } = 5;
		public int B { get; } = 5;
	}

	class DerivedA : BaseA
	{
		public new int A { get; } = 6;
	}

	sealed class ATest : IB, IC
	{
		public string A => "Some A";
		public int B => 42;
		public double C => 123.123456;
		public IB CB => new BImpl();
		public IB[] Array => new IB[] { new BImpl(), new BImpl(), new BImpl() };
		public IEnumerable<IB> TypedEnumerable => new IB[] { new BImpl(), new BImpl(), new BImpl() };
		public IEnumerable UntypedEnumerable => new[] { 1, 2, 3 };
		public Dictionary<string, int> Dictionary => new Dictionary<string, int>()
		{ { "A", 1 }, { "B", 2 }, { "C", 3 } };
		public IEnumerable<IEnumerable<KeyValuePair<string, int>>> NestNest => new[] { new Dictionary<string, int>()
		{ { "A", 1 }, { "B", 2 } }, new Dictionary<string, int>()
		{ { "C", 3 }, { "D", 4 } } };
		public CustomEnumeration MyEnumerable => new CustomEnumeration();
		public List<int> List => new[] { 1, 2, 3 }.ToList();
		public IList UntypedList => new[] { 1, 2, 3 }.ToList();
		public BusinessObject IgnoreThis => new BusinessObject();
	}
}
