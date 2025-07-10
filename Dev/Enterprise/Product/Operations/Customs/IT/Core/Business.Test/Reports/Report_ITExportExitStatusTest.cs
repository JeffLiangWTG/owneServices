using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Reports.Testing;

sealed class NoFiltersReportFRInvoiceLinesTest : Report_ITExportExitStatusTest
{
	protected override IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults()
	{
		yield return new (string, string)[]
		{
			("MRN", "ITMRN"),
			("JobNumber", "DECREF"),
			("EntryStyle", "EX "),
			("Destination", "HO"),
			("AgentsReference", "RFI CIA AGENT"),
			("CustomsRegistrationNumber", "4-1234"),
			("ReleaseDate", new DateTime(1995, 03, 25).ToString("s")),
			("DeclarationType", "XXX"),
			("OfficeCode", "IT000001"),
			("OfficeDescription", "GOLDENFIELD"),
			("ExitDate", new DateTime(2021, 03, 01).ToString("s")),
			("ExitStatus", "EXT"),
			("StatusDescription", "EXT DESCR"),
			("SupplierFullName", "SUPPL FULL NAME"),
			("LocalClientFullName", "LOC CLI FULL NAME"),
			("AuthorizationNumber", "AUTH"),
			("TransportID", "TRID"),
		};
	}

	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters() => Enumerable.Empty<(string, string)>();

	protected override void PrepareTestData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Test", "IT");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT000001", "GOLDENFIELD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "Test", "IT");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "EXT", "EXT DESCR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_DeclarationReference = "DECREF";
		declaration.JE_GoodsDestination = "HO";
		declaration.JE_MessageType = "EXP";
		declaration.JE_MessageSubType = "EX";
		declaration.JE_AgentsReference = "RFI CIA AGENT";
		declaration.ZG_AuthorisationNumber = "AUTH";
		declaration.ZG_Box18TransportID = "TRID";
		var supplier = Factory.NewWithValidTestData<OrgHeader>();
		supplier.OH_FullName = "SUPPL FULL NAME";
		declaration.JE_OH_Supplier = supplier.PK;
		jobHeader = new Job.Loader(declaration).TryLoadOrCreate();
		var localClient = Factory.NewWithValidTestData<OrgHeader>();
		localClient.OH_FullName = "LOC CLI FULL NAME";
		jobHeader.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "XXX";
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.MovementReferenceNumberSetter("ITMRN");
		entryHeader.CH_EntryReleaseDate = new DateTime(1995, 03, 25);
		Factory.NewCusEntryNumber(entryHeader, "REG", "4-1234", new DateTime(2021, 03, 02));
		var ivistoEntryNum = Factory.NewCusEntryNumber(entryHeader, "IVI", "", new DateTime(2021, 03, 01), "IT000001");
		ivistoEntryNum.CE_EntryStatus = "EXT";
		ivistoEntryNum.CE_RN_NKCountryCode = "IT";
	}
	JobDeclaration declaration;
	JobHeader jobHeader;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;
}

#region Filter By Test Classes

sealed class FilterByCompanyPKReport_ITExportExitStatusTest : FilterByBaseReportITExportExitStatusTest
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@CompanyPK", GlbCompany.CurrentCompany.PK.ToString());
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		var newCompany = Factory.New<GlbCompany>();
		var newBranch = newCompany.Branches.AddNew();
		var declarationFromAnotherBranch = Factory.New<JobDeclaration>();
		declarationFromAnotherBranch.JE_GB = newBranch.PK;
		declarationFromAnotherBranch.JE_GC = newCompany.PK;
		var entryInstruction = declarationFromAnotherBranch.CustomsEntryInstructions.AddNew();
		var entryHeader = declarationFromAnotherBranch.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.MovementReferenceNumberSetter("MRN2");
		Factory.NewCusEntryNumber(entryHeader, "REG", "4-5678", null);
	}
}

