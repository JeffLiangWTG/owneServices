using System.Collections.Generic;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.Testing
{
	[TestedType(typeof(EntryHeaderFilterUserControl))]
	public class EntryHeaderFilterUserControlTest : EU.Module.Testing.EntryHeaderFilterUserControlTest
	{
		protected override List<string> FilteredGridColumns
		{
			get
			{
				var list = new List<string>()
				{
					EntryHeaderFilterUserControl.Schema.ExitActualOffice,
					EntryHeaderFilterUserControl.Schema.ExitDate,
					EntryHeaderFilterUserControl.Schema.InventoryConsignmentReference,
				};
				list.AddRange(base.FilteredGridColumns);
				return list;
			}
		}

		protected override List<string> ColumnNamesInSortOrder
		{
			get
			{
				var list = base.ColumnNamesInSortOrder;
				list.AddRange(new List<string>()
				{
					EntryHeaderFilterUserControl.Schema.ExitActualOffice,
					EntryHeaderFilterUserControl.Schema.ExitDate,
					EntryHeaderFilterUserControl.Schema.InventoryConsignmentReference,
				});
				return list;
			}
		}

		public void TestAdditionalColumnsExist()
		{
			using (var filterUserControl = GetNewEntryHeaderFilterUserControl())
			{
				ZDisplayGrid filteredGrid = filterUserControl.FilteredGrid;
				var exitActualOfficeColumnStyle = (ZTextBoxColumnStyleInfo)filteredGrid
					.GetColumnStyle(EntryHeaderFilterUserControl.Schema.ExitActualOffice);

				var exitDateColumnStyle = (ZTextBoxColumnStyleInfo)filteredGrid
					.GetColumnStyle(EntryHeaderFilterUserControl.Schema.ExitDate);

				var inventoryConsignmentReferenceColumnStyle = (ZTextBoxColumnStyleInfo)filteredGrid
					.GetColumnStyle(EntryHeaderFilterUserControl.Schema.InventoryConsignmentReference);

				AssertNotNull("Exit Actual Office Column style", exitActualOfficeColumnStyle);
				AssertEquals(true, exitActualOfficeColumnStyle.IsVisible);

				AssertNotNull("Exit Date Office Column style", exitDateColumnStyle);
				AssertEquals(true, exitDateColumnStyle.IsVisible);

				AssertNotNull("Inventory Consignment Reference Column style", inventoryConsignmentReferenceColumnStyle);
				AssertEquals(true, inventoryConsignmentReferenceColumnStyle.IsVisible);
			}
		}

		protected override Customs.Module.EntryHeaderFilterUserControl GetNewEntryHeaderFilterUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeaders = new EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>(declaration, Factory);
			var filterBusinessObject = new EntryHeaderFilterBusinessObject();
			return new EntryHeaderFilterUserControl(cusEntryHeaders, filterBusinessObject);
		}
	}
}
