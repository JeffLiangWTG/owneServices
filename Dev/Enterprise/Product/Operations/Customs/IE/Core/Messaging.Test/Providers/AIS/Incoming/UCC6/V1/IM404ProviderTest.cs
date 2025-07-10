using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM404;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC6.V1.Testing
{
	class IM404ProviderTest : TestCaseWithFactory
	{
		public void TestAmendmentAcceptanceDate()
		{
			AssertEquals(new ZDateTime(2024, 03, 01), provider.AmendmentAcceptanceDateAndTime);
		}

		protected override void SetUp()
		{
			provider = new IM404Provider(new Im404()
			{
				Declaration = new DeclarationType()
				{
					AmendmentAcceptanceDate = "20240301",
					Mrn = "MRN12345",
					CustomsOffices = new CustomsOffices02Type()
					{
						CustomsOfficeLodgement = "IEDUB100"
					}
				}
			});
		}
		IM404Provider provider;
	}
}