sealed class FilterByAcceptanceDateReportITExportExitStatusTest : FilterByBaseReportITExportExitStatusTest
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@AcceptanceDateFrom", new ZDateTime(2021, 01, 01).ToString("s"));
		yield return ("@AcceptanceDateTo", new ZDateTime(2021, 05, 01).ToString("s"));
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		entryInstruction.CEI_DateForDuty = new ZDateTime(2021, 03, 01);

		var otherDeclaration = Factory.New<JobDeclaration>();
		var otherEntryInstruction = otherDeclaration.CustomsEntryInstructions.AddNew();
		otherEntryInstruction.CEI_DateForDuty = new ZDateTime(2021, 06, 01);
		var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
		otherEntryHeader.CH_CEI_Instruction = otherEntryInstruction.PK;
		otherEntryHeader.MovementReferenceNumberSetter("MRN2");
		Factory.NewCusEntryNumber(otherEntryHeader, "REG", "4-5678", null);
	}
}

sealed class FilterBySupplierReportITExportExitStatusTest : FilterByBaseReportITExportExitStatusTest
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@SupplierOrganisationPK", Supplier.PK.ToString());
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		declaration.JE_OH_Supplier = Supplier.PK;

		var otherDeclaration = Factory.New<JobDeclaration>();
		otherDeclaration.JE_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;
		var otherEntryInstruction = otherDeclaration.CustomsEntryInstructions.AddNew();
		var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
		otherEntryHeader.CH_CEI_Instruction = otherEntryInstruction.PK;
		otherEntryHeader.MovementReferenceNumberSetter("MRN2");
		Factory.NewCusEntryNumber(otherEntryHeader, "REG", "4-5678", null);
	}

	OrgHeader Supplier => supplier ?? (supplier = Factory.NewWithValidTestData<OrgHeader>());
	OrgHeader supplier;
}

sealed class FilterByLocalClientReportITExportExitStatusTest : FilterByBaseReportITExportExitStatusTest
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@LocalClientPK", LocalClient.PK.ToString());
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		jobHeader.JH_OA_LocalChargesAddr = LocalClient.MainAddress.PK;

		var otherDeclaration = Factory.New<JobDeclaration>();
		var otherJobHeader = new Job.Loader(otherDeclaration).TryLoadOrCreate();
		otherJobHeader.Parent = otherDeclaration;
		otherJobHeader.JH_OA_LocalChargesAddr = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
		var otherEntryInstruction = otherDeclaration.CustomsEntryInstructions.AddNew();
		var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
		otherEntryHeader.CH_CEI_Instruction = otherEntryInstruction.PK;
		otherEntryHeader.MovementReferenceNumberSetter("MRN2");
		Factory.NewCusEntryNumber(otherEntryHeader, "REG", "4-5678", null);
	}

	OrgHeader LocalClient => localClient ?? (localClient = Factory.NewWithValidTestData<OrgHeader>());
	OrgHeader localClient;
}

sealed class FilterByDeclarationTypeReportITExportExitStatusTest : FilterByBaseReportITExportExitStatusTest
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@DeclarationType", "AAA");
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		entryInstruction.CEI_Style = "AAA";

		var otherDeclaration = Factory.New<JobDeclaration>();
		var otherEntryInstruction = otherDeclaration.CustomsEntryInstructions.AddNew();
		otherEntryInstruction.CEI_Style = "BBB";
		var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
		otherEntryHeader.CH_CEI_Instruction = otherEntryInstruction.PK;
		otherEntryHeader.MovementReferenceNumberSetter("MRN2");
		Factory.NewCusEntryNumber(otherEntryHeader, "REG", "4-5678", null);
	}
}

sealed class FilterByExitStateReportITExportExitStatusTest : FilterByBaseReportITExportExitStatusTest
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@ExitState", "exit");
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		var otherDeclaration = Factory.New<JobDeclaration>();
		var otherEntryInstruction = otherDeclaration.CustomsEntryInstructions.AddNew();
		var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
		otherEntryHeader.CH_CEI_Instruction = otherEntryInstruction.PK;
		otherEntryHeader.MovementReferenceNumberSetter("MRN2");
		Factory.NewCusEntryNumber(otherEntryHeader, "REG", "4-5678", null);
	}
}

