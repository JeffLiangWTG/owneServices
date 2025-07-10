using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Israel
{
	public class IsraelEInvoicingObjectFactory : CountryEInvoicingObjectFactory
	{
		protected override ZString CountryCode => CountryCodes.Israel;

		protected override IncludeUniversalTransactionStrategy GEIMessageIncludeUniversalTransactionStrategy(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
			=> IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override IEInvoicingCredentialSettings Credentials => new IsraelEInvoicingCredentialSettings();

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> IsraelEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider
			=> new IsraelGlobalXUEFunctionalityProvider(this);

		protected override IElectronicMessagingNotificationEmailCreator GetElectronicMessagingNotificationEmailCreator()
			=> new GlobalEInvoiceTokenNotificationEmailCreator();

		protected override BaseEInvoicingDataValidator GetDataValidator(GlbCompany company)
			=> new EInvoicingDataValidatorForIsrael(company);

		protected override EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company)
			=> new NoGroupingEInvoicingBatchCreator(company);

		protected override ZQuery GetQueryForCompany(GlbCompany company, DateTime localTimeNow, int alertDays)
		{
			var query = new ZQuery(GlbExternalPasswordSchema.GP_GC, company.PK);
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.EIM);
			query.AddToFilter(GlbExternalPasswordSchema.GP_GB, SQLComparisonOperator.Equal, DBNull.Value);
			query.AddToFilter(GlbExternalPasswordSchema.GP_ExpiryDate, SQLComparisonOperator.LessThanOrEqualTo, localTimeNow.AddDays(alertDays));
			query.AddToFilter(GlbExternalPasswordSchema.GP_ExpiryDate, SQLComparisonOperator.GreaterThan, localTimeNow);

			return query;
		}
	}
}
