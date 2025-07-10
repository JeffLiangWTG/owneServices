using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using static Enterprise.MasterFiles.Business.AutoJobService.Schema;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class ServicesGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(IHaveServices), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns", new[] {
				ES_ServiceCode,
				ES_OH_Contractor,
				"ServiceProviderPK",
				ES_OA_Location,
				ES_SubLocation,
				ES_Booked,
				ES_ServiceCount,
				ES_Completed,
				ES_Duration,
				ES_ServiceRate,
				ES_RX_NKServiceRateCurrency,
				ES_MeasurementBasis,
				ES_ServiceNote,
				ES_References,
			},
			servicesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumnCaptions()
		{
			CombineAssertions(() =>
			{
				AssertColumnCaptions("ServiceProviderPK", "Service Location", "Serv. Location", "Location");
				AssertColumnCaptions(ES_OA_Location, "Service Location Address", "Serv. Loc. Address", "Serv. Loc.");
				AssertColumnCaptions(ES_SubLocation, "Sub Location", "", "Sub Loc.");
				AssertColumnCaptions(ES_Duration, "Service Duration", "Serv. Duration", "Duration");
				AssertColumnCaptions(ES_ServiceRate, "Service Rate", "Serv. Rate", "Rate");
				AssertColumnCaptions(ES_RX_NKServiceRateCurrency, "Service Rate Currency", "Serv. Rate Currency", "Currency");
				AssertColumnCaptions(ES_MeasurementBasis, "Measurement Basis", "", "Meas. Basis");
				AssertColumnCaptions(ES_ServiceNote, "Service Notes", "Serv. Notes", "Notes");
				AssertColumnCaptions(ES_References, "Service Reference", "Serv. Reference", "Reference");
			});
		}

		public void TestColumns()
		{
			CombineAssertions(() =>
			{
				AssertColumn<ZDropEditColumnStyleInfo>(ES_ServiceCode, 0, 50);
				AssertColumn<ZGuidFindBoxColumnStyleInfo>(ES_OH_Contractor, 1, 100);
				AssertColumn<ZOrganisationFindBoxColumnStyleInfo>("ServiceProviderPK", 2, 96);
				AssertColumn<ZAddressDropEditColumnStyleInfo>(ES_OA_Location, 3, 132);
				AssertColumn<ZTextBoxColumnStyleInfo>(ES_SubLocation, 4, 126);
				AssertColumn<ZDateEditColumnStyleInfo>(ES_Booked, 5, 100);
				AssertColumn<ZCalcEditColumnStyleInfo>(ES_ServiceCount, 6, 50);
				AssertColumn<ZDateEditColumnStyleInfo>(ES_Completed, 7, 100);
				AssertColumn<ZTimeEditExColumnStyleInfo>(ES_Duration, 8, 50);
				AssertColumn<ZCalcEditColumnStyleInfo>(ES_ServiceRate, 9, 66);
				AssertColumn<ZCodeFindBoxColumnStyleInfo>(ES_RX_NKServiceRateCurrency, 10, 50);
				AssertColumn<ZDropEditColumnStyleInfo>(ES_MeasurementBasis, 11, 64);
				AssertColumn<ZTextBoxColumnStyleInfo>(ES_ServiceNote, 12, 85);
				AssertColumn<ZTextBoxColumnStyleInfo>(ES_References, 13, 66);
			});
		}

		void AssertColumn<T>(string name, int index, int width) where T : ZGridColumnInfo
		{
			var columnStyle = servicesGrid.GetColumnStyle(name);

			AssertType<T>($"{name} Type", columnStyle);
			AssertEquals($"{name} Index", index, servicesGrid.ColumnStyles.IndexOf(columnStyle));
			AssertEquals($"{name} Width", width, columnStyle.Width);
			AssertEquals($"{name} Visible", true, columnStyle.IsVisible);
		}

		void AssertColumnCaptions(string name, string caption, string mediumCaption, string shortCaption)
		{
			var columnStyle = servicesGrid.GetColumnStyle(name);

			AssertEquals($"{name} Caption", caption, columnStyle.CaptionResourceString.Caption);
			AssertEquals($"{name} MediumCaption", mediumCaption, columnStyle.CaptionResourceString.MediumCaption);
			AssertEquals($"{name} ShortCaption", shortCaption, columnStyle.CaptionResourceString.ShortCaption);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			var srv = header.Services.AddNew();
			userControl = new ServicesGridUserControl();
			userControl.SetDataBinding(header, "");
			servicesGrid = userControl.ServicesGrid;
		}
		ServicesGridUserControl userControl;
		ZGrid servicesGrid;
		NctsHeader header;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
