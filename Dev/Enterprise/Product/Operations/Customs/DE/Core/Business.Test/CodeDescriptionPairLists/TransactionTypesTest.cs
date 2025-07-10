using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class TransactionTypesTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestList()
		{
			NUnit.Framework.Assert.That(new TransactionTypes().CodesAsString, Is.EqualTo("ADJ, OBL, TRN, STA"));
		}
	}
}
