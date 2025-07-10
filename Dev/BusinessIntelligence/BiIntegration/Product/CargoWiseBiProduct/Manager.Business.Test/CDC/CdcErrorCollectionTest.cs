using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business.Testing
{
	[TestedType(typeof(CdcErrorCollection))]
	class CdcErrorCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CdcErrorCollection>
	{
		protected override CdcErrorCollection GetCollectionToTest()
		{
			return new CdcErrorCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CdcError();
		}
	}
}
