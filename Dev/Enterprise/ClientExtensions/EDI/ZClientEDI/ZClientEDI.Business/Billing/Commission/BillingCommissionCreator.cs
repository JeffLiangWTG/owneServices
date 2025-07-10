using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BillingCommissionCreator : CommissionCreator
	{
		public BillingCommissionCreator(InvoicingBase invoice, IBilledUsageCommissionGroupsCalculator usageGroupsCalculator)
			: base(invoice.Factory)
		{
			Argument.NotNull(invoice, "invoice");
			Argument.NotNull(usageGroupsCalculator, "usageGroupsCalculator");

			this.invoice = invoice;
			this.UsageGroupsCalculator = usageGroupsCalculator;
		}

		readonly InvoicingBase invoice;
		public readonly IBilledUsageCommissionGroupsCalculator UsageGroupsCalculator;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public sealed override void CreateCommissions(CreateCommissionContext context)
		{
			if (!invoice.AH_IsCancelled)
			{
				var commissionDate = invoice.AH_PostDate.Date;
				var customerPk = invoice.AH_OH;
				const string product = ProductTypes.Codes.Enterprise;

				var groupedUsages = UsageGroupsCalculator.GetGroupedUsages(invoice);
				var builder = new MultipleCommissionHeaderBuilder(invoice, context);
				var majorGroupUsages = from m in groupedUsages group m by new { m.Key.Service, m.Key.SubModule };
				foreach (var majorGroupUsage in majorGroupUsages)
				{
					var service = majorGroupUsage.Key.Service;
					var subModule = majorGroupUsage.Key.SubModule;

					var majorGroupUsageList = majorGroupUsage.ToList();
					var licenceDatabases = majorGroupUsageList.Where(c => c.Key.ClientCompanyPk.IsEmpty).Select(d => d.Key.LicenceDatabasePk).Distinct().ToArray();
					var clientCompanies = majorGroupUsageList.Where(c => !c.Key.ClientCompanyPk.IsEmpty).Select(d => d.Key.ClientCompanyPk).Distinct().ToArray();

					var calculator = new BillingResponsibleAgreementsCalculator(Factory, customerPk, product, service, subModule, commissionDate);

					var responsibleAgreementsCompany = new Dictionary<(string Stream, ZGuid PivotPk), EdiCommissionAgreement>();
					var responsibleAgreementsDatabase = new Dictionary<(string Stream, ZGuid PivotPk), EdiCommissionAgreement>();

					if (clientCompanies.Any())
					{
						responsibleAgreementsCompany = calculator.GetForClientCompanies(clientCompanies);
					}

					if (licenceDatabases.Any())
					{
						responsibleAgreementsDatabase = calculator.GetForLicenseDatabases(licenceDatabases);
					}

					var responsibleAgreements = (responsibleAgreementsCompany.Concat(responsibleAgreementsDatabase)).ToDictionary(e => e.Key, e => e.Value);

					foreach (var usageGroup in majorGroupUsage)
					{
						var snapshotDateTime = invoice.AH_PostDate;
						var snapshotEventCode = AccCommissionHeaderSnapshotEventList.Codes.Posted;

						var buildItemArgs = new BillingCommissionHeaderBuildItemArgs(invoice, invoice, customerPk, usageGroup.Key.ClientCompanyPk, usageGroup.Key.LicenceDatabasePk, product, service, subModule, commissionDate, snapshotDateTime, snapshotEventCode);
						var buildItem = new BillingCommissionHeaderBuildItem(context, buildItemArgs, responsibleAgreements, (commissionHeader, agreementAndRatesProvider) =>
						{
							var usagesGroupedByChargeCodes =
								from usage in usageGroup
								group usage by
								new
								{
									ChargeCodePk = usage.BU9_AC_AmountChargeCode,
									DiscountChargeCodePk = usage.BU9_AC_DiscountChargeCode
								};

							foreach (var chargeCodeGroup in usagesGroupedByChargeCodes)
							{
								CreatePercentageCommissionLineGroups(commissionHeader, chargeCodeGroup, chargeCodeGroup.Key.ChargeCodePk, chargeCodeGroup.Key.DiscountChargeCodePk);
							}
						});

						builder.Create(buildItem);
					}
				}
				builder.DeleteInvalidCommission();
			}
			else
			{
				ReversalTransactionCommissionCreator.CreateReversalTransactionCommissionsIfRequired(invoice);
			}
		}

		void CreatePercentageCommissionLineGroups(AccCommissionHeader commissionHeader, IEnumerable<EdiBilledUsage> billedUsages, ZGuid chargeCodePk, ZGuid discountChargeCodePk)
		{
			var amountInTransactionCurrency = billedUsages.Sum(x => x.BU9_TransactionAmountPreDiscount);
			var amountInLocalCurrency = billedUsages.Sum(x => x.BU9_LocalAmountPreDiscount);
			if (amountInTransactionCurrency != 0 || amountInLocalCurrency != 0)
			{
				var amountCommissionLineGroup = commissionHeader.LineGroups.AddNew();

				amountCommissionLineGroup.CLG_AC = chargeCodePk;
				amountCommissionLineGroup.CLG_TransactionAmount = amountInTransactionCurrency;
				amountCommissionLineGroup.CLG_RX_NKTransactionCurrency = invoice.AH_RX_NKTransactionCurrency;
				amountCommissionLineGroup.CLG_TotalCommissionableAmount = amountInLocalCurrency;
				amountCommissionLineGroup.CLG_RX_NKCommissionCurrency = invoice.AH_Calc_LocalRXCode;
			}

			var discountAmountInTransactionCurrency = amountInTransactionCurrency - billedUsages.Sum(x => x.BU9_TransactionAmountPostDiscount) + billedUsages.Sum(x => x.BU9_TransactionProcessingAmount);
			var discountAmountInLocalCurrency = amountInLocalCurrency - billedUsages.Sum(x => x.BU9_LocalAmountPostDiscount) + billedUsages.Sum(x => x.BU9_LocalProcessingAmount);
			if (discountAmountInTransactionCurrency != 0 || discountAmountInLocalCurrency != 0)
			{
				var discountCommissionLineGroup = commissionHeader.LineGroups.AddNew();
				discountCommissionLineGroup.CLG_AC = discountChargeCodePk;
				discountCommissionLineGroup.CLG_TransactionAmount = -discountAmountInTransactionCurrency;
				discountCommissionLineGroup.CLG_RX_NKTransactionCurrency = invoice.AH_RX_NKTransactionCurrency;
				discountCommissionLineGroup.CLG_TotalCommissionableAmount = -discountAmountInLocalCurrency;
				discountCommissionLineGroup.CLG_RX_NKCommissionCurrency = invoice.AH_Calc_LocalRXCode;
			}
		}
	}

	class MultipleCommissionHeaderBuilder : CommissionHeaderBuilder
	{
		public MultipleCommissionHeaderBuilder(InvoicingBase invoice, CreateCommissionContext context)
			: base(invoice.Factory, context)
		{
			this.invoice = invoice;
		}

		readonly InvoicingBase invoice;
		readonly List<ICommissionHeaderBuildItem> allExpectedItems = new List<ICommissionHeaderBuildItem>();

		protected override void OnCreating(ICommissionHeaderBuildItem buildItem)
		{
			base.OnCreating(buildItem);
			allExpectedItems.Add(buildItem);
		}

		#region DeleteInvalidCommission

		public void DeleteInvalidCommission()
		{
			var headersFilter = Context.GetCommissionHeaderStreamsFilter();
			var expectedCommissionHeaderPks = new HashSet<ZGuid>();
			foreach (var builder in allExpectedItems)
			{
				foreach (var expectedCommissionHeader in builder.GetExistingCommissionHeaders(headersFilter))
				{
					expectedCommissionHeaderPks.Add(expectedCommissionHeader.PK);
				}
			}

			var existingCommissionHeadersQuery = new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice.PK);
			existingCommissionHeadersQuery.AddToFilter(AccCommissionHeaderSchema.CH0_OverridenDateTimeUtc, null);
			existingCommissionHeadersQuery.AddToFilter(headersFilter);
			var existingCommissionHeaders = Factory.Load<AccCommissionHeader>(existingCommissionHeadersQuery);
			foreach (var header in existingCommissionHeaders)
			{
				if (!expectedCommissionHeaderPks.Contains(header.PK))
				{
					header.MarkAsOverriden();
				}
			}
		}

		#endregion
	}
}

