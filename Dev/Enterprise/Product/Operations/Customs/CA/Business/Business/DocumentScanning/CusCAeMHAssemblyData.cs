using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

[assembly: AssemblyDataProvider(
	typeof(Enterprise.Customs.CA.Business.CusCAeMHAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.CAeManifest)]

namespace Enterprise.Customs.CA.Business
{
	class CusCAeMHAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(CusCAeMHMaster); }
		}

		protected override Type CollectionType
		{
			get { return typeof(CusCAeMHMasterCollection); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.CA.CAHouseBilleManifest; }
		}

		public override MultilingualString HumanReadableName
		{
			get { return ResString.GetMultilingualString("79FC4993-DF52-4B7A-8038-DC0776711ACD", "eManifest Forwarder (CA)"); }
		}

		public override string ReferenceType
		{
			get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; }
		}
	}
}
