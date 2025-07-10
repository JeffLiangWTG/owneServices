using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.IT.Business.Reports.Testing;

#region Filter By Test Classes

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
		var declarationFromAnotherBranch = Factory.New<JobDeclaration>();
		declarationFromAnotherBranch.JE_GB = newBranch.PK;
		declarationFromAnotherBranch.JE_GC = newCompany.PK;
		new Job.Loader(declarationFromAnotherBranch).TryLoadOrCreate();
		var entryHeaderFromAnotherBranch = declarationFromAnotherBranch.CustomsEntryHeaders.AddNew();
		var entryPayInfoFromAnotherBranch = entryHeaderFromAnotherBranch.EntryPayInfos.AddNew();
		entryPayInfoFromAnotherBranch.C9_IncomingPayResponseNo = "5678";
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

		declaration.JE_GC = jobHeader.JH_GC = ITCompany.PK;
		declaration.JE_GB = jobHeader.JH_GB = ITCompany.Branches[0].PK;
		var entryNumber = Factory.NewCusEntryNumber(entryHeader, "REG", "123", new ZDateTime(2021, 03, 01));
		entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		var otherDeclaration = Factory.New<JobDeclaration>();
		new Job.Loader(otherDeclaration).TryLoadOrCreate();
		var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
		var otherEntryNumber = Factory.NewCusEntryNumber(otherEntryHeader, "REG", "456", new ZDateTime(2021, 06, 01));
		otherEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		var otherEntryPayInfo = otherEntryHeader.EntryPayInfos.AddNew();
		otherEntryPayInfo.C9_IncomingPayResponseNo = "5678";
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

		entryPayInfo.C9_PaymentDate = new ZDateTime(2021, 03, 01);

		var otherDeclaration = Factory.New<JobDeclaration>();
		new Job.Loader(otherDeclaration).TryLoadOrCreate();
		var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
		var otherEntryPayInfo = otherEntryHeader.EntryPayInfos.AddNew();
		otherEntryPayInfo.C9_IncomingPayResponseNo = "5678";
		otherEntryPayInfo.C9_PaymentDate = new ZDateTime(2021, 06, 01);
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

		declaration.JE_DefermentAccountNumber = "AAA";

		var otherDeclaration = Factory.New<JobDeclaration>();
		otherDeclaration.JE_DefermentAccountNumber = "BBB";
		new Job.Loader(otherDeclaration).TryLoadOrCreate();
		var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
		var otherEntryPayInfo = otherEntryHeader.EntryPayInfos.AddNew();
		otherEntryPayInfo.C9_IncomingPayResponseNo = "5678";
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

		declaration.JE_CustomsOffice = "AAA";

		var otherDeclaration = Factory.New<JobDeclaration>();
		otherDeclaration.JE_CustomsOffice = "BBB";
		new Job.Loader(otherDeclaration).TryLoadOrCreate();
		var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
		var otherEntryPayInfo = otherEntryHeader.EntryPayInfos.AddNew();
		otherEntryPayInfo.C9_IncomingPayResponseNo = "5678";
	}
}

abstract class FilterByBaseReport_ITEntryPayInfoTest : Report_ITEntryPayInfoTest
{
	protected override IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults()
	{
		yield return new (string, string)[]
		{
			("A93Number", entryPayInfo.C9_IncomingPayResponseNo),
		};
	}

	protected override void PrepareTestData()
	{
		declaration = Factory.New<JobDeclaration>();
		jobHeader = new Job.Loader(declaration).TryLoadOrCreate();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryPayInfo = entryHeader.EntryPayInfos.AddNew();
		entryPayInfo.C9_IncomingPayResponseNo = "1234";
	}
	protected JobDeclaration declaration;
	protected JobHeader jobHeader;
	protected CusEntryHeader entryHeader;
	protected CusEntryPayInfo entryPayInfo;
}

#endregion

#region Report_ITEntryPayInfoTest

public abstract class Report_ITEntryPayInfoTest : ITReportFunctionalTestCase
{
	protected sealed override ZString ObjectName => "Report_ITEntryPayInfo";

	protected sealed override IEnumerable<(Type Type, string Name)> GetExpectedColumnsInAnyOrder()
	{
		yield return (typeof(string), "A93Number");
		yield return (typeof(string), "CustomsOffice");
		yield return (typeof(string), "JobNumber");
		yield return (typeof(string), "ImporterCode");
		yield return (typeof(string), "ImporterFullName");
		yield return (typeof(string), "LocalClientCode");
		yield return (typeof(string), "LocalClientFullName");
		yield return (typeof(string), "A93MethodOfPayment");
		yield return (typeof(decimal), "A93Amount");
		yield return (typeof(DateTime), "A93PaymentDate");
		yield return (typeof(string), "CustomsRegistrationNumber");
		yield return (typeof(DateTime), "CustomsRegistrationDate");
	}

	protected sealed override IEnumerable<string> GetParameterNameList()
	{
		yield return "@CompanyPk";
		yield return "@CustomsRegistrationDateFrom";
		yield return "@CustomsRegistrationDateTo";
		yield return "@PaymentDateFrom";
		yield return "@PaymentDateTo";
		yield return "@DefermentAccountNumber";
		yield return "@CustomsOffice";
	}
}

#endregion
