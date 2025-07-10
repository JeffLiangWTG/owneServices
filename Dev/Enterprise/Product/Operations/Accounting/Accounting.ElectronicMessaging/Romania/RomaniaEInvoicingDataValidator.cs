using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Romania
{
	public class RomaniaEInvoicingDataValidator : BaseEInvoicingDataValidator
	{
		public RomaniaEInvoicingDataValidator(GlbCompany company)
			: base(company, shouldSendErrorNotificationEmail: true)
		{
		}

		public override IReadOnlyCollection<ZString> ValidateTransaction(InvoicingBase transaction, AccEInvoicingTransactionPivot pivot)
		{
			return ValidateCredential(transaction);
		}

		protected override void RunCore(ILogger logger)	=> ValidateBatchedTransactions(logger);

		IReadOnlyCollection<ZString> ValidateCredential(InvoicingBase transaction)
		{
			var errorMessages = new List<ZString>();
			var credential = LoadCredential(transaction);

			if (credential == null)
			{
				errorMessages.Add(Res.GetString("4C8E18C6-B057-4368-8B65-03F999ACECA2", "Enable E-Reporting Functionality is set to Yes and Company {0} has no valid E-Invoicing Credentials.", transaction.Company.GC_Code));
			}

			return errorMessages;
		}

		GlbExternalPassword LoadCredential(InvoicingBase transaction)
		{
			var query = new ZDBOnlyQuery(typeof(GlbExternalPassword));
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.EIM);
			query.AddToFilter(GlbExternalPasswordSchema.GP_GC, transaction.AH_GC);

			var statusQuery = new ZDBOnlyQuery(typeof(GlbExternalPassword));
			var statusSubQueryForValid = new ZQuery();
			statusSubQueryForValid.AddToFilter(GlbExternalPasswordSchema.GP_PasswordStatus, PasswordStatusList.Codes.Valid);
			var statusSubQueryForPending = new ZQuery();
			statusSubQueryForPending.AddToFilter(GlbExternalPasswordSchema.GP_PasswordStatus, PasswordStatusList.Codes.Pending);
			statusSubQueryForPending.AddToFilter(GlbExternalPasswordSchema.GP_StatusReason, OAuthHelper.CredentialStatusReasonRefreshingToken);
			statusQuery.AddToFilter(statusSubQueryForValid, JoinCondition.Or);
			statusQuery.AddToFilter(statusSubQueryForPending, JoinCondition.Or);

			query.AddToFilter(statusQuery);

			return transaction.Factory.LoadTop1<GlbExternalPassword>(query);
		}
	}
}
