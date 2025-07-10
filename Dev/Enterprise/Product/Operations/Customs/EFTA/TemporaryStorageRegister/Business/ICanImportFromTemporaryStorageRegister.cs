namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public interface ICanImportFromTemporaryStorageRegister
{
	string TemporaryStorageApplicationCode { get; }
	string DepartureCustomsOfficeCode { get; }
}
