using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	[TestedType(typeof(ExitControlMessageSendingObjectParent))]
	class ExitControlMessageSendingObjectParentTest : EU.ExitControl.Business.Testing.ExitControlMessageSendingObjectParentTest
	{
		public new void TestSendingObjectsCollectionType()
		{
			AssertType<ExitControlMessageSendingObjectCollection>(exitControlMessageSendingObjectParent.SendingObjectsCollection);
		}

		public void TestParentExitHeader()
		{
			var objectParent = (ExitControlMessageSendingObjectParent)GetNewBusinessObject();
			AssertType<CusExitHeader>(objectParent.ParentExitHeader);
		}

		public void TestGetSendingObjectsCollectionCore()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			var testWrapper1 = new ExitControlMessageSendingObjectParent(new MessageSendingObject(exitHeader, GlbStaff.CurrentUser));
			AssertEquals("PreReq - no object if no ExitReports", 0, testWrapper1.SendingObjectsCollection.Count);

			var exitReport1 = exitHeader.CusExitReports.AddNew();
			var consignment = exitHeader.CusExitConsignments.AddNew();
			consignment.CXC_LocalReference = "Ref1";
			exitReport1.CER_CXC_Consignment = consignment.PK;

			var exitReport2 = exitHeader.CusExitReports.AddNew();
			var consignment2 = exitHeader.CusExitConsignments.AddNew();
			consignment2.CXC_LocalReference = "Ref1";
			exitReport2.CER_CXC_Consignment = consignment2.PK;

			var testWrapper2 = new ExitControlMessageSendingObjectParent(new MessageSendingObject(exitHeader, GlbStaff.CurrentUser));
			AssertEquals(2, testWrapper2.SendingObjectsCollection.Count);
		}

		protected override EU.ExitControl.Business.ExitControlMessageSendingObjectParent GetNewExitControlMessageSendingObjectParent(EU.ExitControl.Business.CusExitHeader exitHeader)
			=> new ExitControlMessageSendingObjectParent(new MessageSendingObject((CusExitHeader)exitHeader, GlbStaff.CurrentUser));

		protected override EU.ExitControl.Business.CusExitHeader GetNewCusExitHeader() => Factory.New<CusExitHeader>();

		protected override BusinessObject GetNewBusinessObject() => exitControlMessageSendingObjectParent;

		protected override void SetUp()
		{
			base.SetUp();
			var cusExitHeader = Factory.New<CusExitHeader>();
			exitControlMessageSendingObjectParent = new ExitControlMessageSendingObjectParent(new MessageSendingObject(cusExitHeader, GlbStaff.CurrentUser));
		}
		ExitControlMessageSendingObjectParent exitControlMessageSendingObjectParent;
	}
}
