using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectPayment.Testing
{
	[TestedType(typeof(DirectPaymentCollection))]
	public class DirectPaymentCollectionTest : ActiveBusinessObjectCollectionTestCase<DirectPaymentCollection>
	{
		#region TestRelationshipFilter

		public void TestRelationshipFilter()
		{
			DirectPayment validPayment = Factory.New<DirectPayment>();
			validPayment.AH_TransactionType = ZArchitecture.Core.TransactionTypes.DirectPayment;
			validPayment.AH_Ledger = ZArchitecture.Core.LedgerTypes.CashBook;

			DirectPayment invalidPayment = Factory.New<DirectPayment>();
			invalidPayment.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Payment;
			invalidPayment.AH_Ledger = ZArchitecture.Core.LedgerTypes.CashBook;

			AssertCollectionContains(validPayment, Collection);

			AssertCollectionNotContains(invalidPayment, Collection);
		}

		#endregion

		#region TestFindBoxProviderAlwaysAppliesRelationshipFilter

		/// <summary>
		/// Needed because DocScanning will try to load the first matching AccTransactionHeader
		/// when allocating eDocs (the Code field on AccTransactionHeader is not unique).
		/// </summary>
		public void TestFindBoxProviderAlwaysAppliesRelationshipFilter()
		{
			PropertyInfo info = Collection.GetType().GetProperty("FindBoxListProvider", BindingFlags.NonPublic | BindingFlags.Instance);
			IFindBoxListProvider listProvider = (IFindBoxListProvider)info.GetValue(Collection, null);

			DirectPayment invalidPayment = Factory.New<DirectPayment>();
			invalidPayment.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Payment;
			invalidPayment.AH_Ledger = ZArchitecture.Core.LedgerTypes.CashBook;
			invalidPayment.AH_TransactionNum = "1000";
			AssertNull(listProvider.GetBusinessObjectFromCode("1000"));

			DirectPayment validPayment = Factory.New<DirectPayment>();
			validPayment.AH_TransactionType = ZArchitecture.Core.TransactionTypes.DirectPayment;
			validPayment.AH_Ledger = ZArchitecture.Core.LedgerTypes.CashBook;
			validPayment.AH_TransactionNum = "1000";
			AssertEquals(validPayment, listProvider.GetBusinessObjectFromCode("1000"));
		}

		#endregion
	}
}
