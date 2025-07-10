using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ARClaimsAndQueriesData),
	Enterprise.Core.Constants.DocManagerCodes.ARClaimsAndQueries)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Accounting.Business.AccQueryClaims;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;
	using Enterprise.ZArchitecture.Schema;

	public class ARClaimsAndQueriesData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ARAccQueryClaim); } }
		protected override Type CollectionType
		{
			get { return typeof(ARAccQueryClaimCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new ARAccQueryClaimCollection(factory);
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams)
		{
			return new ARAccQueryClaimCollection(factory, factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, assemblyDataParams.CompanyCode));
		}

		public override ModuleIdentifier ModuleID { get { return ModuleIDs.ARAccQueryClaim; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("56522469-8734-4404-b79a-e34cddf99f3b", "Receivable Claims and Queries"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
