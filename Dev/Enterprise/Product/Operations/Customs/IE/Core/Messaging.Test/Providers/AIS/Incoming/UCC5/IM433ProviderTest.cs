using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM433;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging.Testing;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	sealed class IM433ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestRejectionDate()
		{
			AssertEquals(new ZDateTime(2024, 02, 29), provider.RejectionDate);
		}

		public void TestRejectionReason()
		{
			AssertEquals("Rejection Reason", provider.RejectionReason);
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
			provider = new IM433Provider(new Im433
			{
				Declaration = new DeclarationType()
				{
					Mrn = "12MRN345CDEFG678R9",
					RejectionDate = "20240229",
					RejectionReason = "Rejection Reason",
					CustomsOffices = new DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "ABCDE123",
					},
					Parties = new DeclarationTypeParties()
					{
						Declarant = new DeclarantType()
						{
							Declarant318 = "TOM THE DECLARANT",
							Declarant317 = new TraderType()
							{
								Name = "BOB THE BUILDER",
								StreetAndNumber = "Grand Canal Street Upper 1",
								CountryCode = "IE",
								Postcode = "D04 Y7R5",
								City = "Dublin",
							}
						}
					},
				},
				FunctionalError = AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeObjects(),
			});
		}
		IM433Provider provider;
	}
}
