using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class InvoiceCriticalValidationNonTransactionalTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestSaveAsIncompleteAfterCriticalValidationLogs()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);
			var invoice = creator.CreateInvoiceWithLine(typeof(APInvoice), "APInvoice001", creator.AUD, 1, 100, 0, 100, 0);
			invoice.Lines[0].AL_AC = creator.FRT.PK;
			invoice.IsCancelled = true;     //this causes critical validation

			var criticalValidationRaised = false;

			try
			{
				factory.Save();
			}
			catch (OnSavingCriticalCheckException<AccTransactionHeader>)
			{
				criticalValidationRaised = true;
			}

			Assert("Precondition: critical validation should have been triggeret so that StmALog was created prior to save failure", criticalValidationRaised);
			Assert(!invoice.IsInDatabase);

			invoice.SaveAsIncomplete();
			AssertEquals("IN|INI|Saved as Incomplete", invoice.Logs.AutoCreatedLog.SL_Reference);

			ExceptionReporterTestListener.Instance.Clear();
		}
	}
}