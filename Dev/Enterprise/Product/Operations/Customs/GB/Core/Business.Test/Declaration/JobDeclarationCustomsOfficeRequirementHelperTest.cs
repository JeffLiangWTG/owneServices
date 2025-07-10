using System.Linq;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Test.Declaration
{
	[TestedType(typeof(JobDeclarationCustomsOfficeRequirementHelper))]
	class JobDeclarationCustomsOfficeRequirementHelperTest : EU.Business.Declaration.Testing.JobDeclarationCustomsOfficeRequirementHelperAbstractTest<JobDeclarationCustomsOfficeRequirementHelper>
	{
		public override void TestMainOffice_Import()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertNull(officeHelper.MainOffice);
		}

		public override void TestMainOffice_Export()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertCustomsOfficeRequirementEquals("Export", new CustomsOfficeRequirement("EXT", false, false, true, "[29] Office of Exit"), officeHelper.MainOffice);
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
			AssertEquals(false, officeHelper.OtherRequirements.Any());
		}

		public override void TestOtherRequirements_Miscellaneous()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals(false, officeHelper.OtherRequirements.Any());
		}

		protected override string SetupDeclarationForCacheKey()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			return "JobDeclarationCustomsOfficeRequirementHelper,GB,MSC,XXX";
		}

		protected override EU.Business.Declaration.JobDeclaration GetJobDeclaration()
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_ApplicationCode = "XXX"; //Not Chief or CDS
			return result;
		}
	}
}
