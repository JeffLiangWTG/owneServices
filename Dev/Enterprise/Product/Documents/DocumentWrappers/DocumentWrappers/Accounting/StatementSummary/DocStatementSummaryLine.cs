using CargoWise.EntityFramework;
using CargoWise.Types;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocStatementSummaryLine : NonPersistentBusinessObject, IObsoleteValidation, IGenericTransactionLinePlugIn
	{
		public DocStatementSummaryLine(DocStatement statement, DocTransactionHeader transactionHeader)
		{
			this.statement = statement;
			this.transactionHeader = transactionHeader;
			AddValuesFromTransaction(transactionHeader);
		}

		readonly DocStatement statement;
		readonly DocTransactionHeader transactionHeader;

		#region IGenericTransactionLinePlugIn members

		GenericTransactionLineSupporter IGenericTransactionLinePlugIn.LineSupporter
		{
			get { return fGenericTransactionSupporter ?? (fGenericTransactionSupporter = new DocStatementSummaryLineGenericTransactionSupporter(this)); }
		}
		DocStatementSummaryLineGenericTransactionSupporter fGenericTransactionSupporter;

		#endregion

		class DocStatementSummaryLineGenericTransactionSupporter : GenericTransactionLineSupporter
		{
			public DocStatementSummaryLineGenericTransactionSupporter(DocStatementSummaryLine parent)
			{
				this.Parent = parent;
			}
			protected readonly DocStatementSummaryLine Parent;

			protected internal override ZString GetDescriptionOne()
			{
				return Parent.DescriptionOne;
			}

			protected internal override ZString GetDescriptionTwo()
			{
				return Parent.DescriptionTwo;
			}

			protected internal override ZInt GetCount()
			{
				return Parent.Count;
			}

			protected internal override ZDecimal GetLineAmount()
			{
				return Parent.Amount;
			}

			protected internal override DocCurrency GetCurrency()
			{
				return Parent.statement.Currency;
			}

			protected internal override ZString GetOrganisationCode()
			{
				return Parent.OrganisationCode;
			}

			protected internal override ZString GetOrganisationName()
			{
				return Parent.OrganisationName;
			}

			protected internal override ZString GetTotalOverdueByCurrencyFormatted()
			{
				return Parent.TotalOverdueByCurrencyFormatted;
			}
		}

		public ZString DescriptionOne
		{
			get
			{
				if (statement.IssueByTransactionBranch)
				{
					return transactionHeader.Branch.BranchName;
				}
				else
				{
					if (statement.IssueByTransactionDepartment)
					{
						return transactionHeader.Department.Desc;
					}
					else
					{
						return transactionHeader.Organisation.Name;
					}
				}
			}
		}

		public ZString DescriptionTwo
		{
			get
			{
				if (statement.IssueByTransactionBranch && statement.IssueByTransactionDepartment)
				{
					return transactionHeader.Department.Desc;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString CurrencyCode
		{
			get { return statement.Currency.Code; }
		}

		public ZString CurrencyName
		{
			get { return statement.Currency.Desc; }
		}

		public ZString OrganisationCode
		{
			get { return transactionHeader.Organisation.Code; }
		}

		public ZString OrganisationName
		{
			get { return transactionHeader.Organisation.Name; }
		}

		public ZInt Count { get; private set; }
		public ZDecimal Amount { get; private set; }
		public ZDecimal Overdue { get; private set; }
		public ZDecimal TotalOverdueByCurrency { get; set; }

		public ZString TotalOverdueByCurrencyFormatted
		{
			get
			{
				ZString result = ZString.Empty;
				if (TotalOverdueByCurrency > 0)
				{
					result = Res.GetString("c9944403-d277-4f0c-80e2-0b591a3b0619", "Overdue at statement date: {0} {1}", FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalOverdueByCurrency, statement.Currency), CurrencyCode);
				}
				return result;
			}
		}

		public void AddValuesFromTransaction(DocTransactionHeader transactionHeader)
		{
			Count++;
			Amount += transactionHeader.Balance;

			if (transactionHeader.DueDate < ZDateTime.Today)
			{
				Overdue += transactionHeader.Balance;
			}
		}
	}
}
