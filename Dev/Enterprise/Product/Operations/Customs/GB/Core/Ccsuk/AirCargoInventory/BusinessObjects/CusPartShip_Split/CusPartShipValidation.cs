using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusPartShipValidation : Customs.Business.CusPartShipValidation, IAgentBadgeValidationProvider
	{
		public CusPartShipValidation(Customs.Business.AutoCusPartShip parent)
			: base(parent)
		{ }

		void IAgentBadgeValidationProvider.ValidateAgentBadge()
		{
			ValidateCG_FlightNo();
		}

		protected override void CheckCG_FlightNo()
		{
			base.CheckCG_FlightNo();
			if (Parent.AWB != null)
			{
				CusMAWB mawb = null;
				if (Parent.AWB is CusHAWB)
				{
					mawb = (Parent.AWB as CusHAWB).MAWB;
				}
				else if (Parent.AWB is CusMAWB)
				{
					mawb = Parent.AWB as CusMAWB;
				}
				if (mawb != null)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CG_FlightNoInfo, mawb.Lookups.AgentsList, "badge");
				}
			}
		}

		public new SplitConsignment Parent
		{
			get { return (SplitConsignment)base.Parent; }
		}
	}
}
