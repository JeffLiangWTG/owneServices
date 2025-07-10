using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class CusSealSequenceNumberGeneratorTest : TestCaseWithFactory
	{
		public void TestSequenceStartingNumber()
		{
			var generator = new CusSealSequenceNumberGenerator(() => System.Array.Empty<CusSeal>());
			AssertEquals("Sequence should start with 3", 3, generator.SequenceStartingNumber);
		}

		public void TestRecalculateWhenAdded()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var sealCollection = header.DepartureHeaderContainers.AddNew().AdditionalSeals;

			var seal1 = sealCollection.AddNew();
			var seal2 = sealCollection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("seal1.BK_SequenceNumber", (ZShort)3, seal1.BK_SequenceNumber);
				AssertEquals("seal2.BK_SequenceNumber", (ZShort)4, seal2.BK_SequenceNumber);

				var seal3 = sealCollection.AddNew();
				AssertEquals("seal3.BK_SequenceNumber", (ZShort)5, seal3.BK_SequenceNumber);

				seal2.Delete();
				AssertEquals("seal3.BK_SequenceNumber recalculated", (ZShort)4, seal3.BK_SequenceNumber);

				var seal4 = sealCollection.AddNew();
				AssertEquals("seal4.BK_SequenceNumber after delete one", (ZShort)5, seal4.BK_SequenceNumber);
			});
		}
	}
}
