using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.Module.Testing
{
	class SumAFilterControlTest : TestCaseWithFactory
	{
		public void TestGridColumnNames()
		{
			using (var form = new ZForm())
			using (var filterControl = new SumAFilterControl(new EU.Business.CusTempStorage.CusTempStorageJobHeaderCollection<CusTempStorageJobHeader>(Factory, GlbBranch.CurrentBranch), new SumAFilterStripBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.Grid;

				CombineAssertions(() =>
				{
					foreach (var (columnName, columnCaption, columnWidth) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnCaption, grid.GetColumnCaption(columnName));
					}
				});
			}
		}

		public void TestGridDefaultColumnOrder()
		{
			using (var form = new ZForm())
			using (var filterControl = new SumAFilterControl(new EU.Business.CusTempStorage.CusTempStorageJobHeaderCollection<CusTempStorageJobHeader>(Factory, GlbBranch.CurrentBranch), new SumAFilterStripBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();

				AssertSequencesEqual(OrderedColumnDetails.Select(x => x.ColumnName), filterControl.Grid.DefaultColumns.Where(x => x.IsVisible).Select(x => x.ColumnName));
			}
		}

		public void TestGridColumnWidths()
		{
			using (var filterControl = new SumAFilterControl(new EU.Business.CusTempStorage.CusTempStorageJobHeaderCollection<CusTempStorageJobHeader>(Factory, GlbBranch.CurrentBranch), new SumAFilterStripBusinessObject()))
			{
				var grid = filterControl.Grid;
				CombineAssertions(() =>
				{
					foreach (var (columnName, columnCaption, columnWidth) in OrderedColumnDetails)
					{
						AssertEquals(columnName, columnWidth, grid.GetColumnStyle(columnName).Width);
					}
				});
			}
		}

		public void TestPresentationDateColumnFormat()
		{
			using (var filterControl = new SumAFilterControl(new EU.Business.CusTempStorage.CusTempStorageJobHeaderCollection<CusTempStorageJobHeader>(Factory, GlbBranch.CurrentBranch), new SumAFilterStripBusinessObject()))
			{
				var presentationDateColumn = filterControl.Grid.GetColumnStyle(CusTempStorageJobHeader.Schema.SJH_PresentationDate) as ZDateEditColumnStyleInfo;
				AssertEquals(Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long, presentationDateColumn.DateTimeFormat);
			}
		}

		(string ColumnName, string ColumnCaption, int ColumnWidth)[] OrderedColumnDetails => new (string, string, int)[]
		{
			(CusTempStorageJobHeader.Schema.SJH_AppCode, "Application", 76),
			(CusTempStorageJobHeader.Schema.MostRecentlyModifiedDeclarationType, "Current Declaration Type", 144),
			(CusTempStorageJobHeader.Schema.SJH_JobReference, "Job Number", 82),
			(CusTempStorageJobHeader.Schema.SJH_ReferenceNumber, "Reference Number", 149),
			(CusTempStorageJobHeader.Schema.MostRecentlyModifiedDeclarationRegistrationNumber, "Current Registration No.", 141),
			(CusTempStorageJobHeader.Schema.MostRecentlyModifiedDeclarationMessageStatus, "Current Message Status", 138),
			(nameof(CusTempStorageJobHeader.Customer) + "+" + nameof(OrgHeader.OH_Code), "Customer Code", 97),
			(nameof(CusTempStorageJobHeader.Customer) + "+" + nameof(OrgHeader.OH_FullName), "Customer Name", 250),
			(nameof(CusTempStorageJobHeader.Presenter) + "+" + nameof(OrgAddress.EffectiveCompanyName), "Presenter", 250),
			(nameof(CusTempStorageJobHeader.Representative) + "+" + nameof(OrgAddress.EffectiveCompanyName), "Representative", 250),
			(CusTempStorageJobHeader.Schema.SJH_TransportMode, "Transport Mode", 100),
			(CusTempStorageJobHeader.Schema.SJH_RL_NKLoading, "Loading", 61),
			(CusTempStorageJobHeader.Schema.SJH_ArrivalDate, "Arrival Date", 80),
			(CusTempStorageJobHeader.Schema.SJH_PresentationDate, "Presentation Date", 109),
			(CusTempStorageJobHeader.Schema.SJH_NCTSFlag, "NCTS", 50),
			(CusTempStorageJobHeader.Schema.SJH_CustomsOffice, "Customs Office", 96),
			(CusTempStorageJobHeader.Schema.SJH_CustomsOfficeOfEntryIntoEU, "Office of Entry", 93),
			(CusTempStorageJobHeader.Schema.SJH_PreviousReferenceType, "Previous Ref Type", 111),
			(CusTempStorageJobHeader.Schema.SJH_PreviousReferenceNumber, "Previous Ref Number", 125),
			(CusTempStorageJobHeader.Schema.SJH_GB, "Branch", 57)
		};
	}
}
