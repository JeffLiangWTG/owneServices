using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBCollectionWithOneBill : DependentBusinessObjectCollection<CusHAWB, CusMAWB>
	{
		public CusHAWBCollectionWithOneBill(CusMAWB parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		public CusHAWB HouseBill
		{
			get { return fHouseBill; }
			set
			{
				fHouseBill = value;
				this.Load();
			}
		}

		#region Implementation
		protected CusHAWB fHouseBill;

		protected override ZQuery CreateAdditionalFilter()
		{
			return HouseBill != null ? new ZQuery(CusHAWBSchema.PK, HouseBill.PK) : new ZQuery();
		}

		#endregion

	}
}
