using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ComplianceSequenceData),
	Enterprise.Core.Constants.DocManagerCodes.ComplianceSequence)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class ComplianceSequenceData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(AccComplianceSequence); } }
		protected override Type CollectionType
		{
			get { return typeof(AccComplianceSequenceCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new AccComplianceSequenceCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.AccComplianceSequence; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("516e001d-babf-46a4-a944-04fc507fc422", "Compliance Sequence"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
