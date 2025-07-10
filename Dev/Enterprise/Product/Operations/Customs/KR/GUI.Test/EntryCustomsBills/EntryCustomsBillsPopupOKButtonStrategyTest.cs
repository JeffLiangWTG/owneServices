using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(EntryCustomsBillsPopupOKButtonStrategy))]
	sealed class EntryCustomsBillsPopupOKButtonStrategyTest : TestCaseWithFactory
	{
		public void TestHandleFindBoxOKButton()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry1 = CreateCusEntryHeader("1234567890M", new ZDateTime(2025, 7, 30));
			var entry2 = CreateCusEntryHeader("1234567891M", new ZDateTime(2025, 8, 30));
			var cusStatementLine1 = CreateCusStatementHeaderAndLine(entry1.EntryNumber, "0127012112100053179");
			var cusStatementLine2 = CreateCusStatementHeaderAndLine(entry2.EntryNumber, "0127012112100053180");
			Factory.Save();

			var customsBillsViewCollection = new KREntryCustomsBillsViewCollection(Factory, new ZQuery(), GlbCompany.CurrentCompany.PK);
			AssertEquals(2, customsBillsViewCollection.Count);

			var reconDeclaration = Factory.New<CusReconDeclaration>();
			using (var form = new RefundDeclarationForm(reconDeclaration))
			{
				form.Show();

				var tabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var tabPage = tabControl.FindSingle<ZTabPage>("ImportEntriesTabPage");
				tabControl.SelectedTab = tabPage;
				var importEntriesGrid = tabPage.FindSingle<ZGrid>("ImportEntriesGrid");
				importEntriesGrid.Focus();
				importEntriesGrid.GetNextControl(importEntriesGrid, false).Focus();

				var columnStyle = importEntriesGrid.Columns["Header+" + nameof(CusReconEntry.CRE_CustomsBillNumber)].ColumnStyle as ZCodeFindBoxColumnStyle;
				var zGridFindBox = (ZGridFindBox)(columnStyle.EditControl);
				var iFindBox = zGridFindBox as IFindBox;
				var popupForm = (EmbeddedModulePopup)iFindBox.PopupForm;
				popupForm.Owner = form;

				using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(zGridFindBox.ModuleID))
				{
					var grid = (ZDisplayGrid)module.DisplayGrid;
					using (var popup = new EmbeddedModulePopup(module))
					{
						var strategy = new EntryCustomsBillsPopupOKButtonStrategy(iFindBox);
						popup.EmbeddedModulePopupOKButtonStrategy = strategy;
						popup.Show();
						AssertEquals(ModuleIDs.Customs.KR.EntryCustomsBillsFor5UL.Description, popup.FormCaption);

						var moduleColl = (KREntryCustomsBillsViewCollection)grid.List;
						moduleColl.AdditionalFilter = module.FilterBusinessObject.Filter;
						AssertEquals(2, moduleColl.Count);

						popup.ExposedOKButtonForTesting.PerformClick();
						strategy.HandleFindBoxOKButton(moduleColl.ToArray());
						AssertEquals(2, reconDeclaration.CusReconEntryLines.Count);
					}
				}
			}

			CusEntryHeader CreateCusEntryHeader(ZString entryNumber, ZDateTime issueDate)
			{
				var entry = declaration.ActiveEntryHeaders.AddNew();
				entry.EntryNumber = entryNumber;
				entry.CusEntryNumber.CE_IssueDate = issueDate;

				return entry;
			}

			CusStatementLine CreateCusStatementHeaderAndLine(ZString entryNumber, ZString billNumber)
			{
				var cusStatementHeader = Factory.New<CusStatementHeader>();
				cusStatementHeader.B2_GC = declaration.JE_GC;
				cusStatementHeader.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
				cusStatementHeader.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
				var cusStatementLine = cusStatementHeader.StatementLines.AddNew();
				cusStatementLine.B3_EntryNum = entryNumber;
				cusStatementLine.B3_AssociatedEntry = billNumber;

				return cusStatementLine;
			}
		}
	}
}
