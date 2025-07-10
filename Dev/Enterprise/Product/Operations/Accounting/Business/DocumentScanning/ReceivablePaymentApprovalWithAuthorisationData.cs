using System;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ReceivablePaymentApprovalWithAuthorisationData),
	Enterprise.Core.Constants.DocManagerCodes.ReceivablePaymentApprovalWithAuthorisation)]

namespace Enterprise.Accounting.Business
{
	public class ReceivablePaymentApprovalWithAuthorisationData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ARPaymentApprovalWithAuthorisation); } }
		protected override Type CollectionType => null;
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.ARPaymentProcessing; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("382b135d-4332-4ea7-8e92-aa31acb56efe", "AR Payment Approval"); } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new PaymentApprovalEDocsViaUniversalXmlSupport<ARPaymentApprovalWithAuthorisation>(LedgerTypes.AccountsReceivable);
	}
}
