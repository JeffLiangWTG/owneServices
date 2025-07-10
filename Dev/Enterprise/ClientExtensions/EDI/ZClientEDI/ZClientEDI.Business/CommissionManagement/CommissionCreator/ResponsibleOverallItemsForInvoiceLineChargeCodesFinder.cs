using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.EDI.CommissionManagement.Business
{
	public static class ResponsibleOverallItemsForInvoiceLineChargeCodesFinder
	{
		public static IEnumerable<ViewCommissionAgreementOverallItem> GetResponsibleOverallItems(ICommissionableTransaction invoice, ZString? commissionStream = null)
		{
			var factory = invoice.Factory;
			var mainInvoice = TransactionCommissionCreator.GetMainTransaction(invoice);
			var commissionDate = mainInvoice.AH_PostDate.Date;
			var debtorPk = invoice.AH_OH;
			var linesGroupedByChargeCode =
				from AccTransactionLines line in invoice.Lines
				where line.ChargeCode != null
				group line by line.ChargeCode;

			foreach (var lineGrouping in linesGroupedByChargeCode)
			{
				var chargeCode = lineGrouping.Key;
				if (chargeCode.AC_IsCommissionable && !chargeCode.AC_DefaultCommissionProduct.IsEmpty)
				{
					var itemArgs = new CommissionItemArgs(ZGuid.Empty, chargeCode.AC_DefaultCommissionProduct, chargeCode.AC_DefaultCommissionService, chargeCode.AC_DefaultCommissionSubModule, commissionDate, "", "", "");
					var opportunityOverallItemsQuery = ResponsibleOverallItemsQueryBuilder.New(debtorPk, itemArgs, commissionStream);
					var opportunityQuery = new ZDBOnlyQuery(typeof(ViewCommissionAgreementOverallItem));
					opportunityQuery.AddFilterAndZSQLParameterCollection(opportunityOverallItemsQuery.ParameterisedText.ParameterisedQueryText, new ZSqlParameterCollection(opportunityOverallItemsQuery.Params));

					foreach (var opportunityItem in factory.Load<ViewCommissionAgreementOverallItem>(opportunityQuery)
						.Where(x => new CommissionAgreementAndRates(x.CommissionAgreement, commissionDate).RecipientRatePairs.Any()))
					{
						yield return opportunityItem;
					}

					itemArgs = new CommissionItemArgs(debtorPk, chargeCode.AC_DefaultCommissionProduct, chargeCode.AC_DefaultCommissionService, chargeCode.AC_DefaultCommissionSubModule, commissionDate, "", "", "");
					var customerOverallItemsItems = ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs, commissionStream);
					var customerQuery = new ZDBOnlyQuery(typeof(ViewCommissionAgreementOverallItem));
					customerQuery.AddFilterAndZSQLParameterCollection(customerOverallItemsItems.ParameterisedText.ParameterisedQueryText, new ZSqlParameterCollection(customerOverallItemsItems.Params));

					foreach (var customerItem in factory.Load<ViewCommissionAgreementOverallItem>(customerQuery)
						.Where(x => new CommissionAgreementAndRates(x.CommissionAgreement, commissionDate).RecipientRatePairs.Any()))
					{
						yield return customerItem;
					}
				}
			}
		}
	}
}
