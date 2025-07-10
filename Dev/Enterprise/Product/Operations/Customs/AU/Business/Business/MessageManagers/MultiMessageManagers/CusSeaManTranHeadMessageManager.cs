using System.Collections;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManTranHeadMessageManager : Customs.Business.MultiMessageManager
	{
		public CusSeaManTranHeadMessageManager(CusSeaManTranHead transportHeader)
			: base()
		{
			this.transportHeader = transportHeader;
		}

		#region Implementation

		public override Customs.Business.IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return transportHeader; }
		}

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return false; }
		}

		protected override Customs.Business.SingleMessageManager[] GetAllMessageManagers()
		{
			ArrayList result = new ArrayList();
			result.Add(new CusSeaManTranHeadSEAIARManager(transportHeader));
			foreach (CusSeaManArrivalPort arrival in transportHeader.Arrivals)
			{
				result.Add(new CusSeaManArrivalPortSEAAARManager(arrival));
			}
			foreach (CusSeaManArrivalPort arrival in transportHeader.Arrivals)
			{
				if (arrival.GetCargoListDetails().Length > 0)
				{
					result.Add(new CusSeaManArrivalPortCARLSTManager(arrival));
				}
			}
			foreach (CusSeaManOBLHeader oceanBill in transportHeader.OceanBills)
			{
				if (oceanBill.BO_HeaderCargoType == CMRImportCargoCodes.Codes.Import)
				{
					result.Add(new CusSeaManOBLHeaderSEACRManager(oceanBill));
				}
			}
			foreach (CusUnderbond underbond in transportHeader.AllUnderbonds)
			{
				result.Add(new CusUnderbondUBMREQManager(underbond));
			}
			return (Customs.Business.SingleMessageManager[])result.ToArray(typeof(Customs.Business.SingleMessageManager));
		}
		readonly CusSeaManTranHead transportHeader;

		protected override bool CheckTopLevelBusinessObjectsChildren
		{
			get { return false; }
		}

		#endregion
	}
}
