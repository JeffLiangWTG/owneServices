using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class UPECargoReportQueueLookupsTest : UPEProcessQueueLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestCommercialQueueList()
		{
			var mockCommercialQueueLookupsHelper = new Mock<UPECommercialQueueLookupsHelper>(new object[] { Queue });
			var mockLookups = new Mock<UPECargoReportQueueLookups>(new object[] { Queue });
			mockLookups.CallBase = true;
			mockLookups.Protected()
				.Setup<UPECommercialQueueLookupsHelper>("GetNewCommercialQueueLookupsHelper")
				.Returns(mockCommercialQueueLookupsHelper.Object);
			var lookups = mockLookups.Object;

			mockCommercialQueueLookupsHelper.Setup(m => m.GetQueueNameList());
			_ = lookups.CommercialQueueList;
			mockCommercialQueueLookupsHelper.VerifyAll();
			mockCommercialQueueLookupsHelper.Setup(m => m.GetQueueNameList());
			_ = lookups.CommercialQueueList;
			mockCommercialQueueLookupsHelper.VerifyAll();
			mockLookups.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestCommercialStatusList()
		{
			var mockCommercialQueueLookupsHelper = new Mock<UPECommercialQueueLookupsHelper>(new object[] { Queue });
			var mockLookups = new Mock<UPECargoReportQueueLookups>(new object[] { Queue });
			mockLookups.CallBase = true;
			mockLookups.Protected()
				.Setup<UPECommercialQueueLookupsHelper>("GetNewCommercialQueueLookupsHelper")
				.Returns(mockCommercialQueueLookupsHelper.Object);
			var lookups = mockLookups.Object;

			mockCommercialQueueLookupsHelper.Setup(m => m.GetReasonCodeList(It.IsAny<DefaultQueueCodeDescriptionPairList>()));
			_ = lookups.CommercialStatusList;
			mockCommercialQueueLookupsHelper.VerifyAll();
			mockLookups.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestCommercialSubStatusList()
		{
			var mockCommercialQueueLookupsHelper = new Mock<UPECommercialQueueLookupsHelper>(new object[] { Queue });
			var mockLookups = new Mock<UPECargoReportQueueLookups>(new object[] { Queue });
			mockLookups.CallBase = true;
			mockLookups.Protected()
				.Setup<UPECommercialQueueLookupsHelper>("GetNewCommercialQueueLookupsHelper")
				.Returns(mockCommercialQueueLookupsHelper.Object);
			var lookups = mockLookups.Object;

			mockCommercialQueueLookupsHelper.Setup(m => m.GetStatusCodeList());
			_ = lookups.CommercialSubStatusList;
			mockCommercialQueueLookupsHelper.VerifyAll();
			mockLookups.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestCommercialTaskAssignedTos()
		{
			var mockCommercialQueueLookupsHelper = new Mock<UPECommercialQueueLookupsHelper>(new object[] { Queue });
			var mockLookups = new Mock<UPECargoReportQueueLookups>(new object[] { Queue });
			mockLookups.CallBase = true;
			mockLookups.Protected()
				.Setup<UPECommercialQueueLookupsHelper>("GetNewCommercialQueueLookupsHelper")
				.Returns(mockCommercialQueueLookupsHelper.Object);
			var lookups = mockLookups.Object;

			mockCommercialQueueLookupsHelper.Setup(m => m.GetTaskAssignedToList());
			_ = lookups.TaskAssignedTos;
			mockCommercialQueueLookupsHelper.VerifyAll();
			mockLookups.VerifyAll();
		}

		public void TestCommercialQueueLookupsHelper()
		{
			UPECargoReportQueueLookups lookups = (UPECargoReportQueueLookups)GetNewUPEProcessQueueLookups();
			AssertEquals(typeof(UPECommercialQueueLookupsHelper), lookups.CommercialQueueLookupsHelper.GetType());
		}

		[ExpectNoExceptions]
		public override void TestCustomsQueueList()
		{
			var mockCustomsQueueLookupsHelper = new Mock<UPECargoReportQueueLookupsHelper>(new object[] { Queue });
			var mockLookups = new Mock<UPECargoReportQueueLookups>(new object[] { Queue });
			mockLookups.CallBase = true;
			mockLookups.Protected()
				.Setup<UPECustomsQueueLookupsHelper>("GetNewCustomsProcessQueueLookupsHelper")
				.Returns(mockCustomsQueueLookupsHelper.Object);
			var lookups = (UPEProcessQueueLookups)mockLookups.Object;
			mockCustomsQueueLookupsHelper.Setup(m => m.GetQueueNameList());
			_ = lookups.CustomsQueueList;
			mockCustomsQueueLookupsHelper.VerifyAll();
			mockCustomsQueueLookupsHelper.Setup(m => m.GetQueueNameList());
			_ = lookups.CustomsQueueList;
			mockCustomsQueueLookupsHelper.VerifyAll();
			mockCustomsQueueLookupsHelper.VerifyAll();
			mockLookups.VerifyAll();
		}

		[ExpectNoExceptions]
		public override void TestCustomsStatusList()
		{
			var mockCustomsQueueLookupsHelper = new Mock<UPECargoReportQueueLookupsHelper>(new object[] { Queue });
			var mockLookups = new Mock<UPECargoReportQueueLookups>(new object[] { Queue });
			mockLookups.CallBase = true;
			mockLookups.Protected()
				.Setup<UPECustomsQueueLookupsHelper>("GetNewCustomsProcessQueueLookupsHelper")
				.Returns(mockCustomsQueueLookupsHelper.Object);
			var lookups = (UPEProcessQueueLookups)mockLookups.Object;
			mockCustomsQueueLookupsHelper.Setup(m => m.GetReasonCodeList(It.IsAny<DefaultQueueCodeDescriptionPairList>()));
			_ = lookups.CustomsStatusList;
			mockCustomsQueueLookupsHelper.VerifyAll();
			mockLookups.VerifyAll();
		}

		[ExpectNoExceptions]
		public override void TestCustomsSubStatusList()
		{
			var mockCustomsQueueLookupsHelper = new Mock<UPECargoReportQueueLookupsHelper>(new object[] { Queue });
			var mockLookups = new Mock<UPECargoReportQueueLookups>(new object[] { Queue });
			mockLookups.CallBase = true;
			mockLookups.Protected()
				.Setup<UPECustomsQueueLookupsHelper>("GetNewCustomsProcessQueueLookupsHelper")
				.Returns(mockCustomsQueueLookupsHelper.Object);
			var lookups = (UPEProcessQueueLookups)mockLookups.Object;
			mockCustomsQueueLookupsHelper.Setup(m => m.GetStatusCodeList());
			_ = lookups.CustomsSubStatusList;
			mockCustomsQueueLookupsHelper.VerifyAll();
			mockLookups.VerifyAll();
		}

		[ExpectNoExceptions]
		public override void TestCustomsTaskAssignedTos()
		{
			var mockCustomsQueueLookupsHelper = new Mock<UPECargoReportQueueLookupsHelper>(new object[] { Queue });
			var mockLookups = new Mock<UPECargoReportQueueLookups>(new object[] { Queue });
			mockLookups.CallBase = true;
			mockLookups.Protected()
				.Setup<UPECustomsQueueLookupsHelper>("GetNewCustomsProcessQueueLookupsHelper")
				.Returns(mockCustomsQueueLookupsHelper.Object);
			var lookups = (UPEProcessQueueLookups)mockLookups.Object;
			mockCustomsQueueLookupsHelper.Setup(m => m.GetTaskAssignedToList());
			_ = lookups.CustomsTaskAssignedTos;
			mockCustomsQueueLookupsHelper.VerifyAll();
			mockLookups.VerifyAll();
		}

		public override void TestCustomsQueueLookupsHelper()
		{
			UPEProcessQueueLookups lookups = GetNewUPEProcessQueueLookups();
			AssertEquals(typeof(UPECargoReportQueueLookupsHelper), lookups.CustomsQueueLookupsHelper.GetType());
		}

		public override void TestStatusAndSubStatusListNotCached()
		{
			var mockCustomsQueueLookupsHelper = new Mock<UPECargoReportQueueLookupsHelper>(new object[] { Queue });
			mockCustomsQueueLookupsHelper.CallBase = true;
			var mockLookups = new Mock<UPECargoReportQueueLookups>(new object[] { Queue });
			mockLookups.CallBase = true;
			mockLookups.Protected()
				.Setup<UPECustomsQueueLookupsHelper>("GetNewCustomsProcessQueueLookupsHelper")
				.Returns(mockCustomsQueueLookupsHelper.Object);
			var lookups = (UPEProcessQueueLookups)mockLookups.Object;

			CodeDescriptionPairList firstInstance = lookups.CustomsStatusList;
			Assert("Should be a different instance", firstInstance != lookups.CustomsStatusList);
			firstInstance = lookups.CustomsSubStatusList;
			Assert("Should be a different instance", firstInstance != lookups.CustomsSubStatusList);
			firstInstance = lookups.CommercialStatusList;
			Assert("Should be a different instance", firstInstance != lookups.CommercialStatusList);
			firstInstance = lookups.CommercialSubStatusList;
			Assert("Should be a different instance", firstInstance != lookups.CommercialSubStatusList);
			mockLookups.VerifyAll();
			mockCustomsQueueLookupsHelper.VerifyAll();
		}

		#region Implementation
		protected override UPEProcessQueue GetNewUPEProcessQueue()
		{
			return Factory.New<UPECargoReportQueue>();
		}

		protected override UPEProcessQueueLookups GetNewUPEProcessQueueLookups()
		{
			return new UPECargoReportQueueLookups((UPECargoReportQueue)Queue);
		}
		#endregion
	}
}
