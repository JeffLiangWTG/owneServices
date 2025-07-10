using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(SimplifiedDeclarationModuleForm))]
	class SimplifiedDeclarationModuleFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			AssertEquals("Simplified Declaration", moduleForm.Text);
		}

		public void TestFilterBusinessObjectSet()
		{
			AssertType<SimplifiedDeclarationFilterStripBusinessObject>(FilterControl.FilterBusinessObject);
		}

		public void TestCancelButton()
		{
			CombineAssertions(() =>
			{
				var cusReconEntry = Factory.NewWithValidTestData<CusReconEntry>();
				cusReconEntry.CRE_EntryDate = reconDeclaration.CRD_PeriodFrom;
				filterModule.PerformSearch_ForTest();
				AssertGreaterThan("Entries to Select", FilterControl.GridCollection.Count, 0);
				FilterControl.Grid.Select(0);
				moduleForm.FindSingle<ZButton>("Cancel_Button").PerformClick();
				AssertEquals("Not linked", 0, reconDeclaration.CusReconEntries.Count);
			});
		}

		public void TestConfirmButtonLinksCRE_CRD()
		{
			var today = ZDate.Today;
			var cusReconEntry1 = Factory.NewWithValidTestData<CusReconEntry>();
			cusReconEntry1.CRE_EntryDate = today;
			var cusReconEntry2 = Factory.NewWithValidTestData<CusReconEntry>();
			cusReconEntry2.CRE_EntryDate = today;
			filterModule.PerformSearch_ForTest();
			CombineAssertions(() =>
			{
				AssertEquals("Initial state: no selection", false, Selection.Any());
				FilterControl.Grid.Select(0);
				AssertContainsExactElementsInAnyOrder("First entry selected", new[] { cusReconEntry1 }, Selection);
				FilterControl.Grid.Select(1);
				AssertContainsExactElementsInAnyOrder("Second entry selected", new[] { cusReconEntry1, cusReconEntry2 }, Selection);
				FilterControl.Grid.UnSelect(0);
				AssertContainsExactElementsInAnyOrder("First entry unselected", new[] { cusReconEntry2 }, Selection);
				ConfirmButton.PerformClick();
				AssertEquals("One entry added", 1, reconDeclaration.CusReconEntries.Count);
				AssertEquals(cusReconEntry2.CRE_CRD, reconDeclaration.PK);
			});
		}

		public void TestConfirmButtonLinksCRL_LineNumber()
		{
			var entry1 = reconDeclaration.CusReconEntries.AddNew();
			var line1 = entry1.CusReconEntryLines.AddNew();
			line1.CRL_LineNumber = 1;
			var line2 = entry1.CusReconEntryLines.AddNew();
			line2.CRL_LineNumber = 3;

			var entry2 = Factory.New<CusReconEntry>();
			var line3 = entry2.CusReconEntryLines.AddNew();
			var line4 = entry2.CusReconEntryLines.AddNew();
			var entry3 = Factory.New<CusReconEntry>();
			var line5 = entry3.CusReconEntryLines.AddNew();
			var entry4 = Factory.New<CusReconEntry>();
			var line6 = entry4.CusReconEntryLines.AddNew();

			filterModule.PerformSearch_ForTest();
			FilterControl.Grid.Select(0);
			FilterControl.Grid.Select(1);
			ConfirmButton.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("line1 pre-existing, CRL_LineNumber not updated", (ZShort)1, line1.CRL_LineNumber);
				AssertEquals("line2 pre-existing, CRL_LineNumber not updated", (ZShort)3, line2.CRL_LineNumber);
				AssertEquals("line3 added, CRL_LineNumber updated", (ZShort)4, line3.CRL_LineNumber);
				AssertEquals("line4 added, CRL_LineNumber updated", (ZShort)5, line4.CRL_LineNumber);
				AssertEquals("line5 added, CRL_LineNumber updated", (ZShort)6, line5.CRL_LineNumber);
				AssertEquals("line6 not added, CRL_LineNumber not updated", ZShort.Zero, line6.CRL_LineNumber);
			});
		}

		public void TestConfirmButtonLinksCRL_CustomsStatus()
		{
			var entry1 = Factory.New<CusReconEntry>();
			var line1 = entry1.CusReconEntryLines.AddNew();
			line1.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX5;
			var line2 = entry1.CusReconEntryLines.AddNew();
			line2.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX5;
			var entry2 = Factory.New<CusReconEntry>();
			var line3 = entry2.CusReconEntryLines.AddNew();
			line3.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX5;
			var entry3 = Factory.New<CusReconEntry>();
			var line4 = entry3.CusReconEntryLines.AddNew();
			line4.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX5;

			filterModule.PerformSearch_ForTest();
			FilterControl.Grid.Select(0);
			FilterControl.Grid.Select(1);
			ConfirmButton.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("line1 added, CRL_CustomsStatus updated", ZString.Empty, line1.CRL_CustomsStatus);
				AssertEquals("line2 added, CRL_CustomsStatus updated", ZString.Empty, line2.CRL_CustomsStatus);
				AssertEquals("line3 added, CRL_CustomsStatus updated", ZString.Empty, line3.CRL_CustomsStatus);
				AssertEquals("line4 not added, CRL_CustomsStatus not updated", UniversalReferenceConstants.EntryStatus.TX5, line4.CRL_CustomsStatus);
			});
		}

		public void TestNoSelectedEntries()
		{
			filterModule.PerformSearch_ForTest();
			ConfirmButton.PerformClick();
			AssertEquals("Please select entries.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestMaxNumberLines()
		{
			CombineAssertions(() =>
			{
				var selectedCusReconEntry = reconDeclaration.CusReconEntries.AddNew();
				selectedCusReconEntry.CRE_EntryDate = ZDate.Today;
				Enumerable.Range(0, 1000).ForEach(_ => selectedCusReconEntry.CusReconEntryLines.AddNew());

				var cusReconEntry = Factory.NewWithValidTestData<CusReconEntry>();
				cusReconEntry.CRE_EntryDate = ZDate.Today;
				cusReconEntry.CusReconEntryLines.AddNew();

				using (DECustomsDataRegistry.Instance.MonthlyClosingMaximumNumberOfLines.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000))
				{
					filterModule.PerformSearch_ForTest();
					FilterControl.Grid.Select(0);
					ConfirmButton.PerformClick();
					AssertEquals("Maximum Lines Exceeded", "Number of entry lines exceeds the configured number in registry.", UnitTestUserNotification.Instance.LastMessage.Text);
					cusReconEntry.CusReconEntryLines.DeleteAll();
					ConfirmButton.PerformClick();
					AssertEquals("ReconEntry linked", cusReconEntry.CRE_CRD, reconDeclaration.PK);
				}
			});
		}

		protected override Form GetFormToBashCore() => new SimplifiedDeclarationModuleForm();

		protected override void SetUp()
		{
			base.SetUp();
			var today = ZDate.Today;
			reconDeclaration = Factory.NewWithValidTestData<CusReconDeclaration>();
			reconDeclaration.CRD_PeriodFrom = today.AddDays(-15);
			reconDeclaration.CRD_PeriodTo = today.AddDays(10);
			filterModule = ZModuleFactory.Instance.Create(ModuleIDs.Customs.EU.DE.SimplifiedDeclaration) as ZFilterModule;
			parentForm = new ZForm(reconDeclaration);
			filterModule.SetFormsModalTo(parentForm);
			moduleForm = filterModule.ShowPopup() as SimplifiedDeclarationModuleForm;
			moduleForm.Show(parentForm);
		}

		protected override void TearDown()
		{
			base.TearDown();
			moduleForm.Dispose();
			parentForm.Dispose();
			filterModule.Dispose();
		}
		CusReconDeclaration reconDeclaration;
		ZForm parentForm;
		SimplifiedDeclarationModuleForm moduleForm;
		ZFilterModule filterModule;

		ZButton ConfirmButton => moduleForm.FindSingle<ZButton>("OK_Button");

		ZFilterStripControl FilterControl => moduleForm.FindSingle<ZFilterStripControl>();

		IEnumerable<CusReconEntry> Selection => filterModule.DisplayGrid.SelectedElements.Cast<CusReconEntry>();
	}
}
