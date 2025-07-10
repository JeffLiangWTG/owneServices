using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APJournalCollection))]
	public class APJournalCollectionTest : ActiveBusinessObjectCollectionTestCase<APJournalCollection>
	{
		APMatchingBase apMatchingBase;

		protected override void SetUp()
		{
			base.SetUp();
			apMatchingBase = new APMatchingBase(Factory);
		}

		protected override APJournalCollection GetCollectionToTest()
		{
			return new APJournalCollection(Factory, apMatchingBase, new AdhocCollectionRelationship(typeof(APJournal)));
		}

		public void TestSetDefaultsForNewElementCore()
		{
			apMatchingBase.MatchDate = ZDateTime.BrettsBirthday;
			ZGuid orgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			apMatchingBase.PrimaryOrganization = orgPK;

			APJournal apJournal = Collection.AddNew();

			AssertEquals(ZDateTime.BrettsBirthday, apJournal.AH_InvoiceDate);
			AssertEquals(ZDateTime.BrettsBirthday, apJournal.AH_PostDate);
			AssertEquals(orgPK, apJournal.AH_OH);
			AssertEquals((Guid)AccountingConfigurationRegistry.Instance.APJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), apJournal.AH_AG);

			apMatchingBase.MatchDate = ZDateTime.Today.AddDays(1);
			apJournal = Collection.AddNew();
			AssertEquals(ZDateTime.Today.AddDays(1), apJournal.AH_InvoiceDate);
			AssertEquals(ZDateTime.Today, apJournal.AH_PostDate);
		}

		public void TestShowGLAccountsForImportAction()
		{
			var collection = GetCollectionToTest();
			collection.ShowGLAccountsForImportAction = (glHeaderCollection, glHeaderList) => { glHeaderList.Add(Factory.New<AccGLHeader>()); };
			var line = collection.AddNew();
			AssertNotNull(line.ShowGLAccountsForImportAction);
			AssertEquals(collection.ShowGLAccountsForImportAction, line.ShowGLAccountsForImportAction);
		}

		public void TestSetUseJournalValidation()
		{
			APJournal apJournal1 = Collection.AddNew();
			APJournal apJournal2 = Collection.AddNew();

			Assert(!apJournal1.UseJournalValidation);
			Assert(!apJournal2.UseJournalValidation);

			Collection.SetUseJournalValidation(true);

			Assert(apJournal1.UseJournalValidation);
			Assert(apJournal2.UseJournalValidation);

			Collection.SetUseJournalValidation(false);

			Assert(!apJournal1.UseJournalValidation);
			Assert(!apJournal2.UseJournalValidation);
		}
	}
}
