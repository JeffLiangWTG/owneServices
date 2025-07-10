using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(ECSExitHeaderMessageSendingObjectParent))]
	public class ECSExitHeaderMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_Parent = declaration;
			return new ECSExitHeaderMessageSendingObjectParent(new ECSMessageSendingObject(exitHeader, GlbStaff.CurrentUser));
		}

		public void TestParentDeclaration()
		{
			var objectParent = (ECSExitHeaderMessageSendingObjectParent)GetNewBusinessObject();
			AssertType<CusExitControlHeader>(objectParent.ParentExitHeader);
		}

		public void TestGetSendingObjectsCollectionCore()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader1 = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader1.CEH_Parent = declaration1;
			var testWrapper1 = new ECSExitHeaderMessageSendingObjectParent(new ECSMessageSendingObject(exitHeader1, GlbStaff.CurrentUser));
			AssertEquals(0, testWrapper1.SendingObjectsCollection.Count);

			var exitDetail1 = exitHeader1.CusExitDetails.AddNew();
			exitDetail1.CED_Status = "XXX";
			var exitDetail2 = exitHeader1.CusExitDetails.AddNew();
			exitDetail2.CED_Status = "XXX";
			AssertEquals(0, testWrapper1.SendingObjectsCollection.Count);

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader2 = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader2.CEH_Parent = declaration2;
			var exitDetail3 = exitHeader2.CusExitDetails.AddNew();
			exitDetail3.CED_Status = "XXX";
			var exitDetail4 = exitHeader2.CusExitDetails.AddNew();
			exitDetail4.CED_Status = "XXX";
			var testWrapper2 = new ECSExitHeaderMessageSendingObjectParent(new ECSMessageSendingObject(exitHeader2, GlbStaff.CurrentUser));
			AssertEquals(2, testWrapper2.SendingObjectsCollection.Count);
		}

		public void TestMessageSendingObjectProperties()
		{
			var parent = (ECSExitHeaderMessageSendingObjectParent)GetNewBusinessObject();
			var properties = parent.MessageSendingObjectProperties;
			CombineAssertions(() =>
			{
				AssertEquals("MRN", properties.ElementAt(0).PropertyName);
				AssertEquals("ArrivalNotificationDate", properties.ElementAt(1).PropertyName);
				AssertEquals("Status", properties.ElementAt(2).PropertyName);

				AssertEquals("MRN", 140, properties.ElementAt(0).ColumnWidth);
				AssertEquals("Arrival Notification Date", 140, properties.ElementAt(1).ColumnWidth);
				AssertEquals("Status", 100, properties.ElementAt(2).ColumnWidth);

				Assert(properties.All(x => x.IsMandatory));
			});
		}
	}

	public class ECSExitHeaderMessageSendingObjectParentBaseOnlyTest : TestCaseWithFactory
	{
		public void TestBizObjValidationMessageErrors()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
			exitHeader.CEH_Parent = declaration;
			var testItem = new ECSExitHeaderMessageSendingObjectParent(new ECSMessageSendingObject(exitHeader, GlbStaff.CurrentUser));

			var exitDetail1 = exitHeader.CusExitDetails.AddNew();
			exitDetail1.CED_Status = "XXX";
			exitDetail1.CED_CustomsOffice = "ES009999";
			var exitDetail2 = exitHeader.CusExitDetails.AddNew();
			exitDetail2.CED_Status = "XXX";
			exitDetail2.CED_CustomsOffice = "ES009998";

			AssertEquals("", testItem.BizObjValidationMessageErrors);

			testItem.SendingObjectsCollection.Cast<ECSExitHeaderMessageSendingObject>().First().ShouldSend = true;
			Assert(testItem.BizObjValidationMessageErrors.StartsWith("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:\r\n"));
		}
	}
}
