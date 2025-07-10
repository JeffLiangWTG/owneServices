using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public abstract class AutomaticFilterTest : TestCaseWithFactory
	{
		public void AssertHelperFiltersWork(string methodName, BusinessObject filterStripBusinessObject, Type businessObjectType, Func<BusinessObjectFactory, Type, BusinessObject> getNewBusinessObject)
		{
			var filterBusinessObject = (FilterStripBusinessObject)filterStripBusinessObject;

			var method = GetType().GetMethod(methodName);
			var parameters = new object[] { filterBusinessObject, businessObjectType, getNewBusinessObject };
			method.Invoke(this, parameters);
		}

		public abstract void SetUpForHelperFiltersWorkTests(Type businessObjectType);
	}
}
