using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ErrorReporting.Business.Test
{
	[TestedType(typeof(StmErrorReportCollection))]
	sealed class StmErrorReportCollectionTest : ActiveBusinessObjectCollectionTestCase<StmErrorReportCollection>
	{
	}
}
