using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(RefCountryWrapperForB13A))]
	sealed class RefCountryWrapperForB13ATest : NonPersistentBusinessObjectTestCase
	{
		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RefCountryWrapperForB13A();
		}

		#endregion
	}
}
