#if DEBUG
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ReportTesting;

namespace Enterprise.Customs.AsycudaCustoms.Business.ReportTesting.Testing
{
	class Report_RiskTestCase : ReportFunctionalTestCase
	{
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public new void TestRows() => base.TestRows();

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				// How to prepare this. 
				// Profile CW1 in SQLProfiler - include RPC:Completed event. 
				// Run the report in the GUI, selecting all the available columns.
				// Grab this hit fromt he profiler, and grab all the columns names from the select list.
				// User notepad++ to wrpap the names in quotes.
				// Then divvy up the list into data types. 

				var allCols = new List<ReportSchemaColumn>();
				foreach (var stringCol in new string[]
				{
					"JE_DECLARATIONREFERENCE", "GC_CODE", "GC_NAME", "GB_CODE", "GB_BRANCHNAME", "GE_CODE", "GE_DESC",
					"JE_TRANSPORTMODE", "JE_MESSAGETYPE", "IMPORTERNAME", "IMPORTERCODE", "SUPPLIERNAME", "SUPPLIERCODE", "CUSTOMERNAME", "CUSTOMERCODE",
					"CE_ENTRYNUM", "CH_BGMREFERENCE", "CH_ENTRYSTATUS", "CEI_STYLE"
				})
				{
					allCols.Add(new ReportSchemaColumn(typeof(string), stringCol));
				}

				foreach (var dateCol in new string[] { "CH_ENTRYRELEASEDATE", "CH_BONDVALIDTODATE" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(DateTime), dateCol));
				}

