using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class ExtendedHoursRequestOKButtonStrategyTest : TestCaseWithFactory
	{
		[TestDate(2022, 01, 01)]
		public void TestHandleFindBoxOKButtonForExport()
		{
			SetupTestData(KRJobMessageTypeList.Codes.Export, out var orgHeaders, out var declarations);
			AssertHandleFindBoxOKButton(ElectronicDocumentTypeList.Codes._5AC, ModuleIDs.Customs.KR.ExportEntryDetails, orgHeaders, declarations);
		}

		[TestDate(2022, 01, 01)]
		public void TestHandleFindBoxOKButtonForImport()
		{
			SetupTestData(KRJobMessageTypeList.Codes.Import, out var orgHeaders, out var declarations);
			AssertHandleFindBoxOKButton(ElectronicDocumentTypeList.Codes._5GW, ModuleIDs.Customs.KR.ImportEntryDetails, orgHeaders, declarations);
		}

		void SetupTestData(string messageType, out List<OrgHeader> orgHeaders, out List<JobDeclaration> declarations)
		{
			orgHeaders = new List<OrgHeader>();
			declarations = new List<JobDeclaration>();

			for (int i = 1; i <= 3; i++)
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Category = "BUS";
				orgHeader.OH_Code = $"RK{i}";
				orgHeader.OH_FullName = $"RK TestData{i}";
				orgHeaders.Add(orgHeader);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = messageType;
				declaration.JE_ApplicationCode = "BLT";
				declaration.JE_OH_Supplier = orgHeader.PK;
				declaration.JE_OA_SupplierAddress = orgHeader.MainAddress.PK;

				var checkDigit = messageType == KRJobMessageTypeList.Codes.Export ? "X" : "M";
				var entry = declaration.CustomsEntryHeaders.AddNew();
				var entryNum = entry.EntryNumbers.AddNew();
				entryNum.CE_EntryType = messageType;
				entryNum.CE_EntryNum = $"123452212345{i}{checkDigit}";
				entryNum.CE_IssueDate = new ZDateTime("2022-01-01");

				var entryLine = entry.MergedLines.AddNew();
				entryLine.CL_CustomsValue = i * 1000;

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_NoOfPacks = i * 100;
				invoice.JZ_Weight = i * 100;
				invoice.JZ_WeightUQ = "KG";

				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;

				declarations.Add(declaration);
			}

			Factory.Save();
		}

		void AssertHandleFindBoxOKButton(string messageType, ModuleIdentifier moduleId, List<OrgHeader> orgHeaders, List<JobDeclaration> declarations)
		{
			var extendedHoursRequestHeader = new ExtendedHoursRequestHeader(Factory, messageType, GlbBranch.CurrentBranch.GB_GC);
			var extendedHoursRequestLine = extendedHoursRequestHeader.ExtendedHoursRequestLines;

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(moduleId))
			{
				var grid = (ZDisplayGrid)module.DisplayGrid;
				using (var popup = new EmbeddedModulePopup(module))
				{
					var strategy = new KREntryDetailsPopupOKButtonStrategy(popup, extendedHoursRequestLine);
					popup.EmbeddedModulePopupOKButtonStrategy = strategy;
					popup.Show();
					AssertEquals(moduleId.Description, popup.FormCaption);

					var moduleColl = (KREntryHeaderDetailsViewCollection)grid.List;
					moduleColl.AdditionalFilter = module.FilterBusinessObject.Filter;
					popup.ExposedOKButtonForTesting.PerformClick();
					strategy.HandleFindBoxOKButton(moduleColl.ToArray());
					AssertEquals(3, extendedHoursRequestLine.Count);
					AssertRequestLines(0);
					AssertRequestLines(1);
					AssertRequestLines(2);
				}
			}

			void AssertRequestLines(int i)
			{
				var messageType = declarations[i].JE_MessageType;
				var entry = declarations[i].ActiveEntryHeaders[0];
				var entryNum = entry.CusEntryNumber;
				var entryLine = (CusEntryLine)entry.RandomEntryLine;
				var invoice = declarations[i].Invoices[0];
				var checkDigit = messageType == KRJobMessageTypeList.Codes.Export ? "X" : "M";
				var row = extendedHoursRequestLine.Cast<ExtendedHoursRequestLine>().FirstOrDefault(x => x.ReferenceNumber == $"123452212345{i + 1}{checkDigit}");

				AssertEquals(entryNum.CE_EntryNum, row.ReferenceNumber);
				AssertEquals(invoice.JZ_Weight, row.TotalWeight);
				AssertEquals((int)invoice.JZ_NoOfPacks, row.PackageCount);
				AssertEquals(orgHeaders[i].OH_FullName, row.SupplierName);
			}
		}
	}
}
