using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CreditControlledDocumentsApprovalDataForDocumentScanning),
	Enterprise.Core.Constants.DocManagerCodes.CreditControlledDocumentsApproval)]

namespace Enterprise.Accounting.Business
{
	class CreditControlledDocumentsApprovalDataForDocumentScanning : AssemblyData
	{
		public override Type BusinessObjectType => typeof(CreditControlledDocumentsApproval);

		public override string ReferenceType => Core.Constants.ReferenceTypes.Accounting;

		protected override Type CollectionType => typeof(CreditControlledDocumentsApprovalCollection);

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("f108b290-87b4-4c3a-b77f-eba5f88e7b0d", "Credit Controlled Documents Approval");

		public override ModuleIdentifier ModuleID => ModuleIDs.CreditControlledDocumentsApproval;

		public override bool IsAllowedForUnallocatedeDocs => true;

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new CreditControlledDocumentsApprovalCollection(factory);
	}
}
