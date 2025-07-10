using System.Collections;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusMAWBMessageManager : MultiMessageManager
	{
		public CTOCusMAWBMessageManager(GetCTOCusMAWBDelegate getHAWBDelegate)
		{
			this.getHAWBDelegate = getHAWBDelegate;
		}

		#region Implementation

		public override Customs.Business.IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return MAWB; }
		}

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return false; }
		}

		protected override Customs.Business.SingleMessageManager[] GetAllMessageManagers()
		{
			ArrayList result = new ArrayList();
			foreach (CTOCusHAWB hAWB in MAWB.ChildBills)
			{
				result.Add(new CTOCusHAWBAIRCRMessageManager(hAWB));
				foreach (CusPartShip partShip in hAWB.PartShips)
				{
					result.Add(new CusPartShipAIRCRManager(partShip));
				}
			}
			foreach (CusUnderbond underbond in ((ICusUnderbondUnionCollectionParent)MAWB).AllUnderbonds)
			{
				result.Add(new CusUnderbondUBMREQManager(underbond));
			}
			result.Add(new CusUnderbondAIROUTManager(MAWB.FakeFlightOuturnUnderbond));
			return (Customs.Business.SingleMessageManager[])result.ToArray(typeof(Customs.Business.SingleMessageManager));
		}

		public delegate CTOCusMAWB GetCTOCusMAWBDelegate();
		public CTOCusMAWB MAWB
		{
			get { return getHAWBDelegate == null ? null : getHAWBDelegate(); }
		}
		readonly GetCTOCusMAWBDelegate getHAWBDelegate;

		#endregion
	}
}
