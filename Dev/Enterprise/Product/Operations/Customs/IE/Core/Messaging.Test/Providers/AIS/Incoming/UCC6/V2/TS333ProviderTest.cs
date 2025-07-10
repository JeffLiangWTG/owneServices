using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS333;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging.AIS.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	[TestedType(typeof(TS333Provider))]
	sealed class TS333ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestRejectionDate()
		{
			AssertEquals("Normal Datetime", new ZDateTime(2023, 09, 15, 14, 10, 59), provider.RejectionDate);

			provider = new TS333Provider(new Ts333 { Declaration = new DeclarationType10 { RejectionDate = DateTime.MinValue } });
			AssertEquals("DateTime.MinValue", ZDateTime.Empty, provider.RejectionDate);
		}

		public void TestRejectionReason()
		{
			AssertEquals("Rejection Reason", provider.RejectionReason);
		}

		public void TestFunctionalErrros()
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

			provider = new TS333Provider(new Ts333
			{
				Declaration = new DeclarationType10
				{
					Mrn = "12MRN345CDEFG678R9",
					RejectionDate = new DateTime(2023, 09, 15, 14, 10, 59),
					RejectionReason = "Rejection Reason",
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
					}
				}
			});
		}

		TS333Provider provider;
	}
}
