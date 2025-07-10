using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocChargeCodeCollection))]
	public class DocChargeCodeCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocChargeCodeCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			return DocChargeCode.New(chargeCode, Factory);
		}

		protected override DocChargeCodeCollection GetCollectionToTest()
		{
			return new DocChargeCodeCollection(Factory);
		}
	}
}
