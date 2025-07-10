using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	public class DatiPagamento
	{
		public XStreamingElement BuildXML(TransactionInfo transaction)
		{
			if (transaction.InvoiceTerm == InvoiceTermType.MLI)
			{
				return new XStreamingElement("DatiPagamento",
					new XElement("CondizioniPagamento", "TP01"),
					BuildXmlForDettagliPagamentoARate(transaction));
			}
			else
			{
				return new XStreamingElement("DatiPagamento",
					new XElement("CondizioniPagamento", "TP02"),
					BuildXmlForDettaglioPagamento(transaction));
			}
		}

		XStreamingElement BuildXmlForDettaglioPagamento(TransactionInfo transaction)
		{
			return new XStreamingElement("DettaglioPagamento",
				BuildXmlForModalitaPagamento(transaction.AgreedPaymentMethod),
				new XElement("DataScadenzaPagamento", transaction.DueDate.ToDateType()),
				new XElement("ImportoPagamento", transaction.LocalTotal.ToAmountDecimalType(transaction))
				);
		}

		List<XStreamingElement> BuildXmlForDettagliPagamentoARate(TransactionInfo transaction)
		{
			var listOfXElems = new List<XStreamingElement>();
			//TODO: extend XUT, then use transaction.InstalmentJournalCollection
			foreach (var instalment in FatturaElettronicaDataHelper.GetInstalmentJournals(transaction).OrderBy(arj => arj.AH_DueDate))
			{
				listOfXElems.Add(new XStreamingElement("DettaglioPagamento",
					BuildXmlForModalitaPagamento(instalment.AH_AgreedPaymentMethodOverride),
					new XElement("DataScadenzaPagamento", instalment.AH_DueDate.ToDateType()),
					new XElement("ImportoPagamento", instalment.AH_LocalTotal.ToAmountDecimalType(transaction))
					));
			}
			return listOfXElems;
		}

		XElement BuildXmlForModalitaPagamento(ZString? transPayMethod)
		{
			var modalitaPagamento = ZString.Empty;
			var agreedPaymentMethod = transPayMethod ?? ZString.Empty;
			if (!agreedPaymentMethod.IsEmpty)
			{
				switch (agreedPaymentMethod)
				{
					case OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck:
					case OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck:
						modalitaPagamento = "MP02";
						break;
					case OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer:
						modalitaPagamento = "MP05";
						break;
					case OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard:
					case OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard:
						modalitaPagamento = "MP08";
						break;
					case OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest:
						modalitaPagamento = "MP12";
						break;
					default:
						modalitaPagamento = string.Format(CultureInfo.InvariantCulture, "MP{0}", isGovernmentAllowedAgreedPaymentMethodCode(agreedPaymentMethod) ? agreedPaymentMethod.ToString() : "05"); // No localization required.
						break;
				}
			}
			return new XElement("ModalitaPagamento", modalitaPagamento);
		}

		bool isGovernmentAllowedAgreedPaymentMethodCode(ZString agreedPaymentMethod)
		{
			var isGovAllowedValue = false;
			if (agreedPaymentMethod.Length == 2)
			{
				if (Int32.TryParse(agreedPaymentMethod, out int number))
				{
					isGovAllowedValue = number >= 1 && number <= (FatturaElettronicaDataHelper.ShouldUseNewSchema ? 23 : 22);
				}
			}
			return isGovAllowedValue;
		}
	}
}
