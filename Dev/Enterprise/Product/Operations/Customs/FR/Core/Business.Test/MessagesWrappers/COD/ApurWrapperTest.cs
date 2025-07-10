using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.OperationalActions;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.COD.Testing
{
	public class ApurWrapperTest : TestCaseWithFactory
	{
		public void TestIndicateurApurement()
		{
			var wrapper = new ApurWrapper(codDataObject);
			Assert(!wrapper.IndicateurApurement);
			codDataObject.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			wrapper = new ApurWrapper(codDataObject);
			Assert(wrapper.IndicateurApurement);
			codDataObject.CreditMethod = CreditMethodList.Codes.CreditPreviousEntryLine;
			wrapper = new ApurWrapper(codDataObject);
			Assert(!wrapper.IndicateurApurement);
			codDataObject.CreditMethod = CreditMethodList.Codes.CreditPreviousEntry;
			wrapper = new ApurWrapper(codDataObject);
			Assert(!wrapper.IndicateurApurement);
		}

		public void TestRefdecapur()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "BGM0001";
			entry.EntryNumber = "ENT0001";
			codDataObject.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			codDataObject.ReleasingEntryReference = "BGM0001";
			var wrapper = new ApurWrapper(codDataObject);
			AssertEquals("ENT0001", wrapper.Refdecapur);
		}

		public void TestMnt()
		{
			var wrapper = new ApurWrapper(codDataObject);
			AssertEquals(ZDecimal.Zero, wrapper.Mnt);

			codDataObject.Amount = 5m;
			wrapper = new ApurWrapper(codDataObject);
			AssertEquals(ZDecimal.Zero, wrapper.Mnt);

			codDataObject.CreditMethod = CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;
			wrapper = new ApurWrapper(codDataObject);
			AssertEquals(5m, wrapper.Mnt);
		}

		protected override void SetUp()
		{
			base.SetUp();
			headerApplicator = new FrCreditCODApplicator(Factory);
			codDataObject = headerApplicator.FrCreditCODItemApplicators.AddNew();
		}
		FrCreditCODApplicator headerApplicator;
		CreditCODDataObject codDataObject;
	}
}
