using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ActiveCusEntryHeaderCollection))]
	public class ActiveCusEntryHeaderCollectionTest : Customs.Business.Testing.ActiveCusEntryHeaderCollectionTest<ActiveCusEntryHeaderCollection>
	{
		public void TestAllMergedLinesFees()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals(0, declaration.ActiveEntryHeaders.AllMergedLinesFees.Count());

			var entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader1.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate(ChargeTypesList.Codes.DTY, 10.10m);
			entryLine1.Fees.AddOrUpdate(Constants.RateTypes.PIS, 20.10m);
			AssertEquals(2, declaration.ActiveEntryHeaders.AllMergedLinesFees.Count());

			var entryLine2 = entryHeader1.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate(Constants.RateTypes.Antidumping, 22.10m);
			AssertEquals(3, declaration.ActiveEntryHeaders.AllMergedLinesFees.Count());

			var entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			AssertEquals(3, declaration.ActiveEntryHeaders.AllMergedLinesFees.Count());

			var entryLine3 = entryHeader2.MergedLines.AddNew();
			entryLine3.Fees.AddOrUpdate(Constants.RateTypes.IPI, 22.10m);
			AssertEquals(4, declaration.ActiveEntryHeaders.AllMergedLinesFees.Count());
		}

		public void TestFormalEntries()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.CDI;
			declaration.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.CDI;
			declaration.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.SUF;
			declaration.CustomsEntryHeaders.AddNew().CH_MessageType = MessageTypeList.Codes.SUF;

			AssertEquals("FormalEntries", 2, declaration.ActiveEntryHeaders.FormalEntries.Count());
			Assert(declaration.ActiveEntryHeaders.FormalEntries.All(x => x.CH_MessageType == MessageTypeList.Codes.CDI));

			AssertEquals("SiscomexUsageFeeEntries", 2, declaration.ActiveEntryHeaders.SiscomexUsageFeeEntries.Count());
			Assert(declaration.ActiveEntryHeaders.SiscomexUsageFeeEntries.All(x => x.CH_MessageType == MessageTypeList.Codes.SUF));
		}

		protected override ActiveCusEntryHeaderCollection GetCollectionToTest()
		{
			return new ActiveCusEntryHeaderCollection((JobDeclaration)Declaration);
		}
	}
}
