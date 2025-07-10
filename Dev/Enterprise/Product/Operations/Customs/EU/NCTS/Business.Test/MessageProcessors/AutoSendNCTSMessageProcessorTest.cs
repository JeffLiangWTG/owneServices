using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Data.Mutex;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(testsSubClassesOf: typeof(AutoSendNCTSMessageProcessor), excludedFromTestAttributeType: null, excludedTypesAndTheirDescendants: new [] { typeof(AutoSendNCTSP5MessageProcessor) })]
	public abstract class AutoSendNCTSMessageProcessorTest<T> : TestCaseWithFactory
		where T : AutoSendNCTSMessageProcessor
	{
		[TestDate(2019, 1, 1, 12, 0, 0)]
		[TestUtcOffset(1, 0, 0)]
		public void TestEndToEndTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			PrepareNctsHeader(nctsHeader);

			IProcessor processor = CreateProcessor(nctsHeader);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);

			var informationNotifications = notifications.GetEventsByType(CargoWise.EntityFramework.NotificationType.Information);
			AssertEquals(1, informationNotifications.Length);
			AssertContains(@"message has been sent to customs for Job", informationNotifications[0].Message);

			var newFactory = new BusinessObjectFactory();
			var loadedHeader = newFactory.Load<NctsHeader>(nctsHeader.PK);
			AssertEntryAndMessageResultForEndToEndTest(nctsHeader);
		}

		public void TestNctsHeaderHasBeenAcceptedByCustoms()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			PrepareNctsHeader(nctsHeader);
			AssertEquals(0, nctsHeader.Messages.Count);
			Factory.Save();

			var processor = CreateProcessor(nctsHeader);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);
			AssertContains("has been sent to customs", notifications.AsString);

			var newFactory = new BusinessObjectFactory();
			var loadedHeader = newFactory.Load<NctsHeader>(nctsHeader.PK);
			AssertEquals(1, loadedHeader.Messages.Count);
		}

		public void TestReportNctsHeaderErrors()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_JobReference = "NCTS12345";
			PrepareNctsHeader(nctsHeader);
			nctsHeader.LocalReferenceNumber = "我";
			Assert(nctsHeader.LocalReferenceNumberInfo.HasErrors());

			var processor = CreateProcessor(nctsHeader);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);
			AssertContains(ExpectedNctsHeaderErrors, notifications.AsString);

			nctsHeader.LocalReferenceNumber = "NCTS12345";
			Factory.Save();
			processor = CreateProcessor(nctsHeader);
			notifications = new NotificationBuffer();
			processor.Process(notifications);
			AssertNotContains(ExpectedNctsHeaderErrors, notifications.AsString);
		}

		protected abstract ZString ExpectedNctsHeaderErrors { get; }

		public void TestLockNctsHeaderWhenSendingMessage()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			PrepareNctsHeader(nctsHeader);

			var nctsHeaderMutex = new ZGlobalMutex(Enterprise.ZArchitecture.Modules.MutexIDs.SendCustomsMessage, nctsHeader.PK.ToString());
			AssertEquals(true, nctsHeaderMutex.Lock());

			Factory.Save();
			var processor = CreateProcessor(nctsHeader);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);
			AssertContains("as CargoWise Support", notifications.AsString);
			AssertContains("is trying to send the same message for this entry. Please wait until the lock has been released before trying to send the message again.", notifications.AsString);

			nctsHeaderMutex.Unlock();
			processor = CreateProcessor(nctsHeader);
			notifications = new NotificationBuffer();
			processor.Process(notifications);
			AssertContains("has been sent to customs", notifications.AsString);
		}

		public void TestMessageDescription()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			PrepareNctsHeader(nctsHeader);
			var processor = CreateProcessor(nctsHeader);
			AssertEquals(ExpectedMessageDescription, (ZString)processor.GetType().GetProperty("MessageDescription", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(processor));
		}

		protected abstract ZString ExpectedMessageDescription { get; }

		protected abstract IProcessor CreateProcessor(NctsHeader nctsHeader);
		protected virtual void PrepareNctsHeader(NctsHeader nctsHeader)
		{
		}
		protected abstract void AssertEntryAndMessageResultForEndToEndTest(NctsHeader nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
		}
	}
}
