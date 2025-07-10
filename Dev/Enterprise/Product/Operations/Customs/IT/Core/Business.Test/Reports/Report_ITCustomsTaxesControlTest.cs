using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Reports.Testing;

sealed class NoFiltersReportITCustomsTaxesControl_JobChargePartTest : Report_ITCustomsTaxesControlTest
{
	protected override IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults()
	{
		yield return new (string, string)[]
		{
			("CustomsOffice", "IT654321"),
			("JobNumber", "B000001"),
			("ImporterCode",  "IMPOHCO"),
			("LocalClientCode", "LOCLICO"),
			("A93MethodOfPayment", ""),
			("A93Amount", ""),
			("A93Number", ""),
			("A93PaymentDate", ""),
			("DefermentAccountNumber", ""),
			("CustomsRegistrationNumber", ""),
			("EntryReleaseDate", ""),
			("InvoiceNumber", accHeader.AH_TransactionNum),
			("InvoiceDate", new DateTime(2020, 01, 23).ToString("s")),
			("DebtorCode", "DEOHCO"),
			("ChargedAmount", "999.9900"),
			("DeclarationPK", declaration.PK.ToString()),
			("ImporterPK", importer.PK.ToString()),
			("LocalClientPK", localClient.PK.ToString())
		};
	}

	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters() => Enumerable.Empty<(string, string)>();

	protected override void PrepareTestData()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		declaration.JE_CustomsOffice = "IT654321";
		var jobLoader = new JobHeader.Loader(declaration);
		var jobHeader = jobLoader.TryCreate();

		declaration.JE_DeclarationReference = "B000001";

		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_EntryReleaseDate = new DateTime(2020, 01, 25);

		importer = Factory.NewWithValidTestData<OrgHeader>();
		importer.OH_FullName = "IMPORTER OH FULL NAME";
		importer.OH_Code = "IMPOHCO";
		declaration.JE_OH_Importer = importer.PK;

		debtor = Factory.NewWithValidTestData<OrgHeader>();
		debtor.OH_FullName = "DEBTOR OH FULL NAME";
		debtor.OH_Code = "DEOHCO";
		var testCreditor = Factory.NewWithValidTestData<OrgHeader>();
		testCreditor.OH_IsCreditor = true;

		accChargeCode = Factory.New<AccChargeCode>();
		accChargeCode.AC_ChargeGroup = "CDS";
		accChargeCode.AC_Code = "99";

		var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
		accHeader = Factory.NewWithValidTestData<ARInvoice>();
		var accLines1 = Factory.NewWithValidTestData<ARInvoiceLine>();
		accLines1.AL_AH = accHeader.PK;
		accLines1.AL_AG = glHeader.PK;
		accLines1.AL_LineAmount = 999.99;
		accLines1.AL_OSAmount = 999.99;
		accLines1.AL_TaxDate = new ZDate(2020, 01, 23);

		localClient = Factory.NewWithValidTestData<OrgHeader>();
		localClient.OH_FullName = "LOCAL CLIENT OH FULL NAME";
		localClient.OH_Code = "LOCLICO";

		var localClientAddress = Factory.New<OrgAddress>();
		localClientAddress.OA_RL_NKRelatedPortCode = "CNSHA";
		localClientAddress.OA_Address1 = "ADDRESS";
		localClientAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);
		localClientAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
		localClientAddress.OA_OH = localClient.PK;
		jobHeader.JH_OA_LocalChargesAddr = localClientAddress.PK;

		jobCharge = Factory.New<JobCharge>();
		jobCharge.JR_JH = declaration.Job.PK;
		jobCharge.JR_AC = accChargeCode.PK;
		jobCharge.JR_APInvoiceNum = "AKK321";
		jobCharge.JR_APInvoiceDate = new DateTime(2020, 01, 25);
		jobCharge.JR_SellTaxDate = accLines1.AL_TaxDate;
		jobCharge.JR_OH_SellAccount = debtor.PK;
		jobCharge.JR_LocalSellAmt = 999.99;

		jobCharge.JR_AL_ARLine = accLines1.PK;
		jobCharge.JR_OH_CostAccount = testCreditor.PK;

		entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = entryHeader.PK;
		entryNumber.CE_ParentTable = entryHeader.TableName;
		entryNumber.CE_EntryIsSystemGenerated = true;
		entryNumber.CE_EntryType = "REG";
		entryNumber.CE_EntryNum = "XXYYZZ";
		entryNumber.CE_IssueDate = new ZDateTime(2020, 01, 02);
		entryNumber.CE_Category = "CUS";

		Factory.Save();
	}

	OrgHeader importer;
	OrgHeader localClient;
	OrgHeader debtor;
	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;
	CusEntryNumber entryNumber;
	JobCharge jobCharge;
	AccChargeCode accChargeCode;
	ARInvoice accHeader;
}

