using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing.Universal
{
	[TestedType(typeof(BatchNumberDataContextManager))]
	class BatchNumberDataContextManagerTestClass : DataContextManagerTestCase<BatchNumberDataContextManager, GenExportBatchSequence>
	{
	}
}
