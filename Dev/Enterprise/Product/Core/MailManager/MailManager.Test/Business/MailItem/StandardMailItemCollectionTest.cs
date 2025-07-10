using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using MailManager;
using NUnit.Framework;

namespace Enterprise.MailManager.Business
{
	[TestedType(typeof(StandardMailItemCollection))]
	sealed class StandardMailItemCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestMailItemCollection()
		{
			AssertNotNull(MyCollection);
		}

		public void TestGetMailItem()
		{
			MailItem myItem = Factory.New<MailItem>();
			MyCollection.Add(myItem);
			AssertEquals("MyCollection[0]", myItem, MyCollection[0]);
		}

		public void TestFilter()
		{
			MyCollection.Load();
			MyCollection.RemoveAndDeleteAll();
			MailItem myItem = Factory.New<MailItem>();
			myItem.MI_Application = "CSV";
			myItem.MI_Direction = DirectionList.Codes.Receive;
			myItem.MI_ReceivedDateTime = Env.Time.CurrentLocalDateTime;
			myItem.MI_SendDateTime = Env.Time.CurrentLocalDateTime;
			myItem.Factory.Save();
			MyCollection.Add(myItem);
			myItem = Factory.New<MailItem>();
			myItem.MI_Application = "STD";
			myItem.MI_Direction = DirectionList.Codes.Receive;
			myItem.MI_ReceivedDateTime = Env.Time.CurrentLocalDateTime;
			myItem.MI_SendDateTime = Env.Time.CurrentLocalDateTime;
			myItem.Factory.Save();
			MyCollection.Add(myItem);
			MyCollection.Load();
			AssertEquals("No more filtering - rejoice!", 2, MyCollection.Count);
			AssertContainsExactElementsInAnyOrder("No more filtering - rejoice!", new ZString[] { "STD", "CSV" }, MyCollection.Select(x => x.MI_Application));
		}

		public void TestSetMailItem()
		{
			MailItem myItem = Factory.New<MailItem>();
			MailItem myItem2 = Factory.New<MailItem>();
			MyCollection.Add(myItem);
			MyCollection.Add(myItem2);
			AssertEquals("MyCollection[1]", myItem2, MyCollection[1]);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StandardMailItemCollection(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			MyCollection = (StandardMailItemCollection)GetCollectionToTest();
		}

		StandardMailItemCollection MyCollection;

		#endregion
	}
}