sealed class NoFiltersReportITCustomsTaxesControl_EntryPayInfoPartTest : Report_ITCustomsTaxesControlTest
{
	protected override IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults()
	{
		yield return new (string, string)[]
		{
			("CustomsOffice", "IT654321"),
			("JobNumber", "B000001"),
			("ImporterCode",  "IMPOHCO"),
			("LocalClientCode", "LOCLICO"),
			("A93MethodOfPayment", "F"),
			("A93Amount", "777.7700"),
			("A93Number", "KKKK"),
			("A93PaymentDate", new DateTime(2020, 01, 25).ToString("s")),
			("DefermentAccountNumber", "555555H"),
			("CustomsRegistrationNumber", "XXYYZZ"),
			("EntryReleaseDate", new DateTime(2020, 01, 25).ToString("s")),
			("InvoiceNumber", ""),
			("InvoiceDate", ""),
			("DebtorCode", ""),
			("ChargedAmount", ""),
			("DeclarationPK", declaration.PK.ToString()),
			("ImporterPK", importer.PK.ToString()),
			("LocalClientPK", localClient.PK.ToString())
		};
	}

	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters() => Enumerable.Empty<(string, string)>();

	protected override void PrepareTestData()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		declaration.JE_CustomsOffice = "IT654321";
		var jobLoader = new JobHeader.Loader(declaration);
		var jobHeader = jobLoader.TryCreate();

		declaration.JE_DeclarationReference = "B000001";

		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_EntryReleaseDate = new DateTime(2020, 01, 25);

		payInfo = entryHeader.EntryPayInfos.AddNew();
		payInfo.C9_PaymentParty = "F";
		payInfo.C9_PaymentAmount = 777.77;
		payInfo.C9_IncomingPayResponseNo = "KKKK";
		payInfo.C9_PaymentDate = new DateTime(2020, 01, 25);

		importer = Factory.NewWithValidTestData<OrgHeader>();
		importer.OH_FullName = "IMPORTER OH FULL NAME";
		importer.OH_Code = "IMPOHCO";
		declaration.JE_OH_Importer = importer.PK;

		debtor = Factory.NewWithValidTestData<OrgHeader>();
		debtor.OH_FullName = "DEBTOR OH FULL NAME";
		debtor.OH_Code = "DEOHCO";
		var testCreditor = Factory.NewWithValidTestData<OrgHeader>();
		testCreditor.OH_IsCreditor = true;

		localClient = Factory.NewWithValidTestData<OrgHeader>();
		localClient.OH_FullName = "LOCAL CLIENT OH FULL NAME";
		localClient.OH_Code = "LOCLICO";

		var localClientAddress = Factory.New<OrgAddress>();
		localClientAddress.OA_RL_NKRelatedPortCode = "CNSHA";
		localClientAddress.OA_Address1 = "ADDRESS";
		localClientAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);
		localClientAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
		localClientAddress.OA_OH = localClient.PK;
		jobHeader.JH_OA_LocalChargesAddr = localClientAddress.PK;

		entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = entryHeader.PK;
		entryNumber.CE_ParentTable = entryHeader.TableName;
		entryNumber.CE_EntryIsSystemGenerated = true;
		entryNumber.CE_EntryType = "REG";
		entryNumber.CE_EntryNum = "XXYYZZ";
		entryNumber.CE_IssueDate = new ZDateTime(2020, 01, 02);
		entryNumber.CE_Category = "CUS";

		declaration.JE_DefermentAccountNumber = "555555H";

		Factory.Save();
	}

	OrgHeader importer;
	OrgHeader localClient;
	OrgHeader debtor;
	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;
	CusEntryNumber entryNumber;
	CusEntryPayInfo payInfo;
}

