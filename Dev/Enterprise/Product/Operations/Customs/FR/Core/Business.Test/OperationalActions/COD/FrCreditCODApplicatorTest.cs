using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	[TestedType(typeof(FrCreditCODApplicator))]
	public class FrCreditCODApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestBuild()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1.EntryNumber = "1";
			entryHeader1.CH_BGMReference = "3-B00001000";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.EntryNumber = "2";
			entryHeader2.CH_BGMReference = "3-B00001001";

			var entryHeader3 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader3.EntryNumber = "3";
			entryHeader3.CH_BGMReference = "3-B00001002";

			var entryHeader4 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader4.CH_BGMReference = "3-B00001003";

			Factory.Save();

			var applicator = new FrCreditCODApplicator(Factory);
			AssertEquals(0, applicator.FrCreditCODItemApplicators.Count);

			applicator.Build(new ZGuid[] { declaration1.PK, declaration2.PK });
			AssertEquals(4, applicator.FrCreditCODItemApplicators.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "3-B00001000", "3-B00001001", "3-B00001002", "3-B00001003" }, applicator.FrCreditCODItemApplicators.Select(x => x.ReleasingEntryReference));

			applicator.FrCreditCODItemApplicators.RemoveAndDeleteAll();
			applicator.Build(new ZGuid[] { entryHeader1.PK, entryHeader2.PK, entryHeader3.PK, entryHeader4.PK });
			AssertEquals(4, applicator.FrCreditCODItemApplicators.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "3-B00001000", "3-B00001001", "3-B00001002", "3-B00001003" }, applicator.FrCreditCODItemApplicators.Select(x => x.ReleasingEntryReference));
		}
	}
}
