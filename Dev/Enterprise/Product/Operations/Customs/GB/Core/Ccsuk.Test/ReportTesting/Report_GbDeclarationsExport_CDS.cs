using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.Ccsuk.ReportTesting.Testing
{
	class Report_GbDeclarationsExport_CDS : ReportFunctionalTestCase
	{
		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				// How to prepare this. 
				// Profile CW1 in SQLProfiler - include RPC:Completed event. 
				// Run the report in the GUI, selecting all the available columns.
				// Grab this hit from the profiler, and grab all the columns names from the select list.
				// User notepad++ to wrap the names in quotes.
				// Then divvy up the list into data types. 

				var allCols = new List<ReportSchemaColumn>();
				foreach (var stringCol in new string[] { "JOBNUMBER", "BOX1DECLARATIONTYPE", "BOX1ENTRYTYPE", "BOX2SUPPLIERCODE", "BOX2SUPPLIERNAME", "BOX8IMPORTERCODE", "BOX8IMPORTERNAME",
															"ALLENTRYNUMBERS", "PORTOFLOADING", "PORTOFDISCHARGE", "BOX15PORTOFORIGIN", "BOX17PORTOFDESTINATION", "MASTERUCR", "BADGE",
															"BASICORHOUSE", "BOX14DECLARANT", "BOX14REPRESENTATIONTYPE", "BOX20INCOTERM", "BOX21NATIONALITY", "BOX21VESSEL", "BOX25TRANSPORTMODE",
															"BOX26MOT", "BOX29TRANSPORTCHARGESMOP", "BOX30LOCATION", "BOX30SHED","BOX30SHEDNAME", "BOX44SUPERVISINGOFFICE", "BOX49WAREHOUSE",
															"BOX7DECLARANTSREFERENCE", "BRANCHCODE", "BRANCHNAME", "BROKERUSERCODE","BROKERUSERNAME", "CARRIER","CARRIERNAME", "CONTAINERMODE", "CONTAINERS",
															"CREATINGUSERCODE","CREATINGUSERNAME", "CSP", "CTSTATUSID", "DELIVERYCODE", "DELIVERYNAME", "DELIVERYTRANSPORTCODE",
															"DELIVERYTRANSPORTNAME", "DEPOTCODE", "DEPOTNAME","FORWARDER","FORWARDERNAME", "GOODSDESCRIPTION", "HOUSEBILL", "ICS",
															"INTERNALSTATUSCODE", "MASTERBILL", "MERGEBY", "OFFICEOFEXIT", "PICKUPCODE","PICKUPNAME", "PICKUPTRANSPORTCODE",
															"PICKUPTRANSPORTNAME", "ROUTEOFENTRY", "SERVICELEVELCODE", "SERVICELEVELNAME", "SOE", "TOTALVOLUMEUQ", "TOTALWEIGHTUQ", "VOYAGEFLIGHT",
															"CDSLOCATIONOFGOODS",
															"IRCINVENTORYRETURNCODE", "MESSAGETYPE", "Box48OtherDeferralType", JobDeclaration.Schema.JE_AddInfo, "BOX48OTHERDEFERRALACCOUNT", "BOX61FARP", JobDeclaration.Schema.JE_ApplicationCode }) // returned by export report, not shown in XLS, used by import report															})
				{
					allCols.Add(new ReportSchemaColumn(typeof(string), stringCol));
				}

				foreach (var dateCol in new string[] { "ARRIVALDATE", "CLEARANCEDATE","CREATEDTIME","DEPARTUREDATE", "ETA", "ETD", "LCPDEPARTUREDATE",
															"LCPINSPECTIONDATE",     "TAXPOINT",
															"CCCEVENTTIME"  // returned by export report, not shown in XLS, used by import report
															})
				{
					allCols.Add(new ReportSchemaColumn(typeof(DateTime), dateCol));
				}

				foreach (var decimalCol in new string[] { "ALLARINVOICESINVOICEDAMOUNT", "ALLARINVOICESOUTSTANDINGAMOUNT", "BILLEDAMOUNT", "INVOICEDAMOUNT", "OUTSTANDINGAMOUNT", "TOTALVOLUME", "TOTALWEIGHT" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(decimal), decimalCol));
				}

				foreach (var intCol in new string[] { "BOX6PACKAGES", })
				{
					allCols.Add(new ReportSchemaColumn(typeof(int), intCol));
				}

				foreach (var boolCol in new string[] { "ROUTEFREQUESTED", "ISTRAININGENTRY", })
				{
					allCols.Add(new ReportSchemaColumn(typeof(bool), boolCol));
				}

				foreach (var guidCol in new string[] { JobDeclaration.Schema.PK, JobDeclaration.Schema.JE_GB, JobDeclaration.Schema.JE_OH_Importer, JobDeclaration.Schema.JE_OH_Supplier })
				{
					allCols.Add(new ReportSchemaColumn(typeof(Guid), guidCol));
				}

				return allCols;
			}
		}

		protected override List<string> ParametersValuesList => DeclarationReportTestHelper.GetParametersValuesListExport("CDS");

		protected override void PrepareTestData()
		{
			// Make sure you make the row to be included first, so that it gets the lowest/first B-job number
			DeclarationReportTestHelper.MakeDeclaration(Factory, out var exp);
			DeclarationReportTestHelper.MakeDeclaration(Factory, out var imp);
			DeclarationReportTestHelper.MakeDeclaration(Factory, out var exp2);
			var invHeader = exp.Invoices.AddNew();
			invHeader.ZG_TransportChargesMethodOfPayment = "Y";
			invHeader.InvoiceLines.AddNew();

			exp.JE_ApplicationCode = "CHF";
			exp.Logs.AddNew(Events.CustomsCleared, new ZDateTimeOffset(1979, 8, 9, 9, 56, 0), null);
			imp.JE_MessageType = "IMP";
			imp.JE_TransportMode = Core.Constants.TransportModes.Air;
			exp2.JE_ApplicationCode = "CDS";
			exp2.Logs.AddNew(Events.CustomsCleared, new ZDateTimeOffset(1979, 8, 9, 9, 56, 0), null);
		}

		protected override SqlObjectType SqlObjectType => Enterprise.ReportTesting.SqlObjectType.FunctionTable;

		protected override ZString ObjectName => "Report_GbDeclarationsExport";

		protected override void AssertTestResults(System.Data.DataTable results)
		{
			AssertEquals(2, results.Rows.Count);
			var row1 = FormatRowsValues(results.Rows[0], results);
			AssertContainsMoreHelpfully("[JobNumber]='B00001002'", row1);
		}

		protected virtual void AssertJobNumber(string row1)
		{
			AssertContainsMoreHelpfully("[JobNumber]='B00001000'", row1);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GBCustomsDataRegistry.Instance.AllowLocationOfGoodsCalculationForNotArrivedGoods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}

	class Report_GbDeclarationsExport_AllApplicationCodes : Report_GbDeclarationsExport_CDS
	{
		protected override List<string> ParametersValuesList => DeclarationReportTestHelper.GetParametersValuesListExport();

		protected override void AssertTestResults(System.Data.DataTable results)
		{
			AssertEquals(4, results.Rows.Count);
			var row1 = FormatRowsValues(results.Rows[0], results);
			AssertContainsMoreHelpfully("[JobNumber]='B00001000'", row1);
			var row2 = FormatRowsValues(results.Rows[1], results);
			AssertContainsMoreHelpfully("[JobNumber]='B00001000'", row2);
			var row3 = FormatRowsValues(results.Rows[2], results);
			AssertContainsMoreHelpfully("[JobNumber]='B00001002'", row3);
			var row4 = FormatRowsValues(results.Rows[3], results);
			AssertContainsMoreHelpfully("[JobNumber]='B00001002'", row4);
		}
	}

	static class OrgCusCodeHelper
	{
		public static OrgCusCode FindOrAddNew(this OrgCusCodeCollection col, string typeCode, string cusCode, string country)
		{
			var result = col.OfType<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == typeCode && c.OK_CustomsRegNo == cusCode && c.OK_RN_NKCodeCountry == country) ?? col.AddNew(typeCode, cusCode, country);
			return result;
		}
	}
}
