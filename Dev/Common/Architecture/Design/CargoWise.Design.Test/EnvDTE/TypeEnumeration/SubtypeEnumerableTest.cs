using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using CargoWise.Common.Collections;
using CargoWise.Design.DTE.TypeEnumeration.Testing.TestNamespaceForSubtypeEnumerable;
using NUnit.Framework;
using static CargoWise.Design.DTE.Testing.SubtypeEnumerableTest;

namespace CargoWise.Design.DTE.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class SubtypeEnumerableTest : TestCase
	{
		public void TestEnumerateAcrossAssemblies()
		{
			Assembly[] assemblies = new Assembly[] { typeof(ListUtil).Assembly, typeof(SubtypeEnumerable).Assembly };
			ArrayList expectedResults = new ArrayList();
			foreach (Assembly assembly in assemblies)
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (typeof(IList).IsAssignableFrom(type) && type != typeof(IList))
					{
						expectedResults.Add(type);
					}
				}
			}

			ArrayList actualResults = new ArrayList();
			foreach (Type type in new SubtypeEnumerable(true, assemblies, typeof(IList)))
			{
				actualResults.Add(type);
			}

			AssertEquals(expectedResults.Count, actualResults.Count);
			for (int i = 0; i < actualResults.Count; i++)
			{
				AssertEquals(expectedResults[i], actualResults[i]);
			}
		}

		public void TestPrivateFilter()
		{
			int foundPublicCount = 0;
			Assembly[] assemblies = new Assembly[] { GetType().Assembly };
			foreach (Type type in new SubtypeEnumerable(false, assemblies, typeof(SubtypeEnumerableTestBaseType), ""))
			{
				if (type == typeof(PublicClass))
				{
					foundPublicCount++;
				}

				if (type == typeof(PublicNestedClass))
				{
					foundPublicCount++;
				}

				AssertNotEquals(typeof(PrivateClass), type);
				AssertNotEquals(typeof(PrivateNestedClass), type);
			}

			AssertEquals("Found 2 public classes", 2, foundPublicCount);
			int foundPrivateAndPublicCount = 0;
			foreach (Type type in new SubtypeEnumerable(true, assemblies, typeof(SubtypeEnumerableTestBaseType), ""))
			{
				if (type == typeof(PublicClass))
				{
					foundPrivateAndPublicCount++;
				}

				if (type == typeof(PublicNestedClass))
				{
					foundPrivateAndPublicCount++;
				}

				if (type == typeof(PrivateClass))
				{
					foundPrivateAndPublicCount++;
				}

				if (type == typeof(PrivateNestedClass))
				{
					foundPrivateAndPublicCount++;
				}
			}

			AssertEquals("Found 4 public/classes", 4, foundPrivateAndPublicCount);
		}

		public void TestNamespaceFilter()
		{
			Assembly[] assemblies = new Assembly[] { GetType().Assembly };
			Type[] types = new SubtypeEnumerable(true, assemblies, typeof(SubtypeEnumerableTestBaseType), "CargoWise.Design.DTE.TypeEnumeration.Testing.TestNamespaceForSubtypeEnumerable").GetTypes();
			AssertEquals("Only 1 type found in the namespace", 1, types.Length);
			AssertEquals("The correct type in namespace", typeof(TestClass), types[0]);
		}

		class PrivateNestedClass : SubtypeEnumerableTestBaseType
		{
		}

		public class PublicNestedClass : SubtypeEnumerableTestBaseType
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public class SubtypeEnumerableTestBaseType { }
		[EditorBrowsable(EditorBrowsableState.Never)]
		internal class PrivateClass : SubtypeEnumerableTestBaseType { }
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class PublicClass : SubtypeEnumerableTestBaseType { }
	}
}

namespace CargoWise.Design.DTE.TypeEnumeration.Testing.TestNamespaceForSubtypeEnumerable
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class TestClass : SubtypeEnumerableTestBaseType { }
}
