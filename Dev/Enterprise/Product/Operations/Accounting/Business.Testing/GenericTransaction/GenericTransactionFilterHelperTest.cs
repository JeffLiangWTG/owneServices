using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GenericTransaction
{
	public class GenericTransactionFilterHelperTest : TestCaseWithFactory
	{
		public void TestParameterisedText()
		{
			ZQuery filter = new ZQuery();
			GenericTransactionFilterHelper filterHelper = new GenericTransactionFilterHelper(filter);
			ZString parametersList = GlbCompany.CurrentCompany.PK.ToGuid().ToString();
			ZString expectedText = "SELECT * FROM GetGenericTransactions('" + parametersList + "')";
			AssertEquals("Parameterised Text", expectedText, filterHelper.ParameterisedText);
			filter.MaximumRows = 100;
			filterHelper = new GenericTransactionFilterHelper(filter);
			expectedText = "SELECT TOP 100 * FROM GetGenericTransactions('" + parametersList + "')";
			AssertEquals("Parameterised Text", expectedText, filterHelper.ParameterisedText);
		}

		public void TestMaximumRows()
		{
			ZQuery filter = new ZQuery();
			GenericTransactionFilterHelper filterHelper = new GenericTransactionFilterHelper(filter);
			AssertNull("MaximumRows", filterHelper.MaximumRows);
			filter.MaximumRows = 10;
			filterHelper = new GenericTransactionFilterHelper(filter);
			AssertEquals("MaximumRows", 10, filterHelper.MaximumRows);
		}

		public void TestObserveMaximumRows()
		{
			ZQuery filter = new ZQuery();
			filter.MaximumRows = 1000;
			GenericTransactionFilterHelper helper1 = new GenericTransactionFilterHelper(filter, true);
			AssertContains(" TOP ", helper1.ParameterisedText);
			GenericTransactionFilterHelper helper2 = new GenericTransactionFilterHelper(filter, false);
			AssertNotContains(" TOP ", helper2.ParameterisedText);
		}
	}
}