using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(AccCashBasisVAT))]
	class AccCashBasisVATSupportCriticalValidationTest : SupportCriticalValidationTestBase
	{
		protected override IConflictWithCriticalFields GetNewBusinessObject()
		{
			return Factory.New<AccCashBasisVAT>();
		}
	}
}
