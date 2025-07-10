using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.AIS_complex;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.RF416;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	[TestedType(typeof(RF416Provider))]
	class RF416ProviderTest : TestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new RF416Provider(null));
		}

		public void TestApplicationReferenceId()
		{
			AssertEquals("AppReferenceID", "APPREF_1234", provider.ApplicationReferenceId);
		}

		public void TestRejectionDate()
		{
			AssertEquals("Rejection Date", new ZDateTime(2023, 8, 2), provider.RejectionDate);
		}

		public void TestRejectionReason()
		{
			AssertEquals("RejectionReason", "Test Reason", provider.RejectionReason);
		}

		public void TestDecisionTakingCustomsAuthority()
		{
			AssertEquals("DecisionTakingCustomsAuthority", "Autority 123", provider.DecisionTakingCustomsAuthority);
		}

		public void TestApplicantOrHolderIdentification()
		{
			AssertEquals("ApplicantOrHolderIdentification", "APP_3_2_Content", provider.ApplicantOrHolderIdentification);
		}

		public void TestRepresentativeIdentification()
		{
			AssertEquals("RepresentativeIdentification", "REPR_ID_3_4_Value", provider.RepresentativeIdentification);
		}

		public void TestFunctionalErrors()
		{
			AISProviderTestHelper.AssertFunctionalErrors(new[]
			{
				(ErrorPointer: "ErrorPointer001", ErrorType: "13", ErrorReason: "ER1", ErrorMessage: "Functional Error Message 1", OriginalAttributeValue: "Original Attribute Value 1"),
				(ErrorPointer: "ErrorPointer002", ErrorType: "40", ErrorReason: "ER2", ErrorMessage: "Functional Error Message 2", OriginalAttributeValue: "Original Attribute Value 2")
			}, provider.FunctionalErrors);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new RF416Provider(GenerateMessage());
		}

		RF416Provider provider;

		Rf416 GenerateMessage()
		{
			return new Rf416
			{
				Header = new HeaderType
				{
					ApplicationReferenceId = "APPREF_1234",
					RejectionDate = "20230802",
					RejectionReason = "Test Reason",
					DecisionTakingCustomsAuthority = "Autority 123",
				},
				Parties = new Rf416Parties
				{
					Applicant32 = "APP_3_2_Content",
					RepresentativeIdentification34 = "REPR_ID_3_4_Value",
				},
				FunctionalError = new Collection<FunctionalErrorType>
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
				}
			};
		}
	}
}
