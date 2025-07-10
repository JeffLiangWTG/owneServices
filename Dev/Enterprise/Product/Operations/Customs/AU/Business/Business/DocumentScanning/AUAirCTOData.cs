using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(AUAirCTOData),
	Enterprise.Core.Constants.DocManagerCodes.AirCTO,
	Country = "AU")]

namespace Enterprise.Customs.AU.Declaration.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class AUAirCTOData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CTOCusMAWB); } }
		protected override Type CollectionType
		{
			get { return typeof(CTOCusMAWBCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CTOCusMAWBCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Customs.AU.AirCTOImport; } }
		public override string ReferenceType { get { return "ACG"; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("37a9a05b-0cf7-4e0a-87b6-81df7582e67c", "Air CTO - Import"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
