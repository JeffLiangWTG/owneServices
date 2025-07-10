//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccComplianceDocumentHeaderLookups
//
//    This class should be used for overriding collections in AutoAccComplianceDocumentHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class AccComplianceDocumentHeaderLookups : AutoAccComplianceDocumentHeaderLookups
	{
		public AccComplianceDocumentHeaderLookups(AutoAccComplianceDocumentHeader parent) : base(parent)
		{
		}

		public virtual RefCurrencyCollection TransactionCurrencies => new RefCurrencyCollection(Factory);

		public virtual AccComplianceSequenceCollection ComplianceSequences => new AccComplianceSequenceCollection(Factory);

		public virtual OrgHeaderCollection Headers => new OrgHeaderCollection(Factory);

		public static ICodeDescriptionPairList GetSupportingReasonCodesList(ZString ledger)
		{
			if (ledger == LedgerTypes.AccountsPayable)
			{
				return AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingReasonsPayables.Value.GetCodeDescriptionPairList();
			}
			else
			{
				return AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingReasonsReceivables.Value.GetCodeDescriptionPairList();
			}
		}

		public static ICodeDescriptionPairList SupportingDocumentTypeList => AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingDocumentType.Value.GetCodeDescriptionPairList();
	}
}
