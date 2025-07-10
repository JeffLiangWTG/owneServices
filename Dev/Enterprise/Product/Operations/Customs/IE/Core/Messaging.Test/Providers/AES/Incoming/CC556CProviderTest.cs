using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC556C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class CC556CProviderTest : TestCaseWithFactory
	{
		public void TestExportOperation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("LRN", provider.LocalReferenceNumber);
				AssertEquals("MRN", provider.MovementReferenceNumber);
				AssertEquals("TestType", provider.BusinessRejectionType);
				AssertEquals(new ZDateTime(1996, 12, 11, 16, 0, 0), provider.RejectionDateAndTime);
				AssertEquals("TestCode", provider.RejectionCode);
				AssertEquals("Test Reason", provider.RejectionReason);
			});
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
			provider = new CC556CProvider(new Cc556C
			{
				ExportOperation = new ExportOperationType20
				{
					Lrn = "LRN",
					Mrn = "MRN",
					BusinessRejectionType = "TestType",
					RejectionDateAndTime = new DateTime(1996, 12, 11, 16, 0, 0),
					RejectionCode = "TestCode",
					RejectionReason = "Test Reason"
				},
				FunctionalError = new System.Collections.ObjectModel.Collection<FunctionalErrorType04>
				{
					new FunctionalErrorType04 { ErrorReason = "TestReasonItem12", ErrorCode = AesNctsP5FunctionalErrorCodes.Item12, ErrorPointer = "TestPointerItem12", OriginalAttributeValue = "OriginalAttributeValue" },
					new FunctionalErrorType04 { ErrorReason = "TestReasonItem13", ErrorCode = AesNctsP5FunctionalErrorCodes.Item13, ErrorPointer = "TestPointerItem13", OriginalAttributeValue = "OriginalAttributeValue" },
					new FunctionalErrorType04 { ErrorReason = "TestReasonItem14", ErrorCode = AesNctsP5FunctionalErrorCodes.Item14, ErrorPointer = "TestPointerItem14", OriginalAttributeValue = "OriginalAttributeValue" },
				}
			});
		}
		CC556CProvider provider;
	}
}
