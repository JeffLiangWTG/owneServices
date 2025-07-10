using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportUCC6JobDeclarationValidation))]
	sealed class ImportUCC6JobDeclarationValidationTest : JobDeclarationValidationAbstractTest
	{
		public void TestInheritsImportJobDeclarationValidation()
		{
			Assert(typeof(ImportUCC6JobDeclarationValidation).IsSubclassOf(typeof(ImportJobDeclarationValidation)));
		}

		public void TestValidateRuleBR3403_MAIFIX()
		{
			AssertBR3403(new string[] { "" }, new string[] { "MAI", "FIX" });
		}

		public void TestValidateRuleBR3403_SEA()
		{
			AssertBR3403(new string[] { "10", "11", "80", "81" }, new string[] { "SEA" });
		}

		public void TestValidateRuleBR3403_ROA()
		{
			AssertBR3403(new string[] { "30" }, new string[] { "ROA" });
		}

		public void TestValidateRuleBR3403_AIR()
		{
			AssertBR3403(new string[] { "40", "41" }, new string[] { "AIR" });
		}

		void AssertBR3403(string[] transportMeans, string[] validValues) => CombineAssertions(() =>
		{
			var jobDeclaration = declaration;

			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "H1";
			entryInstruction.CEI_SubStyle = "Z";

			var stylesThatShouldTriggerError = new string[] { "H1", "H2", "H3", "H4" };
			var stylesThatShouldNotTriggerError = new ImportDeclarationTypeList().GetAllCodes().ToList();
			stylesThatShouldNotTriggerError.RemoveAll(c => stylesThatShouldTriggerError.Contains(c));
			stylesThatShouldNotTriggerError.Add("XX");

			var subStylesThatShouldNotTriggerError = new string[] { "Z", string.Empty };
			var subStylesThatShouldTriggerError = new EntrySubStyleList().GetAllCodes().ToList();
			subStylesThatShouldTriggerError.RemoveAll(c => subStylesThatShouldNotTriggerError.Contains(c));

			string GetAssertMessage()
			{
				return $"CEI_Style '{entryInstruction.CEI_Style}' CEI_SubStyle '{entryInstruction.CEI_SubStyle}' JE_TransportMeans '{jobDeclaration.JE_TransportMeans}' JE_TransportMode '{jobDeclaration.JE_TransportMode}' JE_TransportModeInland '{jobDeclaration.JE_TransportModeInland}'";
			}

			string GetTransportModeErrorMessage() => $"[BR3403] {jobDeclaration.JE_TransportModeInfo.Description} must be {string.Join(" or ", validValues.Select(v => $"'{v}'"))}.";
			string GetTransportModeInlandErrorMessage() => $"[BR3403] {jobDeclaration.JE_TransportModeInlandInfo.Description} must be {string.Join(" or ", validValues.Select(v => $"'{v}'"))}.";

			void AssertMessageError(string transportMeansValue, bool expectMessageError)
			{
				jobDeclaration.JE_TransportMeans = transportMeansValue;
				jobDeclaration.Validation.ValidateJE_TransportMode();
				jobDeclaration.Validation.ValidateJE_TransportModeInland();

				if (expectMessageError)
				{
					AssertHasMessageError(GetAssertMessage(), jobDeclaration.JE_TransportModeInfo, GetTransportModeErrorMessage());
					AssertHasMessageError(GetAssertMessage(), jobDeclaration.JE_TransportModeInlandInfo, GetTransportModeInlandErrorMessage());
				}
				else
				{
					AssertNoMessageErrorContaining(GetAssertMessage(), jobDeclaration.JE_TransportModeInfo, GetTransportModeErrorMessage());
					AssertNoMessageErrorContaining(GetAssertMessage(), jobDeclaration.JE_TransportModeInlandInfo, GetTransportModeInlandErrorMessage());
				}
			}

			foreach (var transportMeansValue in transportMeans)
			{
				foreach (var validValue in validValues)
				{
					jobDeclaration.JE_TransportModeInland = validValue;
					jobDeclaration.JE_TransportMode = validValue;
					AssertMessageError(transportMeansValue, false);
				}

				jobDeclaration.JE_TransportModeInland = "X";
				jobDeclaration.JE_TransportMode = "X";
				foreach (var style in stylesThatShouldTriggerError)
				{
					foreach (var subStyle in subStylesThatShouldTriggerError)
					{
						entryInstruction.CEI_Style = style;
						entryInstruction.CEI_SubStyle = subStyle;
						AssertMessageError(transportMeansValue, true);
					}
				}

				jobDeclaration.JE_TransportModeInland = validValues[0];
				jobDeclaration.JE_TransportMode = validValues[0];
				foreach (var style in stylesThatShouldNotTriggerError)
				{
					foreach (var subStyle in subStylesThatShouldNotTriggerError)
					{
						entryInstruction.CEI_Style = style;
						entryInstruction.CEI_SubStyle = subStyle;
						AssertMessageError(transportMeansValue, false);
					}
				}

				entryInstruction.CEI_Style = "H5";
				foreach (var validValue in validValues)
				{
					jobDeclaration.JE_TransportModeInland = validValue;
					jobDeclaration.JE_TransportMode = validValue;
					AssertMessageError(transportMeansValue, false);
				}

				jobDeclaration.JE_TransportModeInland = "X";
				jobDeclaration.JE_TransportMode = "X";
				AssertMessageError(transportMeansValue, true);

				entryInstruction.CEI_SubStyle = "X";
			}
		});

		protected override string MessageType => Common.EU.EUJobMessageTypeList.Codes.Import;

		protected override JobDeclarationValidation GetValidation() => new ImportUCC6JobDeclarationValidation(declaration);

		public void TestCheckJE_VesselName()
		{
			AssertTransportIDMandatory(Core.Constants.TransportModes.Air, ImportDeclarationTypeList.Codes.H1, false);
			AssertTransportIDMandatory(Core.Constants.TransportModes.Sea, ImportDeclarationTypeList.Codes.H1, false);
			AssertTransportIDMandatory(Core.Constants.TransportModes.OwnPropulsion, ImportDeclarationTypeList.Codes.H2, false);

			AssertTransportIDMandatory(Core.Constants.TransportModes.OwnPropulsion, ImportDeclarationTypeList.Codes.H1, true);
			AssertTransportIDMandatory(Core.Constants.TransportModes.InlandWaterwayTransport, ImportDeclarationTypeList.Codes.H1, true);
			AssertTransportIDMandatory(Core.Constants.TransportModes.Rail, ImportDeclarationTypeList.Codes.H1, true);
			AssertTransportIDMandatory(Core.Constants.TransportModes.Road, ImportDeclarationTypeList.Codes.H1, true);
			AssertTransportIDMandatory(Core.Constants.TransportModes.OwnPropulsion, ImportDeclarationTypeList.Codes.H3, true);
			AssertTransportIDMandatory(Core.Constants.TransportModes.OwnPropulsion, ImportDeclarationTypeList.Codes.H4, true);
			AssertTransportIDMandatory(Core.Constants.TransportModes.OwnPropulsion, ImportDeclarationTypeList.Codes.H5, true);
		}

		void AssertTransportIDMandatory(ZString transportMode, ZString entryStyle, bool mandatory)
		{
			declaration.JE_TransportMode = transportMode;
			var entryInstruction = declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault() ?? declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = entryStyle;
			declaration.Validation.ValidateJE_VesselName();  // to clear the dirty notifications when we set `JE_TransportMode`

			if (mandatory)
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_VesselNameInfo);
			}
			else
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_VesselNameInfo);
			}
		}

		public void TestCheckJE_RN_NKTransportNationalityInlandH1() => AssertJE_RN_NKTransportNationalityInland(ImportDeclarationTypeList.Codes.H1);

		public void TestCheckJE_RN_NKTransportNationalityInlandH3() => AssertJE_RN_NKTransportNationalityInland(ImportDeclarationTypeList.Codes.H3);

		public void TestCheckJE_RN_NKTransportNationalityInlandH4() => AssertJE_RN_NKTransportNationalityInland(ImportDeclarationTypeList.Codes.H4);

		public void TestCheckJE_RN_NKTransportNationalityInlandH5() => AssertJE_RN_NKTransportNationalityInland(ImportDeclarationTypeList.Codes.H5);

		public void TestCheckJE_RN_NKTransportNationalityInlandOtherStyle()
		{
			var instructions = declaration.CustomsEntryInstructions.FirstOrDefault();
			instructions.CEI_Style = ImportDeclarationTypeList.Codes.H2;

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_RN_NKTransportNationalityInland = ZString.Empty;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_RN_NKTransportNationalityInfo);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			declaration.JE_RN_NKTransportNationalityInland = Core.Constants.CountryCodes.Ireland;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.JE_RN_NKTransportNationalityInfo);
		}

		void AssertJE_RN_NKTransportNationalityInland(string styleCode)
		{
			var errMessage = "[C0010] Nationality cannot be used when Transport Mode is 'RAI' or 'MAI' or 'FIX'.";
			var instructions = declaration.CustomsEntryInstructions.FirstOrDefault();
			instructions.CEI_Style = styleCode;

			CombineAssertions("JE_RN_NKTransportNationalityInland is NOT EMPTY", () =>
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_RN_NKTransportNationalityInland = Core.Constants.CountryCodes.Ireland;
				AssertNoMessageErrorContaining("JE_TransportMode AIR", declaration.JE_RN_NKTransportNationalityInlandInfo, errMessage);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
				declaration.JE_RN_NKTransportNationalityInland = Core.Constants.CountryCodes.Israel;
				AssertHasMessageErrorContaining("JE_TransportMode RAI", declaration.JE_RN_NKTransportNationalityInlandInfo, errMessage);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				declaration.JE_RN_NKTransportNationalityInland = Core.Constants.CountryCodes.Spain;
				AssertHasMessageErrorContaining("JE_TransportMode MAI", declaration.JE_RN_NKTransportNationalityInlandInfo, errMessage);

				declaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
				declaration.JE_RN_NKTransportNationalityInland = Core.Constants.CountryCodes.Turkey;
				AssertHasMessageErrorContaining("JE_TransportMode FIX", declaration.JE_RN_NKTransportNationalityInlandInfo, errMessage);
			});

			CombineAssertions("JE_RN_NKTransportNationalityInland is EMPTY", () =>
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
				declaration.JE_RN_NKTransportNationalityInland = ZString.Empty;
				AssertNoMessageErrorContaining("JE_TransportMode RAI", declaration.JE_RN_NKTransportNationalityInlandInfo, "[C0010] Please enter a Nationality.");

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_RN_NKTransportNationalityInland = ZString.Empty;
				AssertHasMessageErrorContaining("JE_TransportMode AIR", declaration.JE_RN_NKTransportNationalityInlandInfo, "[C0010] Please enter a Nationality.");
			});
		}
	}
}
