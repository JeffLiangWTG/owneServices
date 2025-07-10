using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM416ProviderTest : TestCaseWithFactory
	{
		public void TestAdditionalDeclarationType()
		{
			AssertEquals("A", providerUCC6.AdditionalDeclarationType);
		}

		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN001", providerUCC6.LocalReferenceNumber);
		}

		public void TestRejectionDate()
		{
			AssertEquals(new DateTime(2023, 08, 10, 14, 30, 45), providerUCC6.RejectionDate);
		}

		public void TestRejectionMotivationText()
		{
			AssertEquals("UCC6 Rejection Motivation Text", providerUCC6.RejectionMotivationText);
		}

		public void TestHasFunctionalErrors()
		{
			Assert("ProviderUCC6 has Functional Errors", providerUCC6.HasFunctionalErrors);
		}

		public void TestEntryStatus()
		{
			AssertEquals("REJ", providerUCC6.EntryStatus);

			providerUCC6 = new IM416Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM416.Im416
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType416
				{
					AdditionalDeclarationType = "X",
				},
			});

			AssertEquals("SUP", providerUCC6.EntryStatus);
		}

		protected override void SetUp()
		{
			base.SetUp();
			providerUCC6 = new IM416Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM416.Im416
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType416
				{
					AdditionalDeclarationType = "A",
					Lrn = "LRN001",
					RejectionDate = new DateTime(2023, 08, 10, 14, 30, 45),
					RejectionMotivationText = "UCC6 Rejection Motivation Text",
				},
				FunctionalError = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MFunctionalErrorType01>
				{
					new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MFunctionalErrorType01
					{
						SequenceNumber = "1",
						ErrorPointer = "ErrorPointer001",
						ErrorCode = "13",
						ErrorReason = "ER1",
						Remarks = "Functional Error Remarks 1",
						OriginalAttributeValue = "Original Attribute Value 1",
					}
				}
			});
		}
		IM416Provider providerUCC6;
	}
}