sealed class FilterByExitDateReportITExportExitStatusTest : FilterByBaseReportITExportExitStatusTest
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@ExitDateFrom", new ZDateTime(2021, 01, 01).ToString("s"));
		yield return ("@ExitDateTo", new ZDateTime(2021, 05, 01).ToString("s"));
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		var otherDeclaration = Factory.New<JobDeclaration>();
		var otherEntryInstruction = otherDeclaration.CustomsEntryInstructions.AddNew();
		otherEntryInstruction.CEI_DateForDuty = new ZDateTime(2021, 06, 01);
		var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
		otherEntryHeader.CH_CEI_Instruction = otherEntryInstruction.PK;
		otherEntryHeader.MovementReferenceNumberSetter("MRN2");
		Factory.NewCusEntryNumber(otherEntryHeader, "REG", "4-5678", null);
		Factory.NewCusEntryNumber(otherEntryHeader, "IVI", "EXT", new DateTime(2021, 06, 01));
	}
}

sealed class FilterByExportTypeReportITExportExitStatusTest : FilterByBaseReportITExportExitStatusTest
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters() => [];

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		declaration.JE_MessageType = "EXP";

		var otherDeclaration = Factory.New<JobDeclaration>();
		otherDeclaration.JE_MessageType = "COM";
		var otherEntryInstruction = otherDeclaration.CustomsEntryInstructions.AddNew();
		var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
		otherEntryHeader.CH_CEI_Instruction = otherEntryInstruction.PK;
		otherEntryHeader.MovementReferenceNumberSetter("MRN2");
		Factory.NewCusEntryNumber(otherEntryHeader, "REG", "4-5678", null);
	}
}

abstract class FilterByBaseReportITExportExitStatusTest : Report_ITExportExitStatusTest
{
	protected override IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults()
	{
		yield return new (string, string)[]
		{
			("MRN", entryHeader.MovementReferenceNumber.ToString()),
		};
	}

	protected override void PrepareTestData()
	{
		declaration = Factory.New<JobDeclaration>();
		jobHeader = new Job.Loader(declaration).TryLoadOrCreate();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.MovementReferenceNumberSetter("MRN");
		Factory.NewCusEntryNumber(entryHeader, "REG", "4-1234", null);
		Factory.NewCusEntryNumber(entryHeader, "IVI", "EXT", new DateTime(2021, 03, 01));
	}
	protected JobDeclaration declaration;
	protected JobHeader jobHeader;
	protected CusEntryInstruction entryInstruction;
	protected CusEntryHeader entryHeader;
}

#endregion

#region Report_ITExportExitStatusTest

abstract class Report_ITExportExitStatusTest : ITReportFunctionalTestCase
{
	protected sealed override ZString ObjectName => "Report_ITExportExitStatus";

	protected sealed override IEnumerable<(Type Type, string Name)> GetExpectedColumnsInAnyOrder()
	{
		yield return (typeof(string), "MRN");
		yield return (typeof(string), "JobNumber");
		yield return (typeof(string), "EntryStyle");
		yield return (typeof(string), "Destination");
		yield return (typeof(string), "AgentsReference");
		yield return (typeof(string), "CustomsRegistrationNumber");
		yield return (typeof(DateTime), "ReleaseDate");
		yield return (typeof(string), "DeclarationType");
		yield return (typeof(string), "OfficeCode");
		yield return (typeof(string), "OfficeDescription");
		yield return (typeof(DateTime), "ExitDate");
		yield return (typeof(string), "ExitStatus");
		yield return (typeof(string), "StatusDescription");
		yield return (typeof(string), "SupplierFullName");
		yield return (typeof(string), "LocalClientFullName");
		yield return (typeof(string), "AuthorizationNumber");
		yield return (typeof(string), "TransportID");
	}

	protected sealed override IEnumerable<string> GetParameterNameList()
	{
		yield return "@CompanyPK";
		yield return "@AcceptanceDateFrom";
		yield return "@AcceptanceDateTo";
		yield return "@SupplierOrganisationPK";
		yield return "@LocalClientPK";
		yield return "@DeclarationType";
		yield return "@ExitState";
		yield return "@ExitDateFrom";
		yield return "@ExitDateTo";
	}
}

#endregion
