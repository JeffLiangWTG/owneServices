using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM415V;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AIS.Testing
{
	class IM415VMessageProviderTest : TestCaseWithFactory
	{
		public void TestAdditionalDeclarationType()
		{
			AssertEquals("A", provider.AdditionalDeclarationType);
		}

		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN", provider.MovementReferenceNumber);
		}

		public void TestDeclarationAcknowledgementDate()
		{
			AssertEquals(new DateTime(2023, 08, 11), provider.DeclarationAcknowledgementDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM415VProvider(new Im415V
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType415V
				{
					AdditionalDeclarationType = "A",
					Lrn = "LRN",
					Mrn = "MRN",
					DeclarationAcknowledgementDate = new DateTime(2023, 08, 11)
				}
			});
		}
		IM415VProvider provider;
	}
}
