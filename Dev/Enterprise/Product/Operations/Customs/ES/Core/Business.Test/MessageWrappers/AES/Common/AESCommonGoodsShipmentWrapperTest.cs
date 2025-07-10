using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AESCommonGoodsShipmentWrapperTest : WrapperHelperTest<AESCommonGoodsShipmentWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if entryHeader is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryHeader"), () => GetWrapper(null));

				var entryHeader = Factory.New<CusEntryHeader>();
				AssertExceptionThrown("Constructor Throws Exception if jobDeclaration is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","Declaration"), () => GetWrapper(entryHeader));
			});
		}

		public void TestWarehouse()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Spain, "", "10", "76", "123", "Desc", "EXP", outOfWarehouse: true);
			Factory.Save();

			CombineAssertions(() =>
			{
				var auth1 = entryInstruction.CusAuthorizationUsages.AddNew();
				auth1.AGC_Code = "CW1";

				wrapper = GetWrapper(entryHeader);
				AssertNull("Expected empty Warehouse when entryInstruction is not B nor C but OfficeOfPresentation is not declared", wrapper.Warehouse);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				var customsOffice1 = declaration.CustomsOffices.AddNew();
				customsOffice1.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfPresentation;
				customsOffice1.CY_Data = "FR008889";
				wrapper = GetWrapper(entryHeader);
				AssertNull("Expected empty Warehouse when entryInstruction is not B nor C and OfficeOfPresentation is declared but there is no 71, 76 or 77 previous procedure declared in lines", wrapper.Warehouse);

				invoiceLine.JI_Procedure = "1049123";
				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Procedure = "1076123";
				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader);
				var warehouse = wrapper.Warehouse;
				AssertNotNull("Expected filled Warehouse when entryInstruction is not B nor C and OfficeOfPresentation is declared and there is at least one 71, 76 or 77 previous procedure declared in lines", warehouse);
				AssertSame("Cached Warehouse", wrapper.Warehouse, warehouse);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
				wrapper = GetWrapper(entryHeader);
				AssertNull("Expected empty Warehouse when entryInstruction is B and ComplX flag is false", wrapper.Warehouse);

				wrapper = GetWrapper(entryHeader, isComplX: true);
				AssertNotNull("Expected filled Warehouse when entryInstruction is B but isComplX flag is true and OfficeOfPresentation is declared and there is at least one 71, 76 or 77 previous procedure declared in lines", warehouse);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
				wrapper = GetWrapper(entryHeader);
				AssertNull("Expected empty Warehouse when entryInstruction is C and isComplementaryCWithMRN is false", wrapper.Warehouse);

				wrapper = GetWrapper(entryHeader, isComplementaryCWithMRN: true);
				AssertNotNull("Expected filled Warehouse when entryInstruction is C, isComplX flag is false but isComplementaryCWithMRN is true and OfficeOfPresentation is declared and there is at least one 71, 76 or 77 previous procedure declared in lines", warehouse);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				auth1.AGC_Code = "AAA";
				wrapper = GetWrapper(entryHeader);
				AssertNull("Expected empty Warehouse when all conditions are met but authorization code is not in the expected list", wrapper.Warehouse);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "11";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = GetWrapper(entryHeader);
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		AESCommonGoodsShipmentWrapper wrapper;

		AESCommonGoodsShipmentWrapper GetWrapper(CusEntryHeader entryHeader, bool isComplX = false, bool isComplementaryCWithMRN = false) => new AESCommonGoodsShipmentWrapper(entryHeader, isComplX, isComplementaryCWithMRN);

		protected override AESCommonGoodsShipmentWrapper GetProvider() => wrapper;
	}
}
