using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAOceanBillValidation : Customs.Business.CusSCAOceanBillValidation
	{
		public CusSCAOceanBillValidation(CusSCAOceanBill parent)
			: base(parent)
		{
		}

		public void ValidateCustomsShipmentStatusFilter()
		{
			ValidateCalculatedProperty(SCAOceanBill.CustomsShipmentStatusFilterInfo);
		}

		public void ValidateCustomsMessageStatusFilter()
		{
			ValidateCalculatedProperty(SCAOceanBill.CustomsMessageStatusFilterInfo);
		}

		protected CusSCAOceanBill SCAOceanBill
		{
			get
			{
				return (CusSCAOceanBill)Parent;
			}
		}

		public MessageValidation MessageValidation
		{
			get { return GetMessageValidationCore(); }
		}

		protected MessageValidation GetMessageValidationCore()
		{
			return new MessageValidation(SCAOceanBill);
		}

		protected void CheckCustomsShipmentStatusFilter()
		{
			ListValidation.MessageErrorIfInvalidCode(SCAOceanBill.CustomsShipmentStatusFilterInfo);
		}

		protected void CheckCustomsMessageStatusFilter()
		{
			ListValidation.MessageErrorIfInvalidCode(SCAOceanBill.CustomsMessageStatusFilterInfo);
		}

		protected override void CheckCB_ApplicationCode()
		{
			base.CheckCB_ApplicationCode();
			ListValidation.MessageErrorIfInvalidCode(SCAOceanBill.CB_ApplicationCodeInfo, SCAOceanBill.Lookups.ApplicationCodeList);
		}

		protected override void CheckCB_OceanBill()
		{
			base.CheckCB_OceanBill();
			MessageValidation.CheckEntered(SCAOceanBill.CB_OceanBillInfo);
			new CustomsValidation(SCAOceanBill.CB_OceanBillInfo).ErrorOnKeyData(SCAOceanBill);
		}

		protected override void CheckCB_RL_NKPortOfLoading()
		{
			base.CheckCB_RL_NKPortOfLoading();
			MessageValidation.CheckEntered(SCAOceanBill.CB_RL_NKPortOfLoadingInfo);
			if (SCAOceanBill.CB_RL_NKPortOfLoading.Right(3) != "///")
			{
				ListValidation.MessageErrorIfInvalidCode(SCAOceanBill.CB_RL_NKPortOfLoadingInfo, SCAOceanBill.PortOfLoadingList);
				if (SCAOceanBill.PortOfLoading != null && !SCAOceanBill.PortOfLoading.RL_HasSeaport)
				{
					SCAOceanBill.CB_RL_NKPortOfLoadingInfo.AddWarning("The port of loading has not been configured as a sea port. Use F3/F4 to edit the UNLOCO.");
				}
			}
			else if (!CheckCountryHasValidSeaPorts(SCAOceanBill.CB_RL_NKPortOfLoading.Left(2)))
			{
				SCAOceanBill.CB_RL_NKPortOfLoadingInfo.AddWarning("This country/region may not have any sea ports as it is land locked. If this is incorrect add the UNLOCO and configure it as a sea port.");
			}
		}

		protected override void CheckCB_RL_NKPortOfDischarge()
		{
			base.CheckCB_RL_NKPortOfDischarge();
			MessageValidation.CheckEntered(SCAOceanBill.CB_RL_NKPortOfDischargeInfo);
			ListValidation.MessageErrorIfInvalidCode(SCAOceanBill.CB_RL_NKPortOfDischargeInfo, SCAOceanBill.PortOfDischargeList);

			if (SCAOceanBill.PortOfDischarge != null)
			{
				if (SCAOceanBill.PortOfDischarge.RL_Code != "AUADL" && !SCAOceanBill.PortOfDischarge.RL_HasSeaport)
				{
					SCAOceanBill.CB_RL_NKPortOfDischargeInfo.AddWarning("The port of discharge has not been configured as a sea port. Use F3/F4 to edit the UNLOCO.");
				}
			}
		}

		protected override void CheckCB_VesselName()
		{
			base.CheckCB_VesselName();
			var header = SCAOceanBill;
			if (ShouldValidateVesselIsValidFromList)
			{
				ListValidation.MessageErrorIfInvalidCode(header.CB_VesselNameInfo, header.Lookups.VesselNames);
			}

			if (!header.CB_VesselName.IsEmpty && !header.CB_LloydsIMO.IsEmpty && !VesselHelper.IsExist(header.Factory, header.CB_VesselName, header.CB_LloydsIMO))
			{
				header.CB_VesselNameInfo.AddWarning("Vessel is not on file.");
			}

			if (!RefVessel.LookupVesselsByNameAndLloyds(Parent.CB_VesselName, Parent.CB_LloydsIMO, Parent.Factory).Any())
			{
				Parent.CB_VesselNameInfo.AddWarning("A Vessel with this Name and Lloyds Number cannot be found.");
			}
		}

		protected bool ShouldValidateVesselIsValidFromList
		{
			get { return SCAOceanBill.CB_LloydsIMO.IsEmpty; }
		}

		protected override void CheckCB_LloydsIMO()
		{
			base.CheckCB_LloydsIMO();
			ValidateCB_VesselName();
			MessageValidation.CheckEntered(SCAOceanBill.CB_LloydsIMOInfo);
			LloydsNumberValidation lloydsValidation = new LloydsNumberValidation();
			lloydsValidation.Validate(SCAOceanBill.CB_LloydsIMO);
			if (!lloydsValidation.IsValid)
			{
				SCAOceanBill.CB_LloydsIMOInfo.AddMessageError(lloydsValidation.ErrorText);
			}

			ValidateCB_VesselName();
			new CustomsValidation(SCAOceanBill.CB_LloydsIMOInfo).ErrorOnKeyData(SCAOceanBill);
		}

		protected override void CheckCB_Voyage()
		{
			base.CheckCB_Voyage();
			MessageValidation.CheckEntered(SCAOceanBill.CB_VoyageInfo);
			if (SCAOceanBill.CB_Voyage.Length > 6)
			{
				SCAOceanBill.CB_VoyageInfo.AddMessageError("Voyage Number should be no more than 6 characters long");
			}
			new CustomsValidation(SCAOceanBill.CB_VoyageInfo).ErrorOnKeyData(SCAOceanBill);
		}

		protected bool CheckCountryHasValidSeaPorts(ZString countryCode)
		{
			return SCAOceanBill.CountriesWithSeaPorts.Contains(SCAOceanBill.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode));
		}

		#region First Arrival Port

		protected override void CheckCB_RL_NKPortOfFirstArrival()
		{
			base.CheckCB_RL_NKPortOfFirstArrival();
			if (!Parent.CB_RL_NKPortOfDischarge.StartsWith(Core.Constants.CountryCodes.Australia))
			{
				string message = "";
				if (Parent.CB_RL_NKPortOfFirstArrival.IsEmpty)
				{
					message = "Discharge port is an overseas one and this cargo is recognised as a transit cargo.\r\nFirst Australian arrival port may not be empty for a transit cargo.";
				}
				else if (!Parent.CB_RL_NKPortOfFirstArrival.StartsWith(Core.Constants.CountryCodes.Australia))
				{
					message = "For transit cargo, First arrival port should be an Australian port.";
				}

				if (!string.IsNullOrEmpty(message))
				{
					Parent.CB_RL_NKPortOfFirstArrivalInfo.AddMessageError(message);
				}
			}
		}

		#endregion

		#region Principal ID

		protected override void CheckCB_PrincipalID()
		{
			base.CheckCB_PrincipalID();
			MessageValidation.CheckEntered(Parent.CB_PrincipalIDInfo);

			if (!Parent.CB_PrincipalIDInfo.HasMessageErrors())
			{
				if (Parent.CB_PrincipalID.IsEmpty)
				{
					Parent.CB_PrincipalIDInfo.AddMessageError("The shipping line needs to have an ABN number.");
				}
				else if (Parent.CB_PrincipalID.Length > 11)
				{
					Parent.CB_PrincipalIDInfo.AddMessageError("The principal number cannot be more than 11 characters.");
				}
				if (!ABNValidation.CheckValidABN(Parent.CB_PrincipalID))
				{
					Parent.CB_PrincipalIDInfo.AddMessageError("The principal id is not a valid ABN");
				}
			}
			new CustomsValidation(SCAOceanBill.CB_PrincipalIDInfo).ErrorOnKeyData(SCAOceanBill);
		}

		#endregion

		#region Responsible Party ID

		protected override void CheckCB_ResponsiblePartyID()
		{
			base.CheckCB_ResponsiblePartyID();
			ValidateCB_PrincipalID();

			if (!Parent.CB_ResponsiblePartyIDInfo.HasMessageErrors())
			{
				if (Parent.CB_ResponsiblePartyID.IsEmpty)
				{
					Parent.CB_ResponsiblePartyIDInfo.AddMessageError("The responsible party id needs to have an ABN number.");
				}
				else if (Parent.CB_ResponsiblePartyID.Length > 11)
				{
					Parent.CB_ResponsiblePartyIDInfo.AddMessageError("The number cannot be more than 11 characters.");
				}
				if (!ABNValidation.CheckValidABN(Parent.CB_ResponsiblePartyID))
				{
					Parent.CB_ResponsiblePartyIDInfo.AddMessageError("The responsible party id is not a valid ABN");
				}
			}
			new CustomsValidation(SCAOceanBill.CB_ResponsiblePartyIDInfo).ErrorOnKeyData(SCAOceanBill);
		}

		#endregion
	}
}
