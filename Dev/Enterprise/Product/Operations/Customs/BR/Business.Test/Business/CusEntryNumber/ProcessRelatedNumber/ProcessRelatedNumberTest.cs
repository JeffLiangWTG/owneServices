
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ProcessRelatedNumber))]
	public class ProcessRelatedNumberTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var instructionNumber = (ProcessRelatedNumber)base.GetBusinessObjectForFetchForLoad();
			instructionNumber.CE_ParentTable = "JobDeclaration";

			return instructionNumber;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var instructionNumber = (ProcessRelatedNumber)base.GetNewBusinessObjectForDeleteTest(factory);
			instructionNumber.CE_ParentTable = "JobDeclaration";

			return instructionNumber;
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Process Related Number", Factory.New<ProcessRelatedNumber>().HumanReadableName);
		}

		public void TestSetDefaultValues()
		{
			var number = Factory.New<ProcessRelatedNumber>();
			AssertEquals("CE_EntryIsSystemGenerated", false, number.CE_EntryIsSystemGenerated);
			AssertEquals("CE_RN_NKCountryCode", Core.Constants.CountryCodes.Brazil, number.CE_RN_NKCountryCode);
			AssertEquals("CE_Category", CusEntryNumber.Categories.DocumentsRelated, number.CE_Category);
		}

		public void TestDescription()
		{
			var number = Factory.New<ProcessRelatedNumber>();
			number.CE_EntryType = ProcessRelatedTypeList.Codes.ADM;
			AssertEquals(ProcessRelatedTypeList.Descriptions.ADM, number.AdditionalReferenceNumberTypeDescription);
		}
	}
}
