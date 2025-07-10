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
	[TestedType(typeof(ARJournalCollection))]
	public class ARJournalCollectionTest : ActiveBusinessObjectCollectionTestCase<ARJournalCollection>
	{
		ARMatchingBase arMatchingBase;

		protected override void SetUp()
		{
			base.SetUp();
			arMatchingBase = new ARMatchingBase(Factory);
		}

		protected override ARJournalCollection GetCollectionToTest()
		{
			return new ARJournalCollection(Factory, arMatchingBase, new AdhocCollectionRelationship(typeof(ARJournal)));
		}

		public void TestShowGLAccountsForImportAction()
		{
			var collection = GetCollectionToTest();
			collection.ShowGLAccountsForImportAction = (glHeaderCollection, glHeaderList) => { glHeaderList.Add(Factory.New<AccGLHeader>()); };
			var line = collection.AddNew();
			AssertNotNull(line.ShowGLAccountsForImportAction);
			AssertEquals(collection.ShowGLAccountsForImportAction, line.ShowGLAccountsForImportAction);
		}

		public void TestSetDefaultsForNewElementCore()
		{
			arMatchingBase.MatchDate = ZDateTime.BrettsBirthday;
			ZGuid orgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			arMatchingBase.PrimaryOrganization = orgPK;

			ARJournal arJournal = Collection.AddNew();

			AssertEquals(ZDateTime.BrettsBirthday, arJournal.AH_InvoiceDate);
			AssertEquals(ZDateTime.BrettsBirthday, arJournal.AH_PostDate);
			AssertEquals(orgPK, arJournal.AH_OH);
			AssertEquals((Guid)AccountingConfigurationRegistry.Instance.ARJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), arJournal.AH_AG);

			arMatchingBase.MatchDate = ZDateTime.Today.AddDays(1);
			arJournal = Collection.AddNew();
			AssertEquals(ZDateTime.Today.AddDays(1), arJournal.AH_InvoiceDate);
			AssertEquals(ZDateTime.Today, arJournal.AH_PostDate);
		}

		public void TestSetUseJournalValidation()
		{
			ARJournal arJournal1 = Collection.AddNew();
			ARJournal arJournal2 = Collection.AddNew();

			Assert(!arJournal1.UseJournalValidation);
			Assert(!arJournal2.UseJournalValidation);

			Collection.SetUseJournalValidation(true);

			Assert(arJournal1.UseJournalValidation);
			Assert(arJournal2.UseJournalValidation);

			Collection.SetUseJournalValidation(false);

			Assert(!arJournal1.UseJournalValidation);
			Assert(!arJournal2.UseJournalValidation);
		}
	}
}
