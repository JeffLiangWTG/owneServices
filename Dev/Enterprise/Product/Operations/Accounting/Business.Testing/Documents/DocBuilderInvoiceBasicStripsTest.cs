using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	class DocBuilderInvoiceBasicStripsTest : TestCaseWithFactory
	{
		/// <summary>
		/// WI00157257 Make sure all copies of DocBuilder Invoice have the basic doc strips
		/// </summary>
		public void TestMenuItemContainsAllRequiredDocStrips()
		{
			const int customStripsIndex = 4;

			var errors = new StringBuilder();
			var missing = new List<int>();
			var custom = true;
			var whiteList = new List<string> { "Malaysia eInvoice QR Code" };
			using (var strips = DocStripsTestHelper.RunScript("Invoice", "DocBuilder Invoice", "System Document Elements"))
			{
				var mandatory = strips.AsEnumerable().Where(r => r.Field<string>(0) == "ARInvoice" && !whiteList.Contains(r.Field<string>(2))).Select(r => r.Field<string>(2)).ToList();
				string currentContext = string.Empty;
				int index = mandatory.Count;
				foreach (DataRow row in strips.Rows)
				{
					var context = row.Field<string>(0);
					if (context != currentContext)
					{
						ContextFinished(currentContext, mandatory, index, missing, custom, errors);
						missing.Clear();
						index = 0;
						currentContext = context;
					}

					var strip = row.Field<string>(2);
					if (whiteList.Contains(strip))
					{
						continue;
					}

					if (index < mandatory.Count && (strip == mandatory[index]))
					{
						++index;
					}
					else
					{
						var position = mandatory.IndexOf(strip);
						if (position >= 0)
						{
							AppendError(errors, "position", currentContext, index, strip);
							if (index > position)
							{
								missing.Remove(position);
							}
							else
							{
								do
								{
									missing.Add(index);
								} while (++index < position);
								++index;
							}
						}
						else
						{
							if (index != customStripsIndex)
							{
								AppendError(errors, "misplaced", currentContext, index, strip);
							}
						}
					}
				}
				ContextFinished(currentContext, mandatory, index, missing, custom, errors);

				Assert($"Types with missing or misplaced DocBuilder sections: {errors.ToString()}", errors.Length == 0);
			}
		}

		public void TestAllDocBuilderInvoicesHavePrintTaxDetailAsLastBEXStrips()
		{
			var expectedTaxDetailStripNames = new List<string>
			{
				"Invoice Tax Transaction in Invoice Listing - SPR",
				"Invoice Tax Transaction in Invoice Listing - TRX",
				"Invoice Tax Transaction in Invoice Listing - PER RII SLX"
			};

			var strips = RunTaxDetailDocStripsScript();

			var stripsGroupedBycontext = from strip in strips.AsEnumerable().ToList()
										 group strip.Field<string>(0) by strip.Field<string>(1) into g
										 select (context: g.Key, stripNames: g);

			foreach (var strip in stripsGroupedBycontext)
			{
				AssertContainsExactElementsInExactOrder($"Tax detail strips for {strip.context} context", expectedTaxDetailStripNames, strip.stripNames);
			}
		}

		void AppendError(StringBuilder errors, string status, string context, int index, string strip)
		{
			errors.Append($"\r\n\t{context}\t[{status}]\t{index + 1}\t{strip}");
		}

		void ContextFinished(string context, List<string> mandatory, int index, List<int> missing, bool custom, StringBuilder errors)
		{
			foreach (var i in missing)
			{
				AppendError(errors, "missing", context, i, mandatory[i]);
			}

			while (index < mandatory.Count)
			{
				AppendError(errors, "missing", context, index, mandatory[index]);
				++index;
			}
		}

		DataTable RunTaxDetailDocStripsScript()
		{
			string sql = @"SELECT S4_SectionItemName, SU_BusinessContext FROM
	(SELECT S4_SectionItemName, SU_BusinessContext,
	RANK() OVER (PARTITION BY SU_BusinessContext
				 ORDER BY SU_BusinessContext, S4_PrintOrder DESC
				) AS Rank
	FROM dbo.StmMenuDocumentConfigItem
	JOIN dbo.StmMenuDocumentConfig ON S4_S3 = S3_PK
	JOIN dbo.StmMenuTemplatePivot ON S3_SI = SI_PK
	JOIN dbo.StmMenuItem ON SI_SU = SU_PK
	JOIN dbo.StmTemplate ON SI_SO = SO_PK
	WHERE SI_DocumentTitle='Invoice' 
	AND SU_MenuName='DocBuilder Invoice'
	AND SO_Name = 'System Document Elements'
	AND S4_SectionType = 'BEX') TaxDetails
	WHERE Rank <= 3";

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}