#region Filter By Test Classes EntryPayInfo Part

sealed class FilterByCompanyPkReportITCustomsTaxesControl_EntryPayInfoPart_Test : FilterByBaseReportITCustomsTaxesControlTest_EntryPayInfoPart
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
	}
}

sealed class FilterByCustomsReleaseDateReportITCustomsTaxesControl_EntryPayInfoPart_Test : FilterByBaseReportITCustomsTaxesControlTest_EntryPayInfoPart
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@CustomsReleaseDateFrom", new ZDateTime(2020, 01, 01).ToString("s"));
		yield return ("@CustomsReleaseDateTo", new ZDateTime(2021, 12, 31).ToString("s"));
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		var otherDeclaration = Factory.New<JobDeclaration>();
		otherDeclaration.JE_MessageType = "IMP";
		otherDeclaration.JE_ApplicationCode = "BLT";
		otherDeclaration.JE_CustomsOffice = "IT654321";
		otherDeclaration.JE_DeclarationReference = "B000002";

		var otherEntryInstruction = otherDeclaration.CustomsEntryInstructions.AddNew();
		var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
		otherEntryHeader.CH_CEI_Instruction = otherEntryInstruction.PK;
		otherEntryHeader.CH_EntryReleaseDate = new DateTime(2018, 01, 25);
	}
}

sealed class FilterByPaymentDateReportITCustomsTaxesControl_EntryPayInfoPart_Test : FilterByBaseReportITCustomsTaxesControlTest_EntryPayInfoPart
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@PaymentDateFrom", new ZDateTime(2020, 01, 01).ToString("s"));
		yield return ("@PaymentDateTo", new ZDateTime(2021, 12, 31).ToString("s"));
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		var otherDeclaration = Factory.New<JobDeclaration>();
		otherDeclaration.JE_MessageType = "IMP";
		otherDeclaration.JE_ApplicationCode = "BLT";
		otherDeclaration.JE_CustomsOffice = "IT654321";
		otherDeclaration.JE_DeclarationReference = "B000002";

		var otherEntryInstruction = otherDeclaration.CustomsEntryInstructions.AddNew();
		var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
		otherEntryHeader.CH_CEI_Instruction = otherEntryInstruction.PK;

		var payInfo = otherEntryHeader.EntryPayInfos.AddNew();
		payInfo.C9_PaymentParty = "F";
		payInfo.C9_PaymentAmount = 777.77;
		payInfo.C9_IncomingPayResponseNo = "KKKK";
		payInfo.C9_PaymentDate = new DateTime(2017, 03, 25);
	}
}

sealed class FilterByDefermentAccountNumberReportITCustomsTaxesControl_EntryPayInfoPart_Test : FilterByBaseReportITCustomsTaxesControlTest_EntryPayInfoPart
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@DefermentAccountNumber", "555555H");
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		var otherDeclaration = Factory.New<JobDeclaration>();
		otherDeclaration.JE_MessageType = "IMP";
		otherDeclaration.JE_ApplicationCode = "BLT";
		otherDeclaration.JE_CustomsOffice = "IT654321";
		otherDeclaration.JE_DeclarationReference = "B000002";

		otherDeclaration.JE_DefermentAccountNumber = "999999H";
	}
}

sealed class FilterByCustomsOfficeReportITCustomsTaxesControl_EntryPayInfoPart_Test : FilterByBaseReportITCustomsTaxesControlTest_EntryPayInfoPart
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@CustomsOffice", "IT654321");
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		var otherDeclaration = Factory.New<JobDeclaration>();
		otherDeclaration.JE_MessageType = "IMP";
		otherDeclaration.JE_ApplicationCode = "BLT";
		otherDeclaration.JE_CustomsOffice = "IT888888";
	}
}

