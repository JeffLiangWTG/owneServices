using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CusEntryHeaderFetchStrategyTest : Customs.Business.FetchStrategies.Testing.CusEntryHeaderFetchStrategyTest
	{
		public new void TestFetchForView()
		{
			var header1 = Factory.New<CusEntryHeader>();
			header1.CH_JE = Master.PK;
			var instruction1 = Master.CustomsEntryInstructions.AddNew();
			header1.CH_CEI_Instruction = instruction1.PK;
			instruction1.CusInBondPermitsHeaders.AddNew();
			var header2 = Factory.New<CusEntryHeader>();
			header2.CH_JE = Master.PK;
			var instruction2 = Master.CustomsEntryInstructions.AddNew();
			header2.CH_CEI_Instruction = instruction1.PK;
			instruction2.CusInBondPermitsHeaders.AddNew();
			Factory.Save();
			TestFetchForView(header1, header2);
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory) => new CusEntryHeaderCollection<CusEntryHeader>(factory.Load<JobDeclaration>(Master.PK), factory);

		JobDeclaration Master => master ?? (master = Factory.New<JobDeclaration>());
		JobDeclaration master;
	}
}
