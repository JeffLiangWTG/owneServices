using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsBillPhase5RuleR0473ValidationTest : TestCaseWithFactory
	{
		public void TestValidate_VesselName()
		{
			using (SetTransitionPeriod(false))
			{
				departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._8_InlandWaterwayTransport;
				bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._80;

				CombineAssertions("NCTSTP = OFF, TransportMode = 8, TypeOfDeparture = 80", () =>
				{
					bill.VesselNameAtDeparture = "DFF";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Vessel Name = DFF", bill.VesselNameAtDepartureInfo, R0473ErrorMessage);

					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._80;
					bill.VesselNameAtDeparture = "dff";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageErrorContaining("Vessel Name = dff", bill.VesselNameAtDepartureInfo, R0473ErrorMessage);
				});
			}

			using (SetTransitionPeriod(true))
			{
				bill.Validation.ValidateFirstDepartureTransportMeansID();
				AssertNoMessageErrorContaining("NCTSTP = ON, TransportMode = 8, Vessel Name = dff", bill.VesselNameAtDepartureInfo, R0473ErrorMessage);
			}
		}

		public void TestValidate_TransportAtDeparture()
		{
			departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._9_OwnPropulsion;
			var typeOfTransportIdsForOwnPropulsionArray = NCTSTestHelper.SetupTransportIdsForOwnPropulsionToTestR0473();

			using (SetTransitionPeriod(false))
			{
				foreach (var transportId in typeOfTransportIdsForOwnPropulsionArray)
				{
					CombineAssertions($"NCTSTP = OFF, TransportMode = 9, TypeOfDeparture = {transportId}", () =>
					{
						bill.TransportTypeAtDeparture = transportId;
						bill.TransportAtDeparture = "DFF";
						bill.Validation.ValidateFirstDepartureTransportMeansID();
						AssertNoMessageErrorContaining("Transport ID = DFF", bill.TransportAtDepartureInfo, R0473ErrorMessage);

						bill.TransportAtDeparture = "dff";
						bill.Validation.ValidateFirstDepartureTransportMeansID();
						AssertHasMessageErrorContaining("Transport ID = dff", bill.TransportAtDepartureInfo, R0473ErrorMessage);
					});
				}
			}

			using (SetTransitionPeriod(true))
			{
				bill.TransportAtDeparture = "dff";
				foreach (var transportId in typeOfTransportIdsForOwnPropulsionArray)
				{
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining($"NCTSTP = ON, TransportMode = 9, TypeOfDeparture = {transportId}, Transport ID = dff", bill.TransportAtDepartureInfo, R0473ErrorMessage);
				}
			}
		}

		public void TestValidate_TrainNumber()
		{
			using (SetTransitionPeriod(false))
			{
				departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._2_RailTransport;

				CombineAssertions("NCTSTP = OFF, TransportMode = 2", () =>
				{
					bill.TransportAtDeparture = "DFF";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Train Number = DFF", bill.TransportAtDepartureInfo, R0473ErrorMessage);

					bill.TransportAtDeparture = "dff";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageErrorContaining("Train Number = dff", bill.TransportAtDepartureInfo, R0473ErrorMessage);
				});
			}

			using (SetTransitionPeriod(true))
			{
				bill.TransportAtDeparture = "dff";
				bill.Validation.ValidateFirstDepartureTransportMeansID();
				AssertNoMessageErrorContaining("NCTSTP = ON, TransportMode = 2, Train Number = dff", bill.TransportAtDepartureInfo, R0473ErrorMessage);
			}
		}

		public void TestValidate_TransportId()
		{
			using (SetTransitionPeriod(false))
			{
				departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._3_RoadTransport;

				CombineAssertions("NCTSTP = OFF, TransportMode = 3", () =>
				{
					bill.TransportAtDeparture = "DFF";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Transport ID = DFF", bill.TransportAtDepartureInfo, R0473ErrorMessage);

					bill.TransportAtDeparture = "dff";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageErrorContaining("Transport ID = dff", bill.TransportAtDepartureInfo, R0473ErrorMessage);
				});
			}

			using (SetTransitionPeriod(true))
			{
				bill.Validation.ValidateFirstDepartureTransportMeansID();
				AssertNoMessageErrorContaining("NCTSTP = ON, TransportMode = 3, Train Name = dff", bill.TransportAtDepartureInfo, R0473ErrorMessage);
			}
		}

		public void TestValidate_AircraftId()
		{
			using (SetTransitionPeriod(false))
			{
				departureMovement.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._4_AirTransport;

				CombineAssertions("NCTSTP = OFF, TransportMode = 4", () =>
				{
					bill.TransportAtDeparture = "DFF";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Aircraft ID = DFF", bill.TransportAtDepartureInfo, R0473ErrorMessage);

					bill.TransportAtDeparture = "dff";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageErrorContaining("Aircraft ID = dff", bill.TransportAtDepartureInfo, R0473ErrorMessage);
				});
			}

			using (SetTransitionPeriod(true))
			{
				bill.Validation.ValidateFirstDepartureTransportMeansID();
				AssertNoMessageErrorContaining("NCTSTP = ON, TransportMode = 4, Aircraft ID = dff", bill.TransportAtDepartureInfo, R0473ErrorMessage);
			}
		}

		#region SetUp

		IDisposable SetTransitionPeriod(bool isActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isActive);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = header.MovementHeader;
			bill = header.Bills.AddNew();
		}

		const string R0473ErrorMessage = "[R0473] Must not contain lower case letters.";
		NctsHeader header;
		NctsDepartureMovementHeader departureMovement;
		NctsBill bill;

		#endregion
	}
}
