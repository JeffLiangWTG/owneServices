using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocCASSBillingLineCollection))]
	public class DocCASSBillingLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocCASSBillingLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CASSBillingLine cASSBillingLine = new CASSBillingLine(Factory);
			return DocCASSBillingLine.New(cASSBillingLine, Factory);
		}

		protected override DocCASSBillingLineCollection GetCollectionToTest()
		{
			return new DocCASSBillingLineCollection(Factory);
		}
	}
}
