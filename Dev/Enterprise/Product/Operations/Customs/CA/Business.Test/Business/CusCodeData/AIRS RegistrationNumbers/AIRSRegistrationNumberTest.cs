using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AIRSRegistrationNumber))]
	sealed class AIRSRegistrationNumberTest : Customs.Business.Testing.CusCodeDataTest<AIRSRegistrationNumber>
	{
		public void TestDefaultCY_DataWhenSafeFoodForCanadiansLicence()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TSTIMPORTER";
			var orgImpAddInfo = OrgImpAddInfo.Get(importer);
			var license = orgImpAddInfo.SafeFoodLicenses.AddNew();
			license.CY_Code = "SFC";
			license.CY_Data = "SafeFoodLicense";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;

			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;
			var cfia = invoiceLine.CFIAPGAHeader;
			var number = cfia.AIRSRegistrationNumbers.AddNew();
			AssertEquals(ZString.Empty, number.CY_Data);

			number.CY_Code = RegistrationNumberHelper.SafeFoodForCanadiansLicence;
			AssertEquals("SFC", number.CY_Data);

			number.CY_Code = ZString.Empty;
			number.CY_Data = "A";
			number.CY_Code = RegistrationNumberHelper.SafeFoodForCanadiansLicence;
			AssertEquals("A", number.CY_Data);
		}

		public void TestAIRSRegistrationDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(this.Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIAAIRSRegistrationType, "AIRS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIAAIRSRegistrationType, "123", "TEST1", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIAAIRSRegistrationType, "ABC", "TEST2", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			this.Factory.Save();

			var airsRegistrationNumber = this.Factory.New<AIRSRegistrationNumber>();
			airsRegistrationNumber.CY_Code = "123";
			AssertEquals("TEST1", airsRegistrationNumber.CY_Description);

			airsRegistrationNumber.CY_Code = "ABC";
			AssertEquals("TEST2", airsRegistrationNumber.CY_Description);
		}

		public void TestDefaultDataWhenCodeFormatIsConfirmation()
		{
			var startDate = ZDateTime.UtcToday.AddDays(-1);
			var endDate = ZDateTime.UtcToday.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Canada);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIAAIRSRegistrationType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIAAIRSRegistrationType);
			var code = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "TTT", "Test code", startDate, endDate);
			var attributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CFIAFormat, "CFIA format", codeType.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attribute = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeName.ZXE_Name, RegistrationNumberHelper.Confirmation);
			Factory.Save();

			var regNum = Factory.New<AIRSRegistrationNumber>();
			regNum.CY_Code = code.ZZD_Code;
			AssertEquals(YesNoList.Codes.Yes, regNum.CY_Data);
		}

		public void TestValidation()
		{
			var regNum = Factory.New<AIRSRegistrationNumber>();
			AssertEquals("Validation", typeof(AIRSRegistrationNumberValidation), regNum.Validation.GetType());
		}

		public void TestLookups()
		{
			var regNum = Factory.New<AIRSRegistrationNumber>();
			AssertEquals("Lookups", typeof(AIRSRegistrationNumberLookups), regNum.Lookups.GetType());
		}

		public void TestSetDefaultValues()
		{
			var regNum = Factory.New<AIRSRegistrationNumber>();
			AssertEquals(CusCodeDataTypeList.Codes.AIRSNumber, regNum.CY_Type);
		}

		public void TestParents()
		{
			var regNum = Factory.New<AIRSRegistrationNumber>();
			regNum.CY_ParentID = Header.PK;
			regNum.CY_ParentTableCode = Header.TablePrefix;
			AssertEquals(Header, regNum.Parent);
		}

		public void TestRunPreSaveValidationCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;

			var cfia = invoiceLine.CFIAPGAHeader;
			cfia.CA_AllProgramInd = YesNoList.Codes.Yes;
			var airsRegistrationNo = cfia.AIRSRegistrationNumbers.AddNew();
			airsRegistrationNo.CY_Code = "5001";
			airsRegistrationNo.RunPreSaveValidation();
			AssertHasMessageErrorContaining(airsRegistrationNo.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_CFIAInd = YesNoList.Codes.Yes;

			var cfia2 = invoiceLine2.CFIAPGAHeader;
			cfia2.CA_AllProgramInd = YesNoList.Codes.Yes;
			var airsRegistrationNo2 = cfia2.AIRSRegistrationNumbers.AddNew();
			airsRegistrationNo2.CY_Code = "5001";
			airsRegistrationNo2.RunPreSaveValidation();
			AssertNoMessageErrorContaining(airsRegistrationNo2.CY_DataInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Header.AIRSRegistrationNumbers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = "Y";
			var header = invoiceLine.CFIAPGAHeader;
			return header.AIRSRegistrationNumbers.AddNew();
		}

		CFIAPGAHeader Header
		{
			get { return header ?? (header = Factory.New<CFIAPGAHeader>()); }
		}
		CFIAPGAHeader header;
	}
}