sealed class FilterByMethodOfPaymentReportITCustomsTaxesControl_EntryPayInfoPart_Test : FilterByBaseReportITCustomsTaxesControlTest_EntryPayInfoPart
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@A93MethodOfPayment", "F,L");
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		var otherDeclaration = Factory.New<JobDeclaration>();
		otherDeclaration.JE_MessageType = "IMP";
		otherDeclaration.JE_ApplicationCode = "BLT";
		otherDeclaration.JE_CustomsOffice = "IT888888";

		otherDeclaration.JE_DeclarationReference = "B000002";

		var otherEntryInstruction = otherDeclaration.CustomsEntryInstructions.AddNew();
		var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
		otherEntryHeader.CH_CEI_Instruction = otherEntryInstruction.PK;

		var payInfo = otherEntryHeader.EntryPayInfos.AddNew();
		payInfo.C9_PaymentParty = "G";
		payInfo.C9_PaymentAmount = 111.11;
		payInfo.C9_IncomingPayResponseNo = "TTTT";
		payInfo.C9_PaymentDate = new DateTime(2020, 03, 25);
	}
}

sealed class FilterByImporterReportITCustomsTaxesControl_EntryPayInfoPart_Test : FilterByBaseReportITCustomsTaxesControlTest_EntryPayInfoPart
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@ImporterPk", Importer.PK.ToString());
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		Importer.OH_FullName = "IMPORTER OH FULL NAME";
		Importer.OH_Code = "IMPORT";
		declaration.JE_OH_Importer = Importer.PK;

		var otherDeclaration = Factory.New<JobDeclaration>();
		otherDeclaration.JE_MessageType = "IMP";
		otherDeclaration.JE_ApplicationCode = "BLT";
		otherDeclaration.JE_CustomsOffice = "IT888888";

		var otherImporter = Factory.NewWithValidTestData<OrgHeader>();
		otherImporter.OH_FullName = "OTHER IMPORTER OH FULL NAME";
		otherImporter.OH_Code = "OTHER";
		otherDeclaration.JE_OH_Importer = otherImporter.PK;
	}

	OrgHeader Importer => importer ?? (importer = Factory.NewWithValidTestData<OrgHeader>());
	OrgHeader importer;
}

sealed class FilterByLocalClientReportITCustomsTaxesControl_EntryPayInfoPart_Test : FilterByBaseReportITCustomsTaxesControlTest_EntryPayInfoPart
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

		var otherLocalClient = Factory.NewWithValidTestData<OrgHeader>();
		otherLocalClient.OH_FullName = "OTHER LOCAL CLIENT OH FULL NAME";
		otherLocalClient.OH_Code = "OTHERLC";
	}

	OrgHeader LocalClient => localClient ?? (localClient = Factory.NewWithValidTestData<OrgHeader>());
	OrgHeader localClient;
}

#endregion

#region Filter By Test Classes JobCharge Part

sealed class FilterByCompanyPkReportITCustomsTaxesControl_JobChargePart_Test : FilterByBaseReportITCustomsTaxesControlTest_JobChargePart
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
	}
}

sealed class FilterByCustomsReleaseDateReportITCustomsTaxesControl_JobChargePart_Test : FilterByBaseReportITCustomsTaxesControlTest_JobChargePart
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@CustomsReleaseDateFrom", new ZDateTime(2020, 01, 01).ToString("s"));
		yield return ("@CustomsReleaseDateTo", new ZDateTime(2021, 12, 31).ToString("s"));
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		var otherDeclaration = Factory.New<JobDeclaration>();
		otherDeclaration.JE_MessageType = "IMP";
		otherDeclaration.JE_ApplicationCode = "BLT";
		otherDeclaration.JE_CustomsOffice = "IT654321";
		otherDeclaration.JE_DeclarationReference = "B000002";

		var otherEntryInstruction = otherDeclaration.CustomsEntryInstructions.AddNew();
		var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
		otherEntryHeader.CH_CEI_Instruction = otherEntryInstruction.PK;
		otherEntryHeader.CH_EntryReleaseDate = new DateTime(2018, 01, 25);
	}
}

sealed class FilterByPaymentDateReportITCustomsTaxesControl_JobChargePart_Test : FilterByBaseReportITCustomsTaxesControlTest_JobChargePart
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@PaymentDateFrom", "");
		yield return ("@PaymentDateTo", "");
	}
}

sealed class FilterByDefermentAccountNumberReportITCustomsTaxesControl_JobChargePart_Test : FilterByBaseReportITCustomsTaxesControlTest_JobChargePart
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@DefermentAccountNumber", "");
	}
}

