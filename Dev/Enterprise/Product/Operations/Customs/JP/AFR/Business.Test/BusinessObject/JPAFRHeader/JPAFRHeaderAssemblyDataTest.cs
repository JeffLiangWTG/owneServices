using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	sealed class JPAFRHeaderAssemblyDataTest : TestCase
	{
		public void TestOverrides()
		{
			var data = new JPAFRHeaderAssemblyData();
			AssertEquals("BusinessObjectType", typeof(JPAFRHeader), data.BusinessObjectType);
			AssertEquals("ReferenceType", Core.Constants.ReferenceTypes.SupplyChainLogistics, data.ReferenceType);
		}

		public void TestGetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			var data = new JPAFRHeaderAssemblyData();
			AssertEquals("CollectionType", typeof(JPAFRHeaderCollection), data.GetBusinessObjectCollection(factory));
		}
	}
}
