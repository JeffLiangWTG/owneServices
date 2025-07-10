using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Reports.Testing
{
	class NoFiltersReportFRInvoicesTest : Report_FRInvoicesTest
	{
		protected override IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults()
		{
			yield return new (string, string)[]
			{
				("InvoicePK", invoice.PK.ToString()),
				("DeclarationPK", declaration.PK.ToString()),
				("InvoiceNumber", invoice.JZ_InvoiceNumber),
				("InvoiceDate", invoice.JZ_InvoiceDate.ToString("s")),
				("IncoTerm", invoice.JZ_IncoTerm),
				("IncoTermPlace", invoice.JZ_IncoTermPlace),
				("DeclarationID", declaration.JE_DeclarationReference),
				("EntryID", entryNumber.CE_EntryNum),
				("DeclarationType", entryInstruction.CEI_Style),
				("EntryStatusDateTime",new ZDateTime(2020,01,02).ToString("s")),
				("EntryStatus", "BAE"),
				("InvoiceGrossWeight", invoice.JZ_Weight.ToString("0.000")),
				("InvoiceGrossWeightUQ", invoice.JZ_WeightUQ),
				("TotalInvoiceValue",invoice.JZ_InvoiceAmount.ToString("0.0000")),
				("InvoiceCurrency", invoice.JZ_RX_NKInvoice_Currency),
				("InvoiceCurrencyExRate", invoice.JZ_InvoiceCurrExRate.ToString("0.000000000")),
				("NumberOfInvoiceLines", invoice.InvoiceLines.Count.ToString()),
				("TotalInvoiceDuty", lineFeeDuty.CF_ChargeAmount.ToString("0.0000")),
				("TotalInvoiceVAT", lineFeeVat.CF_ChargeAmount.ToString("0.0000")),
				("SupplierPK", supplier.PK.ToString()),
				("SupplierCode", supplier.OH_Code),
				("SupplierName", supplier.OH_FullName),
				("SupplierAddress", invoice.JZ_OA_SupplierAddress.ToString()),
				("ImporterCode", importer.OH_Code),
				("ImporterName", importer.OH_FullName),
				("TransportMode", declaration.JE_TransportMode),
				("DateOfDischarge", declaration.JE_DateOfArrival.ToString("s")),
				("PortOfDischarge", declaration.JE_RL_NKPortOfArrival),
				("DeltaMode", declaration.JE_DeltaMode),
				("CustomsProfile", declaration.JE_CustomsProfile),
				("BranchPK", GlbBranch.CurrentBranch.PK.ToString())
			};
		}

		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters() => Enumerable.Empty<(string, string)>();

		protected override void PrepareTestData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dtyRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.France, "DTY");
			dtyRateType.ZZR_IsPayable = true;
			helper.CreateCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.A00, dtyRateType.PK);
			var vatRateType = helper.CreateCusRateType(Core.Constants.CountryCodes.France, "VAT");
			vatRateType.ZZR_IsPayable = true;
			helper.CreateCusRateCode(Factory, "B00", vatRateType.PK);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			lineFeeDuty = entryLine.ConfirmedFees.AddNew();
			lineFeeVat = entryLine.ConfirmedFees.AddNew();
			supplier = Factory.NewWithValidTestData<OrgHeader>();
			invoice.JZ_OA_SupplierAddress = supplier.MainAddress.PK;
			importer = Factory.NewWithValidTestData<OrgHeader>();
			invoice.JZ_InvoiceNumber = "123456";
			invoice.JZ_InvoiceDate = new ZDateTime(2020, 01, 01);
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_IncoTermPlace = "A";
			declaration.JE_DeclarationReference = "B000001";
			entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = entryHeader.PK;
			entryNumber.CE_EntryIsSystemGenerated = true;
			entryNumber.CE_EntryType = declaration.JE_MessageType;
			entryNumber.CE_EntryNum = "XXYYZZ";
			entryNumber.CE_IssueDate = new ZDateTime(2020, 01, 02);
			entryInstruction.CEI_Style = "X";
			entryHeader.CH_EntryStatus = "100";
			invoice.JZ_Weight = 100m;
			invoice.JZ_WeightUQ = "KG";
			invoice.JZ_InvoiceAmount = 200m;
			invoice.JZ_RX_NKInvoice_Currency = "EUR";
			invoice.JZ_InvoiceCurrExRate = 1.01m;
			lineFeeDuty.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A00;
			lineFeeDuty.CF_ChargeAmount = 999m;
			lineFeeDuty.CF_ChargeType = "B00";
			lineFeeVat.CF_ChargeAmount = 1999m;
			invoice.JZ_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_TransportMode = "ROA";
			declaration.JE_DateOfArrival = new ZDateTime(2020, 01, 01);
			declaration.JE_RL_NKPortOfArrival = "PARIS";
			declaration.JE_DeltaMode = "X";
			declaration.JE_CustomsProfile = "ME";
		}
		OrgHeader supplier;
		OrgHeader importer;
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryNumber entryNumber;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CusEntryLineFee lineFeeDuty;
		CusEntryLineFee lineFeeVat;
	}

	#region Filter By Test Classes

	class FilterByCompanyPkReport_FRInvoicesTest : FilterByReport_FRInvoicesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@CompanyPk", GlbCompany.CurrentCompany.PK.ToString());
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = "NC";
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "NB";

			var declarationForOtherCompany = Factory.New<JobDeclaration>();
			declarationForOtherCompany.JE_GB = newBranch.PK;
			declarationForOtherCompany.Invoices.AddNew();
		}
	}

	class FilterByBranchPkReport_FRInvoicesTest : FilterByReport_FRInvoicesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@BranchPk", GlbBranch.CurrentBranch.PK.ToString());
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			var newBranchInCurrentCompany = GlbCompany.CurrentCompany.Branches.Cast<GlbBranch>().SingleOrDefault(x => x.GB_Code == "TES") ?? GlbCompany.CurrentCompany.Branches.AddNew();
			newBranchInCurrentCompany.GB_Code = "TB1";

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = "NC1";
			var branchInNewCompany = newCompany.Branches.AddNew();
			branchInNewCompany.GB_Code = "TB2";

			var declarationForBranchSameCompany = Factory.New<JobDeclaration>();
			declarationForBranchSameCompany.JE_GB = newBranchInCurrentCompany.PK;
			declarationForBranchSameCompany.Invoices.AddNew();

			var declarationForBranchNewCompany = Factory.New<JobDeclaration>();
			declarationForBranchNewCompany.JE_GB = branchInNewCompany.PK;
			declarationForBranchNewCompany.Invoices.AddNew();
		}
	}

	class FilterByImporterPkReport_FRInvoicesTest : FilterByReport_FRInvoicesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@ImporterPk", Importer.PK.ToString());
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			Declaration.JE_OH_Importer = Importer.PK;

			var otherDeclaration = Factory.New<JobDeclaration>();
			otherDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			otherDeclaration.Invoices.AddNew();
		}

		OrgHeader Importer => importer ?? (importer = Factory.NewWithValidTestData<OrgHeader>());
		OrgHeader importer;
	}

	class FilterBySupplierPkReport_FRInvoicesTest : FilterByReport_FRInvoicesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@SupplierPk", Supplier.PK.ToString());
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			Declaration.JE_OH_Supplier = Supplier.PK;
			var otherDeclaration = Factory.New<JobDeclaration>();
			otherDeclaration.JE_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;
			otherDeclaration.Invoices.AddNew();
		}

		OrgHeader Supplier => supplier ?? (supplier = Factory.NewWithValidTestData<OrgHeader>());
		OrgHeader supplier;
	}

	class FilterByInvoiceNumberReport_FRInvoicesTest : FilterByReport_FRInvoicesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@InvoiceNumber", Invoice.JZ_InvoiceNumber);
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();
			var otherDeclaration = Factory.New<JobDeclaration>();
			var otherInvoice = otherDeclaration.Invoices.AddNew();
			otherInvoice.JZ_InvoiceNumber = "123456";
		}
	}

	class FilterByJobNumberReport_FRInvoicesTest : FilterByReport_FRInvoicesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@JobNumber", Declaration.JE_DeclarationReference);
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();
			Declaration.JE_DeclarationReference = "B1";

			var otherDeclaration = Factory.New<JobDeclaration>();
			otherDeclaration.JE_DeclarationReference = "B2";
			otherDeclaration.Invoices.AddNew();
		}
	}

	class FilterByTransportModeReport_FRInvoicesTest : FilterByReport_FRInvoicesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@TransportMode", Declaration.JE_TransportMode);
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();
			Declaration.JE_TransportMode = "ROA";

			var otherDeclaration = Factory.New<JobDeclaration>();
			otherDeclaration.JE_TransportMode = "SEA";
			otherDeclaration.Invoices.AddNew();
		}
	}

	class FilterByMessageTypeReport_FRInvoicesTest : FilterByReport_FRInvoicesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@MessageType", Declaration.JE_MessageType);
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();
			Declaration.JE_MessageType = "IMP";

			var otherDeclaration = Factory.New<JobDeclaration>();
			otherDeclaration.JE_MessageType = "EXP";
			otherDeclaration.Invoices.AddNew();
		}
	}

	class FilterByCreateDateReport_FRInvoicesTest : FilterByReport_FRInvoicesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@CreateDateFrom", new ZDateTime(2020, 01, 01).ToString("s"));
			yield return ("@CreateDateTo", new ZDateTime(2020, 12, 31).ToString("s"));
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			Declaration.JE_SystemCreateTimeUtc = new ZDateTime(2020, 11, 01);

			var notInRangeDeclaration1 = Factory.New<JobDeclaration>();
			notInRangeDeclaration1.Invoices.AddNew();
			notInRangeDeclaration1.JE_SystemCreateTimeUtc = new ZDateTime(2019, 11, 01);

			var notInRangeDeclaration2 = Factory.New<JobDeclaration>();
			notInRangeDeclaration2.Invoices.AddNew();
			notInRangeDeclaration2.JE_SystemCreateTimeUtc = new ZDateTime(2021, 11, 01);
		}
	}

	class FilterByArrivalDateReport_FRInvoicesTest : FilterByReport_FRInvoicesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@ArrivalDateFrom", new ZDateTime(2020, 01, 01).ToString("s"));
			yield return ("@ArrivalDateTo", new ZDateTime(2020, 12, 31).ToString("s"));
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			Declaration.JE_DateOfArrival = new ZDateTime(2020, 11, 01);

			var notInRangeDeclaration1 = Factory.New<JobDeclaration>();
			notInRangeDeclaration1.Invoices.AddNew();
			notInRangeDeclaration1.JE_DateOfArrival = new ZDateTime(2019, 11, 01);

			var notInRangeDeclaration2 = Factory.New<JobDeclaration>();
			notInRangeDeclaration2.Invoices.AddNew();
			notInRangeDeclaration2.JE_DateOfArrival = new ZDateTime(2021, 11, 01);
		}
	}

	class FilterByPortOfDischargeReport_FRInvoicesTest : FilterByReport_FRInvoicesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@PortOfDischarge", Declaration.JE_RL_NKPortOfArrival);
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			Declaration.JE_RL_NKPortOfArrival = "ITMRS";

			var otherDeclaration = Factory.New<JobDeclaration>();
			otherDeclaration.JE_RL_NKPortOfArrival = "ITVCE";
			otherDeclaration.Invoices.AddNew();
		}
	}

	class FilterByCustomsOfficeReport_FRInvoicesTest : FilterByReport_FRInvoicesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@CustomsOffice", Declaration.JE_CustomsOffice);
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			Declaration.JE_CustomsOffice = "FR123";

			var otherDeclaration = Factory.New<JobDeclaration>();
			otherDeclaration.JE_CustomsOffice = "FR456";
			otherDeclaration.Invoices.AddNew();
		}
	}

	class FilterByDeltaReferenceReport_FRInvoicesTest : FilterByReport_FRInvoicesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@DeltaReference", DeltaReference.CE_EntryNum);
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();
			Declaration.JE_MessageType = "IMP";
			var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			DeltaReference.CE_ParentID = entryHeader.PK;
			DeltaReference.CE_ParentTable = CusEntryHeader.Schema.TableName;
			DeltaReference.CE_EntryIsSystemGenerated = true;
			DeltaReference.CE_EntryType = Declaration.JE_MessageType;
			DeltaReference.CE_EntryNum = "123456";
			var invoiceLine = Invoice.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var otherDeclaration = Factory.New<JobDeclaration>();
			otherDeclaration.CustomsEntryHeaders.AddNew();
			otherDeclaration.Invoices.AddNew();
		}
		CusEntryNumber DeltaReference => deltaReference ?? (deltaReference = Factory.New<CusEntryNumber>());
		CusEntryNumber deltaReference;
	}

	class FilterByEntryStatusReport_FRInvoicesTest : FilterByReport_FRInvoicesTest
	{
		protected override IEnumerable<(string ParameterName, string ParameterValue)> GetPopulatedFilterParameters()
		{
			yield return ("@EntryStatus", EntryHeader.CH_EntryStatus);
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();
			Declaration.JE_MessageType = "IMP";
			EntryHeader.CH_JE = Declaration.PK;
			EntryHeader.CH_EntryStatus = "100";
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = entryHeader.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNumber.CE_EntryIsSystemGenerated = true;
			entryNumber.CE_EntryType = Declaration.JE_MessageType;
			entryNumber.CE_EntryNum = "123456";
			var invoiceLine = Invoice.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var otherDeclaration = Factory.New<JobDeclaration>();
			otherDeclaration.CustomsEntryHeaders.AddNew();
			otherDeclaration.Invoices.AddNew();
		}
		CusEntryHeader EntryHeader => entryHeader ?? (entryHeader = Factory.New<CusEntryHeader>());
		CusEntryHeader entryHeader;
	}

	abstract class FilterByReport_FRInvoicesTest : Report_FRInvoicesTest
	{
		protected override IEnumerable<(string ColumnName, string Value)[]> GetOrderedExpectedResults()
		{
			yield return new (string ColumnName, string Value)[]
			{
			   ("InvoiceNumber", Invoice.JZ_InvoiceNumber)
			};
		}

		protected override void PrepareTestData()
		{
			Invoice.JZ_InvoiceNumber = "111111";
		}

		protected JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		protected JobComInvoiceHeader Invoice => invoice ?? (invoice = Declaration.Invoices.AddNew());
		JobComInvoiceHeader invoice;
	}

	#endregion

	#region Report_FRInvoicesTest

	abstract class Report_FRInvoicesTest : FRReportFunctionalTestCase
	{
		protected sealed override ZString ObjectName => "Report_FRInvoices";

		protected sealed override IEnumerable<(Type Type, string Name)> GetExpectedColumnsInAnyOrder()
		{
			yield return (typeof(Guid), "InvoicePK");
			yield return (typeof(Guid), "DeclarationPK");
			yield return (typeof(string), "InvoiceNumber");
			yield return (typeof(DateTime), "InvoiceDate");
			yield return (typeof(string), "IncoTerm");
			yield return (typeof(string), "IncoTermPlace");
			yield return (typeof(string), "DeclarationID");
			yield return (typeof(string), "EntryID");
			yield return (typeof(string), "DeclarationType");
			yield return (typeof(DateTime), "EntryStatusDateTime");
			yield return (typeof(string), "EntryStatus");
			yield return (typeof(decimal), "InvoiceGrossWeight");
			yield return (typeof(string), "InvoiceGrossWeightUQ");
			yield return (typeof(decimal), "TotalInvoiceValue");
			yield return (typeof(string), "InvoiceCurrency");
			yield return (typeof(decimal), "InvoiceCurrencyExRate");
			yield return (typeof(int), "NumberOfInvoiceLines");
			yield return (typeof(decimal), "TotalInvoiceDuty");
			yield return (typeof(decimal), "TotalInvoiceVAT");
			yield return (typeof(Guid), "SupplierPK");
			yield return (typeof(string), "SupplierCode");
			yield return (typeof(string), "SupplierName");
			yield return (typeof(Guid), "SupplierAddress");
			yield return (typeof(string), "ImporterCode");
			yield return (typeof(string), "ImporterName");
			yield return (typeof(string), "TransportMode");
			yield return (typeof(DateTime), "DateOfDischarge");
			yield return (typeof(string), "PortOfDischarge");
			yield return (typeof(string), "DeltaMode");
			yield return (typeof(string), "CustomsProfile");
			yield return (typeof(Guid), "BranchPK");
		}

		protected sealed override IEnumerable<string> GetParameterNameList()
		{
			yield return "@CompanyPk";
			yield return "@BranchPk";
			yield return "@ImporterPk";
			yield return "@SupplierPk";
			yield return "@InvoiceNumber";
			yield return "@JobNumber";
			yield return "@TransportMode";
			yield return "@MessageType";
			yield return "@CreateDateFrom";
			yield return "@CreateDateTo";
			yield return "@ArrivalDateFrom";
			yield return "@ArrivalDateTo";
			yield return "@PortOfDischarge";
			yield return "@CustomsOffice";
			yield return "@DeltaReference";
			yield return "@EntryStatus";
		}
	}

	#endregion
}
