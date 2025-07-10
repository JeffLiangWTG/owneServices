using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocTransactionLineCollection))]
	public class DocTransactionLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocTransactionLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var line = Factory.New<GLJournalLine>();
			return DocTransactionLine.New(line, Factory);
		}

		protected override DocTransactionLineCollection GetCollectionToTest()
		{
			return new DocTransactionLineCollection(Factory);
		}
	}
}
