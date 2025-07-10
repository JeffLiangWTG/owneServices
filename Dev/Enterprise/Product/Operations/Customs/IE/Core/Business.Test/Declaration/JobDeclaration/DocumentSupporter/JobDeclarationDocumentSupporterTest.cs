using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclarationDocumentSupporter))]
	class JobDeclarationDocumentSupporterTest : EU.Business.Declaration.Testing.JobDeclarationDocumentSupporterTest
	{
		public void TestGetDocumentWrappersInternal()
		{
			var menuItemForTesting = Factory.New<IStmMenuItem>();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var documentSupporter = new JobDeclarationDocumentSupporter(declaration);
			CombineAssertions("", () =>
			{
				var wrappers = documentSupporter.GetDocumentWrappers(DataContext.IEEAD, menuItemForTesting);
				AssertContainsExactElementsInAnyOrder("Wrapped Objects", [entryHeader1, entryHeader2], wrappers.Select(x => x.WrappedObject));
				AssertEquals("IEEAD", "Enterprise.Customs.IE.DocumentWrappers.IEDocEAD", wrappers[0].GetType().FullName);
				wrappers = documentSupporter.GetDocumentWrappers(DataContext.SADH, menuItemForTesting);
				AssertEquals("SADH", "Enterprise.Customs.IE.DocumentWrappers.IEDocEAD", wrappers[0].GetType().FullName);
				wrappers = documentSupporter.GetDocumentWrappers(DataContext.IEImportAccompanyingDocument, menuItemForTesting);
				AssertEquals("IEImportAccompanyingDocument", "Enterprise.Customs.IE.DocumentWrappers.ImportAccompanyingDocumentWrapper", wrappers[0].GetType().FullName);
				wrappers = documentSupporter.GetDocumentWrappers(DataContext.IEIADClearanceSlip, menuItemForTesting);
				AssertEquals("IEIADClearanceSlip", "Enterprise.Customs.IE.DocumentWrappers.IADClearanceSlip", wrappers[0].GetType().FullName);
			});
		}

		public void TestGetSupportedDataContexts()
		{
			var declaration = Factory.New<JobDeclaration>();
			var documentSupporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var list = documentSupporter.ListOfSupportedDataContexts;

			CombineAssertions(() =>
			{
				Assert("IEEAD", list.ContainsCode(DataContext.IEEAD));
				Assert("IEEAD - Full", list.ContainsCode(DataContext.SADH));
				Assert("IEImportAccompanyingDocument", list.ContainsCode(DataContext.IEImportAccompanyingDocument));
				Assert("IEIADClearanceSlip", list.ContainsCode(DataContext.IEIADClearanceSlip));
			});
		}

		public void TestGetFilterValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var supporter = new JobDeclarationDocumentSupporter(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("IE for DocumentFilters.CTYEGSADH", Core.Constants.CountryCodes.Ireland, supporter.GetFilterValue(DocumentFilters.CTYEGSADH));
				AssertEquals("EUN for DocumentFilters.CTYEG", EconomicGroupList.Codes.EuropeanUnion, supporter.GetFilterValue(DocumentFilters.CTYEG));
				AssertEquals("No exception for DocumentFilters.CTY", Core.Constants.CountryCodes.Ireland, supporter.GetFilterValue(DocumentFilters.CTY));
			});
		}

		public void TestGetCustomWatermarkText()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryStatus = AISEntryStatusList.Codes.Released;
			entry.CH_Status = LogicalStatusList.Codes.Accepted;
			var supporter = new JobDeclarationDocumentSupporter(declaration);
			var context = new DataContextValue(".CusEntryHeader");
			var docCommand = Factory.New<DocumentCommand>();
			docCommand.SU_MenuName = "Import Accompanying Document";
			var dataProvider = supporter.GetBODocDataProviders(context, docCommand)[0];
			AssertEquals("Watermark should not be visible. No LCK or UCK events", "calculated duties\r\nestimated", supporter.GetCustomWatermarkText(docCommand, dataProvider));

			var lckLog = declaration.Logs.AddNew();
			using (lckLog.LockForUpdatingKeyFieldsForTesting())
			{
				lckLog.SL_SE_NKEvent = AutoEvents.LockForEdit.Code;
			}

			supporter = new JobDeclarationDocumentSupporter(declaration);
			entry.MergedLines.AddNew().ConfirmedFees.AddNew().CF_ChargeType = "A00";
			AssertEquals("Watermark should not be visible. Only 1 LCK event", null, supporter.GetCustomWatermarkText(docCommand, dataProvider));

			WaitForTimeToPass(TimeSpan.FromMilliseconds(1500));

			var uckLog = declaration.Logs.AddNew();
			using (uckLog.LockForUpdatingKeyFieldsForTesting())
			{
				uckLog.SL_SE_NKEvent = AutoEvents.UnlockForEdit.Code;
			}
			supporter = new JobDeclarationDocumentSupporter(declaration);
			AssertEquals("Watermark should be visible. UCK event + ACC + REL", "UNCONFIRMED\r\nBY CUSTOMS\r\nUnder Amendment", supporter.GetCustomWatermarkText(docCommand, dataProvider));

			WaitForTimeToPass(TimeSpan.FromMilliseconds(1500));

			var lckLog1 = declaration.Logs.AddNew();
			using (lckLog1.LockForUpdatingKeyFieldsForTesting())
			{
				lckLog1.SL_SE_NKEvent = AutoEvents.LockForEdit.Code;
			}
			supporter = new JobDeclarationDocumentSupporter(declaration);
			AssertEquals("Watermark should not be visible. UCK followed by LCK", null, supporter.GetCustomWatermarkText(docCommand, dataProvider));

			void WaitForTimeToPass(TimeSpan timeToWait)
			{
				var stopwatch = Stopwatch.StartNew();

				while (stopwatch.ElapsedMilliseconds < timeToWait.TotalMilliseconds)
				{
					Thread.SpinWait(500);
				}
			}
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var landedCostHeader = (BusinessObject)Factory.New<LandedCosting.ILandedCostHeader>();
			landedCostHeader[LandedCostHeaderSchema.LT_ParentTableCode.Name] = declaration.TablePrefix;
			landedCostHeader[LandedCostHeaderSchema.LT_ParentID.Name] = declaration.PK;

			return declaration;
		}

		protected override Dictionary<string, int> MaxDBHitCounts
		{
			get
			{
				var maxHits = base.MaxDBHitCounts;
				maxHits[nameof(CusEntryInstruction)] = 2;
				return maxHits;
			}
		}
	}
}
