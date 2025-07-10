using CargoWise.Types;
namespace Enterprise.Customs.FR.Messaging.Interfaces.CIN
{
	public interface ICINMessage755
	{
		ZString UniqueMessageNumber { get; }
		ZString BGMReference { get; }
		ZString Date { get; }
		ZString NumLTA { get; }
		ZString OACI_Shipper { get; }
		ZString OACI_Carrier { get; }
		ZString RefDos { get; }
		ZString Hwb { get; }
		ZString StoreCode { get; }
		ZString PackagesCount { get; }
		ZString GrossWeight { get; }
		ZString MrnNumber { get; }
		ZString CustomOffice { get; }
		ZString BAEDate { get; }
		ZString Magasin { get; }
	}
}
