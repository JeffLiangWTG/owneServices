using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.CLE.Testing
{
	[TestedType(typeof(CLEDataRegistry))]
	public class CLEDataRegistryTest : RegistryItemSetTestCase<CLEDataRegistry>
	{
		#region TestUserVisibleRegistryItems
		public void TestUserVisibleRegistryItems()
		{
			AssertEquals("Count", 10, AllItems.Count);
			AssertVisible(ItemSet.MattelDebtorRaw);
			AssertVisible(ItemSet.ARInvoiceExportHighWaterMarkRaw, true);
			AssertVisible(ItemSet.MattelEmailAddressRaw);
			AssertVisible(ItemSet.MattelEmailSubjectRaw);
			AssertVisible(ItemSet.MattelMailboxNumberRaw);
			AssertVisible(ItemSet.ClemengerEmailAddressRaw);
			AssertVisible(ItemSet.ContainerUploadEmailNotificationGroup);
			AssertVisible(ItemSet.SwitchOrderImportItem);
			AssertVisible(ItemSet.MaximumOrdersToDeliverRaw);
		}

		#endregion
		public void TestContainerUploadEmailNotificationGroup()
		{
			AssertEquals("Default Value", Guid.Empty, ItemSet.ContainerUploadEmailNotificationGroup.DefaultValue);
			Guid testValue = Guid.NewGuid();
			ItemSet.ContainerUploadEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testValue);
			AssertEquals("Assigned Value", testValue, ItemSet.ContainerUploadEmailNotificationGroup.Value);
		}

		#region TestMattelDebtor
		public void TestMattelDebtor()
		{
			AssertEquals("Mattel Debtor", Guid.Empty, ItemSet.MattelDebtor);
			Guid debtor = Guid.NewGuid();
			ItemSet.MattelDebtor = debtor;
			AssertEquals("Debtor", debtor, ItemSet.MattelDebtor);
		}

		#endregion
		#region TestMattelEmailAddress
		public void TestMattelEmailAddress()
		{
			AssertEquals("MattelEmailAddress", "", ItemSet.MattelEmailAddress);
			string emailAddress = "Test@Test.com";
			ItemSet.MattelEmailAddress = emailAddress;
			AssertEquals("MattelEmailAddress", emailAddress, ItemSet.MattelEmailAddress);
		}

		#endregion
		#region TestMattelEmailAddress
		public void TestClemengerEmailAddress()
		{
			AssertEquals("ClemengerEmailAddress", "", ItemSet.ClemengerEmailAddress);
			string emailAddress = "Test@Test.com";
			ItemSet.ClemengerEmailAddress = emailAddress;
			AssertEquals("ClemengerEmailAddress", emailAddress, ItemSet.ClemengerEmailAddress);
		}

		#endregion
		#region TestMattelEmailSubject
		public void TestMattelSubject()
		{
			AssertEquals("MattelEmailSubject", "", ItemSet.MattelEmailSubject);
			string emailSubject = "Email Subject";
			ItemSet.MattelEmailSubject = emailSubject;
			AssertEquals("MattelEmailSubject", emailSubject, ItemSet.MattelEmailSubject);
		}

		#endregion
		#region TestMattelMailboxNumber
		public void TestMattelMailboxNumber()
		{
			AssertEquals("MattelMailboxNumber", "", ItemSet.MattelMailboxNumber);
			string mailboxNumber = "123123";
			ItemSet.MattelMailboxNumber = mailboxNumber;
			AssertEquals("MattelEmailSubject", mailboxNumber, ItemSet.MattelMailboxNumber);
		}

		#endregion
		#region TestClemengerMailboxNumber
		public void TestClemengerMailboxNumber()
		{
			AssertEquals("ClemengerMailboxNumber", "", ItemSet.ClemengerMailboxNumber);
			string mailboxNumber = "123123";
			ItemSet.ClemengerMailboxNumber = mailboxNumber;
			AssertEquals("MattelEmailSubject", mailboxNumber, ItemSet.ClemengerMailboxNumber);
		}

		#endregion
		#region TestMattelHighWaterMark
		public void TestMattelHighWaterMark()
		{
			ZDateTime systemHighWaterMark = (SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.Value == DateTime.MinValue) ? ZDateTime.Empty : new ZDateTime(SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.Value);
			AssertEquals("MattelHighWaterMark", systemHighWaterMark, ItemSet.ARInvoiceExportHighWaterMark);
			ZDateTime now = ZDateTime.Now;
			ItemSet.ARInvoiceExportHighWaterMark = now;
			AssertEquals("SpeedoHighWaterMark", now, ItemSet.ARInvoiceExportHighWaterMark);
		}

		#endregion
		#region Order Import Interface
		public void TestSwitchOrderImportItemProperties()
		{
			AssertEquals("Order Import Settings", ItemSet.SwitchOrderImportItem.Caption);
			AssertEquals("", ItemSet.SwitchOrderImportItem.Hint);
			AssertEquals(RegistryStorageFlags.Company, ItemSet.SwitchOrderImportItem.Storage);
			AssertEquals("CLE Client Specific/Orders Import", ItemSet.SwitchOrderImportItem.Category);
			AssertEquals("SwitchOrderImportItem", ItemSet.SwitchOrderImportItem.Name);
			DataTransferSwitchRegistryBusinessObject u = new DataTransferSwitchRegistryBusinessObject();
			u.Directory = Env.TempPath;
			u.NextRunDateTime = ZDateTime.BrettsBirthday;
			BusinessObjectFactory fac = new BusinessObjectFactory();
			GlbGroup g = fac.NewWithValidTestData<GlbGroup>();
			g.GG_Code = "TMP";
			var staff = g.Staff.AddNew();
			staff.GS_EmailAddress = "bbb@ccc.com";
			staff.GS_Code = "ZAC";
			fac.Save();
			u.GroupPK = g.PK;
			u.Interval = 5;
			u.EnableInterface = true;
			ItemSet.SwitchOrderImportItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, u);
			AssertEquals(Env.TempPath, ItemSet.OrderImportDirectory);
			Assert(ItemSet.EnableOrdersDataImportInterface);
			AssertEquals(g.PK, ItemSet.OrderImportNotifyGroupPK);
		}

		#endregion
		#region TestMaximumOrdersToDeliver
		public void TestMaximumOrdersToDeliver()
		{
			AssertEquals("MaximumOrdersToDeliver", 100, ItemSet.MaximumOrdersToDeliver);
			ItemSet.MaximumOrdersToDeliver = 500;
			AssertEquals("MattelDataExportFrequency", 500, ItemSet.MaximumOrdersToDeliver);
		}
		#endregion
	}
}
