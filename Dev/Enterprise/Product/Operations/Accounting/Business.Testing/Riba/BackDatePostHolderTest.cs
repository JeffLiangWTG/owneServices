using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Riba.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(BackDatePostHolder))]
	internal class BackDatePostHolderTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2016, 04, 08)]
		public void TestConstructor()
		{
			var holder = new BackDatePostHolder();
			AssertEquals(new ZDate(2016, 04, 08), holder.BackPostDate);
			AssertEquals(new ZDate(2016, 04, 08), holder.BackInvoiceDate);
		}

		[TestDate(2016, 04, 08)]
		public void TestBackPostDate()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupSinglePeriod(201604, new ZDateTime(2016, 04, 01), new ZDateTime(2016, 04, 30));
			var holder = new BackDatePostHolder();
			holder.BackPostDate = ZDate.Empty;
			holder.ValidateBackPostDate();
			AssertHasErrorContaining(holder.BackPostDateInfo, "Please enter a value.");
			holder.BackPostDate = ZDate.Invalid;
			holder.ValidateBackPostDate();
			AssertHasErrorContaining(holder.BackPostDateInfo, "Enter a valid selection.");
			holder.BackPostDate = new ZDateTime(2016, 04, 09);
			holder.ValidateBackPostDate();
			AssertHasErrorContaining(holder.BackPostDateInfo, "Post date must be equal or less than today.");
			holder.BackPostDate = new ZDateTime(2016, 04, 08);
			holder.ValidateBackPostDate();
			AssertNoErrors(holder.BackPostDateInfo);
		}

		[TestDate(2016, 04, 08)]
		public void TestBackInvoice()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupSinglePeriod(201604, new ZDateTime(2016, 04, 01), new ZDateTime(2016, 04, 30));
			var holder = new BackDatePostHolder();
			holder.BackInvoiceDate = ZDate.Empty;
			holder.ValidateBackInvoiceDate();
			AssertHasErrorContaining(holder.BackInvoiceDateInfo, "Please enter a value.");
			holder.BackInvoiceDate = ZDate.Invalid;
			holder.ValidateBackInvoiceDate();
			AssertHasErrorContaining(holder.BackInvoiceDateInfo, "Enter a valid selection.");
			holder.BackInvoiceDate = new ZDateTime(2016, 04, 09);
			holder.ValidateBackInvoiceDate();
			AssertHasErrorContaining(holder.BackInvoiceDateInfo, "Invoice date must be equal or less than today.");
			holder.BackInvoiceDate = new ZDateTime(2016, 04, 08);
			holder.ValidateBackInvoiceDate();
			AssertNoErrors(holder.BackInvoiceDateInfo);
		}
	}
}
