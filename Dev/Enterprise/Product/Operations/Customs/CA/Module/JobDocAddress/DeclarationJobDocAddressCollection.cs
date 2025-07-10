using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Module
{
	public class DeclarationJobDocAddressCollection : JobDocAddressCollection
	{
		public DeclarationJobDocAddressCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			SetFilterBusinessObjectDefaults();
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
			var docAddressQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
			var declarationFilter = new ZDBOnlySubQuery(typeof(JobDeclaration), JobDocAddressSchema.E2_ParentID);
			declarationFilter.AddToFilter(JobDeclarationSchema.JE_MessageType, SQLComparisonOperator.NotEqual, new[] { JobMessageTypeList.Codes.LowValueShipments, JobMessageTypeList.Codes.LVSForConsolidation });
			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbBranchSchema.GB_GC);
			companyQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);
			companyQuery.AddToFilter(GlbCompanySchema.GC_IsActive, true);
			branchQuery.AddSubQuery(companyQuery, JoinCondition.And);
			declarationFilter.AddSubQuery(branchQuery, JoinCondition.And);
			docAddressQuery.AddSubQuery(declarationFilter, JoinCondition.And);
			result.AddToFilter(docAddressQuery);
			return result;
		}

		void SetFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(JobDocAddressesFilterBusinessObject.Schema.AddressDescription, "Property", (ZString)DocAddressTypes.Codes.ImporterPickupDeliveryAddress));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(JobDocAddressesFilterBusinessObject.Schema.Organization, "Property", ZGuid.Empty));
		}
	}
}
