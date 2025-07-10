using System.Collections;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondMessageManager : MultiMessageManager
	{
		public CusUnderbondMessageManager(CusUnderbond masterBusinessObject) : base()
		{
			CusUnderbond = masterBusinessObject;
		}

		public readonly CusUnderbond CusUnderbond;

		public override Customs.Business.IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return CusUnderbond; }
		}

		#region Implementation

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return false; }
		}

		protected override Customs.Business.SingleMessageManager[] GetAllMessageManagers()
		{
			ArrayList result = new ArrayList();
			if (CusUnderbond != null && CusUnderbond.CanDoOutturn)
			{
				result.Add(new CusUnderbondAIROUTManager(CusUnderbond));
			}
			return (Customs.Business.SingleMessageManager[])result.ToArray(typeof(Customs.Business.SingleMessageManager));
		}

		#endregion
	}
}
