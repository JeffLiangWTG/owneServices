using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(EntriesTabUserControl))]
	sealed class EntriesTabUserControlTest : TestCaseWithFactory
	{
		public void TestEntryLineGrid()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			using (var control = new EntriesTabUserControl())
			{
				control.SetDataBinding(declaration, string.Empty);
				control.Show();
				CombineAssertions("EntryLine Grid column ability for import", () =>
				{
					var entryLineGrid = control.FindSingle<ZGrid>("EntryLineGrid");
					AssertEquals(false, entryLineGrid.GetColumnStyle(nameof(CusEntryLine.DutyReductionAmount)).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle("RandomLine+" + JobComInvoiceLine.Schema.JI_PrimaryPreference).IsUnavailable);
					AssertEquals(true, entryLineGrid.GetColumnStyle("RandomLine+" + JobComInvoiceLine.Schema.JI_FEFTAArticle48).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle("RandomLine+" + JobComInvoiceLine.Schema.JI_StorageType).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle("RandomLine+" + JobComInvoiceLine.Schema.JI_AdvanceRulingOnClassification).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle("RandomLine+" + JobComInvoiceLine.Schema.JI_AdvanceRulingOnOrigin).IsUnavailable);
					AssertEquals(true, entryLineGrid.GetColumnStyle("RandomLine+" + JobComInvoiceLine.Schema.JI_DomesticConsumptionTaxExemptionCode).IsUnavailable);
					AssertEquals(true, entryLineGrid.GetColumnStyle("RandomLine+" + nameof(JobComInvoiceLine.DomesticConsumptionTaxExemptionType)).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle(nameof(CusEntryLine.CL_ConfirmedCustomsValue)).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle(nameof(CusEntryLine.CL_ParentLineNumber)).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle(nameof(CusEntryLine.CL_MergedCustomsValue)).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle(nameof(CusEntryLine.CL_PriceCheck)).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle(nameof(CusEntryLine.CL_PriceCheckDescription)).IsUnavailable);
				});
			}

			declaration.JE_MessageType = JPJobMessageTypeList.Codes.Export;
			using (var control = new EntriesTabUserControl())
			{
				control.Show();
				control.SetDataBinding(declaration, string.Empty);
				CombineAssertions("EntryLine Grid column ability for export", () =>
				{
					var entryLineGrid = control.FindSingle<ZGrid>("EntryLineGrid");
					AssertEquals(true, entryLineGrid.GetColumnStyle(nameof(CusEntryLine.DutyReductionAmount)).IsUnavailable);
					AssertEquals(true, entryLineGrid.GetColumnStyle("RandomLine+" + JobComInvoiceLine.Schema.JI_PrimaryPreference).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle("RandomLine+" + JobComInvoiceLine.Schema.JI_FEFTAArticle48).IsUnavailable);
					AssertEquals(true, entryLineGrid.GetColumnStyle("RandomLine+" + JobComInvoiceLine.Schema.JI_StorageType).IsUnavailable);
					AssertEquals(true, entryLineGrid.GetColumnStyle("RandomLine+" + JobComInvoiceLine.Schema.JI_AdvanceRulingOnClassification).IsUnavailable);
					AssertEquals(true, entryLineGrid.GetColumnStyle("RandomLine+" + JobComInvoiceLine.Schema.JI_AdvanceRulingOnOrigin).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle("RandomLine+" + JobComInvoiceLine.Schema.JI_DomesticConsumptionTaxExemptionCode).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle("RandomLine+" + nameof(JobComInvoiceLine.DomesticConsumptionTaxExemptionType)).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle(nameof(CusEntryLine.CL_ConfirmedCustomsValue)).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle(nameof(CusEntryLine.CL_ParentLineNumber)).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle(nameof(CusEntryLine.CL_MergedCustomsValue)).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle(nameof(CusEntryLine.CL_PriceCheck)).IsUnavailable);
					AssertEquals(false, entryLineGrid.GetColumnStyle(nameof(CusEntryLine.CL_PriceCheckDescription)).IsUnavailable);
				});
			}
		}

		public void TestGetBaseMessagesTabUserControlType()
		{
			using (var control = new EntriesTabUserControl())
			{
				AssertEquals("MessagesTab is correct type", typeof(MessagesTabUserControl), control.BaseMessageUserControl.UserControlType);
			}
		}

		public void TestBGMReferenceColumnNotSetCaption()
		{
			using (var control = new EntriesTabUserControl())
			{
				AssertEquals(true, control.EntriesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().SingleOrDefault(s => s.ColumnName == nameof(CusEntryHeader.CH_BGMReference)).CaptionResourceString.IsEmpty());
			}
		}

		public void TestEntryNumberColumnNotSetCaption()
		{
			using (var control = new EntriesTabUserControl())
			{
				AssertEquals(true, control.EntriesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().SingleOrDefault(s => s.ColumnName == nameof(CusEntryHeader.EntryNumber)).CaptionResourceString.IsEmpty());
			}
		}

		public void TestInspectionStatusColumns()
		{
			using (var control = new EntriesTabUserControl())
			{
				var grid = control.EntriesBoundGrid;
				var columnStyleInspectionStatus = grid.GetColumnStyle("CH_InspectionStatus");
				var columnStyleInspectionStatusDescription = grid.GetColumnStyle("InspectionStatusDescription");
				var columnStyleCargoType = grid.GetColumnStyle("CH_CargoType");
				var columnStyleCargoTypeDescription = grid.GetColumnStyle("CargoTypeDescription");
				var columnStyleInspectionType = grid.GetColumnStyle("CH_InspectionType");
				var columnStyleInspectionTypeDescription = grid.GetColumnStyle("InspectionTypeDescription");
				var columnStyleInspectionSubType = grid.GetColumnStyle("CH_InspectionSubType");
				var columnStyleInspectionSubTypeDescription = grid.GetColumnStyle("InspectionSubTypeDescription");
				var columnStyleDocumentRequestType = grid.GetColumnStyle("CH_DocumentRequestType");
				var columnStyleDocumentRequestTypeDescription = grid.GetColumnStyle("DocumentRequestTypeDescription");

				AssertNotNull(columnStyleInspectionStatus);
				AssertNotNull(columnStyleInspectionStatusDescription);
				AssertEquals(columnStyleInspectionStatus.GroupName, columnStyleInspectionStatusDescription.GroupName);
				AssertNotNull(columnStyleCargoType);
				AssertNotNull(columnStyleCargoTypeDescription);
				AssertEquals(columnStyleCargoType.GroupName, columnStyleCargoTypeDescription.GroupName);
				AssertNotNull(columnStyleInspectionType);
				AssertNotNull(columnStyleInspectionTypeDescription);
				AssertEquals(columnStyleInspectionType.GroupName, columnStyleInspectionTypeDescription.GroupName);
				AssertNotNull(columnStyleInspectionSubType);
				AssertNotNull(columnStyleInspectionSubTypeDescription);
				AssertEquals(columnStyleInspectionSubType.GroupName, columnStyleInspectionSubTypeDescription.GroupName);
				AssertNotNull(columnStyleDocumentRequestType);
				AssertNotNull(columnStyleDocumentRequestTypeDescription);
				AssertEquals(columnStyleDocumentRequestType.GroupName, columnStyleDocumentRequestTypeDescription.GroupName);
			}
		}

		public void TestMessageStatuesColumn()
		{
			using (var control = new EntriesTabUserControl())
			{
				var grid = control.EntriesBoundGrid;
				var columnStyle = grid.GetColumnStyle(CusEntryHeader.Schema.CH_Status);
				var columnStyle2 = grid.GetColumnStyle("CH_StatusDescription");

				AssertNotNull(columnStyle);
				AssertNotNull(columnStyle2);
				AssertEquals(columnStyle.GroupName, columnStyle2.GroupName);
			}
		}

		public void TestPhaseColumns()
		{
			using (var control = new EntriesTabUserControl())
			{
				var grid = control.EntriesBoundGrid;
				var columnStyle = grid.GetColumnStyle(CusEntryHeader.Schema.CH_PhaseStatus);
				var columnStyle2 = grid.GetColumnStyle("PhaseDescription");

				AssertNotNull(columnStyle);
				AssertNotNull(columnStyle2);
				Assert(columnStyle.IsReadOnly);
				Assert(columnStyle2.IsReadOnly);
				AssertEquals(columnStyle.GroupName, columnStyle2.GroupName);
			}
		}

		public void TestEntryStatusColumns()
		{
			using (var control = new EntriesTabUserControl())
			{
				var grid = control.EntriesBoundGrid;
				var columnStyle = grid.GetColumnStyle(CusEntryHeader.Schema.CH_EntryStatus);
				var columnStyle2 = grid.GetColumnStyle(nameof(CusEntryHeader.CH_EntryStatusDescription));

				AssertNotNull(columnStyle);
				AssertNotNull(columnStyle2);
				AssertEquals(columnStyle.GroupName, columnStyle2.GroupName);
			}
		}
	}
}
