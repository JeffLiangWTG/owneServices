using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(DocumentSendingObjectCollection))]
	class DocumentSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentSendingObjectCollection>
	{
		protected override DocumentSendingObjectCollection GetCollectionToTest() => new DocumentSendingObjectCollection(cusExitReport);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DocumentSendingObject(cusExitReport);

		protected override void SetUp()
		{
			base.SetUp();
			cusExitReport = Factory.New<CusExitHeader>().CusExitReports.AddNew();
		}

		CusExitReport cusExitReport;
	}
}
