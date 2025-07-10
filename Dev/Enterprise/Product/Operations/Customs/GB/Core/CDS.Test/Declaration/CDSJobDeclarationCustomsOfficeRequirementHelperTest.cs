using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(JobDeclarationCustomsOfficeRequirementHelper))]
	class CDSJobDeclarationCustomsOfficeRequirementHelperTest : EU.Business.Declaration.Testing.JobDeclarationCustomsOfficeRequirementHelperAbstractTest<JobDeclarationCustomsOfficeRequirementHelper>
	{
		public void TestGetOfficeCode()
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			declaration.CustomsOffices.RemoveAndDeleteAll();
			Factory.Save();
			var cusOffice1 = declaration.CustomsOffices.AddNew();
			cusOffice1.CY_Code = EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance;
			cusOffice1.CY_Data = "GB00001";
			declaration.JE_CustomsOffice = "FR00001";

			CombineAssertions(() =>
			{
				AssertEquals("Office with role CCL starts with GB", "GB00001", officeHelper.GetOfficeCode(EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance));
				AssertEquals("Office with role EXT starts with FR", "FR00001", officeHelper.GetOfficeCode(EuOfficeCodesTypes.Codes.OfficeOfExit));
				AssertEquals("Office with role DES is empty", ZString.Empty, officeHelper.GetOfficeCode(EuOfficeCodesTypes.Codes.OfficeOfDestination));
			});
		}

		public override void TestMainOffice_Import()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertCustomsOfficeRequirementEquals("Import", new CustomsOfficeRequirement("CCL", false, false, true, "[UCC 5/26] Customs Office of Presentation"), officeHelper.MainOffice);
		}

		public override void TestMainOffice_Export()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var gbOffices = new CustomsOfficeRequirement("EXT", true, true, "[UCC 5/12] Customs Office of Exit");
			var niOffices = new CustomsOfficeRequirement("EXT", true, false, "[UCC 5/12] Customs Office of Exit");

			((JobDeclaration)declaration).JE_NorthernIrelandMode = NIModeList.Codes.NotToOrFromNi;
			AssertCustomsOfficeRequirementEquals("Export", gbOffices, officeHelper.MainOffice);
			((JobDeclaration)declaration).JE_NorthernIrelandMode = NIModeList.Codes.MovementFromGreatBritainToNi;
			AssertCustomsOfficeRequirementEquals("Export", gbOffices, officeHelper.MainOffice);

			((JobDeclaration)declaration).JE_NorthernIrelandMode = NIModeList.Codes.ExportFromNiToRestOfWorld;
			AssertCustomsOfficeRequirementEquals("Export", niOffices, officeHelper.MainOffice);
		}

		public override void TestMainOffice_Miscellaneous()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertCustomsOfficeRequirementEquals("Misc", new CustomsOfficeRequirement("", true, false), officeHelper.MainOffice);
		}

		public override void TestOtherRequirements_Import()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals(false, officeHelper.OtherRequirements.Any());
		}

		public override void TestOtherRequirements_Export()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertCustomsOfficeRequirementEquals("Export CCL", new CustomsOfficeRequirement("CCL", false, false, "[UCC 5/26] Customs Office of Presentation"), officeHelper.OtherRequirements.Single());
		}

		public override void TestOtherRequirements_Miscellaneous()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals(false, officeHelper.OtherRequirements.Any());
		}

		protected override string SetupDeclarationForCacheKey()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			return "JobDeclarationCustomsOfficeRequirementHelper,GB,EXP,CDS";
		}

		protected override EU.Business.Declaration.JobDeclaration GetJobDeclaration()
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			return result;
		}
	}
}
