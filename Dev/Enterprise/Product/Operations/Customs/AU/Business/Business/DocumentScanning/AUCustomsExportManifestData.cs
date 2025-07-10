using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(AUCustomsExportManifestData),
	Enterprise.Core.Constants.DocManagerCodes.CustomsExportManifest,
	Country = "AU")]

namespace Enterprise.Customs.AU.Declaration.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class AUCustomsExportManifestData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ExportCustomsManifestHeader); } }
		protected override Type CollectionType
		{
			get { return typeof(ExportCustomsManifestHeaderCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new ExportCustomsManifestHeaderCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Customs.AU.ExportCustomsManifest; } }
		public override string ReferenceType { get { return "CEM"; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("c5cd951a-fce2-4944-b106-9ed9e6f6cc6a", "Customs Export Manifest"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
