using System.Collections;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusMAWBMessageManager : MultiMessageManager
	{
		public CusMAWBMessageManager(GetCusMAWBDelegate getCusMAWBDelegate)
			: this(getCusMAWBDelegate, ZString.Empty)
		{
		}

		public CusMAWBMessageManager(GetCusMAWBDelegate getCusMAWBDelegate, ZString ownerCompanyABN)
			: base()
		{
			this.getCusMAWBDelegate = getCusMAWBDelegate;
			this.OwnerCompanyABN = ownerCompanyABN;
		}

		public readonly ZString OwnerCompanyABN;

		public delegate CusMAWB GetCusMAWBDelegate();

		public override Customs.Business.IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return MAWB; }
		}

		public CusMAWB MAWB
		{
			get { return getCusMAWBDelegate == null ? null : getCusMAWBDelegate(); }
		}
		readonly GetCusMAWBDelegate getCusMAWBDelegate;

		public bool SendOutturnMessage(Customs.Business.ISendsMessagesToCustoms sender, CusUnderbond underbond)
		{
			Initialise();
			var outturnManager = new CusUnderbondAIROUTManager(underbond);
			return SendOriginal(sender, new CusUnderbondAIROUTManager[] { outturnManager }).Any();
		}

		#region Implementation

		public OrgHeader ResponsibleParty
		{
			get
			{
				if (fResponsibleParty == null)
				{
					fResponsibleParty = GlbCompany.CurrentCompany.OrgProxy;
				}
				return fResponsibleParty;
			}
			set { fResponsibleParty = value; }
		}
		OrgHeader fResponsibleParty;

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return false; }
		}

		protected override bool ShowNotificationsAfterSaveCore
		{
			get { return true; }
		}

		protected override Customs.Business.SingleMessageManager[] GetAllMessageManagers()
		{
			ArrayList result = new ArrayList();
			if (MAWB != null)
			{
				foreach (CusHAWB hAWB in MAWB.ChildBills)
				{
					result.Add(new CusHAWBAIRCRMessageManager(hAWB, OwnerCompanyABN));
				}

				ICusUnderbondUnionCollectionParent parent = MAWB;
				foreach (CusUnderbond underbond in parent.AllUnderbonds)
				{
					result.Add(new CusUnderbondUBMREQManager(underbond, OwnerCompanyABN));
				}

				foreach (CusUnderbond underbond in ((ICusUnderbondUnionCollectionParent)MAWB).AllUnderbonds)
				{
					if (underbond.CanDoOutturn)
					{
						result.Add(new CusUnderbondAIROUTManager(underbond, OwnerCompanyABN));
					}
				}
			}

			return (Customs.Business.SingleMessageManager[])result.ToArray(typeof(Customs.Business.SingleMessageManager));
		}

		#endregion
	}
}
