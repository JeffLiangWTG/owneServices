using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(APClaimsAndQueriesData),
	Enterprise.Core.Constants.DocManagerCodes.APClaimsAndQueries)]

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

	public class APClaimsAndQueriesData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(APAccQueryClaim); } }
		protected override Type CollectionType
		{
			get { return typeof(APAccQueryClaimCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new APAccQueryClaimCollection(factory);
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams)
		{
			return new APAccQueryClaimCollection(factory, factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, assemblyDataParams.CompanyCode));
		}

		public override ModuleIdentifier ModuleID { get { return ModuleIDs.APAccQueryClaim; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("c5b7cf1a-176a-4b67-a109-7e40572e238e", "Payable Claims and Queries"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
