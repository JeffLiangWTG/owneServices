using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(JobDeclarationCustomsOfficeRequirementHelper))]
	class JobDeclarationCustomsOfficeRequirementHelperTest : EU.Business.Declaration.Testing.JobDeclarationCustomsOfficeRequirementHelperAbstractTest<JobDeclarationCustomsOfficeRequirementHelper>
	{
		public void TestGetOfficeCode()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			declaration.CustomsOffices.RemoveAndDeleteAll();
			Factory.Save();
			var cusOffice1 = declaration.CustomsOffices.AddNew();
			cusOffice1.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
			cusOffice1.CY_Data = "GB00001";
			declaration.JE_CustomsOffice = "FR00001";

			CombineAssertions(() =>
			{
				AssertEquals("Office with role EXT starts with GB", "GB00001", officeHelper.GetOfficeCode(EuOfficeCodesTypes.Codes.OfficeOfExit));
				AssertEquals("Office with role EXP starts with FR", "FR00001", officeHelper.GetOfficeCode(EuOfficeCodesTypes.Codes.OfficeOfExport));
				AssertEquals("Office with role DES is empty", ZString.Empty, officeHelper.GetOfficeCode(EuOfficeCodesTypes.Codes.OfficeOfDestination));
			});
		}

		public override void TestMainOffice_Import()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertCustomsOfficeRequirementEquals("Import", new CustomsOfficeRequirement(ZString.Empty, true, true), officeHelper.MainOffice);
		}

		public override void TestMainOffice_Export()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertCustomsOfficeRequirementEquals("Export", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExport, true, true), officeHelper.MainOffice);
		}

		public override void TestMainOffice_Miscellaneous()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertCustomsOfficeRequirementEquals("MiscellaneousCustoms", new CustomsOfficeRequirement(ZString.Empty, true, false), officeHelper.MainOffice);
		}

		public override void TestOtherRequirements_Import()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("Other Requirements Count", 1, officeHelper.OtherRequirements.Count());
			AssertCustomsOfficeRequirementEquals("Other Requirements Import", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, false, false, "Office of Entry (First or Subsequent)"), officeHelper.OtherRequirements.Single());
		}

		public override void TestOtherRequirements_Export()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertEquals("Other Requirements Count", 4, officeHelper.OtherRequirements.Count());
			AssertCustomsOfficeRequirementEquals("ActualExitOffice", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.ActualExitOffice, false, false)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland }
			}, officeHelper.OtherRequirements.Single(x => x.OfficeRole == EuOfficeCodesTypes.Codes.ActualExitOffice));

			AssertCustomsOfficeRequirementEquals("SupplementaryDeclarationOffice", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice, false, false)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExport }
			}, officeHelper.OtherRequirements.Single(x => x.OfficeRole == EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice));

			AssertCustomsOfficeRequirementEquals("OfficeOfExit", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExit, false, false), officeHelper.OtherRequirements.Single(x => x.OfficeRole == EuOfficeCodesTypes.Codes.OfficeOfExit));

			AssertCustomsOfficeRequirementEquals("OfficeOfPresentation", new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfPresentation, false, false, true)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExport }
			}, officeHelper.OtherRequirements.Single(x => x.OfficeRole == EuOfficeCodesTypes.Codes.OfficeOfPresentation));
		}

		public override void TestOtherRequirements_Miscellaneous()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("MiscellaneousCustoms", false, officeHelper.OtherRequirements.Any());
		}

		protected override string SetupDeclarationForCacheKey()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			return "JobDeclarationCustomsOfficeRequirementHelper,DE,EXP";
		}

		protected override EU.Business.Declaration.JobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();
	}
}
