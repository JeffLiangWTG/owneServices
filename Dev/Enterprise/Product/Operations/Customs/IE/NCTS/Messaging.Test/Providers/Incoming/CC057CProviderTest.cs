using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC057C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	public class CC057CProviderTest : TestCaseWithFactory
	{
		public void TestEmptyValue()
		{
			CombineAssertions(() =>
			{
				var emptyProvider = new CC057CProvider(new Cc057CType());
				AssertEquals("Should return empty value", ZString.Empty, emptyProvider.MRN);
				AssertEquals("Should return empty value", ZString.Empty, emptyProvider.RejectionCode);
				AssertEquals("Should return empty value", ZString.Empty, emptyProvider.RejectionReason);
				AssertEquals("Should return empty value", 0, emptyProvider.FunctionalErrors.Count);
			});
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "MRN", provider.MRN);
		}

		public void TestRejectionCode()
		{
			AssertEquals("RejectionCode", "Test Code", provider.RejectionCode);
		}

		public void TestRejectionReason()
		{
			AssertEquals("RejectionReason", "Test Reason", provider.RejectionReason);
		}

		public void TestFunctionalError()
		{
			CombineAssertions(() =>
			{
				var errors = provider.FunctionalErrors.ToArray();
				var expected = new[]
				{
					("TestReasonItem12", "12", "TestPointerItem12", "OriginalAttributeValue12"),
					("TestReasonItem13", "13", "TestPointerItem13", "OriginalAttributeValue13"),
					("TestReasonItem14", "14", "TestPointerItem14", "OriginalAttributeValue14"),
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
			base.SetUp();
			provider = new CC057CProvider(new Cc057CType
			{
				TransitOperation = new TransitOperationType21
				{
					Mrn = "MRN",
					RejectionCode = "Test Code",
					RejectionReason = "Test Reason"
				},
				FunctionalError = new System.Collections.ObjectModel.Collection<FunctionalErrorType04>
				{
					new FunctionalErrorType04 { ErrorReason = "TestReasonItem12", ErrorCode = AesNctsP5FunctionalErrorCodes.Item12, ErrorPointer = "TestPointerItem12", OriginalAttributeValue = "OriginalAttributeValue12" },
					new FunctionalErrorType04 { ErrorReason = "TestReasonItem13", ErrorCode = AesNctsP5FunctionalErrorCodes.Item13, ErrorPointer = "TestPointerItem13", OriginalAttributeValue = "OriginalAttributeValue13" },
					new FunctionalErrorType04 { ErrorReason = "TestReasonItem14", ErrorCode = AesNctsP5FunctionalErrorCodes.Item14, ErrorPointer = "TestPointerItem14", OriginalAttributeValue = "OriginalAttributeValue14" },
				}
			});
		}
		CC057CProvider provider;
	}
}
