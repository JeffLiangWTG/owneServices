using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.MFI.Testing
{
	[TestedType(typeof(MFIDataRegistry))]
	internal class MFIDataRegistryTest : RegistryItemSetTestCaseWithFactory<MFIDataRegistry>
	{
		public void TestUserVisibleRegistryItems()
		{
			AssertEquals("Collection count", 11, AllItems.Count);
			AssertVisible(ItemSet.CaroTransAgentsItem);
			AssertVisible(ItemSet.CaroTransNextExportRunItem, true);
			AssertVisible(ItemSet.CaroTransTrackEmailAddressItem);
			AssertVisible(ItemSet.CaroTransHighWaterMarkItem);
			AssertVisible(ItemSet.CaroTransExportFileExtensionItem);
			AssertVisible(ItemSet.InvoiceLetterheadItem);
			AssertVisible(ItemSet.StatementLetterheadItem);
			AssertVisible(ItemSet.AutoeDocAllocationNotificationGroupItem);
			AssertVisible(ItemSet.AutoeDocAllocationSourceDirectoryItem);
			AssertVisible(ItemSet.AutoeDocAllocationHoldDirectoryItem);
			AssertVisible(ItemSet.AutoeDocAllocationHoldPeriodItem);
		}

		#region MFI New Zealand Specific
		public void TestInvoiceLetterhead()
		{
			Image expectedImage = new Bitmap(1, 1);
			ItemSet.InvoiceLetterhead = expectedImage;
			AssertEquals("Invoice letterhead", expectedImage.Size, ItemSet.InvoiceLetterhead.Size);
		}

		public void TestStatementLetterhead()
		{
			Image expectedImage = new Bitmap(2, 2);
			ItemSet.StatementLetterhead = expectedImage;
			AssertEquals("Statement letterhead", expectedImage.Size, ItemSet.StatementLetterhead.Size);
		}

		#endregion
		#region CaroTrans
		public void TestCaroTransAgents()
		{
			AssertEquals("CaroTransAgents", 0, ItemSet.CaroTransAgents.Length);
			Guid agentPK1 = Guid.NewGuid();
			Guid agentPK2 = Guid.NewGuid();
			ItemSet.CaroTransAgents = new Guid[] { agentPK1, agentPK2 };
			AssertEquals("CaroTransAgents", 2, ItemSet.CaroTransAgents.Length);
			AssertEquals("CaroTransAgents 1", agentPK1, ItemSet.CaroTransAgents[0]);
			AssertEquals("CaroTransAgents 2", agentPK2, ItemSet.CaroTransAgents[1]);
		}

		public void TestCaroTransTrackEmailAddress()
		{
			AssertEquals("CaroTransTrackEmailAddress", "", ItemSet.CaroTransTrackEmailAddress);
			ItemSet.CaroTransTrackEmailAddress = "export@test.com";
			AssertEquals("CaroTransTrackEmailAddress", "export@test.com", ItemSet.CaroTransTrackEmailAddress);
		}

		public void TestCaroTransHighWaterMark()
		{
			ZDateTime systemHighWaterMark = (SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.Value == DateTime.MinValue) ? ZDateTime.Empty : new ZDateTime(SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.Value);
			AssertEquals("CaroTransHighWaterMark", systemHighWaterMark, ItemSet.CaroTransHighWaterMark);
			ZDateTime now = ZDateTime.Now;
			ItemSet.CaroTransHighWaterMark = now;
			AssertEquals("CaroTransHighWaterMark", now, ItemSet.CaroTransHighWaterMark);
		}

		public void TestCaroTransExportFileExtension()
		{
			var fileExtensions = new CodeDescriptionPairList();
			fileExtensions.AddPair(Core.Constants.CountryCodes.Australia, "abc");
			AssertEquals("CaroTransExportFileExtension", new ReadOnlyCodeDescriptionPairList(), ItemSet.CaroTransExportFileExtensionItem.DefaultValue);
			ItemSet.CaroTransExportFileExtensionItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fileExtensions);
			var fileExtensionForAU = ItemSet.CaroTransExportFileExtensions.GetDescriptionFromCode(Core.Constants.CountryCodes.Australia);
			AssertEquals("Registry should contain file extension for import country 'abc'", "abc", fileExtensionForAU);
		}

		#endregion
		#region AutoeDocAllocation
		public void TestAutoeDocAllocationNotificationGroup()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbCompany otherCompany = factory.New<GlbCompany>();
			GlbGroup postmasterGroup = factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			AssertEquals("AutoeDocAllocationNotification defaults to PMG Group", postmasterGroup.PK, ItemSet.AutoeDocAllocationNotificationGroup);
			Guid newNotificatonGroup = Guid.NewGuid();
			ItemSet.AutoeDocAllocationNotificationGroup = newNotificatonGroup;
			AssertEquals("AutoeDocAllocationNotification now set to new group", newNotificatonGroup, ItemSet.AutoeDocAllocationNotificationGroup);
		}

		public void TestAutoeDocAllocationSourceDirectory()
		{
			AssertEquals("AutoeDocAllocationSourceDirectory", ZString.Empty, ItemSet.AutoeDocAllocationSourceDirectory);
			ItemSet.AutoeDocAllocationSourceDirectory = "\\Testthisdirectorylocation";
			AssertEquals("AutoeDocAllocationSourceDirectory", "\\Testthisdirectorylocation", ItemSet.AutoeDocAllocationSourceDirectory);
		}

		public void TestAutoeDocAllocationHoldDirectory()
		{
			AssertEquals("AutoeDocAllocationHoldDirectory", ZString.Empty, ItemSet.AutoeDocAllocationHoldDirectory);
			ItemSet.AutoeDocAllocationHoldDirectory = "\\TestHoldDirectoryLocation";
			AssertEquals("AutoeDocAllocationHoldDirectory", "\\TestHoldDirectoryLocation", ItemSet.AutoeDocAllocationHoldDirectory);
		}

		public void TestAutoeDocHoldPeriod()
		{
			AssertEquals("AutoeDocAllocationHoldPeriod", ZInt.Zero, ItemSet.AutoeDocAllocationHoldPeriod);
			ItemSet.AutoeDocAllocationHoldPeriod = 14;
			AssertEquals("AutoeDocAllocationHoldPeriod", 14, ItemSet.AutoeDocAllocationHoldPeriod);
		}
		#endregion
	}
}
