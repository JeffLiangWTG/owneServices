using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(DocumentSendingObject))]
	class DocumentSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetValidation()
		{
			AssertType<DocumentSendingObjectValidation>(sendingObject.Validation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var cusExitReport = Factory.New<CusExitHeader>().CusExitReports.AddNew();
			var add1 = cusExitReport.Header.DocManagerInfo.AddFileOrDocument(new byte[1], "AddInfo1.pdf", "CIV");
			sendingObject = new DocumentSendingObject(cusExitReport);
			sendingObject.EDoc = add1.UniqueKey;
		}

		protected override BusinessObject GetNewBusinessObject() => sendingObject;

		DocumentSendingObject sendingObject;
	}
}
