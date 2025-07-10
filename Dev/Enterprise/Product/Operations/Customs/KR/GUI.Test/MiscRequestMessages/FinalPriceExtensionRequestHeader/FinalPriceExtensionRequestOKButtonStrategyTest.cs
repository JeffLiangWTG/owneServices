using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed public class FinalPriceExtensionRequestOKButtonStrategyTest : TestCaseWithFactory
	{
		[TestDate(2022, 01, 01)]
		public void TestHandleFindBoxOKButton()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Category = "BUS";
			orgHeader1.OH_Code = "RK1";
			orgHeader1.OH_FullName = "RK TestData1";

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = "IMP";
			declaration1.JE_ApplicationCode = "BLT";
			declaration1.JE_OH_Supplier = orgHeader1.PK;
			declaration1.JE_OA_SupplierAddress = orgHeader1.MainAddress.PK;

			var invoice1 = declaration1.Invoices.AddNew();
			invoice1.JZ_NoOfPacks = 100;
			invoice1.JZ_Weight = 100;
			invoice1.JZ_WeightUQ = "KG";

			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_EntryReleaseDate = new ZDateTime(2022, 01, 01);
			var entryNum1 = entry1.EntryNumbers.AddNew();
			entryNum1.CE_EntryType = "IMP";
			entryNum1.CE_EntryNum = "1234522123450";
			entryNum1.CE_IssueDate = new ZDateTime(2022, 01, 01);

			var entryNum934 = entry1.EntryNumbers.AddNew();
			entryNum934.CE_EntryType = "934";
			entryNum934.CE_ExpiryDate = new ZDateTime(2022, 01, 01);

			var entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 100;

			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_Weight = 100;

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Category = "BUS";
			orgHeader2.OH_Code = "RK2";
			orgHeader2.OH_FullName = "RK TestData2";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = "IMP";
			declaration2.JE_ApplicationCode = "BLT";
			declaration2.JE_OH_Importer = orgHeader2.PK;

			var invoice2 = declaration2.Invoices.AddNew();
			invoice2.JZ_NoOfPacks = 200;
			invoice2.JZ_Weight = 200;
			invoice2.JZ_WeightUQ = "KG";

			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_EntryReleaseDate = new ZDateTime(2022, 01, 01);
			var entryNum2 = entry2.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = "IMP";
			entryNum2.CE_EntryNum = "1234522123451";
			entryNum2.CE_IssueDate = new ZDateTime(2022, 01, 01);

			entryNum934 = entry2.EntryNumbers.AddNew();
			entryNum934.CE_EntryType = "934";
			entryNum934.CE_ExpiryDate = new ZDateTime(2022, 01, 01);

			var entryLine2 = entry2.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 200;

			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Weight = 200;

			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.OH_Category = "BUS";
			orgHeader3.OH_Code = "RK3";
			orgHeader3.OH_FullName = "RK TestData3";

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = "IMP";
			declaration3.JE_ApplicationCode = "BLT";
			declaration3.JE_OH_DutyPayer = orgHeader3.PK;

			var invoice3 = declaration3.Invoices.AddNew();
			invoice3.JZ_NoOfPacks = 300;
			invoice3.JZ_Weight = 300;
			invoice3.JZ_WeightUQ = "KG";

			var entry3 = declaration3.CustomsEntryHeaders.AddNew();
			entry3.CH_EntryReleaseDate = new ZDateTime(2022, 01, 01);
			var entryNum3 = entry3.EntryNumbers.AddNew();
			entryNum3.CE_EntryType = "IMP";
			entryNum3.CE_EntryNum = "1234522123452";
			entryNum3.CE_IssueDate = new ZDateTime(2022, 01, 01);

			entryNum934 = entry3.EntryNumbers.AddNew();
			entryNum934.CE_EntryType = "934";
			entryNum934.CE_ExpiryDate = new ZDateTime(2022, 01, 01);

			var entryLine3 = entry3.MergedLines.AddNew();
			entryLine3.CL_CustomsValue = 300;

			var invoiceLine3 = invoice3.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_Weight = 300;
			Factory.Save();

			var finalPriceReportByDateExtensionHeader = new FinalPriceReportByDateExtensionHeader(Factory);
			var finalPriceReportByDateExtensionLines = finalPriceReportByDateExtensionHeader.FinalPriceReportByDateExtensionLines;
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.KR.EntryDetailsFor5SG))
			{
				var grid = (ZDisplayGrid)module.DisplayGrid;
				using (var popup = new EmbeddedModulePopup(module))
				{
					var strategy = new KREntryDetailsPopupOKButtonStrategy(popup, finalPriceReportByDateExtensionLines);
					popup.EmbeddedModulePopupOKButtonStrategy = strategy;
					popup.Show();
					var moduleColl = (KREntryHeaderDetailsViewCollection)grid.List;
					moduleColl.AdditionalFilter = module.FilterBusinessObject.Filter;
					popup.ExposedOKButtonForTesting.PerformClick();
					strategy.HandleFindBoxOKButton(moduleColl.ToArray());
					AssertEquals(3, finalPriceReportByDateExtensionLines.Count);

					var firstRow = finalPriceReportByDateExtensionLines.Cast<FinalPriceReportByDateExtensionLine>().FirstOrDefault(x => x.ImportDeclarationNumber == "1234522123450");
					AssertEquals(entryNum1.CE_EntryNum, firstRow.ImportDeclarationNumber);

					var secondRow = finalPriceReportByDateExtensionLines.Cast<FinalPriceReportByDateExtensionLine>().FirstOrDefault(x => x.ImportDeclarationNumber == "1234522123451");
					AssertEquals(entryNum2.CE_EntryNum, secondRow.ImportDeclarationNumber);

					var thirdRow = finalPriceReportByDateExtensionLines.Cast<FinalPriceReportByDateExtensionLine>().FirstOrDefault(x => x.ImportDeclarationNumber == "1234522123452");
					AssertEquals(entryNum3.CE_EntryNum, thirdRow.ImportDeclarationNumber);
				}
			}
		}
	}
}
