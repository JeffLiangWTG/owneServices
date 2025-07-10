using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using MailManager;

namespace Enterprise.Client.EDI.Mail.Business.Test
{
	public abstract class EDIMailItemCollectionTestCase : BusinessObjectCollectionTestCase
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
			myItem.MI_Application = ExpectedMailApplicationCode;
			myItem.MI_ReceivedDateTime = Env.Time.CurrentLocalDateTime;
			myItem.MI_SendDateTime = Env.Time.CurrentLocalDateTime;
			myItem.MI_Direction = DirectionList.Codes.Receive;
			myItem.Factory.Save();
			MyCollection.Add(myItem);
			myItem = Factory.New<MailItem>();
			myItem.MI_Application = "STD";
			myItem.MI_ReceivedDateTime = Env.Time.CurrentLocalDateTime;
			myItem.MI_SendDateTime = Env.Time.CurrentLocalDateTime;
			myItem.MI_Direction = DirectionList.Codes.Receive;
			myItem.Factory.Save();
			MyCollection.Add(myItem);
			MyCollection.Load();
			AssertEquals("MyCollection should filter mail Items with 'STD' application", 1, MyCollection.Count);
			AssertEquals("MyCollection should filter mail Items with 'STD' application", ExpectedMailApplicationCode, MyCollection[0].MI_Application);
		}

		public void TestSetMailItem()
		{
			MailItem myItem = Factory.New<MailItem>();
			MailItem myItem2 = Factory.New<MailItem>();
			MyCollection.Add(myItem);
			MyCollection.Add(myItem2);
			AssertEquals("MyCollection[1]", myItem2, MyCollection[1]);
		}

		protected abstract string ExpectedMailApplicationCode { get; }
		protected abstract EDIMailItemCollection GetEDIMailItemCollection(BusinessObjectFactory factory);

		#region Implementation

		protected sealed override BusinessObjectCollection GetCollectionToTest()
		{
			return GetEDIMailItemCollection(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			MyCollection = GetEDIMailItemCollection(Factory);
		}

		EDIMailItemCollection MyCollection;

		#endregion
	}
}
