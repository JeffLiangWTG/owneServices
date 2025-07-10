using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class ProcedureProviderTest : DataProviderTestCase<ProcedureProvider>
	{
		public void TestRequestedProcedure()
		{
			AssertNull("Requested Procedure", procedureProvider.RequestedProcedure);
		}

		public void TestPreviousProcedure()
		{
			AssertNull("Previous Procedure", procedureProvider.PreviousProcedure);
		}

		public void TestAdditionalProcedure()
		{
			AssertEquals("Count of Additional Procedures", 1, procedureProvider.AdditionalProcedure.Count);
			Assert("An Empty Additional Procedure exists", procedureProvider.AdditionalProcedure.Any(x => string.IsNullOrEmpty(x.AdditionalProcedure)));

			bill.ABL_Procedure = EUH7AdditionalProcedureCodeList.Codes.C08;
			procedureProvider = new ProcedureProvider(packedItem);

			CombineAssertions(() =>
			{
				AssertEquals("Count of Additional Procedures", 1, procedureProvider.AdditionalProcedure.Count);
				Assert("Additional Procedure exists", procedureProvider.AdditionalProcedure.Any(x => x.AdditionalProcedure == "C08"));
			});

			bill.ABL_Procedure = EUH7AdditionalProcedureCodeList.Codes.C07F48;
			procedureProvider = new ProcedureProvider(packedItem);

			CombineAssertions(() =>
			{
				AssertEquals("Count of Additional Procedures", 2, procedureProvider.AdditionalProcedure.Count);
				Assert("Additional Procedure exists", procedureProvider.AdditionalProcedure.Any(x => x.AdditionalProcedure == "C07"));
				Assert("Additional Procedure exists", procedureProvider.AdditionalProcedure.Any(x => x.AdditionalProcedure == "F48"));
			});

			bill.ABL_Procedure = EUH7AdditionalProcedureCodeList.Codes.C07F49;
			procedureProvider = new ProcedureProvider(packedItem);

			CombineAssertions(() =>
			{
				AssertEquals("Count of Additional Procedures", 2, procedureProvider.AdditionalProcedure.Count);
				Assert("Additional Procedure exists", procedureProvider.AdditionalProcedure.Any(x => x.AdditionalProcedure == "C07"));
				Assert("Additional Procedure exists", procedureProvider.AdditionalProcedure.Any(x => x.AdditionalProcedure == "F49"));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			bill = Factory.New<AsycudaBill>();
			packedItem = bill.PackedItems.AddNew();
			procedureProvider = new ProcedureProvider(packedItem);
		}

		ProcedureProvider procedureProvider;
		AsycudaBill bill;
		AsycudaPackedItem packedItem;

		protected sealed override ProcedureProvider GetProvider()
		{
			return new ProcedureProvider(packedItem);
		}
	}
}
