using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.MasterFiles.Business.CountryCompliance.UruguayComplianceInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay
{
	public interface IIdDocumentoBuilder
	{
		IdDoc_Fact BuildEFacIdDocumento(TransactionInfo transaction);
		IdDoc_Tck BuildETicketIdDocumento(TransactionInfo transaction);
	}

	class IdDocumentoBuilder : IIdDocumentoBuilder
	{
		public IdDocumentoBuilder()
		{
			CFEHelper_constructorInitializedOnly = new CFEHelper();
		}

		ICFEHelper CFEHelper => CFEHelper_constructorInitializedOnly;
		ICFEHelper CFEHelper_constructorInitializedOnly;

#if DEBUG
		public void SubstituteCFEHelper_ForTestOnly(ICFEHelper replacement) => CFEHelper_constructorInitializedOnly = replacement;
		public ICFEHelper CFEHelper_ExposedForTestOnly => CFEHelper;
#endif

		IdDoc_Fact IIdDocumentoBuilder.BuildEFacIdDocumento(TransactionInfo transaction)
		{
			var idDoc = new IdDoc_Fact();

			if (EFacturaContingencySubTypes(transaction?.ComplianceSubType))
			{
				(var serie, var numero) = CFEHelper.GetInvoiceSerieAndNumber(transaction?.TransactionReference ?? "");

				idDoc.Serie = serie;
				idDoc.Nro = numero;
			}

			switch (transaction?.ComplianceSubType)
			{
				case ComplianceSubTypeCodes.TXI:
					idDoc.TipoCFE = IdDoc_FactTipoCFE.Item111;
					break;
				case ComplianceSubTypeCodes.TCR:
					idDoc.TipoCFE = IdDoc_FactTipoCFE.Item112;
					break;
				case ComplianceSubTypeCodes.TCD:
					idDoc.TipoCFE = IdDoc_FactTipoCFE.Item113;
					break;
				case ComplianceSubTypeCodes.YXI:
					idDoc.TipoCFE = IdDoc_FactTipoCFE.Item211;
					break;
				case ComplianceSubTypeCodes.YCR:
					idDoc.TipoCFE = IdDoc_FactTipoCFE.Item212;
					break;
				case ComplianceSubTypeCodes.YCD:
					idDoc.TipoCFE = IdDoc_FactTipoCFE.Item213;
					break;
				default:
					return null;
			}

			if (transaction != null)
			{
				if (transaction.TransactionDate.HasValue)
				{
					idDoc.FchEmis = transaction.TransactionDate.Value.ToDateTime();
				}

				if (transaction.InvoiceTerm.HasValue)
				{
					idDoc.FmaPago = IsCashInvoice(transaction.InvoiceTerm.Value) ? IdDoc_FactFmaPago.Item1 : IdDoc_FactFmaPago.Item2;
				}

				if (DoesContainExcludedTax(transaction))
				{
					idDoc.IndPagCta3rosSpecified = true;
					idDoc.IndPagCta3ros = IndPagCta3rosType.Item1;
				}

				if (transaction.DueDate.HasValue)
				{
					idDoc.FchVenc = transaction.DueDate.Value.ToDateTime();
					idDoc.FchVencSpecified = true;
				}
			}

			return idDoc;
		}

		IdDoc_Tck IIdDocumentoBuilder.BuildETicketIdDocumento(TransactionInfo transaction)
		{
			var idDoc = new IdDoc_Tck();

			if (ETicketContingencySubTypes(transaction?.ComplianceSubType))
			{
				(var serie, var numero) = CFEHelper.GetInvoiceSerieAndNumber(transaction?.TransactionReference ?? "");

				idDoc.Serie = serie;
				idDoc.Nro = numero;
			}

			switch (transaction?.ComplianceSubType)
			{
				case ComplianceSubTypeCodes.TKT:
					idDoc.TipoCFE = IdDoc_TckTipoCFE.Item101;
					break;
				case ComplianceSubTypeCodes.TKC:
					idDoc.TipoCFE = IdDoc_TckTipoCFE.Item102;
					break;
				case ComplianceSubTypeCodes.TKD:
					idDoc.TipoCFE = IdDoc_TckTipoCFE.Item103;
					break;
				case ComplianceSubTypeCodes.YKT:
					idDoc.TipoCFE = IdDoc_TckTipoCFE.Item201;
					break;
				case ComplianceSubTypeCodes.YKR:
					idDoc.TipoCFE = IdDoc_TckTipoCFE.Item202;
					break;
				case ComplianceSubTypeCodes.YKD:
					idDoc.TipoCFE = IdDoc_TckTipoCFE.Item203;
					break;
				default:
					return null;
			}

			if (transaction != null)
			{
				if (transaction.TransactionDate.HasValue)
				{
					idDoc.FchEmis = transaction.TransactionDate.Value.ToDateTime();
				}

				if (transaction.InvoiceTerm.HasValue)
				{
					idDoc.FmaPago = IsCashInvoice(transaction.InvoiceTerm.Value) ? IdDoc_TckFmaPago.Item1 : IdDoc_TckFmaPago.Item2;
				}

				if (DoesContainExcludedTax(transaction))
				{
					idDoc.IndPagCta3rosSpecified = true;
					idDoc.IndPagCta3ros = IndPagCta3rosType.Item1;
				}

				if (transaction.DueDate.HasValue)
				{
					idDoc.FchVenc = transaction.DueDate.Value.ToDateTime();
					idDoc.FchVencSpecified = true;
				}
			}

			return idDoc;
		}
		bool IsCashInvoice(InvoiceTermType invoiceTerm)
		{
			return (invoiceTerm == InvoiceTermType.COD || invoiceTerm == InvoiceTermType.PIA);
		}

		bool DoesContainExcludedTax(TransactionInfo transaction)
		{
			return transaction.PostingJournalCollection != null && transaction.PostingJournalCollection.Any(x => x.VATTaxID?.TaxType?.Code.ToString() == AccTaxRate.Types.ExcludedFromTheTaxBase);
		}

		bool EFacturaContingencySubTypes(string complianceSubtype)
		{
			switch (complianceSubtype)
			{
				case ComplianceSubTypeCodes.YXI:
				case ComplianceSubTypeCodes.YCD:
				case ComplianceSubTypeCodes.YCR:
					return true;
				default:
					return false;
			}
		}

		bool ETicketContingencySubTypes(string complianceSubtype)
		{
			switch (complianceSubtype)
			{
				case ComplianceSubTypeCodes.YKT:
				case ComplianceSubTypeCodes.YKD:
				case ComplianceSubTypeCodes.YKR:
					return true;
				default:
					return false;
			}
		}
	}
}