				foreach (var decimalCol in new string[] { "REMAININGCUSTOMSVALUE", "REMAININGNETWEIGHT", "REMAININGCUSTOMSQUANTITY" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(decimal), decimalCol));
				}

				return allCols;
			}
		}

		protected override List<string> ParametersValuesList
		{
			get
			{
				return new List<string>()
				{
					FormattableString.Invariant($"'2021-11-01'"), // @EffectiveDate AS smalldatetime
					FormattableString.Invariant($"''"), // @EnterpriseCode AS varchar(3),
					FormattableString.Invariant($"''"), // @ServerCode AS varchar(3)
				};
			}
		}

		protected override bool AllowColumnCountMismatch => true;

		protected override void PrepareTestData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Congo, "Congo");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, "FUNCS", dataGrouping: Core.Constants.CountryCodes.Congo, allowCreationOfFuncsOrPFunc: true);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Congo, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, "CG Risk", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Namibia, "Namibia");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, "FUNCS", dataGrouping: Core.Constants.CountryCodes.Namibia, allowCreationOfFuncsOrPFunc: true);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Namibia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FUNCS, Enterprise.Customs.Universal.Constants.FunctionalityTypes.Risk, "NA Risk", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			(var branchCG, var departmentCG) = SetupCompany(1, Core.Constants.CountryCodes.Congo);
			var shipmentCG = Factory.New<ForwardingShipment>();
			var declarationCG = SetupDeclaration(Core.Constants.CountryCodes.Congo, branchCG, departmentCG, 1, shipmentCG.PK);
			var standAloneDeclarationCG = SetupDeclaration(Core.Constants.CountryCodes.Congo, branchCG, departmentCG, 2, ZGuid.Empty);

			(var branchNA, var departmentNA) = SetupCompany(2, Core.Constants.CountryCodes.Namibia);
			var shipmentNA = Factory.New<ForwardingShipment>();
			var declarationNA = SetupDeclaration(Core.Constants.CountryCodes.Namibia, branchNA, departmentNA, 3, shipmentNA.PK);
			var standAloneDeclarationNA = SetupDeclaration(Core.Constants.CountryCodes.Namibia, branchNA, departmentNA, 4, ZGuid.Empty);

			(var branchCA, var departmentCA) = SetupCompany(3, Core.Constants.CountryCodes.Canada);
			var shipmentCA = Factory.New<ForwardingShipment>();
			var declarationCA = SetupDeclaration(Core.Constants.CountryCodes.Canada, branchCA, departmentCA, 5, shipmentCA.PK);
			var standAloneDeclarationCA = SetupDeclaration(Core.Constants.CountryCodes.Canada, branchCA, departmentCA, 6, ZGuid.Empty);
		}

		(GlbBranch branch, GlbDepartment department) SetupCompany(int identifier, string countryCode)
		{
			var reference = identifier.ToString("D3");
			var company = Factory.New<GlbCompany>();
			company.GC_Code = reference;
			company.GC_Name = $"TEST COMPANY {countryCode}";
			company.GC_RN_NKCountryCode = countryCode;
			var branch = company.Branches.AddNew();
			branch.GB_Code = reference;
			branch.GB_BranchName = $"TEST BRANCH {countryCode}";
			var department = Factory.New<GlbDepartment>();
			department.GE_Code = reference;
			department.GE_Desc = $"TEST DEPARTMENT {countryCode}";
			return (branch, department);
		}

		OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = Factory.New<OrgHeader>();
					importer.OH_Code = "IMPFZ";
					importer.OH_FullName = "Test Importer";
				}
				return importer;
			}
		}
		OrgHeader importer;

		OrgHeader Supplier
		{
			get
			{
				if (supplier == null)
				{
					supplier = Factory.New<OrgHeader>();
					supplier.OH_Code = "SUPFZ";
					supplier.OH_FullName = "Test Supplier";
				}
				return supplier;
			}
		}
		OrgHeader supplier;

		OrgHeader Customer
		{
			get
			{
				if (customer == null)
				{
					customer = Factory.New<OrgHeader>();
					customer.OH_Code = "CUSFZ";
					customer.OH_FullName = "Test Customer";
				}
				return customer;
			}
		}
		OrgHeader customer;

		JobDeclaration SetupDeclaration(string countryCode, GlbBranch branch, GlbDepartment department, int identifier, ZGuid shipmentPK)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			declaration.JE_JS = shipmentPK;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			var reference = identifier.ToString("D3");
			var declarationReference = $"B{countryCode}00{reference}";
			declaration.JE_DeclarationReference = declarationReference;
			var shipment = declaration.Shipment;
			if (shipment != null)
			{
				shipment.JS_UniqueConsignRef = declarationReference;
			}
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_OH_Supplier = Supplier.PK;
			declaration.JE_OH_ControllingCustomer = Customer.PK;

			var job = new JobHeader.Loader(shipmentPK.IsEmpty ? declaration : declaration.Shipment).TryCreate();
			job.JH_GC = branch.GB_GC;
			job.JH_GB = branch.PK;
			job.JH_GE = department.PK;
			job.JH_Status = "WRK";

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "EX8";
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "EX8";

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryHeader1.CH_BGMReference = declarationReference + "/1";
			entryHeader1.CH_EntryStatus = "CLR";
			entryHeader1.CH_EntryReleaseDate = new ZDateTime(2021, 09, 10);
			entryHeader1.CH_BondValidToDate = new ZDate(2022, 01, 01);
			entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryHeader2.CH_BGMReference = declarationReference + "/2";
			entryHeader2.CH_EntryStatus = "CLR";
			entryHeader2.CH_EntryReleaseDate = new ZDateTime(2021, 09, 10);
			entryHeader2.CH_BondValidToDate = new ZDate(2022, 01, 01);
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;

			entryHeader1.EntryNumber = "A:32112345" + reference;
			entryHeader2.EntryNumber = "A:12312345" + reference;

			var entryLine1 = entryHeader1.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 100m;
			var entryLine2 = entryHeader2.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 100m;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 100m;
			invoiceLine1.JI_NetWeight = 10m;
			invoiceLine1.JI_NetWeightUQ = "KG";
			invoiceLine1.JI_CustomsQuantity = 20;
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 100m;
			invoiceLine2.JI_NetWeight = 20m;
			invoiceLine2.JI_NetWeightUQ = "KG";
			invoiceLine2.JI_CustomsQuantity = 40;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "1000";

			var riskManagement1 = entryInstruction1.RiskManagements.AddNew();
			riskManagement1.CSI_Value = 100m;
			riskManagement1.CSI_Quantity = 10m;
			riskManagement1.CSI_Quantity2 = 20m;

			var riskManagement2 = entryInstruction2.RiskManagements.AddNew();
			riskManagement2.CSI_Value = 20m;
			riskManagement2.CSI_Quantity = 3m;
			riskManagement2.CSI_Quantity2 = 3m;
			var riskManagement3 = entryInstruction2.RiskManagements.AddNew();
			riskManagement3.CSI_Value = 10m;
			riskManagement3.CSI_Quantity = 2m;
			riskManagement3.CSI_Quantity2 = 2m;
			return declaration;
		}

		protected override SqlObjectType SqlObjectType
		{
			get { return Enterprise.ReportTesting.SqlObjectType.FunctionTable; }
		}

		protected override ZString ObjectName
		{
			get { return "Report_Risk"; }
		}

		protected override void AssertTestResults(DataTable results)
		{
			AssertEquals("We should only find 4 rows", 4, results.Rows.Count);
			var columns = results.Columns.Cast<DataColumn>().Where(x => x.ColumnName != JobDeclaration.Schema.JE_SystemCreateTimeUtc).OrderBy(x => x.ColumnName).ToArray();
			AssertTestResult(results.Rows[0], columns, "CG", "001", "001");
			AssertTestResult(results.Rows[1], columns, "CG", "001", "002");
			AssertTestResult(results.Rows[2], columns, "NA", "002", "003");
			AssertTestResult(results.Rows[3], columns, "NA", "002", "004");
		}

		void AssertTestResult(DataRow row, DataColumn[] dataColumns, string countryCode, string companyIdentifier, string identifier)
		{
			var row1 = FormatRowsValues(row, dataColumns, ignoreGuid: true);
			var declarationReference = $"B{countryCode}00{identifier}";
			var builder = new ZStringBuilder();
			builder.Append($"[CE_EntryNum]='A:12312345{identifier}'");
			builder.Append("[CEI_Style]='EX8'");
			builder.Append($"[CH_BGMReference]='{declarationReference}/2'");
			builder.Append("[CH_BondValidToDate]='2022-01-01T00:00:00'");
			builder.Append("[CH_EntryReleaseDate]='2021-09-10T00:00:00'");
			builder.Append("[CH_EntryStatus]='CLR'");
			builder.Append("[CustomerCode]='CUSFZ'");
			builder.Append("[CustomerName]='Test Customer'");
			builder.Append($"[GB_BranchName]='TEST BRANCH {countryCode}'");
			builder.Append($"[GB_Code]='{companyIdentifier}'");
			builder.Append($"[GC_Code]='{companyIdentifier}'");
			builder.Append($"[GC_Name]='TEST COMPANY {countryCode}'");
			builder.Append($"[GE_Code]='{companyIdentifier}'");
			builder.Append($"[GE_Desc]='TEST DEPARTMENT {countryCode}'");
			builder.Append("[ImporterCode]='IMPFZ'");
			builder.Append("[ImporterName]='Test Importer'");
			builder.Append($"[JE_DeclarationReference]='{declarationReference}'");
			builder.Append("[JE_MessageType]='IMP'");
			builder.Append("[JE_TransportMode]='AIR'");
			builder.Append("[RemainingCustomsQuantity]='35.000000'");
			builder.Append("[RemainingCustomsValue]='70.0000'");
			builder.Append("[RemainingNetWeight]='15.000000'");
			builder.Append("[SupplierCode]='SUPFZ'");
			builder.Append("[SupplierName]='Test Supplier'");

			var expectedData = builder.ToStringWithDelimiterBetweenAppends("; ");
			AssertContainsMoreHelpfully(expectedData, row1, $"{countryCode}-{companyIdentifier}-{identifier}");
		}
	}
}
#endif
