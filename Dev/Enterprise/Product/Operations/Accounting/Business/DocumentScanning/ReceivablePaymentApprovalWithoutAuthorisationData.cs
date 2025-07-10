using System;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ReceivablePaymentApprovalWithoutAuthorisationData),
	Enterprise.Core.Constants.DocManagerCodes.ReceivablePaymentApprovalWithoutAuthorisation)]

namespace Enterprise.Accounting.Business
{
	public class ReceivablePaymentApprovalWithoutAuthorisationData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ARPaymentApprovalWithoutAuthorisation); } }
		protected override Type CollectionType => null;
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.ARPaymentProcessing; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("8f8b867e-c84a-4f73-9e99-ddea4568a815", "AR Payment Approval"); } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new PaymentApprovalEDocsViaUniversalXmlSupport<ARPaymentApprovalWithoutAuthorisation>(LedgerTypes.AccountsReceivable);
	}
}
