using System;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	internal class ElectronicMessagingProcessingServiceTaskDataProviderForSaudiArabia : ElectronicMessagingProcessingServiceTaskDataProvider
	{
		protected override string GetAdditionalQueryForGetPKsOfCompaniesWithQueuedTransactions()
		{
			return FormattableString.Invariant($@" AND {AccEInvoicingTransactionPivotSchema.Constants.AIP_GC} NOT IN
								(SELECT {AccEInvoicingTransactionPivotSchema.Constants.AIP_GC}
								FROM	{AccEInvoicingTransactionPivotSchema.Constants.SqlSchemaName}.{AccEInvoicingTransactionPivotSchema.Constants.TableName}
								WHERE	{AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode} = '{CountryCodes.SaudiArabia}'
										AND	{AccEInvoicingTransactionPivotSchema.Constants.AIP_Status} = '{EInvoicingPivotState.Sent}')");  // Inline SQL is not translatable.
		}
	}
}
