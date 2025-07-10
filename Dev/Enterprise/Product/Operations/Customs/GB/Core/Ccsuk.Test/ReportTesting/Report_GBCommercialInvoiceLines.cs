using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ReportTesting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.ReportTesting.Testing
{
	class Report_GBCommercialInvoiceLines : Report_GBCommercialInvoiceHeaders_CDS
	{
		protected override List<string> ParametersValuesList => GetParametersValuesList(null, null);
		protected override ZString ObjectName => "Report_GBCommercialInvoiceLines";

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				var colls = base.ExpectedColumnsInAnyOrder;

				foreach (var col in new string[] {
					JobComInvoiceLine.Schema.JI_NetWeightUQ,
					JobComInvoiceLine.Schema.JI_OrderNumber,
					JobComInvoiceLine.Schema.JI_PartNo,
					JobComInvoiceLine.Schema.JI_PrimaryPreference,
					JobComInvoiceLine.Schema.JI_Procedure,
					JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport,
					JobComInvoiceLine.Schema.JI_CountryOfOrigin,
					JobComInvoiceLine.Schema.JI_Tariff,
					JobComInvoiceLine.Schema.JI_ValuationCode,
					JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty,
					CusEntryNumber.Schema.CE_EntryNum,
					CusEntryHeader.Schema.CH_BGMReference,
					JobComInvoiceLine.Schema.JI_ConcessionOrder,
					JobComInvoiceLine.Schema.JI_WeightUQ,
					JobComInvoiceLine.Schema.JI_ZZF_NKTaxType,
					"StatisticalValue",
					"ValueAdjustmentCode",
					JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty,
					JobComInvoiceLine.Schema.JI_CustomsUnitQty,
					JobComInvoiceLine.Schema.JI_Description })
				{
					colls.Add(new ReportSchemaColumn(typeof(string), col));
				}

				foreach (var col in new string[] {
					JobComInvoiceLine.Schema.JI_LinePrice,
					CusEntryLine.Schema.CL_StatisticalValue,
					CusEntryLine.Schema.CL_ValueForVAT,
					CusEntryLine.Schema.CL_CustomsValue,
					JobComInvoiceLine.Schema.JI_CustomsThirdQuantity,
					JobComInvoiceLine.Schema.JI_ValuationMarkup,
					JobComInvoiceLine.Schema.JI_Weight,
					JobComInvoiceLine.Schema.JI_CustomsQuantity,
					JobComInvoiceLine.Schema.JI_CustomsSecondQuantity,
					JobComInvoiceLine.Schema.JI_NetWeight,
					"EntryLineDutyA00",
					"EntryLineVATB00" })
				{
					colls.Add(new ReportSchemaColumn(typeof(decimal), col));
				}

				foreach (var col in new string[] {
					CusEntryLine.Schema.CL_LineNumber,
					JobComInvoiceLine.Schema.JI_LineNo, })
				{
					colls.Add(new ReportSchemaColumn(typeof(short), col));
				}

				return colls;
			}
		}

		protected override SqlObjectType SqlObjectType => SqlObjectType.FunctionTable;

		protected override void PrepareTestData()
		{
			base.PrepareTestData();

			var invoices = Factory.Load<JobComInvoiceHeader>(new ZQuery(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, SQLComparisonOperator.StartsWith, "INVNR987")).OrderBy(x => x.JZ_InvoiceNumber).ToArray();

			AssertEquals("There should be 4 invoices", 4, invoices.Length);

			var line = invoices[0].JobComInvoiceLines.AddNew();
			SetupInvoiceLine(line, 1);
			line = invoices[0].JobComInvoiceLines.AddNew();
			SetupInvoiceLine(line, 2);

			line = invoices[1].JobComInvoiceLines.AddNew();
			SetupInvoiceLine(line, 3);
			line = invoices[1].JobComInvoiceLines.AddNew();
			SetupInvoiceLine(line, 4);

			line = invoices[2].JobComInvoiceLines.AddNew();
			SetupInvoiceLine(line, 5);

			invoices[0].JobDeclaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();
		}

		void SetupInvoiceLine(JobComInvoiceLine line, int offset)
		{
			var txt = $"AB{offset}";

			line.JI_PartNo = txt;
			line.JI_Description = txt;
			line.JI_LinePrice = 100m + offset;
			line.JI_CustomsQuantity = offset;
			line.JI_CustomsUnitQty = txt;
			line.JI_CustomsSecondQuantity = offset;
			line.JI_CustomsSecondUnitQty = txt;
			line.JI_CustomsThirdQuantity = offset;
			line.JI_CustomsThirdUnitQty = txt;
			line.JI_Weight = offset;
			line.JI_WeightUQ = "KG";
			line.JI_NetWeight = offset;
			line.JI_NetWeightUQ = "KG";
			line.JI_Tariff = "123";
			line.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
			line.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.UnitedStates;
			line.JI_ConcessionOrder = txt;
			line.JI_PrimaryPreference = txt;
			line.JI_Procedure = txt;
			line.JI_ValuationCode = "A";
			line.JI_ValuationMarkup = 10m + offset;
			line.JI_ZZF_NKTaxType = txt;
			line.JI_OrderNumber = $"{offset}";

			line.ZG_ValueAdjustmentCode = "A";
			line.ZG_StatisticalValue = 20m + offset;
		}

		protected override void AssertTestResults(DataTable results)
		{
			AssertEquals("Results should have 5 rows", 5, results.Rows.Count);

			foreach (DataRow r in results.Rows)
			{
				var orderNum = r[JobComInvoiceLine.Schema.JI_OrderNumber].ToString();

				int.TryParse(orderNum, out var number);

				var formattedRow = FormatRowsValues(r, results);
				var expecetdRow = GetExpectedRow(number);

				AssertContainsMoreHelpfully(expecetdRow, formattedRow);
			}
		}

		string GetExpectedRow(int offset)
		{
			var cusLineValue = "";
			var cusLineVat = "";
			var cusLineNum = "";
			var cusEntryNum = "";
			var cusBGMRef = "";

			if (offset < 3)
			{
				cusBGMRef = "5GB1234-B1234";
				cusLineValue = offset == 1 ? "67.3300" : "68.0000";
				cusLineVat = offset == 1 ? "67.3300" : "68.0000";
				cusLineNum = $"{offset}";
				cusEntryNum = "071-111111A";
			}

			//{JobComInvoiceLine.Schema.

			var data = new List<string> {
				$"[{JobComInvoiceLine.Schema.JI_PartNo}]='AB{offset}'",
				$"[{JobComInvoiceLine.Schema.JI_Description}]='AB{offset}'",
				$"[{JobComInvoiceLine.Schema.JI_LinePrice}]='{100m + offset:N4}'",
				$"[{JobComInvoiceLine.Schema.JI_CustomsQuantity}]='{offset:N6}'",
				$"[{JobComInvoiceLine.Schema.JI_CustomsUnitQty}]='AB{offset}'",
				$"[{JobComInvoiceLine.Schema.JI_CustomsSecondQuantity}]='{offset:N6}'",
				$"[{JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty}]='AB{offset}'",
				$"[{JobComInvoiceLine.Schema.JI_CustomsThirdQuantity}]='{offset:N6}'",
				$"[{JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty}]='AB{offset}'",
				$"[{JobComInvoiceLine.Schema.JI_Weight}]='{offset:N3}'",
				$"[{JobComInvoiceLine.Schema.JI_WeightUQ}]='KG'",
				$"[{JobComInvoiceLine.Schema.JI_NetWeight}]='{offset:N3}'",
				$"[{JobComInvoiceLine.Schema.JI_NetWeightUQ}]='KG'",
				$"[{JobComInvoiceLine.Schema.JI_Tariff}]='123'",
				$"[{JobComInvoiceLine.Schema.JI_CountryOfOrigin}]='GB'",
				$"[{JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport}]='US'",
				$"[{JobComInvoiceLine.Schema.JI_ConcessionOrder}]='AB{offset}'",
				$"[{JobComInvoiceLine.Schema.JI_PrimaryPreference}]='AB{offset}'",
				$"[{JobComInvoiceLine.Schema.JI_Procedure}]='AB{offset}'",
				$"[{JobComInvoiceLine.Schema.JI_ValuationCode}]='A'",
				$"[{JobComInvoiceLine.Schema.JI_ValuationMarkup}]='{10m + offset:N3}'",
				$"[{JobComInvoiceLine.Schema.JI_ZZF_NKTaxType}]='AB{offset}'",
				$"[{JobComInvoiceLine.Schema.JI_OrderNumber}]='{offset}'",
				$"[ValueAdjustmentCode]='A'",
				$"[StatisticalValue]='{20 + offset}'",
				$"[{CusEntryHeader.Schema.CH_BGMReference}]='{cusBGMRef}'",
				$"[{CusEntryLine.Schema.CL_LineNumber}]='{cusLineNum}'",
				$"[{CusEntryLine.Schema.CL_CustomsValue}]='{cusLineValue}'",
				$"[{CusEntryLine.Schema.CL_StatisticalValue}]='{cusLineValue}'",
				$"[{CusEntryLine.Schema.CL_ValueForVAT}]='{cusLineVat}'",
				$"[{CusEntryNumber.Schema.CE_EntryNum}]='{cusEntryNum}'"
			};

			var sb = new StringBuilder();
			int counter = 0;

			foreach (var str in data)
			{
				if (0 != counter)
				{
					sb.Append("; ");
				}
				sb.Append(str);
				counter++;
			}

			return sb.ToString();
		}
	}
}
