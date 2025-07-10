using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CodeAndDescriptionWrapper))]
	class CodeAndDescriptionWrapperTest : GenericWrapperTest
	{
		protected override ZString ExpectedDefaultFormatting => "Registry : (No Default Field Value Available on Registry)";
		protected override string ExpectedFieldMap => @"CodeAndDescription                 (Default Field: CodeAndDescription)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Code                                    String
CodeAndDescription                      String
CodeInParenthesesAndDesc                String
Description                             String";
		public override void TestWrapperMappingsEmpty()
		{
			var list = new FeeMarkTypeList();
			AssertEquals("", CodeAndDescriptionWrapper.New("", list, Factory).Code);
			AssertEquals("", CodeAndDescriptionWrapper.New("", list, Factory).Description);
			AssertEquals("", CodeAndDescriptionWrapper.New("", list, Factory).CodeInParenthesesAndDesc);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return CodeAndDescriptionWrapper.New("Code", "Description", Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return CodeAndDescriptionWrapper.New("Code", "Description", Factory);
		}

		public void TestCodeInParenthesesAndDesc()
		{
			AssertEquals("", CodeAndDescriptionWrapper.New("", "", Factory).CodeInParenthesesAndDesc);
			AssertEquals("(Code)Desc", CodeAndDescriptionWrapper.New("Code", "Desc", Factory).CodeInParenthesesAndDesc);
			AssertEquals("(1)是", CodeAndDescriptionWrapper.New(Factory, true).CodeInParenthesesAndDesc);
			AssertEquals("(0)否", CodeAndDescriptionWrapper.New(Factory, false).CodeInParenthesesAndDesc);
		}

		public void TestNewCodeAndDescriptionWrapperForCNCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2300", "Nanjing", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			Factory.Save();
			AssertEquals("(2300)Nanjing", CodeAndDescriptionWrapper.New(Factory, "2300", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today).CodeInParenthesesAndDesc);
			AssertEquals("(2301)", CodeAndDescriptionWrapper.New(Factory, "2301", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today).CodeInParenthesesAndDesc);
		}
	}
}
