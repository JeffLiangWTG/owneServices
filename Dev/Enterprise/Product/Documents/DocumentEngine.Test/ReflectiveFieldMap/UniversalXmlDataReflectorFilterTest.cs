using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap.Testing
{
	sealed class UniversalXmlDataReflectorFilterTest : TestCase
	{
		public void TestIsAllowed()
		{
			var filter = new UniversalXmlDataReflectorFilter();
			var testType = typeof(ClassToReflect);
			AssertEquals("bool", false, filter.IsAllowed(testType.GetProperty(nameof(ClassToReflect.Bool1))));
			AssertEquals("ZBool", true, filter.IsAllowed(testType.GetProperty(nameof(ClassToReflect.ZBool1))));
			AssertEquals("Nullable ZBool", true, filter.IsAllowed(testType.GetProperty(nameof(ClassToReflect.NullableZBool))));
			AssertEquals("string", false, filter.IsAllowed(testType.GetProperty(nameof(ClassToReflect.String1))));
			AssertEquals("ZString", true, filter.IsAllowed(testType.GetProperty(nameof(ClassToReflect.ZString1))));
			AssertEquals("Nullable ZString", true, filter.IsAllowed(testType.GetProperty(nameof(ClassToReflect.NullableZString))));
			AssertEquals("class implementing IDataObject", true, filter.IsAllowed(testType.GetProperty(nameof(ClassToReflect.Mode))));
			AssertEquals("enum", true, filter.IsAllowed(testType.GetProperty(nameof(ClassToReflect.TransportModeEnum))));
		}

		public void TestCanHaveChildMembers()
		{
			var filter = new UniversalXmlDataReflectorFilter();
			AssertEquals("bool", false, filter.CanHaveChildMembers(typeof(bool)));
			AssertEquals("ZBool", false, filter.CanHaveChildMembers(typeof(ZBool)));
			AssertEquals("Nullable ZBool", false, filter.CanHaveChildMembers(typeof(ZBool?)));

			AssertEquals("string", false, filter.CanHaveChildMembers(typeof(string)));
			AssertEquals("ZString", false, filter.CanHaveChildMembers(typeof(ZString)));
			AssertEquals("Nullable ZString", false, filter.CanHaveChildMembers(typeof(ZString?)));
			AssertEquals("class implementing IDataObject", true, filter.CanHaveChildMembers(typeof(CodeDescriptionPairForTest)));

			var testType = typeof(ClassToReflect);

			AssertEquals("List<> CanHaveChildMembers", true, filter.CanHaveChildMembers(testType.GetProperty(nameof(ClassToReflect.List1)).PropertyType));
			AssertEquals("IList<> CanHaveChildMembers", true, filter.CanHaveChildMembers(testType.GetProperty(nameof(ClassToReflect.IList1)).PropertyType));
			AssertEquals("IEnumerable<> CanHaveChildMembers", true, filter.CanHaveChildMembers(testType.GetProperty(nameof(ClassToReflect.IEnumerable1)).PropertyType));

			AssertEquals("Collection<> CanHaveChildMembers", true, filter.CanHaveChildMembers(testType.GetProperty(nameof(ClassToReflect.Collection1)).PropertyType));
			AssertEquals("ICollection<> CanHaveChildMembers", true, filter.CanHaveChildMembers(testType.GetProperty(nameof(ClassToReflect.ICollection1)).PropertyType));
			AssertEquals("CustomList CanHaveChildMembers", true, filter.CanHaveChildMembers(testType.GetProperty(nameof(ClassToReflect.CustomList1)).PropertyType));
		}

		public void TestIsCollection()
		{
			var filter = new UniversalXmlDataReflectorFilter();
			var testType = typeof(ClassToReflect);

			AssertEquals("List<> IsCollection", true, filter.IsCollection(testType.GetProperty(nameof(ClassToReflect.List1)).PropertyType));
			AssertEquals("IList<> IsCollection", true, filter.IsCollection(testType.GetProperty(nameof(ClassToReflect.IList1)).PropertyType));
			AssertEquals("CustomList IsCollection", true, filter.IsCollection(testType.GetProperty(nameof(ClassToReflect.CustomList1)).PropertyType));

			AssertEquals("Collection<> IsCollection", true, filter.IsCollection(testType.GetProperty(nameof(ClassToReflect.Collection1)).PropertyType));
			AssertEquals("ICollection<> IsCollection", true, filter.IsCollection(testType.GetProperty(nameof(ClassToReflect.ICollection1)).PropertyType));

			AssertEquals("IEnumerable<> IsCollection", true, filter.IsCollection(testType.GetProperty(nameof(ClassToReflect.IEnumerable1)).PropertyType));
		}

		public void TestDocDataProviderReflector()
		{
			var testType = typeof(ClassToReflect);
			var dataReflector = new DocDataProviderReflector(testType, new UniversalXmlDataReflectorFilter(), MemberDescription.MacroTagTypes.Document);
			var members = dataReflector.Members;
			var fullPaths = members.Select(x => x.GetFullPath()).OrderBy(x => x).ToArray();

			var list1ExpectedPath = @"List1[1]";
			var list1Member = members.First(x => x.GetFullPath() == list1ExpectedPath);
			var list1Reflector = new DocDataReflector(list1Member);
			var list1MemberPaths = list1Reflector.Members.Select(x => x.GetFullPath()).OrderBy(x => x).ToArray();

			var portExpectedPath = "Port";
			var portMember = members.First(x => x.GetFullPath() == portExpectedPath);
			var portReflector = new DocDataReflector(portMember);
			var portMemberPaths = portReflector.Members.Select(x => x.GetFullPath()).OrderBy(x => x).ToArray();

			var expectedClasstoReflectPaths = new[]
			{
				@"Collection1[1]",
				@"CustomList1[1]",
				@"ICollection1[1]",
				@"IEnumerable1[1]",
				@"IList1[1]",
				@"List1[1]",
				@"Mode",
				@"NullableZBool",
				@"NullableZString",
				@"Port",
				@"TransportModeEnum",
				@"ZBool1",
				@"ZString1",
			};

			var expectedList1MemberPaths = expectedClasstoReflectPaths
				.Select(x => list1ExpectedPath + "." + x)
				.ToArray();

			var expectedPortMemberPaths = new[]
			{
				"Port.Code",
				"Port.Name"
			};

			AssertArrayEqualsByElements(expectedClasstoReflectPaths, fullPaths);
			AssertArrayEqualsByElements(expectedList1MemberPaths, list1MemberPaths);
			AssertArrayEqualsByElements(expectedPortMemberPaths, portMemberPaths);
		}

		class ClassToReflect
		{
			// Not reflected since bool is in TypesToIgnore
			public bool Bool1 { get; set; }

			public ZBool ZBool1 { get; set; }
			public ZBool? NullableZBool { get; set; }

			public string String1 { get; set; }
			public ZString ZString1 { get; set; }
			public ZString? NullableZString { get; set; }

			public CodeDescriptionPairForTest Mode { get; set; }

			public TransportModeEnumForTest TransportModeEnum { get; set; }

			public List<ClassToReflect> List1 { get; private set; }
			public IList<ChildClassToReflect> IList1 { get; private set; }
			public IEnumerable<ChildClassToReflect> IEnumerable1 { get; private set; }
			public Collection<ChildClassToReflect> Collection1 { get; private set; }
			public ICollection<ChildClassToReflect> ICollection1 { get; private set; }
			public CustomList CustomList1 { get; private set; }
			public UNLOCOForTest Port { get; set; }
		}

		class CustomList : List<ClassToReflect>
		{
		}

		class UNLOCOForTest : UniversalDataBuss.Integration.ICodeDataObject
		{
			[MaxLength(5), Mandatory]
			public ZString? Code { get; set; }
			[MaxLength(35)]
			public ZString? Name { get; set; }
		}

		public enum TransportModeEnumForTest
		{
			Air,
			Sea,
			Road,
			Rail,
		}

		class ChildClassToReflect
		{
			public ZByte? Order { get; set; }
			public TransportModeEnumForTest? TransportMode { get; set; }
		}

		class CodeDescriptionPairForTest : UniversalDataBuss.Integration.IDataObject
		{
			public ZString? Code { get; set; }
			public ZString? Description { get; set; }
		}
	}
}
