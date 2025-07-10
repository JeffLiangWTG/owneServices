using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class UPENumberFountainsTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCalloutJobHeaderNumberFountain()
		{
			Db.Connection.BeginTransaction();
			try
			{
				var fountain = UPENumberFountains.Instance.CalloutJobHeaderNumberFountain;
				fountain.SetNext(Factory, 1);
				AssertEquals("1", fountain.GetNextFormatted(Factory));
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		[ExpectNoExceptions]
		public void TestPartPaymentControlNumberFountain()
		{
			Db.Connection.BeginTransaction();
			try
			{
				var fountain = UPENumberFountains.Instance.PartPaymentControlNumberFountain;

				fountain.SetNext(Factory, 1);
				AssertEquals("1", fountain.GetNextFormatted(Factory));
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		[ExpectNoExceptions]
		public void TestRefundControlNumberFountain()
		{
			Db.Connection.BeginTransaction();
			try
			{
				var fountain = UPENumberFountains.Instance.RefundControlNumberFountain;

				fountain.SetNext(Factory, 1);
				AssertEquals("1", fountain.GetNextFormatted(Factory));
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		public void TestBISIEntryPrintBatchNumber()
		{
			Db.Connection.BeginTransaction();
			try
			{
				long firstNumber = UPENumberFountains.Instance.BISIEntryPrintBatchNumber.GetNext(Factory);
				long nextNumber = UPENumberFountains.Instance.BISIEntryPrintBatchNumber.GetNext(Factory);
				AssertEquals("Should start from 1", 1, firstNumber);
				AssertEquals("Next number should be 2", 2, nextNumber);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}
	}
}
