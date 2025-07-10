using CargoWise.Types;
using Enterprise.Client.JAS.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.Cognos.Testing
{
	class CognosDataExporterBizOValidationTest : JASDataExporterBizOValidationTest
	{
		public void TestValidateEndingPeriod()
		{
			CreatePeriod(200601, new ZDateTime(2006, 1, 1), new ZDateTime(2006, 3, 31));
			AssertNoErrors("Pre-condition", DataExporter.EndingPeriodInfo);
			((CognosDataExporterBizOValidation)DataExporter.Validation).ValidateEndingPeriod();
			AssertMandatoryValidationError(DataExporter.EndingPeriodInfo, true);
			DataExporter.EndingPeriod = 200401;
			AssertHasError(DataExporter.EndingPeriodInfo, "Invalid period. Please enter period as the following format YYYYMM (i.e. 200512) and please ensure that the period is set up in GL.");
			DataExporter.EndingPeriod = 200601;
			AssertNoErrors(DataExporter.EndingPeriodInfo);
		}

		public void TestValidateEndingPeriodIsCalledByValidateAll()
		{
			AssertNoErrors("Pre-condition", DataExporter.EndingPeriodInfo);
			DataExporter.Validation.ValidateAll();
			AssertHasErrors("ValidateAll() should call ValidateEndingPeriod", DataExporter.EndingPeriodInfo);
		}

		#region Implementation
		CognosDataExporterBizO DataExporter
		{
			get
			{
				if (fDataExporter == null)
				{
					fDataExporter = new CognosDataExporterBizO();
				}

				return fDataExporter;
			}
		}

		void CreatePeriod(ZInt period, ZDateTime start, ZDateTime end)
		{
			AccPeriodManagement periodManagement = Factory.New<AccPeriodManagement>();
			periodManagement.AM_Period = period;
			periodManagement.AM_StartDate = start;
			periodManagement.AM_EndDate = end;
			periodManagement.AM_Year = (ZShort)start.Year;
			periodManagement.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();
		}

		CognosDataExporterBizO fDataExporter;
		#endregion
	}
}
