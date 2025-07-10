using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class TradeChainPartnerCollection : DependentCusAddInfoCollection<TradeChainPartner, OrgHeader>
	{
		public TradeChainPartnerCollection(OrgHeader master)
			: base(master, CusAddInfoTypeAttribute.Codes.CACSAMessage)
		{
		}

		public TradeChainPartner GetTCPByOrgCode(string orgCode)
		{
			foreach (TradeChainPartner tcp in Elements.ToArray())
			{
				if (tcp.Organization.OH_Code == orgCode)
				{
					return tcp;
				}
			}
			return null;
		}

		public TradeChainPartner GetTCPByCSAID(string csaID)
		{
			foreach (TradeChainPartner tcp in Elements.ToArray())
			{
				if (String.Equals(tcp.CA_CSAID, csaID, StringComparison.CurrentCultureIgnoreCase))
				{
					return tcp;
				}
			}
			return null;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var tcp = child as TradeChainPartner;
			if (tcp != null && tcp.CA_CSAStatus.IsEmpty)
			{
				tcp.CA_CSAStatus = CSAStatusList.Codes.New;
				tcp.CA_Action = CSAActionTypeList.Codes.ReqAdd;
			}
		}
	}
}
