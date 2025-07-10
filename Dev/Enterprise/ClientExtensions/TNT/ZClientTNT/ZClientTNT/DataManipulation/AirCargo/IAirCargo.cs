
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.TNT.AirCargo
{
	public interface IAirCargo
	{
		void PopulateNewData(CusMAWB masterBill);
		void PopulateData(CusMAWB masterBill);
		event TNTProgressEventHandler Progress;
		bool IsValid { get; }
		void Save();
	}
}
