using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocStatementSummary : DocumentWrapper, IGenericTransactionHeaderPlugIn
	{
		DocStatementSummary(PrintSummary printSummary, BusinessObjectFactory factoryToWrap)
			: base(printSummary, factoryToWrap)
		{
			this.PrintSummary = printSummary;
		}

		readonly PrintSummary PrintSummary;

		public static DocStatementSummary New(PrintSummary printSummary, BusinessObjectFactory factoryToWrap)
		{
			if (printSummary == null)
			{
				return null;
			}
			else
			{
				return factoryToWrap.GetCachedValue(printSummary.PK.ToStringKey(), delegate
				{ return new DocStatementSummary(printSummary, factoryToWrap); }, CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

		#region IGenericTransactionPlugIn members

		GenericTransactionHeaderSupporter IGenericTransactionHeaderPlugIn.HeaderSupporter
		{
			get { return fGenericTransactionSupporter ?? (fGenericTransactionSupporter = new DocStatementSummaryGenericTransactionSupporter(this)); }
		}
		DocStatementSummaryGenericTransactionSupporter fGenericTransactionSupporter;

		#endregion

		internal class DocStatementSummaryGenericTransactionSupporter : GenericTransactionHeaderSupporter
		{
			public DocStatementSummaryGenericTransactionSupporter(DocStatementSummary parent)
			{
				this.Parent = parent;
			}

			protected readonly DocStatementSummary Parent;

			protected internal override ZString GetTransactionType()
			{
				return "PSY";
			}

			protected internal override ZString GetDocumenTitle()
			{
				return Parent.DocumentTitle;
			}

			protected internal override ZString GetTaxId()
			{
				return Parent.TaxId;
			}

			protected internal override ZString GetOrganisationCode()
			{
				return Parent.OrganisationCode;
			}

			protected internal override ZString GetOrganisationName()
			{
				return Parent.OrganisationName;
			}

			protected internal override ZGuid GetOrganisationARAddressOrgAddressPK()
			{
				return Parent.AROrgAddressPK;
			}

			protected internal override ZDateTime GetDisplayDate()
			{
				return Parent.DisplayDate;
			}

			protected internal override ZString GetSTDTerms()
			{
				return Parent.STDTerms;
			}

			protected internal override ZString GetDSBTerms()
			{
				return Parent.DSBTerms;
			}

			protected internal override ZBool GetShouldHideCompanyName()
			{
				return Parent.ShouldHideCompanyName;
			}

			protected internal override DocGenericTransactionLineCollection GetStatementSummaryLines()
			{
				var result = new DocGenericTransactionLineCollection(Parent.Factory);

				foreach (DocStatementSummaryLine line in Parent.Lines)
				{
					result.Add(DocGenericTransactionLine.New(line, Parent.Factory));
				}
				return result;
			}
		}

		DocStatement FirstDocStatement
		{
			get
			{
				if (fFirstDocStatement == null)
				{
					fFirstDocStatement = DocStatement.New(PrintSummary.FirstPrintStatement, Factory);
				}
				return fFirstDocStatement;
			}
		}
		DocStatement fFirstDocStatement;

		public ZString DocumentTitle
		{
			get { return FirstDocStatement.DocumentTitle + " SUMMARY"; }
		}

		public ZString TaxId
		{
			get { return FirstDocStatement.TaxId; }
		}

		public ZGuid AROrgAddressPK
		{
			get { return FirstDocStatement.Organisation.ARAddress.OrgAddress.PK; }
		}

		public ZString OrganisationCode
		{
			get { return FirstDocStatement.Organisation.Code; }
		}

		public ZString OrganisationName
		{
			get { return FirstDocStatement.Organisation.Name; }
		}

		public ZString STDTerms
		{
			get { return FirstDocStatement.Organisation.MiscServ.ShortenedCreditTerms; }
		}

		public ZString DSBTerms
		{
			get { return FirstDocStatement.Organisation.MiscServ.ShortenedDisbursementCreditTerms; }
		}

		public ZDateTime DisplayDate
		{
			get { return FirstDocStatement.DisplayDate; }
		}

		public bool ShouldHideCompanyName
		{
			get
			{
				return !(FirstDocStatement.IssueBySettlementGroup &&
					(FirstDocStatement.IssueByTransactionBranch || FirstDocStatement.IssueByTransactionDepartment));
			}
		}

		public DocStatementSummaryLineCollection Lines
		{
			get
			{
				if (fLines == null)
				{
					fLines = new DocStatementSummaryLineCollection(Factory);

					foreach (PrintStatement statement in PrintSummary.PrintStatements.Values)
					{
						DocStatementSummaryLineCollection tempLines = new DocStatementSummaryLineCollection(Factory);
						DocStatement docStatement = DocStatement.New(statement, Factory);
						foreach (DocTransactionHeader transaction in docStatement.Transactions)
						{
							DocStatementSummaryLine line = tempLines.GetLineFromOrgCode(transaction.Organisation.Code);
							if (line != null)
							{
								line.AddValuesFromTransaction(transaction);
							}
							else
							{
								tempLines.Add(new DocStatementSummaryLine(docStatement, transaction));
							}
						}
						fLines.AddRange(tempLines);
					}

					SetTotalOverdueByCurrency(fLines);
					fLines.Sort("DescriptionTwo");
					fLines.Sort("DescriptionOne");
				}
				return fLines;
			}
		}
		DocStatementSummaryLineCollection fLines;

		void SetTotalOverdueByCurrency(DocStatementSummaryLineCollection lines)
		{
			Dictionary<string, decimal> overdueByCurrency = new Dictionary<string, decimal>();

			foreach (DocStatementSummaryLine line in lines)
			{
				if (overdueByCurrency.ContainsKey(line.CurrencyCode))
				{
					overdueByCurrency[line.CurrencyCode] += line.Overdue;
				}
				else
				{
					overdueByCurrency.Add(line.CurrencyCode, line.Overdue);
				}
			}

			foreach (DocStatementSummaryLine line in fLines)
			{
				line.TotalOverdueByCurrency = overdueByCurrency[line.CurrencyCode];
			}
		}
	}
}
