using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using CusTempStorageRegHeader = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
{
	class TempStorageRegisterFilterStripControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestGridColumnNames()
		{
			using (var form = new ZForm())
			using (var filterControl = GetFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.Grid;

				CombineAssertions(() =>
				{
					foreach (var (columnName, columnCaption, _) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnCaption, grid.GetColumnCaption(columnName));
					}
				});
			}
		}

		[RequiresSTA]
		public void TestGridDefaultColumnOrder()
		{
			using (var form = new ZForm())
			using (var filterControl = GetFilterControl())
			{
				form.Controls.Add(filterControl);
				form.Show();
				var gridColumns = filterControl.Grid.DefaultColumns;

				AssertSequencesEqual(OrderedColumnDetails.Select(x => x.ColumnName), gridColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		[RequiresSTA]
		public void TestGridColumnWidths()
		{
			using (var filterControl = GetFilterControl())
			{
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					foreach (var (columnName, _, columnWidth) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnWidth, grid.GetColumnStyle(columnName).Width);
					}
				});
			}
		}

		[RequiresSTA]
		public void TestPresentationDateColumnFormat()
		{
			using (var filterControl = GetFilterControl())
			{
				var presentationDateColumn = filterControl.Grid.GetColumnStyle(CusTempStorageRegHeader.Schema.SRH_PresentationDate) as ZDateEditColumnStyleInfo;
				AssertEquals(Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long, presentationDateColumn.DateTimeFormat);
			}
		}

		[RequiresSTA]
		public void TestPreviousReferenceGroup()
		{
			using (var filterControl = GetFilterControl())
			{
				CombineAssertions(() =>
				{
					var previousReferenceColumn = filterControl.Grid.GetColumnStyle(CusTempStorageRegHeader.Schema.SRH_PreviousReference) as ZTextBoxColumnStyleInfo;
					AssertEquals("ShortCaption", "Previous Ref.", previousReferenceColumn.GroupName.ShortCaption);
					AssertEquals("Caption", "Previous Reference", previousReferenceColumn.GroupName.Caption);

					var previousReferenceTypeColumn = filterControl.Grid.GetColumnStyle(CusTempStorageRegHeader.Schema.SRH_PreviousReferenceType) as ZTextBoxColumnStyleInfo;
					AssertEquals("PreviousReferenceType has the same GroupName", previousReferenceTypeColumn.GroupName.Key, previousReferenceColumn.GroupName.Key);
				});
			}
		}

		TempStorageRegisterFilterStripControl GetFilterControl()
		{
			return new TempStorageRegisterFilterStripControl(new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory), new TempStorageRegisterFilterBusinessObject());
		}

		(string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails => new[]
		{
			(CusTempStorageRegHeader.Schema.SRH_Reference, "TSD Number", 86),
			(CusTempStorageRegHeader.Schema.SRH_InternalReference, "Job Reference", 120),
			(CusTempStorageRegHeader.Schema.SRH_ArrivalDate, "Arrival Date", 80),
			(CusTempStorageRegHeader.Schema.SRH_PresentationDate, "Presentation Date", 109),
			(CusTempStorageRegHeader.Schema.SRH_PreviousReferenceType, "Previous Reference Type", 143),
			(CusTempStorageRegHeader.Schema.SRH_PreviousReference, "Previous Reference Number", 158),
			(CusTempStorageRegHeader.Schema.SRH_Status, "Status", 53),
			("PackageType", "Package Type", 150),
			("PackageQty", "Package Qty", 150),
			("LineCount", "Number of Lines", 150),
			("RemainingPackagesQty", "Remaining Packages Qty", 150),
			(CusTempStorageRegHeader.Schema.SRH_CustomsOffice, "Customs Office", 150)
		};
	}
}
