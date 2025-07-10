using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC557C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class CC557CProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN", provider.MovementReferenceNumber);
		}

		public void TestBusinessRejectionType()
		{
			AssertEquals("RT1", provider.BusinessRejectionType);
		}

		public void TestRejectionDateAndTime()
		{
			AssertEquals(ZDateTime.BrettsBirthday, provider.RejectionDateAndTime);
		}

		public void TestRejectionCode()
		{
			AssertEquals("C1", provider.RejectionCode);
		}

		public void TestRejectionReason()
		{
			AssertEquals("Reason 1", provider.RejectionReason);
		}

		public void TestFunctionalError()
		{
			CombineAssertions(() =>
			{
				var errors = provider.FunctionalErrors.ToArray();
				var expected = new[]
				{
					("TestReasonItem12","12", "TestPointerItem12"),
					("TestReasonItem13","13", "TestPointerItem13"),
					("TestReasonItem14","14", "TestPointerItem14"),
				};

				for (int i = 0; i < expected.Length; i++)
				{
					AssertEquals("ErrorReason at index:" + i, expected[i].Item1, errors[i].ErrorReason);
					AssertEquals("ErrorCode at index:" + i, expected[i].Item2, errors[i].ErrorCode);
					AssertEquals("ErrorPointer at index:" + i, expected[i].Item3, errors[i].ErrorPointer);
				}
			});
		}

		protected override void SetUp()
		{
			provider = new CC557CProvider(new Cc557C()
			{
				ExportOperation = new ExportOperationType21
				{
					Lrn = "LRN",
					Mrn = "MRN",
					BusinessRejectionType = "RT1",
					RejectionDateAndTime = ZDateTime.BrettsBirthday.ToDateTime(),
					RejectionCode = "C1",
					RejectionReason = "Reason 1"
				},
				FunctionalError = new System.Collections.ObjectModel.Collection<FunctionalErrorType04>
				{
					new FunctionalErrorType04 { ErrorReason = "TestReasonItem12", ErrorCode = AesNctsP5FunctionalErrorCodes.Item12, ErrorPointer = "TestPointerItem12", OriginalAttributeValue = "OriginalAttributeValue" },
					new FunctionalErrorType04 { ErrorReason = "TestReasonItem13", ErrorCode = AesNctsP5FunctionalErrorCodes.Item13, ErrorPointer = "TestPointerItem13", OriginalAttributeValue = "OriginalAttributeValue" },
					new FunctionalErrorType04 { ErrorReason = "TestReasonItem14", ErrorCode = AesNctsP5FunctionalErrorCodes.Item14, ErrorPointer = "TestPointerItem14", OriginalAttributeValue = "OriginalAttributeValue" },
				}
			});
		}
		CC557CProvider provider;
	}
}
