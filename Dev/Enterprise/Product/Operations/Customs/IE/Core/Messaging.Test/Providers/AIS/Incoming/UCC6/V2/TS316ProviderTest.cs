using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS316;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	[TestedType(typeof(TS316Provider))]
	sealed class TS316ProviderTest : TestCaseWithFactory
	{
		public void TestLRN()
		{
			AssertEquals("LRN", "LRN", provider.LRN);
		}

		public void TestRejectionDate()
		{
			AssertEquals("Rejection Date", new DateTime(2023, 12, 1), provider.RejectionDate);
		}

		public void TextRejectionDate_Null()
		{
			provider = new TS316Provider(new Ts316
			{
				Declaration = new DeclarationType11
				{
					Lrn = "LRN",
					RejectionMotivationText = "Sample Text",
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
					new MFunctionalErrorType01
					{
						SequenceNumber = "2",
						ErrorPointer = "ErrorPointer002",
						ErrorCode = "52",
						ErrorReason = "ER2",
						Remarks = "Functional Error Remarks 2",
						OriginalAttributeValue = "Original Attribute Value 2",
					},
				}
			});

			AssertEquals("Null Rejection Date", ZDateTime.Empty, provider.RejectionDate);
		}

		public void TestRejectionMotivationText()
		{
			AssertEquals("Rejection Motivation Test", "Sample Text", provider.RejectionMotivationText);
		}

		public void TestFunctionalErrors()
		{
			var expectedArray = new[]
			{
				(SequenceNumber: "1", ErrorPointer: "ErrorPointer001", ErrorCode: "13", ErrorReason: "ER1", Remarks: "Functional Error Remarks 1", OriginalAttributeValue: "Original Attribute Value 1"),
				(SequenceNumber: "2", ErrorPointer: "ErrorPointer002", ErrorCode: "52", ErrorReason: "ER2", Remarks: "Functional Error Remarks 2", OriginalAttributeValue: "Original Attribute Value 2")
			};

			for (int i = 0; i < expectedArray.Length; i++)
			{
				var expected = expectedArray[i];
				var functionalError = provider.FunctionalErrors.ElementAt(i);
				AssertEquals(expected.SequenceNumber, functionalError.SequenceNumber);
				AssertEquals(expected.ErrorPointer, functionalError.ErrorPointer);
				AssertEquals(expected.ErrorCode, functionalError.ErrorCode);
				AssertEquals(expected.ErrorReason, functionalError.ErrorReason);
				AssertEquals(expected.Remarks, functionalError.Remarks);
				AssertEquals(expected.OriginalAttributeValue, functionalError.OriginalAttributeValue);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new TS316Provider(new Ts316
			{
				Declaration = new DeclarationType11
				{
					Lrn = "LRN",
					RejectionDate = new DateTime(2023, 12, 1),
					RejectionMotivationText = "Sample Text",
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
					new MFunctionalErrorType01
					{
						SequenceNumber = "2",
						ErrorPointer = "ErrorPointer002",
						ErrorCode = "52",
						ErrorReason = "ER2",
						Remarks = "Functional Error Remarks 2",
						OriginalAttributeValue = "Original Attribute Value 2",
					},
				}
			});
		}
		TS316Provider provider;
	}
}
