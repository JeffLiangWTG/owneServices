using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing
{
	public class GovtTaxInvoicePrintTask : InvoicePrintTask
	{
		public GovtTaxInvoicePrintTask(Configuration configuration)
			: base(configuration)
		{
		}

		public const string GovtTaxInvoice = "TAX";
		public const string EnterpriseInvoice = "ENT";
		public const string AllocateSequenceNumberOnly = "ALS";
		public const string BothGovtTaxAndEnterpriseInvoice = "ALL";

		protected override void AddInvoiceToPack(InvoicingBase invoice, List<DocumentPack> packs)
		{
			if (invoicesGroupedByOrg.Count == 1 && invoicesGroupedByOrg.Values.First().Count == 1)
			{
				if (InvoicePrintingOptionCode.IsEmpty)
				{
					InvoicePrintingOptionCode = AccountingConfigurationRegistry.Instance.InvoicePrintingOption.Value;
				}

				var classAInvoice = invoicesGroupedByOrg.Values.First()[0];

				if (InvoicePrintingOptionCode == GovtTaxInvoice)
				{
					ThrowIndonesianConstraintExceptionIfNecessary(classAInvoice);
					ZGuid menuPK = GetMenuPK(classAInvoice);
					if (!menuPK.IsValid)
					{
						AddInvoiceToPackCore(classAInvoice, packs, GetMenuNames(classAInvoice));
					}
					else
					{
						AddInvoiceToPackCore(classAInvoice, packs, menuPK);
					}
				}
				else if (InvoicePrintingOptionCode == EnterpriseInvoice)
				{
					AddInvoiceToPackCore(classAInvoice, packs, base.GetMenuNames(classAInvoice));
				}
				else if (InvoicePrintingOptionCode == BothGovtTaxAndEnterpriseInvoice)
				{
					// Put A4 Document first to make sure footer is handled correctly in DocEngine
					AddInvoiceToPackCore(classAInvoice, packs, base.GetMenuNames(classAInvoice));

					ThrowIndonesianConstraintExceptionIfNecessary(classAInvoice);
					ZGuid menuPK = GetMenuPK(classAInvoice);
					if (!menuPK.IsValid)
					{
						AddInvoiceToPackCore(classAInvoice, packs, GetMenuNames(classAInvoice));
					}
					else
					{
						AddInvoiceToPackCore(classAInvoice, packs, menuPK);
					}
				}
			}
			else
			{
				base.AddInvoiceToPack(invoice, packs);
			}
		}

		internal override ZGuid GetMenuPK(TransactionHeader header)
		{
			return header.GovernmentInvoiceMenuPK;
		}

		internal override ZString[] GetMenuNames(TransactionHeader header)
		{
			List<ZString> result = new List<ZString>();
			ZString govtComplianceInvoiceMenuName = header.GovernmentInvoiceMenuName;
			if (!govtComplianceInvoiceMenuName.IsEmpty)
			{
				result.Add(govtComplianceInvoiceMenuName);
			}
			return result.ToArray();
		}

		public static string CheckIndonesianConstraints(InvoicingBase invoice)
		{
			string errorMessage = "";

			if (invoice != null)
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Indonesia)
				{
					if (invoice.AH_TransactionType != TransactionTypes.Invoice)
					{
						errorMessage = IndonesiaConstraintMessage.IndonesiaTransactionMustBeAnInvoice;
					}
					else if (invoice.AH_GSTAmount == 0m)
					{
						errorMessage = IndonesiaConstraintMessage.IndonesiaTransactionMustHaveAnAmountOfTax;
					}
					else if (invoice.IsReversed && invoice.AH_TransactionBelongsToGroup.IsValid)
					{
						errorMessage = IndonesiaConstraintMessage.IndonesiaReversalTransactionMayNotBeUpdated;
					}
					else if (invoice.IsReversed && invoice.AH_TransactionReference.IsEmpty)
					{
						errorMessage = IndonesiaConstraintMessage.IndonesiaReversedTransactionWithEmptyGovernmentComplianceNumberMayNotBeUpdated;
					}
				}
			}
			return errorMessage;
		}

		void ThrowIndonesianConstraintExceptionIfNecessary(InvoicingBase invoice)
		{
			if (invoice != null)
			{
				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Indonesia)
				{
					if (invoice.AH_TransactionType != TransactionTypes.Invoice)
					{
						throw new IndonesianConstraintFailedException(IndonesiaConstraintMessage.IndonesiaTransactionMustBeAnInvoice);
					}
					else if (invoice.AH_GSTAmount == 0m)
					{
						throw new IndonesianConstraintFailedException(IndonesiaConstraintMessage.IndonesiaTransactionMustHaveAnAmountOfTax);
					}
					else if (invoice.IsReversed && invoice.AH_TransactionBelongsToGroup.IsValid)
					{
						throw new IndonesianConstraintFailedException(IndonesiaConstraintMessage.IndonesiaReversalTransactionMayNotBeUpdated);
					}
					else if (invoice.IsReversed && invoice.AH_TransactionReference.IsEmpty)
					{
						throw new IndonesianConstraintFailedException(IndonesiaConstraintMessage.IndonesiaReversedTransactionWithEmptyGovernmentComplianceNumberMayNotBeUpdated);
					}
				}
			}
		}

		class IndonesiaConstraintMessage
		{
			internal static string IndonesiaTransactionMustBeAnInvoice
			{
				get { return Res.GetString("4af11dc9-c1a7-4a23-a060-6671cb900f26", "Transaction must be an Invoice."); }
			}

			internal static string IndonesiaTransactionMustHaveAnAmountOfTax
			{
				get { return Res.GetString("9f4f0386-dd6e-4c8b-803c-17e77ef38c80", "Transaction must have an amount of tax."); }
			}

			internal static string IndonesiaReversalTransactionMayNotBeUpdated
			{
				get { return Res.GetString("3439656b-d359-4bdc-83a8-72b1dd44e188", "Reversal transaction may not be updated."); }
			}

			internal static string IndonesiaReversedTransactionWithEmptyGovernmentComplianceNumberMayNotBeUpdated
			{
				get { return Res.GetString("55b0c349-6e1e-4481-b5c6-e0d3b8ea062c", "Reversed transaction with empty government compliance number may not be updated."); }
			}
		}
	}
}
