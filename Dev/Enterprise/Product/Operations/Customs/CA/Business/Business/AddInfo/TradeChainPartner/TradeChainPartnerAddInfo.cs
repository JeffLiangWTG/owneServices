using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.CA.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.CACSAMessage)]
	public class TradeChainPartnerAddInfo : AutoTradeChainPartnerAddInfo
	{
		public TradeChainPartnerAddInfo(ZPropertyInfo addInfoProerty)
			: base(addInfoProerty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProerty);
		}

		public new TradeChainPartner Parent
		{
			get { return (TradeChainPartner)base.Parent; }
			protected set { base.Parent = value; }
		}

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (HasChanges && Parent != null && !Parent.IsMarkingAsNeedingValidationSuspended)
				{
					Parent.MarkAsNeedingValidation();
				}
			}
		}
	}
}
