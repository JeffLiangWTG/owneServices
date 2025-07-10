using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM099;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	sealed class IM099ProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN123456789", provider.LocalReferenceNumber);
		}

		public void TestDateLimitOfResponse()
		{
			AssertEquals(new DateTime(2023, 08, 11), provider.DateLimitOfResponse);
		}

		public void TestRemarks()
		{
			AssertEquals("Remarks001", provider.Remarks);
		}

		public void TestCustomsOfficeLodgement()
		{
			AssertEquals("LCO123456", provider.CustomsOfficeLodgement);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM099Provider(new Im099
			{
				Declaration = new DeclarationType
				{
					Lrn25 = "LRN123456789",
					DateLimitOfResponse = "20230811",
					Remarks = "Remarks001",
					CustomsOffices = new DeclarationTypeCustomsOffices { CustomsOfficeLodgement = "LCO123456" },
				}
			});
		}
		IM099Provider provider;
	}
}
