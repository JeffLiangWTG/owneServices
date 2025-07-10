using System.Collections.ObjectModel;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	class PartyIdProviderTest : DataProviderTestCase<PartyIdProvider>
	{
		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber", "Num123", Provider.IdentificationNumber);
		}

		public void TestCommunications()
		{
			AssertNotNull("Communications", Provider.Communications);
		}

		protected override void SetUp()
		{
			base.SetUp();

			identificationNumber = "Num123";
		}
		string identificationNumber;

		PartyIdProvider GenerateProvider(string identificationNumber) => new PartyIdProvider(identificationNumber, new Collection<IIdentifierTypePair>());

		protected sealed override PartyIdProvider GetProvider()
		{
			return GenerateProvider(identificationNumber);
		}
	}
}
