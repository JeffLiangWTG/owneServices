using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public abstract class SCDPlugIn : ZAlwaysLoadPlugIn
	{
		public SCDPlugIn(IBusiness hostEntity) : base(hostEntity)
		{
		}

		protected MultiMessageManager Manager
		{
			get
			{
				if (fManager == null && IsCMR)
				{
					fManager = GetManager();
				}
				return fManager;
			}
			set { fManager = value; }
		}
		MultiMessageManager fManager;

		protected internal abstract MultiMessageManager GetManager();

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.SeaCargoDepot; }
		}

		public override string Name
		{
			get { return "Sea Cargo"; }
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			return new ZMenuItem("Sea Cargo");
		}

		protected override Control GetNewUserControl()
		{
			if (IsCMR)
			{
				return new CMRDepotUnderbondUserControl();
			}
			else
			{
				return new SCDMessageUserControl();
			}
		}

		public override void OnGUIShown()
		{
			base.OnGUIShown();
			ICusUnderbondUnionCollectionParent cMRParent = DepotBusinessObject as ICusUnderbondUnionCollectionParent;
			if (cMRParent != null)
			{
				if (cMRParent.AllUnderbonds.Count > 0 && fSynchroniser == null)
				{
					Synchroniser.Synchronise(Customs.Business.SynchroniseAction.Start);
				}
			}
		}

		public CMRDepotSynchroniser Synchroniser
		{
			get
			{
				if (fSynchroniser == null)
				{
					fSynchroniser = new CMRDepotSynchroniser(DepotBusinessObject.ParentConsol);
				}
				return fSynchroniser;
			}
		}
		CMRDepotSynchroniser fSynchroniser;

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		public SeaCargoDepotBusinessObject DepotBusinessObject
		{
			get { return BusinessEntity as SeaCargoDepotBusinessObject; }
		}

		public bool IsCMR
		{
			get
			{
				return new CMRUtilities().ShouldImportMessageBeSentCMR(ZDateTime.Now, DateOfFirstArrival, ForceLegacy, ForceCMR);
			}
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			if (IsCMR)
			{
				return GetBusinessEntityForPlugIn_CMR();
			}
			else
			{
				return GetBusinessEntityForPlugIn_Legacy();
			}
		}

		protected abstract IBusiness GetBusinessEntityForPlugIn_CMR();
		protected abstract IBusiness GetBusinessEntityForPlugIn_Legacy();

		protected internal abstract bool ForceLegacy { get; }
		protected internal abstract bool ForceCMR { get; }
		protected abstract ZDateTime DateOfFirstArrival { get; }

		protected bool ConsolHasCMRUnderbonds(CFSLoadListConsol consol)
		{
			ZQuery pKList = new ZQuery(CusUnderbondSchema.C4_ParentID, consol.PK);
			pKList.AddToFilter(JoinCondition.Or, CusUnderbondSchema.C4_ParentID, consol.Shipments.GetPKs());
			pKList.AddToFilter(JoinCondition.Or, CusUnderbondSchema.C4_ParentID, consol.Containers.GetPKs());
			return consol.Factory.LoadTop1<CusUnderbond>(pKList) != null;
		}

		protected bool ConsolHasLegacyMessages(CFSLoadListConsol consol)
		{
			ZQuery pKList = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, consol.PK);
			pKList.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_LinkUniqueID, consol.Shipments.GetPKs());
			pKList.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_LinkUniqueID, SQLComparisonOperator.Equal, consol.Containers.GetPKs());
			pKList.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.SeaCargo);
			return consol.Factory.LoadTop1<EDIMessage>(pKList) != null;
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			ContinueWithSave result = base.ShowPreSaveDialogsCore();
			if (result == ContinueWithSave.Yes && Manager != null)
			{
				result = new SendsMessagesToCustomsGUI().DetermineRequiredMessagesAndSendThem(Manager);
			}
			return result;
		}
	}
}
