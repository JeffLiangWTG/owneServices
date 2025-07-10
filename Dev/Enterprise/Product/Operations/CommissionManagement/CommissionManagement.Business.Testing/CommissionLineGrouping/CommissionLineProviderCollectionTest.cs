using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionLineProviderCollection<ViewCommissionLine>))]
	public class CommissionLineProviderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CommissionLineProviderCollection<ViewCommissionLine>(Factory, new[] { Factory.New<ViewCommissionLine>() });
		}
	}
}
