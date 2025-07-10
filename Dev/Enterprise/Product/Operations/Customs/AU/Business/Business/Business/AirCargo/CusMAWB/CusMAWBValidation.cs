using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CusMAWBValidation : Customs.Business.CusMAWBValidation
	{
		public CusMAWBValidation(CusMAWBBase parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCM_fUseAltPartShipModel();
		}

		#region System Defined Values

		public void ValidateCM_fUseAltPartShipModel()
		{
			ValidateCalculatedProperty(MAWB.CM_fUseAltPartShipModelInfo);
		}

		protected virtual void CheckCM_fUseAltPartShipModel()
		{
		}

		#endregion

		#region Flight Number

		protected override void CheckCM_FlightNo()
		{
			base.CheckCM_FlightNo();
			if (MAWB.CM_FlightNo.IsEmpty)
			{
				MAWB.CM_FlightNoInfo.AddMessageError("Flight no is required.");
			}
			else
			{
				ZString warningMessage = FlightNoValidation.ValidateFlightNo(MAWB.CM_FlightNoInfo);

				if (!warningMessage.IsEmpty)
				{
					MAWB.CM_FlightNoInfo.AddWarning(warningMessage);
				}
				else
				{
					if (Parent.CM_MAWB.Length >= 3)
					{
						ValidateCM_MAWB();
					}
				}
			}
		}

		#endregion

		protected override void CheckCM_MAWB()
		{
			base.CheckCM_MAWB();
			if (Parent.CM_FlightNo.Length >= 2 && Parent.CM_MAWB.Length >= 3)
			{
				RefAirline airline = RefAirline.LoadFromAirline2LetterCode(Parent.Factory, Parent.CM_FlightNo.Left(2));
				if (airline != null && airline.RM_EagleAddedAirlinePrefixOrAccountingCode != Parent.CM_MAWB.Left(3))
				{
					Parent.CM_MAWBInfo.AddWarning("The Airline Prefix does not match the Airline 2 Letter Code in the Flight Number.");
				}
			}
		}

		protected override void CheckCM_OA_UnpackDepotAddress()
		{
			base.CheckCM_OA_UnpackDepotAddress();
			var depotAddress = Parent.UnpackDepotAddress;
			if (depotAddress != null && depotAddress.LocalControlledPremisesID.IsEmpty)
			{
				Parent.CM_OA_UnpackDepotAddressInfo.AddMessageError(NoCCP);
			}
		}
		internal const string NoCCP = "There is no Australian Customs Controlled Premise Code linked to this address nor the organisation linked to this address.";

		#region Responsible Party ID

		protected override void CheckCM_ResponsiblePartyID()
		{
			base.CheckCM_ResponsiblePartyID();
			if (!Parent.CM_ResponsiblePartyID.IsEmpty)
			{
				if (!ABNValidation.CheckValidABN(Parent.CM_ResponsiblePartyID) && (Parent.ResponsibleParty == null || Parent.ResponsibleParty.GetCustomsClientID().IsEmpty))
				{
					Parent.CM_ResponsiblePartyIDInfo.AddMessageError("The Responsible Party ID is not a valid ABN");
				}
			}

			var responsibleParty = Parent.ResponsibleParty;
			if (responsibleParty != null && Parent.CM_ResponsiblePartyID.IsEmpty)
			{
				Parent.CM_ResponsiblePartyIDInfo.AddWarning("Responsible Party is entered, but it does not have an ID.");
			}
		}

		protected override void CheckCM_OH_ResponsibleParty()
		{
			base.CheckCM_OH_ResponsibleParty();
			Parent.Validation.ValidateCM_ResponsiblePartyID();
		}

		#endregion

		#region Arrival Date

		protected override void CheckCM_ArrivalDate()
		{
			base.CheckCM_ArrivalDate();
			if (MAWB.CM_ArrivalDate.IsEmpty)
			{
				MAWB.CM_ArrivalDateInfo.AddMessageError("Arrival date is required.");
			}
			else if (!MAWB.CM_ArrivalDate.IsValid)
			{
				MAWB.CM_ArrivalDateInfo.AddError("You must enter a valid arrival date.");
			}
		}

		#endregion

		#region Discharge Port

		protected override void CheckCM_RL_NKDischargePort()
		{
			base.CheckCM_RL_NKDischargePort();
			if (MAWB.DischargePort == null)
			{
				MAWB.CM_RL_NKDischargePortInfo.AddMessageError("Valid discharge port is required.");
			}
			else
			{
				ZString portWarning = MessageValidation.ValidatePortType(MAWB.CM_RL_NKDischargePort, true, false);
				if (!portWarning.IsEmpty)
				{
					MAWB.CM_RL_NKDischargePortInfo.AddWarning(portWarning);
				}

				if (MAWB.CM_RL_NKDischargePort.EndsWith("ZZZ"))
				{
					MAWB.CM_RL_NKDischargePortInfo.AddMessageError("Discharge port is invalid. Please check the job detail and enter a correct port.");
				}
				else if (MAWB.DischargePort.Country != null && MAWB.DischargePort.RL_RN_NKCountryCode != "AU")
				{
					MAWB.CM_RL_NKDischargePortInfo.AddMessageError("Port of discharge must be a domestic one.");
				}
			}

			ValidateCM_RL_NKFirstArrivalPort();
		}

		#endregion

		#region First Arrival Port

		protected override void CheckCM_RL_NKFirstArrivalPort()
		{
			base.CheckCM_RL_NKFirstArrivalPort();
			ListValidation.MessageErrorIfInvalidCode(Parent.CM_RL_NKFirstArrivalPortInfo, Parent.Lookups.DischargePorts);
			if (!Parent.CM_RL_NKDischargePort.StartsWith(Core.Constants.CountryCodes.Australia))
			{
				MessageValidation.CheckEntered(Parent.CM_RL_NKFirstArrivalPortInfo, "Port of First Arrival is required when cargo is in transit (not discharging in Australia).");

				if (!Parent.CM_RL_NKFirstArrivalPort.IsEmpty && !Parent.CM_RL_NKFirstArrivalPort.StartsWith(Core.Constants.CountryCodes.Australia))
				{
					Parent.CM_RL_NKFirstArrivalPortInfo.AddMessageError("Port of First Arrival must be an Australian port.");
				}
			}
			else if (!Parent.CM_RL_NKFirstArrivalPort.IsEmpty)
			{
				Parent.CM_RL_NKFirstArrivalPortInfo.AddMessageError("Port of First Arrival must be blank for non-transit cargo (discharging in Australia).");
			}
		}

		#endregion

		#region Application Code

		protected override void CheckCM_ApplicationCode()
		{
			base.CheckCM_ApplicationCode();
			ListValidation.ErrorIfInvalidCode(MAWB.CM_ApplicationCodeInfo, MAWB.Lookups.ApplicationCodeList);
		}

		#endregion

		#region Routing Ports

		protected override void CheckCM_RL_NKRoutePort1()
		{
			base.CheckCM_RL_NKRoutePort1();
			ListValidation.ErrorIfInvalidCode(Parent.CM_RL_NKRoutePort1Info, Parent.Lookups.DischargePorts);
		}

		protected override void CheckCM_RL_NKRoutePort2()
		{
			base.CheckCM_RL_NKRoutePort2();
			ListValidation.ErrorIfInvalidCode(Parent.CM_RL_NKRoutePort2Info, Parent.Lookups.DischargePorts);
		}

		protected override void CheckCM_RL_NKRoutePort3()
		{
			base.CheckCM_RL_NKRoutePort3();
			ListValidation.ErrorIfInvalidCode(Parent.CM_RL_NKRoutePort3Info, Parent.Lookups.DischargePorts);
		}

		#endregion

		#region Implementation

		protected MessageValidation MessageValidation
		{
			get { return new MessageValidation(MAWB); }
		}

		public virtual CusMAWBBase MAWB
		{
			get { return (CusMAWBBase)Parent; }
		}

		#endregion
	}
}
