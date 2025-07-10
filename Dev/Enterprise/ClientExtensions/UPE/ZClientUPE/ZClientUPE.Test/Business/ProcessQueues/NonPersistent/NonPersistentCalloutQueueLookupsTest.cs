using System;
using Moq;
using Moq.Protected;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class NonPersistentCalloutQueueLookupsTest : NonPersistentProcessQueueLookupsTestCase
	{
		protected override Type ExpectedLookupsHelperType
		{
			get
			{
				return typeof(UPECommercialQueueLookupsHelper);
			}
		}

		protected override Type NonPersistentProcessQueueLookupsTypeToTest
		{
			get
			{
				return typeof(NonPersistentCalloutQueueLookups);
			}
		}

		protected override NonPersistentProcessQueueLookups GetNewNonPersistentProcessQueueLookups()
		{
			return new NonPersistentCalloutQueueLookups((NonPersistentCalloutQueue)Queue);
		}

		protected override NonPersistentProcessQueue GetNewNonPersistentQueue()
		{
			return new NonPersistentCalloutQueue(Factory);
		}

		public override void TestQueueList()
		{
			var mockLookupsHelper = new Mock<UPECommercialQueueLookupsHelper>(new object[] { Queue });
			var mockLookups = new Mock<NonPersistentCalloutQueueLookups>(new object[] { Queue });
			mockLookups.Protected()
				.Setup<UPEProcessQueueLookupsHelper>("GetNewUPEProcessQueueLookupsHelper")
				.Returns(mockLookupsHelper.Object);
			mockLookupsHelper.Setup(m => m.GetQueueNameList());
			object notUsed = mockLookups.Object.QueueList;
			mockLookupsHelper.VerifyAll();
			mockLookupsHelper.Setup(m => m.GetQueueNameList());
			notUsed = mockLookups.Object.QueueList;
			AssertNoExceptionThrown(() => mockLookupsHelper.VerifyAll());
		}

		public override void TestStatusList()
		{
			var mockLookupsHelper = new Mock<UPECommercialQueueLookupsHelper>(new object[] { Queue });
			var mockLookups = new Mock<NonPersistentCalloutQueueLookups>(new object[] { Queue });
			mockLookups.Protected()
				.Setup<UPEProcessQueueLookupsHelper>("GetNewUPEProcessQueueLookupsHelper")
				.Returns(mockLookupsHelper.Object);
			mockLookupsHelper.Setup(m => m.GetReasonCodeList(It.IsAny<DefaultQueueCodeDescriptionPairList>()));
			object notUsed = mockLookups.Object.StatusList;
			AssertNoExceptionThrown(() => mockLookupsHelper.VerifyAll());
		}

		public override void TestSubStatusList()
		{
			var mockLookupsHelper = new Mock<UPECommercialQueueLookupsHelper>(new object[] { Queue });
			var mockLookups = new Mock<NonPersistentCalloutQueueLookups>(new object[] { Queue });
			mockLookups.Protected()
				.Setup<UPEProcessQueueLookupsHelper>("GetNewUPEProcessQueueLookupsHelper")
				.Returns(mockLookupsHelper.Object);
			mockLookupsHelper.Setup(m => m.GetStatusCodeList());
			object notUsed = mockLookups.Object.SubStatusList;
			AssertNoExceptionThrown(() => mockLookupsHelper.VerifyAll());
		}

		public override void TestTaskAssignedToList()
		{
			var mockLookupsHelper = new Mock<UPECommercialQueueLookupsHelper>(new object[] { Queue });
			var mockLookups = new Mock<NonPersistentCalloutQueueLookups>(new object[] { Queue });
			mockLookups.Protected()
				.Setup<UPEProcessQueueLookupsHelper>("GetNewUPEProcessQueueLookupsHelper")
				.Returns(mockLookupsHelper.Object);
			mockLookupsHelper.Setup(m => m.GetTaskAssignedToList());
			object notUsed = mockLookups.Object.TaskAssignedToList;
			AssertNoExceptionThrown(() => mockLookupsHelper.VerifyAll());
		}
	}
}
