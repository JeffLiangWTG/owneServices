using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusHAWBValidation : Customs.Business.CusHAWBValidation
	{
		public CusHAWBValidation(Customs.Business.AutoCusHAWB parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCargoTerminalOperatorAirportAndShed();
			ValidateShipmentDescriptionCode();
			ValidateConsignmentOrEntryType();
		}

		protected override void CheckCS_RL_NKLoadPort()
		{
			base.CheckCS_RL_NKLoadPort();
			if (!Parent.CS_IsMasterHouse)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CS_RL_NKLoadPortInfo, Parent.Lookups.NonUkAirports);
			}
			ValidateShipmentDescriptionCode();
		}

		bool IsUFO
		{
			get { return Parent.CS_IsMasterHouse && Parent.MAWB != null && Parent.MAWB.IsUFO; }
		}

		// Shipment description code SDC
		public void ValidateShipmentDescriptionCode()
		{
			ValidateCalculatedProperty(Parent.ShipmentDescriptionCodeInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "ZPropertyInfo validation method call via reflection. Origin call is ValidateShipmentDescriptionCode")]
		void CheckShipmentDescriptionCode()
		{
			ListValidation.ErrorIfInvalidCode((NoResString)"Please select a valid Shipment Description Code", Parent.ShipmentDescriptionCodeInfo);
			if (!IsUFO)
			{
				MandatoryValidation.CheckEntered(Parent.ShipmentDescriptionCodeInfo, "Shipment Description Code");
			}

			ICcsukCusAwb topAwb = null;
			if (Parent.CS_IsMasterHouse && Parent.MAWB != null)
			{
				topAwb = Parent.MAWB;
			}
			else
			{
				topAwb = Parent;
			}

			if (!IsUFO)
			{
				CheckSDCAndAirportOfOriginAndDestinationCombinations(Parent.ShipmentDescriptionCode, topAwb.AirportOfOrigin, topAwb.AirportOfDestination, Parent.ShipmentDescriptionCodeInfo, Parent.Factory);
			}

			if (Parent.MAWB != null && Parent.MAWB.ShipmentDescriptionCode == ShipmentDescriptionCodes.Codes.ConsignmentManifestedOnMultipleFlights
				&& Parent.ShipmentDescriptionCode == ShipmentDescriptionCodes.Codes.TotalConsignmentManifested)
			{
				Parent.ShipmentDescriptionCodeInfo.AddMessageError("SDC cannot be T because the master's SDC is M");
			}

			if (Parent.IsPrearrival && Parent.ShipmentDescriptionCode != ShipmentDescriptionCodes.Codes.TotalConsignmentManifested)
			{
				Parent.ShipmentDescriptionCodeInfo.AddError("Prearrivals must have SDC=T");
			}
		}

		static void CheckSDCAndAirportOfOriginAndDestinationCombinations(ZString shipmentDescriptionCode, ZString airportOfOrigin, ZString airportOfDestination, ZPropertyInfo zPropertyInfo, BusinessObjectFactory factory)
		{
			var portsInEU = CusMAWBValidation.ArePortsInEU(airportOfOrigin, airportOfDestination, factory);
			switch (portsInEU)
			{
				case CusMAWBValidation.PortsInEU.BothInEU:
					// All 4 SDC codes are allowed
					break;
				case CusMAWBValidation.PortsInEU.OriginNotInEU:
					// M or T
					if (shipmentDescriptionCode != ShipmentDescriptionCodes.Codes.ConsignmentManifestedOnMultipleFlights && shipmentDescriptionCode != ShipmentDescriptionCodes.Codes.TotalConsignmentManifested)
					{
						zPropertyInfo.AddError("Origin outside EU requires SDC T or M");
					}
					break;
				case CusMAWBValidation.PortsInEU.OriginInEU:
					if (shipmentDescriptionCode != ShipmentDescriptionCodes.Codes.ConsignmentManifestedOnMultipleFlights && shipmentDescriptionCode != ShipmentDescriptionCodes.Codes.TotalConsignmentManifested)
					{
						zPropertyInfo.AddError("Origin inside EU with destination outside requires SDC T or M");
					}
					break;
			}
		}

		protected override void CheckCS_RL_NKDestination()
		{
			base.CheckCS_RL_NKDestination();
			ValidateShipmentDescriptionCode();
		}

		protected override void CheckCS_FolioReference()
		{
			base.CheckCS_FolioReference();
			MandatoryValidation.CheckEntered(Parent.CS_FolioReferenceInfo, "profile (PIMA)");
			ListValidation.ErrorIfInvalidCode((NoResString)"Please select a valid profile (PIMA)", Parent.CS_FolioReferenceInfo);
			if (Parent.MAWB != null)
			{
				if (Parent.CS_IsMasterHouse)
				{
					Parent.MAWB.Validation.ValidateCM_ArrivalDate();
					Parent.MAWB.Validation.ValidateCM_GB();
				}
				if (Parent.Profile != Parent.MAWB.Profile)
				{
					Parent.ProfileInfo.AddWarning(string.Format(CultureInfo.CurrentCulture, "There is a mismatch between this HAWB's Profile and its parent MAWB's. The parent MAWB has value '{0}'.", Parent.MAWB.Profile));
				}
			}
			CheckChangingPimasIsOk();
			var isFallbackShed = LicenceAndPimaHelper.IsFallbackShed(Parent);
			var licenceType = LicenceAndPimaHelper.CcsukLicenceType.Base;
			if (LicenceAndPimaHelper.IsFullShed(Parent) || isFallbackShed)
			{
				LicenceAndPimaHelper.CheckHasShedLicenceButDoNotDetermineWhetherOneIsNeeded(Parent.CS_FolioReferenceInfo, LicenceAndPimaHelper.ShedFunctionsNeedToBeEnabled);
				EnsureSecurityRightForShedPima(Parent.CS_FolioReferenceInfo);
				licenceType = LicenceAndPimaHelper.CcsukLicenceType.ErtsShed;
			}
			else
			{
				licenceType = LicenceAndPimaHelper.CcsukLicenceType.Agent;
			}
			if (!Parent.CS_FolioReferenceInfo.HasErrors() && Parent.CanRaiseCcsukLicenceLogin(licenceType))
			{
				LicenceAndPimaHelper.RecordCcsukLicenceLogin(Parent, licenceType);
			}
			if (isFallbackShed)
			{
				Parent.CS_FolioReferenceInfo.AddWarning(AgentIsInAirlineFallbackWarning);
			}
		}

		internal static void EnsureSecurityRightForShedPima(ZPropertyInfo info)
		{
			if (!Environment.Env.Security.AirCcsukShed.IsAllowed)
			{
				info.AddError("You do not have the security right to use shed functions.");
			}
		}

		void CheckChangingPimasIsOk()
		{
			if (Parent.IsInDatabase)
			{
				var newProfile = Parent.CS_FolioReference;
				var dbHawb = new BusinessObjectFactory().Load<CusHAWB>(Parent.PK);
				if (
						// if changing pimas and the one in the db is known to be on the network or has status 3
						dbHawb != null && dbHawb.Profile != newProfile && !Parent.CanUpdateShedAndAgentUsingPima
					)
				{
					// Changing PIMAs between the current shed's pima and the current agent's Pima is allowed, for when the two share a database.  But if you're changing from an agent to another agent, or from a shed to another shed, it's not allowed
					var importandPartOfRequestedNewPima = newProfile.Right(6);
					if (importandPartOfRequestedNewPima != "000" + Parent.AgentBadge && importandPartOfRequestedNewPima != Parent.CS_WarehouseLocation)
					{
						Parent.CS_FolioReferenceInfo.AddError("Cannot change to the PIMA to that of an unrelated entity. This record is lodged or has status 3.");
					}
				}
			}
		}
		public const string AgentIsInAirlineFallbackWarning = "Agent is in airline fallback";

		public void ValidateCargoTerminalOperatorAirportAndShed()
		{
			ValidateCalculatedProperty(Parent.CargoTerminalOperatorAirportAndShedInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "ZPropertyInfo validation method call via reflection. Origin call is AgentIsInAirlineFallbackWarning")]
		void CheckCargoTerminalOperatorAirportAndShed()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CargoTerminalOperatorAirportAndShedInfo);
			if (Parent.MAWB != null)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CargoTerminalOperatorAirportAndShedInfo);
				if (Parent.CS_IsMasterHouse)
				{
					Parent.MAWB.Validation.ValidateCM_MAWB();
				}
				if (Parent.MAWB.CargoTerminalOperatorAirportAndShed != Parent.CargoTerminalOperatorAirportAndShed)
				{
					Parent.CargoTerminalOperatorAirportAndShedInfo.AddMessageError(string.Format(CultureInfo.CurrentCulture, "There is a mismatch between this HAWB's CTO and its parent MAWB's. The parent MAWB has value '{0}'.", Parent.MAWB.CargoTerminalOperatorAirportAndShed));
				}
			}
		}

		protected override void CheckCS_HAWB()
		{
			base.CheckCS_HAWB();
			if (Parent.CS_HAWB.Length > 8)
			{
				Parent.CS_HAWBInfo.AddMessageError("HAWB must be 8 characters long");
			}
			if (!Parent.CS_IsMasterHouse)
			{
				MandatoryValidation.CheckEntered(Parent.CS_HAWBInfo);
				if (Parent.MAWB != null && !Parent.CS_HAWB.IsEmpty)
				{
					if ((from CusHAWB h in Parent.MAWB.ChildBills where h.PK != Parent.PK && h.CS_HAWB == Parent.CS_HAWB select h).Any())
					{
						var errorText = "This bill number is a duplicate on the same MAWB. It should not be added.";
						// Duplicate hawb number
						if (MasterFiles.Business.GlbStaff.CurrentUser.IsSupportUser || MasterFiles.Business.GlbStaff.CurrentUser.GS_IsController)
						{
							Parent.CS_HAWBInfo.AddMessageError(errorText);
						}
						else
						{
							Parent.CS_HAWBInfo.AddError(errorText);
						}
					}
				}
			}

			AwbSerialNumberHelper.WarnIfAwbNumberIsChanging(Parent.CS_HAWBInfo, Parent);
		}

		// Agent badge
		protected override void CheckCS_ResponsiblePartyID()
		{
			base.CheckCS_ResponsiblePartyID();
			if (Parent.MAWB != null)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CS_ResponsiblePartyIDInfo, Parent.MAWB.Lookups.AgentsList, "badge");
			}
		}

		protected override void CheckCS_Weight()
		{
			base.CheckCS_Weight();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CS_WeightInfo);
		}

		protected override void CheckCS_WeightUQ()
		{
			base.CheckCS_WeightUQ();
			if (Parent.CS_WeightUQ != "KG")
			{
				Parent.CS_WeightUQInfo.AddMessageError("Unit of weight must be KG");
			}
		}

		protected override void CheckCS_GoodsDescription()
		{
			base.CheckCS_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CS_GoodsDescriptionInfo);
			if (Parent.CS_GoodsDescription.ToUpper().Contains("CONSOL"))
			{
				if (!Parent.CS_IsMasterHouse || (Parent.MAWB != null && Parent.MAWB.IsBasic))
				{
					Parent.CS_GoodsDescriptionInfo.AddMessageError("The description 'consol' is not acceptable for house bills or basics.");
				}
			}
		}

		protected override void CheckCS_PiecesLanded()
		{
			base.CheckCS_PiecesLanded();
			if (Parent.CS_PiecesLanded.IsEmpty && Parent.MAWB != null)
			{
				if (!Parent.MAWB.CM_ArrivalDate.IsEmpty
					&& (LicenceAndPimaHelper.IsFullShed(Parent) || LicenceAndPimaHelper.IsFallbackShed(Parent))  // don't encourage agents to supply a date when doing so will only generate a red error
					)
				{
					LicenceAndPimaHelper.CheckHasShedLicenceButDoNotDetermineWhetherOneIsNeeded(Parent.CS_PiecesLandedInfo, LicenceAndPimaHelper.PostArrivalRequiresShedFunctionsEnabled);
				}
				if (Parent.MAWB.IsUFO)
				{
					var suffix = Parent.IsInDatabase ? "You should cancel your changes without saving (NPR will now not be re-calculated), and consider deleting the record (send FRX) instead." : ""; // don't show this on the "Make new UFO" popup
					Parent.CS_PiecesLandedInfo.AddError("NPR cannot be zero for a UFO. " + suffix);
				}
			}
		}

		protected override void CheckCS_PiecesManifested()
		{
			base.CheckCS_PiecesManifested();
			if (Parent.CS_IsMasterHouse && Parent.MAWB != null && Parent.MAWB.IsUFO)
			{
				Parent.CS_PiecesManifestedInfo.AddWarning("This is a UFO so NPX=0 is acceptable. When the record is identified, delete this one with FRX and create a new one.");
			}
			else
			{
				MandatoryValidation.CheckEntered(Parent.CS_PiecesManifestedInfo, "number of pieces expected (NPX)");
			}
		}

		protected new CusHAWB Parent
		{
			get { return (CusHAWB)base.Parent; }
		}

		internal void ValidateConsignmentOrEntryType()
		{
			ValidateCalculatedProperty(Parent.ConsignmentOrEntryTypeInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "ZPropertyInfo validation method call via reflection. Origin call is ValidateConsignmentOrEntryType")]
		void CheckConsignmentOrEntryType()
		{
			ListValidation.WarnIfInvalidCode(Parent.ConsignmentOrEntryTypeInfo);
		}

		protected override void CheckCS_RL_NKDischargePort()
		{
			base.CheckCS_RL_NKDischargePort();
			CusMAWBValidation.CheckPortIsNotLON(Parent.CS_RL_NKDischargePortInfo);
			ValidateShipmentDescriptionCode();
		}
	}
}
