using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	abstract class FilterFieldTest : NonPersistentBusinessObjectTestCase
	{
		FilterField filterField;
		protected FilterField FilterField => filterField ?? (filterField = (FilterField)GetNewBusinessObject());

		protected override BusinessObject GetNewBusinessObject() => (BusinessObject)Activator.CreateInstance(ExpectedBusinessObjectType, new object[] { Factory });
	}
}
