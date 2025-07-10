using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

public class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
{
	public void TestParent()
	{
		var parent = Factory.New<JobComInvoiceLine>();
		AssertEquals(parent.Validation.Parent, parent);
	}

	public void TestHumanReadableNameNetWeight()
	{
		var jobInvoice = Factory.New<JobComInvoiceLine>();
		var info = jobInvoice.JI_NetWeightInfo;
		AssertEquals("Net Weight", info.HumanReadableName);
	}

	public void TestHumanReadableNameCustomsQty()
	{
		var jobInvoice = Factory.New<JobComInvoiceLine>();
		var info = jobInvoice.JI_CustomsQuantityInfo;
		AssertEquals("[38] Customs Qty", info.HumanReadableName);
	}

	public void TestJI_CustomsQuantity()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var expectedMessageError = "Customs Quantity[38] can not be greater than GWT[35]";

		invoiceLine.JI_Weight = 10m;
		invoiceLine.JI_WeightUQ = "KG";

		CombineAssertions(() =>
		{
			invoiceLine.JI_CustomsQuantity = 9m;
			AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, expectedMessageError);

			invoiceLine.JI_CustomsQuantity = 12m;
			AssertHasMessageError(invoiceLine.JI_CustomsQuantityInfo, expectedMessageError);

			invoiceLine.JI_CustomsQuantity = 10m;
			AssertNoMessageError(invoiceLine.JI_CustomsQuantityInfo, expectedMessageError);
		});
	}

	public void TestValidateDangerousGoods()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

		var expectedMessageError = "In provisional period, only one Dangerous Goods Code can be used.";

		CombineAssertions(() =>
		{
			invoiceLine.UNDGs.AddNew();
			invoiceLine.UNDGs.AddNew();

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				invoiceLine.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(invoiceLine, expectedMessageError);

				invoiceLine.UNDGs.DeleteAll();
				invoiceLine.UNDGs.AddNew();
				invoiceLine.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(invoiceLine, expectedMessageError);
			}

			invoiceLine.UNDGs.AddNew();
			invoiceLine.UNDGs.AddNew();

			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
			{
				invoiceLine.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(invoiceLine, expectedMessageError);
			}
		});
	}

	public void TestCheckMaxNumberOfContainers()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();

		for (int i = 0; i < 100; i++)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = $"CONTAINER{i}";

			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(container.CO_ContainerNumber).IsForInvoiceLine = true;
		}

		CombineAssertions(() =>
		{
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, "Customs will not accept a declaration with more than 99 containers per line.");

			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber("CONTAINER2").IsForInvoiceLine = false;
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, "Customs will not accept a declaration with more than 99 containers per line.");
		});
	}

	public void TestOnePreviousDocumentPerEntryLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();

		var previousDoc = invoiceLine.PreviousDocuments.AddNew();
		SetDataForPreviousDocument(previousDoc, "AA", 1);
		var previousDoc2 = invoiceLine.PreviousDocuments.AddNew();
		SetDataForPreviousDocument(previousDoc2, "BB", 2);

		CombineAssertions(() =>
		{
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, "Customs will not accept a declaration with more than 1 previous document for Box 40 per line");

			previousDoc2.Delete();
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, "Customs will not accept a declaration with more than 1 previous document for Box 40 per line");
		});
	}

	void SetDataForPreviousDocument(PreviousDocument doc, string code, ZShort lineNo)
	{
		doc.CSI_Code = code;
		doc.CSI_DateOfIssue = ZDateTime.BrettsBirthday;
		doc.CSI_LineNo = lineNo;
	}

	public void TestEmptyPackaging_WithoutEntryLine()
	{
		Factory.SetBulkTypeHelper();
		var messageErrorText = "This line has no packaging details";
		var declaration = Factory.New<JobDeclaration>();
		var invHeader = declaration.Invoices.AddNew();
		var invoiceLine = invHeader.InvoiceLines.AddNew();

		declaration.JE_MasterBill = "M";
		var packingGroup = declaration.Bills[0].PackingGroups[0];
		var cw1 = packingGroup.Packages.AddNew();
		cw1.CW_PackQty = 1;
		cw1.CW_PackType = "CT";
		var cw2 = packingGroup.Packages.AddNew();
		cw2.CW_PackQty = 1;
		cw2.CW_PackType = "VG";

		CombineAssertions(() =>
		{
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, messageErrorText);
			var pack1 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw1);
			pack1.CHC_NumberOfPacks = 1;
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, messageErrorText);
			pack1.CHC_NumberOfPacks = 0;
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, messageErrorText);

			pack1.Delete();
			var pack2 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw2);
			pack2.CHC_NumberOfPacks = 1;
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, messageErrorText);
			pack2.CHC_NumberOfPacks = 0;
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, messageErrorText);

			pack1 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw1);
			pack1.CHC_NumberOfPacks = 1;
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, messageErrorText);
			pack1.CHC_NumberOfPacks = 0;
			pack2.CHC_NumberOfPacks = 1;
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, messageErrorText);

			pack1.Delete();
			pack2.Delete();
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, messageErrorText);

			var vehicle = invoiceLine.Vehicles.AddNew();
			vehicle.CVH_VehicleIdentificationNumber = "aaaaa";
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, messageErrorText);
		});
	}

	public void TestEmptyPackaging_WithEntryLine()
	{
		Factory.SetBulkTypeHelper();
		var messageErrorText = "This line has no packaging details";
		var declaration = Factory.New<JobDeclaration>();
		var invHeader = declaration.Invoices.AddNew();
		var invoiceLine = invHeader.InvoiceLines.AddNew();

		var entryLine = declaration.CustomsEntryHeaders.AddNew().AllEntryLines.AddNew();
		entryLine.CL_LineNumber = 1;
		invoiceLine.JI_CL = entryLine.PK;

		declaration.JE_MasterBill = "M";
		var packingGroup = declaration.Bills[0].PackingGroups[0];
		var cw1 = packingGroup.Packages.AddNew();
		cw1.CW_PackQty = 1;
		cw1.CW_PackType = "CT";
		var cw2 = packingGroup.Packages.AddNew();
		cw2.CW_PackQty = 1;
		cw2.CW_PackType = "VG";

		CombineAssertions(() =>
		{
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, messageErrorText);
			var pack1 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw1);
			pack1.CHC_NumberOfPacks = 1;
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, messageErrorText);
			pack1.CHC_NumberOfPacks = 0;
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, messageErrorText);

			pack1.Delete();
			var pack2 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw2);
			pack2.CHC_NumberOfPacks = 1;
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, messageErrorText);
			pack2.CHC_NumberOfPacks = 0;
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, messageErrorText);

			pack1 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw1);
			pack1.CHC_NumberOfPacks = 1;
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, messageErrorText);
			pack1.CHC_NumberOfPacks = 0;
			pack2.CHC_NumberOfPacks = 1;
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, messageErrorText);

			entryLine.CL_LineNumber = 2;
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, messageErrorText);

			pack1.Delete();
			pack2.Delete();
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, messageErrorText);

			var vehicle = invoiceLine.Vehicles.AddNew();
			vehicle.CVH_VehicleIdentificationNumber = "aaaaa";
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(invoiceLine, messageErrorText);
		});
	}

	public void TestCheckCHC_NumberOfPacks()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "BK", "Bulk", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

		var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
		package1.CW_PackQty = 2;
		package1.CW_PackType = "BK";
		package1.CW_MarksAndNos = "AAAAAAAAAAAA";

		var package2 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
		package2.CW_PackQty = 2;
		package2.CW_PackType = "BK";
		package2.CW_MarksAndNos = "AAAAAAAAAAAA";

		var packing1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
		packing1.IsLinked = true;
		packing1.PackQty = 0;

		var packing2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0];
		packing2.IsLinked = true;
		packing2.PackQty = 0;

		var invoiceLine1PackagePivot = invoiceLine1.PackagesPivot.Cast<InvoiceLinePackagePivot>().FirstOrDefault();

		var entryLine = Factory.New<CusEntryLine>();

		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine2.JI_CL = entryLine.PK;

		AssertEquals(2, entryLine.PackagingDetails.Count());

		entryLine.CL_LineNumber = 1;
		invoiceLine1.PackagesPivot.RunPreSaveValidation();
		AssertNoMessageErrorContaining("Bulk type of packs always allow 0 as pack number whatever the entry line it eventually applies to", invoiceLine1PackagePivot.CHC_NumberOfPacksInfo, "You have not entered");

		package1.CW_PackType = "CT";
		invoiceLine1.PackagesPivot.RunPreSaveValidation();
		AssertHasMessageErrorContaining("Non bulk type of packs don't allow 0 as pack number when the entry line it eventually applies to is the first of its entry and shows a package count of 0", invoiceLine1PackagePivot.CHC_NumberOfPacksInfo, "You have not entered");

		entryLine.CL_LineNumber = 2;
		invoiceLine1.PackagesPivot.RunPreSaveValidation();
		AssertNoMessageErrorContaining("Non bulk type of packs allow 0 as pack number when the entry line it eventually applies to is not the first of its entry ", invoiceLine1PackagePivot.CHC_NumberOfPacksInfo, "You have not entered");

		entryLine.CL_LineNumber = 1;
		packing2.PackQty = 2;
		invoiceLine1.PackagesPivot.RunPreSaveValidation();
		AssertNoMessageErrorContaining("Non bulk type of packs allow 0 as pack number when the entry line it eventually applies to is the first of its entry but shows a package count > 0 ", invoiceLine1PackagePivot.CHC_NumberOfPacksInfo, "You have not entered");
	}

	public void TestCheckJI_CEI()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertNoMessageErrorContaining("Assert mandatory JI_CEI has value for Export", invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JI_CEI has no value for Export", invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertNoMessageErrorContaining("Assert mandatory JI_CEI has value for Import", invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JI_CEI has no value for Import", invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertNoMessageErrorContaining("Assert mandatory JI_CEI has value for T2LExpedition", invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JI_CEI has no value for T2LExpedition", invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertNoMessageErrorContaining("Assert mandatory JI_CEI has value for T2LClearance export", invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JI_CEI has no value for T2LClearance export", invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertNoMessageErrorContaining("Assert mandatory JI_CEI has value for T2LReception", invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JI_CEI has no value for T2LReception", invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertNoMessageErrorContaining("Assert mandatory JI_CEI has value for T2LClearance import", invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertHasMessageErrorContaining("Assert mandatory JI_CEI has no value for T2LClearance import", invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckJI_Weight()
	{
		CombineAssertions(() =>
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();

			invoiceLine.JI_Weight = -1;
			AssertHasMessageErrorContaining("Negative", invoiceLine.JI_WeightInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.JI_Weight = 0;
			AssertHasMessageErrorContaining("Zero", invoiceLine.JI_WeightInfo, MandatoryValidation.ValueCannotBeZero);

			invoiceLine.JI_Weight = 1;
			AssertNoMessageErrorContaining("Not Negative", invoiceLine.JI_WeightInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining("Not Zero", invoiceLine.JI_WeightInfo, MandatoryValidation.ValueCannotBeZero);
		});
	}

	public void TestCheckJI_NetWeight()
	{
		CombineAssertions(() =>
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();

			invoiceLine.JI_NetWeight = -1;
			AssertHasMessageErrorContaining("Negative", invoiceLine.JI_NetWeightInfo, MandatoryValidation.ValueCannotBeNegative);

			invoiceLine.JI_NetWeight = 1;
			AssertNoMessageErrorContaining("Not Negative", invoiceLine.JI_NetWeightInfo, MandatoryValidation.ValueCannotBeNegative);
		});
	}

	public void TestCheckJI_TariffHasWarningIfConditionsShouldNotApply_ForImport()
	{
		var message = @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        Direction:Either: YBDesc AND
        Direction:Import: (XADesc or XBDesc) and XCDesc and (XDDesc or XEDesc)
            (Please refer to: www.google.com)
    Test Ctrl Condition Type 2:
        cond2_1: ZADesc";

		var tariff = SetupTariff(Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
		SetupConditionData(tariff);

		CombineAssertions("For Import", () =>
		{
			var invoiceLine = CreateInvoiceLine();
			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("A message error is expected when no CustomsEntryInstructions exists.", invoiceLine.JI_TariffInfo, message);

			var declaration = invoiceLine.Declaration;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			instruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasWarningContaining("A warning is expected when CustomsEntryInstructions.CEI_Style has value H2.", invoiceLine.JI_TariffInfo, message);

			instruction.CEI_Style = IMPDeclarationTypeList.Codes.IM;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("A message error is expected when CustomsEntryInstructions.CEI_Style has value other than H2.", invoiceLine.JI_TariffInfo, message);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasWarningContaining("A warning is expected when CustomsEntryInstructions.CEI_SubStyle = 'T2L'.", invoiceLine.JI_TariffInfo, message);

			instruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("A message error is expected when CustomsEntryInstructions.CEI_SubStyle has value other than T2L and T2C.", invoiceLine.JI_TariffInfo, message);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasWarningContaining("A warning is expected when CustomsEntryInstructions.CEI_SubStyle = 'T2C'.", invoiceLine.JI_TariffInfo, message);
		});
	}

	public void TestCheckJI_TariffHasWarningIfConditionsShouldNotApply_ForExport()
	{
		var message = @"The Customs Control condition is not satisfied
    Test Ctrl Condition Type 1:
        Direction:Either: YBDesc AND
        Direction:Export: YADesc
    Test Ctrl Condition Type 2:
        cond2_1: ZADesc";

		var tariff = SetupTariff(Customs.Business.UniversalReferenceConstants.CusTariffTypes.ExportTariff);
		SetupConditionData(tariff);

		CombineAssertions("For Export", () =>
		{
			var invoiceLine = CreateInvoiceLine();
			invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("A message error is expected when no CustomsEntryInstructions exists.", invoiceLine.JI_TariffInfo, message);

			var declaration = invoiceLine.Declaration;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			instruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.T2L;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasWarningContaining("A warning is expected when CustomsEntryInstructions.CEI_SubStyle has value T2L.", invoiceLine.JI_TariffInfo, message);

			instruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("A message error is expected when CustomsEntryInstructions.CEI_SubStyle has value other than T2L and EXS. (Example Substyle = 'A').", invoiceLine.JI_TariffInfo, message);

			instruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasWarningContaining("A warning is expected when CustomsEntryInstructions.CEI_SubStyle has value EXS.", invoiceLine.JI_TariffInfo, message);

			instruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.B;
			invoiceLine.Validation.ValidateJI_Tariff();
			AssertHasMessageErrorContaining("A message error is expected when CustomsEntryInstructions.CEI_SubStyle has value other than T2L and EXS. (Example Substyle = 'B').", invoiceLine.JI_TariffInfo, message);
		});
	}

	sealed class JobComInvoiceLineForConditionTest : JobComInvoiceLine
	{
		public JobComInvoiceLineForConditionTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString AdditionalCode => JI_SecondaryPreference;

		protected override bool UseUniversalConditionCheck => true;

		public override ConditionChecker.EvaluateConditionValue EvaluateConditionValue => (_, __, x) => false;
		public override ConditionChecker.GetFriendlyConditionValue GetFriendlyConditionValue => (_, __, input) => input + "Desc";
	}

	JobComInvoiceLineForConditionTest CreateInvoiceLine()
	{
		var declaration = Factory.New<BaseJobDeclaration>();
		var invoiceLine = Factory.New<JobComInvoiceLineForConditionTest>();
		invoiceLine.JI_JZ = declaration.Invoices.AddNew().PK;
		invoiceLine.JI_Tariff = "1234567890";
		return invoiceLine;
	}

	void SetupConditionData(TariffView tariff)
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var countryCode = GlbCompany.CurrentCompany.Country.RN_Code;

		var ctrlType1 = helper.CreateOrGetExistingRefCusConditionType(countryCode, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "TSTC1", "Test Ctrl Condition Type 1");
		var ctrlType2 = helper.CreateOrGetExistingRefCusConditionType(countryCode, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "TSTC2", "Test Ctrl Condition Type 2");
		Factory.Save();

		var testValueType = helper.CreateOrGetExistingRefCusConditionValueType(countryCode, "TSTVT");

		var testCondCtrl1_1 = helper.CreateOrGetExistingRefCusCondition(countryCode, ctrlType1.PK, tariff.PK, "Direction:Import", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		testCondCtrl1_1.ZX1_Source = "www.google.com";
		testCondCtrl1_1.Factory.Save();
		helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_1.PK, "XA", c => c.ZX3_LogicalORWithinGroup = 0);
		helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_1.PK, "XB", c => c.ZX3_LogicalORWithinGroup = 0);
		helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_1.PK, "XC", c => c.ZX3_LogicalORWithinGroup = 1);
		helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_1.PK, "XD", c => c.ZX3_LogicalORWithinGroup = 2);
		helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_1.PK, "XE", c => c.ZX3_LogicalORWithinGroup = 2);

		var testCondCtrl1_2 = helper.CreateOrGetExistingRefCusCondition(countryCode, ctrlType1.PK, tariff.PK, "Direction:Export", false, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_2.PK, "YA");

		var testCondCtrl1_3 = helper.CreateOrGetExistingRefCusCondition(countryCode, ctrlType1.PK, tariff.PK, "Direction:Either", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl1_3.PK, "YB");

		var testCondCtrl2_1 = helper.CreateOrGetExistingRefCusCondition(countryCode, ctrlType2.PK, tariff.PK, "cond2_1", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		testCondCtrl2_1.Factory.Save();
		helper.CreateOrGetExistingRefCusConditionValue(testValueType.PK, testCondCtrl2_1.PK, "ZA");
		Factory.Save();
	}

	TariffView SetupTariff(ZString type)
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var countryCode = GlbCompany.CurrentCompany.Country.RN_Code;
		var tariffType = helper.CreateNewOrGetExistingTariffType(countryCode, type);
		Factory.Save();

		var tariff = helper.CreateTariff(countryCode, tariffType.PK, "1234567890", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
		return tariff;
	}
}