sealed class FilterByCustomsOfficeReportITCustomsTaxesControl_JobChargePart_Test : FilterByBaseReportITCustomsTaxesControlTest_JobChargePart
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@CustomsOffice", "IT654321");
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		var otherDeclaration = Factory.New<JobDeclaration>();
		otherDeclaration.JE_MessageType = "IMP";
		otherDeclaration.JE_ApplicationCode = "BLT";
		otherDeclaration.JE_CustomsOffice = "IT888888";
	}
}

sealed class FilterByMethodOfPaymentReportITCustomsTaxesControl_JobChargePart_Test : FilterByBaseReportITCustomsTaxesControlTest_JobChargePart
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@A93MethodOfPayment", "");
	}
}

sealed class FilterByImporterReportITCustomsTaxesControl_JobChargePart_Test : FilterByBaseReportITCustomsTaxesControlTest_JobChargePart
{
	protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
	{
		yield return ("@ImporterPk", Importer.PK.ToString());
	}

	protected override void PrepareTestData()
	{
		base.PrepareTestData();

		Importer.OH_FullName = "IMPORTER OH FULL NAME";
		Importer.OH_Code = "IMPORT";
		declaration.JE_OH_Importer = Importer.PK;

		var otherDeclaration = Factory.New<JobDeclaration>();
		otherDeclaration.JE_MessageType = "IMP";
		otherDeclaration.JE_ApplicationCode = "BLT";
		otherDeclaration.JE_CustomsOffice = "IT888888";

		var otherImporter = Factory.NewWithValidTestData<OrgHeader>();
		otherImporter.OH_FullName = "OTHER IMPORTER OH FULL NAME";
		otherImporter.OH_Code = "OTHER";
		otherDeclaration.JE_OH_Importer = otherImporter.PK;
	}

	OrgHeader Importer => importer ?? (importer = Factory.NewWithValidTestData<OrgHeader>());
	OrgHeader importer;
}

sealed class FilterByLocalClientReportITCustomsTaxesControl_JobChargePart_Test : FilterByBaseReportITCustomsTaxesControlTest_JobChargePart
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

		var accChargeCode = Factory.New<AccChargeCode>();
		accChargeCode.AC_ChargeGroup = "CDS";
		accChargeCode.AC_Code = "98";

		var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
		var accHeader1 = Factory.NewWithValidTestData<ARInvoice>();
		accHeader1.AH_TransactionNum = "INV0001";
		var accLines1 = Factory.NewWithValidTestData<ARInvoiceLine>();
		accLines1.AL_AH = accHeader1.PK;
		accLines1.AL_AG = glHeader.PK;
		accLines1.AL_LineAmount = 999.99;
		accLines1.AL_OSAmount = 999.99;

		var otherLocalClient = Factory.NewWithValidTestData<OrgHeader>();
		otherLocalClient.OH_FullName = "OTHER LOCAL CLIENT OH FULL NAME";
		otherLocalClient.OH_Code = "OTHERLC";

		var debtor = Factory.NewWithValidTestData<OrgHeader>();
		debtor.OH_FullName = "OTHER DEBTOR OH FULL NAME";
		debtor.OH_Code = "OTHERDE";
		var testCreditor = Factory.NewWithValidTestData<OrgHeader>();
		testCreditor.OH_IsCreditor = true;

		var jobCharge = Factory.New<JobCharge>();
		jobCharge.JR_JH = otherDeclaration.Job.PK;
		jobCharge.JR_AC = accChargeCode.PK;
		jobCharge.JR_APInvoiceNum = "OTHER";
		jobCharge.JR_APInvoiceDate = new DateTime(2020, 01, 25);
		jobCharge.JR_OH_SellAccount = debtor.PK;
		jobCharge.JR_LocalSellAmt = 999.99;

		jobCharge.JR_AL_ARLine = accLines1.PK;
		jobCharge.JR_OH_CostAccount = testCreditor.PK;
	}

	OrgHeader LocalClient => localClient ?? (localClient = Factory.NewWithValidTestData<OrgHeader>());
	OrgHeader localClient;
}

#endregion

#region Report_ITCustomsTaxesControlTest

