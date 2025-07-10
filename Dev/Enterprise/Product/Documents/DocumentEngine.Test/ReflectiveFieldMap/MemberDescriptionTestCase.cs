using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap.Testing
{
	abstract class MemberDescriptionTestCase : TestCaseWithFactory
	{
		public void TestDefaultValueShowIndex()
		{
			var memberDescription = new MemberDescriptionForTest(null, "", MemberDescription.MacroTagTypes.Document);
			AssertEquals(true, memberDescription.ShowIndex);
		}

		public void TestGetMacro()
		{
			var memberDescription1 = new MemberDescriptionForTest(null, "", MemberDescription.MacroTagTypes.Document);
			AssertEquals("<FullPath>", memberDescription1.GetMacro());

			var memberDescription2 = new MemberDescriptionForTest(null, "", MemberDescription.MacroTagTypes.Email);
			AssertEquals("(*FullPath*)", memberDescription2.GetMacro());

			AssertEquals("FullPath", new MemberDescriptionForTest(null, "", MemberDescription.MacroTagTypes.None).GetMacro());
		}

		public void TestGetMacro_NamespacePrefix()
		{
			AssertEquals("<@UXML.FullPath>", new MemberDescriptionForTest(null, "", MemberDescription.MacroTagTypes.Document, new UniversalXmlDataReflectorFilter()).GetMacro());
			AssertEquals("(*@UXML.FullPath*)", new MemberDescriptionForTest(null, "", MemberDescription.MacroTagTypes.Email, new UniversalXmlDataReflectorFilter()).GetMacro());
			AssertEquals("@UXML.FullPath", new MemberDescriptionForTest(null, "", MemberDescription.MacroTagTypes.None, new UniversalXmlDataReflectorFilter()).GetMacro());
		}

		public void TestGetPropertyInformation()
		{
			var memberDescription1 = new MemberDescriptionForTest(null, "", MemberDescription.MacroTagTypes.Document);
			AssertEquals("Test\r\n\r\n<FullPath>", memberDescription1.GetMemberInformation());

			var memberDescription2 = new MemberDescriptionForTest(null, "Help Text", MemberDescription.MacroTagTypes.Email);
			AssertEquals("Test\r\n\r\n(*FullPath*)\r\n\r\nHelp Text", memberDescription2.GetMemberInformation());

			var memberDescription3 = new MemberDescriptionForTest(null, "Help Text", MemberDescription.MacroTagTypes.Email);
			memberDescription3.UsePreviewText = true;
			AssertEquals("Test\r\n\r\nPreview:\r\n(*FullPath*)\r\n\r\nHelp Text", memberDescription3.GetMemberInformation());
		}

		class DummyItem
		{
		}

		class DummyCollection : IEnumerable
		{
			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotImplementedException();
			}

			public DummyItem this[int index]
			{
				get
				{
					throw new NotImplementedException();
				}
			}
		}

		public void TestGetCollectionChildTypeIEnumerableCheck()
		{
			var memberDescription = new MemberDescriptionForTest(null, "", MemberDescription.MacroTagTypes.Document);

			var collectionType1 = typeof(Collection<string>);
			var childType1 = memberDescription.GetCollectionChildType(collectionType1);
			AssertEquals(typeof(string), childType1);

			var collectionType2 = typeof(ICollection<string>);
			var childType2 = memberDescription.GetCollectionChildType(collectionType2);
			AssertEquals(typeof(string), childType2);

			var collectionType3 = typeof(List<string>);
			var childType3 = memberDescription.GetCollectionChildType(collectionType3);
			AssertEquals(typeof(string), childType3);

			var collectionType4 = typeof(IList<string>);
			var childType4 = memberDescription.GetCollectionChildType(collectionType4);
			AssertEquals(typeof(string), childType4);

			var collectionType5 = typeof(IEnumerable<string>);
			var childType5 = memberDescription.GetCollectionChildType(collectionType5);
			AssertEquals(typeof(string), childType5);

			var collectionType6 = typeof(string[]);
			var childType6 = memberDescription.GetCollectionChildType(collectionType6);
			AssertEquals(typeof(string), childType6);

			var collectionType7 = typeof(DummyCollection);
			AssertEquals(typeof(DummyItem), memberDescription.GetCollectionChildType(collectionType7));
		}

		class MemberDescriptionForTest : MemberDescription
		{
			public MemberDescriptionForTest(MemberDescription parentMemberDescription, string helpText, MacroTagTypes macroTagType)
				: this(parentMemberDescription, helpText, macroTagType, new DocDataReflectorFilter())
			{
			}

			public MemberDescriptionForTest(MemberDescription parentMemberDescription, string helpText, MacroTagTypes macroTagType, IDataReflectorFilter filter)
				: base(parentMemberDescription, helpText, macroTagType, filter)
			{
			}

			public override bool CanHaveChildMembers()
			{
				return false;
			}

			public override (Type ChildType, Type PossibleCollectionType) GetChildTypes()
			{
				return default;
			}

			public override string GetFullPath()
			{
				return "FullPath";
			}

			public override string GetFormattedTextLabel()
			{
				return "Test";
			}
		}
	}
}
