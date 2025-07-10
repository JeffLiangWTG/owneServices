using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.ReportTesting.Testing
{
	class Report_GbDeclarationsImport_CDS : Report_GbDeclarationsExport_CDS
	{
		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				var colls = base.ExpectedColumnsInAnyOrder;
				colls.RemoveByName("OFFICEOFEXIT");
				colls.RemoveByName("CCCEVENTTIME");
				colls.RemoveByName("LCPDEPARTUREDATE");
				colls.RemoveByName("LCPINSPECTIONDATE");
				colls.RemoveByName("CDSLOCATIONOFGOODS");

				colls.Add(new ReportSchemaColumn(typeof(DateTime), "ImportCustomsCommencedDate"));

				foreach (var decimalCol in new[] { "DeclarationTotalDuty", "TOTALDUTYA30", "TOTALDUTYA35", "DeclarationtotalVAT" })
				{
					colls.Add(new ReportSchemaColumn(typeof(decimal), decimalCol));
				}
				foreach (var col in new[] { "Box48VATDeferralAccount", "Box48VATDeferralType", "Box62AirTransportCosts", "Box63FreightCharges", "Box63FreightChargesCurrency", "Box64ApportionByWeight", "Box65DiscountAmount", "Box65DiscountCurrency", "Box65DiscountPercent", "Box66Insurance", "Box66InsuranceCurrency", "Box67OtherCharges", "Box67OtherChargesCurrency", "Box68VatAdjustment", })
				{
					colls.Add(new ReportSchemaColumn(typeof(string), col));
				}

				return colls;
			}
		}

		protected override List<string> ParametersValuesList => DeclarationReportTestHelper.GetParametersValuesListImport("CDS");

		protected override void PrepareTestData()
		{
			// Make sure you make the row to be included first, so that it gets the lowest/first B-job number
			DeclarationReportTestHelper.MakeDeclaration(Factory, out var imp, isShipmentLinked: ShouldBeShipmentLinked);
			DeclarationReportTestHelper.MakeDeclaration(Factory, out var imp2, isShipmentLinked: ShouldBeShipmentLinked);

			var invHeader = imp.Invoices.AddNew();
			invHeader.ZG_TransportChargesMethodOfPayment = "Y";
			invHeader.InvoiceLines.AddNew();

			ModifyImportJob(imp);
			imp.JE_ApplicationCode = "CHF";

			ModifyImportJob(imp2);
			imp2.JE_ApplicationCode = "CDS";
		}

		protected virtual void ModifyImportJob(JobDeclaration imp)
		{
			imp.ImporterDeliveryAddress.E2_OA_Address = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "BACCRI")).MainAddress.PK;
			var entryLine1 = imp.CustomsEntryHeaders[0].MergedLines.AddNew();
			var entryLine2 = imp.CustomsEntryHeaders[1].MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate("A00", 100m);
			entryLine1.Fees.AddOrUpdate("B00", 200m);
			entryLine1.Fees.AddOrUpdate("A30", 300m);
			entryLine1.Fees.AddOrUpdate("A35", 350m);
			entryLine2.Fees.AddOrUpdate("A00", 1.00m);
			entryLine2.Fees.AddOrUpdate("B00", 2.00m);
			entryLine2.Fees.AddOrUpdate("A30", 3.00m);
			entryLine2.Fees.AddOrUpdate("A35", 3.50m);
			imp.ZG_ManualCalc = true;
			imp.JE_IATALoadPort = "SYD";
			imp.ZG_OSAirTransportAmount = 62m;
			imp.ZG_FrtChgAmt = 63m;
			imp.ZG_ApportionByWeight = true;
			imp.ZG_DiscAmt = 65.1m;
			imp.ZG_DiscPerc = 65.2m;
			imp.ZG_InsAmt = 66m;
			imp.ZG_OthChgAmt = 67m;
			imp.ZG_VATAdjAmt = 68m;
			SetCustomsClearedEvent(imp);
			imp.JE_MessageType = "IMP";
			imp.JE_TransportMode = Core.Constants.TransportModes.Air;
			imp.JE_MessageSubType = "IM ";
		}

		protected virtual void SetCustomsClearedEvent(JobDeclaration importJob)
		{
			importJob.Logs.AddNew(Events.CustomsCleared, new ZDateTimeOffset(1979, 8, 9, 9, 56, 0), null);
		}

		protected virtual bool ShouldBeShipmentLinked => false;

		protected override SqlObjectType SqlObjectType => Enterprise.ReportTesting.SqlObjectType.FunctionTable;

		protected override ZString ObjectName => "Report_GbDeclarationsImport";

		protected override void AssertTestResults(DataTable results)
		{
			AssertEquals(2, results.Rows.Count);
			var row1 = FormatRowsValues(results.Rows[0], results);
			AssertContainsMoreHelpfully("[JobNumber]='B00001001'", row1);
		}
	}

	static class ListReportSchemaColumnExtensions
	{
		public static void RemoveByName(this List<ReportSchemaColumn> list, string name)
		{
			foreach (var item in list.ToArray())
			{
				if (item.ColumnName == name)
				{
					list.Remove(item);
					break;
				}
			}
		}
	}
}
