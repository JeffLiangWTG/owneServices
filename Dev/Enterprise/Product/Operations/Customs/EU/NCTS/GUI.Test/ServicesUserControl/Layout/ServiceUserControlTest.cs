using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class ServiceUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(IHaveServices), userControl.BindingSource.DataSourceType);
		}

		public void TestMeasurementBasisDropEdit()
		{
			var control = userControl.MeasurementBasisDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", control);
				AssertEquals("BindTo", nameof(IHaveServices.Services) + "." + nameof(JobService.ES_MeasurementBasis), control.BindTo);
			});
		}

		public void TestReferenceTextBox()
		{
			var control = userControl.ReferenceTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", control);
				AssertEquals("BindTo", nameof(IHaveServices.Services) + "." + nameof(JobService.ES_References), control.BindTo);
			});
		}

		public void TestDurationTimeEdit()
		{
			var control = userControl.MeasurementBasisDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", control);
				AssertEquals("BindTo", nameof(IHaveServices.Services) + "." + nameof(JobService.ES_MeasurementBasis), control.BindTo);
			});
		}

		public void TestServiceCountCalcEdit()
		{
			var control = userControl.ServiceCountCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>("Type", control);
				AssertEquals("BindTo", nameof(IHaveServices.Services) + "." + nameof(JobService.ES_ServiceCount), control.BindTo);
			});
		}

		public void TestSubLocationTextBox()
		{
			var control = userControl.SubLocationTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", control);
				AssertEquals("BindTo", nameof(IHaveServices.Services) + "." + nameof(JobService.ES_SubLocation), control.BindTo);
			});
		}

		public void TestContractorCodeFindBox()
		{
			var control = userControl.ContractorCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZOrganisationFindBox>("Type", control);
				AssertEquals("BindTo", nameof(IHaveServices.Services) + "." + nameof(JobService.ES_OH_Contractor), control.BindTo);
			});
		}

		public void TestNotesTextBox()
		{
			var control = userControl.NotesTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", control);
				AssertEquals("BindTo", nameof(IHaveServices.Services) + "." + nameof(JobService.ES_ServiceNote), control.BindTo);
			});
		}

		public void TestServiceLocationAddressControl()
		{
			var control = userControl.ServiceLocationAddressControl;
			CombineAssertions(() =>
			{
				AssertType<ZAddressControl>("Type", control);
				AssertEquals("BindTo", nameof(IHaveServices.Services) + "." + nameof(JobService.ES_OA_Location), control.BindTo);
			});
		}

		public void TestCompletedDateEdit()
		{
			var control = userControl.CompletedDateEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDateEdit>("Type", control);
				AssertEquals("BindTo", nameof(IHaveServices.Services) + "." + nameof(JobService.ES_Completed), control.BindTo);
			});
		}

		public void TestBookedDateEdit()
		{
			var control = userControl.BookedDateEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDateEdit>("Type", control);
				AssertEquals("BindTo", nameof(IHaveServices.Services) + "." + nameof(JobService.ES_Booked), control.BindTo);
			});
		}

		public void TestServiceTypeDropEdit()
		{
			var control = userControl.ServiceTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", control);
				AssertEquals("BindTo", nameof(IHaveServices.Services) + "." + nameof(JobService.ES_ServiceCode), control.BindTo);
			});
		}

		public void TestRateAndCurrencyCalcFindBox()
		{
			var control = userControl.RateAndCurrencyCalcFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCalcFindBox>("Type", control);
				AssertEquals("BindToAmount", nameof(IHaveServices.Services) + "." + nameof(JobService.ES_ServiceRate), control.BindToAmount);
				AssertEquals("BindToUnit", nameof(IHaveServices.Services) + "." + nameof(JobService.ES_RX_NKServiceRateCurrency), control.BindToUnit);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ServiceUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
		ServiceUserControl userControl;
	}
}
