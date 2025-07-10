using CargoWise.EntityFramework.Testing;
using Moq;
using Moq.Protected;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class UPECalloutValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateP4_QueueName()
		{
			var mockCommercialQueueValidationHelper = new Mock<UPECommercialQueueValidationHelper>(new object[] { Queue });
			mockCommercialQueueValidationHelper.Setup(m => m.ValidateQueueName());
			var mockValidation = new Mock<UPECalloutQueueValidation>(new object[] { Queue });
			mockValidation.CallBase = true;
			mockValidation.Protected()
				.Setup<UPECommercialQueueValidationHelper>("GetNewCommercialQueueValidationHelper")
				.Returns(mockCommercialQueueValidationHelper.Object);
			var validation = mockValidation.Object;
			AssertNoExceptionThrown(() => validation.ValidateP4_QueueName());
			mockCommercialQueueValidationHelper.VerifyAll();
			mockValidation.VerifyAll();
		}

		public void TestValidateP4_Status()
		{
			var mockCommercialQueueValidationHelper = new Mock<UPECommercialQueueValidationHelper>(new object[] { Queue });
			var mockValidation = new Mock<UPECalloutQueueValidation>(new object[] { Queue });
			mockValidation.CallBase = true;
			mockValidation.Protected()
				.Setup<UPECommercialQueueValidationHelper>("GetNewCommercialQueueValidationHelper")
				.Returns(mockCommercialQueueValidationHelper.Object);
			mockCommercialQueueValidationHelper.Setup(m => m.ValidateReasonCode());
			mockCommercialQueueValidationHelper.Setup(m => m.ValidateQueueName());
			var validation = mockValidation.Object;
			AssertNoExceptionThrown(() => validation.ValidateP4_Status());
			mockCommercialQueueValidationHelper.VerifyAll();
			mockValidation.VerifyAll();
		}

		public void TestValidateP4_SubStatus()
		{
			var mockCommercialQueueValidationHelper = new Mock<UPECommercialQueueValidationHelper>(new object[] { Queue });
			var mockValidation = new Mock<UPECalloutQueueValidation>(new object[] { Queue });
			mockValidation.CallBase = true;
			mockValidation.Protected()
				.Setup<UPECommercialQueueValidationHelper>("GetNewCommercialQueueValidationHelper")
				.Returns(mockCommercialQueueValidationHelper.Object);
			mockCommercialQueueValidationHelper.Setup(m => m.ValidateStatusCode());
			var validation = mockValidation.Object;
			AssertNoExceptionThrown(() => validation.ValidateP4_SubStatus());
			mockCommercialQueueValidationHelper.VerifyAll();
			mockValidation.VerifyAll();
		}

		public void TestValidateP4_GS_NKTaskAssignedTo()
		{
			var mockCommercialQueueValidationHelper = new Mock<UPECommercialQueueValidationHelper>(new object[] { Queue });
			var mockValidation = new Mock<UPECalloutQueueValidation>(new object[] { Queue });
			mockValidation.CallBase = true;
			mockValidation.Protected()
				.Setup<UPECommercialQueueValidationHelper>("GetNewCommercialQueueValidationHelper")
				.Returns(mockCommercialQueueValidationHelper.Object);
			mockCommercialQueueValidationHelper.Setup(m => m.ValidateTaskAssignedTo());
			var validation = mockValidation.Object;
			AssertNoExceptionThrown(() => validation.ValidateP4_GS_NKTaskAssignedTo());
			mockCommercialQueueValidationHelper.VerifyAll();
			mockValidation.VerifyAll();
		}

		public void TestCommercialValidationHelperType()
		{
			UPECalloutQueueValidation validation = new UPECalloutQueueValidation(Queue);
			AssertEquals(typeof(UPECommercialQueueValidationHelper), validation.CommercialQueueValidationHelper.GetType());
		}

		#region Implementation
		UPECalloutQueue Queue
		{
			get
			{
				if (fQueue == null)
				{
					fQueue = Factory.New<UPECalloutQueue>();
				}

				return fQueue;
			}
		}

		UPECalloutQueue fQueue;
		#endregion
	}
}
