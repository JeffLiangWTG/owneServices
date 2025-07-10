using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM415V;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	class IM415VProviderTest : TestCaseWithFactory
	{
		public void TestAdditionalDeclarationType()
		{
			AssertEquals("D", provider.AdditionalDeclarationType);
		}

		public void TestLocalReferenceNumber()
		{
			AssertEquals("ACPTESTIM0990446123456", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("21IEDUB11A782454R2", provider.MovementReferenceNumber);
		}

		public void TestDeclarationAcknowledgementDate()
		{
			AssertEquals(new DateTime(2021, 02, 15), provider.DeclarationAcknowledgementDate);

			provider = new IM415VProvider(new Im415V { Declaration = new DeclarationType() });
			AssertEquals(ZDateTime.Empty, provider.DeclarationAcknowledgementDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM415VProvider(new Im415V
			{
				Declaration = new DeclarationType
				{
					AdditionalDeclarationType12 = "D",
					Lrn25 = "ACPTESTIM0990446123456",
					Mrn = "21IEDUB11A782454R2",
					DeclarationAcknowledgementDate = "20210215",
				},
			});
		}
		IM415VProvider provider;
	}
}
