using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Module
{
	public class APComplianceDocumentFilterStripBusinessObject : ComplianceDocumentFilterStripBusinessObject
	{
		public APComplianceDocumentFilterStripBusinessObject()
		{ }

		public override ZString LedgerType => LedgerTypes.AccountsPayable;
	}
}