using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	public class EntryHeaderFilterUserControlTest : Customs.Module.Testing.EntryHeaderFilterUserControlTest
	{
		protected override List<string> FilteredGridColumns
		{
			get
			{
				var list = new List<string>()
				{
					EntryHeaderFilterUserControl.Schema.EntryStyle,
					EntryHeaderFilterUserControl.Schema.Warehouse,
					EntryHeaderFilterUserControl.Schema.CustomsOfficeOfPresentation,
					EntryHeaderFilterUserControl.Schema.SupplierCode,
					EntryHeaderFilterUserControl.Schema.ImporterCode,
					EntryHeaderFilterUserControl.Schema.DeclarationOrigin,
					EntryHeaderFilterUserControl.Schema.DeclarationDestination,
					EntryHeaderFilterUserControl.Schema.ApprovalDeferNo,
					EntryHeaderFilterUserControl.Schema.TotalGrossWeight,
					EntryHeaderFilterUserControl.Schema.TotalCustomsQuantity,
					EntryHeaderFilterUserControl.Schema.LocalClientCode,
					EntryHeaderFilterUserControl.Schema.FromWarehouse,
					EntryHeaderFilterUserControl.Schema.ToWarehouse,
					EntryHeaderFilterUserControl.Schema.InvoiceCurrency,
					EntryHeaderFilterUserControl.Schema.InvoiceAmount,
					EntryHeaderFilterUserControl.Schema.EntryLinesCount,
					EntryHeaderFilterUserControl.Schema.Mrn,
					EntryHeaderFilterUserControl.Schema.ExitStatus,
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
					EntryHeaderFilterUserControl.Schema.EntryStyle,
					EntryHeaderFilterUserControl.Schema.Warehouse,
					EntryHeaderFilterUserControl.Schema.CustomsOfficeOfPresentation,
					EntryHeaderFilterUserControl.Schema.SupplierCode,
					EntryHeaderFilterUserControl.Schema.ImporterCode,
					EntryHeaderFilterUserControl.Schema.DeclarationOrigin,
					EntryHeaderFilterUserControl.Schema.DeclarationDestination,
					EntryHeaderFilterUserControl.Schema.ApprovalDeferNo,
					EntryHeaderFilterUserControl.Schema.TotalGrossWeight,
					EntryHeaderFilterUserControl.Schema.TotalCustomsQuantity,
					EntryHeaderFilterUserControl.Schema.LocalClientCode,
					EntryHeaderFilterUserControl.Schema.FromWarehouse,
					EntryHeaderFilterUserControl.Schema.ToWarehouse,
					EntryHeaderFilterUserControl.Schema.InvoiceCurrency,
					EntryHeaderFilterUserControl.Schema.InvoiceAmount,
					EntryHeaderFilterUserControl.Schema.EntryLinesCount,
					EntryHeaderFilterUserControl.Schema.Mrn,
					EntryHeaderFilterUserControl.Schema.ExitStatus,
				});
				return list;
			}
		}

		protected override Customs.Module.EntryHeaderFilterUserControl GetNewEntryHeaderFilterUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeaders = new CusEntryHeaderCollection<CusEntryHeader>(declaration, Factory);
			var filterBusinessObject = new EntryHeaderFilterBusinessObject();
			return new EntryHeaderFilterUserControl(cusEntryHeaders, filterBusinessObject);
		}

		[RequiresSTA]
		public void TestTotalCustomsQuantityColumnHasDecimalValueEqualToCustomsQuantityDecimalScale()
		{
			using (var filterUserControl = GetNewEntryHeaderFilterUserControl())
			{
				ZDisplayGrid filteredGrid = filterUserControl.FilteredGrid;
				var totalCustomsQuantityColumnStyle = (ZCalcEditColumnStyleInfo)filteredGrid
					.GetColumnStyle(EntryHeaderFilterUserControl.Schema.TotalCustomsQuantity);

				AssertNotNull("Total Customs Quantity Column style", totalCustomsQuantityColumnStyle);
				AssertEquals("Decimal Property", JobComInvoiceLineSchema.JI_CustomsQuantity.Scale, totalCustomsQuantityColumnStyle.Decimals);
			}
		}
	}
}
