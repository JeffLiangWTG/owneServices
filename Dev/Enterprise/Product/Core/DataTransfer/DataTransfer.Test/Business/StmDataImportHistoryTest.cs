using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	[TestedType(typeof(StmDataImportHistory))]
	sealed class StmDataImportHistoryTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.NewWithValidTestData<StmDataImportHistory>();
	}
}
