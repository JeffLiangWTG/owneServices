using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ComplianceDocumentData),
	Enterprise.Core.Constants.DocManagerCodes.ComplianceDocument)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class ComplianceDocumentData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(ARComplianceDocumentHeader);

		protected override Type CollectionType => typeof(AccComplianceDocumentHeaderCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new AccComplianceDocumentHeaderCollection(factory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.ARComplianceDocument;

		public override string ReferenceType => Core.Constants.ReferenceTypes.Accounting;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("BA53737F-A289-4A4F-8BF1-F3DAD8425F13", "Accounting Compliance Document");

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
