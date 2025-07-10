using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAHouseMessageManager : SeaCargoMessageManager
	{
		public CusSCAHouseMessageManager(CusSCAHouse house)
			: this(house, ZString.Empty)
		{
		}

		public CusSCAHouseMessageManager(CusSCAHouse house, ZString ownerCompanyABN)
			: base(house?.OceanBill, ownerCompanyABN)
		{
			HouseBill = house;
		}

		public override IMessageManageableBizObj TopLevelBizObjToManage => OceanBill;

		public CusSCAHouse HouseBill
		{
			get => fHouse;
			set
			{
				fHouse = value;
				OceanBill = value?.OceanBill;
			}
		}
		CusSCAHouse fHouse;

		#region Implementation

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			var result = new List<SingleMessageManager>();

			if (HouseBill is CusSCAHouse cmrHouseBill)
			{
				result.Add(new CusSCAHouseSEACRManager(cmrHouseBill));
				foreach (CusUnderbond underbond in ((ICusUnderbondUnionCollectionParent)HouseBill).AllUnderbonds)
				{
					result.Add(new CusUnderbondUBMREQManager(underbond));
				}
			}

			return result.ToArray();
		}

		#endregion
	}
}
