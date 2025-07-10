using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.SaudiArabia
{
	public class EInvoicingDataValidatorForSaudiArabia : BaseEInvoicingDataValidator
	{
		public EInvoicingDataValidatorForSaudiArabia(GlbCompany company)
			: base(company, shouldSendErrorNotificationEmail: true)
		{
		}

		protected override void RunCore(ILogger logger)
			=> ValidateBatchedTransactions(logger);

		public override IReadOnlyCollection<ZString> ValidateTransaction(InvoicingBase transaction, AccEInvoicingTransactionPivot pivot)
		{
			var errorMessages = new List<ZString>();

			ValidateCredential(transaction, errorMessages);
			ValidateNegativeLines(transaction, errorMessages);

			return errorMessages;
		}

		static void ValidateCredential(InvoicingBase transaction, List<ZString> errors)
		{
			var credential = LoadCredentials(transaction.Branch);
			if (credential == null)
			{
				errors.Add((NoResString)"Enable E-Reporting Functionality is set to Yes and Branch " + transaction.Branch.GB_Code + (NoResString)" has no valid E-Invoicing Credentials.");
			}
		}

		static void ValidateNegativeLines(InvoicingBase transaction, List<ZString> errors)
		{
			var negativeLines = transaction.Lines.OfType<InvoicingLineBase>().Any(x => x.AL_LocalTotalAmount < 0);
			if (negativeLines)
			{
				errors.Add((NoResString)"Negative charges are not permitted in e-Invoicing transactions. Registry settings can prevent negative charges from being posted.");
			}
		}

		static EInvoicingCertificateCredential LoadCredentials(GlbBranch branch)
		{
			var branchCredential = EInvoicingCertificateCredential.LoadBestCertificate(branch,
										currentTime: ZDateTime.Now,
										passwordType: PasswordTypesList.Codes.EIM,
										orderByColumn: GlbExternalPasswordSchema.GP_ExpiryDate.Name
									);

			return branchCredential;
		}
	}
}
