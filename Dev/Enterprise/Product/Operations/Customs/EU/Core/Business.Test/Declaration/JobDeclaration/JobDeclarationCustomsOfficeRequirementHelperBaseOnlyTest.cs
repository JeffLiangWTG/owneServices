using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationCustomsOfficeRequirementHelper))]
	class JobDeclarationCustomsOfficeRequirementHelperBaseOnlyTest : JobDeclarationCustomsOfficeRequirementHelperAbstractTest<JobDeclarationCustomsOfficeRequirementHelper>
	{
		public void TestValidateMainOffice()
		{
			var helper = declaration.CustomsOfficeRequirementHelper;
			var errors = helper.Validate();
			if (helper.MainOffice != null && helper.MainOffice.IsMandatory)
			{
				AssertEquals("Has Error", true, errors.Contains($"You have not entered an office of type {helper.MainOffice.FriendlyName}."));
			}
			else
			{
				AssertEquals("No Error", false, errors.Any(e => e.StartsWith("You have not entered an office of type")));
			}
		}

		public void TestGetOfficeCode()
		{
			declaration.CustomsOffices.RemoveAndDeleteAll();
			Factory.Save();
			var cusOffice1 = declaration.CustomsOffices.AddNew();
			cusOffice1.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
			cusOffice1.CY_Data = "GB00001";
			declaration.JE_CustomsOffice = "FR00001";

			CombineAssertions(() =>
			{
				AssertEquals("Office with role EXT starts with GB", "GB00001", officeHelper.GetOfficeCode(EuOfficeCodesTypes.Codes.OfficeOfExit));
				AssertEquals("Office with no role (main office) starts with FR", "FR00001", officeHelper.GetOfficeCode(ZString.Empty));
				AssertEquals("Office with role DES is empty", ZString.Empty, officeHelper.GetOfficeCode(EuOfficeCodesTypes.Codes.OfficeOfDestination));
			});
		}

		public override void TestMainOffice_Import()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertCustomsOfficeRequirementEquals("Main Office Import", new CustomsOfficeRequirement("", true, false), officeHelper.MainOffice);
		}

		public override void TestMainOffice_Export()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertCustomsOfficeRequirementEquals("Main Office Export", new CustomsOfficeRequirement("", true, false), officeHelper.MainOffice);
		}

		public override void TestMainOffice_Miscellaneous()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertCustomsOfficeRequirementEquals("Main Office Misc", new CustomsOfficeRequirement("", true, false), officeHelper.MainOffice);
		}

		public override void TestOtherRequirements_Import()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("Import declaration has no other requirements.", false, officeHelper.OtherRequirements.Any());
		}

		public override void TestOtherRequirements_Export()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertCustomsOfficeRequirementEquals("Export declaration has one EXT requirement.", new CustomsOfficeRequirement("EXT", true, false, "Office of Exit"), officeHelper.OtherRequirements.Single());
		}

		public override void TestOtherRequirements_Miscellaneous()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Miscellaneous declaration has no other requirements.", false, officeHelper.OtherRequirements.Any());
		}

		protected override string SetupDeclarationForCacheKey()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			return "JobDeclarationCustomsOfficeRequirementHelper,LV,IMP";
		}

		protected override JobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();
	}
}
