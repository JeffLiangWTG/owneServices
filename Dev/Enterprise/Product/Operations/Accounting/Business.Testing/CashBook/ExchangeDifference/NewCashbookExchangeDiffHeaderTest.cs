using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference.Testing
{
	[TestedType(typeof(NewCashbookExchangeDiffHeader))]
	public class NewCashbookExchangeDiffHeaderTest : NonPersistentBusinessObjectTestCase
	{
		#region Test overrides

		public void TestPKSchemaColumn()
		{
			AssertEquals(AccTransactionHeaderSchema.PK, Header.PKSchemaColumn);
		}

		#endregion

		#region Test Properties

		[TestDate(2020, 2, 8)]
		public void TestAH_InvoiceDate()
		{
			var header = Factory.New<NewCashbookExchangeDiffHeader>();
			var newCashbookExchangeDiff = header.NewCashbookExchangeDiffCollection.AddNew();
			header.AH_InvoiceDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, newCashbookExchangeDiff.AH_InvoiceDate);

			header.AH_InvoiceDate = ZDateTime.Now;
			AssertEquals(ZDateTime.Now, newCashbookExchangeDiff.AH_InvoiceDate);
		}

		[TestDate(2020, 2, 8)]
		public void TestAH_PostDate()
		{
			var header = Factory.New<NewCashbookExchangeDiffHeader>();
			var newCashbookExchangeDiff = header.NewCashbookExchangeDiffCollection.AddNew();
			header.AH_PostDate = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, newCashbookExchangeDiff.AH_PostDate);

			header.AH_PostDate = ZDateTime.Now;
			AssertEquals(ZDateTime.Now, newCashbookExchangeDiff.AH_PostDate);
		}

		public void TestAH_Desc()
		{
			var header = Factory.New<NewCashbookExchangeDiffHeader>();
			var newCashbookExchangeDiff = header.NewCashbookExchangeDiffCollection.AddNew();
			header.AH_Desc = ZString.Empty;
			AssertEquals(ZString.Empty, newCashbookExchangeDiff.AH_Desc);

			header.AH_Desc = "Test";
			AssertEquals("Test", newCashbookExchangeDiff.AH_Desc);
		}

		public void TestNewCashbookExchangeDiffCollection()
		{
			AssertEquals(typeof(NewCashbookExchangeDiffCollection), Header.NewCashbookExchangeDiffCollection.GetType());
		}

		#endregion

		#region Test Validate

		public void TestValidateHasAtLeastOneCashbookExchangeDiff()
		{
			var atLeastOneMessage = "Please select at least one bank account for bank currency adjustment.";
			var header = Factory.New<NewCashbookExchangeDiffHeader>();
			var newCashbookExchangeDiff = header.NewCashbookExchangeDiffCollection.AddNew();
			header.RunPreSaveValidation();
			AssertNoRowError(header, atLeastOneMessage);

			newCashbookExchangeDiff.Include = false;
			header.RunPreSaveValidation();
			AssertHasRowError("has row error", header, atLeastOneMessage);
		}

		#endregion

		#region Implementation

		[TestDate(2020, 2, 20)]
		public void TestRefreshDateFromHeader()
		{
			var header = Factory.New<NewCashbookExchangeDiffHeader>();
			header.AH_InvoiceDate = new ZDateTime(2020, 2, 19);
			header.AH_PostDate = new ZDateTime(2020, 2, 18);
			header.AH_Desc = "Test";

			var newCashbookExchangeDiff = header.NewCashbookExchangeDiffCollection.AddNew();
			AssertEquals("invoice date", new ZDateTime(2020, 2, 19), newCashbookExchangeDiff.AH_InvoiceDate);
			AssertEquals("post date", new ZDateTime(2020, 2, 18), newCashbookExchangeDiff.AH_PostDate);
			AssertEquals("description", "Test", newCashbookExchangeDiff.AH_Desc);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Header = Factory.New<NewCashbookExchangeDiffHeader>();
		}

		NewCashbookExchangeDiffHeader Header;

		#endregion
	}
}
