using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using static Enterprise.Customs.IN.Business.CusEntryInstruction;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(EntryInstructionTopPanelUserControl))]
sealed class EntryInstructionTopPanelUserControlTest : TestCaseWithFactory
{
	public void TestColumnsInfos()
	{
		using var userControl = new EntryInstructionTopPanelUserControl();

		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = Constants.TransportModes.Sea;
		userControl.JobDeclaration = Declaration;

		var controlGrid = (ZGrid)userControl.Controls.Find("EntryInstructionGrid", searchAllChildren: true).Single();
		var styleColumn = controlGrid.GetColumnStyle(Schema.CEI_Style);
		var subStyleColumn = controlGrid.GetColumnStyle(Schema.CEI_SubStyle);
		var sbNumberColumn = controlGrid.GetColumnStyle(Schema.ShippingBillNumber);
		var sbDateColumn = controlGrid.GetColumnStyle(Schema.ShippingBillDate);
		CombineAssertions("Column availability check", () =>
		{
			Assert("CEI_Style column is available for export", !styleColumn.IsUnavailable);
			Assert("CEI_SubStyle column is available for export", !subStyleColumn.IsUnavailable);
			Assert("SBNumber column is available for export", !sbNumberColumn.IsUnavailable);
			Assert("SBDate column is available for export", !sbDateColumn.IsUnavailable);

			Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			Assert("CEI_Style column is unavailable for import", styleColumn.IsUnavailable);
			Assert("CEI_SubStyle column is unavailable for import", subStyleColumn.IsUnavailable);
			Assert("SBNumber column is unavailable for import", sbNumberColumn.IsUnavailable);
			Assert("SBDate column is unavailable for import", sbDateColumn.IsUnavailable);
		});

		CombineAssertions("Column Readonly check", () =>
		{
			Assert("SBNumber column is readonly", sbNumberColumn.IsReadOnly);
			Assert("SBDate column is readonly", sbDateColumn.IsReadOnly);
		});
	}

	public void TestEntryInstructionGridColumnOrder()
	{
		using var userControl = new EntryInstructionTopPanelUserControl();
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = Constants.TransportModes.Sea;
		userControl.JobDeclaration = Declaration;

		var controlGrid = (ZGrid)userControl.Controls.Find("EntryInstructionGrid", searchAllChildren: true).Single();
		var columnsOrder = controlGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).Take(4).ToArray();
		var expectedColumnOrder = new[]
		{
			Schema.LocalReferenceNumber,
			Schema.LocalReferenceNumberDate,
			Schema.CEI_Style,
			Schema.CEI_SubStyle
		};
		AssertArrayEqualsByElements(expectedColumnOrder, columnsOrder);
	}

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;
}
