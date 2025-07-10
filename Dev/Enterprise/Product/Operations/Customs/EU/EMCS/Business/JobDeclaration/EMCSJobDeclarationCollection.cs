using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobDeclarationCollection : Customs.Business.BaseJobDeclarationCollection
	{
		public EMCSJobDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
			: base(factory, GetAppCodeQuery(companyPkToFilterOn))
		{
		}

		public static ZDBOnlyQuery GetAppCodeQuery(ZGuid companyPkToFilterOn)
		{
			var query = GetCompanyQuery(companyPkToFilterOn);
			query.AddToFilter(JobDeclarationSchema.JE_ApplicationCode, EMCSJobDeclaration.EMCSApplicationCode);
			return query;
		}

		protected override bool AllowNewCore => false;

		public new EMCSJobDeclaration this[int index] => (EMCSJobDeclaration)Elements[index];

		public new EMCSJobDeclaration AddNew() => (EMCSJobDeclaration)base.AddNew();
	}
}
