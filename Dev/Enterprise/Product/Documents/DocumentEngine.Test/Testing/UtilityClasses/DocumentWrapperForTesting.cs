using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	[TestClass]
	[TestExcludeBusinessObjectsAllHaveTestCases()]
	public class DocumentWrapperForTesting : DocumentWrapper
	{
		public DocumentWrapperForTesting(string testStringValue, ZInt testIntValue)
		{
			fTestString = testStringValue;
			fTestInt = testIntValue;
		}

		public DocumentWrapperForTesting(string testStringValue)
			: this(testStringValue, 110)
		{
		}

		public ZString Name
		{
			get { return "TestBusinessObjectOverridenName"; }
		}

		public ZString Charlie
		{
			get { return "EatsDogs"; }
		}

		public override string TableName
		{
			get
			{
				return "Test[NonPersistent]";
			}
		}

		public override string TablePrefix => "ACR";

		public string TestValue;

		readonly ZInt fTestInt;

		public ZInt TestInt
		{
			get { return fTestInt; }
		}

		public BusinessObjectForTestingWithZStringValue SimpleClass
		{
			get { return new BusinessObjectForTestingWithZStringValue("Simple"); }
		}

		public ZString TestString
		{
			get { return fTestString; }
			set { fTestString = value; }
		}

		public ZString[] StringArray
		{
			get { return fStringArray; }
		}

		public BusinessObjectForTestingWithZStringValue ValueHolder
		{
			get { return new BusinessObjectForTestingWithZStringValue(TestValue); }
		}

		public ZString ObsoleteString
		{
			[DocumentEngineObsoleteField("unit test")]
			get { return "Obsolete"; }
		}

		public DocumentWrapperCollectionForTesting Children
		{
			get
			{
				if (fChildren == null)
				{
					fChildren = new DocumentWrapperCollectionForTesting();
					fChildren.Add(new DocumentWrapperForTesting("Child 0"));
					fChildren.Add(new DocumentWrapperForTesting("Child 1"));
					fChildren.Add(new DocumentWrapperForTesting("Child 2"));
				}

				return fChildren;
			}
		}

		public DocumentWrapperForTesting SingleChild
		{
			get { return singleChild ?? (singleChild = new DocumentWrapperForTesting("Single Child")); }
		}

		public DocumentWrapperCollectionForTesting NullChild
		{
			get
			{
				return null;
			}
		}

		public DocumentWrapperCollectionForTesting EmptyCollection
		{
			get
			{
				return new DocumentWrapperCollectionForTesting();
			}
		}

		public override string ToString()
		{
			return "TestBusinessObject";
		}

		string fTestString;
		readonly ZString[] fStringArray = new ZString[] { "str1", "str2", "str3", "str4" };
		DocumentWrapperCollectionForTesting fChildren;
		DocumentWrapperForTesting singleChild;
	}
}
