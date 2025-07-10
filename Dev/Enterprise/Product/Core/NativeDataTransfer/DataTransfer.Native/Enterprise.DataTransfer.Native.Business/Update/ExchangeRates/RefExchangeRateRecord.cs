using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.ExchangeRates;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
class RefExchangeRateRecord
{
	readonly BusinessObjectFactory factory;
	readonly IEntity entity;

	public RefExchangeRateRecord(BusinessObjectFactory factory, IEntity entity)
	{
		this.factory = factory;
		this.entity = entity;
	}

	IEntity CurrencyEntity => entity.GetParentEntity("RefCurrency");

	IEntity CompanyEntity => entity.GetParentEntity("GlbCompany");

	string CompanyCode => CompanyEntity.GetPropertyOrBlankString("Code");

	public ZString ExRateType => entity.GetPropertyOrBlankString(nameof(ExRateType));

	public ZString Currency => CurrencyEntity.GetPropertyOrBlankString("Code");

	public ZDateTime StartDate => ZDateTime.TryParseExact(entity.GetPropertyOrBlankString(nameof(StartDate)), out var startDate, "yyyy-MM-dd") ? startDate : ZDateTime.Empty;

	public ZDateTime ExpiryDate => ZDateTime.TryParseExact(entity.GetPropertyOrBlankString(nameof(ExpiryDate)), out var expiryDate, "yyyy-MM-dd") ? expiryDate : ZDateTime.Empty;

	public GlbCompany Company => factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, CompanyCode));
}
