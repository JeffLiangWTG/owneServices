using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusSeal))]
	class CusSealTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSequence()
		{
			CombineAssertions(() =>
			{
				var seal1 = SealCollection.AddNew();
				seal1.BK_SealNumber = "S1";
				var seal2 = SealCollection.AddNew();
				seal2.BK_SealNumber = "S2";
				AssertEquals("Sequence when added.", (short)1, seal1.BK_SequenceNumber);
				AssertEquals("Sequence when added.", (short)2, seal2.BK_SequenceNumber);
				seal2.Delete();
				var seal3 = SealCollection.AddNew();
				seal3.BK_SealNumber = "S3";
				AssertEquals("Sequence when added.", (short)2, seal3.BK_SequenceNumber);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => Seal;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => Seal;

		CusContainer Container => container ??= Factory.NewWithValidTestData<CusContainer>();
		CusContainer container;

		CusSealCollection SealCollection => sealCollection ??= new CusSealCollection(Container);
		CusSealCollection sealCollection;

		CusSeal Seal
		{
			get
			{
				if (seal == null)
				{
					seal = SealCollection.AddNew();
					seal.BK_SealNumber = "Seal1";
				}

				return seal;
			}
		}
		CusSeal seal;
	}
}
