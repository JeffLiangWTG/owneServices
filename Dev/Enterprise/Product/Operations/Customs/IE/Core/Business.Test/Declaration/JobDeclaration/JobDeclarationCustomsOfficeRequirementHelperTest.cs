using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationCustomsOfficeRequirementHelper))]
	class JobDeclarationCustomsOfficeRequirementHelperTest : JobDeclarationCustomsOfficeRequirementHelperAbstractTest<JobDeclarationCustomsOfficeRequirementHelper>
	{
		public void TestGetOfficeCode()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.CustomsOffices.RemoveAndDeleteAll();
			Factory.Save();

			declaration.JE_CustomsOffice = "FR00001";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit).CY_Data = "GB00001";
			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDestination).CY_Data = "DE00001";

			CombineAssertions(() =>
			{
				AssertEquals("Requirement EXP being MAIN, GetOfficeCode returns JE_CustomsOffice.", "FR00001", officeHelper.GetOfficeCode(EuOfficeCodesTypes.Codes.OfficeOfExport));
				AssertEquals("Requirement EXT as Other, should be able to find CY_Data from EXT code.", "GB00001", officeHelper.GetOfficeCode(EuOfficeCodesTypes.Codes.OfficeOfExit));
				AssertEquals("No Requirement DES, returns is empty ignoring CustomsOffices DES.", ZString.Empty, officeHelper.GetOfficeCode(EuOfficeCodesTypes.Codes.OfficeOfDestination));
			});
		}

		public override void TestMainOffice_Import()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertCustomsOfficeRequirementEquals("Import", new CustomsOfficeRequirement(ZString.Empty, true, true, "Office of Lodgement"), officeHelper.MainOffice);
		}

		public override void TestMainOffice_Export()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertCustomsOfficeRequirementEquals(
				"Export",
				new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExport, false, true, "Office of Export"),
				officeHelper.MainOffice
			);
		}

		public override void TestMainOffice_Miscellaneous()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertCustomsOfficeRequirementEquals("MiscellaneousCustoms", new CustomsOfficeRequirement(ZString.Empty, true, false, "Office of Lodgement"), officeHelper.MainOffice);
		}

		public override void TestOtherRequirements_Import()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var otherRequirements = officeHelper.OtherRequirements;

			AssertEquals("Count of OtherRequirements", 4, otherRequirements.Count());

			var actualPresentationRequirement = otherRequirements.Single(req => req.OfficeRole == EuOfficeCodesTypes.Codes.OfficeOfPresentation);
			var expectedPresentationRequirement = new CustomsOfficeRequirement(
				EuOfficeCodesTypes.Codes.OfficeOfPresentation, isMandatory: false, isLocalCountryOnly: false, isForeignCountryOnly: false
			)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance }
			};
			AssertCustomsOfficeRequirementEquals("PRE requirement", expectedPresentationRequirement, actualPresentationRequirement);

			var actualExitRequirement = otherRequirements.Single(req => req.OfficeRole == EuOfficeCodesTypes.Codes.OfficeOfExit);
			var expectedExitRequirement = new CustomsOfficeRequirement(
				EuOfficeCodesTypes.Codes.OfficeOfExit, isMandatory: false, isLocalCountryOnly: false, isForeignCountryOnly: false
			)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExitInland, EuOfficeCodesTypes.Codes.OfficeOfLodgementExit, EuOfficeCodesTypes.Codes.OfficeOfExit }
			};
			AssertCustomsOfficeRequirementEquals("EXT requirement", expectedExitRequirement, actualExitRequirement);

			var actualDishargeRequirement = otherRequirements.Single(req => req.OfficeRole == EuOfficeCodesTypes.Codes.OfficeOfDischarge);
			var expectedDishargeRequirement = new CustomsOfficeRequirement(
				EuOfficeCodesTypes.Codes.OfficeOfDischarge, isMandatory: false, isLocalCountryOnly: false, isForeignCountryOnly: false
			)
			{
				OfficeRolesForLookup = Array.Empty<ZString>()
			};
			AssertCustomsOfficeRequirementEquals("DSC requirement", expectedExitRequirement, actualExitRequirement);
		}

		public override void TestOtherRequirements_Export()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var otherRequirements = officeHelper.OtherRequirements;

			AssertEquals("Count of OtherRequirements", 3, otherRequirements.Count());

			var actualPresentationRequirement = otherRequirements.Single(req => req.OfficeRole == EuOfficeCodesTypes.Codes.OfficeOfPresentation);
			var expectedPresentationRequirement = new CustomsOfficeRequirement(
				EuOfficeCodesTypes.Codes.OfficeOfPresentation, isMandatory: false, isLocalCountryOnly: false, isForeignCountryOnly: false
			)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance }
			};
			AssertCustomsOfficeRequirementEquals("PRE requirement", expectedPresentationRequirement, actualPresentationRequirement);

			var actualExitRequirement = otherRequirements.Single(req => req.OfficeRole == EuOfficeCodesTypes.Codes.OfficeOfExit);
			var expectedExitRequirement = new CustomsOfficeRequirement(
				EuOfficeCodesTypes.Codes.OfficeOfExit, isMandatory: false, isLocalCountryOnly: false, isForeignCountryOnly: false
			)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExitInland, EuOfficeCodesTypes.Codes.OfficeOfLodgementExit, EuOfficeCodesTypes.Codes.OfficeOfExit }
			};
			AssertCustomsOfficeRequirementEquals("EXT requirement ", expectedExitRequirement, actualExitRequirement);

			var actualSupervisingRequirement = otherRequirements.Single(req => req.OfficeRole == EuOfficeCodesTypes.Codes.SupervisingOffice);
			var expectedSupervisingRequirement = new CustomsOfficeRequirement(
				EuOfficeCodesTypes.Codes.SupervisingOffice, isMandatory: false, isLocalCountryOnly: true, isForeignCountryOnly: false
			)
			{
				OfficeRolesForLookup = Array.Empty<ZString>()
			};
			AssertCustomsOfficeRequirementEquals("SVO requirement ", expectedSupervisingRequirement, actualSupervisingRequirement);
		}

		public override void TestOtherRequirements_Miscellaneous()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			var otherRequirements = officeHelper.OtherRequirements;

			AssertEquals("Count of OtherRequirements", 3, otherRequirements.Count());

			var actualPresentationRequirement = otherRequirements.Single(req => req.OfficeRole == EuOfficeCodesTypes.Codes.OfficeOfPresentation);
			var expectedPresentationRequirement = new CustomsOfficeRequirement(
				EuOfficeCodesTypes.Codes.OfficeOfPresentation, isMandatory: false, isLocalCountryOnly: false, isForeignCountryOnly: false
			)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance }
			};
			AssertCustomsOfficeRequirementEquals("PRE requirement", expectedPresentationRequirement, actualPresentationRequirement);

			var actualExitRequirement = otherRequirements.Single(req => req.OfficeRole == EuOfficeCodesTypes.Codes.OfficeOfExit);
			var expectedExitRequirement = new CustomsOfficeRequirement(
				EuOfficeCodesTypes.Codes.OfficeOfExit, isMandatory: false, isLocalCountryOnly: false, isForeignCountryOnly: false
			)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExitInland, EuOfficeCodesTypes.Codes.OfficeOfLodgementExit, EuOfficeCodesTypes.Codes.OfficeOfExit }
			};
			AssertCustomsOfficeRequirementEquals("EXT requirement", expectedExitRequirement, actualExitRequirement);
		}

		protected override string SetupDeclarationForCacheKey()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			return "JobDeclarationCustomsOfficeRequirementHelper,IE,IMP";
		}

		protected override EU.Business.Declaration.JobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();
	}
}
