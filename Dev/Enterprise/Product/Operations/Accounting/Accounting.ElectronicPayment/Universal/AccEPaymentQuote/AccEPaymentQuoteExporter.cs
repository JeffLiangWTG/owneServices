using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Export.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalBranch = Enterprise.UniversalDataBuss.DataObjects.Branch;
using UniversalCurrency = Enterprise.UniversalDataBuss.DataObjects.Universal.Currency;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicPayment.Universal
{
	internal class AccEPaymentQuoteExporter
	{
		public AccEPaymentQuoteExporter()
		{
		}

		internal UniversalTransaction CreateUniversalTransaction(BatchExportDataAccess dataAccess, AccEPaymentQuote quote)
		{
			var writerStrategy = DefaultDataObjectWriterStrategy.Instance;
			var universalTransaction = new UniversalTransaction(writerStrategy);
			universalTransaction.DataContext = GetContextDataObject(dataAccess, quote.Company?.GC_Code, quote.QU_InternalReference, SchemaVersionManager.Current.Namespace);
			PopulateUniversalTransaction(quote, universalTransaction);
			return universalTransaction;
		}

		internal void PopulateUniversalTransaction(AccEPaymentQuote quote, UniversalTransaction dataObject)
		{
			PopulateValuesFromQuote(quote, dataObject);
			PopulateValuesFromPaymentApproval(quote, dataObject);
		}

		IDataContextDataObject GetContextDataObject(BatchExportDataAccess dataAccess, string companyCode, string quoteReference, string nameSpace)
		{
			if (!nameSpace.IsValidUniversalXmlNamespace())
			{
				throw new XmlProcessingException(string.Format(CultureInfo.InvariantCulture, "Invalid namespace [{0}] - Please use a valid Universal Namespace.", nameSpace ?? "(null)"));
			}
			IDataContextDataObject context = DataContextFactory.New(nameSpace);
			CompanyRow companyRow = dataAccess.LoadCompany(companyCode);
			context.SetCompanyAndDataProviderDetails(companyRow);
			context.AddDataSource(DataContextType.AccEPaymentQuote, quoteReference);
			return context;
		}

		void PopulateValuesFromQuote(AccEPaymentQuote sourceBO, UniversalTransaction dataObject)
		{
			var localCurrency = new UniversalCurrency();
			localCurrency.Code = sourceBO.QU_RX_NKFromCurrency;
			localCurrency.Description = sourceBO.FromCurrency?.RX_Desc;
			dataObject.LocalCurrency = localCurrency;
			dataObject.LocalTotal = sourceBO.QU_FromAmount;

			var osCurrency = new UniversalCurrency();
			osCurrency.Code = sourceBO.QU_RX_NKToCurrency;
			osCurrency.Description = sourceBO.ToCurrency?.RX_Desc;
			dataObject.OSCurrency = osCurrency;
			dataObject.OSTotal = sourceBO.QU_ToAmount;

			dataObject.TransactionReference = sourceBO.QU_InternalReference;
		}

		void PopulateValuesFromPaymentApproval(AccEPaymentQuote sourceBO, UniversalTransaction dataObject)
		{
			var paymentApproval = sourceBO.Factory.LoadTop1<AccPaymentApproval>(new ZQuery(AccPaymentApprovalSchema.PK, sourceBO.QU_AV));

			if (paymentApproval != null)
			{
				dataObject.Ledger = paymentApproval.AV_Ledger;
				dataObject.PostDate = paymentApproval.AV_PostDate;
				dataObject.TransactionDate = paymentApproval.AV_PaymentDate;
				dataObject.TransactionType = TransactionType.PAY;
				dataObject.Description = paymentApproval.AV_PaymentComment;
				dataObject.PaymentOrReceiptType = string.IsNullOrWhiteSpace(paymentApproval.AV_PaymentType) ? null : new PaymentOrReceiptTypeConverter().ToEnumValue(paymentApproval.AV_PaymentType);
				dataObject.CheckNumberOrPaymentRef = paymentApproval.AV_ChequeOrReference;
				dataObject.BankAccount = paymentApproval.BankAccount?.AB_Code;

				var branch = new UniversalBranch();
				branch.Code = paymentApproval.Branch?.GB_Code;
				branch.Name = paymentApproval.Branch?.GB_BranchName;
				dataObject.Branch = branch;
			}
		}
	}
}
