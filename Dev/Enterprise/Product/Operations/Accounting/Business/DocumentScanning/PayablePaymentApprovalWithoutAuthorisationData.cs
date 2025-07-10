using System;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(PayablePaymentApprovalWithoutAuthorisationData),
	Enterprise.Core.Constants.DocManagerCodes.PayablePaymentApprovalWithoutAuthorisation)]

namespace Enterprise.Accounting.Business
{
	public class PayablePaymentApprovalWithoutAuthorisationData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(APPaymentApprovalWithoutAuthorisation); } }
		protected override Type CollectionType => null;
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.APPaymentProcessing; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("1c869be2-cdbb-4552-a492-22df95fa009a", "AP Payment Approval"); } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new PaymentApprovalEDocsViaUniversalXmlSupport<APPaymentApprovalWithoutAuthorisation>(LedgerTypes.AccountsPayable);
	}
}
