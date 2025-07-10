using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(AUImportClassificationData),
	Enterprise.Core.Constants.DocManagerCodes.ImportClassification,
	Country = "AU")]

namespace Enterprise.Customs.AU.Declaration.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class AUImportClassificationData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(Classification); } }
		protected override Type CollectionType
		{
			get { return typeof(ImportClassificationCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new ImportClassificationCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.ImportClassification; } }
		public override string ReferenceType { get { return "CLS"; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("fd01434b-3e3b-448f-8c86-b23f03047277", "Import Classification"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
