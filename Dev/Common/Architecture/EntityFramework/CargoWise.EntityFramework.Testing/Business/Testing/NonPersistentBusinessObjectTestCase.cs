using System;
using System.Reflection;
using CargoWise.Integration;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestsSubclassesOf(
			typeof(NonPersistentBusinessObject),
			typeof(TestExcludeBusinessObjectsAllHaveTestCasesAttribute),
			new Type[] { typeof(IDocumentWrapper) })]
	public abstract class NonPersistentBusinessObjectTestCase : BusinessObjectBaseTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			try
			{
				BusinessObject result;
				ConstructorInfo constructorWithFactory = GetExpectedBusinessObjectType().GetConstructor(new Type[] { typeof(BusinessObjectFactory) });
				if (constructorWithFactory != null)
				{
					result = (BusinessObject)Activator.CreateInstance(GetExpectedBusinessObjectType(), new object[] { Factory });
				}
				else
				{
					result = (BusinessObject)Activator.CreateInstance(GetExpectedBusinessObjectType(), null);
				}
				return result;
			}
			catch
			{
				throw new ApplicationException("Error occurred when trying to instantiate NonPersistentBusinessObject - override GetNewBusinessObject and construct it yourself");
			}
		}

		public void TestIWrapPersistentBizOImplementersOverridePKSchemaColumn()
		{
			var bizO = GetNewBusinessObject();
			if (!(bizO is IWrapPersistentBizO))
			{
				Assert(true);
				return;
			}
			AssertNotEquals("PKSchemaColumn should not be GenericPkColumn", CargoWise.Schema.Schema.GenericPkColumn, bizO.PKSchemaColumn);
		}
	}
}
