using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GAApprovalValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Procedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.OGARegulationCategory, "KR OGA Regulation Category");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var cusCode_01 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "01", "약사법", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			var cusCode_02 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.OGARegulationCategory, "02", "마약법", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = invoice.InvoiceLines.AddNew();
			var gaApproval1 = invoiceline.GAApprovalDataCollection.AddNew();
			gaApproval1.CSI_Procedure = "";
			AssertHasMessageErrorContaining(gaApproval1.CSI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);

			gaApproval1.CSI_Procedure = "XX";
			AssertHasMessageErrorContaining(gaApproval1.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError);

			gaApproval1.CSI_Procedure = cusCode_01.ZZD_Code;
			AssertNoMessageErrors(gaApproval1.CSI_ProcedureInfo);

			var gaApproval2 = invoiceline.GAApprovalDataCollection.AddNew();
			gaApproval2.CSI_Procedure = cusCode_01.ZZD_Code;
			AssertHasMessageErrorContaining(gaApproval2.CSI_ProcedureInfo, "You cannot enter a 'Regulation Category' that already exists.");

			gaApproval2.CSI_Procedure = cusCode_02.ZZD_Code;
			AssertNoMessageErrors(gaApproval2.CSI_ProcedureInfo);
		}
		public void TestCkeckCSI_SubType()
		{
			gaApproval.CSI_SubType = "";
			AssertHasMessageErrorContaining(gaApproval.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);

			gaApproval.CSI_SubType = "5";
			AssertHasMessageErrorContaining(gaApproval.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError);

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._1;
			AssertNoMessageErrors(gaApproval.CSI_SubTypeInfo);

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._2;
			AssertNoMessageErrors(gaApproval.CSI_SubTypeInfo);

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._3;
			AssertNoMessageErrors(gaApproval.CSI_SubTypeInfo);

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._4;
			AssertNoMessageErrors(gaApproval.CSI_SubTypeInfo);

			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._9;
			AssertNoMessageErrors(gaApproval.CSI_SubTypeInfo);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			gaApproval = declaration.Invoices[0].JobComInvoiceLines[0].GAApprovalDataCollection.AddNew();

			gaApproval.CSI_SubType = "";
			AssertHasMessageErrorContaining(gaApproval.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);

			gaApproval.CSI_SubType = "5";
			AssertHasMessageErrorContaining(gaApproval.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError);

			gaApproval.CSI_SubType = CommodityUsageCodeList.Codes._01;
			AssertNoMessageErrors(gaApproval.CSI_SubTypeInfo);

			gaApproval.CSI_SubType = CommodityUsageCodeList.Codes._01;
			AssertNoMessageErrors(gaApproval.CSI_SubTypeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceline = invoice.InvoiceLines.AddNew();
			gaApproval = invoiceline.GAApprovalDataCollection.AddNew();
		}
		JobDeclaration declaration;
		GAApproval gaApproval;
	}
}
