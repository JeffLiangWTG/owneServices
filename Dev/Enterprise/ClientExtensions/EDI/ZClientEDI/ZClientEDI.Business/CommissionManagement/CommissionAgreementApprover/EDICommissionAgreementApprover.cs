using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.CommissionManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.CommissionManagement.Business
{
	public class EDICommissionAgreementApprover : CommissionAgreementApprover
	{
		#region Constructor

		protected EDICommissionAgreementApprover(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region Execute

		protected override void CreateCommissionsCore(CreateCommissionContext context, Progress progress)
		{
			CreateOdplBillingCommissions(context, progress);
			CreateStlBillingCommissions(context, progress);
			CreateBorderWiseBillingCommissions(context, progress);

			base.CreateCommissionsCore(context, progress);
		}

		#region Odpl Billing without Revenue Breakdown Commissions

		void ValidateOdplBilling(CreateCommissionContext context)
		{
			var filter = GetOdplInvoicesWithoutRevenueBreakdownFilter(context);
			var invoices = string.Join(", ", Factory.Load<InvoicingBase>(filter).Where(x => x.AH_InvoiceAmount != 0).Select(x => x.AH_TransactionNum));

			if (!string.IsNullOrEmpty(invoices))
			{
				var agreements = string.Join(", ", context.AgreementsBeingApproved?.Select(x => x.AgreementId) ?? Enumerable.Empty<ZString>());
				var errorMessage = FormattableString.Invariant($@"The following Invoices were created prior to July 2016 and therefore have no associated revenue breakdown. As a result, these invoices cannot be processed by the Commission Agreement Generator and must be processed manually.

In order to process these Commission Agreements, please set the Backdate Commission Options 'From' date on the Commission Agreement Approval form to a Specified Date from 01-July-2016, and then reprocess the agreement.

Invoice(s): {invoices}

Agreement(s): {agreements}");
				var email = new EmailDef();
				email.Body = errorMessage;
				email.Subject = "Error generating commissions";
				Env.OutgoingMailManager.CreateAndSave(email, EDIDataRegistry.Instance.CommissionGeneratorNotificationGroup.Value, GroupSourceLocator.GetFromRegistryItem(EDIDataRegistry.Instance.CommissionGeneratorNotificationGroup));
				throw new CommissionAgreementApprovalWizardException(errorMessage);
			}
		}

		public static ZDBOnlyQuery GetOdplInvoicesWithoutRevenueBreakdownFilter(CreateCommissionContext context)
		{
			var filter = GetOdplInvoicesWithoutRevenueBreakdownFilterCore();
			filter.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.GreaterThanOrEqualTo, context.FromDate);
			filter.AddToFilter(ReversalTransactionCommissionCreator.GetIsNotReversalTransactionQuery());

			var applicableQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			if (context.OnlyCreateForAgreementsBeingApproved)
			{
				var pksOfAgreementsBeingApprovedSql = string.Join(", ", context.PksOfAgreementsBeingApproved.Select(x => "'" + x.ToString() + "'"));
				applicableQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, @"
AH_PK IN
(
	SELECT AH_PK
	FROM
		dbo.ClientChargeableUsage
		JOIN dbo.AccTransactionHeader ON U1_AH_Invoice = AH_PK
		JOIN dbo.LicenceCompany ON U1_LC = LC_PK
		JOIN dbo.ViewCommissionAgreement ON VCA_OH_Customer = LC_OH
	WHERE
		VCA_PK IN ({0})
		AND VCA_EffectiveDate <= AH_PostDate
		AND (VCA_ExpiredDate IS NULL OR VCA_ExpiredDate > AH_PostDate)
		AND VCA_ReversedDateTimeUtc IS NULL
		AND (
				SELECT COUNT(*) 
				FROM 
					dbo.AccTransactionLines
					JOIN dbo.AccChargeCode ON AC_PK = AL_AC
				WHERE 
					AL_AH = AH_PK
					AND AC_IsCommissionable = 1
			) > 0
)
", pksOfAgreementsBeingApprovedSql), new ZSqlParameterCollection());
			}

			if (context.OverwriteOldValues && context.PksOfAgreementsBeingApproved != null)
			{
				applicableQuery.AddSubQuery(GetHasExistingCommissionHeaderSubQuery(context.PksOfAgreementsBeingApproved), JoinCondition.Or);
			}

			filter.AddToFilter(applicableQuery);
			return filter;
		}

		static ZDBOnlyQuery GetOdplInvoicesWithoutRevenueBreakdownFilterCore()
		{
			var filter = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			filter.AddToFilter(AccTransactionHeaderSchema.AH_JH, null);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);

			var hasClientUsageSubQuery = new ZDBOnlySubQuery(typeof(ClientChargeableUsage), ClientChargeableUsageSchema.U1_AH_Invoice);
			hasClientUsageSubQuery.AddToFilter(ClientChargeableUsageSchema.U1_Code, SQLComparisonOperator.NotEqual, BillingConstants.BillingSystem.Maintenance);
			filter.AddSubQuery(hasClientUsageSubQuery, JoinCondition.And);

			var noRevenueBreakdownSubQuery = new ZDBOnlySubQuery(typeof(EdiBilledUsage), EdiBilledUsageSchema.BU9_AH_Invoice, true);
			filter.AddSubQuery(noRevenueBreakdownSubQuery, JoinCondition.And);

			return filter;
		}

		#endregion

		#region Odpl Billing Commissions

		void CreateOdplBillingCommissions(CreateCommissionContext context, Progress progress)
		{
			ValidateOdplBilling(context);

			CreateBillingCommissions(
				BillingConstants.PriceHeaderType.ODM,
				context,
				progress,
				new OdplBilledUsageCommissionGroupsCalculator(),
				GetCreatingOdplBillingCommissionStatusMessage);
		}

		static string GetCreatingOdplBillingCommissionStatusMessage(int processed, int total)
		{
			return Res.GetString("6de23b38-ac65-45bb-adcb-312f0a1a3bde", "Creating ODPL Billing Commissions: {0} of {1}", processed, total);
		}

		#endregion

		#region Stl Billing Commissions

		void CreateStlBillingCommissions(CreateCommissionContext context, Progress progress)
		{
			CreateBillingCommissions(
				BillingConstants.PriceHeaderType.STL,
				context,
				progress,
				new StlBilledUsageCommissionGroupsCalculator(),
				GetCreatingStlBillingCommissionStatusMessage);
		}

		static string GetCreatingStlBillingCommissionStatusMessage(int processed, int total)
		{
			return Res.GetString("a965652d-e8da-464e-aecc-9d40cd9fe010", "Creating STL Billing Commissions: {0} of {1}", processed, total);
		}

		#endregion

		#region BorderWise Billing Commissions

		void CreateBorderWiseBillingCommissions(CreateCommissionContext context, Progress progress)
		{
			CreateBillingCommissions(
				BillingConstants.PriceHeaderType.BorderWise,
				context,
				progress,
				new BorderWiseBilledUsageCommissionGroupsCalculator(),
				GetCreatingBorderWiseBillingCommissionStatusMessage,
				new[] { BillingConstants.PriceHeaderType.ODM, BillingConstants.PriceHeaderType.STL });
		}

		static string GetCreatingBorderWiseBillingCommissionStatusMessage(int processed, int total)
		{
			return Res.GetString("668d44d8-ed83-4e1d-a157-c2e04abc7c12", "Creating BorderWise Billing Commissions: {0} of {1}", processed, total);
		}

		#endregion

		#region Create Billing Commissions

		void CreateBillingCommissions(
			string billingModel,
			CreateCommissionContext context,
			Progress progress,
			IBilledUsageCommissionGroupsCalculator usageGroupsCalculator,
			Func<int, int, string> getStatusMessageDelegate,
			string[] billingModelsToExclude = null)
		{
			var invoicesQuery = GetInvoicesFilter(context, billingModel, billingModelsToExclude);
			var invoices = Factory.Load<InvoicingBase>(invoicesQuery);
			if (invoices.Length == 0)
			{
				return;
			}

			var processed = 0;
			foreach (var invoice in invoices)
			{
				var progressStatus = getStatusMessageDelegate(processed + 1, invoices.Length);
				var percentComplete = 100 * processed / invoices.Length;
				NotifyProgress(progress, progressStatus, percentComplete);

				new BillingCommissionCreator(invoice, usageGroupsCalculator).CreateCommissions(context);

				processed++;
			}

			NotifyProgress(progress, getStatusMessageDelegate(invoices.Length, invoices.Length), 100);

			DeleteAmbigiousCommissions(progress, invoices);
		}

		public static ZDBOnlyQuery GetInvoicesFilter(CreateCommissionContext context, string billingModel, string[] billingModelsToExclude = null)
		{
			var filter = GetInvoicesFilterCore(billingModel, billingModelsToExclude);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.GreaterThanOrEqualTo, context.FromDate);
			filter.AddToFilter(ReversalTransactionCommissionCreator.GetIsNotReversalTransactionQuery());

			var applicableQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			if (context.OnlyCreateForAgreementsBeingApproved)
			{
				var agreementSubQuery = new ZDBOnlySubQuery(typeof(ViewCommissionAgreement), ViewCommissionAgreementSchema.VCA_OH_Customer);
				agreementSubQuery.AddToFilter(ViewCommissionAgreementSchema.PK, context.PksOfAgreementsBeingApproved);
				agreementSubQuery.AddToFilter(ViewCommissionAgreementSchema.VCA_EffectiveDate, SQLComparisonOperator.LessThanOrEqualTo, AccTransactionHeaderSchema.AH_PostDate);

				agreementSubQuery.AddFilterAndZSQLParameterCollection(@"
VCA_ExpiredDate IS NULL
OR
VCA_ExpiredDate > AH_PostDate"
					, new ZSqlParameterCollection());

				agreementSubQuery.AddToFilter(ViewCommissionAgreementSchema.VCA_ReversedDateTimeUtc, null);

				applicableQuery.AddSubQuery(AccTransactionHeaderSchema.AH_OH, ViewCommissionAgreementSchema.VCA_OH_Customer, agreementSubQuery, JoinCondition.And);
			}

			if (context.OverwriteOldValues && context.PksOfAgreementsBeingApproved != null)
			{
				applicableQuery.AddSubQuery(GetHasExistingCommissionHeaderSubQuery(context.PksOfAgreementsBeingApproved), JoinCondition.Or);
			}

			filter.AddToFilter(applicableQuery);
			return filter;
		}

		static ZDBOnlyQuery GetInvoicesFilterCore(string billingModel, string[] billingModelsToExclude)
		{
			var filter = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			filter.AddToFilter(AccTransactionHeaderSchema.AH_JH, null);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);

			var billingModelSubQuery = new ZDBOnlySubQuery(typeof(EdiBilledUsage), EdiBilledUsageSchema.BU9_AH_Invoice);
			billingModelSubQuery.AddToFilter(EdiBilledUsageSchema.BU9_BillingModel, billingModel);
			filter.AddSubQuery(billingModelSubQuery, JoinCondition.And);

			if (billingModelsToExclude != null)
			{
				var exclusions = billingModelsToExclude.Where(billingModelToExclude => !string.IsNullOrEmpty(billingModelToExclude)).ToList();
				if (exclusions.Count > 0)
				{
					var excludedBillingModelsSubQuery = new ZDBOnlySubQuery(typeof(EdiBilledUsage), EdiBilledUsageSchema.BU9_AH_Invoice, true);
					excludedBillingModelsSubQuery.AddToFilter(EdiBilledUsageSchema.BU9_BillingModel, exclusions);

					filter.AddSubQuery(excludedBillingModelsSubQuery, JoinCondition.And);
				}
			}

			return filter;
		}

		#endregion

		#endregion

		#region AmbigiousCommissions

		void DeleteAmbigiousCommissions(Progress progress, InvoicingBase[] invoices)
		{
			var legacyAmbigiousCommissionsQuery = new ZQuery();
			legacyAmbigiousCommissionsQuery.AddToFilter(AccAmbiguousCommissionSchema.AC0_AH_Source, invoices.Select(x => x.PK));
			var legacyAmbigiousCommissions = Factory.Load<AccAmbiguousCommission>(legacyAmbigiousCommissionsQuery);

			var processed = 0;
			foreach (var ambiguousCommission in legacyAmbigiousCommissions)
			{
				var progressStatus = GetDeletingLegacyAmbigiousCommissionsStatusMessage(processed + 1, invoices.Length);
				NotifyProgress(progress, progressStatus, 100 * processed / invoices.Length);
				ambiguousCommission.Delete();
				processed++;
			}
		}

		static string GetDeletingLegacyAmbigiousCommissionsStatusMessage(int processed, int total)
		{
			return Res.GetString("56ad1663-df68-4976-b71e-a1af159cc650", "Deleting Legacy Ambiguous Commissions: {0} of {1}", processed, total);
		}

		#endregion

		#region NonJobRelatedCommissions

		protected override ZDBOnlyQuery GetNonJobRelatedTransactionFilter(CreateCommissionContext context)
		{
			var filter = base.GetNonJobRelatedTransactionFilter(context);

			var notUsageBillingFilter = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK, true);
			notUsageBillingFilter.AddToFilter(GetOdplInvoicesWithoutRevenueBreakdownFilterCore(), JoinCondition.Or);
			var hasBilledUsageSubQuery = new ZDBOnlySubQuery(typeof(EdiBilledUsage), EdiBilledUsageSchema.BU9_AH_Invoice);
			notUsageBillingFilter.AddSubQuery(hasBilledUsageSubQuery, JoinCondition.Or);

			filter.AddSubQuery(notUsageBillingFilter, JoinCondition.And);
			return filter;
		}

		protected override ZQuery GetNonJobRelatedTransactionHasAgreementBeingApprovedFilter(CreateCommissionContext context)
		{
			var customersOfAgreementsBeingApproved = context.AgreementsBeingApproved.Select(x => x.CA0_OH_Customer).Distinct().ToArray();
			var invoiceForAgreementCustomerFilterPart = new ZQuery(AccTransactionHeaderSchema.AH_OH, customersOfAgreementsBeingApproved);

			var opportunityClientsOfAgreementsBeingApproved = context.AgreementsBeingApproved.Select(x => x.Opportunity).Where(x => x != null).Select(x => x.P8_OH).Distinct().ToArray();
			var invoiceForAgreementOpportunityClientFilterPart = new ZQuery(AccTransactionHeaderSchema.AH_OH, opportunityClientsOfAgreementsBeingApproved);

			return new ZQuery(invoiceForAgreementCustomerFilterPart, JoinCondition.Or, invoiceForAgreementOpportunityClientFilterPart);
		}

		#endregion
	}
}
