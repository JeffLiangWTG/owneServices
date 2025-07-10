using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class TypeSafeJobDeclarationCollection : Customs.Business.BaseJobDeclarationCollection
	{
		protected TypeSafeJobDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
			: base(factory, GetAppCodeQuery(companyPkToFilterOn))
		{
		}

		static ZDBOnlyQuery GetAppCodeQuery(ZGuid companyPkToFilterOn)
		{
			var query = GetCompanyQuery(companyPkToFilterOn);
			query.AddToFilter(JobDeclarationSchema.JE_ApplicationCode, SQLComparisonOperator.NotEqual, Enterprise.Customs.Business.BaseJobDeclarationTypeDecider.EMCSApplicationCode);
			return query;
		}

		public new JobDeclaration this[int index] => (JobDeclaration)Elements[index];

		public new JobDeclaration AddNew() => (JobDeclaration)base.AddNew();
	}
}
