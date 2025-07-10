using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MailManager.Business.Testing
{
	sealed class MailStatusAssignerTest : TestCaseWithFactory
	{
		public void TestConcurrencyException()
		{
			var item1 = GetNewMailItem();
			item1.RunPreSaveValidation();
			Assert("Save without Error", !item1.HasNotifications());

			var item2 = GetNewMailItem();
			item2.RunPreSaveValidation();
			Assert("Save without Error", !item2.HasNotifications());

			var item3 = GetNewMailItem();
			item3.MI_Status = MailStatus.QueuedWithAck;
			item3.RunPreSaveValidation();
			Assert("Save without Error", !item3.HasNotifications());

			Factory.Save();

			var assigner = new MailStatusAssigner(new BusinessObject[] { item1, item2, item3 }, MailStatus.Queued);

			//emulate DB values being changed mid Assign
			Factory.RefreshEnabled = false;
			assigner.SavingFactory.RefreshEnabled = false;
			assigner.SavingFactory.Load(typeof(MailItem), new ZQuery(MailDBItemsSchema.PK, new ZGuid[] { item1.PK, item2.PK, item3.PK }));
			item1.MI_Status = MailStatus.Failed;
			item3.MI_Status = MailStatus.Processed;
			Factory.Save();

			assigner.Assign();

			AssertEquals("Should assign with no errors", "", assigner.Errors);
			AssertEquals("Should not return as changed in DB", false, assigner.HasDBChanged);
			AssertEquals("Should affect two Items", 3, assigner.ItemsAffected);

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var item1InFactory2 = factory2.Load<MailItem>(item1.PK);
			item1InFactory2.Delete();
			factory2.Save();

			assigner.Assign();
		}

		public void TestSuccessfullyAssignQueued()
		{
			MailItem item1 = GetNewMailItem();
			item1.RunPreSaveValidation();
			Assert("Save without Error", !item1.HasNotifications());

			MailItem item2 = GetNewMailItem();
			item2.RunPreSaveValidation();
			Assert("Save without Error", !item2.HasNotifications());

			MailItem item3 = GetNewMailItem();
			item3.MI_Status = MailStatus.QueuedWithAck;
			item3.RunPreSaveValidation();
			Assert("Save without Error", !item3.HasNotifications());

			Factory.Save();

			MailStatusAssigner assigner = new MailStatusAssigner(new BusinessObject[] { item1, item2, item3 }, MailStatus.Queued);
			assigner.Assign();

			AssertEquals("Should assign with no errors", "", assigner.Errors);
			AssertEquals("Should not return as changed in DB", false, assigner.HasDBChanged);
			AssertEquals("Should affect two Items", 2, assigner.ItemsAffected);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			item1 = newFactory.Load<MailItem>(item1.PK);
			AssertEquals("Reset Status", MailStatus.Queued, item1.MI_Status);

			item2 = newFactory.Load<MailItem>(item2.PK);
			AssertEquals("Reset Status", MailStatus.Queued, item2.MI_Status);

			item3 = newFactory.Load<MailItem>(item3.PK);
			AssertEquals("Keep QueuedWithAck Status unchanged", MailStatus.QueuedWithAck, item3.MI_Status);
		}

		public void TestAssignWitNoItemsAffected()
		{
			MailItem item1 = GetNewMailItem();
			item1.MI_Status = MailStatus.Queued;
			item1.RunPreSaveValidation();
			Assert("Save without Error", !item1.HasNotifications());

			MailItem item2 = GetNewMailItem();
			item2.MI_Status = MailStatus.QueuedWithAck;
			item2.RunPreSaveValidation();
			Assert("Save without Error", !item2.HasNotifications());

			Factory.Save();

			MailStatusAssigner assigner = new MailStatusAssigner(new BusinessObject[] { item1, item2 }, MailStatus.Queued);
			assigner.Assign();

			AssertEquals("Should assign with no errors", "", assigner.Errors);
			AssertEquals("Should not return as changed in DB", false, assigner.HasDBChanged);
			AssertEquals("Should affect no Items", 0, assigner.ItemsAffected);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			item1 = newFactory.Load<MailItem>(item1.PK);
			AssertEquals("Keep Status unchanged", MailStatus.Queued, item1.MI_Status);

			item2 = newFactory.Load<MailItem>(item2.PK);
			AssertEquals("Keep QueuedWithAck Status unchanged", MailStatus.QueuedWithAck, item2.MI_Status);
		}

		public void TestAssignWithChangedInDB()
		{
			MailItem item1 = GetNewMailItem();
			item1.RunPreSaveValidation();
			Assert("Save without Error", !item1.HasNotifications());

			MailItem item2 = GetNewMailItem();
			item2.RunPreSaveValidation();
			Assert("Save without Error", !item2.HasNotifications());

			Factory.Save();

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			BusinessObject otherItem2 = otherFactory.Load(typeof(MailItem), item2.PK);
			otherItem2.Delete();
			otherFactory.Save();

			MailStatusAssigner assigner = new MailStatusAssigner(new BusinessObject[] { item1, item2 }, MailStatus.Queued);
			assigner.Assign();

			AssertEquals("Should return no errors", "", assigner.Errors);
			AssertEquals("Should return as changed in DB", true, assigner.HasDBChanged);
			AssertEquals("Should affect no Items", 0, assigner.ItemsAffected);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			item1 = newFactory.Load<MailItem>(item1.PK);
			AssertEquals("Keep Status unchanged", MailStatus.Sent, item1.MI_Status);
		}

		[StressTest]
		public void TestAssigningStatusDoesNotFailOn2000Items()
		{
			int numberOfItems = 2000;
			MailItem[] manyItems = new MailItem[numberOfItems];
			for (int i = 0; i < manyItems.Length; i++)
			{
				manyItems[i] = GetNewMailItem();
			}
			Factory.Save();

			MailStatusAssigner assigner = new MailStatusAssigner(manyItems, MailStatus.Queued);

			try
			{
				assigner.Assign();
			}
			catch (Exception e)
			{
				Fail("Schould be no exception but " + e.Message);
			}

			AssertEquals("Should return no errors", "", assigner.Errors);
			AssertEquals("Should return as changed in DB", false, assigner.HasDBChanged);
			AssertEquals("Should affect no Items", numberOfItems, assigner.ItemsAffected);
		}

		#region Implementation

		MailItem GetNewMailItem()
		{
			MailItem item = Factory.New<MailItem>();
			item.MI_Direction = MailDirection.Transmit;
			item.MI_Status = MailStatus.Sent;
			item.MI_SendDateTime = ZDateTime.UtcNow;
			item.MI_ReceivedDateTime = ZDateTime.UtcNow;
			item.MI_From = "sender@domain.com";
			return item;
		}

		#endregion
	}
}
