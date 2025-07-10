using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocBankAccountCollection))]
	public class DocBankAccountCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocBankAccountCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var account = Factory.New<AccBankAccount>();
			return DocBankAccount.New(account, Factory);
		}

		protected override DocBankAccountCollection GetCollectionToTest()
		{
			return new DocBankAccountCollection(Factory);
		}
	}
}
