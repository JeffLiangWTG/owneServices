using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	public static class AISUCC6ProviderTestHelper
	{
		internal static void AssertFunctionalErrors((string SequenceNumber, string ErrorPointer, string ErrorCode, string ErrorReason, string Remarks, string OriginalAttributeValue)[] expectedItems, IReadOnlyCollection<MFunctionalError01Provider> functionalErrors)
		{
			var expectedCount = expectedItems.Length;
			Assertion.AssertEquals("Count", expectedCount, functionalErrors.Count);

			for (int i = 0; i < expectedCount; i++)
			{
				var expectedItem = expectedItems[i];
				var functionalError = functionalErrors.ElementAt(i);
				Assertion.AssertEquals("SequenceNumber", expectedItem.SequenceNumber, functionalError.SequenceNumber);
				Assertion.AssertEquals("ErrorPointer", expectedItem.ErrorPointer, functionalError.ErrorPointer);
				Assertion.AssertEquals("ErrorCode", expectedItem.ErrorCode, functionalError.ErrorCode);
				Assertion.AssertEquals("ErrorReason", expectedItem.ErrorReason, functionalError.ErrorReason);
				Assertion.AssertEquals("Remarks", expectedItem.Remarks, functionalError.Remarks);
				Assertion.AssertEquals("OriginalAttributeValue", expectedItem.OriginalAttributeValue, functionalError.OriginalAttributeValue);
			}
		}
	}
}
