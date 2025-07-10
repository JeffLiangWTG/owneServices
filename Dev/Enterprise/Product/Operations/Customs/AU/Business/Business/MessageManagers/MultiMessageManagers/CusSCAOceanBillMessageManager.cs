using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAOceanBillMessageManager : SeaCargoMessageManager
	{
		public CusSCAOceanBillMessageManager(CusSCAOceanBill oceanBill)
			: this(oceanBill, ZString.Empty)
		{
		}

		public CusSCAOceanBillMessageManager(CusSCAOceanBill oceanBill, ZString ownerCompanyABN)
			: base(oceanBill, ownerCompanyABN)
		{
		}

		public override IMessageManageableBizObj TopLevelBizObjToManage => OceanBill;

		public OrgHeader ResponsibleParty
		{
			get => fResponsibleParty ?? (fResponsibleParty = GlbCompany.CurrentCompany.OrgProxy);
			set => fResponsibleParty = value;
		}
		OrgHeader fResponsibleParty;

		#region Implementation

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			var result = new List<SingleMessageManager>();

			if (OceanBill is CusSCAOceanBill cmrOceanBill)
			{
				foreach (CusSCAHouse house in cmrOceanBill.HouseBills)
				{
					result.Add(new CusSCAHouseSEACRManager(house, OwnerCompanyABN));
				}
				foreach (CusUnderbond underbond in ((ICusUnderbondUnionCollectionParent)cmrOceanBill).AllUnderbonds)
				{
					result.Add(new CusUnderbondUBMREQManager(underbond, OwnerCompanyABN));
				}
			}

			return result.ToArray();
		}

		protected override void OnMessagesSending(SingleMessageManager[] singleMessageManagers)
		{
			singleMessageManagers.Select(x => x.BusinessObject.PK).Batch(PreloadBatchSize).ForEach(group =>
			{
				Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, group));
			});
		}

		protected virtual int PreloadBatchSize => 5000;

		#endregion
	}
}