abstract class Report_ITCustomsTaxesControlTest : ITReportFunctionalTestCase
{
	protected sealed override ZString ObjectName => "Report_ITCustomsEntryPayment";

	protected sealed override IEnumerable<(Type Type, string Name)> GetExpectedColumnsInAnyOrder()
	{
		yield return (typeof(string), "CustomsOffice");
		yield return (typeof(string), "JobNumber");
		yield return (typeof(string), "ImporterCode");
		yield return (typeof(string), "LocalClientCode");
		yield return (typeof(string), "A93MethodOfPayment");
		yield return (typeof(decimal), "A93Amount");
		yield return (typeof(string), "A93Number");
		yield return (typeof(DateTime), "A93PaymentDate");
		yield return (typeof(string), "DefermentAccountNumber");
		yield return (typeof(string), "CustomsRegistrationNumber");
		yield return (typeof(DateTime), "EntryReleaseDate");
		yield return (typeof(string), "InvoiceNumber");
		yield return (typeof(DateTime), "InvoiceDate");
		yield return (typeof(string), "DebtorCode");
		yield return (typeof(decimal), "ChargedAmount");
		yield return (typeof(Guid), "DeclarationPK");
		yield return (typeof(Guid), "ImporterPK");
		yield return (typeof(Guid), "LocalClientPK");
	}

	protected sealed override IEnumerable<string> GetParameterNameList()
	{
		yield return "@CompanyPk";
		yield return "@CustomsReleaseDateFrom";
		yield return "@CustomsReleaseDateTo";
		yield return "@PaymentDateFrom";
		yield return "@PaymentDateTo";
		yield return "@DefermentAccountNumber";
		yield return "@CustomsOffice";
		yield return "@MethodOfPaymentList";
		yield return "@ImporterPk";
		yield return "@LocalClientPK";
	}
}

abstract class FilterByBaseReportITCustomsTaxesControlTest_EntryPayInfoPart : Report_ITCustomsTaxesControlTest
{
	protected override IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults()
	{
		yield return new (string ColumnName, string Value)[]
		{
		   ("JE_PK", declaration.PK.ToString())
		};
	}

	protected override void PrepareTestData()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		declaration.JE_CustomsOffice = "IT654321";
		var jobLoader = new JobHeader.Loader(declaration);
		jobHeader = jobLoader.TryCreate();

		declaration.JE_DeclarationReference = "B000001";

		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_EntryReleaseDate = new DateTime(2020, 01, 25);

		payInfo = entryHeader.EntryPayInfos.AddNew();
		payInfo.C9_PaymentParty = "F";
		payInfo.C9_PaymentAmount = 777.77;
		payInfo.C9_IncomingPayResponseNo = "KKKK";
		payInfo.C9_PaymentDate = new DateTime(2020, 03, 25);

		importer = Factory.NewWithValidTestData<OrgHeader>();
		importer.OH_FullName = "IMPORTER OH FULL NAME";
		importer.OH_Code = "IMPOHCO";
		declaration.JE_OH_Importer = importer.PK;

		debtor = Factory.NewWithValidTestData<OrgHeader>();
		debtor.OH_FullName = "DEBTOR OH FULL NAME";
		debtor.OH_Code = "DEOHCO";
		var testCreditor = Factory.NewWithValidTestData<OrgHeader>();
		testCreditor.OH_IsCreditor = true;

		localClient = Factory.NewWithValidTestData<OrgHeader>();
		localClient.OH_FullName = "LOCAL CLIENT OH FULL NAME";
		localClient.OH_Code = "LOCLICO";

		var localClientAddress = Factory.New<OrgAddress>();
		localClientAddress.OA_RL_NKRelatedPortCode = "CNSHA";
		localClientAddress.OA_Address1 = "ADDRESS";
		localClientAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);
		localClientAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
		localClientAddress.OA_OH = localClient.PK;
		jobHeader.JH_OA_LocalChargesAddr = localClientAddress.PK;

		entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = entryHeader.PK;
		entryNumber.CE_ParentTable = entryHeader.TableName;
		entryNumber.CE_EntryIsSystemGenerated = true;
		entryNumber.CE_EntryType = "REG";
		entryNumber.CE_EntryNum = "XXYYZZ";
		entryNumber.CE_IssueDate = new ZDateTime(2020, 01, 02);
		entryNumber.CE_Category = "CUS";

		declaration.JE_DefermentAccountNumber = "555555H";

		Factory.Save();
	}

	OrgHeader importer;
	OrgHeader localClient;
	OrgHeader debtor;
	protected JobHeader jobHeader;
	protected JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	protected CusEntryHeader entryHeader;
	CusEntryNumber entryNumber;
	CusEntryPayInfo payInfo;
}

