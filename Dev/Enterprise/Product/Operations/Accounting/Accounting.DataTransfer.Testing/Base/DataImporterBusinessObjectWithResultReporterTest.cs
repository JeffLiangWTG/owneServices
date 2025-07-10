using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile.Testing
{
	[TestedType(typeof(DataImporterBusinessObjectWithResultReporter))]
	public class DataImporterBusinessObjectWithResultReporterTest : DataImporterBusinessObjectTest
	{
		public void TestMessageOverrides()
		{
			DataImporterBusinessObjectWithResultReporter importerBusinessObject = (DataImporterBusinessObjectWithResultReporter)GetNewBusinessObject();
			IDataTransferResultReporterForTest importResultReporter = new IDataTransferResultReporterForTest();
			importerBusinessObject.DataTransferResultReporter = importResultReporter;

			importResultReporter.WasTheLastDataTransferSuccessful = false;
			AssertEquals("ProgressMessageForFatalError_ForTestOnly must be as fatal error message.", importerBusinessObject.ProgressMessageForFatalError_ForTestOnlyBase_ForTestOnly, importerBusinessObject.ProgressMessageForFatalError_ForTestOnly);

			importResultReporter.WasTheLastDataTransferSuccessful = true;
			AssertEquals("ProgressMessageForFatalError_ForTestOnly must be as successful message.", importerBusinessObject.ProgressMessageForSuccessfulImport_ForTestOnly, importerBusinessObject.ProgressMessageForFatalError_ForTestOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DataImporterBusinessObjectWithResultReporter(Factory);
		}

		class IDataTransferResultReporterForTest : IDataTransferResultReporter
		{
			#region IDataTransferResultReporter Members

			public bool WasTheLastDataTransferSuccessful
			{
				get;
				set;
			}

			#endregion
		}
	}
}
