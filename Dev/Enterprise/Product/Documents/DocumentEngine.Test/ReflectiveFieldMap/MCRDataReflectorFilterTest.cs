using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap.Testing
{
	sealed class MCRDataReflectorFilterTest : TestCase
	{
		public void TestIsCollection()
		{
			var filter = new MCRDataReflectorFilter();
			var testType = typeof(ClassToReflect);

			AssertEquals("List<> IsCollection", true, filter.IsCollection(testType.GetProperty(nameof(ClassToReflect.List1)).PropertyType));
			AssertEquals("IList<> IsCollection", true, filter.IsCollection(testType.GetProperty(nameof(ClassToReflect.IList1)).PropertyType));
			AssertEquals("CustomList IsCollection", true, filter.IsCollection(testType.GetProperty(nameof(ClassToReflect.CustomList1)).PropertyType));

			AssertEquals("Collection<> IsCollection", true, filter.IsCollection(testType.GetProperty(nameof(ClassToReflect.Collection1)).PropertyType));
			AssertEquals("ICollection<> IsCollection", true, filter.IsCollection(testType.GetProperty(nameof(ClassToReflect.ICollection1)).PropertyType));

			AssertEquals("IEnumerable<> IsCollection", true, filter.IsCollection(testType.GetProperty(nameof(ClassToReflect.IEnumerable1)).PropertyType));
			AssertEquals("IReadOnlyCollection<> IsCollection", true, filter.IsCollection(testType.GetProperty(nameof(ClassToReflect.IReadOnlyCollection1)).PropertyType));
		}

		class ClassToReflect
		{
			public List<ClassToReflect> List1 { get; private set; }
			public IList<ChildClassToReflect> IList1 { get; private set; }
			public IEnumerable<ChildClassToReflect> IEnumerable1 { get; private set; }
			public IReadOnlyCollection<ChildClassToReflect> IReadOnlyCollection1 { get; private set; }

			public Collection<ChildClassToReflect> Collection1 { get; private set; }
			public ICollection<ChildClassToReflect> ICollection1 { get; private set; }
			public CustomList CustomList1 { get; private set; }
		}

		class CustomList : List<ClassToReflect>
		{
		}

		class ChildClassToReflect
		{
			public ZByte? Order { get; set; }
		}
	}
}