abstract class FilterByBaseReportITCustomsTaxesControlTest_JobChargePart : Report_ITCustomsTaxesControlTest
{
	protected override IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults()
	{
		yield return new (string ColumnName, string Value)[]
		{
		   ("JE_PK", declaration.PK.ToString())
		};
	}

	protected override void PrepareTestData()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		declaration.JE_CustomsOffice = "IT654321";
		var jobLoader = new JobHeader.Loader(declaration);
		jobHeader = jobLoader.TryCreate();

		declaration.JE_DeclarationReference = "B000001";

		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_EntryReleaseDate = new DateTime(2020, 01, 25);

		importer = Factory.NewWithValidTestData<OrgHeader>();
		importer.OH_FullName = "IMPORTER OH FULL NAME";
		importer.OH_Code = "IMPOHCO";
		declaration.JE_OH_Importer = importer.PK;

		debtor = Factory.NewWithValidTestData<OrgHeader>();
		debtor.OH_FullName = "DEBTOR OH FULL NAME";
		debtor.OH_Code = "DEOHCO";
		var testCreditor = Factory.NewWithValidTestData<OrgHeader>();
		testCreditor.OH_IsCreditor = true;

		accChargeCode = Factory.New<AccChargeCode>();
		accChargeCode.AC_ChargeGroup = "CDS";
		accChargeCode.AC_Code = "99";

		var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
		var accHeader1 = Factory.NewWithValidTestData<ARInvoice>();
		accHeader1.AH_TransactionNum = "INV0001";
		var accLines1 = Factory.NewWithValidTestData<ARInvoiceLine>();
		accLines1.AL_AH = accHeader1.PK;
		accLines1.AL_AG = glHeader.PK;
		accLines1.AL_LineAmount = 999.99;
		accLines1.AL_OSAmount = 999.99;

		localClient = Factory.NewWithValidTestData<OrgHeader>();
		localClient.OH_FullName = "LOCAL CLIENT OH FULL NAME";
		localClient.OH_Code = "LOCLICO";

		var localClientAddress = Factory.New<OrgAddress>();
		localClientAddress.OA_RL_NKRelatedPortCode = "CNSHA";
		localClientAddress.OA_Address1 = "ADDRESS";
		localClientAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.AWB);
		localClientAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);
		localClientAddress.OA_OH = localClient.PK;
		jobHeader.JH_OA_LocalChargesAddr = localClientAddress.PK;

		jobCharge = Factory.New<JobCharge>();
		jobCharge.JR_JH = declaration.Job.PK;
		jobCharge.JR_AC = accChargeCode.PK;
		jobCharge.JR_APInvoiceNum = "AKK321";
		jobCharge.JR_APInvoiceDate = new DateTime(2020, 01, 25);
		jobCharge.JR_OH_SellAccount = debtor.PK;
		jobCharge.JR_LocalSellAmt = 999.99;

		jobCharge.JR_AL_ARLine = accLines1.PK;
		jobCharge.JR_OH_CostAccount = testCreditor.PK;

		entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = entryHeader.PK;
		entryNumber.CE_ParentTable = entryHeader.TableName;
		entryNumber.CE_EntryIsSystemGenerated = true;
		entryNumber.CE_EntryType = "REG";
		entryNumber.CE_EntryNum = "XXYYZZ";
		entryNumber.CE_IssueDate = new ZDateTime(2020, 01, 02);
		entryNumber.CE_Category = "CUS";

		Factory.Save();
	}

	OrgHeader importer;
	OrgHeader localClient;
	OrgHeader debtor;
	protected JobDeclaration declaration;
	protected JobHeader jobHeader;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;
	CusEntryNumber entryNumber;
	JobCharge jobCharge;
	AccChargeCode accChargeCode;
}

#endregion
