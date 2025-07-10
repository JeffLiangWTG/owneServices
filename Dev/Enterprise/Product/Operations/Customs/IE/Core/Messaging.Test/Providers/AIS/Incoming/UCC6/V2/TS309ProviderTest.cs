using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS309;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	[TestedType(typeof(TS309Provider))]
	sealed class TS309ProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TS309Provider(null));
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("Movement Reference Number", "12MRN345ABCDE678R9", provider.MovementReferenceNumber);
		}

		public void TestInvalidationDecision()
		{
			AssertEquals("Invalidation Decision", true, provider.InvalidationDecision);
		}

		public void TestInvalidationInitiatedByCustoms()
		{
			AssertEquals("Invalidation Initiated ByCustoms", true, provider.InvalidationInitiatedByCustoms);
		}

		public void TestInvalidationJustification()
		{
			AssertEquals("Invalidation Justification", "Invalidation Justification Text", provider.InvalidationJustification);
		}

		public void TestDateOfInvalidationDecision()
		{
			AssertEquals("Date Of Invalidation Decision", new ZDateTime(2023, 08, 10, 14, 30, 45), provider.DateOfInvalidationDecision);
		}

		public void TestDateOfInvalidationRequest()
		{
			AssertEquals("Date Of Invalidation Request", new ZDateTime(2023, 08, 11, 14, 30, 45), provider.DateOfInvalidationRequest);
		}

		public void TestDateOfInvalidation()
		{
			AssertEquals("Date Of Invalidation", new ZDateTime(2023, 08, 12, 14, 30, 45), provider.DateOfInvalidation);
		}
		
		public void TestFunctionalErrors()
		{
			AssertNotNull("FunctionalErrors", provider.FunctionalErrors);
			AssertEquals("FunctionalErrors Count", 1, provider.FunctionalErrors.Count);

			var providerWithNoFunctionalErrors = new TS309Provider(new Ts309());
			AssertNotNull("FunctionalErrors when TS309 errors are null", providerWithNoFunctionalErrors.FunctionalErrors);
			AssertEquals("FunctionalErrors Count when TS309 errors are null", 0, providerWithNoFunctionalErrors.FunctionalErrors.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new TS309Provider(GenerateMessage());
		}

		TS309Provider provider;
		Ts309 GenerateMessage()
		{
			return new Ts309
			{
				Declaration = new DeclarationType04
				{
					Mrn = "12MRN345ABCDE678R9",
					InvalidationDecision = true,
					InvalidationInitiatedByCustoms = true,
					InvalidationJustification = "Invalidation Justification Text",
					DateOfInvalidationDecision = new DateTime(2023, 08, 10, 14, 30, 45),
					DateOfInvalidationRequest = new DateTime(2023, 08, 11, 14, 30, 45),
					DateOfInvalidation = new DateTime(2023, 08, 12, 14, 30, 45)
				},
				FunctionalError = new System.Collections.ObjectModel.Collection<MFunctionalErrorType01>
				{
					new MFunctionalErrorType01
					{
						SequenceNumber = "1",
						ErrorPointer = "ErrorPointer001",
						ErrorCode = "13",
						ErrorReason = "ER1",
						Remarks = "Functional Error Remarks 1",
						OriginalAttributeValue = "Original Attribute Value 1",
					},
				}
			};
		}
	}
}
