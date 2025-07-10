using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class InvoiceLinePackagePivotValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCHC_NumberOfPacks_IMP() => AssertCheckCHC_NumberOfPacks(JobMessageTypeList.Codes.Import, ValidationMessages.InvoiceLinePackage.TotalLineQtyIsZero);

	public void TestCheckCHC_NumberOfPacks_NP70020() => AssertCheckCHC_NumberOfPacks(JobMessageTypeList.Codes.Export, PassarValidationMessages.MessageNP70020);

	public void TestCheckCHC_NumberOfPacks_NP70178()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var declarationBill = Declaration.Bills.AddNew();
		declarationBill.CU_BillType = BillTypeList.Codes.MasterBill;
		declarationBill.CU_BillNum = "999";

		Package.CW_PackQty = 0;
		Package.CW_HouseBill = declarationBill.CU_BillUniqueCode;

		var packagePivot1 = InvoiceLine.PackagesPivot.AddNew();
		packagePivot1.CHC_CW = Package.PK;
		packagePivot1.CHC_NumberOfPacks = 0;
		var packagePivot2 = InvoiceLine.PackagesPivot.AddNew();
		packagePivot2.CHC_CW = Package.PK;
		packagePivot2.CHC_NumberOfPacks = 0;
		var packagePivot3 = InvoiceLine.PackagesPivot.AddNew();
		packagePivot3.CHC_CW = Package.PK;
		packagePivot3.CHC_NumberOfPacks = 0;

		CombineAssertions(() =>
		{
			packagePivot1.Validation.ValidateCHC_NumberOfPacks();
			AssertNoMessageError("All CHC_NumberOfPacks are zero", packagePivot1.CHC_NumberOfPacksInfo, PassarValidationMessages.MessageNP70178);

			packagePivot2.CHC_NumberOfPacks = 3;
			packagePivot3.CHC_NumberOfPacks = 4;
			packagePivot1.Validation.ValidateCHC_NumberOfPacks();
			AssertHasMessageError("non-scoped CHC_NumberOfPacks arent zero", packagePivot1.CHC_NumberOfPacksInfo, PassarValidationMessages.MessageNP70178);

			packagePivot3.Validation.ValidateCHC_NumberOfPacks();
			AssertHasMessageError("scoped CHC_NumberOfPacks isnt zero, but at least one is", packagePivot3.CHC_NumberOfPacksInfo, PassarValidationMessages.MessageNP70178);
		});
	}

	void AssertCheckCHC_NumberOfPacks(string declarationType, string messageError)
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);

		Declaration.JE_MessageType = declarationType;

		Package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;
		var otherPackage = Declaration.Packages.AddNew();
		otherPackage.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;

		var invoiceLine1 = Invoice.InvoiceLines.AddNew();
		var invoiceLine2 = Invoice.InvoiceLines.AddNew();
		var invoiceLine3 = Invoice.InvoiceLines.AddNew();
		var invoiceLine4 = Invoice.InvoiceLines.AddNew();

		invoiceLine1.JI_Description = "Line1";
		invoiceLine2.JI_Description = "Line2";
		invoiceLine3.JI_Description = "Line3";
		invoiceLine4.JI_Description = "Line4";

		var bindingLine1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
		bindingLine1.IsLinked = true;

		var bindingLine2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0];
		bindingLine2.IsLinked = true;

		var bindingLine3 = invoiceLine3.PackagesForInvoiceLinesForBindingOnly[1];
		bindingLine3.IsLinked = true;

		var bindingLine4 = invoiceLine4.PackagesForInvoiceLinesForBindingOnly[0];
		bindingLine4.IsLinked = true;

		var instruction = Declaration.CustomsEntryInstructions.AddNew();
		var otherInstruction = Declaration.CustomsEntryInstructions.AddNew();

		invoiceLine1.JI_CEI = instruction.PK;
		invoiceLine2.JI_CEI = instruction.PK;
		invoiceLine3.JI_CEI = instruction.PK;
		invoiceLine4.JI_CEI = otherInstruction.PK;

		CombineAssertions(() =>
		{
			bindingLine1.PackQty = 0;
			bindingLine2.PackQty = 0;
			bindingLine1.Validation.ValidatePackQty();
			AssertHasMessageError("When sum = 0", bindingLine1.PackQtyInfo, messageError);

			bindingLine1.PackQty = 0;
			bindingLine2.PackQty = 0;
			bindingLine3.PackQty = 5;
			bindingLine1.Validation.ValidatePackQty();
			AssertHasMessageError("Don't take other packages into account", bindingLine1.PackQtyInfo, messageError);

			bindingLine1.PackQty = 0;
			bindingLine2.PackQty = 0;
			bindingLine4.PackQty = 6;
			bindingLine1.Validation.ValidatePackQty();
			AssertHasMessageError("Don't take other instructions into account", bindingLine1.PackQtyInfo, messageError);

			bindingLine1.PackQty = 8;
			bindingLine2.PackQty = 0;
			bindingLine1.Validation.ValidatePackQty();
			AssertNoMessageError("When PackQty > 0", bindingLine1.PackQtyInfo, messageError);

			bindingLine1.PackQty = 0;
			bindingLine2.PackQty = 7;
			bindingLine1.Validation.ValidatePackQty();
			AssertNoMessageError("When other PackQty > 0", bindingLine1.PackQtyInfo, messageError);

			Package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeWithBulkYes;
			bindingLine1.PackQty = 0;
			bindingLine2.PackQty = 0;
			bindingLine1.Validation.ValidatePackQty();
			AssertNoMessageError("When loose package", bindingLine1.PackQtyInfo, messageError);
			Package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;

			bindingLine1.IsLinked = false;
			bindingLine1.Validation.ValidatePackQty();
			AssertNoMessageError("When not linked", bindingLine1.PackQtyInfo, messageError);
			bindingLine1.IsLinked = true;
		});
	}

	public void TestCheckCHC_NumberOfPacks_R132()
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);
		AssertNumberOfPacksForLoosePackaging(CHJobMessageTypeList.Codes.Import, ValidationMessages.Plausi.R132);
	}

	public void TestCheckCHC_NumberOfPacks_NS30021()
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);
		AssertNumberOfPacksForLoosePackaging(CHJobMessageTypeList.Codes.Export, PassarValidationMessages.NS30021);
	}

	void AssertNumberOfPacksForLoosePackaging(string messageType, string ruleNumber) => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = messageType;

		var packQtyForLoosePackagingMessage = PassarValidationMessages.MessageNS30021R132_PackQtyForLoosePackaging(ruleNumber);

		AssertMessage(RefCusCodeTestHelper.UNPKGCodeWithBulkYes, 5, packQtyForLoosePackagingMessage);
		AssertMessage(RefCusCodeTestHelper.UNPKGCodeWithBreakBulkYes, 5, messageType == CHJobMessageTypeList.Codes.Import ? packQtyForLoosePackagingMessage : null);
		AssertMessage(RefCusCodeTestHelper.UNPKGCodeNoBulk, 5, null);

		void AssertMessage(string packType, int numberOfPacks, string expectedMessage)
		{
			Package.CW_PackType = packType;
			PackagePivot.CHC_NumberOfPacks = numberOfPacks;
			var assertionMessage = $"CW_PackType={packType}";
			if (expectedMessage != null)
			{
				AssertHasMessageError(assertionMessage, PackagePivot.CHC_NumberOfPacksInfo, expectedMessage);
			}
			if (expectedMessage != packQtyForLoosePackagingMessage)
			{
				AssertNoMessageError(assertionMessage, PackagePivot.CHC_NumberOfPacksInfo, packQtyForLoosePackagingMessage);
			}
		}
	});

	public void TestCheckCHC_NumberOfPacks_NS30181()
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);

		var packQtyZeroForBreakBulk = PassarValidationMessages.MessageNS30181_BreakBulkPackQtyNotZero;

		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertMessage(RefCusCodeTestHelper.UNPKGCodeWithBulkYes, 0, null);
		AssertMessage(RefCusCodeTestHelper.UNPKGCodeWithBreakBulkYes, 0, packQtyZeroForBreakBulk);
		AssertMessage(RefCusCodeTestHelper.UNPKGCodeWithBreakBulkYes, 5, null);
		AssertMessage(RefCusCodeTestHelper.UNPKGCodeNoBulk, 0, null);
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		AssertMessage(RefCusCodeTestHelper.UNPKGCodeWithBreakBulkYes, 0, null);

		void AssertMessage(string packType, int numberOfPacks, string expectedMessage)
		{
			Package.CW_PackType = packType;
			PackagePivot.CHC_NumberOfPacks = numberOfPacks;
			var assertionMessage = $"CW_PackType={packType}";
			if (expectedMessage != null)
			{
				AssertHasMessageError(assertionMessage, PackagePivot.CHC_NumberOfPacksInfo, expectedMessage);
			}
			if (expectedMessage != packQtyZeroForBreakBulk)
			{
				AssertNoMessageError(assertionMessage, PackagePivot.CHC_NumberOfPacksInfo, packQtyZeroForBreakBulk);
			}
		}
	}

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	JobComInvoiceHeader Invoice => invoice ??= Declaration.Invoices.AddNew();
	JobComInvoiceHeader invoice;

	JobComInvoiceLine InvoiceLine => invoiceLine ??= Invoice.InvoiceLines.AddNew();
	JobComInvoiceLine invoiceLine;

	Package Package => package ??= Declaration.Packages.AddNew();
	Package package;

	InvoiceLinePackagePivot PackagePivot => packagePivot ??= CreatePackagePivot(Package, InvoiceLine);
	InvoiceLinePackagePivot packagePivot;

	InvoiceLinePackagePivot CreatePackagePivot(Package package, JobComInvoiceLine invoiceLine)
	{
		var packagePivot = InvoiceLine.PackagesPivot.AddNew();
		packagePivot.CHC_CW = Package.PK;
		return (InvoiceLinePackagePivot)packagePivot;
	}
}
