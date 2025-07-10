using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.XmlIO.XmlReading.ElementReaders;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.Testing
{
	class IElementParserExtensionsTest : TestCaseWithFactory
	{
		class DummyElementParser : IElementNavigator
		{
			readonly string name;

			public DummyElementParser(string name)
			{
				this.name = name;
			}
			public void ParseOutContent(string currentLineText, object targetDataObject1)
			{
				throw new NotImplementedException();
			}

			public IElementNavigator ParentElement
			{
				get;
				set;
			}

			public string CurrentElementName
			{
				get { return name; }
			}
		}

		public void TestReturnsEmptyIfUsedOnNullIElementParser()
		{
			DummyElementParser nameElementParser = null;
			AssertEquals("", nameElementParser.GetFullName());
		}

		public void TestIElementParserExtensions()
		{
			var nameElementParser = new DummyElementParser("Name");
			AssertEquals("<Name>", nameElementParser.GetFullName());

			var parentElementParser = new DummyElementParser("Parent");
			nameElementParser.ParentElement = parentElementParser;
			AssertEquals("<Parent>.<Name>", nameElementParser.GetFullName());

			var parentsParentElementParser = new DummyElementParser("ParentsParent");
			parentElementParser.ParentElement = parentsParentElementParser;
			AssertEquals("<ParentsParent>.<Parent>.<Name>", nameElementParser.GetFullName());

			var parentsParentsParentElementParser = new DummyElementParser("ParentsParentsParent");
			parentsParentElementParser.ParentElement = parentsParentsParentElementParser;
			AssertEquals("<ParentsParentsParent>.<ParentsParent>.<Parent>.<Name>", nameElementParser.GetFullName());
		}
	}
}

