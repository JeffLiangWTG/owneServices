using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.IT.Business.Reports.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class FilterByCompanyPkReport_ITEntryPayInfoTest : FilterByBaseReport_ITEntryPayInfoTest
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@CompanyPk", GlbCompany.CurrentCompany.PK.ToString());
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		var newCompany = Factory.New<GlbCompany>();
		var newBranch = newCompany.Branches.AddNew();
		var nctsHeaderFromAnotherBranch = Factory.NewDepartureNctsHeader();
		nctsHeaderFromAnotherBranch.BH_GB = newBranch.PK;
		new Job.Loader(nctsHeaderFromAnotherBranch).TryLoadOrCreate();
		var payInfoFromAnotherBranch = nctsHeaderFromAnotherBranch.MovementHeader.PayInfoCollection.AddNew();
		payInfoFromAnotherBranch.BPI_IncomingPayResponseNo = "5678";
	}
}

sealed class FilterByCustomsRegistrationDateReport_ITEntryPayInfoTest : FilterByBaseReport_ITEntryPayInfoTest
{
	[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
	[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
	public new void TestRows() => base.TestRows();

	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@CompanyPk", ITCompany.PK.ToString());
		yield return ("@CustomsRegistrationDateFrom", new ZDateTime(2021, 01, 01).ToString("s"));
		yield return ("@CustomsRegistrationDateTo", new ZDateTime(2021, 05, 01).ToString("s"));
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		nctsHeader.BH_GB = jobHeader.JH_GB = ITCompany.Branches[0].PK;
		jobHeader.JH_GC = ITCompany.PK;
		var entryNumber = Factory.NewCusEntryNumber(nctsHeader, "REG", "123", new ZDateTime(2021, 03, 01));
		entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;

		var otherNctsHeader = Factory.NewDepartureNctsHeader();
		var otherEntryNumber = Factory.NewCusEntryNumber(otherNctsHeader, "REG", "456", new ZDateTime(2021, 06, 01));
		otherEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		new Job.Loader(otherNctsHeader).TryLoadOrCreate();
		var otherPayInfo = otherNctsHeader.MovementHeader.PayInfoCollection.AddNew();
		otherPayInfo.BPI_IncomingPayResponseNo = "5678";
	}

	GlbCompany ITCompany
	{
		get
		{
			if (itCompany == null)
			{
				itCompany = Factory.New<GlbCompany>();
				itCompany.GC_Code = "ITC";
				itCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
				var itBranch = itCompany.Branches.AddNew();
				itBranch.GB_Code = "ITB";
				itBranch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			}
			return itCompany;
		}
	}
	GlbCompany itCompany;
}

sealed class FilterByPaymentDateReport_ITEntryPayInfoTest : FilterByBaseReport_ITEntryPayInfoTest
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@CompanyPk", GlbCompany.CurrentCompany.PK.ToString());
		yield return ("@PaymentDateFrom", new ZDateTime(2021, 01, 01).ToString("s"));
		yield return ("@PaymentDateTo", new ZDateTime(2021, 05, 01).ToString("s"));
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		payInfo.BPI_PaymentDate = new ZDateTime(2021, 03, 01);

		var otherNctsHeader = Factory.NewDepartureNctsHeader();
		new Job.Loader(otherNctsHeader).TryLoadOrCreate();
		var otherPayInfo = otherNctsHeader.MovementHeader.PayInfoCollection.AddNew();
		otherPayInfo.BPI_IncomingPayResponseNo = "5678";
		otherPayInfo.BPI_PaymentDate = new ZDateTime(2021, 06, 01);
	}
}

sealed class FilterByDefermentAccountNumberReport_ITEntryPayInfoTest : FilterByBaseReport_ITEntryPayInfoTest
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@CompanyPk", GlbCompany.CurrentCompany.PK.ToString());
		yield return ("@DefermentAccountNumber", "AAA");
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		nctsHeader.MovementHeader.DefermentAccountNumber = "AAA";

		var otherNctsHeader = Factory.NewDepartureNctsHeader();
		otherNctsHeader.MovementHeader.DefermentAccountNumber = "BBB";
		new Job.Loader(otherNctsHeader).TryLoadOrCreate();
		var otherPayInfo = otherNctsHeader.MovementHeader.PayInfoCollection.AddNew();
		otherPayInfo.BPI_IncomingPayResponseNo = "5678";
	}
}

sealed class FilterByCustomsOfficeReport_ITEntryPayInfoTest : FilterByBaseReport_ITEntryPayInfoTest
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@CompanyPk", GlbCompany.CurrentCompany.PK.ToString());
		yield return ("@CustomsOffice", "AAA");
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		nctsHeader.CustomsOffices.AddNew("DEP", "AAA");

		var otherNctsHeader = Factory.NewDepartureNctsHeader();
		otherNctsHeader.CustomsOffices.AddNew("DEP", "BBB");
		new Job.Loader(otherNctsHeader).TryLoadOrCreate();
		var otherPayInfo = otherNctsHeader.MovementHeader.PayInfoCollection.AddNew();
		otherPayInfo.BPI_IncomingPayResponseNo = "5678";
	}
}

abstract class FilterByBaseReport_ITEntryPayInfoTest : Report_ITEntryPayInfoTest
{
	protected override IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults()
	{
		yield return new (string, string)[]
		{
			("A93Number", payInfo.BPI_IncomingPayResponseNo),
		};
	}

	protected override void PrepareTestData()
	{
		nctsHeader = Factory.NewDepartureNctsHeader();
		jobHeader = new Job.Loader(nctsHeader).TryLoadOrCreate();
		payInfo = nctsHeader.MovementHeader.PayInfoCollection.AddNew();
		payInfo.BPI_IncomingPayResponseNo = "1234";
	}

	protected NctsHeader nctsHeader;
	protected JobHeader jobHeader;
	protected NctsDeparturePayInfo payInfo;
}
