using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	public abstract class DocBaseBillOfLadingTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateBillOfLading();
		}

		protected abstract DocBaseBillOfLading CreateBillOfLading();
	}
}
