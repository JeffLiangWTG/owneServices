using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS315V;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	[TestedType(typeof(TS315VProvider))]
	sealed class TS315VProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN001", provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345ABCDE678R9", provider.MovementReferenceNumber);
		}

		public void TestDeclarationAcknowledgementDate()
		{
			AssertEquals(new ZDateTime(2023, 08, 10, 14, 30, 45), provider.DeclarationAcknowledgementDate);
		}

		protected override void SetUp()
		{
			base.SetUp();

			provider = new TS315VProvider(new Ts315V
			{
				Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.DeclarationType12
				{
					Lrn = "LRN001",
					Mrn = "12MRN345ABCDE678R9",
					DeclarationAcknowledgementDate = new DateTime(2023, 08, 10, 14, 30, 45),
				},
				SupervisingCustomsOffice = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.SupervisingcustomofficeType { ReferenceNumber = "SCO12345" },
				CustomsOfficeLodgement = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType01 { ReferenceNumber = "LCO12345" },
				Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.DeclarantType03 { IdentificationNumber = "ID1" },
			});
		}

		TS315VProvider provider;
	}
}
