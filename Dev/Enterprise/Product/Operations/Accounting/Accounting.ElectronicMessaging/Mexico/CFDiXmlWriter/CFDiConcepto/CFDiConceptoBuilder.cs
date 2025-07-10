using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public interface ICFDiConceptoBuilder
	{
		ComprobanteConcepto[] BuildComprobanteConceptoInfo(TransactionInfo transaction, BusinessObjectFactory factory);
	}

	class CFDiConceptoBuilder : ICFDiConceptoBuilder
	{
		public CFDiConceptoBuilder()
		{
			conceptoImpuestoBuilder_constructorInitializedOnly = new CFDiConceptoImpuestosBuilder();
			transactionInfoHelper_constructorInitializedOnly = new TransactionInfoHelper();
		}

		ComprobanteConcepto[] ICFDiConceptoBuilder.BuildComprobanteConceptoInfo(TransactionInfo transaction, BusinessObjectFactory factory)
		{
			int decimalsOfInvoiceCurrency = TransactionInfoHelper.GetOSCurrencyDecimals(factory, transaction);

			var invoiceLineList = new List<ComprobanteConcepto>();
			var filteredJournalCollection = transaction?.PostingJournalCollection?.Where(line => (line.ChargeCode?.ChargeType?.Code).GetValueOrDefault() != "CMT");
			var postingJournalCollection = filteredJournalCollection ?? new List<PostingJournal>();

			foreach (var postingJournal in postingJournalCollection)
			{
				var invoiceLine = new ComprobanteConcepto()
				{
					Cantidad = 1,
					ClaveUnidad = "E48",
					ClaveProdServ = postingJournal.GovernmentReportingChargeCode,
					Descripcion = postingJournal.Description,
				};

				if (transaction.TransactionType.HasValue)
				{
					var lineOSAmount = postingJournal.OSAmount.HasValue ? GetValueToReport(postingJournal.OSAmount.Value, transaction.TransactionType.Value == TransactionType.CRD) : 0;

					invoiceLine.ValorUnitario = lineOSAmount;
					invoiceLine.Importe = lineOSAmount;
					invoiceLine.Impuestos = ConceptoImpuestoBuilder.BuildComprobanteConceptoImpuestosInfo(postingJournal, transaction.TransactionType.Value, decimalsOfInvoiceCurrency);
					invoiceLine.ObjetoImp = invoiceLine.Impuestos != null ? c_ObjetoImp.ObjetoImpuesto : c_ObjetoImp.NoObjetoImpuesto;
				}

				invoiceLineList.Add(invoiceLine);
			}
			return invoiceLineList.Count != 0 ? invoiceLineList.ToArray() : null;
		}

		decimal GetValueToReport(decimal value, bool changeSign)
		{
			return (changeSign) ? value * -1 : value;
		}

		ICFDiConceptoImpuestosBuilder ConceptoImpuestoBuilder => conceptoImpuestoBuilder_constructorInitializedOnly;
		ICFDiConceptoImpuestosBuilder conceptoImpuestoBuilder_constructorInitializedOnly;

		ITransactionInfoHelper TransactionInfoHelper => transactionInfoHelper_constructorInitializedOnly;
		ITransactionInfoHelper transactionInfoHelper_constructorInitializedOnly;

#if DEBUG
		public void SubstituteConceptoImpuestoBuilder_ForTestOnly(ICFDiConceptoImpuestosBuilder replacement) => conceptoImpuestoBuilder_constructorInitializedOnly = replacement;
		public ICFDiConceptoImpuestosBuilder ConceptoImpuestoBuilder_ExposedForTestOnly => ConceptoImpuestoBuilder;
		public void SubstituteTransactionInfoHelper_ForTestOnly(ITransactionInfoHelper replacement) => transactionInfoHelper_constructorInitializedOnly = replacement;
		public ITransactionInfoHelper TransactionInfoHelper_ExposedForTestOnly => TransactionInfoHelper;
#endif
	}
}
