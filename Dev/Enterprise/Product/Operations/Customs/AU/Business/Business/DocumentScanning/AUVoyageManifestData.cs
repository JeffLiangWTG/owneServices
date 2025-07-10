using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(AUVoyageManifestData),
	Enterprise.Core.Constants.DocManagerCodes.VoyageManifest,
	Country = "AU")]

namespace Enterprise.Customs.AU.Declaration.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	class AUVoyageManifestData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CusSeaManTranHead); } }
		protected override Type CollectionType
		{
			get { return typeof(CusSeaManTranHeadCollection); }
		}
		public override string ReferenceType { get { return "CIM"; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("1aed7a90-0a54-41a0-9351-b2342845d464", "Customs Import Manifest"); } }
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Customs.AU.VoyageManifest; } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CusSeaManTranHeadCollection(factory);
		}
	}
}
