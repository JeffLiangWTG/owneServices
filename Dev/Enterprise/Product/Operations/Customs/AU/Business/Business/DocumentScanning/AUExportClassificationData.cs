using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(AUExportClassificationData),
	Enterprise.Core.Constants.DocManagerCodes.ExportClassification,
	Country = "AU")]

namespace Enterprise.Customs.AU.Declaration.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class AUExportClassificationData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(Classification); } }
		protected override Type CollectionType
		{
			get { return typeof(ExportClassificationCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new ExportClassificationCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.ExportClassification; } }
		public override string ReferenceType { get { return "CLS"; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("715b9a5b-fb14-495d-89fc-21219d8ec1ff", "Export Classification"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
