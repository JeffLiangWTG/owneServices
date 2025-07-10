using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	[ModuleID(ModuleId.CusAuthorisations)]
	public class CusAuthorisationHeaderCollectionFiltered : CusAuthorisationHeaderCollection
	{
		public CusAuthorisationHeaderCollectionFiltered(BusinessObjectFactory factory, ZString agcCode, ZGuid agcOwner) : base(factory)
		{
			this.owner = agcOwner;
			this.code = agcCode;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			if (!code.IsEmpty)
			{
				query.AddToFilter(CusPermitHeaderSchema.CPH_Type, code);
			}

			if (!owner.IsEmpty)
			{
				query.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, owner);
			}

			return query;
		}

		protected override bool AllowNew => false;

		readonly ZGuid owner;
		readonly ZString code;
	}
}
