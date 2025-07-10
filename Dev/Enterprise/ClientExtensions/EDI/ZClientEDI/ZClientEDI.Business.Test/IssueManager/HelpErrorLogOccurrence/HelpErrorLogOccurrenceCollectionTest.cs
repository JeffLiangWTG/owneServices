using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	[TestedType(typeof(HelpErrorLogOccurrenceCollection))]
	class HelpErrorLogOccurrenceCollectionTest : BusinessObjectCollectionTestCase
	{
		EdiHelpErrorLog master;

		protected new HelpErrorLogOccurrenceCollection Collection
		{
			get { return (HelpErrorLogOccurrenceCollection)base.Collection; }
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new HelpErrorLogOccurrenceCollection(Master);
		}

		public void TestForeignPK()
		{
			AssertEquals("AddNew().HO_HE", Master.PK, Collection.AddNew().HO_HE);
		}

		public void TestToString()
		{
			AssertEquals("ToString()", "0 Occurrences", Collection.ToString());

			Collection.AddNew();
			AssertEquals("ToString()", "1 Occurrence", Collection.ToString());

			Collection.AddNew();
			AssertEquals("ToString()", "2 Occurrences", Collection.ToString());
		}

		EdiHelpErrorLog Master
		{
			get { return master ?? (master = Factory.New<EdiHelpErrorLog>()); }
		}
	}
}
