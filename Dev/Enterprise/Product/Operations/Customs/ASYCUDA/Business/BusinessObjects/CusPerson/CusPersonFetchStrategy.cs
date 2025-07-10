using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class CusPersonFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusPersonFetchStrategy(CusPerson person)
			: base(person)
		{
		}

		new protected CusPerson BusinessObject => (CusPerson)base.BusinessObject;

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();
			Factory.AddFetchHint(typeof(CusPersonCountry), CusPersonCountrySchema.CPC_CPN_Person, BusinessObject.PK);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(typeof(CusPersonCountry), CusPersonCountrySchema.CPC_CPN_Person, BusinessObject.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(typeof(GlbPerson), GlbPersonSchema.PK, BusinessObject.CPN_PER_Person);
		}
	}
}
