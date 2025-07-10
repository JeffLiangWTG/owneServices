using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.GUI;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using NctsDepartureMovementHeader = Enterprise.Customs.CH.NCTS.Business.NctsDepartureMovementHeader;
using NctsHeader = Enterprise.Customs.CH.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(ExportDeclarationsModuleButtonGrid))]
sealed class ExportDeclarationsModuleButtonGridTest : ZModuleButtonGridTestBase
{
	public void TestGetNewRecordAttacher()
	{
		using (var form = new ZForm(NctsHeader))
		using (var grid = new ExportDeclarationsModuleButtonGridForTesting())
		{
			form.Controls.Add(grid);
			form.Show();

			grid.AttachButtonForTest.PerformClick();
			AssertType<ExportDeclarationsAttacher>(grid.CreatedAttacher);
		}
	}

	public void TestDetach_Click()
	{
		var cusEntryHeader = CreateCusEntryHeader();
		var pivot = RelatedExportEntryHeaderGenPivotCollection.AddPivotFor(cusEntryHeader);

		using (var form = new ZForm(NctsHeader))
		using (var grid = new ExportDeclarationsModuleButtonGridForTesting())
		{
			form.Controls.Add(grid);
			form.Show();

			AssertEquals("Pre-condition", true, RelatedExportEntryHeaderGenPivotCollection.Contains(cusEntryHeader));
			grid.DetachButtonForTest.PerformClick();
			AssertEquals("Entry should have been removed", false, RelatedExportEntryHeaderGenPivotCollection.Contains(cusEntryHeader));
			AssertEquals("Entry should have been deleted", true, pivot.IsDeleted);
		}
	}

	public void TestGetObjectToEdit()
	{
		var cusEntryHeader = CreateCusEntryHeader();
		RelatedExportEntryHeaderGenPivotCollection.AddPivotFor(cusEntryHeader);
		Factory.Save();

		using (var form = new ZForm(NctsHeader))
		using (var grid = new ExportDeclarationsModuleButtonGridForTesting())
		{
			form.Controls.Add(grid);
			form.Show();

			grid.EditButtonForTest.PerformClick();

			using (var declarationForm = grid.Controller.LastShownForm)
			{
				AssertType<JobDeclarationForm>("Declaration opened", declarationForm);
			}
		}
	}

	BusinessObjectFactory Factory =>  factory ?? (factory = new BusinessObjectFactory());
	BusinessObjectFactory factory;

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader;
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	RelatedExportEntryHeaderGenPivotCollection RelatedExportEntryHeaderGenPivotCollection => NctsHeader.MovementHeader.RelatedExportEntryHeaders;

	CusEntryHeader CreateCusEntryHeader()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		return jobDeclaration.CustomsEntryHeaders.AddNew();
	}

	class ExportDeclarationsModuleButtonGridForTesting : ExportDeclarationsModuleButtonGrid
	{
		internal ExportDeclarationsModuleButtonGridForTesting()
		{
			ColumnStyles.Add(new ZTextBoxColumnStyleInfo() { ColumnName = nameof(RelatedExportEntryHeaderGenPivot.EntryNumber) });
			BindToGridList = $"{nameof(NctsHeader.MovementHeader)}.{nameof(NctsDepartureMovementHeader.RelatedExportEntryHeaders)}";
			BindToFindBoxList = $"{nameof(NctsHeader.MovementHeader)}.{nameof(NctsDepartureMovementHeader.Lookups)}.{nameof(NctsDepartureMovementHeaderLookups.ExportEntryHeaderCollection)}";
			ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.EntryHeader;
			DetachMessage = null;
		}

		internal ZRecordAttacher CreatedAttacher { get; private set; }

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			CreatedAttacher = base.GetNewRecordAttacher(destinationCollection, findBoxList, moduleID);
			return CreatedAttacher;
		}

		internal IList<ToolStripItem> CreatedButtons { get; private set; }

		protected override IList<ToolStripItem> CreateButtons()
		{
			CreatedButtons = base.CreateButtons();
			return CreatedButtons;
		}

		public ZController Controller => EntryHeaderController;
	}
}
