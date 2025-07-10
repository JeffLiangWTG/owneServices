using System;
using Moq;
using Moq.Protected;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class UPECargoReportQueueValidationTest : UPECustomsProcessQueueValidationTestCase
	{
		protected override Type ExpectedCustomsValidationHelperType
		{
			get
			{
				return typeof(UPECargoReportQueueValidationHelper);
			}
		}

		protected override Type UPECustomsProcessQueueValidationTypeToTest
		{
			get
			{
				return typeof(UPECargoReportQueueValidation);
			}
		}

		protected override UPEProcessQueue GetNewUPEProcessQueue()
		{
			return Factory.New<UPECargoReportQueue>();
		}

		protected override UPECustomsProcessQueueValidation GetNewValidation()
		{
			return new UPECargoReportQueueValidation((UPECargoReportQueue)Queue);
		}

		public override void TestValidateP4_CustomsQueue()
		{
			var mockCustomsValidationHelper = new Mock<UPECustomsQueueValidationHelper>(new object[] { Queue });
			var mockValidation = new Mock<UPECargoReportQueueValidation>(new object[] { Queue });
			mockValidation.CallBase = true;
			mockValidation.Protected()
				.Setup<UPECustomsQueueValidationHelper>("GetNewCustomsQueueValidationHelper")
				.Returns(mockCustomsValidationHelper.Object);
			var validation = (UPECustomsProcessQueueValidation)mockValidation.Object;

			mockCustomsValidationHelper.Setup(m => m.ValidateQueueName());
			AssertNoExceptionThrown(() => validation.ValidateP4_CustomsQueue());
			mockCustomsValidationHelper.VerifyAll();
		}

		public override void TestValidateP4_CustomsStatus()
		{
			var mockCustomsValidationHelper = new Mock<UPECustomsQueueValidationHelper>(new object[] { Queue });
			var mockValidation = new Mock<UPECargoReportQueueValidation>(new object[] { Queue });
			mockValidation.CallBase = true;
			mockValidation.Protected()
				.Setup<UPECustomsQueueValidationHelper>("GetNewCustomsQueueValidationHelper")
				.Returns(mockCustomsValidationHelper.Object);
			var validation = (UPECustomsProcessQueueValidation)mockValidation.Object;

			mockCustomsValidationHelper.Setup(m => m.ValidateReasonCode());
			AssertNoExceptionThrown(() => validation.ValidateP4_CustomsStatus());
			mockCustomsValidationHelper.VerifyAll();
		}

		public override void TestValidateP4_CustomsSubStatus()
		{
			var mockCustomsValidationHelper = new Mock<UPECustomsQueueValidationHelper>(new object[] { Queue });
			var mockValidation = new Mock<UPECargoReportQueueValidation>(new object[] { Queue });
			mockValidation.CallBase = true;
			mockValidation.Protected()
				.Setup<UPECustomsQueueValidationHelper>("GetNewCustomsQueueValidationHelper")
				.Returns(mockCustomsValidationHelper.Object);
			var validation = (UPECustomsProcessQueueValidation)mockValidation.Object;

			mockCustomsValidationHelper.Setup(m => m.ValidateStatusCode());
			AssertNoExceptionThrown(() => validation.ValidateP4_CustomsSubStatus());
			mockCustomsValidationHelper.VerifyAll();
		}

		public override void TestValidateP4_GS_NKCustomsTaskAssignedTo()
		{
			var mockCustomsValidationHelper = new Mock<UPECustomsQueueValidationHelper>(new object[] { Queue });
			var mockValidation = new Mock<UPECargoReportQueueValidation>(new object[] { Queue });
			mockValidation.CallBase = true;
			mockValidation.Protected()
				.Setup<UPECustomsQueueValidationHelper>("GetNewCustomsQueueValidationHelper")
				.Returns(mockCustomsValidationHelper.Object);
			var validation = (UPECustomsProcessQueueValidation)mockValidation.Object;

			mockCustomsValidationHelper.Setup(m => m.ValidateTaskAssignedTo());
			AssertNoExceptionThrown(() => validation.ValidateP4_GS_NKCustomsTaskAssignedTo());
			mockCustomsValidationHelper.VerifyAll();
		}
	}
}
