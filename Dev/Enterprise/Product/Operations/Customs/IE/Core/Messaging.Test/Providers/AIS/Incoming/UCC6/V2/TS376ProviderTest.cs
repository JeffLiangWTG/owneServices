using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS376;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging.AIS.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	[TestedType(typeof(TS376Provider))]
	sealed class TS376ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345ABCDE678R9", provider.MovementReferenceNumber);
		}

		public void TestAmendmentRejectionDate()
		{
			AssertEquals("Normal DateTime", new DateTime(2023, 08, 10, 14, 30, 45), provider.RejectionDate);

			provider = new TS376Provider(new Ts376 { Declaration = new DeclarationType22 { RejectionDate = DateTime.MinValue } });
			AssertEquals("DateTime.MinValue", ZDateTime.Empty, provider.RejectionDate);
		}

		public void TestAmendmentRejectionMotivationText()
		{
			AssertEquals("Rejection Motivation Text", provider.RejectionMotivationText);
		}

		public void TestFunctionalErrors()
		{
			AISUCC6ProviderTestHelper.AssertFunctionalErrors(new[]
			{
				(SequenceNumber: "1", ErrorPointer: "ErrorPointer001", ErrorCode: "13", ErrorReason: "ER1", Remarks: "Functional Error Remarks 1", OriginalAttributeValue: "Original Attribute Value 1"),
				(SequenceNumber: "2", ErrorPointer: "ErrorPointer002", ErrorCode: "52", ErrorReason: "ER2", Remarks: "Functional Error Remarks 2", OriginalAttributeValue: "Original Attribute Value 2")
			}, provider.FunctionalErrors);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new TS376Provider(new Ts376
			{
				Declaration = new DeclarationType22
				{
					Mrn = "12MRN345ABCDE678R9",
					RejectionDate = new DateTime(2023, 08, 10, 14, 30, 45),
					RejectionMotivationText = "Rejection Motivation Text",
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
		TS376Provider provider;
	}
}
