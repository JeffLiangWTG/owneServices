using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM484;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1.Testing
{
	class IM484ProviderTest : TestCaseWithFactory
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestLRN()
		{
			AssertEquals("LRN123", provider.LocalReferenceNumber);
		}

		public void TestRequestDate()
		{
			AssertEquals(new ZDateTime(2024, 3, 14), provider.RequestDate);
		}

		public void TestDateLimit()
		{
			AssertEquals(new ZDateTime(2024, 4, 13), provider.DateLimit);
		}

		public void TestAdditionalInformations()
		{
			AssertEquals(2, provider.AdditionalInformations.Count);
			AssertType<DocumentAdditionalInformationProvider>(provider.AdditionalInformations.First());
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM484Provider(new Im484
			{
				Declaration = new DeclarationType()
				{
					Mrn = "12MRN345CDEFG678R9",
					Lrn = "LRN123",
					RequestDate = "20240314",
					DateLimit = "20240413",
				},
				GoodsShipment = new Collection<DocumentAdditionalInformationType>()
				{
					new DocumentAdditionalInformationType()
					{
						DocumentComplementaryInformation = "comp info1",
						DocumentType = "Y023",
					},
					new DocumentAdditionalInformationType()
					{
						DocumentComplementaryInformation = "comp info 2",
						DocumentType = "U713",
					}
				}
			});
		}
		IM484Provider provider;
	}
}

