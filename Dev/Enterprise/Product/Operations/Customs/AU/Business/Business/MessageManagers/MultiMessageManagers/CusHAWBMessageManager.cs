using System.Collections;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBMessageManager : MultiMessageManager
	{
		public CusHAWBMessageManager(GetCusHAWBDelegate getHAWBDelegate)
			: base()
		{
			this.getHAWBDelegate = getHAWBDelegate;
		}

		public delegate CusHAWB GetCusHAWBDelegate();
		public CusHAWB HAWB
		{
			get { return getHAWBDelegate == null ? null : getHAWBDelegate(); }
		}
		readonly GetCusHAWBDelegate getHAWBDelegate;

		public override Customs.Business.IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return HAWB; }
		}

		#region Implementation

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return false; }
		}

		protected override Customs.Business.SingleMessageManager[] GetAllMessageManagers()
		{
			ArrayList result = new ArrayList();
			if (HAWB != null)
			{
				result.Add(new CusHAWBAIRCRMessageManager(HAWB));
				foreach (CusUnderbond underbond in ((ICusUnderbondUnionCollectionParent)HAWB).AllUnderbonds)
				{
					result.Add(new CusUnderbondUBMREQManager(underbond));
				}
				foreach (CusUnderbond underbond in ((ICusUnderbondUnionCollectionParent)HAWB).AllUnderbonds)
				{
					if (underbond.CanDoOutturn)
					{
						result.Add(new CusUnderbondAIROUTManager(underbond));
					}
				}
			}

			return (Customs.Business.SingleMessageManager[])result.ToArray(typeof(Customs.Business.SingleMessageManager));
		}

		#endregion
	}
}
