using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusHAWBValidation : CusHAWBValidation
	{
		public CTOCusHAWBValidation(CTOCusHAWB parent)
			: base(parent)
		{
			hAWB = parent;
		}

		protected override void CheckCS_HAWB()
		{
			if (hAWB.CS_HAWB.IsEmpty)
			{
				hAWB.CS_HAWBInfo.AddMessageError("MAWB is required for messaging");
			}
			else if (hAWB.CS_HAWB.Length < 11)
			{
				hAWB.CS_HAWBInfo.AddMessageError("MAWB should be at least 11 characters long.");
			}
		}

		protected override void CheckCS_IsSelfAssessedClearance()
		{
			base.CheckCS_IsSelfAssessedClearance();
			if (hAWB.CS_IsSelfAssessedClearance && hAWB.MAWB != null && !hAWB.MAWB.CM_RL_NKDischargePort.IsEmpty && !hAWB.MAWB.CM_RL_NKDischargePort.StartsWith(Core.Constants.CountryCodes.Australia))
			{
				hAWB.CS_IsSelfAssessedClearanceInfo.AddMessageError("SAC is not valid for transit cargo");
			}
		}

		protected override void CheckCS_RL_NKLoadPort()
		{
			base.CheckCS_RL_NKLoadPort();
			MessageValidation.CheckEntered(hAWB.CS_RL_NKLoadPortInfo);
			ListValidation.MessageErrorIfInvalidCode(hAWB.CS_RL_NKLoadPortInfo, hAWB.Lookups.LoadPorts);
			ZString portWarning = MessageValidation.ValidatePortType(hAWB.CS_RL_NKLoadPort, true, false);
			if (!portWarning.IsEmpty)
			{
				hAWB.CS_RL_NKLoadPortInfo.AddWarning(portWarning);
			}
		}

		protected override void CheckCS_RX_NKGoodsCurrency()
		{
			base.CheckCS_RX_NKGoodsCurrency();
			if (hAWB.CS_GoodsValue > 0)
			{
				MessageValidation.CheckEntered(hAWB.CS_RX_NKGoodsCurrencyInfo);
			}
			ListValidation.ErrorIfInvalidCode(hAWB.CS_RX_NKGoodsCurrencyInfo, hAWB.Lookups.GoodsCurrencies);
		}

		#region Implementation

		protected override bool IsFreightPrepaidCollectRequired
		{
			get { return true; }
		}

		protected override ZString FreightPrepaidOrCollectRequiredMessage
		{
			get { return "Method of Payment is required."; }
		}

		readonly CTOCusHAWB hAWB;

		#endregion
	}
}
