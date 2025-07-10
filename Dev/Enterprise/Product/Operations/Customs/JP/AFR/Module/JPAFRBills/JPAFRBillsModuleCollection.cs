using CargoWise.EntityFramework;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Module
{
	public class JPAFRBillsModuleCollection : BusinessObjectCollection<JPAFRBills>
	{
		public JPAFRBillsModuleCollection(BusinessObjectFactory factory)
			: base(factory, GetBillFilter())
		{
		}

		static ZQuery GetBillFilter()
		{
			var result = new ZDBOnlyQuery(typeof(JPAFRBills));
			var headerQuery = new ZDBOnlySubQuery(typeof(JPAFRHeader), JPAFRHeaderSchema.PK);
			result.AddSubQuery(JPAFRBillsSchema.JPB_JPH_Header, headerQuery, JoinCondition.And);
			return result;
		}
	}
}
