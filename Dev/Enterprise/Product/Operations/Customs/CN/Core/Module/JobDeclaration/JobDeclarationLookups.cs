using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Module
{
	public class JobDeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		OrgHeaderCollection buyerOrgHeaderCollection;
		public OrgHeaderCollection Buyer
		{
			get
			{
				if (buyerOrgHeaderCollection == null)
				{
					buyerOrgHeaderCollection = new OrgHeaderCollection(Factory);
				}
				return buyerOrgHeaderCollection;
			}
		}

		OrgHeaderCollection manufacturerOrgHeaderCollection;
		public OrgHeaderCollection Manufacturer
		{
			get
			{
				if (manufacturerOrgHeaderCollection == null)
				{
					manufacturerOrgHeaderCollection = new OrgHeaderCollection(Factory);
				}
				return manufacturerOrgHeaderCollection;
			}
		}

		public CodeDescriptionPairList EntryInstructionCPCList => CNRefCusProcedure.GetRefCusProcedureList(Factory);

		public override CodeDescriptionPairList MessageSubTypeList() => DecTypeList.GetDecTypeList(Factory);

		public ZZRefCusCodeListCombinedCollection CustomsOfficeList => CNRefCusCodeListTypes.GetCustomsOfficeList(Factory, ZDateTime.Today);
	}
}
