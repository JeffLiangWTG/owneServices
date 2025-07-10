using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeaderAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.NctsInBond
	//,Country="GB"  // Use no country code so that it applies to every country, and rtely on the NCTS module being visible to only NCTS countries. 
	)]

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(NctsHeader); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new NctsHeaderCollection(factory, GlbCompany.CurrentCompany);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.EU.NctsMovementModule; }
		}

		public override string ReferenceType
		{
			get { return Constants.ReferenceTypes.SupplyChainLogistics; }
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}

		protected override Type CollectionType
		{
			get { return typeof(NctsHeaderCollection); }
		}

		public override MultilingualString HumanReadableName
		{
			get { return HumanReadableNameShared; }
		}

		public static MultilingualString HumanReadableNameShared
		{
			get { return ResString.GetMultilingualString("12345678-c5eb-457c-9bfd-b876a57efa85", "NCTS Movement Header"); }
		}
	}
}
