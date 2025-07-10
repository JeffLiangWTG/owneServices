using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManArrivalPortValidation : Customs.Business.CusSeaManArrivalPortValidation
	{
		public CusSeaManArrivalPortValidation(CusSeaManArrivalPort parent)
			: base(parent)
		{
			this.port = parent;
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateStevedoreID();
			ValidateCTOEstablishmentID();
		}

		#endregion

		#region CheckBA_RL_NKArrivalPort

		protected override void CheckBA_RL_NKArrivalPort()
		{
			base.CheckBA_RL_NKArrivalPort();
			ListValidation.ErrorIfInvalidCodeOrEmpty(port.BA_RL_NKArrivalPortInfo, port.Lookups.ArrivalPorts);
		}

		#endregion

		#region CheckBA_ArrivalPortATA

		protected override void CheckBA_ArrivalPortATA()
		{
			base.CheckBA_ArrivalPortATA();

			if (port.Header != null && (port.Header.ImpendingArrivalResponseStatus.Code == CMRBaseStatuses.Codes.OriginalAccepted || port.Header.ImpendingArrivalResponseStatus.Code == CMRBaseStatuses.Codes.AmendmentAccepted))
			{
				MessageValidation.CheckEntered(port.BA_ArrivalPortATAInfo);
			}
		}

		#endregion

		#region CheckBA_ArrivalPortETA

		protected override void CheckBA_ArrivalPortETA()
		{
			base.CheckBA_ArrivalPortETA();

			MessageValidation.CheckEntered(port.BA_ArrivalPortETAInfo);
		}

		#endregion

		#region CheckBA_BerthCode

		protected override void CheckBA_BerthCode()
		{
			base.CheckBA_BerthCode();

			MessageValidation.CheckEntered(port.BA_BerthCodeInfo);
		}

		#endregion

		#region CheckBA_OA_CTOAddress

		protected override void CheckBA_OA_CTOAddress()
		{
			base.CheckBA_OA_CTOAddress();

			MessageValidation.CheckEntered(port.BA_OA_CTOAddressInfo);
			// This validation is probably correct, but clients are complaining.	 W00041954
			//if (Parent.CTOAddress != null && Parent.CTOAddress.Header != null && Parent.CTOAddress.Header.OH_RL_NKClosestPort != Port.BA_RL_NKArrivalPort)
			//{
			//    Parent.BA_OA_CTOAddressInfo.AddMessageError("The selected organisation is not located at the arrival port.");
			//}
		}

		#endregion

		#region CheckBA_DischargeIndicator

		protected override void CheckBA_DischargeIndicator()
		{
			base.CheckBA_DischargeIndicator();

			if (!port.BA_DischargeIndicator && port.IsAnyCargoDischargingHere)
			{
				bool foundDischargeIndicator = false;
				foreach (CusSeaManArrivalPort otherPort in port.Header.Arrivals)
				{
					if (otherPort != port && otherPort.BA_RL_NKArrivalPort == port.BA_RL_NKArrivalPort && otherPort.BA_DischargeIndicator)
					{
						foundDischargeIndicator = true;
						break;
					}
				}

				if (!foundDischargeIndicator)
				{
					port.BA_DischargeIndicatorInfo.AddMessageError("Discharge Indicator should be ticked when there are ocean bills discharging at this port.");
				}
			}
		}

		#endregion

		#region CheckBA_IsFirstArrival

		protected override void CheckBA_IsFirstArrival()
		{
			base.CheckBA_IsFirstArrival();

			if (!port.BA_IsFirstArrival && !OtherPortIsFirstArrival())
			{
				port.BA_IsFirstArrivalInfo.AddMessageError("A First Port of Arrival is required.");
			}
		}

		#endregion

		#region ValidateCTOEstablishmentID

		public void ValidateCTOEstablishmentID()
		{
			ValidateCalculatedProperty(port.CTOEstablishmentIDInfo);
		}

		protected virtual void CheckCTOEstablishmentID()
		{
			MessageValidation.CheckEntered(port.CTOEstablishmentIDInfo, "Discharge CTO Establishment ID is required. Please enter a Customs Controlled Premises (CCP) ID for this CTO Address.");
		}

		#endregion

		#region ValidateStevedoreID

		public void ValidateStevedoreID()
		{
			ValidateCalculatedProperty(port.StevedoreIDInfo);
		}

		protected virtual void CheckStevedoreID()
		{
			MessageValidation.CheckEntered(port.StevedoreIDInfo, "Discharge CTO Stevedore ID is required. Please enter an ABN for this CTO.");
		}

		#endregion

		#region Implementation

		bool OtherPortIsFirstArrival()
		{
			bool result = false;

			if (port.Header != null)
			{
				foreach (CusSeaManArrivalPort otherPort in port.Header.Arrivals)
				{
					if (otherPort != port && otherPort.BA_IsFirstArrival)
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		MessageValidation MessageValidation
		{
			get
			{
				if (fMessageValidation == null)
				{
					fMessageValidation = new MessageValidation(port);
				}
				return fMessageValidation;
			}
		}
		MessageValidation fMessageValidation;

		readonly CusSeaManArrivalPort port;

		#endregion
	}
}
