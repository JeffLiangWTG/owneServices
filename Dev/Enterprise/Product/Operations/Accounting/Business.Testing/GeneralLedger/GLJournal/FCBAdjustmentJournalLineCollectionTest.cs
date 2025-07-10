using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(FCBAdjustmentJournalLineCollection))]
	public class FCBAdjustmentJournalLineCollectionTest : GLJournalLineCollection_InnerTest
	{
		public void TestDefaultFirstJournalLine()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			var line = Journal.GLJournalLines.AddNew();
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, line.AL_RX_NKTransactionCurrency);
			AssertEquals(testObjectCreator.GLHeader1.PK, line.AL_AG);
		}

		protected new FCBAdjustmentJournalLineCollection TestCollection;
		protected new FCBAdjustmentJournal Journal;

		protected override void SetUp()
		{
			base.SetUp();
			Journal = Factory.New<FCBAdjustmentJournal>();
			TestCollection = (FCBAdjustmentJournalLineCollection)Journal.GLJournalLines;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<FCBAdjustmentJournal>();
			AssertNotNull(
@"This is just to initialize 'Lines' collection before any lines are created to prevent loading them in it later as side effect of calling bizo properties.
Such 'accidental', from test position, 'Lines' collection load run some collection code that is interfere with test expectations.",
				parent.Lines);
			return new FCBAdjustmentJournalLineCollection(parent);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
