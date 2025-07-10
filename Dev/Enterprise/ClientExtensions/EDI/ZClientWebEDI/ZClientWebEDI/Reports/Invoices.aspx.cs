using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class Invoices : BasePage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (SiteUser.IsLoggedIn)
			{
				OutstandingInvoiceCollection.Load();
				OutstandingInvoicesDataGrid.DataSource = OutstandingInvoiceCollection;
				OutstandingInvoicesDataGrid.DataBind();
				HistoricalInvoiceCollection.Load();
				HistoricalInvoicesDataGrid.DataSource = HistoricalInvoiceCollection;
				HistoricalInvoicesDataGrid.DataBind();
				PopulateFilters();
			}

			AdjustControlsForLiteViewMode();
		}

		void AdjustControlsForLiteViewMode()
		{
			if (IsInLiteViewMode)
			{
				Breadcrumb.Visible = false;
			}
		}

		void PopulateFilters()
		{
			if (YearDropDownList.Items.Count == 0)
			{
				var now = ZDateTime.Now.Year;
				for (var year = now; year >= now - 10; year--)
				{
					YearDropDownList.Items.Add(year.ToString());
				}
			}

			if (ProductDropDownList.Items.Count == 0)
			{
				ProductDropDownList.Items.Add(new ListItem("All", BillingConstants.BillingSystem.All));

				var usageCodes = GetUsageCodes();
				if (usageCodes.Contains(BillingConstants.BillingSystem.STL))
				{
					ProductDropDownList.Items.Add(new ListItem(ProductTypes.Descriptions.CargoWise, BillingConstants.BillingSystem.STL));
				}

				foreach (ICodeDescription item in new ProductTypes())
				{
					if (usageCodes.Contains(item.Code))
					{
						ProductDropDownList.Items.Add(new ListItem(item.Description, item.Code));
					}
				}
			}
		}

		#region Data Source

		AccTransactionHeaderCollection OutstandingInvoiceCollection
		{
			get
			{
				if (outstandingInvoiceCollection == null && SiteUser.LoggedInOrganisation != null)
				{
					ZQuery query = GetInvoicesQuery(false);
					outstandingInvoiceCollection = new AccTransactionHeaderCollection(Factory, query);
				}
				return outstandingInvoiceCollection;
			}
		}
		AccTransactionHeaderCollection outstandingInvoiceCollection;

		AccTransactionHeaderCollection HistoricalInvoiceCollection
		{
			get
			{
				if (hitoricalInvoiceCollection == null && SiteUser.LoggedInOrganisation != null)
				{
					ZQuery query = GetInvoicesQuery(true);
					hitoricalInvoiceCollection = new AccTransactionHeaderCollection(Factory, query);
				}
				return hitoricalInvoiceCollection;
			}
		}
		AccTransactionHeaderCollection hitoricalInvoiceCollection;

		ZQuery GetInvoicesQuery(bool isPaid)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			query.AddToFilter(AccTransactionHeaderSchema.AH_OH, SiteUser.LoggedInOrganisation.PK);
			query.AddToFilter(AccTransactionHeaderSchema.AH_InvoicePrinted, ZBool.True);
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);

			ZQuery typeFilter = new ZQuery();
			typeFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote);
			typeFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
			query.AddToFilter(typeFilter);

			if (isPaid)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, SQLComparisonOperator.Equal, null);
			}

			var yearFilter = int.TryParse(YearDropDownList.Text, out var result) ? result : ZDateTime.Now.Year;
			query.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, new ZDate(yearFilter, 1, 1));
			query.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, new ZDate(yearFilter, 12, 31));

			var productFilter = ProductDropDownList.SelectedValue ?? BillingConstants.BillingSystem.All;
			if (!productFilter.IsNullOrEmpty() && productFilter != BillingConstants.BillingSystem.All)
			{
				var productQuery = new ZDBOnlySubQuery(typeof(ClientChargeableUsage), ClientChargeableUsageSchema.U1_AH_Invoice);
				productQuery.AddToFilter(ClientChargeableUsageSchema.U1_Code, productFilter);
				productQuery.AddToFilter(ClientChargeableUsageSchema.U1_AH_Invoice, SQLComparisonOperator.IsNotBlank, null);
				query.AddSubQuery(productQuery, JoinCondition.And);
			}

			query.OrderBy = AccTransactionHeaderSchema.Constants.AH_TransactionNum + OrderByClause.Descending;

			return query;
		}

		HashSet<string> GetUsageCodes()
		{
			var sql = @"SELECT DISTINCT U1_Code 
FROM dbo.AccTransactionHeader
JOIN dbo.ClientChargeableUsage ON AH_PK = U1_AH_Invoice
WHERE AH_OH = @AH_OH
AND AH_invoicedate >= @LastYear
AND U1_AH_Invoice IS NOT NULL
AND U1_PeriodStart >= @LastYear;";

			var parameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@AH_OH", SiteUser.LoggedInOrganisation.PK, CargoWise.Schema.Schema.GenericGuidSchemaColumn),
				ZSqlParameter.New("@LastYear", ZDateTime.UtcToday.AddYears(-1), CargoWise.Schema.Schema.GenericDateTimeColumn)
			};

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sql, parameters);

			return collection.Select(d => d["U1_Code"].ToString()).ToHashSet();
		}

		#endregion

		#region Event Handlers

		protected void InvoicesDataGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.SelectedItem)
			{
				AccTransactionHeader transaction = e.Item.DataItem as AccTransactionHeader;
				HyperLink invoiceLink = e.Item.FindControl("InvoiceLink") as HyperLink;

				if (transaction != null && invoiceLink != null)
				{
					if (transaction.DocManagerInfo.AllEDocs.GetMostRecentEDoc("INV") != null)
					{
						invoiceLink.NavigateUrl = LinkHelper.GetHandlerUrl(transaction.PK);
					}
					else
					{
						invoiceLink.Visible = false;
					}
				}
			}
		}

		InvoiceRequestHelper LinkHelper
		{
			get { return linkHelper ?? (linkHelper = new InvoiceRequestHelper()); }
		}
		InvoiceRequestHelper linkHelper;

		#endregion
	}
}
