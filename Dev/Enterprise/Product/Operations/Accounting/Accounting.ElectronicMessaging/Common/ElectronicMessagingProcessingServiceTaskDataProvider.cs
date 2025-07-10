using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public interface IElectronicMessagingProcessingServiceTaskDataProvider
	{
		/// <summary>
		/// List of unique GC_PKs that have queued transactions based on AccEInvoicingTransactionPivot.
		/// </summary>
		IReadOnlyCollection<ZGuid> GetPKsOfCompaniesWithQueuedTransactions(string countryCode);

		/// <summary>
		/// List of unique GC_PKs that have periodic requests to process.
		/// </summary>
		IReadOnlyCollection<ZGuid> GetPKsOfCompaniesWithPeriodicRequests(string countryCode);

		/// <summary>
		/// List of unique GC_PKs that are enabled for e-Invoicing by EnableEInvoicingFunctionality registry item.
		/// </summary>
		IReadOnlyCollection<ZGuid> GetPKsOfCompaniesThatEnabledEInvoicing(string countryCode);
	}

	internal class ElectronicMessagingProcessingServiceTaskDataProvider : IElectronicMessagingProcessingServiceTaskDataProvider
	{
		public virtual IReadOnlyCollection<ZGuid> GetPKsOfCompaniesWithQueuedTransactions(string countryCode)
		{
			var sqlQuery = $@"SELECT DISTINCT {AccEInvoicingTransactionPivotSchema.Constants.AIP_GC}
FROM	{AccEInvoicingTransactionPivotSchema.Constants.SqlSchemaName}.{AccEInvoicingTransactionPivotSchema.Constants.TableName}
WHERE	{AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode} = @CountryCode
AND	{AccEInvoicingTransactionPivotSchema.Constants.AIP_Status} = '{EInvoicingPivotState.Queued}'
{GetAdditionalQueryForGetPKsOfCompaniesWithQueuedTransactions()}";  // Inline SQL is not translatable.

			using (var command = Db.Connection.Command(sqlQuery))   // We are not using a factory here for performance.
			{
				command.AddParameter("@CountryCode", SqlDbType.Char, countryCode);

				var result = DataUtils.GetListOfValuesFromCommand(command).Select(x => new ZGuid(x));
				return result.ToList();
			}
		}

		public virtual IReadOnlyCollection<ZGuid> GetPKsOfCompaniesWithPeriodicRequests(string countryCode) => (IReadOnlyCollection<ZGuid>)Enumerable.Empty<ZGuid>();

		public IReadOnlyCollection<ZGuid> GetPKsOfCompaniesThatEnabledEInvoicing(string countryCode)
		{
			var factory = new BusinessObjectFactory();
			var countrySpecificCompanies = factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, countryCode));
			return countrySpecificCompanies
						.Where(c => AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetValueWithoutFallback(c.PK.ToGuid(), Guid.Empty, Guid.Empty)
								|| AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.GetValueWithoutFallback(c.PK.ToGuid(), Guid.Empty, Guid.Empty))
						.Select(c => c.PK)
						.ToHashSet();
		}

		protected virtual string GetAdditionalQueryForGetPKsOfCompaniesWithQueuedTransactions() => string.Empty;
	}
}
