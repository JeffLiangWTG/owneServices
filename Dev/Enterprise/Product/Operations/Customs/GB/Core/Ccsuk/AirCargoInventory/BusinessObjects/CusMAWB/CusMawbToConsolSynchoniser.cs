using System;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusMawbToConsolSynchoniser : IUpdateFromConsol
	{
		public CusMawbToConsolSynchoniser(CusMAWB mAWB)
		{
			if (mAWB == null)
			{
				throw new ArgumentNullException(nameof(mAWB), "MAWB cannot be null");
			}

			this.mAWB = mAWB;
			IsAir = true;
		}

		public void SynchroniseFromConsol(ForwardingConsol consol)
		{
			var readOnlyHelper = mAWB.ReadOnlyAndPermissionHelper;
			if (mAWB.CM_ArrivalDate.IsEmpty && consol.Transports != null && consol.Transports.MostInterestingTransport != null && !readOnlyHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.FlightDate))
			{
				ArrivalDate = consol.Transports.MostInterestingTransport.JW_ATA;
			}

			if (mAWB.CM_FlightNo.IsEmpty)
			{
				FlightNumber = consol.JK_VoyageFlightForLastImportTransport;
			}

			if (mAWB.CM_MAWB.IsEmpty && !mAWB.CM_MAWBInfo.ReadOnly)
			{
				mAWB.CM_MAWB = consol.JK_MasterBillNum.Left(mAWB.CM_MAWBInfo.MaxLength);
			}

			if (mAWB.AirportOfDestination.IsEmpty)
			{
				DischargePort = PortConverter.UnlocoToIata(consol.JK_RL_NKDischargePort, mAWB.Factory);
			}

			if (mAWB.AirportOfOrigin.IsEmpty)
			{
				LoadPort = consol.JK_RL_NKLoadPort;
			}

			if (mAWB.Weight.IsEmpty && !readOnlyHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Mass))
			{
				mAWB.Weight = consol.JK_TotalShipmentWeight;
			}

			if (mAWB.WeightCode.IsEmpty && !readOnlyHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Mass))
			{
				mAWB.WeightCode = consol.JK_TotalShipmentWeightUnit;
			}

			if (mAWB.NumberOfPiecesExpected.IsEmpty && !readOnlyHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.Npx))
			{
				mAWB.NumberOfPiecesExpected = (ZShort)consol.JK_TotalShipmentQuantity.ToZInt();
			}

			if (mAWB.AirportOfArrival.IsEmpty && !mAWB.AirportOfArrivalInfo.ReadOnly)
			{
				mAWB.AirportOfArrival = PortConverter.UnlocoToIata(consol.JK_RL_NKPortOfFirstArrival, mAWB.Factory);
			}

			if (mAWB.CargoTerminalOperator.IsEmpty && !mAWB.CargoTerminalOperatorAirportAndShedInfo.ReadOnly)
			{
				mAWB.CargoTerminalOperator = SyncShedCodeFromConsol(consol);
			}

			if (consol.AWBHeader != null && consol.AWBHeader.AWBSpecialHandlingItems != null)
			{
				foreach (Freight.Forwarding.AWB.Business.ExportAWBSpecialHandling shc in consol.AWBHeader.AWBSpecialHandlingItems)
				{
					if (!(from CusAddInfo<CommunityHandlingCode> x in mAWB.CommunityHandlingCodes where x.Data.C4_CommunityHandlingCode == shc.EP_SpecialHandling select x).Any())
					{
						var chc = mAWB.CommunityHandlingCodes.AddNew();
						chc.Data.C4_CommunityHandlingCode = shc.EP_SpecialHandling;
					}
				}
			}
			consol.Logs.AddNew(Events.StatusUpdated, "Synchronised to CCSUK MAWB " + mAWB.ReferenceNumber + " " + mAWB.MasterLevelHouseHelper.CS_WarehouseLocation, ZDateTimeOffset.Now);
		}

		string SyncShedCodeFromConsol(ForwardingConsol consol)
		{
			var shedCode = ZString.Empty;
			if (consol.ArrivalCTOAddress != null)
			{
				var consolCTOShed = consol.ArrivalCTOAddress.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.UnitedKingdomCodeTypes.CTOShed, Core.Constants.CountryCodes.UnitedKingdom, consol.JK_OA_ArrivalCTOAddress).Right(3);
				shedCode = mAWB.Lookups.ShedsList.Cast<ICodeDescription>().Any(x => x.Code.PadRight(6).Substring(3, 3) == consolCTOShed) ? consolCTOShed : ZString.Empty;
			}
			return shedCode;
		}

		public bool IsAir { get; set; }

		public ZDateTime DepartureDate { get; set; }

		public ZDateTime ArrivalDate
		{
			set
			{
				if (!mAWB.ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.FlightDate))
				{
					mAWB.CM_ArrivalDate = value;
				}
			}
		}

		public ZString LoadPort
		{
			set
			{
				if (!mAWB.ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.AirportOfOrigin))
				{
					mAWB.AirportOfOrigin = value;
				}
			}
		}

		public ZString MAWBNumber
		{
			set
			{
				if (!mAWB.ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.AwbNumber))
				{
					mAWB.CM_MAWB = value;
				}
			}
		}

		public ZString FlightNumber
		{
			set
			{
				if (!mAWB.ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.FlightNumber))
				{
					mAWB.CM_FlightNo = value;
				}
			}
		}

		public ZString DischargePort
		{
			set
			{
				if (!mAWB.ReadOnlyAndPermissionHelper.IsFieldReadOnly(FieldsThatCanBeReadOnly.AirportOfDestination))
				{
					mAWB.AirportOfDestination = PortConverter.UnlocoToIata(value, mAWB.Factory);
				}
			}
		}

		public void UpdateLoadPort(Transport transport)
		{
			var consol = transport.Parent as ForwardingConsol;
			if (consol != null && consol.ShouldDefaultFlightDetailsFrom(transport))
			{
				LoadPort = transport.JW_RL_NKLoadPort;
			}
		}

		public void UpdateDischargePort(Transport transport)
		{
			var consol = transport.Parent as ForwardingConsol;
			if (consol != null && consol.ShouldDefaultFlightDetailsFrom(transport))
			{
				DischargePort = transport.JW_RL_NKDiscPort;
			}
		}

		readonly CusMAWB mAWB;
	}
}
