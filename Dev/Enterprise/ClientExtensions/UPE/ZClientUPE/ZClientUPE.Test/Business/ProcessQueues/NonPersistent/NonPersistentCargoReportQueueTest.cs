using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(NonPersistentCargoReportQueue))]
	internal class NonPersistentCargoReportQueueTest : NonPersistentCustomsQueueTestCase
	{
		protected override Type ExpectedNonPersistentProcessQueueLookupsType
		{
			get
			{
				return typeof(NonPersistentCargoReportQueueLookups);
			}
		}

		protected override Type ExpectedUPEProcessQueueValidationHelperType
		{
			get
			{
				return typeof(UPECargoReportQueueValidationHelper);
			}
		}

		public override void TestQueueName()
		{
			var mockQueue = new Mock<NonPersistentCargoReportQueue>(new object[] { Factory });
			var queue = (NonPersistentProcessQueue)mockQueue.Object;
			var mockValidationHelper = new Mock<UPECargoReportQueueValidationHelper>(new object[] { queue });
			mockQueue.Protected()
				.Setup<UPEProcessQueueValidationHelper>("GetNewUPEProcessQueueValidationHelper")
				.Returns(mockValidationHelper.Object);
			mockQueue.CallBase = true;

			FieldInfo fValidationHelper = typeof(NonPersistentProcessQueue).GetField("fValidationHelper", BindingFlags.NonPublic | BindingFlags.Instance);
			fValidationHelper.SetValue(mockQueue.Object, null);

			queue.QueueName = DefaultQueueCodeDescriptionPairList.Codes.EIR;
			AssertEquals(DefaultQueueCodeDescriptionPairList.Codes.EIR, queue.QueueName);
			AssertEquals("Queue", queue.QueueNameCaption);
			AssertEquals(ExpectedQueueNameSchemaColumn.MaxLength, queue.QueueNameInfo.MaxLength);
			mockQueue.VerifyAll();
			mockValidationHelper.VerifyAll();
		}

		public override void TestStatus()
		{
			var mockQueue = new Mock<NonPersistentCargoReportQueue>(new object[] { Factory });
			var queue = (NonPersistentProcessQueue)mockQueue.Object;
			var mockValidationHelper = new Mock<UPECargoReportQueueValidationHelper>(new object[] { queue });
			mockQueue.Protected()
				.Setup<UPEProcessQueueValidationHelper>("GetNewUPEProcessQueueValidationHelper")
				.Returns(mockValidationHelper.Object);
			mockQueue.CallBase = true;
			FieldInfo fValidationHelper = typeof(NonPersistentProcessQueue).GetField("fValidationHelper", BindingFlags.NonPublic | BindingFlags.Instance);
			fValidationHelper.SetValue(mockQueue.Object, null);

			queue.Status = ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice;
			AssertEquals(ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice, queue.Status);
			AssertEquals("Reason", queue.StatusCaption);
			AssertEquals(ExpectedStatusSchemaColumn.MaxLength, queue.StatusInfo.MaxLength);
			mockQueue.VerifyAll();
			mockValidationHelper.VerifyAll();
		}

		public override void TestSubStatus()
		{
			var mockQueue = new Mock<NonPersistentCargoReportQueue>(new object[] { Factory });
			var queue = (NonPersistentProcessQueue)mockQueue.Object;
			var mockValidationHelper = new Mock<UPECargoReportQueueValidationHelper>(new object[] { queue });
			mockQueue.Protected()
				.Setup<UPEProcessQueueValidationHelper>("GetNewUPEProcessQueueValidationHelper")
				.Returns(mockValidationHelper.Object);
			mockQueue.CallBase = true;
			FieldInfo fValidationHelper = typeof(NonPersistentProcessQueue).GetField("fValidationHelper", BindingFlags.NonPublic | BindingFlags.Instance);
			fValidationHelper.SetValue(mockQueue.Object, null);

			queue.SubStatus = StatusCodeDescriptionPairList.Codes.KO_ReceiverContacted;
			AssertEquals(StatusCodeDescriptionPairList.Codes.KO_ReceiverContacted, queue.SubStatus);
			AssertEquals("Status", queue.SubStatusCaption);
			AssertEquals(ExpectedSubStatusSchemaColumn.MaxLength, queue.SubStatusInfo.MaxLength);
			mockQueue.VerifyAll();
			mockValidationHelper.VerifyAll();
		}

		public override void TestReason()
		{
			var mockQueue = new Mock<NonPersistentCargoReportQueue>(new object[] { Factory });
			var queue = (NonPersistentProcessQueue)mockQueue.Object;
			var mockValidationHelper = new Mock<UPECargoReportQueueValidationHelper>(new object[] { queue });
			mockQueue.Protected()
				.Setup<UPEProcessQueueValidationHelper>("GetNewUPEProcessQueueValidationHelper")
				.Returns(mockValidationHelper.Object);
			mockQueue.CallBase = true;
			FieldInfo fValidationHelper = typeof(NonPersistentProcessQueue).GetField("fValidationHelper", BindingFlags.NonPublic | BindingFlags.Instance);
			fValidationHelper.SetValue(mockQueue.Object, null);

			queue.Reason = "lkjasdlfkjw";
			AssertEquals("lkjasdlfkjw", queue.Reason);
			AssertEquals("Remarks", queue.ReasonCaption);
			AssertEquals(ExpectedReasonSchemaColumn.MaxLength, queue.ReasonInfo.MaxLength);
			mockQueue.VerifyAll();
			mockValidationHelper.VerifyAll();
		}

		public override void TestAssignedTo()
		{
			var mockQueue = new Mock<NonPersistentCargoReportQueue>(new object[] { Factory });
			var queue = (NonPersistentProcessQueue)mockQueue.Object;
			var mockValidationHelper = new Mock<UPECargoReportQueueValidationHelper>(new object[] { queue });
			mockQueue.Protected()
				.Setup<UPEProcessQueueValidationHelper>("GetNewUPEProcessQueueValidationHelper")
				.Returns(mockValidationHelper.Object);
			mockQueue.CallBase = true;
			FieldInfo fValidationHelper = typeof(NonPersistentProcessQueue).GetField("fValidationHelper", BindingFlags.NonPublic | BindingFlags.Instance);
			fValidationHelper.SetValue(mockQueue.Object, null);

			queue.AssignedTo = "AAA";
			AssertEquals("AAA", queue.AssignedTo);
			AssertEquals("Assigned To", queue.AssignedToCaption);
			AssertEquals(ExpectedAssignedToSchemaColumn.MaxLength, queue.StatusInfo.MaxLength);
			mockQueue.VerifyAll();
			mockValidationHelper.VerifyAll();
		}

		[TestDate(2006, 5, 5)]
		public override void TestP4_CustomDate4()
		{
			var mockQueue = new Mock<NonPersistentCargoReportQueue>(new object[] { Factory });
			var queue = (NonPersistentProcessQueue)mockQueue.Object;
			FieldInfo fValidationHelper = typeof(NonPersistentProcessQueue).GetField("fValidationHelper", BindingFlags.NonPublic | BindingFlags.Instance);
			fValidationHelper.SetValue(mockQueue.Object, null);

			queue.P4_CustomDate4 = ZDateTime.Now;
			AssertEquals(ZDateTime.Now, queue.P4_CustomDate4);
			mockQueue.VerifyAll();
		}

		public override void TestP4_CustomAttrib8()
		{
			var mockQueue = new Mock<NonPersistentCargoReportQueue>(new object[] { Factory });
			var queue = (NonPersistentProcessQueue)mockQueue.Object;
			FieldInfo fValidationHelper = typeof(NonPersistentProcessQueue).GetField("fValidationHelper", BindingFlags.NonPublic | BindingFlags.Instance);
			fValidationHelper.SetValue(mockQueue.Object, null);

			queue.P4_CustomAttrib8 = "test";
			AssertEquals("test", queue.P4_CustomAttrib8);
			mockQueue.VerifyAll();
		}

		public override void TestLookups()
		{
			var mockQueue = new Mock<NonPersistentCargoReportQueue>(new object[] { Factory });
			mockQueue.CallBase = true;
			var queue = (NonPersistentProcessQueue)mockQueue.Object;

			AssertEquals(ExpectedNonPersistentProcessQueueLookupsType, queue.Lookups.GetType());
			mockQueue.VerifyAll();
		}

		public override void TestValidateQueueNameCalledInTheSetter()
		{
			var mockQueue = new Mock<NonPersistentCargoReportQueue>(new object[] { Factory });
			var queue = (NonPersistentProcessQueue)mockQueue.Object;
			var mockValidationHelper = new Mock<UPECargoReportQueueValidationHelper>(new object[] { queue });
			mockQueue.Protected()
				.Setup<UPEProcessQueueValidationHelper>("GetNewUPEProcessQueueValidationHelper")
				.Returns(mockValidationHelper.Object);
			mockQueue.CallBase = true;
			FieldInfo fValidationHelper = typeof(NonPersistentProcessQueue).GetField("fValidationHelper", BindingFlags.NonPublic | BindingFlags.Instance);
			fValidationHelper.SetValue(mockQueue.Object, null);

			queue.QueueNameInfo.AddError("Blah");
			mockValidationHelper.Setup(m => m.ValidateQueueName());
			queue.QueueName = DefaultQueueCodeDescriptionPairList.Codes.EIR;
			AssertEquals("Notifications should be cleared when the Validate method is called", false, queue.QueueNameInfo.HasNotifications());
			mockQueue.VerifyAll();
			mockValidationHelper.VerifyAll();
		}

		public override void TestValidateStatusCalledInTheSetter()
		{
			var mockQueue = new Mock<NonPersistentCargoReportQueue>(new object[] { Factory });
			var queue = (NonPersistentProcessQueue)mockQueue.Object;
			var mockValidationHelper = new Mock<UPECargoReportQueueValidationHelper>(new object[] { queue });
			mockQueue.Protected()
				.Setup<UPEProcessQueueValidationHelper>("GetNewUPEProcessQueueValidationHelper")
				.Returns(mockValidationHelper.Object);
			mockQueue.CallBase = true;
			FieldInfo fValidationHelper = typeof(NonPersistentProcessQueue).GetField("fValidationHelper", BindingFlags.NonPublic | BindingFlags.Instance);
			fValidationHelper.SetValue(mockQueue.Object, null);

			queue.StatusInfo.AddError("Blah");
			mockValidationHelper.Setup(m => m.ValidateReasonCode());
			queue.Status = ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing;
			AssertEquals("Notifications should be cleared when the Validate method is called", false, queue.StatusInfo.HasNotifications());
			mockQueue.VerifyAll();
			mockValidationHelper.VerifyAll();
		}

		public override void TestValidateSubStatusCalledInTheSetter()
		{
			var mockQueue = new Mock<NonPersistentCargoReportQueue>(new object[] { Factory });
			var queue = (NonPersistentProcessQueue)mockQueue.Object;
			var mockValidationHelper = new Mock<UPECargoReportQueueValidationHelper>(new object[] { queue });
			mockQueue.Protected()
				.Setup<UPEProcessQueueValidationHelper>("GetNewUPEProcessQueueValidationHelper")
				.Returns(mockValidationHelper.Object);
			mockQueue.CallBase = true;
			FieldInfo fValidationHelper = typeof(NonPersistentProcessQueue).GetField("fValidationHelper", BindingFlags.NonPublic | BindingFlags.Instance);
			fValidationHelper.SetValue(mockQueue.Object, null);

			queue.SubStatusInfo.AddError("Blah");
			mockValidationHelper.Setup(m => m.ValidateStatusCode());
			queue.SubStatus = StatusCodeDescriptionPairList.Codes.KO_ReceiverContacted;
			AssertEquals("Notifications should be cleared when the Validate method is called", false, queue.SubStatusInfo.HasNotifications());
			mockQueue.VerifyAll();
			mockValidationHelper.VerifyAll();
		}

		public override void TestValidateAssignedToCalledInTheSetter()
		{
			var mockQueue = new Mock<NonPersistentCargoReportQueue>(new object[] { Factory });
			var queue = (NonPersistentProcessQueue)mockQueue.Object;
			var mockValidationHelper = new Mock<UPECargoReportQueueValidationHelper>(new object[] { queue });
			mockQueue.Protected()
				.Setup<UPEProcessQueueValidationHelper>("GetNewUPEProcessQueueValidationHelper")
				.Returns(mockValidationHelper.Object);
			mockQueue.CallBase = true;
			FieldInfo fValidationHelper = typeof(NonPersistentProcessQueue).GetField("fValidationHelper", BindingFlags.NonPublic | BindingFlags.Instance);
			fValidationHelper.SetValue(mockQueue.Object, null);

			queue.AssignedToInfo.AddError("Blah");
			mockValidationHelper.Setup(m => m.ValidateTaskAssignedTo());
			queue.AssignedTo = "SSS";
			AssertEquals("Notifications should be cleared when the Validate method is called", false, queue.AssignedToInfo.HasNotifications());
			mockQueue.VerifyAll();
			mockValidationHelper.VerifyAll();
		}

		[ExpectNoExceptions]
		public override void TestRunPreSaveValidation()
		{
			var mockQueue = new Mock<NonPersistentCargoReportQueue>(new object[] { Factory });
			var queue = (NonPersistentProcessQueue)mockQueue.Object;
			var mockValidationHelper = new Mock<UPECargoReportQueueValidationHelper>(new object[] { queue });
			mockQueue.Protected()
				.Setup<UPEProcessQueueValidationHelper>("GetNewUPEProcessQueueValidationHelper")
				.Returns(mockValidationHelper.Object);
			mockQueue.CallBase = true;
			FieldInfo fValidationHelper = typeof(NonPersistentProcessQueue).GetField("fValidationHelper", BindingFlags.NonPublic | BindingFlags.Instance);
			fValidationHelper.SetValue(mockQueue.Object, null);

			mockValidationHelper.Setup(m => m.ValidateQueueName());
			mockValidationHelper.Setup(m => m.ValidateReasonCode());
			mockValidationHelper.Setup(m => m.ValidateStatusCode());
			mockValidationHelper.Setup(m => m.ValidateTaskAssignedTo());
			queue.RunPreSaveValidation();
			mockQueue.VerifyAll();
			mockValidationHelper.VerifyAll();
		}

		[ExpectNoExceptions]
		public override void TestValidationsNotCalledWhenSuspended()
		{
			var mockQueue = new Mock<NonPersistentCargoReportQueue>(new object[] { Factory });
			var queue = (NonPersistentProcessQueue)mockQueue.Object;
			var mockValidationHelper = new Mock<UPECargoReportQueueValidationHelper>(new object[] { queue });
			mockQueue.Protected()
				.Setup<UPEProcessQueueValidationHelper>("GetNewUPEProcessQueueValidationHelper")
				.Returns(mockValidationHelper.Object);
			mockQueue.CallBase = true;
			FieldInfo fValidationHelper = typeof(NonPersistentProcessQueue).GetField("fValidationHelper", BindingFlags.NonPublic | BindingFlags.Instance);
			fValidationHelper.SetValue(mockQueue.Object, null);

			mockValidationHelper.Setup(m => m.ValidateQueueName());
			mockValidationHelper.Setup(m => m.ValidateReasonCode());
			mockValidationHelper.Setup(m => m.ValidateStatusCode());
			mockValidationHelper.Setup(m => m.ValidateTaskAssignedTo());
			queue.QueueName = DefaultQueueCodeDescriptionPairList.Codes.EIR;
			queue.Status = ReasonCodeDescriptionPairList.Codes.RU_AlternateBroker;
			queue.SubStatus = StatusCodeDescriptionPairList.Codes.KO_ReceiverContacted;
			queue.Reason = "asdflkj";
			queue.AssignedTo = "BBB";
			mockQueue.VerifyAll();
			mockValidationHelper.VerifyAll();
		}

		public override void TestHasSubStatuses()
		{
			var mockQueue = new Mock<NonPersistentCargoReportQueue>(new object[] { Factory });
			var queue = (NonPersistentProcessQueue)mockQueue.Object;
			var mockValidationHelper = new Mock<UPECargoReportQueueValidationHelper>(new object[] { queue });
			mockQueue.Protected()
				.Setup<UPEProcessQueueValidationHelper>("GetNewUPEProcessQueueValidationHelper")
				.Returns(mockValidationHelper.Object);
			mockQueue.CallBase = true;
			FieldInfo fValidationHelper = typeof(NonPersistentProcessQueue).GetField("fValidationHelper", BindingFlags.NonPublic | BindingFlags.Instance);
			fValidationHelper.SetValue(mockQueue.Object, null);

			queue.QueueName = DefaultQueueCodeDescriptionPairList.Codes.EIR;
			queue.Status = ReasonCodeDescriptionPairList.Codes.RU_AlternateBroker;
			AssertEquals(true, queue.HasSubStatuses);
			queue.QueueName = DefaultQueueCodeDescriptionPairList.Codes.Completed;
			queue.Status = "";
			AssertEquals(false, queue.HasSubStatuses);
			mockQueue.VerifyAll();
			mockValidationHelper.VerifyAll();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NonPersistentCargoReportQueue(Factory);
		}
	}
}
