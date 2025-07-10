using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class UPEDeclarationQueueLookupsTest : UPEProcessQueueLookupsTestCase
	{
		[ExpectNoExceptions]
		public override void TestCustomsQueueList()
		{
			var mockCustomsQueueLookupsHelper = new Mock<UPEDeclarationQueueLookupsHelper>(new object[] { Queue });
			var mockLookups = new Mock<UPEDeclarationQueueLookups>(new object[] { Queue });
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
			mockLookups.VerifyAll();
		}

		[ExpectNoExceptions]
		public override void TestCustomsStatusList()
		{
			var mockCustomsQueueLookupsHelper = new Mock<UPEDeclarationQueueLookupsHelper>(new object[] { Queue });
			var mockLookups = new Mock<UPEDeclarationQueueLookups>(new object[] { Queue });
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
			var mockCustomsQueueLookupsHelper = new Mock<UPEDeclarationQueueLookupsHelper>(new object[] { Queue });
			var mockLookups = new Mock<UPEDeclarationQueueLookups>(new object[] { Queue });
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
			var mockCustomsQueueLookupsHelper = new Mock<UPEDeclarationQueueLookupsHelper>(new object[] { Queue });
			var mockLookups = new Mock<UPEDeclarationQueueLookups>(new object[] { Queue });
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
			AssertEquals(typeof(UPEDeclarationQueueLookupsHelper), lookups.CustomsQueueLookupsHelper.GetType());
		}

		public override void TestStatusAndSubStatusListNotCached()
		{
			var mockCustomsQueueLookupsHelper = new Mock<UPEDeclarationQueueLookupsHelper>(new object[] { Queue });
			mockCustomsQueueLookupsHelper.CallBase = true;
			var mockLookups = new Mock<UPEDeclarationQueueLookups>(new object[] { Queue });
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

		protected override UPEProcessQueue GetNewUPEProcessQueue()
		{
			return Factory.New<UPEDeclarationQueue>();
		}

		protected override UPEProcessQueueLookups GetNewUPEProcessQueueLookups()
		{
			return new UPEDeclarationQueueLookups((UPEDeclarationQueue)Queue);
		}
	}
}
