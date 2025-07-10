using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusMAWBValidation : Customs.Business.CusMAWBValidation
	{
		public CusMAWBValidation(Customs.Business.AutoCusMAWB parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateWeight();
			ValidatePieces();
			ValidateDescription();
			ValidateSDC();
			Parent.MasterLevelHouseHelper.Validation.ValidateCS_WarehouseLocation();
			Parent.MasterLevelHouseHelper.Validation.ValidateCS_FolioReference(); // Profile/PIMA
		}

		void ValidateSDC()
		{
			Parent.MasterLevelHouseHelper.Validation.ValidateShipmentDescriptionCode();
		}

		void ValidateDescription()
		{
			Parent.MasterLevelHouseHelper.Validation.ValidateCS_GoodsDescription();
		}

		void ValidatePieces()
		{
			Parent.MasterLevelHouseHelper.Validation.ValidateCS_PiecesLanded();
			Parent.MasterLevelHouseHelper.Validation.ValidateCS_PiecesManifested();
		}

		public void ValidateWeight()
		{
			Parent.MasterLevelHouseHelper.Validation.ValidateCS_Weight();
			Parent.MasterLevelHouseHelper.Validation.ValidateCS_WeightUQ();
		}

		protected override void CheckCM_FlightNo()
		{
			base.CheckCM_FlightNo();

			if (!Parent.IsProfileAnAgent)
			{
				if (Parent.CM_FlightNo.IsEmpty)
				{
					Parent.CM_FlightNoInfo.AddMessageError(
						"A flight number is mandatory for an ETSF record. Supply a flight number or create an agent pre-arrival record using your agent, not ETSF, profile.");
				}
				else if (Parent.CM_FlightNo.Length < 5)
				{
					Parent.CM_FlightNoInfo.AddMessageError(
						"Flight number should be at least 5 characters so that there are at least 3 characters after the carrier code");
				}
			}
		}

		protected override void CheckCM_MAWB()
		{
			base.CheckCM_MAWB();
			MandatoryValidation.CheckEntered(Parent.CM_MAWBInfo);
			WarnIfMawbNumberNotUniqueWithin12Months();
			AwbSerialNumberHelper.WarnIfAwbNumberIsChanging(Parent.CM_MAWBInfo, Parent);
			WarnIfCreatingBasic();
			new CcsukAirWaybillValidator().Validate(Parent.CM_MAWBInfo);
		}

		void WarnIfCreatingBasic()
		{
			if (Parent.IsBasic)
			{
				var agentNoUpgradeCaveat = LicenceAndPimaHelper.IsSimpleAgentProfile(Parent) ? "You will not be able to add houses to the consignment later if you proceed.  " : "";
				Parent.CM_MAWBInfo.AddWarning(string.Format("You are creating a BASIC record.  {0}You may create an empty pre-arrival consol by adding houses and transmitting the FRI from the master, or you may create a populated consol by creating a house record and sending the FRI message from that job.", agentNoUpgradeCaveat));
			}
		}

		protected override void CheckCM_RL_NKLoadPort()
		{
			base.CheckCM_RL_NKLoadPort();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CM_RL_NKLoadPortInfo, Parent.MasterLevelHouseHelper.Lookups.NonUkAirports);
			ValidateSDC();
		}

		protected override void CheckCM_RL_NKDischargePort()
		{
			base.CheckCM_RL_NKDischargePort();
			CheckPort(Parent.CM_RL_NKDischargePortInfo);
			CheckPortIsNotLON(Parent.CM_RL_NKDischargePortInfo);
			ValidateSDC();
		}

		internal static void CheckPortIsNotLON(ZPropertyInfo zPropertyInfo)
		{
			if (zPropertyInfo.Value.ToString() == "LON")
			{
				zPropertyInfo.AddError("IATA city code LON is not acceptable, use airport code instead");
			}
		}

		protected override void CheckCM_RL_NKFirstArrivalPort()
		{
			base.CheckCM_RL_NKFirstArrivalPort();
			CheckPort(Parent.CM_RL_NKFirstArrivalPortInfo);
			ValidateSDC();
		}

		protected override void CheckCM_ArrivalDate()
		{
			base.CheckCM_ArrivalDate();
			var requiresShed = (LicenceAndPimaHelper.IsFullShed(Parent) || LicenceAndPimaHelper.IsFallbackShed(Parent));
			if (requiresShed && Parent.CM_ArrivalDate.IsEmpty && !Parent.NumberOfPiecesReceived.IsEmpty)
			{
				Parent.CM_ArrivalDateInfo.AddMessageError("Please enter the arrival date since the number of pieces received (NPR) is present");
			}
			else if (requiresShed && Parent.CM_ArrivalDate.IsEmpty && Parent.IsUFO)
			{
				Parent.CM_ArrivalDateInfo.AddMessageError("Please enter the arrival date for this UFO");
			}

			if (Parent.IsInDatabase)
			{
				// Agents can open and perform messaging on porst arrivals, but cannot change the date
				var thisMawbReloadedInSecondFactory = SecondFactoryForArrivalDataValidation.Load<CusMAWB>(Parent.PK);
				var databaseValue = thisMawbReloadedInSecondFactory.CM_ArrivalDate;
				if (!Parent.CM_ArrivalDate.IsEmpty && databaseValue != Parent.CM_ArrivalDate)
				{
					EnsurePostArrivalMasterIsAllowedAndLicenced(Parent.CM_ArrivalDateInfo, LicenceAndPimaHelper.PostArrivalRequiresShedFunctionsEnabled);
				}
			}
			else
			{
				if (!Parent.CM_ArrivalDate.IsEmpty)
				{
					// Agents cannot make new post arrival records
					EnsurePostArrivalMasterIsAllowedAndLicenced(Parent.CM_ArrivalDateInfo, LicenceAndPimaHelper.PostArrivalRequiresShedFunctionsEnabled);
				}
			}
			Parent.MasterLevelHouseHelper.Validation.ValidateShipmentDescriptionCode();
		}

		BusinessObjectFactory secondFactoryForArrivalDataValidation;
		BusinessObjectFactory SecondFactoryForArrivalDataValidation
		{
			get { return secondFactoryForArrivalDataValidation ?? (secondFactoryForArrivalDataValidation = new BusinessObjectFactory()); }
		}

		void CheckPort(ZPropertyInfo zPropertyInfo)
		{
			MandatoryValidation.MessageErrorIfNotEntered(zPropertyInfo);
			ZString val = (ZString)zPropertyInfo.Value;
			if (
				(val.Length == 3 && !Parent.Lookups.UkInventoryControlledAirportsList.ContainsCode(val))
				||
				(val.Length == 5 && !Parent.Lookups.UkInventoryControlledAirportsList.ContainsCode(val.Right(3)))
				||
				(val.Length != 3 && val.Length != 5)
				)
			{
				zPropertyInfo.AddMessageError("Please enter a valid airport using 3- or 5-character notation");
			}
		}

		void WarnIfMawbNumberNotUniqueWithin12Months()
		{
			var loader = new CusMAWB.Loader(Parent.Factory);
			var existingMawbs = loader.FindFromMawbNumber(Parent.CM_MAWB, 12, ZString.Empty, "");
			var result = (from CusMAWB m in existingMawbs where m.PK != Parent.PK && m.CM_ApplicationCode == ApplicationCodeList.Codes.GbCcsuk select m).ToList();
			if (result.Any())
			{
				string maybeLinkedToConsol = "";
				var sameAirportAndOperator = result.FirstOrDefault(r => r.CargoTerminalOperatorAirport + r.CargoTerminalOperator == Parent.CargoTerminalOperatorAirport + Parent.CargoTerminalOperator);
				if (sameAirportAndOperator != null)
				{
					if (sameAirportAndOperator.Consol != null)
					{
						maybeLinkedToConsol = ". It is linked to Consol " + sameAirportAndOperator.Consol.JK_UniqueConsignRef;
					}
					Parent.CM_MAWBInfo.AddMessageError("Another record with this Mawb number and shed/airport already exists in the database" + maybeLinkedToConsol + ". Proceed with caution.");
				}
				else
				{
					var consol = result.Where(r => r.Consol != null).Select(r => r.Consol).FirstOrDefault();
					if (consol != null)
					{
						maybeLinkedToConsol = ". It is linked to Consol " + consol.JK_UniqueConsignRef;
					}
					Parent.CM_MAWBInfo.AddWarning("Another record with this Mawb number already exists in the database" + maybeLinkedToConsol + ". Proceed with caution.");
				}
			}
		}

		protected new CusMAWB Parent
		{
			get { return (CusMAWB)base.Parent; }
		}

		public enum PortsInEU
		{
			BothInEU,
			OriginInEU,
			OriginNotInEU
		}

		public static PortsInEU ArePortsInEU(ZString originPortCode, ZString destinationPortCode, BusinessObjectFactory factory)
		{
			var loader = new RefUNLOCO.Loader(factory);
			var origin = originPortCode.Length == 3
				? RefUNLOCO.LoadFromIATA(factory, originPortCode)
				: loader.Load(originPortCode);
			var destination = destinationPortCode.Length == 3
				? RefUNLOCO.LoadFromIATA(factory, destinationPortCode)
				: loader.Load(destinationPortCode);

			if (origin == null || !origin.IsInEU)
			{
				return PortsInEU.OriginNotInEU;
			}
			else if (destination != null && (destination.IsInEU || destination.IsInNorthernIreland))
			{
				return PortsInEU.BothInEU;
			}
			else
			{
				return PortsInEU.OriginInEU;
			}
		}

		void EnsurePostArrivalMasterIsAllowedAndLicenced(ZPropertyInfo zPropertyInfo, string reasonForWhyLicenceRequired)
		{
			var isPostArrival = !Parent.IsPrearrival;
			if (isPostArrival)
			{
				var profileTypeIsAllowedToMakePostArrivals = LicenceAndPimaHelper.IsFullShed(Parent) || LicenceAndPimaHelper.IsFallbackShed(Parent);
				if (profileTypeIsAllowedToMakePostArrivals)
				{
					LicenceAndPimaHelper.CheckHasShedLicenceButDoNotDetermineWhetherOneIsNeeded(zPropertyInfo, reasonForWhyLicenceRequired);
				}
				else
				{
					zPropertyInfo.AddError(LicenceAndPimaHelper.PostArrivalRequiresShedFunctionsEnabled);
				}
			}
		}

		protected override void CheckCM_GB()
		{
			base.CheckCM_GB();  // Error if blank, error if wrong company		
			if (!Parent.CM_GB.IsEmpty)
			{
				var availableProfiles = CusHAWBLookups.GetProfilesList(Parent.Branch, Parent.Factory);
				if (!availableProfiles.ContainsCode(Parent.Profile))
				{
					Parent.CM_GBInfo.AddError("This branch does not have access to the current PIMA. Select a new PIMA or revert the branch");
				}
			}
			Parent.MasterLevelHouseHelper.Validation.ValidateCS_FolioReference();
		}
	}
}
