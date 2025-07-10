using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public abstract class CusUnderbondValidation : Customs.Business.CusUnderbondValidation
	{
		protected CusUnderbondValidation(CusUnderbond parent)
			: base(parent)
		{
		}

		public void AddFieldsToValidateForMessageSending(Dictionary<ZPropertyInfo, Action> fieldsToValidate)
		{
			fieldsToValidate.Add(Parent.C4_ParentIDInfo, Parent.Validation.ValidateC4_ParentID);
			fieldsToValidate.Add(Parent.SplitReferenceToWhichThisRemovalPertainsInfo, ValidateC4_PackageType);
			AddFieldsToValidateForMessageSendingCore(fieldsToValidate);
		}

		protected virtual void AddFieldsToValidateForMessageSendingCore(Dictionary<ZPropertyInfo, Action> fieldsToValidate)
		{ }

		protected new CusUnderbond Parent
		{
			get { return (CusUnderbond)base.Parent; }
		}

		//SplitReferenceToWhichThisRemovalPertains
		protected override void CheckC4_PackageType()
		{
			base.CheckC4_PackageType();
			if (!Parent.ParentDoesNotHaveSplits)
			{
				if (Parent.SplitReferenceToWhichThisRemovalPertains.IsEmpty)
				{
					Parent.SplitReferenceToWhichThisRemovalPertainsInfo.AddError("This AWB has splits, you must select a split for this " + Parent.Description);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.SplitReferenceToWhichThisRemovalPertainsInfo, (NoResString)"Please select a split from the list.  Only splits that do not already have a locking CAC may be selected.");
				}
			}
		}

		protected override void CheckC4_Outurned()
		{
			// Shut up about OutTurns
		}

		//Onward carrier
		protected override void CheckC4_FlightNo()
		{
			base.CheckC4_FlightNo();
			CheckOnwardAwbAndCarrierAsAPair(Parent.C4_FlightNoInfo, Parent.C4_MAWBInfo);
			ValidateC4_MAWB();
		}

		//Onward awb
		protected override void CheckC4_MAWB()
		{
			base.CheckC4_MAWB();
			if (!Parent.C4_MAWB.IsEmpty)
			{
				new CcsukAirWaybillValidator().ValidateAndAddMessageError(Parent.C4_MAWBInfo);
			}
			CheckOnwardAwbAndCarrierAsAPair(Parent.C4_MAWBInfo, Parent.C4_FlightNoInfo);
			ValidateC4_FlightNo();
		}

		void CheckOnwardAwbAndCarrierAsAPair(ZPropertyInfo thisInfo, ZPropertyInfo otherInfo)
		{
			if ((thisInfo.Value.IsEmpty && !otherInfo.Value.IsEmpty)
				||
				(!thisInfo.Value.IsEmpty && otherInfo.Value.IsEmpty))
			{
				thisInfo.AddMessageError("Onward AWB and Onward Carrier must both be present or missing");
			}
		}

		protected override void CheckC4_RL_NKDischargePort()
		{
			base.CheckC4_RL_NKDischargePort();
			if (Parent == null || Parent.Awb == null)
			{ return; }

			if (Parent.Awb.IsThroughAwb && PortConverter.UnlocoToIata(Parent.AirportOrCountryOfDestination, Parent.Factory) != Parent.Awb.AirportOfDestination)
			{
				Parent.C4_RL_NKDischargePortInfo.AddError("Through AWBs may not be removed to anywhere except the port of discharge");
			}
		}

		protected void ValidatateLicenceRestrictionIndicatorCore(ZPropertyInfo info)
		{
			MandatoryValidation.CheckEntered(info);
			ListValidation.ErrorIfInvalidCode(info, Parent.Lookups.YesNoList);
		}

		protected override void CheckC4_ParentID()
		{
			base.CheckC4_ParentID();
			if (Parent == null || Parent.Hawb == null)
			{ return; }

			if (Parent.Hawb.ShipmentDescriptionCode == ShipmentDescriptionCodes.Codes.ConsignmentManifestedOnMultipleFlights)
			{
				if (Parent.Hawb.Status1Date.IsEmpty && !Parent.Hawb.HasSplits)
				{
					Parent.AddRowMessageError("Removals for consignments with SDC=M require status 1 or splits. Split, wait for or set NPR=NPX, change the SDC, or do not create a removal.");
				}
			}
			else if (Parent.Hawb.ShipmentDescriptionCode == ShipmentDescriptionCodes.Codes.CommunityStatusFromOutsideEC || Parent.Hawb.ShipmentDescriptionCode == ShipmentDescriptionCodes.Codes.CommunityStatusFromECAirport)
			{
				Parent.AddRowMessageError("Removals for consignments with SDC=C or E are not allowed.");
			}

			if (!Parent.Hawb.CS_IsMasterHouse && Parent.Hawb.HasEntryWithLodgedOrPrelodgedWithCustoms && Parent.Hawb.CustomsActionCode != CustomsStatusCodes.Codes.EntryOrRequestCancelled)
			{
				Parent.AddRowMessageError("Removals for consignments with an entry are not allowed");
			}
		}

		protected override void CheckC4_PiecesManifested()
		{
			base.CheckC4_PiecesManifested();
			MandatoryValidation.MessageErrorIfIsZero(Parent.C4_PiecesManifestedInfo);
		}
	}

	public class InterAirportRemovalValidation : CusUnderbondValidation
	{
		public InterAirportRemovalValidation(InterAirportRemoval parent)
			: base(parent)
		{
		}

		protected new InterAirportRemoval Parent
		{
			get { return (InterAirportRemoval)base.Parent; }
		}

		protected override void CheckC4_RL_NKDischargePort()
		{
			base.CheckC4_RL_NKDischargePort();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.AirportOrCountryOfDestinationInfo, Parent.Lookups.AirportsOfDestinationList);
			ValidateShedAndAirportCombo(Parent.C4_DischargePremiseID, Parent.C4_RL_NKDischargePort, Parent.AirportOrCountryOfDestinationInfo);
			ValidateC4_DischargePremiseID();
		}

		protected override void CheckC4_DischargePremiseID()
		{
			// Rule 3b. 
			base.CheckC4_DischargePremiseID();
			ValidateShedAndAirportCombo(Parent.C4_DischargePremiseID, Parent.C4_RL_NKDischargePort, Parent.NewShedIdInfo);
			if (Parent.C4_RL_NKDischargePort == "LHR" || Parent.C4_RL_NKDischargePort == "GBLHR")
			{
				if (Parent.NewShedId.IsEmpty)
				{
					Parent.NewShedIdInfo.AddMessageError("Inter-airport removal to Heathrow requires destination shed");
				}
			}
			ValidateC4_RL_NKDischargePort();
		}

		void ValidateShedAndAirportCombo(ZString shed, ZString airport, ZPropertyInfo propInfo)
		{
			var shedFound = true;
			airport = airport.PadRight(3);
			if (!airport.IsEmpty && !shed.IsEmpty)
			{
				var shedsAtThisAirport = Parent.Lookups.ShedsListAtAirport(airport);
				shedFound = shedsAtThisAirport.ContainsCode(airport + shed);
			}
			if (!shedFound)
			{
				propInfo.AddMessageError("This shed-airport combination is not valid.");
			}
		}

		protected override void CheckC4_UnderbondBySeaVoyage()
		{
			ValidatateLicenceRestrictionIndicatorCore(Parent.LicenseRestrictionIndInfo);
		}

		protected override void AddFieldsToValidateForMessageSendingCore(Dictionary<ZPropertyInfo, Action> fieldsToValidate)
		{
			fieldsToValidate.Add(Parent.AirportOrCountryOfDestinationInfo, ValidateC4_RL_NKDischargePort);
			fieldsToValidate.Add(Parent.NewShedIdInfo, ValidateC4_DischargePremiseID);
			fieldsToValidate.Add(Parent.C4_PiecesManifestedInfo, ValidateC4_PiecesManifested);
			fieldsToValidate.Add(Parent.OnwardCarrierInfo, ValidateC4_FlightNo);
			fieldsToValidate.Add(Parent.OnwardAirWaybillNumberInfo, ValidateC4_MAWB);
			fieldsToValidate.Add(Parent.LicenseRestrictionIndInfo, ValidateC4_UnderbondBySeaVoyage);
		}
	}

	public class TranshipmentRemovalValidation : CusUnderbondValidation
	{
		public TranshipmentRemovalValidation(TranshipmentRemoval parent)
			: base(parent)
		{
		}

		protected new TranshipmentRemoval Parent
		{
			get { return (TranshipmentRemoval)base.Parent; }
		}

		protected override void CheckC4_RL_NKDischargePort()
		{
			base.CheckC4_RL_NKDischargePort();
			if (Parent.C4_RL_NKDischargePort.StartsWith(Core.Constants.CountryCodes.UnitedKingdom))
			{   // HMRC accreditation rule 3a.
				Parent.C4_RL_NKDischargePortInfo.AddMessageError("Destination may not be in GB");
			}
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AirportOrCountryOfDestinationInfo);
		}

		protected override void CheckC4_UnderbondBySeaVoyage()
		{
			ValidatateLicenceRestrictionIndicatorCore(Parent.LicenseRestrictionIndInfo);
		}

		protected override void CheckC4_RL_NKTranshipDestPort()
		{
			base.CheckC4_RL_NKTranshipDestPort();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.PortOfShipmentInfo);
		}

		protected override void AddFieldsToValidateForMessageSendingCore(Dictionary<ZPropertyInfo, Action> fieldsToValidate)
		{
			fieldsToValidate.Add(Parent.AirportOrCountryOfDestinationInfo, ValidateC4_RL_NKDischargePort);
			fieldsToValidate.Add(Parent.PortOfShipmentInfo, ValidateC4_RL_NKTranshipDestPort);
			fieldsToValidate.Add(Parent.C4_PiecesManifestedInfo, ValidateC4_PiecesManifested);
			fieldsToValidate.Add(Parent.OnwardCarrierInfo, ValidateC4_FlightNo);
			fieldsToValidate.Add(Parent.OnwardAirWaybillNumberInfo, ValidateC4_MAWB);
			fieldsToValidate.Add(Parent.LicenseRestrictionIndInfo, ValidateC4_UnderbondBySeaVoyage);
		}
	}

	public class InterShedRemovalValidation : CusUnderbondValidation
	{
		public InterShedRemovalValidation(InterShedRemoval parent)
			: base(parent)
		{
		}

		protected override void CheckC4_ParentID()
		{
			base.CheckC4_ParentID();
			if (Parent.Awb != null && Parent.Awb.IsThroughAwb)
			{
				Parent.C4_ParentIDInfo.AddMessageError("Through AWB should not be removed to another shed");
			}
		}

		protected new InterShedRemoval Parent
		{
			get { return (InterShedRemoval)base.Parent; }
		}

		protected override void CheckC4_DischargePremiseID()
		{
			base.CheckC4_DischargePremiseID();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.NewShedIdInfo);
		}

		protected override void AddFieldsToValidateForMessageSendingCore(Dictionary<ZPropertyInfo, Action> fieldsToValidate)
		{
			fieldsToValidate.Add(Parent.C4_PiecesManifestedInfo, ValidateC4_PiecesManifested);
			fieldsToValidate.Add(Parent.NewShedIdInfo, ValidateC4_DischargePremiseID);
			fieldsToValidate.Add(Parent.Awb.AgentBadgeInfo, ValidateAgentBadge);
		}

		void ValidateAgentBadge()
		{
			var provider = Parent.Awb as IAgentBadgeValidationProvider;
			if (provider != null)
			{
				provider.ValidateAgentBadge();
			}
		}
	}

	public class FallbackValidation : CusUnderbondValidation
	{
		public FallbackValidation(Fallback parent)
			: base(parent)
		{
		}

		protected new Fallback Parent
		{
			get { return (Fallback)base.Parent; }
		}

		protected override void AddFieldsToValidateForMessageSendingCore(Dictionary<ZPropertyInfo, Action> fieldsToValidate)
		{
			fieldsToValidate.Add(Parent.C4_PiecesManifestedInfo, ValidateC4_PiecesManifested);
		}
	}
}
