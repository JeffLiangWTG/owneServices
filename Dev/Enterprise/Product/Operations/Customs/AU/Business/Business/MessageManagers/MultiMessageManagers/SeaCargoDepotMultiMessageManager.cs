using System.Collections;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoDepotMultiMessageManager : MultiMessageManager
	{
		public SeaCargoDepotMultiMessageManager(ICusUnderbondUnionCollectionParent underbondParent)
			: base()
		{
			this.UnderbondParent = underbondParent;
		}

		public readonly ICusUnderbondUnionCollectionParent UnderbondParent;

		#region Implementation

		public override Customs.Business.IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return UnderbondParent as Customs.Business.IMessageManageableBizObj; }
		}

		protected override Customs.Business.SingleMessageManager[] GetAllMessageManagers()
		{
			ArrayList result = new ArrayList();
			ArrayList addedOutturnHeaders = new ArrayList();
			foreach (CusUnderbond underbond in UnderbondParent.AllUnderbonds)
			{
				var header = Factory.Load<CusOutturnHeader>(underbond.C4_C6);
				if (header != null && !addedOutturnHeaders.Contains(header))
				{
					addedOutturnHeaders.Add(header);
					result.Add(new CusUnderbondSEAOUTManager(header));
				}
			}
			return (Customs.Business.SingleMessageManager[])result.ToArray(typeof(Customs.Business.SingleMessageManager));
		}

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get
			{
				return true;
			}
		}

		#endregion

	}
}
