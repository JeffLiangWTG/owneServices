using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(DocumentsSendingActionParent))]
	class DocumentsSendingActionParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessageType()
		{
			AssertEquals(AESOutgoingMessageTypeList.Codes.DocumentUpload, documentsSendingActionParent.MessageType);
		}

		public void TestCusExitReport()
		{
			AssertEquals(exitReport, documentsSendingActionParent.CusExitReport);
		}

		public void TestTopLevelBusinessObject()
		{
			AssertEquals(exitReport.Header, documentsSendingActionParent.TopLevelBusinessObject);
		}

		public void TestSendingObjectCollection()
		{
			var testSendingParent = (DocumentsSendingActionParent)GetNewBusinessObject();
			AssertType("Should defined correct Type of SendingObjectCollection.", ExpectedSendingObjectCollectionType, testSendingParent.SendingObjectsCollection);
		}

		protected Type ExpectedSendingObjectCollectionType => typeof(DocumentsSendingActionCollection);

		protected override BusinessObject GetNewBusinessObject() => documentsSendingActionParent;

		protected override void SetUp()
		{
			base.SetUp();
			exitReport = Factory.New<CusExitHeader>().CusExitReports.AddNew();
			documentsSendingActionParent = new DocumentsSendingActionParent(exitReport, AESOutgoingMessageTypeList.Codes.DocumentUpload);
		}

		CusExitReport exitReport;
		DocumentsSendingActionParent documentsSendingActionParent;
	}
}
