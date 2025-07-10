using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(AUPacksConversionData),
	Enterprise.Core.Constants.DocManagerCodes.PacksConversion,
	Country = "AU")]

namespace Enterprise.Customs.AU.Declaration.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	public class AUPacksConversionData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(BaseRefPacks); } }
		protected override Type CollectionType
		{
			get { return typeof(BaseRefPacksCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new BaseRefPacksCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Customs.RefPacks; } }
		public override string ReferenceType { get { return "PAC"; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("079d3bd8-dc66-4f93-8d25-3a21fb547fa8", "Packs Conversion"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
