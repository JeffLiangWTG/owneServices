using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(JobDeclarationCustomsOfficeRequirementHelper))]
	class JobDeclarationCustomsOfficeRequirementHelperTest : EU.Business.Declaration.Testing.JobDeclarationCustomsOfficeRequirementHelperAbstractTest<JobDeclarationCustomsOfficeRequirementHelper>
	{
		public void TestOtherRequirements()
		{
			var declaration = Factory.New<JobDeclaration>();
			var officeHelper = declaration.CustomsOfficeRequirementHelper;

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertCustomsOfficeRequirementEquals("Import ENT when UCC6 ", new CustomsOfficeRequirement("ENT", false, false), officeHelper.OtherRequirements.Single(x => x.OfficeRole == "ENT"));
				AssertCustomsOfficeRequirementEquals("Import PRE when UCC6 ", new CustomsOfficeRequirement("PRE", false, false), officeHelper.OtherRequirements.Single(x => x.OfficeRole == "PRE"));

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertCustomsOfficeRequirementEquals("Export EXT when UCC6 ", new CustomsOfficeRequirement("EXT", false, false)
				{
					OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland }
				}, officeHelper.OtherRequirements.Single(x => x.OfficeRole == "EXT"));
				AssertCustomsOfficeRequirementEquals("Export EXP when UCC6 ", new CustomsOfficeRequirement("EXP", false, false), officeHelper.OtherRequirements.Single(x => x.OfficeRole == "EXP"));
				AssertCustomsOfficeRequirementEquals("Export PRE when UCC6 ", new CustomsOfficeRequirement("PRE", false, false), officeHelper.OtherRequirements.Single(x => x.OfficeRole == "PRE"));
			}
		}

		public override void TestMainOffice_Import()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertCustomsOfficeRequirementEquals("Import", new CustomsOfficeRequirement("", true, false), officeHelper.MainOffice);
		}

		public override void TestMainOffice_Export()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertCustomsOfficeRequirementEquals("Export", new CustomsOfficeRequirement("", true, false), officeHelper.MainOffice);
		}

		public override void TestMainOffice_Miscellaneous()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertCustomsOfficeRequirementEquals("Misc", new CustomsOfficeRequirement("", true, false), officeHelper.MainOffice);
		}

		public override void TestOtherRequirements_Import()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
			{
				AssertEquals("Pre-requisite: ImportUCC6 is false", expected: false, DeclarationConfiguration.HasImportUCC6Functionality());
				AssertHasOfficeRequirement("ENT");
				AssertHasOfficeRequirement("PRE");
			}

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
			{
				AssertEquals("Pre-requisite: ImportUCC6 is true", expected: true, DeclarationConfiguration.HasImportUCC6Functionality());
				AssertHasOfficeRequirement("ENT");
				AssertHasOfficeRequirement("PRE");
				AssertHasOfficeRequirement("SCO");
			}

			void AssertHasOfficeRequirement(string code)
			{
				AssertCustomsOfficeRequirementEquals($"Import {code}", new CustomsOfficeRequirement(code, false, false), officeHelper.OtherRequirements.SingleOrDefault(x => x.OfficeRole == code));
			}
		}

		public override void TestOtherRequirements_Export()
		{
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertCustomsOfficeRequirementEquals("Export EXT when UCC6 ", new CustomsOfficeRequirement("EXT", false, false)
				{
					OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland }
				}, officeHelper.OtherRequirements.Single(x => x.OfficeRole == "EXT"));
				AssertCustomsOfficeRequirementEquals("Export EXP when UCC6 ", new CustomsOfficeRequirement("EXP", false, false), officeHelper.OtherRequirements.Single(x => x.OfficeRole == "EXP"));
				AssertCustomsOfficeRequirementEquals("Export PRE when UCC6 ", new CustomsOfficeRequirement("PRE", false, false), officeHelper.OtherRequirements.Single(x => x.OfficeRole == "PRE"));
			}
		}

		public override void TestOtherRequirements_Miscellaneous()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals(false, officeHelper.OtherRequirements.Any());
		}

		protected override EU.Business.Declaration.JobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

		protected override string SetupDeclarationForCacheKey()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			return "JobDeclarationCustomsOfficeRequirementHelper,ES,MSC";
		}
	}
}
