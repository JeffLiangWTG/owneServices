using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business.Testing
{
	[TestedType(typeof(CdcError))]
	class CdcErrorTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CdcError();
		}

		public void TestCdcError()
		{
			var cdcError = new CdcError();
			AssertNotNull(cdcError);
		}
	}
}
