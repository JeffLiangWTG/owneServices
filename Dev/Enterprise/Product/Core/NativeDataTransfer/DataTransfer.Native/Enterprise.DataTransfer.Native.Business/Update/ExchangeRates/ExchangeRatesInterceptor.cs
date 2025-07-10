using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.ExchangeRates;

public class ExchangeRatesInterceptor : BaseInterceptor
{
	public ExchangeRatesInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices) : base(setting, sessionServices)
	{
		factory = setting.Context.ObjectFactory;
	}
	readonly BusinessObjectFactory factory;

	public override void Invoke(IEntitySet entitySet)
	{
		var entityAction = entitySet?.Root?.Action;
		CheckOverlappingRecords(entitySet.Root);

		Function(entitySet);
	}

	void CheckOverlappingRecords(IEntity entity)
	{
		var exchangRateRecord = new RefExchangeRateRecord(factory, entity);
		var currency = exchangRateRecord.Currency;
		var company = exchangRateRecord.Company;
		var startDate = exchangRateRecord.StartDate;
		var expiryDate = exchangRateRecord.ExpiryDate;
		if (!currency.IsEmpty && startDate.IsValid && expiryDate.IsValid && company != null)
		{
			var exRateType = exchangRateRecord.ExRateType;
			var query = new ZQuery(RefExchangeRateSchema.RE_GC, company.PK);
			query.AddToFilter(new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, currency));
			var exchangeRates = factory.Load<RefExchangeRate>(query);
			foreach (var exchangeRate in exchangeRates)
			{
				if (exRateType == exchangeRate.RE_ExRateType && (!exchangeRate.RE_IsSystem || exchangeRate.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.CustomsRate))
				{
					var isStartDateInOtherRowDateRange = startDate >= exchangeRate.RE_StartDate && startDate <= exchangeRate.RE_ExpiryDate;
					var dateRangeCoversOtherRowDateRange = startDate < exchangeRate.RE_StartDate && expiryDate > exchangeRate.RE_ExpiryDate;
					if (isStartDateInOtherRowDateRange || dateRangeCoversOtherRowDateRange)
					{
						throw new NativeXMLUserVisibleException("Overlapped start date is entered.");
					}
				}
			}
		}
	}
}
