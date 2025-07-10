using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APComplianceDocumentFilterStripBusinessObject))]
	public class APComplianceDocumentFilterStripBusinessObjecTest : ComplianceDocumentFilterStripBusinessObjectTest
	{
		protected override ZString LedgerType => LedgerTypes.AccountsPayable;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new APComplianceDocumentFilterStripBusinessObject();
		}

		protected override AccComplianceDocumentHeader CreateNewComplianceDocumentHeader(BusinessObjectFactory factory)
		{
			return Factory.NewWithValidTestData<APComplianceDocumentHeader>();
		}
	}
}
