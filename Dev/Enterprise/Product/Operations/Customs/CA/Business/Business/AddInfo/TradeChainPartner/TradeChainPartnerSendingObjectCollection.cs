using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public sealed class TradeChainPartnerSendingObjectCollection : NonPersistentBusinessObjectCollection<TradeChainPartnerSendingObject>
	{
		public TradeChainPartnerSendingObjectCollection(OrgImpAddInfo master) : base(master.Factory)
		{
			this.master = master;
			Load();
		}
		readonly OrgImpAddInfo master;

		public override void Load()
		{
			RemoveAndDeleteAll();
			var tcpCollection = master.TradeChainPartners;
			foreach (var tcp in tcpCollection.Cast<TradeChainPartner>())
			{
				if (CanBeSent(tcp.CA_CSAStatus, tcp.CA_Action))
				{
					Add(new TradeChainPartnerSendingObject(tcp, master.OrgHeader));
				}
			}
		}

		bool CanBeSent(ZString status, ZString action)
		{
			if (!status.IsEmpty && !action.IsEmpty)
			{
				if (action == CSAActionTypeList.Codes.ReqAdd
					&& (status == CSAStatusList.Codes.New
					|| status == CSAStatusList.Codes.ErrorAdded
					|| status == CSAStatusList.Codes.Deleted))
				{
					return true;
				}
				else if (action == CSAActionTypeList.Codes.ReqDel
					&& (status == CSAStatusList.Codes.Added || status == CSAStatusList.Codes.ErrorDeleted))
				{
					return true;
				}
			}
			return false;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
	}
}
