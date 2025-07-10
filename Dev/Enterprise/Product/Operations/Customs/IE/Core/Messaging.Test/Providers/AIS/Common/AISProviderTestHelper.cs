using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public static class AISProviderTestHelper
	{
		internal static void AssertFunctionalErrors((string ErrorPointer, string ErrorType, string ErrorReason, string ErrorMessage, string OriginalAttributeValue)[] expectedItems, IReadOnlyCollection<FunctionalErrorTypeProvider> functionalErrors)
		{
			var expectedCount = expectedItems.Length;
			Assertion.AssertEquals("Count", expectedCount, functionalErrors.Count);

			for (int i = 0; i < expectedCount; i++)
			{
				var expectedItem = expectedItems[i];
				var functionalError = functionalErrors.ElementAt(i);
				Assertion.CombineAssertions($"Item {i}", () =>
				{
					Assertion.AssertEquals("ErrorPointer", expectedItem.ErrorPointer, functionalError.ErrorPointer);
					Assertion.AssertEquals("ErrorType", expectedItem.ErrorType, functionalError.ErrorType);
					Assertion.AssertEquals("ErrorReason", expectedItem.ErrorReason, functionalError.ErrorReason);
					Assertion.AssertEquals("ErrorMessage", expectedItem.ErrorMessage, functionalError.ErrorMessage);
					Assertion.AssertEquals("OriginalAttributeValue", expectedItem.OriginalAttributeValue, functionalError.OriginalAttributeValue);
				});
			}
		}
	}
}
