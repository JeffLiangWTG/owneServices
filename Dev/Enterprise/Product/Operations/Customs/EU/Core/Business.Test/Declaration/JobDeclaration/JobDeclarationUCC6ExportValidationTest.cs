using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class JobDeclarationUCC6ExportValidationTest : TestCaseWithFactory
	{
		public void TestCheckJE_VesselName_MaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			const string message = "The maximum length for [21] Vessel is 27 characters.";
			CombineAssertions(() =>
			{
				declaration.JE_VesselName = ZString.Empty.PadLeft(28, 'A');
				JobDeclarationUCC6ExportValidation.CheckJE_VesselName(declaration);
				AssertHasWarning("28 characters", declaration.JE_VesselNameInfo, message);
				declaration.JE_VesselName = ZString.Empty.PadLeft(27, 'A');
				JobDeclarationUCC6ExportValidation.CheckJE_VesselName(declaration);
				AssertNoWarning("27 characters", declaration.JE_VesselNameInfo, message);
			});
		}

		public void TestCheckJE_VesselName_RV_LloydsNumberIsEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			const string message = "There is no Lloyds/IMO number stored in the Vessel data.";
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "TESTVESSEL";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._10;
			CombineAssertions(() =>
			{
				declaration.JE_VesselName = "XX";
				JobDeclarationUCC6ExportValidation.CheckJE_VesselName(declaration);
				AssertNoMessageError("Vessel is invalid", declaration.JE_VesselNameInfo, message);

				declaration.JE_VesselName = ZString.Empty;
				JobDeclarationUCC6ExportValidation.CheckJE_VesselName(declaration);
				AssertNoMessageError("JE_VesselName is empty", declaration.JE_VesselNameInfo, message);

				declaration.JE_VesselName = vessel.RV_Code;
				JobDeclarationUCC6ExportValidation.CheckJE_VesselName(declaration);
				AssertHasMessageError("RV_LloydsNumber is empty", declaration.JE_VesselNameInfo, message);

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				declaration.JE_VesselName = vessel.RV_Code;
				JobDeclarationUCC6ExportValidation.CheckJE_VesselName(declaration);
				AssertNoMessageError("RV_LloydsNumber is empty and JE_TransportMode isn't 'SEA'", declaration.JE_VesselNameInfo, message);

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._11;
				declaration.JE_VesselName = vessel.RV_Code;
				JobDeclarationUCC6ExportValidation.CheckJE_VesselName(declaration);
				AssertNoMessageError("RV_LloydsNumber is empty and ZG_BorderTransportMeans isn't '10'", declaration.JE_VesselNameInfo, message);

				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._10;
				vessel.RV_LloydsNumber = "7894450";
				JobDeclarationUCC6ExportValidation.CheckJE_VesselName(declaration);
				AssertNoMessageError("RV_LloydsNumber isn't empty", declaration.JE_VesselNameInfo, message);
			});
		}

		public void TestCheckJE_VesselName_Mandatory_TransportModeIsSEAOrROA()
		{
			var declaration = Factory.New<JobDeclaration>();
			foreach (var transportMode in new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Road })
			{
				declaration.JE_TransportMode = transportMode;
				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._11;
				CombineAssertions(transportMode, () =>
				{
					JobDeclarationUCC6ExportValidation.CheckJE_VesselName(declaration);
					AssertHasMessageErrorContaining("JE_VesselName is empty (UCC6)", declaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);

					declaration.JE_TransportMode = TransportTypeList.Codes.Air;
					JobDeclarationUCC6ExportValidation.CheckJE_VesselName(declaration);
					AssertNoMessageErrorContaining($"JE_VesselName is empty and JE_TransportMode isn't '{transportMode}' (UCC6)", declaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);

					declaration.JE_TransportMode = transportMode;
					declaration.ZG_BorderTransportMeans = ZString.Empty;
					JobDeclarationUCC6ExportValidation.CheckJE_VesselName(declaration);
					AssertNoMessageErrorContaining("JE_VesselName is empty and ZG_BorderTransportMeans is empty", declaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);

					declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._11;
					declaration.JE_VesselName = "TESTVESSEL";
					JobDeclarationUCC6ExportValidation.CheckJE_VesselName(declaration);
					AssertNoMessageErrorContaining("JE_VesselName isn't empty (UCC6)", declaration.JE_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
				});
			}
		}

		public void TestCheckJE_TransportIDInland_IsAirInland()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
			AssertMutualExclusiveNumbers(declaration,
				declaration.JE_TransportIDInlandInfo,
				declaration.JE_AircraftRegistrationInlandInfo,
				"Flight Number",
				"You may only enter a Flight Number or an Aircraft ID.",
				(x) => JobDeclarationUCC6ExportValidation.CheckJE_TransportIDInland(x));
		}

		public void TestCheckJE_TransportIDInland_IsRailInland_IsTransitionPeriodAES30()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertMutualExclusiveNumbers(declaration,
					declaration.JE_TransportIDInlandInfo,
					declaration.JE_Trailer1RegNoInfo,
					"Train Number",
					"You may only enter a Train or a Wagon Number.",
					(x) => JobDeclarationUCC6ExportValidation.CheckJE_TransportIDInland(x));
			}
		}

		public void TestCheckJE_TransportIDInland_IsRailInland_IsTransitionPeriodAES30False()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				declaration.JE_Trailer1RegNo = ZString.Empty;
				declaration.JE_TransportIDInland = ZString.Empty;
				JobDeclarationUCC6ExportValidation.CheckJE_TransportIDInland(declaration);
				AssertNoNotifications(declaration.JE_TransportIDInlandInfo);
			}
		}

		public void TestCheckJE_TransportIDInland_Mandatory_FIX_MAI()
		{
			var message = MandatoryValidation.YouHaveNotEnteredMessage("Transport ID");
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				foreach (var transportMode in new ZString[] { TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.Mail })
				{
					declaration.JE_TransportModeInland = transportMode;
					declaration.JE_TransportMeans = ZString.Empty;
					AssertNotMandatory(declaration, declaration.JE_TransportIDInlandInfo, JobDeclarationUCC6ExportValidation.CheckJE_TransportIDInland, message, $"TransportMode {transportMode}, TransportMeans empty");
					declaration.JE_TransportMeans = "01";
					AssertMandatory(declaration, declaration.JE_TransportIDInlandInfo, JobDeclarationUCC6ExportValidation.CheckJE_TransportIDInland, message, $"TransportMode {transportMode}, TransportMeans not empty");
				}
			});
		}

		public void TestCheckJE_Trailer1RegNo_IsRailInland_IsTransitionPeriodAES30()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertMutualExclusiveNumbers(declaration,
					declaration.JE_Trailer1RegNoInfo,
					declaration.JE_TransportIDInlandInfo,
					"Wagon Number",
					"You may only enter a Train or a Wagon Number.",
					(x) => JobDeclarationUCC6ExportValidation.CheckJE_Trailer1RegNo(x));
			}
		}

		public void TestCheckJE_Trailer1RegNo_IsRailInland_IsTransitionPeriodAES30False()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				declaration.JE_AircraftRegistrationInland = ZString.Empty;
				declaration.JE_Trailer1RegNo = ZString.Empty;
				JobDeclarationUCC6ExportValidation.CheckJE_Trailer1RegNo(declaration);
				AssertNoNotifications(declaration.JE_Trailer1RegNoInfo);
			}
		}

		public void TestCheckJE_AircraftRegistrationInland_IsAirInland()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
			AssertMutualExclusiveNumbers(declaration,
				declaration.JE_AircraftRegistrationInlandInfo,
				declaration.JE_TransportIDInlandInfo,
				"Aircraft ID",
				"You may only enter a Flight Number or an Aircraft ID.",
				(x) => JobDeclarationUCC6ExportValidation.CheckJE_AircraftRegistrationInland(x));
		}

		public void TestCheckJE_RN_NKTransportNationalityInland_Mandatory_FIX_MAI()
		{
			var message = MandatoryValidation.YouHaveNotEnteredMessage("Nationality");
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				foreach (var transportMode in new ZString[] { TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.Codes.Mail })
				{
					declaration.JE_TransportModeInland = transportMode;
					declaration.JE_TransportMeans = ZString.Empty;
					AssertNotMandatory(declaration, declaration.JE_RN_NKTransportNationalityInlandInfo, JobDeclarationUCC6ExportValidation.CheckJE_RN_NKTransportNationalityInland, message, $"TransportMode {transportMode}, TransportMeans empty");
					declaration.JE_TransportMeans = "01";
					AssertMandatory(declaration, declaration.JE_RN_NKTransportNationalityInlandInfo, JobDeclarationUCC6ExportValidation.CheckJE_RN_NKTransportNationalityInland, message, $"TransportMode {transportMode}, TransportMeans not empty");
				}
			});
		}

		protected override void TearDown()
		{
			base.TearDown();
			RemoveChangedValidationOutsideOfCheckWarning();
		}

		void RemoveChangedValidationOutsideOfCheckWarning()
		{
			// We're testing the validation code outside of the check as we will be plugged in via composition.
			if (ErrorReporter.TotalErrorCount == 1 && ErrorReporter.LastMessageReported.StartsWith("Attempt to change validation on a property info outside of its Check method"))
			{
				ErrorReporter.Clear();
			}
		}

		void AssertMandatory(JobDeclaration declaration, ZPropertyInfo targetPropertyInfo, Action<JobDeclaration> checkMethod, string notificationMessage, string assertionMessage = null)
		{
			targetPropertyInfo.Value = ZString.Empty;
			checkMethod.Invoke(declaration);
			AssertHasMessageErrorContaining(assertionMessage, targetPropertyInfo, notificationMessage);
			targetPropertyInfo.Value = (ZString)"X";
			checkMethod.Invoke(declaration);
			AssertNoMessageErrorContaining(assertionMessage, targetPropertyInfo, notificationMessage);
		}

		void AssertNotMandatory(JobDeclaration declaration, ZPropertyInfo targetPropertyInfo, Action<JobDeclaration> checkMethod, string notificationMessage, string assertionMessage)
		{
			targetPropertyInfo.Value = ZString.Empty;
			checkMethod.Invoke(declaration);
			AssertNoMessageErrorContaining(assertionMessage, targetPropertyInfo, notificationMessage);
		}

		void AssertMutualExclusiveNumbers(JobDeclaration declaration, ZPropertyInfo targetPropertyInfo, ZPropertyInfo relatedPropertyInfo, string mandatoryMessage, string exclusiveMessage, Action<JobDeclaration> checkMethod)
		{
			CombineAssertions(() =>
			{
				relatedPropertyInfo.Value = ZString.Empty;
				targetPropertyInfo.Value = ZString.Empty;
				checkMethod.Invoke(declaration);
				AssertHasMessageErrorContaining("Both are empty, has mandatoryMessage", targetPropertyInfo, mandatoryMessage);
				AssertNoMessageError("Both are empty, no exclusiveMessage", targetPropertyInfo, exclusiveMessage);

				relatedPropertyInfo.Value = ZString.Empty;
				targetPropertyInfo.Value = (ZString)"ABCD1234";
				checkMethod.Invoke(declaration);
				AssertNoMessageErrorContaining("Target isn't empty, no mandatoryMessage", targetPropertyInfo, mandatoryMessage);
				AssertNoMessageError("Only target isn't empty, no exclusiveMessage", targetPropertyInfo, exclusiveMessage);

				relatedPropertyInfo.Value = (ZString)"ABCD1234";
				targetPropertyInfo.Value = ZString.Empty;
				checkMethod.Invoke(declaration);
				AssertNoMessageErrorContaining("Related isn't empty, no mandatoryMessage", targetPropertyInfo, mandatoryMessage);
				AssertNoMessageError("Only related isn't empty, no exclusiveMessage", targetPropertyInfo, exclusiveMessage);

				relatedPropertyInfo.Value = (ZString)"ABCD1234";
				targetPropertyInfo.Value = (ZString)"ABCD1234";
				checkMethod.Invoke(declaration);
				AssertHasMessageError("Neither is empty, has exclusiveMessage", targetPropertyInfo, exclusiveMessage);
			});
		}
	}
}
