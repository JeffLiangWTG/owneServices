using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

internal class SealDataProviderTest : TestCaseWithFactory
{
	public void TestNewCollection()
	{
		CombineAssertions(() =>
		{
			AssertNull("null", SealDataProvider.NewCollection(null));

			var headerContainer = NctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer.Seal1 = ZString.Empty;
			headerContainer.Seal2 = ZString.Empty;
			var sealDataProvider = SealDataProvider.NewCollection(headerContainer);

			AssertNull("no Seals specified", sealDataProvider);

			headerContainer.Seal1 = "1234";

			AssertNotNull("HeaderContainer & Seals != null", SealDataProvider.NewCollection(headerContainer));
		});
	}

	public void TestProvider()
	{
		var headerContainer = NctsHeader.DepartureHeaderContainers.AddNew();
		headerContainer.Seal1 = "1234";
		headerContainer.Seal2 = "5678";
		headerContainer.AdditionalSeals.AddNew().BK_SealNumber = "ADD1";
		headerContainer.AdditionalSeals.AddNew().BK_SealNumber = "ADD2";

		var sealDataProvider = SealDataProvider.NewCollection(headerContainer);

		CombineAssertions(() =>
		{
			AssertEquals("Number Of Rows", 4, sealDataProvider.Count());
			AssertEquals("Seal1 Sequence Number", 1, sealDataProvider.ElementAt(0).SequenceNumber);
			AssertEquals("Seal1 Identifier", "1234", sealDataProvider.ElementAt(0).Identifier);
			AssertEquals("Seal2 Sequence Number", 2, sealDataProvider.ElementAt(1).SequenceNumber);
			AssertEquals("Seal2 Identifier", "5678", sealDataProvider.ElementAt(1).Identifier);
			AssertEquals("Additional Seal1 Sequence Number", 3, sealDataProvider.ElementAt(2).SequenceNumber);
			AssertEquals("Additional Seal1 Identifier", "ADD1", sealDataProvider.ElementAt(2).Identifier);
			AssertEquals("Additional Seal2 Sequence Number", 4, sealDataProvider.ElementAt(3).SequenceNumber);
			AssertEquals("Additional Seal2 Identifier", "ADD2", sealDataProvider.ElementAt(3).Identifier);

			headerContainer.Seal2 = ZString.Empty;
			headerContainer.AdditionalSeals.RemoveAll();

			AssertEquals("Number Of Rows", 1, sealDataProvider.Count());
			AssertEquals("Seal1 Sequence Number", 1, sealDataProvider.ElementAt(0).SequenceNumber);
			AssertEquals("Seal1 Identifier", "1234", sealDataProvider.ElementAt(0).Identifier);
		});
	}

	NctsHeader CreateNctsHeader()
	{
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader;
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;
}
