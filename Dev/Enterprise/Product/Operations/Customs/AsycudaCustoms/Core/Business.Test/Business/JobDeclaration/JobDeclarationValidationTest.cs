using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	sealed class JobDeclarationValidationTest : Customs.Business.Testing.BaseJobDeclarationValidationTest<JobDeclaration>
	{
		public void TestCheckJE_MessageType_ErrorIfChangedAfterMerge()
		{
			AssertErrorsAfterChangingJE_MessageType(false, false, true);
		}

		public void TestCheckJE_MessageType_MessageErrorIfControllerChangedAfterMerge()
		{
			AssertErrorsAfterChangingJE_MessageType(true, false, false);
		}

		public void TestCheckJE_MessageType_MessageErrorIfChangedAfterMerge()
		{
			AssertErrorsAfterChangingJE_MessageType(false, true, false);
		}

		public void TestDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(declaration.Validation.Declaration, declaration);
		}

		public void TestCheckJE_ManifestNumber()
		{
			const string message = "Manifest Number should be alphanumeric.";

			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.Validation.ValidateJE_ManifestNumber();
				AssertNoMessageError("No errors when JE_ManifestNumber is empty", declaration.JE_ManifestNumberInfo, message);

				declaration.JE_ManifestNumber = "@ASDF78901234567";
				AssertHasMessageError("Has errors when JE_ManifestNumber is not alphanumeric and not empty", declaration.JE_ManifestNumberInfo, message);

				declaration.JE_ManifestNumber = "ASDFG78901234567";
				AssertNoMessageError("No errors when JE_ManifestNumber is alphanumeric", declaration.JE_ManifestNumberInfo, message);
			});
		}

		public void TestCheckJE_DeclarationType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGroupingCode = Core.Constants.CountryCodes.Botswana;
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty;
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, "Botswana");
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Entry style");
			helper.CreateNewOrGetExistingCusCodeList(dataGroupingCode, codeType, "E1", "ensty1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(dataGroupingCode, codeType, "E2", "ensty2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			helper.CreateRefCusProcedure(dataGroupingCode, "B", "33", "33", "333", "Three", "IMP", group: "E1");
			helper.CreateRefCusProcedure(dataGroupingCode, "A", "55", "55", "555", "Five", "IMP", group: "E2");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_DeclarationType = "E2";
			AssertEquals("E2", instruction.CEI_Style);
			AssertNoMessageErrors(declaration.JE_DeclarationTypeInfo);
			instruction.CEI_Style = "E1";
			AssertEquals("E1", declaration.JE_DeclarationType);
			AssertNoMessageErrors(declaration.JE_DeclarationTypeInfo);
			declaration.JE_DeclarationType = "ZZ";
			AssertHasMessageError(declaration.JE_DeclarationTypeInfo, ListValidation.InvalidCodeMessageError);
			instruction.CEI_Style = "ZZ";
			declaration.Validation.ValidateJE_DeclarationType();
			AssertHasMessageError(declaration.JE_DeclarationTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckJE_OH_SupplierIsRequiredForEXPBondedWarehouse()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			const string dataGroupingCode = Core.Constants.CountryCodes.Botswana;
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, "Botswana");
			Factory.Save();
			helper.CreateRefCusProcedure(dataGroupingCode, "A", "33", "33", "333", "Three", "EXP", group: "E1", outOfWarehouse: true);
			helper.CreateRefCusProcedure(dataGroupingCode, "A", "33", "55", "555", "Five", "EXP", group: "E1", outOfWarehouse: false);
			Factory.Save();

			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.TermNameForBondedWarehouseReturns = "Bob's System";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "E1";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = "3333333";
			AssertEquals(true, invoiceLine.HasOutOfWarehouseProcedure);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			org.OH_IsWarehouseClient = false;
			declaration.JE_OH_Supplier = org.PK;
			const string messageError = "A Supplier marked as a Warehouse Client is needed when Bob's System is enabled.";
			AssertHasMessageError(declaration.JE_OH_SupplierInfo, messageError);
			org.CompanyData.OB_IMUsedBondedWhs = false;
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertNoMessageError(declaration.JE_OH_SupplierInfo, messageError);
			org.CompanyData.OB_IMUsedBondedWhs = true;
			invoiceLine.JI_Procedure = "3355555";
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertNoMessageError(declaration.JE_OH_SupplierInfo, messageError);
			invoiceLine.JI_Procedure = "3333333";
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertNoMessageError(declaration.JE_OH_SupplierInfo, messageError);
			instruction.CEI_OA_Warehouse = org.MainAddress.PK;
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertHasMessageError(declaration.JE_OH_SupplierInfo, messageError);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Validation.ValidateJE_OH_Supplier();
			AssertNoMessageError(declaration.JE_OH_SupplierInfo, messageError);
		}

		public void TestCheckJE_OH_ImporterIsRequiredForIMPBondedWarehouse()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			const string dataGroupingCode = Core.Constants.CountryCodes.Botswana;
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, "Botswana");
			Factory.Save();
			helper.CreateRefCusProcedure(dataGroupingCode, "A", "33", "33", "333", "Three", "IMP", group: "E1", intoWarehouse: true);
			helper.CreateRefCusProcedure(dataGroupingCode, "A", "33", "55", "555", "Five", "IMP", group: "E1", intoWarehouse: false);
			helper.CreateRefCusProcedure(dataGroupingCode, "A", "33", "66", "666", "Six", "IMP", group: "E1", outOfWarehouse: true);
			Factory.Save();

			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.TermNameForBondedWarehouseReturns = "Bob's System";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "E1";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = "3333333";
			AssertEquals(true, invoiceLine.HasIntoWarehouseProcedure);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			org.OH_IsWarehouseClient = false;
			declaration.JE_OH_Importer = org.PK;
			const string messageError = "An Importer marked as a Warehouse Client is needed when Bob's System is enabled.";
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, messageError);
			org.CompanyData.OB_IMUsedBondedWhs = false;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);
			org.CompanyData.OB_IMUsedBondedWhs = true;
			invoiceLine.JI_Procedure = "3355555";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);
			invoiceLine.JI_Procedure = "3366666";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, messageError);
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);
			instruction.CEI_OA_Warehouse = org.MainAddress.PK;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, messageError);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, messageError);
		}

		void AssertErrorsAfterChangingJE_MessageType(bool setUserIsController, bool messageErrorOverride, bool shouldBeError)
		{
			const string expectedErrorText = "Once an entry has been merged, Shipment Type should not change.";
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			using (Env.CurrentUser.SetIsControllerOverrideForTesting(setUserIsController))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.BlueErrorMessageTypeAfterMessaging, declaration.GetDefaultDataGroupingCode(), ZDateTime.Today, messageErrorOverride))
			{
				CombineAssertions(() =>
				{
					declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
					AssertEquals("No expected Shipment Type notifications", true, declaration.JE_MessageTypeInfo.Notifications == null || !declaration.JE_MessageTypeInfo.Notifications.ContainsNotificationContaining(expectedErrorText));
					var invoice = declaration.Invoices.AddNew();
					invoice.InvoiceLines.AddNew();
					Factory.Save();
					declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
					declaration.DoMerge();
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
					if (shouldBeError)
					{
						AssertHasError("Error for MessageType change", declaration.JE_MessageTypeInfo, expectedErrorText);
					}
					else
					{
						AssertHasMessageError("Message Error for MessageType change", declaration.JE_MessageTypeInfo, expectedErrorText);
					}
				});
			}
		}

		sealed class JobDeclarationForTesting : JobDeclaration
		{
			public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public string TermNameForBondedWarehouseReturns { get; set; }

			public override string TermNameForBondedWarehouse => TermNameForBondedWarehouseReturns;
		}
	}
}
