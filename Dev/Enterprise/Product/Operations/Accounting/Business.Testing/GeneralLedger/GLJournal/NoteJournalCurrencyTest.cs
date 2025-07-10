using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(NoteJournalCurrency))]
	public class NoteJournalCurrencyTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<NoteJournalCurrency>();

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			var noteJournalCurrency = info.BizObj as NoteJournalCurrency;
			if (info == noteJournalCurrency.RX_SubUnitRatioInfo)
			{
				noteJournalCurrency.RX_SubUnitRatio = 1;
				AssertEquals("Valid ZInt on RX_SubUnitRatio", 100, noteJournalCurrency.RX_SubUnitRatio);
			}
			else
			{
				base.TestBizObjectField(info);
			}
		}
	}
}
