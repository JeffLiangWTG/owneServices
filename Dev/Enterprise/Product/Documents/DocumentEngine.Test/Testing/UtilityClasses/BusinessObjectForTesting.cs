using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	[TestExcludeBusinessObjectsAllHaveTestCases()]
	sealed class BusinessObjectForTesting : NonPersistentBusinessObject
	{
		public BusinessObjectForTesting(string name)
		{
			Name = name;
		}
		readonly string Name;

		public override string ToString()
		{
			return Name;
		}

		public ZString TestString
		{
			get { return Name; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public override string TableName
		{
			get
			{
				return "Test[NonPersistent]";
			}
		}

		public BusinessObjectCollectionForTestingWithNoFactoryOnlyConstructor CollectionWithNoFactoryOnlyConstructor
		{
			get
			{
				if (fCollectionWithNoFactoryOnlyConstructor == null)
				{
					fCollectionWithNoFactoryOnlyConstructor = new BusinessObjectCollectionForTestingWithNoFactoryOnlyConstructor();
					fCollectionWithNoFactoryOnlyConstructor.Add(new BusinessObjectForTesting("Child 0"));
					fCollectionWithNoFactoryOnlyConstructor.Add(new BusinessObjectForTesting("Child 1"));
					fCollectionWithNoFactoryOnlyConstructor.Add(new BusinessObjectForTesting("Child 2"));
				}
				return fCollectionWithNoFactoryOnlyConstructor;
			}
		}
		BusinessObjectCollectionForTestingWithNoFactoryOnlyConstructor fCollectionWithNoFactoryOnlyConstructor;
	}
}
