using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class InvoiceLineViewCollectionFetchStrategyTest : TestCaseWithFactory
	{
		public void TestSupplementaryCodeFetchHints()
		{
			var declaration = Factory.New<JobDeclaration>();
			for (var i = 1; i < 6; i++)
			{
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "INV" + i.ToString();
				var addSup1 = i % 2 == 0;
				var addSup2 = i % 2 == 1;
				for (var l = 1; l < 5; l++)
				{
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					if (l != 3)
					{
						var addBoth = l % 2 == 0;
						var key = i.ToString() + l.ToString();
						if (addSup1 || addBoth)
						{
							invoiceLine.JI_SupplementaryCode1 = key + "1";
						}
						if (addSup2 || addBoth)
						{
							invoiceLine.JI_SupplementaryCode2 = key + "2";
						}
					}
				}
			}
			Factory.Save();
			CombineAssertions(() =>
			{
				var ignoreStackTraceBeforeThis = System.Environment.StackTrace.SplitByLine().Last();
				var newFactory = new BusinessObjectFactory();
				var decInNewFactory = newFactory.Load<JobDeclaration>(declaration.PK);
				var invoiceLines = decInNewFactory.FilteredInvoiceLines.OfType<JobComInvoiceLine>().ToList();
				AssertEquals("1Active Fetch Hints for CusCodeData", 0, newFactory.ActiveFetchHintsForTable(CusCodeDataSchema.Constants.TableName));
				AssertEquals("2CusCodeData Hits", 0, newFactory.GetTableHitCount(CusCodeDataSchema.Constants.TableName));
				invoiceLines.ForEach(x =>
				{
					_ = x.JI_SupplementaryCode1;
				});
				AssertEquals("3Active Fetch Hints for CusCodeData", 0, newFactory.ActiveFetchHintsForTable(CusCodeDataSchema.Constants.TableName));
				AssertEquals("4CusCodeData Hits", 20, newFactory.GetTableHitCount(CusCodeDataSchema.Constants.TableName));
				invoiceLines.ForEach(x =>
				{
					_ = x.JI_SupplementaryCode2;
				});
				AssertEquals("5Active Fetch Hints for CusCodeData", 0, newFactory.ActiveFetchHintsForTable(CusCodeDataSchema.Constants.TableName));
				AssertEquals("6CusCodeData Hits", 20, newFactory.GetTableHitCount(CusCodeDataSchema.Constants.TableName));

				newFactory = new BusinessObjectFactory();
				decInNewFactory = newFactory.Load<JobDeclaration>(declaration.PK);
				invoiceLines = decInNewFactory.FilteredInvoiceLines.OfType<JobComInvoiceLine>().ToList();
				AssertEquals("7Active Fetch Hints for CusCodeData", 0, newFactory.ActiveFetchHintsForTable(CusCodeDataSchema.Constants.TableName));
				AssertEquals("8CusCodeData Hits", 0, newFactory.GetTableHitCount(CusCodeDataSchema.Constants.TableName));
				decInNewFactory.FilteredInvoiceLines.FetchStrategy.FetchForView(invoiceLines.ToArray(), new[] { new TableColumn(CusCodeDataSchema.Constants.TableName, JobComInvoiceLine.Schema.JI_SupplementaryCode1) });
				AssertEquals("9Active Fetch Hints for CusCodeData", 20, newFactory.ActiveFetchHintsForTable(CusCodeDataSchema.Constants.TableName));
				AssertEquals("10CusCodeData Hits", 0, newFactory.GetTableHitCount(CusCodeDataSchema.Constants.TableName));
				invoiceLines.ForEach(x =>
				{
					_ = x.JI_SupplementaryCode1;
				});
				AssertEquals("11Active Fetch Hints for CusCodeData", 0, newFactory.ActiveFetchHintsForTable(CusCodeDataSchema.Constants.TableName));
				AssertEquals("12CusCodeData Hits", 1, newFactory.GetTableHitCount(CusCodeDataSchema.Constants.TableName));
				invoiceLines.ForEach(x =>
				{
					_ = x.JI_SupplementaryCode2;
				});
				AssertEquals("13Active Fetch Hints for CusCodeData", 0, newFactory.ActiveFetchHintsForTable(CusCodeDataSchema.Constants.TableName));
				AssertEquals("14CusCodeData Hits", 1, newFactory.GetTableHitCount(CusCodeDataSchema.Constants.TableName));

				newFactory = new BusinessObjectFactory();
				decInNewFactory = newFactory.Load<JobDeclaration>(declaration.PK);
				invoiceLines = decInNewFactory.FilteredInvoiceLines.OfType<JobComInvoiceLine>().ToList();
				AssertEquals("15Active Fetch Hints for CusCodeData", 0, newFactory.ActiveFetchHintsForTable(CusCodeDataSchema.Constants.TableName));
				AssertEquals("16CusCodeData Hits", 0, newFactory.GetTableHitCount(CusCodeDataSchema.Constants.TableName));
				decInNewFactory.FilteredInvoiceLines.FetchStrategy.FetchForView(invoiceLines.ToArray(), new[]
				{
					new TableColumn(CusCodeDataSchema.Constants.TableName, JobComInvoiceLine.Schema.JI_SupplementaryCode1),
					new TableColumn(CusCodeDataSchema.Constants.TableName, JobComInvoiceLine.Schema.JI_SupplementaryCode2)
				});
				AssertEquals("17Active Fetch Hints for CusCodeData", 20, newFactory.ActiveFetchHintsForTable(CusCodeDataSchema.Constants.TableName));
				AssertEquals("18CusCodeData Hits", 0, newFactory.GetTableHitCount(CusCodeDataSchema.Constants.TableName));
				invoiceLines.ForEach(x =>
				{
					_ = x.JI_SupplementaryCode1;
					_ = x.JI_SupplementaryCode2;
				});
				AssertEquals("19Active Fetch Hints for CusCodeData", 0, newFactory.ActiveFetchHintsForTable(CusCodeDataSchema.Constants.TableName));
				AssertEquals("20CusCodeData Hits", 1, newFactory.GetTableHitCount(CusCodeDataSchema.Constants.TableName));
			});
		}
	}
}
