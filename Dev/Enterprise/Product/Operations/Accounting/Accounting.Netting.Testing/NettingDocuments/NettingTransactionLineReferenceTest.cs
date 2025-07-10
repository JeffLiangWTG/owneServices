using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Netting.Testing
{
	[TestedType(typeof(NettingTransactionLineReference))]
	public class NettingTransactionLineReferenceTest : NonPersistentBusinessObjectTestCase
	{
		//*********************************************************************
		//NettingTransactionLineReference is tested in \DocumentWrappers\Accounting\Netting\DocNettingTransactionTest
		//NettingTransactionLineReference is tested in \DocumentWrappers\Accounting\Netting\DocNettingClearingJournalTest
		//*********************************************************************
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NettingTransactionLineReference();
		}
	}
}
