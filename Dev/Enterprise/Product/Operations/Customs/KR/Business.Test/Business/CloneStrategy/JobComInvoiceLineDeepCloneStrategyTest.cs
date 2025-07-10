using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	class JobComInvoiceLineDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCloneForCOOCusSupportingInfo()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine.CertificateOfOriginIssueStatus = CertificateOfOriginIssuedCodeList.Codes.Y;
			invoiceLine.CriteriaForDeterminingCountryOfOrigin = CountryOfOriginDeterminationRuleCodeList.Codes.A;
			invoiceLine.JI_COOLabelLocation = CountryOfOriginLabelLocationCodeList.Codes.B;

			Factory.Save();

			var clonedInvoiceLine = (JobComInvoiceLine)new JobComInvoiceLineDeepCloneStrategy(invoiceLine, CloneType.TemplateCopy, invoice, null).Clone();
			CombineAssertions(() =>
			{
				AssertEquals("JI_CountryOfOrigin should be the same", invoiceLine.JI_CountryOfOrigin, clonedInvoiceLine.JI_CountryOfOrigin);
				AssertEquals("CertificateOfOriginIssueStatus should be the same", invoiceLine.CertificateOfOriginIssueStatus, clonedInvoiceLine.CertificateOfOriginIssueStatus);
				AssertEquals("CriteriaForDeterminingCountryOfOrigin should be the same", invoiceLine.CriteriaForDeterminingCountryOfOrigin, clonedInvoiceLine.CriteriaForDeterminingCountryOfOrigin);
				AssertEquals("JI_COOLabelLocation should be the same", invoiceLine.JI_COOLabelLocation, clonedInvoiceLine.JI_COOLabelLocation);
				AssertEquals("JI_PrimaryPreference should be the same", invoiceLine.JI_PrimaryPreference, clonedInvoiceLine.JI_PrimaryPreference);
			});
		}

		public void TestCloneForApprovalDocuments()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.ENGAR, "Export - Non Government Agency Approval Reason type");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			var testData1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.ENGAR, "69102", "마약류 관리에 관한 법률 제2조 제3호 마목 단서에 따른 신체적 또는 정신적 의존성을 야기하지 아니하는 제제", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeListAttribute(testData1.PK, "MandatoryDocWhenExempt", "향정신성의약품 제외인정 신청서");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var gaApproval = invoiceLine.GAApprovalDataCollection.AddNew();
			gaApproval.CSI_Procedure = "69";
			gaApproval.CSI_SubType = RequirementTypeCodeList.Codes._1;
			gaApproval.CSI_Code = "A";
			gaApproval.CSI_ReferenceNumber = "AAA";
			gaApproval.CSI_DateOfIssue = ZDateTime.Today;
			gaApproval.CSI_ReferenceNumber2 = "BBB";
			gaApproval.NonGAReasonType = "69102";
			gaApproval.CSI_Description = "APPROVAL DOCUMENT DESCRIPTION";
			gaApproval.CSI_AdditionalDescription = "APPROVAL DOCUMENT ADDITIONALDESCRIPTION";

			var clonedInvoiceLine = (JobComInvoiceLine)new JobComInvoiceLineDeepCloneStrategy(invoiceLine, CloneType.TemplateCopy, invoice, null).Clone();
			CombineAssertions(() =>
			{
				var clonedGAApproval = clonedInvoiceLine.GAApprovalDataCollection.First();
				AssertEquals("GAApprovalDataCollection cloned", 1, clonedInvoiceLine.GAApprovalDataCollection.Count);
				AssertEquals("CSI_Procedure cloned", gaApproval.CSI_Procedure, clonedGAApproval.CSI_Procedure);
				AssertEquals("CSI_SubType cloned", gaApproval.CSI_SubType, clonedGAApproval.CSI_SubType);
				AssertEquals("CSI_Code cloned", gaApproval.CSI_Code, clonedGAApproval.CSI_Code);
				AssertEquals("CSI_ReferenceNumber cloned", gaApproval.CSI_ReferenceNumber, clonedGAApproval.CSI_ReferenceNumber);
				AssertEquals("CSI_DateOfIssue cloned", gaApproval.CSI_DateOfIssue, clonedGAApproval.CSI_DateOfIssue);
				AssertEquals("CSI_ReferenceNumber2 cloned", gaApproval.CSI_ReferenceNumber2, clonedGAApproval.CSI_ReferenceNumber2);
				AssertEquals("NonGAReasonType cloned", gaApproval.NonGAReasonType, clonedGAApproval.NonGAReasonType);
				AssertEquals("CSI_Description cloned", gaApproval.CSI_Description, clonedGAApproval.CSI_Description);
				AssertEquals("CSI_AdditionalDescription cloned", gaApproval.CSI_AdditionalDescription, clonedGAApproval.CSI_AdditionalDescription);
			});
		}

		public void TestCloneForSteelExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.PRA_ReferenceNumber = "KR00101010000";
			invoiceLine.PRA_DateOfIssue = new ZDateTime(2020, 01, 01);
			invoiceLine.PRA_DateOfExpiry = new ZDateTime(2020, 09, 01);

			var clonedInvoiceLine = (JobComInvoiceLine)new JobComInvoiceLineDeepCloneStrategy(invoiceLine, CloneType.TemplateCopy, invoice, null).Clone();
			CombineAssertions(() =>
			{
				AssertEquals("PRA_ReferenceNumber cloned", invoiceLine.PRA_ReferenceNumber, clonedInvoiceLine.PRA_ReferenceNumber);
				AssertEquals("PRA_DateOfIssue cloned", invoiceLine.PRA_DateOfIssue, clonedInvoiceLine.PRA_DateOfIssue);
				AssertEquals("PRA_DateOfExpiry cloned", invoiceLine.PRA_DateOfExpiry, clonedInvoiceLine.PRA_DateOfExpiry);
			});
		}

		public void TestCloneForSecondHandVehicles()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var vehicleNumber = invoiceLine.VehicleNumbers.AddNew();
			vehicleNumber.CY_Order = 3;
			vehicleNumber.CY_Data = "12345678909876543";

			var clonedInvoiceLine = (JobComInvoiceLine)new JobComInvoiceLineDeepCloneStrategy(invoiceLine, CloneType.TemplateCopy, invoice, null).Clone();
			CombineAssertions(() =>
			{
				var clonedVehicleNumber = clonedInvoiceLine.VehicleNumbers.First();
				AssertEquals("VehicleNumbers cloned", 1, clonedInvoiceLine.VehicleNumbers.Count);
				AssertEquals("CY_Order cloned", vehicleNumber.CY_Order, clonedVehicleNumber.CY_Order);
				AssertEquals("CY_Data cloned", vehicleNumber.CY_Data, clonedVehicleNumber.CY_Data);
			});
		}
	}
}
