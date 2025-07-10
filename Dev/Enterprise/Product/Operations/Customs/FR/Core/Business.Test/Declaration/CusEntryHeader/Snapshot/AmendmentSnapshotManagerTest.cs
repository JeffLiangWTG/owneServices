using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing;

sealed class AmendmentSnapshotManagerTest : Customs.Business.Testing.AmendmentSnapshotManagerAbstractTest<AmendmentSnapshotManager>
{
	[TestDate(2024, 2, 21, 07, 01, 12)]
	public override void TestRevertToLastLodged_EntryHeaderSnapshotConflictPolicyIsOverride()
	{
		var entry = (CusEntryHeader)GetEntryHeaderForTesting();
		var declaration = entry.Declaration;
		var manager = entry.GetNewAmendmentSnapshotManager();
		manager.CreateNewAndAccept();
		Factory.Save();

		RectifyEntryHeaderRelatedDeclaration(declaration);
		manager.RevertToLastLodged(SnapshotRevertingStrategy.Override);

		CombineAssertions(() =>
		{
			declaration.PackingGroups.Reload(true, true);
			declaration.PackingGroups[0].Packages.Reload(true, true);
			declaration.Invoices[0].Reload();
			declaration.Invoices[1].Reload();
			declaration.Invoices[0].InvoiceLines.Reload(true, true);
			declaration.Invoices[1].InvoiceLines.Reload(true, true);
			declaration.Invoices[0].InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.PackagesPivot.Reload(true, true));
			declaration.Invoices[1].InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.PackagesPivot.Reload(true, true));

			AssertPackage(declaration.PackingGroups[0].Packages[0], 10, "1A", "Coke");
			AssertPackage(declaration.PackingGroups[0].Packages[1], 15, "1B", "Sprite");
			AssertInvoice(declaration.Invoices[0], 0m);
			AssertInvoice(declaration.Invoices[1], 0m);
			AssertInvoiceLine(declaration.Invoices[0].InvoiceLines[0], "11111111", "PRO1", "111", 123m, 134m, 2, "Coke");
			AssertInvoiceLine(declaration.Invoices[0].InvoiceLines[1], "22222222", "PRO2", "222", 234m, 253m, 10, "Sprite");
			AssertInvoiceLine(declaration.Invoices[1].InvoiceLines[0], "33333333", "XXX", "333", 313m, 384m, 8, "Coke");
			AssertInvoiceLine(declaration.Invoices[1].InvoiceLines[1], "44444444", "PRO4", "444", 482m, 428m, 5, "Sprite");
		});
	}

	[TestDate(2024, 2, 21, 07, 01, 12)]
	public override void TestRevertToLastLodged_EntryHeaderSnapshotConflictPolicyIsSkip()
	{
		var entry = (CusEntryHeader)GetEntryHeaderForTesting();
		var declaration = entry.Declaration;
		var manager = entry.GetNewAmendmentSnapshotManager();
		manager.CreateNewAndAccept();
		Factory.Save();

		RectifyEntryHeaderRelatedDeclaration(declaration);
		manager.RevertToLastLodged(SnapshotRevertingStrategy.Skip);

		CombineAssertions(() =>
		{
			declaration.PackingGroups[0].Packages.Reload(true, true);
			declaration.Invoices[0].Reload();
			declaration.Invoices[1].Reload();
			declaration.Invoices[0].InvoiceLines.Reload(true, true);
			declaration.Invoices[1].InvoiceLines.Reload(true, true);
			declaration.Invoices[0].InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.PackagesPivot.Reload(true, true));
			declaration.Invoices[1].InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.PackagesPivot.Reload(true, true));

			AssertPackage(declaration.PackingGroups[0].Packages[0], 15, "2A", "Fanta");
			AssertPackage(declaration.PackingGroups[0].Packages[1], 15, "1B", "Sprite");
			AssertInvoice(declaration.Invoices[0], 0m);
			AssertInvoice(declaration.Invoices[1], 20m);
			AssertInvoiceLine(declaration.Invoices[0].InvoiceLines[0], "11111111", "PRO1", "111", 123m, 134m, 6, "Fanta");
			AssertInvoiceLine(declaration.Invoices[0].InvoiceLines[1], "22222222", "PRO2", "222", 234m, 253m, 10, "Sprite");
			AssertInvoiceLine(declaration.Invoices[1].InvoiceLines[0], "33333333", "XXX", "333", 313m, 384m, 9, "Fanta");
			AssertInvoiceLine(declaration.Invoices[1].InvoiceLines[1], "44444444", "PRO4", "444", 482m, 428m, 5, "Sprite");
		});
	}

	void RectifyEntryHeaderRelatedDeclaration(JobDeclaration declaration)
	{
		var package1 = declaration.PrimaryHouseBill.PackingGroups[0].Packages[0];
		package1.CW_PackQty = 15;
		package1.CW_PackType = "2A";
		package1.CW_MarksAndNos = "Fanta";
		var package2 = declaration.PrimaryHouseBill.PackingGroups[0].Packages[1];
		package2.CW_PackQty = 20;
		package2.CW_PackType = "2B";
		package2.CW_MarksAndNos = "Beer";

		declaration.Invoices[0].JZ_InvoiceAmount = 10m;
		declaration.Invoices[1].JZ_InvoiceAmount = 20m;
		declaration.Invoices[0].InvoiceLines[0].JI_Tariff = "99999999";
		declaration.Invoices[0].InvoiceLines[0].PackagesPivot.Cast<InvoiceLinePackagePivot>().Single().CHC_NumberOfPacks = 6;
		declaration.Invoices[0].InvoiceLines[1].JI_CustomsSecondQuantity = 999m;
		declaration.Invoices[0].InvoiceLines[1].PackagesPivot.Cast<InvoiceLinePackagePivot>().Single().CHC_NumberOfPacks = 12;
		declaration.Invoices[1].InvoiceLines[0].JI_Procedure = "XXX";
		declaration.Invoices[1].InvoiceLines[0].PackagesPivot.Cast<InvoiceLinePackagePivot>().Single().CHC_NumberOfPacks = 9;
		declaration.Invoices[1].InvoiceLines[1].JI_PrimaryPreference = "PPP";
		declaration.Invoices[1].InvoiceLines[1].PackagesPivot.Cast<InvoiceLinePackagePivot>().Single().CHC_NumberOfPacks = 8;

		Factory.Save();
	}

	void AssertPackage(BasePackage package, int packQty, string packType, string marksAndNos)
	{
		AssertEquals(packQty, package.CW_PackQty);
		AssertEquals(packType, package.CW_PackType);
		AssertEquals(marksAndNos, package.CW_MarksAndNos);
	}

	void AssertInvoice(JobComInvoiceHeader invoice, decimal amount)
	{
		AssertEquals(amount, invoice.JZ_InvoiceAmount);
	}

	void AssertInvoiceLine(JobComInvoiceLine invoiceLine, string tariff, string procedure, string preference, decimal secondQuantity, decimal thirdQuantity, int numberOfPacks, string linkedPackMark)
	{
		AssertEquals(tariff, invoiceLine.JI_Tariff);
		AssertEquals(procedure, invoiceLine.JI_Procedure);
		AssertEquals(preference, invoiceLine.JI_PrimaryPreference);
		AssertEquals(secondQuantity, invoiceLine.JI_CustomsSecondQuantity);
		AssertEquals(thirdQuantity, invoiceLine.JI_CustomsThirdQuantity);
		var packagePivot = invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().Single();
		AssertEquals(numberOfPacks, packagePivot.CHC_NumberOfPacks);
		AssertEquals(linkedPackMark, packagePivot.Package.CW_MarksAndNos);
	}

	protected override ZInt ExpectedVersionNumber => 3;

	protected override ZString ExpectedMessageType => DeclarationApplicationCodeList.Codes.DeltaG;

	protected override ZString ExpectedSnapshotXml => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.Declaration.CusEntryHeader.Snapshot.ExpectedSnapshot.xml");

	protected override Customs.Business.CusEntryHeader GetEntryHeaderForTesting()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryInstructions[0].CEI_SubStyle = "F";
		declaration.JE_MessageType = "IMP";
		declaration.JE_HouseBill = "BILL1234";
		var packingGroup = declaration.PrimaryHouseBill.PackingGroups[0];
		var package1 = packingGroup.Packages.AddNew();
		package1.CW_PackType = "1A";
		package1.CW_PackQty = 10;
		package1.CW_MarksAndNos = "Coke";

		var package2 = packingGroup.Packages.AddNew();
		package2.CW_PackType = "1B";
		package2.CW_PackQty = 15;
		package2.CW_MarksAndNos = "Sprite";

		var cei1 = declaration.CustomsEntryInstructions.AddNew();
		cei1.CEI_Style = "A";
		var cei2 = declaration.CustomsEntryInstructions.AddNew();
		cei2.CEI_Style = "F";

		var invoiceHeader1 = declaration.Invoices.AddNew();
		invoiceHeader1.JZ_InvoiceNumber = "INV1";
		invoiceHeader1.JZ_InvoiceAmount = 0;
		var invoiceHeader2 = declaration.Invoices.AddNew();
		invoiceHeader2.JZ_InvoiceNumber = "INV2";
		invoiceHeader1.JZ_InvoiceAmount = 0;

		var invoiceLine1 = AddInvoiceLine(invoiceHeader1, cei1, "11111111", "InvoiceLine1 Desc.", "111", 123m, 134m, "PRO1", "MK1");
		AddPackagePivot(invoiceLine1, package1, 2);

		var invoiceLine2 = AddInvoiceLine(invoiceHeader1, cei1, "22222222", "InvoiceLine2 Desc.", "222", 234m, 253m, "PRO2", "MK2");
		AddPackagePivot(invoiceLine2, package2, 10);

		var invoiceLine3 = AddInvoiceLine(invoiceHeader2, cei2, "33333333", "InvoiceLine3 Desc.", "333", 313m, 384m, "PRO3", "MK3");
		AddPackagePivot(invoiceLine3, package1, 8);

		var invoiceLine4 = AddInvoiceLine(invoiceHeader2, cei1, "44444444", "InvoiceLine4 Desc.", "444", 482m, 428m, "PRO4", "MK4");
		AddPackagePivot(invoiceLine4, package2, 5);

		var merger = new LineMerger(declaration);
		merger.DoMerge();
		Factory.Save();

		var entryToRevert = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First(entry => entry.InvoiceLines.Any(line => line.EntryInstruction.CEI_Style == "A"));
		entryToRevert.CH_SequenceNumber = 3;
		return entryToRevert;

		JobComInvoiceLine AddInvoiceLine(JobComInvoiceHeader invoice, CusEntryInstruction cei, string tariff, string desc, string primaryPreference, decimal secondQuantity, decimal thirdQuantity, string procedure, string matchingKey)
		{
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cei.PK;
			invoiceLine.JI_Tariff = tariff;
			invoiceLine.JI_Description = desc;
			invoiceLine.JI_PrimaryPreference = primaryPreference;
			invoiceLine.JI_CustomsSecondQuantity = secondQuantity;
			invoiceLine.JI_CustomsThirdQuantity = thirdQuantity;
			invoiceLine.JI_Procedure = procedure;
			invoiceLine.JI_MatchingKey = matchingKey;
			return invoiceLine;
		}

		void AddPackagePivot(JobComInvoiceLine invoiceLine, BasePackage package, int number)
		{
			var pivot = invoiceLine.PackagesPivot.AddNew();
			pivot.CHC_CW = package.PK;
			pivot.CHC_NumberOfPacks = number;
		}
	}

	readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
}
