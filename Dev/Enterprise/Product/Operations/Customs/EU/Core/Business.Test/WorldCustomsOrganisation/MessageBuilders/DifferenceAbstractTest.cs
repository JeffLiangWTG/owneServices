using System;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing;

[TestsSubclassesOf(typeof(Difference))]
public abstract class DifferenceAbstractTest<T> : TestCaseWithFactory
	where T : Difference
{
	public T Constructor => constructor ?? (constructor = CreateConstructor());
	T constructor;
	protected abstract T CreateConstructor();
	protected abstract Type GenericClassType { get; }
	protected abstract ZString TypeOfAmendment { get; }
	protected abstract ZString XPath { get; }
	protected abstract ZString FullXPathWithNodeNames { get; }
	protected abstract ZString[] GetHierarchyStrings();
	protected abstract ZString HierarchyStrings { get; }
	protected abstract ZString GetHierarchyString();
	protected abstract ZString HierarchyString { get; }
	protected abstract string FullPointersFromNames { get; }
	protected abstract ZString LastPointer { get; }

	protected virtual IPointerParser PointerParser
	{
		get
		{
			var mockPointerParser = new Mock<IPointerParser>();
			mockPointerParser.Setup(m => m.GetFullPointersFromNames(It.IsAny<string>())).Returns(FullPointersFromNames);
			return mockPointerParser.Object;
		}
	}

	public void TestConstructorType()
	{
		AssertEquals(typeof(T), GenericClassType);
	}

	public void TestType()
	{
		AssertEquals(Constructor.Type, TypeOfAmendment);
	}

	public void TestXPath()
	{
		AssertEquals(XPath, Constructor.XPath);
	}

	public void TestFullXPathWithNodeNames()
	{
		AssertEquals(FullXPathWithNodeNames, Constructor.FullXPathWithNodeNames);
	}

	public void TestGetHierarchyStrings()
	{
		AssertEquals(HierarchyStrings, string.Concat(GetHierarchyStrings()));
	}

	public void TestGetHierarchyString()
	{
		AssertEquals(HierarchyString, GetHierarchyString());
	}

	public void TestRemoveLastPointer()
	{
		string.Concat(Constructor.WCOIDPointersWithoutLastPointer);
		AssertEquals(LastPointer, Constructor.removeLastPointer);
	}

	public void TestWCOIDPointers()
	{
		AssertEquals(FullPointersFromNames, string.Concat(Constructor.WCOIDPointers));
	}

	public void TestWCOIDPointersWithoutLastPointer()
	{
		AssertEquals(FullPointersFromNames, string.Concat(Constructor.WCOIDPointersWithoutLastPointer));
	}
}
