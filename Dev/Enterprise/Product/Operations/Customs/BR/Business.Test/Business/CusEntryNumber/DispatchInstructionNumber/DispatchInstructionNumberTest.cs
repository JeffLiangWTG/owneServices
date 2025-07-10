
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DispatchInstructionNumber))]
	public class DispatchInstructionNumberTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var instructionNumber = (DispatchInstructionNumber)base.GetBusinessObjectForFetchForLoad();
			instructionNumber.CE_ParentTable = "JobDeclaration";

			return instructionNumber;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var instructionNumber = (DispatchInstructionNumber)base.GetNewBusinessObjectForDeleteTest(factory);
			instructionNumber.CE_ParentTable = "JobDeclaration";

			return instructionNumber;
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Dispatch Instruction Documents", Factory.New<DispatchInstructionNumber>().HumanReadableName);
		}

		public void TestSetDefaultValues()
		{
			var number = Factory.New<DispatchInstructionNumber>();
			AssertEquals("CE_EntryIsSystemGenerated", false, number.CE_EntryIsSystemGenerated);
			AssertEquals("CE_RN_NKCountryCode", Core.Constants.CountryCodes.Brazil, number.CE_RN_NKCountryCode);
			AssertEquals("CE_Category", CusEntryNumber.Categories.DispatchInstructionDocument, number.CE_Category);
		}

		public void TestDescription()
		{
			var number = Factory.New<DispatchInstructionNumber>();
			number.CE_EntryType = DispatchInstructionDocumentTypes.Codes._01;
			AssertEquals(DispatchInstructionDocumentTypes.Descriptions._01, number.AdditionalReferenceNumberTypeDescription);
		}
	}
}
