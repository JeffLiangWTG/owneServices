using System;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(PayablePaymentApprovalWithAuthorisationData),
	Enterprise.Core.Constants.DocManagerCodes.PayablePaymentApprovalWithAuthorisation)]

namespace Enterprise.Accounting.Business
{
	public class PayablePaymentApprovalWithAuthorisationData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(APPaymentApprovalWithAuthorisation); } }
		protected override Type CollectionType => null;
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.APPaymentProcessing; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("4edcd3dc-6e4a-44fc-adca-557ad15f65fb", "AP Payment Approval"); } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new PaymentApprovalEDocsViaUniversalXmlSupport<APPaymentApprovalWithAuthorisation>(LedgerTypes.AccountsPayable);
	}
}
