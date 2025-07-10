using CargoWise.EntityFramework.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	public class AsycudaBillAssemblyDataTest : TestCaseWithFactory
	{
		public void TestOverrides()
		{
			var data = new AsycudaBillAssemblyData();
			AssertEquals("BusinessObjectType", typeof(AsycudaBill), data.BusinessObjectType);
			AssertEquals("ReferenceType", ReferenceTypes.SupplyChainLogistics, data.ReferenceType);
			AssertNull("GetBusinessObjectCollection", data.GetBusinessObjectCollection(Factory));
			AssertType<AsycudaBillEDocsViaUniversalXmlSupport>("GetEDocsViaUniversalXmlSupport", data.GetEDocsViaUniversalXmlSupport());
		}
	}
}
