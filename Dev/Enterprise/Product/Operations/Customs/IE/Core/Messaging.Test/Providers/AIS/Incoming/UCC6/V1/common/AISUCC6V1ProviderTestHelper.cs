using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1.Testing
{
	public static class AISUCC6V1ProviderTestHelper
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

		public static void CreateFunctionalErrorTypeReferenceTestData(BusinessObjectFactory factory) => UCC5.Testing.AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeReferenceTestData(factory);

		public static Collection<FunctionalErrorType> CreateFunctionalErrorTypeObjects() => new Collection<FunctionalErrorType>
		{
			new FunctionalErrorType
			{
				ErrorPointer = "ErrorPointer001",
				ErrorType = "13",
				ErrorReason = "ER1",
				ErrorMessage = "Functional Error Message 1",
				OriginalAttributeValue = "Original Attribute Value 1",
			},
			new FunctionalErrorType
			{
				ErrorPointer = "ErrorPointer002",
				ErrorType = "40",
				ErrorReason = "ER2",
				ErrorMessage = "Functional Error Message 2",
				OriginalAttributeValue = "Original Attribute Value 2",
			},
		};

		public const string ExpectedFunctionalErrorInterpretation = UCC5.Testing.AISUCC5InterchangeProcessorTestHelper.ExpectedFunctionalErrorInterpretation;
	}
}
